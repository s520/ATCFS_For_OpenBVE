// Copyright 2018 S520
// All rights reserved.
//
// Redistribution and use in source and binary forms, with or without
// modification, are permitted provided that the following conditions are met :
//
// 1. Redistributions of source code must retain the above copyright notice,
// this list of conditions and the following disclaimer.
// 2. Redistributions in binary form must reproduce the above copyright notice,
// this list of conditions and the following disclaimer in the documentation
// and / or other materials provided with the distribution.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND
// ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
// WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
// DISCLAIMED.IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR
// ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
// (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
// LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
// ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
// (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
// SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using OpenBveApi.Runtime;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace ATCFS {

    /// <summary>
    /// ATC-1, ATC-2, ATC-NS, KS-ATC, DS-ATCを再現するクラス
    /// </summary>
    internal class Atc : Device {

        // --- クラス ---
        /// <summary>
        /// ATC-1, ATC-2を再現するクラス
        /// </summary>
        private class AtcA {

            // --- メンバ ---
            private readonly Train train_;
            internal int signal_;  //!< 車内信号の信号インデックス
            internal int is_stop_eb_;  //!< ATC-02, 03信号ブレーキフラグ
            internal int is_stop_svc_;  //!< ATC-30信号ブレーキフラグ
            internal int is__brake_reset_;  //!< ブレーキ開放フラグ

            // --- コンストラクタ ---
            /// <summary>
            /// 新しいインスタンスを作成する
            /// </summary>
            /// <param name="train">Trainクラスのインスタンス</param>
            internal AtcA(Train train) {
                this.train_ = train;
            }

            // --- 関数 ---
            /// <summary>
            /// Initializeで実行する関数
            /// </summary>
            internal void Init() {
                signal_ = 0;
                is_stop_eb_ = 0;
                is_stop_svc_ = 0;
                is__brake_reset_ = 0;
            }

            /// <summary>
            /// SetSignalで実行され、車内信号を更新する関数
            /// </summary>
            /// <remarks>ATC-02, 03, 30信号のブレーキフラグ管理および、とーほぐ新幹線信号インデックスの変換を含む</remarks>
            /// <param name="signal">現在のセクションの信号番号</param>
            internal void ChangedSignal(int signal) {
                // とーほぐ新幹線信号インデックス変換
                if (this.train_.Atc.atc_type_ == 1 && signal >= 102) {
                    signal = signal % 100 - 2;
                }

                // For Safety
                if (signal < this.train_.Atc.atc_spd_list_.Length) {
                    // 車両最高速度以上の信号は出さない
                    if (signal > this.train_.Atc.max_signal_) { signal = this.train_.Atc.max_signal_; }

                    // ATC-02, 03信号ブレーキフラグOFF
                    if (signal != 0) { is_stop_eb_ = 0; }

                    // ATC-30信号ブレーキフラグOFF
                    if (signal != 1) { is_stop_svc_ = 0; }

                    // ATC-02信号ブレーキフラグON & ブレーキ開放フラグOFF
                    if (signal == 0 && signal_ != 0) {
                        is_stop_eb_ = 1;
                        is__brake_reset_ = (this.train_.Atc.train_spd_ != 0 || is__brake_reset_ != 1) ? 0 : 1;
                    }

                    // ATC-30信号ブレーキフラグON & ブレーキ開放フラグOFF
                    if (signal == 1 && signal_ != 1) {
                        is_stop_svc_ = 1;
                        is__brake_reset_ = (this.train_.Atc.train_spd_ != 0 || is__brake_reset_ != 1) ? 0 : 1;
                    }

                    // ATCベル
                    if (signal != signal_) { this.train_.Sounds.AtcDing.Play(); }

                    // 車内信号の更新
                    signal_ = signal;
                } else {
                    is_stop_eb_ = 1;
                }
            }
        }

        /// <summary>
        /// ATC-NS, KS-ATC, DS-ATCを再現するクラス
        /// </summary>
        private class AtcD {

            // --- クラス ---
            /// <summary>
            /// 停止信号パターン関連を記述するクラス
            /// </summary>
            internal class SectionD {

                // --- メンバ ---
                private readonly Train train_;
                private double prev_loc_;  //! 以前の列車位置[m]
                private List<double> section_loc_list_;  //!< 閉塞境界位置[m]リスト
                internal int track_path_;  //!< 開通区間数
                internal double red_signal_loc_;  //!< 停止信号地点[m]

                // --- コンストラクタ ---
                /// <summary>
                /// 新しいインスタンスを作成する
                /// </summary>
                /// <param name="train">Trainクラスのインスタンス</param>
                internal SectionD(Train train) {
                    this.train_ = train;
                }

                // --- 関数 ---
                /// <summary>
                /// Initializeで実行する関数
                /// </summary>
                internal void Init() {
                    section_loc_list_ = new List<double>();
                    prev_loc_ = 0.0;
                    track_path_ = 0;
                    red_signal_loc_ = 0.0;
                }

                /// <summary>
                /// 閉塞境界位置を登録する関数
                /// </summary>
                /// <param name="distance">対となるセクションまでの距離[m]</param>
                internal void RegSection(double distance) {
                    int def = (int)(this.train_.State.Location - prev_loc_);
                    if (def != 0) {
                        prev_loc_ = this.train_.State.Location;
                        section_loc_list_.Clear();
                    }
                    section_loc_list_.Add(prev_loc_ + distance);
                }

                /// <summary>
                /// 停止位置を算出する関数
                /// </summary>
                internal void CalcSection() {
                    red_signal_loc_ = BaseFunc.ListGetOrDefault(section_loc_list_, track_path_);
                }
            }

            /// <summary>
            /// 駅停車パターン関連を記述するクラス
            /// </summary>
            internal class StationD {

                // --- メンバ ---
                private readonly Train train_;
                private int is_stop_sta_;  //!< 駅停車後方許容地点フラグ
                private int[] pattern_is_ready_;  //!< 駅への停車開始判定フラグ
                internal double[] pattern_end_loc_;  //!< 減速完了地点[m]
                internal int[] pattern_is_valid_;  //!< パターンの状態(0: 無効, 1: 有効)
                internal int[] pattern_tget_spd_;  //!< 目標速度[km/h]

                // --- コンストラクタ ---
                /// <summary>
                /// 新しいインスタンスを作成する
                /// </summary>
                /// <param name="train">Trainクラスのインスタンス</param>
                internal StationD(Train train) {
                    this.train_ = train;
                }

                // --- 関数 ---
                /// <summary>
                /// Initializeで実行する関数
                /// </summary>
                internal void Init() {
                    pattern_is_ready_ = new int[Atc.STA_PATTERN];
                    pattern_end_loc_ = new double[Atc.STA_PATTERN];
                    pattern_is_valid_ = new int[Atc.STA_PATTERN];
                    pattern_tget_spd_ = new int[Atc.STA_PATTERN];
                    is_stop_sta_ = 0;
                }

                /// <summary>
                /// 駅への停車開始判定を行う関数
                /// </summary>
                /// <remarks>出発信号が停止現示の場合に駅停車パターンが有効になる</remarks>
                /// <param name="signal">出発信号の信号番号</param>
                internal void RegStaStop(int signal) {
                    if (this.train_.Atc.atc_type_ > 1 && signal == 0) {
                        pattern_is_ready_[0] = (this.train_.Atc.atc_type_ < 4) ? 1 : 0;
                        pattern_is_ready_[1] = 1;
                        pattern_is_ready_[2] = (this.train_.Atc.atc_type_ > 2) ? 1 : 0;
                    }
                }

                /// <summary>
                /// 駅停車パターン(分岐制限)を登録する関数
                /// </summary>
                /// <param name="distance">減速完了地点までの相対距離[m]</param>
                internal void RegStaBranch(int distance) {
                    pattern_end_loc_[0] = this.train_.State.Location + distance;
                    pattern_tget_spd_[0] = 70;
                    pattern_is_valid_[0] = (pattern_is_ready_[0] == 1) ? 1 : 0;
                }

                /// <summary>
                /// 駅停車パターン(手動頭打ち)を登録する関数
                /// </summary>
                /// <param name="distance">減速完了地点までの相対距離[m]</param>
                internal void RegStaManual(int distance) {
                    pattern_end_loc_[1] = this.train_.State.Location + distance;
                    switch (this.train_.Atc.atc_type_) {
                    case 2:
                        pattern_tget_spd_[1] = 30;
                        break;
                    case 3:
                        pattern_tget_spd_[1] = 15;
                        break;
                    case 4:
                        pattern_tget_spd_[1] = 75;
                        break;
                    default:
                        break;
                    }
                    pattern_is_valid_[1] = (pattern_is_ready_[1] == 1) ? 1 : 0;
                }

                /// <summary>
                /// 駅停車パターン(オーバーラン防止)を登録する関数
                /// </summary>
                /// <param name="distance">減速完了地点までの相対距離[m]</param>
                internal void RegStaEnd(int distance) {
                    pattern_end_loc_[2] = this.train_.State.Location + distance;
                    pattern_tget_spd_[2] = 0;
                    pattern_is_valid_[2] = (pattern_is_ready_[2] == 1) ? 1 : 0;
                }

                /// <summary>
                /// 駅停車許容フラグをONにする関数
                /// </summary>
                internal void RegStaLoc() {
                    is_stop_sta_ = 1;
                }

                /// <summary>
                /// 駅停車完了判定を行う関数
                /// </summary>
                /// <remarks>駅停車完了判定が真の場合は駅停車パターンが消去される</remarks>
                internal void IsStopSta() {
                    if (is_stop_sta_ == 1 && this.train_.Atc.train_spd_ == 0 && this.train_.Handles.BrakeNotch > 0) {
                        for (int i = 0; i < Atc.STA_PATTERN; i++) {
                            pattern_is_ready_[i] = 0;
                            pattern_is_valid_[i] = 0;
                        }
                        is_stop_sta_ = 0;
                    }
                }
            }

            /// <summary>
            /// 制限速度パターン関連を記述するクラス
            /// </summary>
            internal class PatternD {

                // --- メンバ ---
                private readonly Train train_;
                internal double[] pattern_end_loc_;  //!< 減速完了地点[m]
                internal int[] pattern_is_valid_;  //!< パターンの状態(0: 無効, 1: 有効)
                internal int[] pattern_tget_spd_;  //!< 目標速度[km/h]

                // --- コンストラクタ ---
                /// <summary>
                /// 新しいインスタンスを作成する
                /// </summary>
                /// <param name="train">Trainクラスのインスタンス</param>
                internal PatternD(Train train) {
                    this.train_ = train;
                }

                // --- 関数 ---
                /// <summary>
                /// Initializeで実行する関数
                /// </summary>
                internal void Init() {
                    pattern_end_loc_ = new double[Atc.USR_PATTERN];
                    pattern_is_valid_ = new int[Atc.USR_PATTERN];
                    pattern_tget_spd_ = new int[Atc.USR_PATTERN];
                }

                /// <summary>
                /// 速度制限パターンの登録および消去を行う関数
                /// </summary>
                /// <param name="type">パターン番号</param>
                /// <param name="optional">減速完了地点までの相対距離[m]*1000+目標速度[km/h]</param>
                internal void RegPattern(int type, int optional) {
                    int distance = optional / 1000;
                    if (distance < 0) { distance = 0; }
                    int tget_spd = optional % 1000;
                    if (tget_spd != 999) {
                        pattern_end_loc_[type] = this.train_.State.Location + distance;
                        pattern_tget_spd_[type] = tget_spd;
                        pattern_is_valid_[type] = 1;
                    } else {
                        pattern_is_valid_[type] = 0;
                    }
                }
            }

            // --- メンバ ---
            private readonly Train train_;
            internal readonly SectionD section_d_;
            internal readonly StationD station_d_;
            internal readonly PatternD pattern_d_;
            private double prev_spd_;  //!< 1フレーム前の列車速度[km/h]
            internal int is_stop_eb_;  //!< ATC-02, 03信号ブレーキフラグ
            internal int is_stop_svc_;  //!< ATC-30信号ブレーキフラグ
            internal int is__brake_reset_;  //!< ブレーキ開放フラグ
            internal int arrow_spd_;  //!< パターン照査速度[km/h]
            internal int arrow_signal_;  //!< パターン照査速度の信号インデックス
            internal int prev_arrow_signal_;  //!< 以前のパターン照査速度の信号インデックス
            internal int tget_spd_;  //!< 目標速度[km/h]
            internal int tget_signal_;  //!< 目標速度の信号インデックス
            internal int prev_tget_signal_;  //!< 以前の目標速度の信号インデックス

            // --- コンストラクタ ---
            /// <summary>
            /// 新しいインスタンスを作成する
            /// </summary>
            /// <param name="train">Trainクラスのインスタンス</param>
            internal AtcD(Train train) {
                this.train_ = train;
                section_d_ = new SectionD(train);
                station_d_ = new StationD(train);
                pattern_d_ = new PatternD(train);
            }

            // --- 関数 ---
            /// <summary>
            /// Initializeで実行する関数
            /// </summary>
            internal void Init() {
                is_stop_eb_ = 0;
                is_stop_svc_ = 0;
                is__brake_reset_ = 0;
                prev_spd_ = 0.0f;
                arrow_spd_ = 0;
                arrow_signal_ = 0;
                prev_arrow_signal_ = 0;
                tget_spd_ = 0;
                tget_signal_ = 0;
                prev_tget_signal_ = 0;
                section_d_.Init();
                station_d_.Init();
                pattern_d_.Init();
            }

            /// <summary>
            /// SetSignalで実行され、開通区間数を更新する関数
            /// </summary>
            /// <param name="signal">現在のセクションの信号番号</param>
            internal void ChangedSignal(int signal) {
                section_d_.track_path_ = signal;
            }

            /// <summary>
            /// ATC-NSにおいてATC-30信号ブレーキフラグのON, OFFを行う関数
            /// </summary>
            /// <remarks>車内信号がATC-30かつ列車速度が30km/h以上から以下へ変化した場合にONになる</remarks>
            internal void AtcCheck() {
                if (this.train_.Atc.atc_type_ == 2) {
                    // ATC-30信号ブレーキフラグON & ブレーキ開放フラグOFF
                    if (tget_signal_ == 1 && Math.Abs(this.train_.Atc.train_spd_) <= 30.0 && prev_spd_ > 30.0) {
                        is_stop_svc_ = 1;
                        is__brake_reset_ = 0;
                    } else if (Math.Abs(this.train_.Atc.train_spd_) > 30.0 && prev_spd_ <= 30.0) {
                        is_stop_svc_ = 0;
                    }
                }
                prev_spd_ = Math.Abs(this.train_.Atc.train_spd_);
            }
        }

        /// <summary>
        /// 予見Fuzzy制御を再現するクラス
        /// </summary>
        private class Fuzzy {

            // --- メンバ ---
            private readonly Train train_;
            private int prev_brake_notch_;  //!< 以前の出力ブレーキノッチ(HBを含まない)
            private double adj_timer_;  //!< 減速度補正を行う次のゲーム内時刻[ms]
            private double fuzzy_prev_Tc_;  //!< 最後に出力ブレーキノッチが変化したゲーム内時刻[s]
            private int fuzzy_prev_Nc_;  //!< 最後に変化した出力ブレーキノッチの変化量の絶対値
            private List<double> fuzzy_Xp_;  //!< 予測減速完了地点[m]
            private List<double> fuzzy_Ugg_;  //!< 「うまく停止する(GG)」の評価値
            private List<double> fuzzy_Uga_;  //!< 「正確に停止する(GA)」の評価値
            private double fuzzy_Ucg_;  //!< 「乗り心地が良い(CG)」の評価値
            private double fuzzy_Ucb_;  //!< 「乗り心地が悪い(CB)」の評価値
            private List<double> fuzzy_U_;  //!< "(CG >= CB) And GA"の評価値
            private double fuzzy_Usb_;  //!< 「安全性が悪い(SB)」の評価値
            private double fuzzy_Usvb_;  //!< 「安全性がとても悪い(SVB)」の評価値
            internal double adj_deceleration_;  //!< 各ブレーキノッチの減速度補正値[m/s^2]
            internal int[] fuzzy_step_;  //!< 全パターンの予見Fuzzy制御の制御段階
            private double[] brake_timer_;  //!< 全パターンの予見Fuzzy制御を実行する次のゲーム内時刻[ms]
            internal int[] prev_tget_spd_;  //!< 全パターンの以前の目標速度[km/h]
            internal double[] prev_pattern_end_loc_;  //!< 全パターンの以前の減速完了地点[m]
            internal int[] fuzzy_brake_notch_list_;  //!< 全パターンの予見Fuzzy制御の最適ブレーキノッチ

            // --- コンストラクタ ---
            /// <summary>
            /// 新しいインスタンスを作成する
            /// </summary>
            /// <param name="train">Trainクラスのインスタンス</param>
            internal Fuzzy(Train train) {
                this.train_ = train;
            }

            // --- 関数 ---
            /// <summary>
            /// Initializeで実行する関数
            /// </summary>
            internal void Init() {
                prev_brake_notch_ = 0;
                adj_deceleration_ = 0;
                adj_timer_ = 0;
                fuzzy_prev_Tc_ = 0.0f;
                fuzzy_prev_Nc_ = 0;
                fuzzy_Ucg_ = 0.0f;
                fuzzy_Ucb_ = 0.0f;
                fuzzy_Usb_ = 0.0f;
                fuzzy_Usvb_ = 0.0f;
                fuzzy_step_ = new int[Atc.ALL_PATTERN];
                brake_timer_ = new double[Atc.ALL_PATTERN];
                fuzzy_Xp_ = new List<double>();
                fuzzy_Ugg_ = new List<double>();
                fuzzy_Uga_ = new List<double>();
                fuzzy_U_ = new List<double>();
                prev_tget_spd_ = new int[Atc.ALL_PATTERN];
                prev_pattern_end_loc_ = new double[Atc.ALL_PATTERN];
                fuzzy_brake_notch_list_ = new int[Atc.ALL_PATTERN];
            }

            /// <summary>
            /// Fuzzyメンバシップ関数L
            /// </summary>
            /// <remarks>定義域を(a - b, a + b)とする三角形状の関数</remarks>
            /// <param name="x">x</param>
            /// <param name="a">a</param>
            /// <param name="b">b</param>
            /// <returns>評価値</returns>
            private double FuzzyFuncL(double x, double a, double b) {
                if (x <= (a - b) || x >= (a + b)) {
                    return 0.0;
                } else {
                    return (1.0 - Math.Abs(x - a) / b);
                }
            }

            /// <summary>
            /// Fuzzyメンバシップ関数F
            /// </summary>
            /// <remarks>関数Lでaより大きい部分と1.0とした関数</remarks>
            /// <param name="x">x</param>
            /// <param name="a">a</param>
            /// <param name="b">b</param>
            /// <returns>評価値</returns>
            private double FuzzyFuncF(double x, double a, double b) {
                if (x <= (a - b)) {
                    return 0.0;
                } else if (x >= a) {
                    return 1.0;
                } else {
                    return (1.0 - Math.Abs(x - a) / b);
                }
            }

            /// <summary>
            /// Fuzzyメンバシップ関数A
            /// </summary>
            /// <remarks>定義域を(-∞, +∞)とした尖った関数</remarks>
            /// <param name="x">x</param>
            /// <param name="a">a</param>
            /// <param name="b">b</param>
            /// <returns>評価値</returns>
            private double FuzzyFuncA(double x, double a, double b) {
                return (b / (Math.Abs(x - a) + b));
            }

            /// <summary>
            /// Fuzzyメンバシップ関数G
            /// </summary>
            /// <remarks>定義域を(-∞, +∞)とした台形状の関数</remarks>
            /// <param name="x">x</param>
            /// <param name="a">a</param>
            /// <param name="b">b</param>
            /// <returns>評価値</returns>
            private double FuzzyFuncG(double x, double a, double b) {
                if (x >= (a - b) && x <= (a + b)) {
                    return 1.0;
                } else {
                    return (b / Math.Abs(x - a));
                }
            }

            /// <summary>
            /// Fuzzyメンバシップ関数Gの形状を制御する関数
            /// </summary>
            /// <remarks>メンバシップ関数Gの形状(頂上の平坦部の幅)を速度に応じて連続的に制御する</remarks>
            /// <param name="x">速度[km/h]</param>
            /// <param name="a">頂上の平坦部の幅の初期値</param>
            /// <param name="b">補正係数</param>
            /// <returns>頂上の平坦部の幅</returns>
            private double FuzzyFuncQ(double x, double a, double b) {
                if (b <= 0.0) {
                    return a;
                } else if (x <= Math.Sqrt(a / b)) {
                    return a;
                } else {
                    return (b * x * x);
                }
            }

            /// <summary>
            /// 各ブレーキノッチの減速度の補正値を算出する関数
            /// </summary>
            /// <remarks>ブレーキノッチが2秒間変化しなかった場合に補正値を算出する</remarks>
            internal void FuzzyAdjDeceleration() {
                if (this.train_.Atc.brake_notch_ != prev_brake_notch_) {
                    adj_timer_ = this.train_.Atc.time_ + 2000.0;
                    fuzzy_prev_Tc_ = this.train_.Atc.time_ / 1000.0;
                    fuzzy_prev_Nc_ = Math.Abs(this.train_.Atc.brake_notch_ - prev_brake_notch_);
                    prev_brake_notch_ = this.train_.Atc.brake_notch_;
                } else if (this.train_.Atc.time_ >= adj_timer_) {
                    if (this.train_.Atc.brake_notch_ < this.train_.Atc.max_brake_notch_ + 1) {
                        if (Math.Abs(this.train_.Atc.train_spd_) != 0 && this.train_.Handles.PowerNotch == 0) {
                            adj_deceleration_ = this.train_.Accel.ema_accel_ / 3.6 + (this.train_.Atc.max_deceleration_ / 3.6) * (this.train_.Atc.brake_notch_ / (double)(this.train_.Atc.max_brake_notch_));
                        }
                    }
                }
            }

            /// <summary>
            /// 減速完了地点を予測する関数
            /// </summary>
            /// <param name="tget_spd">目標速度[km/h]</param>
            /// <param name="notch_num">ブレーキノッチ</param>
            /// <returns>予測減速完了地点[m]</returns>
            private double FuzzyEstLoc(int tget_spd, int notch_num) {
                double deceleration = (this.train_.Atc.max_deceleration_ / 3.6) * (notch_num / (double)this.train_.Atc.max_brake_notch_) - adj_deceleration_;
                double est_patt_end_loc = double.MaxValue;
                if (deceleration != 0) {
                    est_patt_end_loc = ((this.train_.Atc.train_spd_ / 3.6) * (this.train_.Atc.train_spd_ / 3.6) - (tget_spd / 3.6) * (tget_spd / 3.6)) / (2.0 * deceleration) + this.train_.State.Location;
                    if (notch_num != this.train_.Atc.brake_notch_) {
                        est_patt_end_loc += (this.train_.Atc.train_spd_ / 3.6) * this.train_.Atc.lever_delay_;
                    }
                }
                return est_patt_end_loc;
            }

            /// <summary>
            /// 予見Fuzzy制御を初期化する関数
            /// </summary>
            /// <param name="index">パターン番号</param>
            /// <param name="pattern_start_loc">パターン降下開始地点[m]</param>
            /// <returns>最適ブレーキノッチ</returns>
            internal int FuzzyCtrInit(int index, double pattern_start_loc) {
                int brake_notch = 0;
                double est_loc = this.train_.State.Location + (this.train_.Atc.train_spd_ / 3.6) * 2.0 + 0.5 * (this.train_.Accel.ema_accel_ / 3.6) * 2.0 * 2.0;
                if (est_loc >= pattern_start_loc) {
                    if (this.train_.Atc.brake_notch_ == 0) {
                        brake_notch = 1;
                    } else {
                        brake_notch = this.train_.Atc.brake_notch_;
                    }
                    brake_timer_[index] = this.train_.Atc.time_ + 2000.0;
                    fuzzy_step_[index] = 1;
                }
                return brake_notch;
            }

            /// <summary>
            /// 予見Fuzzy制御を行う関数
            /// </summary>
            /// <param name="index">パターン番号</param>
            /// <param name="tget_spd">目標速度[km/h]</param>
            /// <param name="Xt">減速完了地点[m]</param>
            /// <returns>最適ブレーキノッチ</returns>
            internal int FuzzyCtrExe(int index, int tget_spd, double Xt) {
                int brake_notch = fuzzy_brake_notch_list_[index];

                // ノッチ変化後の経過時間[s]
                double fuzzy_Tc = this.train_.Atc.time_ / 1000.0 - fuzzy_prev_Tc_;

                // 速度による補正をかけたXe[m]
                double fuzzy_Xe = FuzzyFuncQ(Math.Abs(this.train_.Atc.train_spd_) - tget_spd, this.train_.Atc.Xe, this.train_.Atc.Xk);

                // 速度による補正をかけたXo[m]
                double fuzzy_Xo = FuzzyFuncQ(Math.Abs(this.train_.Atc.train_spd_) - tget_spd, this.train_.Atc.Xo, this.train_.Atc.Xk);

                fuzzy_Xp_.Clear();
                fuzzy_Ugg_.Clear();
                fuzzy_Uga_.Clear();
                fuzzy_U_.Clear();

                if (this.train_.Atc.time_ >= brake_timer_[index]) {
                    // 各制御則の評価
                    for (int Np = 0; Np <= this.train_.Atc.max_brake_notch_; Np++) {
                        double Xp = FuzzyEstLoc(tget_spd, Np);
                        fuzzy_Xp_.Add(Xp);
                        double Ugg = FuzzyFuncG(fuzzy_Xp_[Np], Xt, fuzzy_Xe);
                        fuzzy_Ugg_.Add(Ugg);
                        double Uga = FuzzyFuncA(fuzzy_Xp_[Np], Xt, fuzzy_Xe);
                        fuzzy_Uga_.Add(Uga);
                    }
                    fuzzy_Ucg_ = FuzzyFuncF(fuzzy_Tc, (1.0 + fuzzy_prev_Nc_ / 2.0), (fuzzy_prev_Nc_ / 2.0));
                    fuzzy_Ucb_ = 1.0 - fuzzy_Ucg_;
                    fuzzy_Usb_ = FuzzyFuncL(fuzzy_Xp_[this.train_.Atc.max_brake_notch_], Xt + fuzzy_Xo / 2.0, fuzzy_Xo);
                    fuzzy_Usvb_ = FuzzyFuncF(fuzzy_Xp_[this.train_.Atc.max_brake_notch_], Xt + 3.0 / 2.0 * fuzzy_Xo, fuzzy_Xo);

                    // 最適ブレーキノッチの決定
                    if (fuzzy_Usvb_ > 0.5) {
                        brake_notch = this.train_.Atc.max_brake_notch_ + 1;
                    } else if (fuzzy_Usb_ > 0.5) {
                        brake_notch = this.train_.Atc.max_brake_notch_;
                    } else if (BaseFunc.ListGetOrDefault(fuzzy_Ugg_, this.train_.Atc.brake_notch_) == 1 || fuzzy_Ucg_ < fuzzy_Ucb_) {
                        brake_notch = (this.train_.Atc.brake_notch_ > this.train_.Atc.max_brake_notch_) ? this.train_.Atc.max_brake_notch_ : this.train_.Atc.brake_notch_;
                    } else if (fuzzy_Ucg_ >= fuzzy_Ucb_) {
                        int Np_min = this.train_.Atc.brake_notch_ - 2;
                        if (Np_min < 0) { Np_min = 0; }
                        int Np_max = this.train_.Atc.brake_notch_ + 2;
                        if (Np_max > this.train_.Atc.max_brake_notch_) { Np_max = this.train_.Atc.max_brake_notch_; }
                        for (int Np = Np_min; Np <= Np_max; Np++) {
                            double U = fuzzy_Uga_[Np];
                            fuzzy_U_.Add(U);
                        }
                        double U_max = fuzzy_U_.Max();
                        if (U_max == 0) {
                            brake_notch = this.train_.Atc.brake_notch_;
                        } else {
                            int Np_best = fuzzy_U_.IndexOf(U_max);
                            brake_notch = Np_min + Np_best;
                        }
                    }
                    brake_timer_[index] = this.train_.Atc.time_ + 1000.0;
                }
                return brake_notch;
            }
        }

        // --- メンバ ---
        private readonly Train train_;
        private readonly AtcA atc_a_;
        private readonly AtcD atc_d_;
        private readonly Fuzzy fuzzy_;
        private const int ALL_PATTERN = 8;  //!< パターンの総数
        private const int STA_PATTERN = 3;  //!< 駅停車パターンの総数
        private const int USR_PATTERN = 3;  //!< 速度制限パターンの総数
        private int max_brake_notch_;  //!< 常用最大ブレーキノッチ(HBを含まない)
        private int brake_notch_;  //!< 出力ブレーキノッチ(HBを含まない)
        private int[] default_notch_;  //!< 標準ブレーキノッチ
        private int max_signal_;  //!< 車両ATC最高速度に対応する信号インデックス
        private double[] pattern_list_;  //!< デジタルATC用速度照査パターン
        private double[] pattarn_end_loc_list_;  //!< 全パターンの減速完了地点[m]
        private int[] pattern_is_valid_list_;  //!< 全パターンの状態(0: 無効, 1: 有効)
        private int[] pattern_tget_spd_list_;  //!< 全パターンの目標速度[km/h]
        private int[] pattern_arrow_spd_list_;  //!< 全パターンのパターン照査速度[km/h]
        private double[] pattern_start_loc_list_;  //!< 全パターンのパターン降下開始地点[m]
        private double debug_timer_;  //!< Debug出力する次のゲーム内時刻[ms]
        private double brake_timer_;  //!< ブレーキノッチを変更する次のゲーム内時刻[ms]
        private double max_deceleration_;  //!< 常用最大減速度[km/h/s]
        private int atc_power_;  //!< ATC電源(0: 消灯, 1: 点灯)
        private int atc_use_;  //!< ATC(0: 消灯, 1: 点灯)
        private int atc_type_;  //!< ATC方式(0: ATC-1, 1: ATC-2, 2: ATC-NS, 3: KS-ATC, 4: DS-ATC)
        private int atc_max_spd_;  //!< 車両ATC最高速度[km/h]
        private int[] atc_spd_list_;  //!< 信号インデックスに対応する速度[km/h]
        private double[] atc_deceleration_;  //!< ATCブレーキ減速度[km/h/s]
        private int atc_reset_sw_;  //!< 確認ボタンの状態(0: 解放, 1: 押下)
        internal int atc_brake_notch_ { get; private set; }  //!< ATC出力ブレーキノッチ(HBを含まない)
        private int atc_red_signal_;  //!< 停止現示(0: 消灯, 1: 点灯)
        private int atc_green_signal_;  //!< 進行現示(0: 消灯, 1: 点灯)
        private int[] atc_sig_indicator_;  //!< ATC速度表示インジケータ(0: 消灯, 1: 点灯)
        private int[] atc_spd_7seg_;  //!< 7セグ用ATC速度表示
        private int atc_signal_index_;  //!< ATC速度に対応する信号インデックス
        private int atc_spd_;  //!< ATC速度[km/h]
        private int dsatc_arrow_spd_;  //!< DS-ATC用パターン照査速度[km/h]
        private int[] sub_spd_label_1_;  //!< 副速度計用目盛 ATC速度-20 km/h
        private int[] sub_spd_label_2_;  //!< 副速度計用目盛 ATC速度-10 km/h
        private int[] sub_spd_label_3_;  //!< 副速度計用目盛 ATC速度
        private int[] sub_spd_label_4_;  //!< 副速度計用目盛 ATC速度+10 km/h
        private int sub_atc_spd_;  //!< 副速度計用 ATC速度[km/h]
        private int sub_train_spd_;  //!< 副速度計用 列車速度[km/h]
        private double lever_delay_;  //!< ブレーキハンドルの操作から指令出力までの遅れ時間[s]
        private int atc_eb_lamp_;  //!< ATC非常(0: 消灯, 1: 点灯)
        private int atc_svc_lamp_;  //!< ATC常用(0: 消灯, 1: 点灯)
        private double Xe;  //!< 減速完了地点からの許容誤差[m]
        private double Xo;  //!< 減速完了地点からの過走限界距離[m]
        private double Xk;  //!< XeおよびXoを高速域で拡大させる係数

        private double train_spd_;  //!< 列車速度[km/h]
        private double time_;  //!< ゲーム内時刻[ms]
        private List<int> beacon_type_;  //!< 地上子種別
        private List<int> beacon_sig_;  //!< 対となるセクションの信号
        private List<double> beacon_dist_;  //!< 対となるセクションまでの距離[m]
        private List<int> beacon_opt_;  //!< 地上子に設定された任意の値

        // --- コンストラクタ ---
        /// <summary>
        /// 新しいインスタンスを作成する
        /// </summary>
        /// <param name="train">Trainクラスのインスタンス</param>
        internal Atc(Train train) {
            this.train_ = train;
            this.atc_a_ = new AtcA(train);
            this.atc_d_ = new AtcD(train);
            this.fuzzy_ = new Fuzzy(train);
        }

        // --- 関数 ---
        /// <summary>
        /// 速度に対応する信号インデックスを返す関数
        /// </summary>
        /// <remarks>指定された速度以下の近似値に対応する信号インデックスを検索する</remarks>
        /// <param name="spd">速度[km/h]</param>
        /// <returns>速度に対応する信号インデックス</returns>
        private int SearchSignal(int spd) {
            return BaseFunc.UpperBound(atc_spd_list_, spd) - 1;
        }

        /// <summary>
        /// 信号インデックスを速度に変換する関数
        /// </summary>
        /// <remarks>ATC-1のみ220km/h以上300km/h未満は"対応速度+5km/h"、300km/h以上は"対応速度+3km/h"を返す</remarks>
        /// <param name="index">信号インデックス</param>
        /// <returns>信号インデックスに対応する速度[km/h]</returns>
        private int ItoV(int index) {
            int atc_spd = BaseFunc.ArrayGetOrDefault(atc_spd_list_, index);
            if (atc_type_ == 0 && atc_spd >= 220) {
                if (atc_spd >= 300) {
                    atc_spd += 3;
                } else {
                    atc_spd += 5;
                }
            }
            return atc_spd;
        }

        /// <summary>
        /// 信号インデックスを速度に変換する関数
        /// </summary>
        /// <remarks><paramref name="is_display"/>が偽かつATC-1のみ220km/h以上300km/h未満は"対応速度+5km/h"、300km/h以上は"対応速度+3km/h"を返す</remarks>
        /// <param name="index">信号インデックス</param>
        /// <param name="is_display">パネル表示用かどうか</param>
        /// <returns>信号インデックスに対応する速度[km/h]</returns>
        private int ItoV(int index, bool is_display) {
            int atc_spd = BaseFunc.ArrayGetOrDefault(atc_spd_list_, index);
            if (atc_type_ == 0 && atc_spd >= 220 && !is_display) {
                if (atc_spd >= 300) {
                    atc_spd += 3;
                } else {
                    atc_spd += 5;
                }
            }
            return atc_spd;
        }

        /// <summary>
        /// デジタルATC用速度照査パターンを作成する関数
        /// </summary>
        private void SetPatternList() {
            for (int v = 0; v < 71 && v < atc_max_spd_ + 1; v++) {
                pattern_list_[v] = ((v / 3.6) * (v / 3.6)) / (2.0 * (((max_deceleration_ / max_brake_notch_) * default_notch_[3]) / 3.6));
            }
            for (int v = 71; v < 111 && v < atc_max_spd_ + 1; v++) {
                pattern_list_[v] = ((v / 3.6) * (v / 3.6) - (70.0 / 3.6) * (70.0 / 3.6)) / (2.0 * (((max_deceleration_ / max_brake_notch_) * default_notch_[2]) / 3.6)) + pattern_list_[70];
            }
            for (int v = 111; v < 161 && v < atc_max_spd_ + 1; v++) {
                pattern_list_[v] = ((v / 3.6) * (v / 3.6) - (110.0 / 3.6) * (110.0 / 3.6)) / (2.0 * (((max_deceleration_ / max_brake_notch_) * default_notch_[1]) / 3.6)) + pattern_list_[110];
            }
            for (int v = 161; v < atc_max_spd_ + 1; v++) {
                pattern_list_[v] = ((v / 3.6) * (v / 3.6) - (160.0 / 3.6) * (160.0 / 3.6)) / (2.0 * (((max_deceleration_ / max_brake_notch_) * default_notch_[0]) / 3.6)) + pattern_list_[160];
            }
        }

        /// <summary>
        /// ATCを投入する際に実行する関数
        /// </summary>
        private void Start() {
            if (atc_use_ == 0) {
                Initialize(0);
                atc_use_ = 1;

                // ATCベル
                this.train_.Sounds.AtcDing.Play();
            }
        }

        /// <summary>
        /// ATCを遮断する際に実行する関数
        /// </summary>
        private void Exit() {
            if (atc_use_ == 1) {
                atc_use_ = 0;
                Initialize(0);
            }
        }

        /// <summary>
        /// SetSignalで実行される関数
        /// </summary>
        /// <param name="signal">現在のセクションの信号番号</param>
        private void ChangedSignal(int signal) {
            if (atc_use_ == 1) {
                if (atc_type_ < 2) {
                    this.atc_a_.ChangedSignal(signal);
                } else {
                    this.atc_d_.ChangedSignal(signal);
                }
            }
        }

        /// <summary>
        /// SetBeaconDataで実行される関数
        /// </summary>
        /// <param name="index">地上子種別</param>
        /// <param name="signal">対となるセクションの信号番号</param>
        /// <param name="distance">対となるセクションまでの距離[m]</param>
        /// <param name="optional">地上子に設定された任意の値</param>
        private void PassedBeacon(int index, int signal, double distance, int optional) {
            switch (index) {
            case 70:
                PassedLoop(signal);
                break;
            case 80:
                atc_d_.section_d_.RegSection(distance);
                break;
            case 81:
                atc_d_.station_d_.RegStaStop(signal);
                break;
            case 82:
                atc_d_.station_d_.RegStaBranch(optional);
                break;
            case 83:
                atc_d_.station_d_.RegStaManual(optional);
                break;
            case 84:
                atc_d_.station_d_.RegStaEnd(optional);
                break;
            case 85:
                atc_d_.station_d_.RegStaLoc();
                break;
            case 86:
                atc_d_.pattern_d_.RegPattern(0, optional);
                break;
            case 87:
                atc_d_.pattern_d_.RegPattern(1, optional);
                break;
            case 88:
                atc_d_.pattern_d_.RegPattern(2, optional);
                break;
            case 90:
                ChangedAtcType(optional);
                break;
            default:
                break;
            }
        }

        /// <summary>
        /// ATC方式を切り替える関数
        /// </summary>
        /// <param name="atc_type">ATC方式</param>
        private void ChangedAtcType(int atc_type) {
            if (atc_type != atc_type_ && atc_type < 5) {
                if (atc_type < 2 && atc_type_ > 1) {  // デジタルATCからアナログATCへの切り替え
                    atc_a_.signal_ = atc_d_.section_d_.track_path_;
                    atc_d_.Init();
                } else if (atc_type > 1 && atc_type_ < 2) {  // アナログATCからデジタルATCへの切り替え
                    atc_d_.section_d_.track_path_ = atc_a_.signal_;
                    atc_a_.Init();
                }
                atc_type_ = atc_type;
            }
        }

        /// <summary>
        /// ATC-1, 2, NSにおいて停止限界(ループコイル)を通過した際に実行する関数
        /// </summary>
        /// <remarks>信号番号が0の際に03信号を発信する</remarks>
        /// <param name="signal">対となるセクションの信号番号</param>
        private void PassedLoop(int signal) {
            if (atc_use_ == 1) {
                if (signal == 0) {  // ATC-03信号ブレーキフラグON & ブレーキ開放フラグOFF
                    if (atc_type_ < 2 && atc_a_.is_stop_eb_ != 1) {
                        atc_a_.is_stop_eb_ = 1;
                        atc_a_.is__brake_reset_ = 0;
                    }
                    if (atc_type_ == 2 && atc_d_.is_stop_eb_ != 1) {
                        atc_d_.is_stop_eb_ = 1;
                        atc_d_.is__brake_reset_ = 0;
                    }
                    this.train_.Sounds.AtcDing.Play();  // ATCベル
                } else {  // ATC-03信号ブレーキフラグOFF
                    if (atc_type_ == 2 && atc_d_.is_stop_eb_ == 1) {
                        atc_d_.is_stop_eb_ = 0;
                    }
                }
            }
        }

        /// <summary>
        /// ATC-1, 2, NSにおいて確認扱いの判定を行う関数
        /// </summary>
        /// <remarks>ATC-03信号の場合は列車速度が0km/h、ATC-30信号の場合は30km/h以下である場合、ブレーキ開放フラグがONになる</remarks>
        private void Reset() {
            if (atc_use_ == 1) {
                if (train_spd_ == 0.0) {
                    if (atc_type_ < 2 && atc_a_.is_stop_eb_ == 1) {
                        atc_a_.is__brake_reset_ = 1;
                    }
                    if (atc_type_ == 2 && atc_d_.is_stop_eb_ == 1) {
                        atc_d_.is__brake_reset_ = 1;
                    }
                }
                if (Math.Abs(train_spd_) <= 30.0) {
                    if (atc_type_ < 2 && atc_a_.is_stop_svc_ == 1) { atc_a_.is__brake_reset_ = 1; }
                    if (atc_type_ == 2 && atc_d_.is_stop_svc_ == 1) { atc_d_.is__brake_reset_ = 1; }
                }
            }
        }

        /// <summary>
        /// 確認ボタンが押下された際に実行する関数
        /// </summary>
        private void ResetSwDown() {
            Reset();
            if (atc_reset_sw_ == 0) {
                atc_reset_sw_ = 1;
                this.train_.Sounds.AtcSwDownSound.Play();
            }
        }

        /// <summary>
        /// 確認ボタンが開放された際に実行する関数
        /// </summary>
        private void ResetSwUp() {
            if (atc_reset_sw_ == 1) {
                atc_reset_sw_ = 0;
                this.train_.Sounds.AtcSwUpSound.Play();
            }
        }

        /// <summary>
        /// 距離に対応する速度を返す関数
        /// </summary>
        /// <remarks>指定された距離の近似値に対応する速度を検索する</remarks>
        /// <param name="distance">距離[m]</param>
        /// <returns>距離に対応する速度[km/h]</returns>
        private int SearchPattern(double distance) {
            int back_index = (int)BaseFunc.LowerBound(pattern_list_, 1, pattern_list_.Count() - 1, distance, Comparer<double>.Default);
            int front_index = back_index - 1;

            double x = pattern_list_[front_index] - distance;
            double y = pattern_list_[back_index] - distance;

            int approx_index = 0;
            if (x * x < y * y) {
                approx_index = front_index;
            } else {
                approx_index = back_index;
            }
            return approx_index;
        }

        /// <summary>
        /// パターンが無効の場合に目標速度を車両ATC最高速度に修正する関数
        /// </summary>
        /// <param name="tget_spd">目標速度[km/h]</param>
        /// <param name="pattern_status">パターン状態(0: 無効, 1: 有効)</param>
        private void ValidPattern(ref int tget_spd, int pattern_status) {
            if (pattern_status == 0) { tget_spd = atc_max_spd_; }
        }

        /// <summary>
        /// パターン照査速度を算出する関数
        /// </summary>
        /// <remarks>減速完了地点内方は目標速度のフラットパターンとなる</remarks>
        /// <param name="tget_spd">目標速度[km/h]</param>
        /// <param name="pattern_end_loc">減速完了地点[m]</param>
        /// <returns>パターン照査速度[km/h]</returns>
        private int CalcPatternSpd(int tget_spd, double pattern_end_loc) {
            int arrow_spd = 0;
            if (pattern_end_loc <= this.train_.State.Location) {
                if (tget_spd > atc_max_spd_) {
                    arrow_spd = atc_max_spd_;
                } else if (tget_spd < 0) {
                    arrow_spd = 0;
                } else {
                    arrow_spd = tget_spd;
                }
            } else {
                arrow_spd = SearchPattern(BaseFunc.ArrayGetOrDefault(pattern_list_, tget_spd) + pattern_end_loc - this.train_.State.Location);
            }
            return arrow_spd;
        }

        /// <summary>
        /// パターン降下開始地点を算出する関数
        /// </summary>
        /// <param name="tget_spd">目標速度[km/h]</param>
        /// <param name="pattern_end_loc">減速完了地点[m]</param>
        /// <param name="pattern_status">パターン状態(0: 無効, 1: 有効)</param>
        /// <returns>パターン降下開始地点[m]</returns>
        private double CalcPatternLoc(int tget_spd, double pattern_end_loc, int pattern_status) {
            double pattern_start_loc = 0;
            if (pattern_status == 0) {
                pattern_start_loc = double.MaxValue;
            } else if (Math.Abs(train_spd_) <= tget_spd) {
                pattern_start_loc = pattern_end_loc;
            } else {
                pattern_start_loc = BaseFunc.ArrayGetOrDefault(pattern_list_, tget_spd) - BaseFunc.ArrayGetOrDefault(pattern_list_, (int)Math.Abs(train_spd_)) + pattern_end_loc;
            }
            return pattern_start_loc;
        }

        /// <summary>
        /// パターン情報を集約する関数
        /// </summary>
        private void CollectPattern() {
            pattern_tget_spd_list_[0] = 0;
            pattarn_end_loc_list_[0] = atc_d_.section_d_.red_signal_loc_;
            pattern_is_valid_list_[0] = 1;
            for (int i = 0; i < STA_PATTERN; i++) {
                pattern_tget_spd_list_[1 + i] = atc_d_.station_d_.pattern_tget_spd_[i];
                pattarn_end_loc_list_[1 + i] = atc_d_.station_d_.pattern_end_loc_[i];
                pattern_is_valid_list_[1 + i] = atc_d_.station_d_.pattern_is_valid_[i];
            }
            for (int i = 0; i < USR_PATTERN; i++) {
                pattern_tget_spd_list_[1 + STA_PATTERN + i] = atc_d_.pattern_d_.pattern_tget_spd_[i];
                pattarn_end_loc_list_[1 + STA_PATTERN + i] = atc_d_.pattern_d_.pattern_end_loc_[i];
                pattern_is_valid_list_[1 + STA_PATTERN + i] = atc_d_.pattern_d_.pattern_is_valid_[i];
            }
            pattern_tget_spd_list_[1 + STA_PATTERN + USR_PATTERN] = atc_max_spd_;
            pattarn_end_loc_list_[1 + STA_PATTERN + USR_PATTERN] = 0.0;
            pattern_is_valid_list_[1 + STA_PATTERN + USR_PATTERN] = 1;
        }

        /// <summary>
        /// 各パターン情報からパターン照査速度およびパターン降下開始地点を算出する関数
        /// </summary>
        private void CalcPattern() {
            for (int i = 0; i < ALL_PATTERN; i++) {
                ValidPattern(ref pattern_tget_spd_list_[i], pattern_is_valid_list_[i]);
                pattern_arrow_spd_list_[i] = CalcPatternSpd(pattern_tget_spd_list_[i], pattarn_end_loc_list_[i]);
                pattern_start_loc_list_[i] = CalcPatternLoc(pattern_tget_spd_list_[i], pattarn_end_loc_list_[i], pattern_is_valid_list_[i]);
            }
        }

        /// <summary>
        /// デジタルATC用車内信号を生成する関数
        /// </summary>
        private void ChangedArrowSig() {
            int arrow_spd_min = pattern_arrow_spd_list_.Min();
            int index = Array.IndexOf(pattern_arrow_spd_list_, arrow_spd_min);
            if (atc_type_ == 2) {
                if (atc_d_.is_stop_eb_ == 1 && atc_d_.is__brake_reset_ != 1) {
                    atc_d_.arrow_signal_ = 0;
                    atc_d_.arrow_spd_ = 0;
                } else {
                    atc_d_.arrow_signal_ = SearchSignal(arrow_spd_min);
                    atc_d_.arrow_spd_ = ItoV(atc_d_.arrow_signal_);
                }
                atc_d_.tget_signal_ = SearchSignal(pattern_tget_spd_list_[index]);

                // ATCベル
                if (atc_d_.arrow_signal_ != atc_d_.prev_arrow_signal_) {
                    this.train_.Sounds.AtcDing.Play();
                }
            } else {
                atc_d_.arrow_signal_ = SearchSignal(arrow_spd_min);
                atc_d_.arrow_spd_ = arrow_spd_min;
                if (this.train_.State.Location >= BaseFunc.ArrayGetOrDefault(pattern_list_, pattern_tget_spd_list_[index]) - BaseFunc.ArrayGetOrDefault(pattern_list_, atc_max_spd_) + pattarn_end_loc_list_[index]) {
                    atc_d_.tget_signal_ = SearchSignal(pattern_tget_spd_list_[index]);
                    atc_d_.tget_spd_ = pattern_tget_spd_list_[index];
                } else {
                    atc_d_.tget_signal_ = atc_d_.arrow_signal_;
                    atc_d_.tget_spd_ = atc_d_.arrow_spd_;
                }

                // ATCベル
                if (atc_d_.tget_signal_ != atc_d_.prev_tget_signal_) {
                    this.train_.Sounds.AtcDing.Play();
                }
            }
            atc_d_.prev_arrow_signal_ = atc_d_.arrow_signal_;
            atc_d_.prev_tget_signal_ = atc_d_.tget_signal_;
        }

        /// <summary>
        /// パターンに対する最適ブレーキノッチを出力する関数
        /// </summary>
        /// <param name="index">パターン番号</param>
        /// <param name="tget_spd">目標速度[km/h]</param>
        /// <param name="pattern_start_loc">パターン降下開始地点[m]</param>
        /// <param name="pattern_end_loc">減速完了地点[m]</param>
        /// <returns>最適ブレーキノッチ</returns>
        private int CalcBrake(int index, int tget_spd, double pattern_start_loc, double pattern_end_loc) {
            int brake_notch = 0;
            if (Math.Abs(train_spd_) > tget_spd) {
                if (tget_spd != fuzzy_.prev_tget_spd_[index] || pattern_end_loc != fuzzy_.prev_pattern_end_loc_[index]) {
                    fuzzy_.fuzzy_step_[index] = 0;
                    fuzzy_.prev_tget_spd_[index] = tget_spd;
                    fuzzy_.prev_pattern_end_loc_[index] = pattern_end_loc;
                }
                if (fuzzy_.fuzzy_step_[index] == 0) {
                    brake_notch = fuzzy_.FuzzyCtrInit(index, pattern_start_loc);
                } else if (fuzzy_.fuzzy_step_[index] == 1) {
                    brake_notch = fuzzy_.FuzzyCtrExe(index, tget_spd, pattern_end_loc);
                }
            } else {
                fuzzy_.fuzzy_step_[index] = 0;
            }
            return brake_notch;
        }

        /// <summary>
        /// 全パターンに対する最大の最適ブレーキノッチを出力する関数
        /// </summary>
        /// <returns>最適ブレーキノッチ</returns>
        private int SelectBrake() {
            int brake_notch = 0;
            fuzzy_.FuzzyAdjDeceleration();
            for (int i = 0; i < ALL_PATTERN; i++) {
                fuzzy_.fuzzy_brake_notch_list_[i] = CalcBrake(i, pattern_tget_spd_list_[i], pattern_start_loc_list_[i], pattarn_end_loc_list_[i]);
            }
            brake_notch = fuzzy_.fuzzy_brake_notch_list_.Max();
            return brake_notch;
        }

        /// <summary>
        /// 予見Fuzzy制御ではないブレーキノッチを出力する関数
        /// </summary>
        /// <returns>ブレーキノッチ</returns>
        private int NonFuzzyCtrExe() {
            int brake_notch = brake_notch_;
            double default_deceleration = max_deceleration_;
            if (Math.Abs(train_spd_) > 160) {
                //default_deceleration = ((max_deceleration_ / max_brake_notch_) * default_notch_[0]) / 3.6;
                default_deceleration = ((max_deceleration_ / max_brake_notch_) * default_notch_[1]) / 3.6;
            } else if (Math.Abs(train_spd_) > 110) {
                default_deceleration = ((max_deceleration_ / max_brake_notch_) * default_notch_[1]) / 3.6;
            } else if (Math.Abs(train_spd_) > 70) {
                default_deceleration = ((max_deceleration_ / max_brake_notch_) * default_notch_[2]) / 3.6;
            } else {
                default_deceleration = ((max_deceleration_ / max_brake_notch_) * default_notch_[3]) / 3.6;
            }

            // 必要ブレーキノッチの決定
            int req_brake_notch = (int)(Math.Ceiling((default_deceleration + fuzzy_.adj_deceleration_) / (max_deceleration_ / 3.6) * max_brake_notch_));
            if (req_brake_notch < 0) { req_brake_notch = 0; }
            if (req_brake_notch > max_brake_notch_) { req_brake_notch = max_brake_notch_; }

            // ブレーキノッチの出力
            if (brake_notch < req_brake_notch && train_.Atc.time_ > brake_timer_) {
                brake_notch++;
                brake_timer_ = train_.Atc.time_ + 1000.0;
            }
            return brake_notch;
        }

        /// <summary>
        /// アナログATC用のブレーキノッチを出力する関数
        /// </summary>
        private void BrakeExeA() {
            fuzzy_.FuzzyAdjDeceleration();
            if ((atc_a_.is_stop_eb_ == 1 || atc_a_.is_stop_svc_ == 1) && atc_a_.is__brake_reset_ == 1) {
                atc_brake_notch_ = 0;
            } else if (atc_a_.is_stop_eb_ == 1) {
                atc_brake_notch_ = max_brake_notch_ + 1;
            } else if (atc_a_.is_stop_svc_ == 1) {
                atc_brake_notch_ = max_brake_notch_;
            } else if (Math.Abs(train_spd_) > ItoV(atc_a_.signal_) || Math.Abs(train_spd_) > atc_max_spd_) {
                atc_brake_notch_ = NonFuzzyCtrExe();
            } else {
                atc_brake_notch_ = 0;
            }
        }

        /// <summary>
        /// デジタルATC用のブレーキノッチを出力する関数
        /// </summary>
        private void BrakeExeD() {
            atc_d_.AtcCheck();
            if ((atc_d_.is_stop_eb_ == 1 || atc_d_.is_stop_svc_ == 1) && atc_d_.is__brake_reset_ == 1) {
                atc_brake_notch_ = 0;
            } else if (atc_d_.is_stop_eb_ == 1) {
                atc_brake_notch_ = max_brake_notch_ + 1;
            } else if (atc_d_.is_stop_svc_ == 1) {
                atc_brake_notch_ = max_brake_notch_;
            } else {
                atc_brake_notch_ = SelectBrake();
            }
        }

        /// <summary>
        /// 速度を7セグに表示する関数
        /// </summary>
        /// <param name="spd">速度[km/h]</param>
        /// <param name="display_spd">速度を表示する配列</param>
        private void DisplaySpd(int spd, int[] display_spd) {
            if (display_spd.Length == 3) {
                int spd100 = spd / 100;
                int spd10 = (spd % 100) / 10;
                int spd1 = (spd % 100) % 10;
                if (spd100 >= 10) {
                    for (int i = 0; i < 3; i++) {
                        display_spd[i] = 9;
                    }
                } else {
                    display_spd[0] = (spd100 == 0) ? 10 : spd100;
                    display_spd[1] = (spd100 == 0 && spd10 == 0) ? 10 : spd10;
                    display_spd[2] = spd1;
                }
            }
        }

        /// <summary>
        /// インジケーターの表示を初期化する関数
        /// </summary>
        private void ResetIndicator() {
            atc_red_signal_ = 0;
            atc_green_signal_ = 0;
            for (int i = 0; i < atc_sig_indicator_.Length; i++) {
                atc_sig_indicator_[i] = 0;
            }
        }

        /// <summary>
        /// インジケーターの表示を実行する関数
        /// </summary>
        private void RunIndicator() {
            if (atc_type_ < 2) {
                RunIndicatorA();
            } else {
                RunIndicatorD();
            }
        }

        /// <summary>
        /// アナログATC用インジケーターの表示を実行する関数
        /// </summary>
        private void RunIndicatorA() {
            if (atc_a_.signal_ == 0) {
                atc_red_signal_ = 1;
            } else {
                atc_green_signal_ = 1;
            }
            atc_sig_indicator_[atc_a_.signal_] = 1;
            atc_signal_index_ = atc_a_.signal_;
            atc_spd_ = ItoV(atc_a_.signal_, true);
            DisplaySpd(ItoV(atc_a_.signal_, true), atc_spd_7seg_);
        }

        /// <summary>
        /// デジタルATC用インジケーターの表示を実行する関数
        /// </summary>
        private void RunIndicatorD() {
            if (atc_d_.section_d_.track_path_ <= 1) {
                atc_red_signal_ = 1;
            } else {
                atc_green_signal_ = 1;
            }
            if (atc_type_ == 2) {
                atc_sig_indicator_[atc_d_.arrow_signal_] = 1;
                atc_signal_index_ = atc_d_.arrow_signal_;
                atc_spd_ = atc_d_.arrow_spd_;
                DisplaySpd(atc_d_.arrow_spd_, atc_spd_7seg_);
            } else {
                atc_sig_indicator_[atc_d_.tget_signal_] = 1;
                atc_signal_index_ = atc_d_.tget_signal_;
                atc_spd_ = atc_d_.tget_spd_;
                DisplaySpd(atc_d_.tget_spd_, atc_spd_7seg_);
                if (atc_type_ == 4) {
                    dsatc_arrow_spd_ = atc_d_.arrow_spd_;
                }
            }
        }

        /// <summary>
        /// 副速度計の表示を実行する関数
        /// </summary>
        private void RunSubSpeedMeter() {
            if (atc_spd_ <= 10) {
                DisplaySpd(0, sub_spd_label_1_);
                DisplaySpd(10, sub_spd_label_2_);
                DisplaySpd(20, sub_spd_label_3_);
                DisplaySpd(30, sub_spd_label_4_);
                sub_atc_spd_ = atc_spd_;
                sub_train_spd_ = (int)Math.Abs(train_spd_);
                if (sub_train_spd_ > 30) { sub_train_spd_ = 30; }
            } else {
                DisplaySpd(atc_spd_ - 20, sub_spd_label_1_);
                DisplaySpd(atc_spd_ - 10, sub_spd_label_2_);
                DisplaySpd(atc_spd_, sub_spd_label_3_);
                DisplaySpd(atc_spd_ + 10, sub_spd_label_4_);
                sub_atc_spd_ = 20;
                sub_train_spd_ = (int)(Math.Abs(train_spd_) - (atc_spd_ - 20));
                if (sub_train_spd_ < 0) { sub_train_spd_ = 0; }
                if (sub_train_spd_ > 30) { sub_train_spd_ = 30; }
            }
        }

        /// <summary>
        /// SetBeaconDataの実行タイミングを制御するための関数
        /// </summary>
        private void PassedBeacon() {
            while (beacon_type_.Count != 0 || beacon_sig_.Count != 0 || beacon_dist_.Count != 0 || beacon_opt_.Count != 0) {
                PassedBeacon(beacon_type_.First(), beacon_sig_.First(), beacon_dist_.First(), beacon_opt_.First());
                beacon_type_.RemoveAt(0);
                beacon_sig_.RemoveAt(0);
                beacon_dist_.RemoveAt(0);
                beacon_opt_.RemoveAt(0);
            }
        }

        // --- 継承された関数 ---
        /// <summary>
        /// ゲーム開始時に呼び出される関数
        /// </summary>
        /// <param name="mode">初期化モード</param>
        internal override void Initialize(InitializationModes mode) {
            max_deceleration_ = LoadConfig.MaxDeceleration / 1000.0;
            lever_delay_ = LoadConfig.LeverDelay / 1000.0;
            atc_power_ = 1;
            atc_use_ = LoadConfig.AtcUse;
            atc_type_ = LoadConfig.AtcType;
            atc_max_spd_ = LoadConfig.AtcMax;
            atc_spd_list_ = new int[LoadConfig.AtcSpeed.Length];
            Array.Copy(LoadConfig.AtcSpeed, atc_spd_list_, LoadConfig.AtcSpeed.Length);
            atc_deceleration_ = new double[LoadConfig.AtcDeceleration.Length];
            for (int i = 0; i < LoadConfig.AtcDeceleration.Length; i++) {
                atc_deceleration_[i] = LoadConfig.AtcDeceleration[i] / 1000.0;
            }
            Xe = LoadConfig.Xe / 1000.0;
            Xo = LoadConfig.Xo / 1000.0;
            Xk = LoadConfig.Xk / 1000.0;

            max_brake_notch_ = this.train_.Specs.BrakeNotches - this.train_.Specs.AtsNotch + 1;
            default_notch_ = new int[atc_deceleration_.Length];
            for (int i = 0; i < atc_deceleration_.Length; i++) {
                default_notch_[i] = (int)Math.Round((atc_deceleration_[i] / max_deceleration_) * max_brake_notch_);
            }
            max_signal_ = SearchSignal(atc_max_spd_);
            pattern_list_ = new double[atc_max_spd_ + 1];
            SetPatternList();
            pattarn_end_loc_list_ = new double[ALL_PATTERN];
            pattern_is_valid_list_ = new int[ALL_PATTERN];
            pattern_tget_spd_list_ = new int[ALL_PATTERN];
            pattern_arrow_spd_list_ = new int[ALL_PATTERN];
            pattern_start_loc_list_ = new double[ALL_PATTERN];
            brake_notch_ = 0;
            debug_timer_ = 0;
            brake_timer_ = 0;
            atc_reset_sw_ = 0;
            atc_red_signal_ = 0;
            atc_green_signal_ = 0;
            atc_eb_lamp_ = 0;
            atc_svc_lamp_ = 0;
            atc_brake_notch_ = 0;
            atc_spd_ = 0;
            atc_signal_index_ = 0;
            atc_sig_indicator_ = new int[atc_spd_list_.Length];
            atc_spd_7seg_ = new int[3];
            sub_spd_label_1_ = new int[3];
            sub_spd_label_2_ = new int[3];
            sub_spd_label_3_ = new int[3];
            sub_spd_label_4_ = new int[3];
            sub_atc_spd_ = 0;
            sub_train_spd_ = 0;
            atc_a_.Init();
            atc_d_.Init();
            fuzzy_.Init();
            beacon_type_ = new List<int>();
            beacon_sig_ = new List<int>();
            beacon_dist_ = new List<double>();
            beacon_opt_ = new List<int>();
        }

        /// <summary>
        /// 1フレームごとに呼び出される関数
        /// </summary>
        /// <param name="data">The data.</param>
        /// <param name="blocking">保安装置がブロックされているか、後続の保安装置をブロックするかどうか。</param>
        internal override void Elapse(ElapseData data, ref bool blocking) {
            train_spd_ = data.Vehicle.Speed.KilometersPerHour;
            time_ = data.TotalTime.Milliseconds;
            int raw_brake_notch = (this.train_.Handles.BrakeNotch != 0) ? (this.train_.Handles.BrakeNotch - this.train_.Specs.AtsNotch + 1) : 0;
            brake_notch_ = (atc_brake_notch_ > raw_brake_notch) ? atc_brake_notch_ : raw_brake_notch;

            PassedBeacon();

            if (atc_use_ == 0) {
                atc_brake_notch_ = 0;
            } else {
                if (atc_type_ < 2) {
                    BrakeExeA();
                } else {
                    atc_d_.section_d_.CalcSection();
                    atc_d_.station_d_.IsStopSta();
                    CollectPattern();
                    CalcPattern();
                    ChangedArrowSig();
                    BrakeExeD();
                }
                ResetIndicator();
                RunIndicator();
                RunSubSpeedMeter();

                atc_eb_lamp_ = (atc_brake_notch_ == max_brake_notch_) ? 1 : 0;  // ATC非常
                atc_svc_lamp_ = (atc_brake_notch_ > 0) ? 1 : 0;  // ATC常用

                // For debug
                if (time_ >= debug_timer_) {
                    if (atc_type_ < 2) {
                        Trace.WriteLine("Loc: " + (float)this.train_.State.Location + " / Accel: " + (float)this.train_.Accel.ema_accel_ + " / TargetSpd: " + ItoV(atc_a_.signal_));
                        Trace.WriteLine("出力B: B" + atc_brake_notch_ + " / is_stop_eb_: " + atc_a_.is_stop_eb_ + " / is_stop_svc_: " + atc_a_.is_stop_svc_ + " / is__brake_reset_: " + atc_a_.is__brake_reset_);
                    } else {
                        int arrow_spd = pattern_arrow_spd_list_.Min();
                        int index = Array.IndexOf(pattern_arrow_spd_list_, arrow_spd);
                        int tget_spd = pattern_tget_spd_list_[index];
                        float pattern_start_loc = (float)pattern_start_loc_list_[index];
                        float pattern_end_loc = (float)pattarn_end_loc_list_[index];
                        index = Array.IndexOf(fuzzy_.fuzzy_brake_notch_list_, fuzzy_.fuzzy_brake_notch_list_.Max());
                        Trace.WriteLine("Loc: " + (float)this.train_.State.Location + " / Accel: " + (float)this.train_.Accel.ema_accel_ + " / TargetSpd: " + tget_spd + " / PattStart: " + pattern_start_loc + " / PattEnd: " + pattern_end_loc + " / Arrow: " + arrow_spd);
                        Trace.WriteLine("Index: " + index + " / 出力B: B" + atc_brake_notch_ + " / is_stop_eb_: " + atc_d_.is_stop_eb_ + " / is_stop_svc_: " + atc_d_.is_stop_svc_ + " / is__brake_reset_: " + atc_d_.is__brake_reset_);
                    }
                    debug_timer_ = time_ + 1000.0;
                }
            }

            if (atc_brake_notch_ > raw_brake_notch) {
                data.Handles.BrakeNotch = atc_brake_notch_ + this.train_.Specs.AtsNotch - 1;
                data.Handles.PowerNotch = 0;
                data.Handles.ConstSpeed = false;
                blocking = true;
            }

            // --- パネル ---
            this.train_.Panel[13] = atc_power_;  // ATC電源
            this.train_.Panel[14] = atc_use_;  // ATC
            this.train_.Panel[9] = atc_reset_sw_;  // ATC確認ボタン
            this.train_.Panel[15] = atc_eb_lamp_;  // ATC非常
            this.train_.Panel[16] = atc_svc_lamp_;  // ATC常用
            this.train_.Panel[17] = atc_red_signal_;  // ATC停止現示
            this.train_.Panel[18] = atc_green_signal_;  // ATC進行現示
            this.train_.Panel[19] = atc_spd_;  // ATC速度
            this.train_.Panel[20] = atc_sig_indicator_[0]; // ATC-01
            this.train_.Panel[21] = atc_sig_indicator_[1]; // ATC-30
            this.train_.Panel[22] = atc_sig_indicator_[2]; // ATC-70
            this.train_.Panel[23] = atc_sig_indicator_[3]; // ATC-120 (東北, 上越, 北陸: ATC-110)
            this.train_.Panel[24] = atc_sig_indicator_[4]; // ATC-170 (東北, 上越, 北陸: ATC-160)
            this.train_.Panel[25] = atc_sig_indicator_[5]; // ATC-220 (東北, 上越, 北陸: ATC-210)
            this.train_.Panel[26] = atc_sig_indicator_[6]; // ATC-230 (東北, 上越, 北陸: ATC-240)
            this.train_.Panel[27] = atc_sig_indicator_[7]; // ATC-255 (東北, 上越: ATC-245, 北陸: ATC-260)
            this.train_.Panel[28] = atc_sig_indicator_[8]; // ATC-270 (東北, 上越: ATC-275, 北陸: ATC-260)
            this.train_.Panel[29] = atc_sig_indicator_[9]; // ATC-275 (東北, 上越: ATC-300, 北陸: ATC-260)
            this.train_.Panel[30] = atc_sig_indicator_[10]; // ATC-285 (東北, 上越: ATC-320, 北陸: ATC-260)
            this.train_.Panel[31] = atc_sig_indicator_[11]; // ATC-300 (東北, 上越: ATC-360, 北陸: ATC-260)
            this.train_.Panel[33] = atc_type_;  // ATC方式
            this.train_.Panel[34] = atc_signal_index_;  // ATC速度のインデックス
            this.train_.Panel[35] = atc_spd_7seg_[0];  // ATC速度の百の位
            this.train_.Panel[36] = atc_spd_7seg_[1];  // ATC速度の十の位
            this.train_.Panel[37] = atc_spd_7seg_[2];  // ATC速度の一の位
            this.train_.Panel[38] = sub_spd_label_1_[0];  // 副速度計用目盛 ATC速度-20 km/hの百の位
            this.train_.Panel[39] = sub_spd_label_1_[1];  // 副速度計用目盛 ATC速度-20 km/hの十の位
            this.train_.Panel[40] = sub_spd_label_1_[2];  // 副速度計用目盛 ATC速度-20 km/hの一の位
            this.train_.Panel[41] = sub_spd_label_2_[0];  // 副速度計用目盛 ATC速度-10 km/hの百の位
            this.train_.Panel[42] = sub_spd_label_2_[1];  // 副速度計用目盛 ATC速度-10 km/hの十の位
            this.train_.Panel[43] = sub_spd_label_2_[2];  // 副速度計用目盛 ATC速度-10 km/hの一の位
            this.train_.Panel[44] = sub_spd_label_3_[0];  // 副速度計用目盛 ATC速度の百の位
            this.train_.Panel[45] = sub_spd_label_3_[1];  // 副速度計用目盛 ATC速度の十の位
            this.train_.Panel[46] = sub_spd_label_3_[2];  // 副速度計用目盛 ATC速度の一の位
            this.train_.Panel[47] = sub_spd_label_4_[0];  // 副速度計用目盛 ATC速度+10 km/hの百の位
            this.train_.Panel[48] = sub_spd_label_4_[1];  // 副速度計用目盛 ATC速度+10 km/hの十の位
            this.train_.Panel[49] = sub_spd_label_4_[2];  // 副速度計用目盛 ATC速度+10 km/hの一の位
            this.train_.Panel[50] = sub_atc_spd_;  // 副速度計用 ATC速度
            this.train_.Panel[51] = sub_train_spd_;  // 副速度計用 車両速度
            this.train_.Panel[52] = (atc_type_ == 4) ? dsatc_arrow_spd_ : 0;  // DS-ATC用パターン照査速度
            this.train_.Panel[120] = atc_sig_indicator_[0]; // ATC-01
            this.train_.Panel[126] = atc_sig_indicator_[1]; // ATC-30
            this.train_.Panel[134] = atc_sig_indicator_[2]; // ATC-70
            this.train_.Panel[142] = (atc_type_ == 1 || atc_type_ == 4) ? atc_sig_indicator_[3] : 0; // 東北, 上越, 北陸: ATC-110
            this.train_.Panel[144] = (atc_type_ != 1 && atc_type_ != 4) ? atc_sig_indicator_[3] : 0; // 東海, 山陽:       ATC-120
            this.train_.Panel[152] = (atc_type_ == 1 || atc_type_ == 4) ? atc_sig_indicator_[4] : 0; // 東北, 上越, 北陸: ATC-160
            this.train_.Panel[154] = (atc_type_ != 1 && atc_type_ != 4) ? atc_sig_indicator_[4] : 0; // 東海, 山陽:       ATC-170
            this.train_.Panel[162] = (atc_type_ == 1 || atc_type_ == 4) ? atc_sig_indicator_[5] : 0; // 東北, 上越, 北陸: ATC-210
            this.train_.Panel[164] = (atc_type_ != 1 && atc_type_ != 4) ? atc_sig_indicator_[5] : 0; // 東海, 山陽:       ATC-220
            this.train_.Panel[166] = (atc_type_ != 1 && atc_type_ != 4) ? atc_sig_indicator_[6] : 0; // 東海, 山陽:       ATC-230
            this.train_.Panel[168] = (atc_type_ == 1 || atc_type_ == 4) ? atc_sig_indicator_[6] : 0; // 東北, 上越, 北陸: ATC-240
            this.train_.Panel[169] = (atc_type_ == 1 || atc_type_ == 4) ? atc_sig_indicator_[7] : 0; // 東北, 上越:       ATC-245
            this.train_.Panel[171] = (atc_type_ != 1 && atc_type_ != 4) ? atc_sig_indicator_[7] : 0; // 東海, 山陽:       ATC-255
            this.train_.Panel[172] = (atc_type_ == 1 || atc_type_ == 4) ? atc_sig_indicator_[7] : 0; // 北陸:             ATC-260
            this.train_.Panel[174] = (atc_type_ != 1 && atc_type_ != 4) ? atc_sig_indicator_[8] : 0; // 東海, 山陽:       ATC-270
            this.train_.Panel[175] = (atc_type_ == 1 || atc_type_ == 4) ? atc_sig_indicator_[8] : atc_sig_indicator_[9]; // 東海, 山陽, 東北, 上越: ATC-275
            this.train_.Panel[177] = (atc_type_ != 1 && atc_type_ != 4) ? atc_sig_indicator_[10] : 0; // 東海, 山陽: ATC-285
            this.train_.Panel[180] = (atc_type_ == 1 || atc_type_ == 4) ? atc_sig_indicator_[9] : atc_sig_indicator_[11]; // 東海, 山陽, 東北, 上越: ATC-300
            this.train_.Panel[184] = (atc_type_ == 1 || atc_type_ == 4) ? atc_sig_indicator_[10] : 0; // 東北, 上越: ATC-320
            this.train_.Panel[192] = (atc_type_ == 1 || atc_type_ == 4) ? atc_sig_indicator_[11] : 0; // 東北, 上越: ATC-360
        }

        /// <summary>
        /// レバーサーが扱われたときに呼び出される関数
        /// </summary>
        /// <param name="reverser">レバーサ位置</param>
        internal override void SetReverser(int reverser) {
        }

        /// <summary>
        /// 主ハンドルが扱われたときに呼び出される関数
        /// </summary>
        /// <param name="powerNotch">力行ノッチ</param>
        internal override void SetPower(int powerNotch) {
        }

        /// <summary>
        /// ブレーキが扱われたときに呼び出される関数
        /// </summary>
        /// <param name="brakeNotch">ブレーキノッチ</param>
        internal override void SetBrake(int brakeNotch) {
        }

        /// <summary>
        /// ATSキーが押されたときに呼び出される関数
        /// </summary>
        /// <param name="key">ATSキー</param>
        internal override void KeyDown(VirtualKeys key) {
            switch (key) {
            case VirtualKeys.S:  // ATC確認
                ResetSwDown();
                break;
            default:
                break;
            }
        }

        /// <summary>
        /// ATSキーが離されたときに呼び出される関数
        /// </summary>
        /// <param name="key">ATSキー</param>
        internal override void KeyUp(VirtualKeys key) {
            switch (key) {
            case VirtualKeys.S:  // ATC確認
                ResetSwUp();
                break;
            default:
                break;
            }
        }

        /// <summary>
        /// 警笛が扱われたときに呼び出される関数
        /// </summary>
        /// <param name="type">警笛のタイプ</param>
        internal override void HornBlow(HornTypes type) {
        }

        /// <summary>
        /// 現在の閉塞の信号が変化したときに呼び出される関数
        /// </summary>
        /// <param name="signal">信号番号</param>
        internal override void SetSignal(SignalData[] signal) {
            if (atc_power_ == 1 && atc_use_ == 1) {
                ChangedSignal(signal[0].Aspect);
            }
        }

        /// <summary>
        /// 地上子を越えたときに呼び出される関数
        /// </summary>
        /// <param name="beacon">車上子で受け取った情報</param>
        internal override void SetBeacon(BeaconData beacon) {
            beacon_type_.Add(beacon.Type);
            beacon_sig_.Add(beacon.Signal.Aspect);
            beacon_dist_.Add(beacon.Signal.Distance);
            beacon_opt_.Add(beacon.Optional);
        }
    }
}