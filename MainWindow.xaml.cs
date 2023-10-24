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
using System.Windows.Shapes;

namespace EasyFFmpeg
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        /// <value>リストボックスに表示するファイル名のリスト</value>
        private FileList fileList = new FileList();
        /// <value>Fromリストボックス内のScrollViewer</value>
        private ScrollViewer? fromScrollViewer;
        /// <value>Toリストボックス内のScrollViewer</value>
        private ScrollViewer? toScrollViewer;

        public MainWindow()
        {
            InitializeComponent();

            FromListBox.ItemsSource = fileList.FileNameList;
            ToListBox.ItemsSource = fileList.FileNameList;
            FileNameTextBox.Text = fileList.BaseFileName;
        }

        /// <summary>
        /// ファイル選択ダイアログボックスを表示してコピー元フォルダを選択
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void AddButton_Click(object sender, RoutedEventArgs e)
        {
            using (var openFolderDialog = new CommonOpenFileDialog()
            {
                Title = "コピー元フォルダを選択してください",
                IsFolderPicker = true,
            })
            {
                if (openFolderDialog.ShowDialog() == CommonFileDialogResult.Ok)
                {
                    var sourceDir = openFolderDialog.FileName;
                    var rc = fileList.SetSourceDir(sourceDir, ExcludeCheck.IsChecked);
                    if (rc == FileList.Code.NG)
                    {
                        await DialogHost.Show(new ErrorDialog(fileList.Message, ErrorDialog.Type.Error));
                    }
                    else
                    {
                        FromTextBox.Text = sourceDir;
                        // コピーするファイルがない場合もCopyボタンは無効
                        CopyButton.IsEnabled = ((rc == FileList.Code.OK) && (fileList.FileNameList.Count > 0));
                    }
                }
            }
        }

        /// <summary>
        /// ファイル選択ダイアログボックスを表示してコピー先フォルダを選択
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
        /// 実際にファイルの変換を行う
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void ConvButton_Click(object sender, RoutedEventArgs e)
        {
            ConvButton.IsEnabled = false;

            var progress = new FileConversionProgress();
            var copyTask = fileList.CopyFiles(progress);
            var progressTask = DialogHost.Show(progress);

            var taskDone = await Task.WhenAny(copyTask, progressTask);

            if (taskDone == progressTask)
            {
                fileList.CancelCopy();
                await copyTask;
                await DialogHost.Show(new ErrorDialog("キャンセルされました。", ErrorDialog.Type.Warning));
            }
            else
            {
                DialogHost.Close(null);
                await progressTask;

                var rc = copyTask.Result;
                if (rc == FileList.Code.NG)
                {
                    await DialogHost.Show(new ErrorDialog(fileList.Message, ErrorDialog.Type.Error));
                }
                else
                {
                    await DialogHost.Show(new ErrorDialog("コピーしました。", ErrorDialog.Type.Info));
                }
            }

            ConvButton.IsEnabled = true;
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
        /// 共通ファイル名をアップデートする
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FileNameTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            fileList.BaseFileName = FileNameTextBox.Text;
            ToListBox.Items.Refresh();
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
        /// コピー元ファイルのフィルタリング用拡張子のリストを作成</br>
        /// 拡張子は区切り文字',', ' ', '.', ';', ':'を用いて複数指定できる
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ExtensionTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            fileList.SetExtensions(ExtensionTextBox.Text);
        }

        /// <summary>
        /// "FromListBox"を選択した際にUpボタンとDownボタン、Deleteボタンの有効化/無効化を行う</br>
        /// 更に"ToListBox"の選択を"FromListBox"に反映
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FromListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var enable = (FromListBox.SelectedIndex >= 0);

            UpButton.IsEnabled = enable;
            DownButton.IsEnabled = enable;
            DeleteButton.IsEnabled = enable;

            if (FromListBox.SelectedIndex == 0)
            {
                UpButton.IsEnabled = false;
            }

            if (FromListBox.SelectedIndex == (fileList.FileNameList.Count - 1))
            {
                DownButton.IsEnabled = false;
            }

            ToListBox.SelectedIndex = FromListBox.SelectedIndex;
        }

        /// <summary>
        /// "ToListBox"の選択を"FromListBox"に反映
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ToListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            FromListBox.SelectedIndex = ToListBox.SelectedIndex;
        }

        /// <summary>
        /// "FromListBox"から選択したリストアイテムを削除</br>
        /// 削除後にコピー先ファイル名を付けなおし、リスト表示をアップデート
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            fileList.DeleteElement(FromListBox.SelectedIndex);

            FromListBox.Items.Refresh();
            ToListBox.Items.Refresh();
        }

        /// <summary>
        /// "FromListBox"から選択したリストアイテムを上に移動</br>
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
            ToListBox.Items.Refresh();

            UpButton.IsEnabled = (FromListBox.SelectedIndex > 0);
            DownButton.IsEnabled = (FromListBox.SelectedIndex < (fileList.FileNameList.Count - 1));
        }

        /// <summary>
        /// "FromListBox"から選択したリストアイテムを下に移動</br>
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
            ToListBox.Items.Refresh();

            UpButton.IsEnabled = (FromListBox.SelectedIndex > 0);
            DownButton.IsEnabled = (FromListBox.SelectedIndex < (fileList.FileNameList.Count - 1));
        }

        /// <summary>
        /// 渡されたコントロールの指定のタイプの子コントロールを取得
        /// </summary>
        /// <typeparam name="T">取得する子コントロールのタイプ</typeparam>
        /// <param name="dependencyObject">親コントロール</param>
        /// <returns>指定のタイプの子コントロールもしくはnull</returns>
        private T? GetDependencyObject<T> (DependencyObject dependencyObject) where T : DependencyObject
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
        /// "FromListBox"内のScrollViewerがスクロールした際のイベントハンドラ</br>
        /// "FromListBox"のスクロール量を"ToListBox"に反映
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FromListBox_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if ((toScrollViewer != null) && (fromScrollViewer != null))
            {
                toScrollViewer.ScrollToVerticalOffset(fromScrollViewer.VerticalOffset);
            }
        }

        /// <summary>
        /// "ToListBox"内のScrollViewerがスクロールした際のイベントハンドラ</br>
        /// "ToListBox"のスクロール量を"FromListBox"に反映
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ToListBox_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if ((fromScrollViewer != null) && (toScrollViewer != null))
            {
                fromScrollViewer.ScrollToVerticalOffset(toScrollViewer.VerticalOffset);
            }
        }

        /// <summary>
        /// "FromListBox"の描画終了時のイベントハンドラ</br>
        /// "FromListBox"内のScrollViewrを取得すると共にScrollChangedイベントハンドラを追加
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FromListBox_Loaded(object sender, RoutedEventArgs e)
        {
            fromScrollViewer = GetDependencyObject<ScrollViewer>(FromListBox);
            if (fromScrollViewer != null)
            {
                fromScrollViewer.ScrollChanged += new ScrollChangedEventHandler(FromListBox_ScrollChanged);
            }
        }

        /// <summary>
        /// "ToListBox"の描画終了時のイベントハンドラ</br>
        /// "ToListBox"内のScrollViewrを取得すると共にScrollChangedイベントハンドラを追加
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ToListBox_Loaded(object sender, RoutedEventArgs e)
        {
            toScrollViewer = GetDependencyObject<ScrollViewer>(ToListBox);
            if (toScrollViewer != null)
            {
                toScrollViewer.ScrollChanged += new ScrollChangedEventHandler(ToListBox_ScrollChanged);
            }
        }
    }
}
