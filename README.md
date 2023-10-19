# EasyFFmpeg #

指定した動画ファイルを**FFmpeg**を使用して他の動画形式に変換する簡易のフロントエンドアプリケーションです。  

## バージョン ##

1.0.0

## ライセンス ##

本ソフトウェアはMITライセンスにより提供されています。  
詳細は「LICENSE.txt」をご覧ください。  

## パッケージ ##

本ソフトウェアは以下のNuGetパッケージを使用しています。  

| パッケージ                   | ライセンス                                                                                     |
|:-----------------------------|:-----------------------------------------------------------------------------------------------|
| WindowsAPICodePack-Core      | [Custom](https://github.com/aybe/Windows-API-Code-Pack-1.1/blob/master/LICENCE)                |
| WindowsAPICodePack-Shell     | [Custom](https://github.com/aybe/Windows-API-Code-Pack-1.1/blob/master/LICENCE)                |
| MahApps.Metro                | [MIT](https://github.com/MahApps/MahApps.Metro/blob/develop/LICENSE)                           |
| MahApps.Metro.IconPacks      | [MIT](https://github.com/MahApps/MahApps.Metro.IconPacks/blob/develop/LICENSE)                 |
| ControlzEx                   | [MIT](https://github.com/ControlzEx/ControlzEx/blob/develop/LICENSE)                           |
| Microsoft.Xaml.Behaviors.Wpf | [MIT](https://github.com/microsoft/XamlBehaviorsWpf/blob/master/LICENSE)                       |
| System.Text.Json             | [MIT](https://www.nuget.org/packages/System.Text.Json/4.7.2/license)                           |
| MaterialDesignThemes         | [MIT](https://github.com/MaterialDesignInXAML/MaterialDesignInXamlToolkit/blob/master/LICENSE) |
| MaterialDesignColors         | [MIT](https://github.com/MaterialDesignInXAML/MaterialDesignInXamlToolkit/blob/master/LICENSE) |
| MaterialDesignThemes.MahApps | [MIT](https://github.com/MaterialDesignInXAML/MaterialDesignInXamlToolkit/blob/master/LICENSE) |

また、本ソフトウェアが使用する**FFmpeg**は*LGPLv2.1*以降のライセンスとなっていますが、オプションのライブラリには*GPLv2*に準拠するライセンスが適用されるものもあります。  
詳細は[**FFmpeg**のライセンスのページ](https://www.ffmpeg.org/legal.html)を確認してください。

## 使用方法 ##

### Fromボタン ###

Fromボタンをクリックするとフォルダ選択ダイアログが表示されます。

コピーするファイルが含まれるフォルダを選択してください。

選択したコピー元フォルダが表示されると共にリストにコピーするファイルが表示されます。  
同時にコピー後のファイル名がリストに表示されます。

### Toボタン ###

Toボタンをクリックするとフォルダ選択ダイアログが表示されます。

ファイルをコピーする先のフォルダを選択してください。

選択したコピー先フォルダが表示されます。

コピー先フォルダを選択しない場合には、ファイルはコピー元フォルダにコピーされます。

### 共通ファイル名 ###

ファイルをコピーする際のファイル名は「共通ファイル名+連番.拡張子」となります。

共通ファイル名に文字列を入力する事でファイル名を変更する事ができます。

### Copyボタン ###

Copyボタンをクリックすると実際にファイルがコピーされます。

コピーするファイルのサイズ等が大きい場合には時間が掛かる場合があります。

ファイルコピー中はプログレスバーが表示されますので、ファイルコピーを中断したい場合にはCancelボタンをクリックして下さい。

### Closeボタン ###

アプリケーションを終了します。

### 拡張子 ###

拡張子欄に拡張子を記入する事でコピー対象のファイルをフィルタリングする事ができます。

拡張子は区切り文字カンマ',', スペース' ', ピリオド'.', セミコロン';', コロン':'を使用する事で複数指定する事ができます。

拡張子の指定がない場合には全ての拡張子のファイルがコピーの対象となります。

Fromボタンでコピー元フォルダを選択する前に指定を完了している必要があります。

### 隠しファイル / システムファイル除外 ###

チェックする事で隠しファイルやシステムファイルをコピー対象のファイルから除外します。

Fromボタンでコピー元フォルダを選択する前に指定を完了している必要があります。

### Deleteボタン ###

リスト上のファイル名を選択後Deleteボタンをクリックする事で、選択したファイル名をリストから除外する事ができます。

一旦除外したファイル名は元に戻す事はできません。  
元に戻すには再度Fromボタンでコピー元フォルダを選択して下さい。

### Upボタン / Downボタン ###

リスト上のファイル名を選択後、UpボタンとDownボタンでコピーするファイルのリスト上の順番を変える事ができます。

リスト上の順番を変える事で、コピー先ファイル名の連番の番号を変更する事ができます。
