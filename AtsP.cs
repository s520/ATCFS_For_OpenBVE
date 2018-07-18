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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenBveApi.Runtime;

namespace ATCFS
{
    internal class AtsP : Device
    {
        // --- メンバ ---


        // --- コンストラクタ ---

        /// <summary>新しいインスタンスを作成します。</summary>
        internal AtsP()
        {
        }


        // --- 関数 ---


        // --- 継承された関数 ---

        /// <summary>保安装置を初期化するときに呼び出されます。</summary>
        /// <param name="mode">初期化モード</param>
        internal override void Initialize(InitializationModes mode)
        {
        }

        /// <summary>1フレームごとに呼び出されます。</summary>
        /// <param name="data">The data.</param>
        /// <param name="blocking">保安装置がブロックされているか、後続の保安装置をブロックするかどうか。</param>
        internal override void Elapse(ElapseData data, ref bool blocking)
        {
        }

        /// <summary>レバーサが扱われたときに呼び出されます。</summary>
        /// <param name="reverser">新しいレバーサ位置</param>
        internal override void SetReverser(int reverser)
        {
        }

        /// <summary>運転者が力行ノッチを扱った際に呼び出されます。</summary>
        /// <param name="powerNotch">新しい力行ノッチ</param>
        internal override void SetPower(int powerNotch)
        {
        }

        /// <summary>運転者がブレーキノッチを扱った際に呼び出されます。</summary>
        /// <param name="brakeNotch">新しいブレーキノッチ</param>
        internal override void SetBrake(int brakeNotch)
        {
        }

        /// <summary>ATSキーが押されたときに呼び出されます。</summary>
        /// <param name="key">押されたATSキー</param>
        internal override void KeyDown(VirtualKeys key)
        {
        }

        /// <summary>ATSキーが離されたときに呼び出されます。</summary>
        /// <param name="key">離されたATSキー</param>
        internal override void KeyUp(VirtualKeys key)
        {
        }

        /// <summary>警笛が扱われたときに呼び出されます。</summary>
        /// <param name="type">警笛のタイプ</param>
        internal override void HornBlow(HornTypes type)
        {
        }

        /// <summary>現在の閉そくの信号が変化したときに呼び出されます。</summary>
        /// <param name="signal">信号番号</param>
        internal override void SetSignal(SignalData[] signal)
        {
        }

        /// <summary>地上子を越えたときに呼び出されます。</summary>
        /// <param name="beacon">車上子で受け取った情報</param>
        internal override void SetBeacon(BeaconData beacon)
        {
        }
    }
}
