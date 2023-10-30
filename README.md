# EasyFFmpeg #

指定した動画ファイルを**FFmpeg**を使用して他の動画形式に変換する簡易のフロントエンドアプリケーションです。  

## バージョン ##

1.2.0

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

### Addボタン ###

Addボタンをクリックするとファイル選択ダイアログが表示されます。

変換するファイルを選択してください。

リストにコピーするファイルが表示されます。

なお追加できるファイルの種類については、**FFmpeg**の入力に使用できるファイルの種類からは絞っています。

- Video: ".mp4", ".asf", ".avi", ".swf", ".flv", ".mkv", ".mov", ".ogv", ".ogg", ".ogx", ".ts", ".webm"
- Audio: ".aac", ".ac3", ".adpcm", ".amr", ".alac", ".fla", ".flac", ".mp1", ".mp2", ".mp3", ".m4a", ".als", ".pcm", ".qcp", ".ra", ".oga", ".wma"

また、ファイルをドラッグアンドドロップする事でリストにファイルを追加する事もできます。

### Deleteボタン ###

リスト上のファイル名を選択後にDeleteボタンをクリックする事で、選択したファイル名をリストから除外する事ができます。

一旦除外したファイル名は元に戻す事はできません。  
元に戻すには再度Addボタンで変換元ファイルを選択して下さい。

### Clearボタン ###

リスト上のファイル名を全てリストから除外します。

一旦除外したファイル名は元に戻す事はできません。  
元に戻すには再度Addボタンで変換元ファイルを選択して下さい。

### Upボタン / Downボタン ###

リスト上のファイル名を選択後、UpボタンとDownボタンで変換するファイルのリスト上の順番を変える事ができます。

リスト上の順番を変える事で、ファイルを結合する場合の順番を変更する事ができます。  
個別に変換する場合には変換の順番が変わるだけで、あまり影響はありません。

### Convボタン ###

Convボタンをクリックするとリストに表示された全てのファイルの変換が開始されます。

変換するファイルのサイズ等が大きい場合には時間が掛かる場合があります。

ファイルの変換中はコンソール上に**FFmpeg**からの情報表示されます。  
ファイルの変換を中断したい場合にはコンソール右上の"閉じる"ボタンをクリックする等して、コンソールを閉じて下さい。

### Playボタン ###

リスト上のファイル名を選択後にPlayボタンをクリックすると、選択したファイルが再生されます。

再生には"ffplay"コマンドを使用しています。  
スキップや停止等の再生時の操作については"ffplay"と同じとなります。

再生を途中で終了したい場合には<kbd>q</kbd>や<kbd>ESC</kbd>キーを押すか、再生ウィンドウの右上の"閉じる"ボタンをクリックする等して再生ウィンドウを閉じてください。

### Infoボタン ###

リスト上のファイル名を選択後にInfoボタンをクリックすると、選択したファイルの情報が表示されます。

### Individual/Joinラジオボタン

- Individual: リストに表示されたファイルを個別に変換します。
- Join: リストに表示されたファイルを全て結合して変換します。

### Toボタン ###

Toボタンをクリックするとフォルダ選択ダイアログが表示されます。

変換後のファイルを保存する先のフォルダを選択してください。

フォルダを選択しない場合には変換後のファイルは変換元のファイルと同じフォルダに保存されます。

### ClearDirボタン ###

変換後のファイルを保存する先のフォルダをクリアします。

### Video/Audioラジオボタン ###

変換先のファイルタイプを選択します。

拡張子に表示されるファイルタイプが変わります。

### 拡張子 ###

変換先のファイルタイプを拡張子で選択します。

選択肢はVideo/Audioラジオボタンによって変わります。

### Exitボタン ###

アプリケーションを終了します。

## 変更履歴 ##

### 1.2.0 ###

- ファイル情報については**FFprobe**の出力から必要な情報のみを取り出して表示するようにした。
- オーディオのビットレートについては、変換元のオーディオのビットレートをそのまま使用するようにした。

### 1.1.0 ###

ドラッグアンドドロップでファイルを追加できるように機能を追加。

### 1.0.0 ###

とりあえずファイルの変換をできるようにした。
