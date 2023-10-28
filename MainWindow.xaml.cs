﻿using MahApps.Metro.Controls;
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
        private FileList fileList = new FileList();
        /// <value>ビデオ出力の拡張子の選択されたインデックス</value>
        private int videoOutputSelectedIndex = 0;
        /// <value>オーディオ出力の拡張子の選択されたインデックス</value>
        private int audioOutputSelectedIndex = 0;
        /// <value>FFmpegを実行中かどうか</value>
        private bool executing = false;

        public MainWindow()
        {
            InitializeComponent();

            FromListBox.ItemsSource = fileList.FileNameList;
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
                    ExtensionComboBox.ItemsSource = fileList.AudioExtensions;
                    ExtensionComboBox.SelectedIndex = audioOutputSelectedIndex;
                }
                else // (type == AV.Video)
                {
                    ExtensionComboBox.ItemsSource = fileList.VideoExtensions;
                    ExtensionComboBox.SelectedIndex = videoOutputSelectedIndex;
                }
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
                foreach (var ext in fileList.VideoExtensions)
                {
                    extensions += $"*{ext},";
                }
                extensions = extensions.Substring(0, extensions.Length - 2);
                openFolderDialog.Filters.Add(new CommonFileDialogFilter("Video", extensions));
                if (AudioRadio.IsChecked == true)
                {
                    extensions = "";
                    foreach (var ext in fileList.AudioExtensions)
                    {
                        extensions += $"*{ext},";
                    }
                    extensions = extensions.Substring(0, extensions.Length - 2);
                    openFolderDialog.Filters.Add(new CommonFileDialogFilter("Audio", extensions));
                }
                openFolderDialog.Filters.Add(new CommonFileDialogFilter("All", "*.*"));

                if (openFolderDialog.ShowDialog() == CommonFileDialogResult.Ok)
                {
                    fileList.SetSourceFiles(openFolderDialog.FileNames);
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
                    fileList.TargetDir = targetDir;
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
            fileList.TargetDir = "";
        }

        /// <summary>
        /// 各ボタンを使用不可にする
        /// </summary>
        private void DisableButtons()
        {
            AddButton.IsEnabled = false;
            DeleteButton.IsEnabled = false;
            ClearButton.IsEnabled = false;
            UpButton.IsEnabled = false;
            DownButton.IsEnabled = false;
            ConvButton.IsEnabled = false;
            PlayButton.IsEnabled = false;
            InfoButton.IsEnabled = false;
        }

        /// <summary>
        /// 各ボタンを条件に応じて使用可にする
        /// </summary>
        private void EnableButtons()
        {
            var enable_sel = (FromListBox.SelectedIndex >= 0);
            var enable = (FromListBox.Items.Count > 0);

            AddButton.IsEnabled = true;
            DeleteButton.IsEnabled = enable_sel;
            ClearButton.IsEnabled = enable;
            UpButton.IsEnabled = (FromListBox.SelectedIndex > 0);
            DownButton.IsEnabled = (FromListBox.SelectedIndex < (FromListBox.Items.Count - 1)) ? enable_sel : false;
            ConvButton.IsEnabled = enable;
            PlayButton.IsEnabled = enable_sel;
            InfoButton.IsEnabled = enable_sel;
        }

        /// <summary>
        /// 実際にファイルの変換を行う
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void ConvButton_Click(object sender, RoutedEventArgs e)
        {
            DisableButtons();

            var count = fileList.FileNameList.Count;

            if (IndividualRadio.IsChecked == true)
            {
                for (int i = 0; i < count; i++)
                {
                    var file = fileList.FileNameList[i];
                    var result = fileList.ConvertFiles(i);
                    if (result != FileList.Code.OK)
                    {
                        var message = fileList.Message + "\n変換を続けますか？";
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
                var result = fileList.JoinFiles();
                if (result != FileList.Code.OK)
                {
                    var type = (result == FileList.Code.NG) ? ErrorDialog.Type.Error : ErrorDialog.Type.Warning;
                    await DialogHost.Show(new ErrorDialog(fileList.Message, type));
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
            if (executing)
            {
                fileList.CancelFFmpeg();
            }
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
                videoOutputSelectedIndex = ExtensionComboBox.SelectedIndex;
                fileList.Extension = fileList.VideoExtensions[videoOutputSelectedIndex];
            }
            else    // (AudioRadio.IsChecked == true)
            {
                audioOutputSelectedIndex = ExtensionComboBox.SelectedIndex;
                fileList.Extension = fileList.AudioExtensions[audioOutputSelectedIndex];
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
            fileList.DeleteElement(FromListBox.SelectedIndex);
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
                fileList.FileNameList.Clear();
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
            fileList.UpElement(FromListBox.SelectedIndex);
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
            fileList.DownElement(FromListBox.SelectedIndex);
            FromListBox.Items.Refresh();
            EnableButtons();
        }

        /// <summary>
        /// 渡されたコントロールの指定のタイプの子コントロールを取得
        /// </summary>
        /// <typeparam name="T">取得する子コントロールのタイプ</typeparam>
        /// <param name="dependencyObject">親コントロール</param>
        /// <returns>指定のタイプの子コントロールもしくはnull</returns>
        private T? GetDependencyObject<T>(DependencyObject dependencyObject) where T : DependencyObject
        {
            T? obj = null;
            if (dependencyObject != null)
            {
                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(dependencyObject); i++)
                {
                    var child = VisualTreeHelper.GetChild(dependencyObject, i);
                    obj = (child as T) ?? GetDependencyObject<T>(child);
                    if (obj != null)
                    {
                        break;
                    }
                }
            }
            return obj;
        }

        /// <summary>
        /// "FromListBox"から選択したファイルをFFplayで再生
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            DisableButtons();
            executing = true;
            var rc = fileList.PlayFile(FromListBox.SelectedIndex);
            executing = false;
            if (rc == FileList.Code.NG)
            {
                await DialogHost.Show(new ErrorDialog(fileList.Message, ErrorDialog.Type.Warning));
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
            executing = true;
            var info = fileList.GetFileInfo(FromListBox.SelectedIndex);
            executing = false;
            if ((info == null) || (info == ""))
            {
                await DialogHost.Show(new ErrorDialog(fileList.Message, ErrorDialog.Type.Warning));
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
        }

        /// <summary>
        /// オーディオ出力がチェックされた時、拡張子をセット
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AudioRadio_Checked(object sender, RoutedEventArgs e)
        {
            SetOutputExtensions(AV.Audio);
        }

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

        private void FromListBox_Drop(object sender, DragEventArgs e)
        {
            var dropFiles = e.Data.GetData(DataFormats.FileDrop) as string[];
            if (dropFiles == null) return;

            foreach (var file in dropFiles)
            {
                if (File.Exists(file))
                {
                    fileList.AddSourceFile(file);
                }
            }

            EnableButtons();
        }
    }
}
