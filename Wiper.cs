// Copyright 2014 Christopher Lees
// Copyright 2004 Oskari Saarekas
// Copyright 2018 S520
//
// UKTrainSYS public domain safety systems code by Anthony Bowden.
// OpenBVE public domain plugin template by Odyakufan / Michelle.
// OS_SZ_ATS derived code originally licenced under the GPL and
// relicenced with permission from Stefano Zilocchi.
// CAWS code based upon a public - domain template by Odyakufan.
//
// All rights reserved.
//
// Redistribution and use in source and binary forms, with or without
// modification, are permitted provided that the following conditions are met :
//
// *Redistributions of source code must retain the above copyright notice, this
// list of conditions and the following disclaimer.
//
// * Redistributions in binary form must reproduce the above copyright notice,
// this list of conditions and the following disclaimer in the documentation
// and / or other materials provided with the distribution.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
// AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
// DISCLAIMED.IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE
// FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL
// DAMAGES(INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR
// SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER
// CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY,
// OR TORT(INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE
// OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using OpenBveApi.Runtime;

namespace ATCFS {

    internal class Wiper : Device {

        // --- メンバ ---
        private readonly Train train_;
        private int wiper_timer_;
        private int wiper_direction_;
        private int wiper_held_timer_;
        private bool wiper_held_;
        private int wiper_speed_;
        private int wiper_current_position_;
        private int deltaT_;
        private int wiper_rate_;
        private int wiper_hold_position_;
        private int wiper_delay_;
        private int wiper_sound_behaviour_;

        // --- コンストラクタ ---
        /// <summary>
        /// 新しいインスタンスを作成する
        /// </summary>
        /// <param name="train">Trainクラスのインスタンス</param>
        internal Wiper(Train train) {
            this.train_ = train;
        }

        // --- 関数 ---
        private void WiperRequest(int request) {
            if (request == 0 && wiper_speed_ <= 1) {
                wiper_speed_++;
                this.train_.Sounds.WiperSwDownSound.Play();
            } else if (request == 1 && wiper_speed_ > 0) {
                wiper_speed_--;
                this.train_.Sounds.WiperSwDownSound.Play();
            }
        }

        private void MoveWiper() {
            wiper_timer_ += deltaT_;
            if (wiper_timer_ > (wiper_rate_ / 100)) {
                wiper_timer_ = 0;
                if (wiper_direction_ == 1) {
                    wiper_current_position_++;
                } else {
                    wiper_current_position_--;
                }
            }
        }

        private void Exe() {
            if (wiper_current_position_ > 0 && wiper_current_position_ < 100) {
                MoveWiper();
                wiper_held_timer_ = 0;
            } else if (wiper_current_position_ == 0 && wiper_current_position_ == wiper_hold_position_) {
                wiper_direction_ = 1;
                if (wiper_speed_ == 0) {
                    wiper_held_ = true;
                } else if (wiper_speed_ == 1) {
                    wiper_held_timer_ += deltaT_;
                    if (wiper_held_timer_ > wiper_delay_) {
                        wiper_held_ = false;
                        wiper_held_timer_ = 0;
                    } else {
                        wiper_held_ = true;
                    }
                } else {
                    wiper_held_ = false;
                }

                if (wiper_held_ == false) {
                    MoveWiper();
                }
            } else if (wiper_current_position_ == 0 && wiper_current_position_ != wiper_hold_position_) {
                wiper_direction_ = 1;
                MoveWiper();
            } else if (wiper_current_position_ == 100 && wiper_current_position_ != wiper_hold_position_) {
                wiper_direction_ = -1;
                MoveWiper();
            } else if (wiper_current_position_ == 100 && wiper_current_position_ == wiper_hold_position_) {
                wiper_direction_ = -1;
                if (wiper_speed_ == 0) {
                    wiper_held_ = true;
                } else if (wiper_speed_ == 1) {
                    wiper_held_timer_ += deltaT_;
                    if (wiper_held_timer_ > wiper_delay_) {
                        wiper_held_ = false;
                        wiper_held_timer_ = 0;
                    } else {
                        wiper_held_ = true;
                    }
                } else {
                    wiper_held_ = false;
                }

                if (wiper_held_ == false) {
                    MoveWiper();
                }
            } else {
                wiper_direction_ = 1;
                MoveWiper();
            }

            if (wiper_current_position_ == 1 && wiper_direction_ == 1) {
                if (wiper_hold_position_ == 0) {
                    if (wiper_sound_behaviour_ == 0) {
                        this.train_.Sounds.WiperSound.Play();
                    }
                } else {
                    if (wiper_sound_behaviour_ != 0) {
                        this.train_.Sounds.WiperSound.Play();
                    }
                }
            } else if (wiper_current_position_ == 99 && wiper_direction_ == -1) {
                if (wiper_hold_position_ == 0) {
                    if (wiper_sound_behaviour_ != 0) {
                        this.train_.Sounds.WiperSound.Play();
                    }
                } else {
                    if (wiper_sound_behaviour_ == 0) {
                        this.train_.Sounds.WiperSound.Play();
                    }
                }
            }
        }

        // --- 継承された関数 ---
        /// <summary>
        /// ゲーム開始時に呼び出される関数
        /// </summary>
        /// <param name="mode">初期化モード</param>
        internal override void Initialize(InitializationModes mode) {
            wiper_rate_ = LoadConfig.WiperRate;
            wiper_hold_position_ = LoadConfig.WiperHoldPosition;
            wiper_delay_ = LoadConfig.WiperDelay;
            wiper_sound_behaviour_ = LoadConfig.WiperSoundBehaviour;
            wiper_current_position_ = wiper_hold_position_;
        }

        /// <summary>
        /// 1フレームごとに呼び出される関数
        /// </summary>
        /// <param name="data">The data.</param>
        /// <param name="blocking">保安装置がブロックされているか、後続の保安装置をブロックするかどうか。</param>
        internal override void Elapse(ElapseData data, ref bool blocking) {
            deltaT_ = (int)data.ElapsedTime.Milliseconds;
            Exe();

            // --- パネル ---
            //this.train_.panel[192] = wiper_speed_;
            this.train_.Panel[193] = wiper_current_position_;
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
            case VirtualKeys.J:  // ワイパースピードアップ
                WiperRequest(0);
                break;

            case VirtualKeys.K:  // ワイパースピードダウン
                WiperRequest(1);
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
        }
    }
}