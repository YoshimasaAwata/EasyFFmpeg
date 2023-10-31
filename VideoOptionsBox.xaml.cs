using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
    /// VideoOptionsBox.xaml の相互作用ロジック
    /// </summary>
    public partial class VideoOptionsBox : Window
    {
        /// <value>拡張子とビデオのコーデック、エンコーダーの辞書</value>
        private static readonly Dictionary<string, CodecLibPair> videoDic = new Dictionary<string, CodecLibPair>()
        {
            {".mp4", new CodecLibPair("h264", new []{ "libx264", "libopenh264", "h264_amf", "h264_mf", "h264_nvenc", "h264_qsv"})},
            {".asf", new CodecLibPair("msmpeg4v3", new []{"msmpeg4"})},
            {".avi", new CodecLibPair("mpeg4", new []{"mpeg4", "libxvid"})},
            {".swf", new CodecLibPair("flv1", new []{"flv"})},
            {".mkv", new CodecLibPair("h264", new []{ "libx264", "libopenh264", "h264_amf", "h264_mf", "h264_nvenc", "h264_qsv"})},
            {".mov", new CodecLibPair("h264", new []{ "libx264", "libopenh264", "h264_amf", "h264_mf", "h264_nvenc", "h264_qsv"})},
            {".ogg", new CodecLibPair("theora", new []{"libtheora"})},
            {".ts", new CodecLibPair("mpeg2video", new []{"mpeg2video", "mpeg2_qsv"})},
            {".webm", new CodecLibPair("vp9", new []{"libvpx-vp9", "vp9_qsv"})},
        };

        public VideoOptionsBox()
        {
            InitializeComponent();

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
        /// VideoOptionsBoxをクローズ
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        /// <summary>
        /// テキストボックスの入力を整数に限る
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void QualityText_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !new Regex("[0-9]").IsMatch(e.Text);
        }

        /// <summary>
        /// 貼り付けを許可しない
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void QualityText_PreviewExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            if (e.Command == ApplicationCommands.Paste)
            {
                e.Handled = true;
            }
        }
    }
}
