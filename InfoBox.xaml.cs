using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace EasyFFmpeg
{
    /// <summary>
    /// AboutBox.xaml の相互作用ロジック
    /// </summary>
    public partial class InfoBox : Window
    {
        /// <param name="info">表示する情報</param>
        /// <param name="title">タイトル</param>
        public InfoBox(string info, string? title = null)
        {
            InitializeComponent();

            if (title != null)
            {
                TitleLabel.Content = title;
            }
            InfoTextBlock.Text = info;
            if (info == "")
            {
                CopyButton.IsEnabled = false;
            }
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
        }

        /// <summary>
        /// ウィンドウをドラッグして移動
        /// </summary>
        /// <remarks>
        /// マウスボタンがリリースされた後にコールされる場合があるのでマウスボタンが押されている事をチェック
        /// </remarks>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }

        /// <summary>
        /// AboutBoxをクローズ
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void CopyButton_Click(object sender, RoutedEventArgs e)
        {
            var text = InfoTextBlock.Text;
            Clipboard.SetText(text);
        }
    }
}
