# ATCFS For OpenBVE

[![Build status](https://ci.appveyor.com/api/projects/status/kw52b75ccsbp6fv4?svg=true)](https://ci.appveyor.com/project/s520/atcfs-for-openbve)
[![Report](https://inspecode.rocro.com/badges/github.com/s520/ATCFS_For_OpenBVE/report?token=gBBXkwyqWeN38TId3d7yjIzYaT2rrcrUndn3zi1ehYc&branch=master)](https://inspecode.rocro.com/reports/github.com/s520/ATCFS_For_OpenBVE/branch/master/summary)
[![Codacy Badge](https://api.codacy.com/project/badge/Grade/0a57f0f8fc334949b3e30e1119504fde)](https://www.codacy.com/project/s520/ATCFS_For_OpenBVE/dashboard?utm_source=github.com&amp;utm_medium=referral&amp;utm_content=s520/ATCFS_For_OpenBVE&amp;utm_campaign=Badge_Grade_Dashboard)

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
- [DetailManager For OpenBVE](https://github.com/s520/DetailManager_For_OpenBVE)を用いて[MiscFunc For OpenBVE](https://github.com/s520/MiscFunc_For_OpenBVE)と組み合わせることにより、ワイパーや電流計などの再現が可能

## Requirement

- Visual Studio 2010 or more

## License

2-clause BSD license

- Since this project contains INI File Parser source code, that part is provided under the MIT license of INI File Parser described below.

## Postscript

本プロジェクトの英語への翻訳にご協力いただける方を募集しております。

翻訳にご協力いただける方はpull requestを送ってください。