using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
        /// <value>拡張子とビデオのコーデックの辞書</value>
        private static readonly Dictionary<string, string> s_codecDic = new Dictionary<string, string>()
        {
            {".mp4", "h264"},
            {".asf", "msmpeg4v3"},
            {".avi", "mpeg4"},
            {".swf", "flv1"},
            {".mkv", "h264"},
            {".mov", "h264"},
            {".ogg", "theora"},
            {".ts", "mpeg2video"},
            {".webm", "vp9"},
        };

        /// <value>ビデオのコーデック、エンコーダーの辞書</value>
        private static readonly Dictionary<string, string[]> s_encoderDic = new Dictionary<string, string[]>()
        {
            {"h264", new []{ "libopenh264", "h264_amf", "h264_mf", "h264_nvenc", "h264_qsv", "libx264", }},
            {"msmpeg4v3", new []{"msmpeg4", }},
            {"mpeg4", new []{"mpeg4", "libxvid", }},
            {"flv1", new []{"flv", }},
            {"theora", new []{"libtheora", }},
            {"mpeg2video", new []{"mpeg2video", "mpeg2_qsv", }},
            {"vp9", new []{"libvpx-vp9", "vp9_qsv", }},
        };

        /// <value>スクリーンサイズ</value>
        private static readonly string[] s_sizeList = 
        {
            "320x240", "640x480", "720x480", "800x600", "1024x768", "1280x720", "1280x960", "1440x1080", "1600x1200", "1920x1080", "2560x1440", "3840x2160", "7680x4320",
        };

        /// <value>アスペクト比</value>
        private static readonly string[] s_aspectList = {"4:3", "16:9",};

        /// <param name="extension">出力ファイルの拡張子</param>
        public VideoOptionsBox(VideoOptions options)
        {
            InitializeComponent();

            var codec = s_codecDic[options.OutputExtension];
            var encoderList = s_encoderDic[codec];

            HWDecoderCheck.IsChecked = options.UseHWAccel;
            CopyCheck.IsChecked = options.CopyVideo;
            EncoderStack.IsEnabled = (encoderList.Length > 1);
            EncoderCheck.IsChecked = options.SpecifyEncoder;
            EncoderLabel.IsEnabled = options.SpecifyEncoder;
            EncoderCombo.IsEnabled = options.SpecifyEncoder;
            EncoderCombo.ItemsSource = encoderList;
            EncoderCombo.SelectedIndex = (options.Encoder == "") ? 0 : Array.IndexOf(encoderList, options.Encoder);
            SizeCheck.IsEnabled = options.SpecifySize;
            SizeLabel.IsEnabled = options.SpecifySize;
            SizeCombo.IsEnabled = options.SpecifySize;
            SizeCombo.ItemsSource = s_sizeList;
            SizeCombo.SelectedIndex = (options.Size == "") ? 0 : Array.IndexOf(s_sizeList, options.Size);
            AspectCheck.IsEnabled = options.SpecifyAspect;
            AspectLabel.IsEnabled = options.SpecifyAspect;
            AspectCombo.IsEnabled = options.SpecifyAspect;
            AspectCombo.ItemsSource= s_aspectList;
            AspectCombo.SelectedIndex = (options.Aspect == "") ? 0 : Array.IndexOf(s_aspectList, options.Aspect);
            Bitrate1stCheck.IsEnabled = options.SetBitrate;
            AveBitrateDock.IsEnabled = options.SetBitrate;
            MaxBitrateDock.IsEnabled = options.SetBitrate;
            AveBitrateText.Text = options.AveBitrate.ToString();
            MaxBitrateText.Text = options.MaxBitrate.ToString();
            AveBitrateSlider.Value = options.AveBitrate;
            MaxBitrateSlider.Value = options.MaxBitrate;

            WindowStartupLocation = WindowStartupLocation.CenterScreen;
        }

        /// <summary>
        /// 拡張子の変更時にエンコーダーリストのアップデートと有効/無効をセット
        /// </summary>
        /// <param name="extension">出力ファイルの拡張子</param>
        public void SetEncoders(string extension)
        {
            var codec = s_codecDic[extension];
            var encoderList = s_encoderDic[codec];
            EncoderCombo.ItemsSource = encoderList;
            EncoderCombo.SelectedIndex = 0;
            EncoderStack.IsEnabled = (encoderList.Length > 1);
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

        /// <summary>
        /// エンコーダーの選択肢を有効にする
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void EncoderCheck_Checked(object sender, RoutedEventArgs e)
        {
            EncoderLabel.IsEnabled = true;
            EncoderCombo.IsEnabled = true;
        }

        /// <summary>
        /// エンコーダーの選択肢を無効にする
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void EncoderCheck_Unchecked(object sender, RoutedEventArgs e)
        {
            EncoderLabel.IsEnabled = false;
            EncoderCombo.IsEnabled = false;
        }
    }
}
