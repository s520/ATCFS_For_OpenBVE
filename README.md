# ATCFS For OpenBVE

[![Build status](https://ci.appveyor.com/api/projects/status/kw52b75ccsbp6fv4?svg=true)](https://ci.appveyor.com/project/s520/atcfs-for-openbve)

ATC-1, ATC-2, ATC-NS, KS-ATC, DS-ATC, ATS-P Plugin for OpenBVE on Linux or Windows

このプラグインは.NETアセンブリプラグインです。Win32版は[こちら](https://github.com/s520/ATCFS)。

## Description

新幹線の保安装置を再現するBVE用ATCプラグインです。

詳細な仕様は[ATCFS Specifications.pdf](https://github.com/s520/ATCFS_For_OpenBVE/blob/master/doc/ATCFS%20Specifications.pdf)をご覧ください。

ドキュメントは[こちら](https://s520.github.io/ATCFS_For_OpenBVE/)。

## Features

- 各種ATCの挙動を再現
- ATC-NS, KS-ATC, DS-ATCでは、予見Fuzzy制御により1段ブレーキを再現
- 各種ATCのモニタ表示を再現
- ワイパーの動作を再現

## Requirement

- Visual Studio 2010 or more

## License

2-clause BSD license

- Since this project contains part BVEC_ATS source code, that part is provided under the 2-clause BSD license of BVEC_ATS described below.
- Since this project contains INI File Parser source code, that part is provided under the MIT license of INI File Parser described below.

## Postscript

本プロジェクトの英語への翻訳にご協力いただける方を募集しております。

翻訳にご協力いただける方はpull requestを送ってください。