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

namespace ATCFS {

    /// <summary>
    /// その他機能を再現するクラス
    /// </summary>
    internal class Sub : Device {

        // --- メンバ ---
        private readonly Train train_;
        private int brake_status_;  //!< ATCブレーキの状態(0: 緩解, 1: 作動)
        private int prev_brake_status_;  //!< 1フレーム前のATCブレーキの状態(0: 緩解, 1: 作動)

        // --- コンストラクタ ---
        /// <summary>
        /// 新しいインスタンスを作成する
        /// </summary>
        /// <param name="train">Trainクラスのインスタンス</param>
        internal Sub(Train train) {
            this.train_ = train;
        }

        // --- 関数 ---
        /// <summary>
        /// ATCブレーキの緩解音を再生する関数
        /// </summary>
        private void PlayAtcAirSound() {
            brake_status_ = (this.train_.Atc.atc_brake_notch_ > 0) ? 1 : 0;
            if (brake_status_ != prev_brake_status_ && this.train_.Atc.atc_brake_notch_ == 0) {
                this.train_.Sounds.AtcAirSound.Play();
            }
            prev_brake_status_ = brake_status_;
        }

        // --- 継承された関数 ---
        /// <summary>
        /// ゲーム開始時に呼び出される関数
        /// </summary>
        /// <param name="mode">初期化モード</param>
        internal override void Initialize(InitializationModes mode) {
        }

        /// <summary>
        /// 1フレームごとに呼び出される関数
        /// </summary>
        /// <param name="data">The data.</param>
        /// <param name="blocking">保安装置がブロックされているか、後続の保安装置をブロックするかどうか。</param>
        internal override void Elapse(ElapseData data, ref bool blocking) {
            PlayAtcAirSound();

            // --- パネル ---
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