using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using MaterialDesignThemes.Wpf;
using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.IO;
using System.Threading;

namespace EasyFFmpeg
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        /// <summary>
        /// オーディオ、ビデオの区別
        /// </summary>
        protected enum AV
        {
            /// <value>オーディオ</value>
            Audio,
            /// <value>ビデオ</value>
            Video,
        }

        /// <value>リストボックスに表示するファイル名のリスト</value>
        private FileList _fileList = new FileList();
        /// <value>ビデオ出力の拡張子の選択されたインデックス</value>
        private int _videoOutputSelectedIndex = 0;
        /// <value>オーディオ出力の拡張子の選択されたインデックス</value>
        private int _audioOutputSelectedIndex = 0;

        public MainWindow()
        {
            InitializeComponent();

            FromListBox.ItemsSource = _fileList.FileNameList;
            SetOutputExtensions(AV.Video);
        }

        /// <summary>
        /// 出力のビデオ/オーディオの拡張子をセット
        /// </summary>
        /// <param name="type">ビデオ/オーディオの区別</param>
        private void SetOutputExtensions(AV type)
        {
            if (ExtensionComboBox != null)
            {
                if (type == AV.Audio)
                {
                    ExtensionComboBox.ItemsSource = _fileList.AudioExtensions;
                    ExtensionComboBox.SelectedIndex = _audioOutputSelectedIndex;
                }
                else // (type == AV.Video)
                {
                    ExtensionComboBox.ItemsSource = _fileList.VideoExtensions;
                    ExtensionComboBox.SelectedIndex = _videoOutputSelectedIndex;
                }
                _fileList.Extension = ExtensionComboBox.Text;
            }
        }

        /// <summary>
        /// ファイル選択ダイアログボックスを表示して変換元ファイルを選択
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            using (var openFolderDialog = new CommonOpenFileDialog()
            {
                Title = "変換元ファイルを選択してください",
                Multiselect = true,
            })
            {
                string extensions = "";
                foreach (var ext in _fileList.VideoExtensions)
                {
                    extensions += $"*{ext},";
                }
                extensions = extensions.Substring(0, extensions.Length - 2);
                openFolderDialog.Filters.Add(new CommonFileDialogFilter("Video", extensions));
                if (AudioRadio.IsChecked == true)
                {
                    extensions = "";
                    foreach (var ext in _fileList.AudioExtensions)
                    {
                        extensions += $"*{ext},";
                    }
                    extensions = extensions.Substring(0, extensions.Length - 2);
                    openFolderDialog.Filters.Add(new CommonFileDialogFilter("Audio", extensions));
                }
                openFolderDialog.Filters.Add(new CommonFileDialogFilter("All", "*.*"));

                if (openFolderDialog.ShowDialog() == CommonFileDialogResult.Ok)
                {
                    _fileList.SetSourceFiles(openFolderDialog.FileNames);
                    EnableButtons();
                }
            }
        }

        /// <summary>
        /// ファイル選択ダイアログボックスを表示して変換先フォルダを選択
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ToButton_Click(object sender, RoutedEventArgs e)
        {
            using (var openFolderDialog = new CommonOpenFileDialog()
            {
                Title = "コピー先のフォルダを選択してください",
                IsFolderPicker = true,
            })
            {
                if (openFolderDialog.ShowDialog() == CommonFileDialogResult.Ok)
                {
                    var targetDir = openFolderDialog.FileName;
                    ToTextBox.Text = targetDir;
                    _fileList.TargetDir = targetDir;
                }
            }
        }

        /// <summary>
        /// 変換先フォルダをクリア
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ClearDirButton_Click(object sender, RoutedEventArgs e)
        {
            ToTextBox.Clear();
            _fileList.TargetDir = "";
        }

        /// <summary>
        /// 条件なしでアイテムを有効/無効化する
        /// </summary>
        /// <param name="enable">有効/無効</param>
        private void EnableDefaultItems(bool enable)
        {
            AddButton.IsEnabled = enable;

            ToButton.IsEnabled = enable;
            ClearDirButton.IsEnabled = enable;
            CloseButton.IsEnabled = enable;
            ExtensionComboBox.IsEnabled = enable;
            IndividualRadio.IsEnabled = enable;
            JoinRadio.IsEnabled = enable;
            AudioRadio.IsEnabled = enable;
            VideoRadio.IsEnabled = enable;
            FromListBox.IsEnabled = enable;
        }

        /// <summary>
        /// 各ボタンを使用不可にする
        /// </summary>
        private void DisableButtons()
        {
            DeleteButton.IsEnabled = false;
            ClearButton.IsEnabled = false;
            UpButton.IsEnabled = false;
            DownButton.IsEnabled = false;
            ConvButton.IsEnabled = false;
            PlayButton.IsEnabled = false;
            InfoButton.IsEnabled = false;

            EnableDefaultItems(false);
        }

        /// <summary>
        /// 各ボタンを条件に応じて使用可にする
        /// </summary>
        private void EnableButtons()
        {
            var enable_sel = (FromListBox.SelectedIndex >= 0);
            var enable = (FromListBox.Items.Count > 0);

            DeleteButton.IsEnabled = enable_sel;
            ClearButton.IsEnabled = enable;
            UpButton.IsEnabled = (FromListBox.SelectedIndex > 0);
            DownButton.IsEnabled = (FromListBox.SelectedIndex < (FromListBox.Items.Count - 1)) ? enable_sel : false;
            ConvButton.IsEnabled = enable;
            PlayButton.IsEnabled = enable_sel;
            InfoButton.IsEnabled = enable_sel;

            EnableDefaultItems(true);
        }

        /// <summary>
        /// 実際にファイルの変換を行う
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void ConvButton_Click(object sender, RoutedEventArgs e)
        {
            DisableButtons();

            var count = _fileList.FileNameList.Count;

            if ((IndividualRadio.IsChecked == true) || (_fileList.FileNameList.Count == 1))
            {
                for (int i = 0; i < count; i++)
                {
                    var file = _fileList.FileNameList[i];
                    var result = await _fileList.ConvertFiles(i);
                    if (result != FileList.Code.OK)
                    {
                        var message = _fileList.Message + "\n変換を続けますか？";
                        var type = (result == FileList.Code.NG) ? ErrorDialog.Type.Error : ErrorDialog.Type.Warning;
                        var dialog = new ErrorDialog(message, type, ErrorDialog.ButtonType.YesNo);
                        var yesno = await DialogHost.Show(dialog) as bool?;
                        if (yesno != true)
                        {
                            break;
                        }
                    }
                }
            }
            else // (JoinRadio.IsChecked == true)
            {
                var result = await _fileList.JoinFiles();
                if (result != FileList.Code.OK)
                {
                    var type = (result == FileList.Code.NG) ? ErrorDialog.Type.Error : ErrorDialog.Type.Warning;
                    await DialogHost.Show(new ErrorDialog(_fileList.Message, type));
                }
            }

            await DialogHost.Show(new ErrorDialog("変換が終了しました。", ErrorDialog.Type.Info));

            EnableButtons();
        }

        /// <summary>
        /// アプリケーションを終了
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        /// <summary>
        /// AboutBoxを表示
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AboutButton_Click(object sender, RoutedEventArgs e)
        {
            var about = new AboutBox();
            about.ShowDialog();
        }

        /// <summary>
        /// 変換先ファイルの拡張子をセット
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ExtensionComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (VideoRadio.IsChecked == true)
            {
                _videoOutputSelectedIndex = ExtensionComboBox.SelectedIndex;
                _fileList.Extension = _fileList.VideoExtensions[ExtensionComboBox.SelectedIndex];
            }
            else    // (AudioRadio.IsChecked == true)
            {
                _audioOutputSelectedIndex = ExtensionComboBox.SelectedIndex;
                _fileList.Extension = _fileList.AudioExtensions[ExtensionComboBox.SelectedIndex];
            }
        }

        /// <summary>
        /// "FromListBox"を選択した際にUpボタンとDownボタン、Deleteボタン、Playボタン、Infoボタンの有効化/無効化を行う
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FromListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            EnableButtons();
        }

        /// <summary>
        /// "FromListBox"から選択したリストアイテムを削除<br/>
        /// 削除後に変換先ファイル名を付けなおし、リスト表示をアップデート
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            _fileList.DeleteElement(FromListBox.SelectedIndex);
            FromListBox.Items.Refresh();
            EnableButtons();
        }

        /// <summary>
        /// "FromListBox"から全リストアイテムを削除
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            DisableButtons();
            var dialog = new ErrorDialog("本当に全てのファイルをクリアしますか？", ErrorDialog.Type.Warning, ErrorDialog.ButtonType.OkCancel);
            bool? result = await DialogHost.Show(dialog) as bool?;
            if (result == true)
            {
                _fileList.FileNameList.Clear();
            }
            EnableButtons();
        }

        /// <summary>
        /// "FromListBox"から選択したリストアイテムを上に移動<br/>
        /// 移動後にコピー先ファイル名を付けなおし、リスト表示をアップデート
        /// </summary>
        /// <remarks>
        /// 移動後のリストアイテムの位置によりボタンの有効化/無効化を指定
        /// </remarks>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UpButton_Click(object sender, RoutedEventArgs e)
        {
            _fileList.UpElement(FromListBox.SelectedIndex);
            FromListBox.Items.Refresh();
            EnableButtons();
        }

        /// <summary>
        /// "FromListBox"から選択したリストアイテムを下に移動<br/>
        /// 移動後にコピー先ファイル名を付けなおし、リスト表示をアップデート
        /// </summary>
        /// <remarks>
        /// 移動後のリストアイテムの位置によりボタンの有効化/無効化を指定
        /// </remarks>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DownButton_Click(object sender, RoutedEventArgs e)
        {
            _fileList.DownElement(FromListBox.SelectedIndex);
            FromListBox.Items.Refresh();
            EnableButtons();
        }

        /// <summary>
        /// "FromListBox"から選択したファイルをFFplayで再生
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            DisableButtons();
            var rc = await _fileList.PlayFile(FromListBox.SelectedIndex);
            if (rc == FileList.Code.NG)
            {
                await DialogHost.Show(new ErrorDialog(_fileList.Message, ErrorDialog.Type.Warning));
            }
            EnableButtons();
        }

        /// <summary>
        /// "FromListBox"から選択したファイルの情報をFFprobeで取得
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void InfoButton_Click(object sender, RoutedEventArgs e)
        {
            DisableButtons();
            var info = _fileList.GetFileInfo(FromListBox.SelectedIndex);
            if ((info == null) || (info == ""))
            {
                await DialogHost.Show(new ErrorDialog(_fileList.Message, ErrorDialog.Type.Warning));
            }
            else
            {
                var infoBox = new InfoBox(info);
                infoBox.ShowDialog();
            }
            EnableButtons();
        }

        /// <summary>
        /// ビデオ出力がチェックされた時、拡張子をセット
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void VideoRadio_Checked(object sender, RoutedEventArgs e)
        {
            SetOutputExtensions(AV.Video);
            if (VideoOptionsButton != null)
            {
                VideoOptionsButton.IsEnabled = true;
            }
        }

        /// <summary>
        /// オーディオ出力がチェックされた時、拡張子をセット
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AudioRadio_Checked(object sender, RoutedEventArgs e)
        {
            SetOutputExtensions(AV.Audio);
            if (VideoOptionsButton != null)
            {
                VideoOptionsButton.IsEnabled = false;
            }
        }

        /// <summary>
        /// リストボックス上にドラッグした時にカーソルを変更
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FromListBox_DragOver(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effects = DragDropEffects.All;
            }
            else
            {
                e.Effects = DragDropEffects.None;
            }
            e.Handled = true;
        }

        /// <summary>
        /// リストボックスにドロップをした時にファイルならリストに追加
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FromListBox_Drop(object sender, DragEventArgs e)
        {
            var dropFiles = e.Data.GetData(DataFormats.FileDrop) as string[];
            if (dropFiles == null) return;

            foreach (var file in dropFiles)
            {
                if (File.Exists(file))
                {
                    _fileList.AddSourceFile(file);
                }
            }

            EnableButtons();
        }

        /// <summary>
        /// ビデオオプションのダイアログを表示
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void VideoOptionsButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new VideoOptionsBox(_fileList.VideoOptions);
            dialog.ShowDialog();
        }

        /// <summary>
        /// オーディオオプションのダイアログを表示
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AudioOptionsButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new AudioOptionsBox(_fileList.AudioOptions);
            dialog.ShowDialog();
        }
    }
}
