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
    /// ATS-Pを再現するクラス
    /// </summary>
    internal class AtsP : Device {

        // --- クラス ---
        /// <summary>
        /// 停止信号パターン関連を記述するクラス
        /// </summary>
        private class SectionP {

            // --- メンバ ---
            private readonly Train train_;
            private double red_signal_loc_;  //!< 停止信号地点[m]
            internal int is_immediate_stop_eb_;  //!< 即時停止(非常)フラグ
            internal int is_immediate_stop_svc_;  //!< 即時停止(常用)フラグ
            internal int is_immediate_stop_;  //!< 即時停止(非常, 常用)フラグ
            internal int is_stop_svc_;  //!< パターン接触フラグ
            internal int is__brake_reset_;  //!< ブレーキ開放フラグ
            internal double pattern_end_loc_;  //!< 減速完了地点[m]
            internal int pattern_is_valid_;  //!< パターンの状態(0: 無効, 1: 有効)
            internal int pattern_tget_spd_;  //!< 目標速度[km/h]

            // --- コンストラクタ ---
            /// <summary>
            /// 新しいインスタンスを作成する
            /// </summary>
            /// <param name="train">Trainクラスのインスタンス</param>
            internal SectionP(Train train) {
                this.train_ = train;
            }

            // --- 関数 ---
            /// <summary>
            /// Initializeで実行する関数
            /// </summary>
            internal void Init() {
                red_signal_loc_ = 0.0;
                is_immediate_stop_eb_ = 0;
                is_immediate_stop_svc_ = 0;
                is_immediate_stop_ = 0;
                is_stop_svc_ = 0;
                is__brake_reset_ = 0;
                pattern_end_loc_ = 0.0;
                pattern_is_valid_ = 0;
                pattern_tget_spd_ = 0;
            }

            /// <summary>
            /// 停止信号地点を取得する関数
            /// </summary>
            /// <param name="distance">停止信号までの距離[m]</param>
            internal void GetSection(double distance) {
                red_signal_loc_ = this.train_.State.Location + distance;
            }

            /// <summary>
            /// 即時停止(非常)地上子を通過した際に実行する関数
            /// </summary>
            /// <param name="distance">停止信号までの距離[m]</param>
            internal void PassedStopEb(double distance) {
                GetSection(distance);
                if (distance < 50.0f) {
                    is_immediate_stop_eb_ = 1;
                    is__brake_reset_ = 0;
                }
            }

            /// <summary>
            /// 即時停止(常用)地上子を通過した際に実行する関数
            /// </summary>
            /// <param name="distance">停止信号までの距離[m]</param>
            internal void PassedStopSvc(double distance) {
                GetSection(distance);
                if (distance < 50.0f) {
                    is_immediate_stop_svc_ = 1;
                    is__brake_reset_ = 0;
                }
            }

            /// <summary>
            /// 停止信号パターンを生成する関数
            /// </summary>
            internal void CalcSection() {
                if (this.train_.State.Location <= red_signal_loc_) {
                    if (is_immediate_stop_ == 1) {
                        pattern_end_loc_ = red_signal_loc_ - 50.0f;
                        pattern_is_valid_ = 1;
                        pattern_tget_spd_ = 10;
                    } else {
                        pattern_end_loc_ = red_signal_loc_ - 10.0f;
                        pattern_is_valid_ = 1;
                        pattern_tget_spd_ = 10;
                    }
                } else if (this.train_.State.Location > red_signal_loc_ && this.train_.State.Location <= (red_signal_loc_ + 50.0f)) {
                    pattern_end_loc_ = red_signal_loc_;
                    pattern_is_valid_ = 1;
                    pattern_tget_spd_ = 15;
                } else {
                    pattern_is_valid_ = 0;
                    is_immediate_stop_ = 0;
                    is__brake_reset_ = 0;
                }
            }
        }

        /// <summary>
        /// 制限速度パターン関連を記述するクラス
        /// </summary>
        internal class PatternP {

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
            internal PatternP(Train train) {
                this.train_ = train;
            }

            // --- 関数 ---
            /// <summary>
            /// Initializeで実行する関数
            /// </summary>
            internal void Init() {
                pattern_end_loc_ = new double[AtsP.USR_PATTERN_P];
                pattern_is_valid_ = new int[AtsP.USR_PATTERN_P];
                pattern_tget_spd_ = new int[AtsP.USR_PATTERN_P];
            }

            /// <summary>
            /// 速度制限パターンの登録を行う関数
            /// </summary>
            /// <param name="type">パターン番号</param>
            /// <param name="optional">減速完了地点までの相対距離[m]*1000+目標速度[km/h]</param>
            internal void RegPattern(int type, int optional) {
                int distance = optional / 1000;
                if (distance < 0) { distance = 0; }
                int tget_spd = optional % 1000;
                pattern_end_loc_[type] = this.train_.State.Location + distance;
                pattern_tget_spd_[type] = tget_spd;
                pattern_is_valid_[type] = 1;
            }

            /// <summary>
            /// 速度制限パターンの消去を行う関数
            /// </summary>
            /// <param name="type">パターン番号</param>
            internal void DelPattern(int type) {
                pattern_is_valid_[type] = 0;
            }
        }

        // --- メンバ ---
        private readonly Train train_;
        private readonly SectionP section_p_;
        private readonly PatternP pattern_p_;
        private const int ALL_PATTERN_P = 7;  //!< パターンの総数
        private const int USR_PATTERN_P = 5;  //!< 速度制限パターンの総数
        private int max_brake_notch_;  //!< 常用最大ブレーキノッチ(HBを含まない)
        private int default_notch_;  //!< 標準ブレーキノッチ
        private double[] pattern_list_;  //!< 速度照査パターン
        private double adj_deceleration_;  //!< 各ブレーキノッチの減速度補正値[m/s^2]
        private double[] pattern_end_loc_list_;  //!< 全パターンの減速完了地点[m]
        private int[] pattern_is_valid_list_;  //!< 全パターンの状態(0: 無効, 1: 有効)
        private int[] pattern_tget_spd_list_;  //!< 全パターンの目標速度[km/h]
        private int[] pattern_pre_spd_list_;  //!< 全パターンのパターン照査速度[km/h]
        private int[] pattern_aprch_list_;  //!< 全パターンのパターン接近警報の状態(0: 無効, 1: 有効)
        private int prev_aprch_lamp_;  //!< 以前のパターン接近灯(0: 消灯, 1: 点灯)
        private double debug_timer_;  //!< Debug出力する次のゲーム内時刻[ms]
        private double max_deceleration_;  //!< 常用最大減速度[km/h/s]
        private int atsp_power_;  //!< ATS-P電源(0: 消灯, 1: 点灯)
        private int atsp_use_;  //!< ATS-P(0: 消灯, 1: 点灯)
        private int atsp_max_spd_;  //!< 車両ATS-P最高速度[km/h]
        private double atsp_deceleration_;  //!< ATS-Pブレーキ減速度[km/h/s]
        private int atsp_brake_notch_;  //!< ATS-P出力ブレーキノッチ(HBを含まない)
        private int atsp_aprch_lamp_;  //!< パターン接近(0: 消灯, 1: 点灯)
        private int atsp_brake_lamp_;  //!< ブレーキ作動(0: 消灯, 1: 点灯)
        private int atsp_reset_lamp_;  //!< ブレーキ解放(0: 消灯, 1: 点灯)

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
        internal AtsP(Train train) {
            this.train_ = train;
            this.section_p_ = new SectionP(train);
            this.pattern_p_ = new PatternP(train);
        }

        // --- 関数 ---
        /// <summary>
        /// 速度照査パターンを作成する関数
        /// </summary>
        private void SetPatternList() {
            for (int v = 0; v < atsp_max_spd_ + 1; v++) {
                pattern_list_[v] = ((v / 3.6) * (v / 3.6)) / (2.0 * (((max_deceleration_ / max_brake_notch_) * default_notch_) / 3.6 + adj_deceleration_));
            }
        }

        /// <summary>
        /// ATS-Pを投入する際に実行する関数
        /// </summary>
        private void Start() {
            if (atsp_use_ == 0) {
                Initialize(0);
                atsp_use_ = 1;
                this.train_.Sounds.AtspDing.Play();  // ATS-Pベル
            }
        }

        /// <summary>
        /// ATS-Pを遮断する際に実行する関数
        /// </summary>
        private void Exit() {
            if (atsp_use_ == 1) {
                atsp_use_ = 0;
                Initialize(0);
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
            case 3:
                section_p_.GetSection(distance);
                break;
            case 4:
                section_p_.PassedStopEb(distance);
                break;
            case 5:
                section_p_.PassedStopSvc(distance);
                break;
            case 6:
                pattern_p_.RegPattern(0, optional);
                break;
            case 7:
                pattern_p_.RegPattern(1, optional);
                break;
            case 8:
                pattern_p_.RegPattern(2, optional);
                break;
            case 9:
                pattern_p_.RegPattern(3, optional);
                break;
            case 10:
                pattern_p_.RegPattern(4, optional);
                break;
            case 16:
                pattern_p_.DelPattern(0);
                break;
            case 18:
                pattern_p_.DelPattern(2);
                break;
            case 19:
                pattern_p_.DelPattern(3);
                break;
            case 20:
                pattern_p_.DelPattern(4);
                break;
            case 203:
                AdjDeceleration(optional);
                break;
            default:
                break;
            }
        }

        /// <summary>
        /// 勾配補正設定を行う関数
        /// </summary>
        /// <param name="deceleration">減速度補正値[m/s^2]*1000</param>
        private void AdjDeceleration(int deceleration) {
            if (deceleration > 0) {
                adj_deceleration_ = 0.0;
            } else if (deceleration < -35) {
                adj_deceleration_ = -0.035;
            } else {
                adj_deceleration_ = deceleration / 1000.0;
            }
            SetPatternList();
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
        /// パターンが無効の場合に目標速度を車両ATS-P最高速度に修正する関数
        /// </summary>
        /// <param name="tget_spd">目標速度[km/h]</param>
        /// <param name="pattern_status">パターン状態(0: 無効, 1: 有効)</param>
        private void ValidPattern(ref int tget_spd, int pattern_status) {
            if (pattern_status == 0) { tget_spd = atsp_max_spd_; }
        }

        /// <summary>
        /// パターン照査速度を算出する関数
        /// </summary>
        /// <remarks>減速完了地点内方は目標速度のフラットパターンとなる</remarks>
        /// <param name="tget_spd">目標速度[km/h]</param>
        /// <param name="pattern_end_loc">減速完了地点[m]</param>
        /// <returns>パターン照査速度[km/h]</returns>
        private int CalcPatternSpd(int tget_spd, double pattern_end_loc) {
            int pattern_spd = 0;
            if (pattern_end_loc <= this.train_.State.Location) {
                if (tget_spd > atsp_max_spd_) {
                    pattern_spd = atsp_max_spd_;
                } else if (tget_spd < 0) {
                    pattern_spd = 0;
                } else {
                    pattern_spd = tget_spd;
                }
            } else {
                pattern_spd = SearchPattern(BaseFunc.ArrayGetOrDefault(pattern_list_, tget_spd) + pattern_end_loc - this.train_.State.Location);
            }
            return pattern_spd;
        }

        /// <summary>
        /// パターン接近警報を発信する関数
        /// </summary>
        /// <param name="tget_spd">目標速度[km/h]</param>
        /// <param name="pattern_end_loc">減速完了地点[m]</param>
        /// <returns>パターン接近警報の状態(0: 無効, 1: 有効)</returns>
        private int IsAprchPattern(int tget_spd, double pattern_end_loc) {
            int aprch_pattern = 0;
            int aprchSpd1 = CalcPatternSpd(tget_spd - 5, pattern_end_loc - train_spd_ / 3.6 * 5.5);
            int aprchSpd2 = CalcPatternSpd(tget_spd - 5, pattern_end_loc - 50.0);
            if (Math.Abs(train_spd_) >= aprchSpd1 || Math.Abs(train_spd_) >= aprchSpd2) {
                aprch_pattern = 1;
            }
            return aprch_pattern;
        }

        /// <summary>
        /// パターン情報を集約する関数
        /// </summary>
        private void CollectPattern() {
            pattern_tget_spd_list_[0] = section_p_.pattern_tget_spd_;
            pattern_end_loc_list_[0] = section_p_.pattern_end_loc_;
            pattern_is_valid_list_[0] = section_p_.pattern_is_valid_;
            for (int i = 0; i < USR_PATTERN_P; i++) {
                pattern_tget_spd_list_[1 + i] = pattern_p_.pattern_tget_spd_[i];
                pattern_end_loc_list_[1 + i] = pattern_p_.pattern_end_loc_[i];
                pattern_is_valid_list_[1 + i] = pattern_p_.pattern_is_valid_[i];
            }
            pattern_tget_spd_list_[1 + USR_PATTERN_P] = atsp_max_spd_;
            pattern_end_loc_list_[1 + USR_PATTERN_P] = 0.0;
            pattern_is_valid_list_[1 + USR_PATTERN_P] = 1;
        }

        /// <summary>
        /// 各パターン情報からパターン照査速度を算出し、パターン接近警報の状態を取得する関数
        /// </summary>
        private void CalcPattern() {
            for (int i = 0; i < ALL_PATTERN_P; i++) {
                ValidPattern(ref pattern_tget_spd_list_[i], pattern_is_valid_list_[i]);
                pattern_pre_spd_list_[i] = CalcPatternSpd(pattern_tget_spd_list_[i], pattern_end_loc_list_[i]);
                pattern_aprch_list_[i] = IsAprchPattern(pattern_tget_spd_list_[i], pattern_end_loc_list_[i]);
            }
        }

        /// <summary>
        /// フラグのON, OFFを行う関数
        /// </summary>
        private void AtspCheck() {
            if (Math.Abs(train_spd_) > pattern_pre_spd_list_[0] && pattern_pre_spd_list_[0] != atsp_max_spd_) {
                section_p_.is_stop_svc_ = 1;
                section_p_.is__brake_reset_ = 0;
            }
            if (section_p_.is_immediate_stop_eb_ == 1 || section_p_.is_immediate_stop_svc_ == 1) {
                section_p_.is_immediate_stop_ = 1;
            }
        }

        /// <summary>
        /// 復帰扱いの判定を行う関数
        /// </summary>
        private void Reset() {
            if (train_spd_ == 0.0 && this.train_.Handles.BrakeNotch >= this.train_.Specs.BrakeNotches) {
                if (this.train_.Handles.BrakeNotch >= this.train_.Specs.BrakeNotches + 1 && section_p_.is_immediate_stop_eb_ == 1) {
                    section_p_.is_immediate_stop_eb_ = 0;
                    section_p_.is__brake_reset_ = 1;
                }
                if (section_p_.is_immediate_stop_svc_ == 1) {
                    section_p_.is_immediate_stop_svc_ = 0;
                    section_p_.is__brake_reset_ = 1;
                }
                if (section_p_.is_stop_svc_ == 1) {
                    section_p_.is_stop_svc_ = 0;
                    section_p_.is__brake_reset_ = 1;
                }
            }
        }

        /// <summary>
        /// 復帰ボタンが押下された際に実行する関数
        /// </summary>
        private void ResetSwDown() {
            Reset();
        }

        /// <summary>
        /// ブレーキノッチを出力する関数
        /// </summary>
        private void BrakeExe() {
            if (section_p_.is__brake_reset_ == 1) {
                atsp_brake_notch_ = 0;
            } else if (section_p_.is_immediate_stop_eb_ == 1) {
                atsp_brake_notch_ = max_brake_notch_ + 1;
            } else if (section_p_.is_immediate_stop_svc_ == 1 || section_p_.is_stop_svc_ == 1 || Math.Abs(train_spd_) > pattern_pre_spd_list_.Min()) {
                atsp_brake_notch_ = default_notch_;
            } else {
                atsp_brake_notch_ = 0;
            }
        }

        /// <summary>
        /// ランプの点灯, 消灯を行う関数
        /// </summary>
        private void DisplayLamp() {
            if (pattern_aprch_list_.Max() == 1 || atsp_brake_notch_ != 0) {
                atsp_aprch_lamp_ = 1;
            } else {
                atsp_aprch_lamp_ = 0;
            }
            if (atsp_brake_notch_ != 0) {
                atsp_brake_lamp_ = 1;
            } else {
                atsp_brake_lamp_ = 0;
            }
            if (section_p_.is__brake_reset_ == 1) {
                atsp_reset_lamp_ = 1;
            } else {
                atsp_reset_lamp_ = 0;
            }

            // ATS-Pベル
            if (atsp_aprch_lamp_ != prev_aprch_lamp_) {
                this.train_.Sounds.AtspDing.Play();
                prev_aprch_lamp_ = atsp_aprch_lamp_;
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
            atsp_power_ = 1;
            atsp_use_ = LoadConfig.AtspUse;
            atsp_max_spd_ = LoadConfig.AtspMax;
            atsp_deceleration_ = LoadConfig.AtspDeceleration / 1000.0;

            max_brake_notch_ = this.train_.Specs.BrakeNotches - this.train_.Specs.AtsNotch + 1;
            default_notch_ = (int)Math.Round((atsp_deceleration_ / max_deceleration_) * max_brake_notch_);
            pattern_list_ = new double[atsp_max_spd_ + 1];
            SetPatternList();
            adj_deceleration_ = 0.0;
            pattern_end_loc_list_ = new double[ALL_PATTERN_P];
            pattern_is_valid_list_ = new int[ALL_PATTERN_P];
            pattern_tget_spd_list_ = new int[ALL_PATTERN_P];
            pattern_pre_spd_list_ = new int[ALL_PATTERN_P];
            pattern_aprch_list_ = new int[ALL_PATTERN_P];
            debug_timer_ = 0;
            atsp_brake_notch_ = 0;
            atsp_aprch_lamp_ = 0;
            prev_aprch_lamp_ = 0;
            atsp_brake_lamp_ = 0;
            atsp_reset_lamp_ = 0;
            section_p_.Init();
            pattern_p_.Init();
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

            PassedBeacon();

            if (atsp_use_ == 0) {
                atsp_brake_notch_ = 0;
            } else {
                section_p_.CalcSection();
                CollectPattern();
                CalcPattern();
                AtspCheck();
                BrakeExe();
                DisplayLamp();

                // For debug
                if (time_ >= debug_timer_) {
                    int pre_spd = pattern_pre_spd_list_.Min();
                    int index = Array.IndexOf(pattern_pre_spd_list_, pre_spd);
                    int tget_spd = pattern_tget_spd_list_[index];
                    double pattern_end_loc = pattern_end_loc_list_[index];
                    int pattern_pre_spd = pattern_pre_spd_list_[index];
                    Trace.WriteLine("Loc: " + (float)this.train_.State.Location + " / tget_spd: " + tget_spd + " / PattEnd: " + (float)pattern_end_loc + " / pattern_pre_spd:" + pattern_pre_spd);
                    Trace.WriteLine("出力B: B" + atsp_brake_notch_ + " / AprchLamp: " + atsp_aprch_lamp_ + " / BrakeLamp: " + atsp_brake_lamp_ + " / ResetLamp: " + atsp_reset_lamp_);
                    debug_timer_ = time_ + 1000.0;
                }
            }

            if (atsp_brake_notch_ > raw_brake_notch) {
                data.Handles.BrakeNotch = atsp_brake_notch_ + this.train_.Specs.AtsNotch - 1;
                data.Handles.PowerNotch = 0;
                data.Handles.ConstSpeed = false;
                blocking = true;
            }

            // --- パネル ---
            this.train_.Panel[2] = atsp_power_;  // ATS-P電源
            this.train_.Panel[3] = atsp_aprch_lamp_;  // ATS-Pパターン接近
            this.train_.Panel[4] = atsp_reset_lamp_;  // ATS-Pブレーキ開放
            this.train_.Panel[5] = atsp_brake_lamp_;  // ATS-Pブレーキ動作
            this.train_.Panel[6] = atsp_use_;  // ATS-P
            this.train_.Panel[7] = 0;  // 故障
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
            case VirtualKeys.B1:  // ATS-P復帰
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