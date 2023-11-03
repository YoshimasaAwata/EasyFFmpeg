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
        /// <value>ビデオのコーデックとエンコーダーの辞書</value>
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

        /// <value>アスペクト比</value>
        private static readonly string[] s_framerateList = {"24", "30000/1001", "30", "50", "60000/1001", "60", "120", "240"};

        /// <value>スクリーンサイズ</value>
        private static readonly string[] s_sizeList = 
        {
            "320x240", "640x480", "720x480", "800x600", "1024x768", "1280x720", "1280x960", "1440x1080", "1600x1200", "1920x1080", "2560x1440", "3840x2160", "7680x4320",
        };

        /// <value>アスペクト比</value>
        private static readonly string[] s_aspectList = {"4:3", "16:9",};

        /// <value>最大ビットレート(Mbps)</value>
        private const int MAX_BITRATE = 300;
        /// <value>最小ビットレート(Mbps)</value>
        private const int MIN_BITRATE = 1;

        /// <value>ビデオのオプション</value>
        private VideoOptions _options;

        /// <param name="options">ビデオのオプション</param>
        public VideoOptionsBox(VideoOptions options)
        {
            _options = options;

            InitializeComponent();

            var encoderList = s_encoderDic[_options.Codec];

            HWDecoderCheck.IsChecked = options.UseHWAccel;
            CopyCheck.IsChecked = options.CopyVideo;
            EncoderStack.IsEnabled = (encoderList.Length > 1);
            EncoderCheck.IsChecked = options.SpecifyEncoder;
            EncoderLabel.IsEnabled = options.SpecifyEncoder;
            EncoderCombo.IsEnabled = options.SpecifyEncoder;
            EncoderCombo.ItemsSource = encoderList;
            // エンコーダーはリストの最初がお勧め
            EncoderCombo.SelectedIndex = (options.Encoder == "") ? 0 : Array.IndexOf(encoderList, options.Encoder);
            FramerateCheck.IsChecked = options.SpecifyFramerate;
            FramerateLabel.IsEnabled = options.SpecifyFramerate;
            FramerateCombo.IsEnabled = options.SpecifyFramerate;
            FramerateCombo.ItemsSource = s_framerateList;
            // フレームレートは29.97fpsが一般的か?
            FramerateCombo.SelectedIndex = (options.Framerate == "") ? 1 : Array.IndexOf(s_framerateList, options.Framerate);
            FPSLabel.IsEnabled = options.SpecifyFramerate;
            SizeCheck.IsChecked = options.SpecifySize;
            SizeLabel.IsEnabled = options.SpecifySize;
            SizeCombo.IsEnabled = options.SpecifySize;
            SizeCombo.ItemsSource = s_sizeList;
            // サイズはとりあえずDVDに合わせる
            SizeCombo.SelectedIndex = (options.Size == "") ? 2 : Array.IndexOf(s_sizeList, options.Size);
            AspectCheck.IsChecked = options.SpecifyAspect;
            AspectLabel.IsEnabled = options.SpecifyAspect;
            AspectCombo.IsEnabled = options.SpecifyAspect;
            AspectCombo.ItemsSource= s_aspectList;
            // アスペクト比は16:9を標準とする
            AspectCombo.SelectedIndex = (options.Aspect == "") ? 1 : Array.IndexOf(s_aspectList, options.Aspect);
            Bitrate1stCheck.IsChecked = options.SetBitrate;
            AveBitrateDock.IsEnabled = options.SetBitrate;
            MaxBitrateDock.IsEnabled = options.SetBitrate;
            AveBitrateText.Text = options.AveBitrate.ToString();
            MaxBitrateText.Text = options.MaxBitrate.ToString();
            AveBitrateSlider.Value = options.AveBitrate;
            MaxBitrateSlider.Value = options.MaxBitrate;
            AveBitrateSlider.Maximum = MAX_BITRATE;
            AveBitrateSlider.Minimum = MIN_BITRATE;
            MaxBitrateSlider.Maximum = MAX_BITRATE;
            MaxBitrateSlider.Minimum = MIN_BITRATE;
            StatusLabel.Content = "";

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
        /// ハードウェアデコーダーを有効にする
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void HWDecoderCheck_Checked(object sender, RoutedEventArgs e)
        {
            _options.UseHWAccel = true;
        }

        /// <summary>
        /// ハードウェアデコーダーを無効にする
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void HWDecoderCheck_Unchecked(object sender, RoutedEventArgs e)
        {
            _options.UseHWAccel = false;
        }

        /// <summary>
        /// ビデオのコピーを有効にする
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CopyCheck_Checked(object sender, RoutedEventArgs e)
        {
            _options.CopyVideo = true;
        }

        /// <summary>
        /// ビデオのコピーを無効にする
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CopyCheck_Unchecked(object sender, RoutedEventArgs e)
        {
            _options.CopyVideo = false;
        }

        /// <summary>
        /// エンコーダーの選択肢を有効にする
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void EncoderCheck_Checked(object sender, RoutedEventArgs e)
        {
            _options.SpecifyEncoder = true;
            EncoderLabel.IsEnabled = true;
            EncoderCombo.IsEnabled = true;
            _options.Encoder = EncoderCombo.Text;
        }

        /// <summary>
        /// エンコーダーの選択肢を無効にする
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void EncoderCheck_Unchecked(object sender, RoutedEventArgs e)
        {
            _options.SpecifyEncoder = false;
            EncoderLabel.IsEnabled = false;
            EncoderCombo.IsEnabled = false;
            _options.Encoder = "";
        }

        /// <summary>
        /// エンコーダーの選択肢を保存する
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void EncoderCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _options.Encoder = EncoderCombo.Text;
        }

        /// <summary>
        /// フレームレートの選択肢を有効にする
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FramerateCheck_Checked(object sender, RoutedEventArgs e)
        {
            _options.SpecifyFramerate = true;
            FramerateLabel.IsEnabled = true;
            FramerateCombo.IsEnabled = true;
            FPSLabel.IsEnabled = true;
            _options.Framerate = FramerateCombo.Text;
        }

        private void FramerateCheck_Unchecked(object sender, RoutedEventArgs e)
        {
            _options.SpecifyFramerate = false;
            FramerateLabel.IsEnabled = false;
            FramerateCombo.IsEnabled = false;
            FPSLabel.IsEnabled= false;
            _options.Framerate = "";
        }

        private void FramerateCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _options.Framerate = FramerateCombo.Text;
        }

        /// <summary>
        /// 画像サイズの指定を有効にする
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SizeCheck_Checked(object sender, RoutedEventArgs e)
        {
            _options.SpecifySize = true;
            SizeLabel.IsEnabled = true;
            SizeCombo.IsEnabled = true;
            _options.Size = SizeCombo.Text;
        }

        /// <summary>
        /// 画像サイズの指定を無効にする
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SizeCheck_Unchecked(object sender, RoutedEventArgs e)
        {
            _options.SpecifySize = false;
            SizeLabel.IsEnabled = false;
            SizeCombo.IsEnabled = false;
            _options.Size = "";
        }

        /// <summary>
        /// 画像サイズの指定を保存する
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SizeCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _options.Size = SizeCombo.Text;
        }

        /// <summary>
        /// アスペクト比の指定を有効にする
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AspectCheck_Checked(object sender, RoutedEventArgs e)
        {
            _options.SpecifyAspect = true;
            AspectLabel.IsEnabled = true;
            AspectCombo.IsEnabled = true;
            _options.Aspect = AspectCombo.Text;
        }

        /// <summary>
        /// アスペクト比の指定を無効にする
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AspectCheck_Unchecked(object sender, RoutedEventArgs e)
        {
            _options.SpecifyAspect = false;
            AspectLabel.IsEnabled = false;
            AspectCombo.IsEnabled = false;
            _options.Aspect = "";
        }

        /// <summary>
        /// アスペクト比の指定を保存する
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AspectCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _options.Aspect = AspectCombo.Text;
        }

        /// <summary>
        /// ビットレートの指定を有効にする
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Bitrate1stCheck_Checked(object sender, RoutedEventArgs e)
        {
            _options.SetBitrate = true;
            AveBitrateDock.IsEnabled = true;
            MaxBitrateDock.IsEnabled = true;
            StatusLabel.Content = "";
            _options.AveBitrate = (AveBitrateText.Text == "") ? 0 : int.Parse(AveBitrateText.Text);
            _options.MaxBitrate = (MaxBitrateText.Text == "") ? 0 : int.Parse(MaxBitrateText.Text);
        }

        /// <summary>
        /// ビットレートの指定を無効にする
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Bitrate1stCheck_Unchecked(object sender, RoutedEventArgs e)
        {
            _options.SetBitrate = false;
            AveBitrateDock.IsEnabled= false;
            MaxBitrateDock.IsEnabled = false;
            StatusLabel.Content = "";
            _options.AveBitrate = 0;
            _options.MaxBitrate = 0;
        }

        /// <summary>
        /// ビットレートテキストボックスの入力を整数に限る
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BitrateText_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !Regex.IsMatch(e.Text, "[0-9]");
        }

        /// <summary>
        /// ビットレートテキストボックスへの貼り付けを許可しない
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BitrateText_PreviewExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            if (e.Command == ApplicationCommands.Paste)
            {
                e.Handled = true;
            }
        }

        /// <summary>
        /// 平均ビットレートの入力
        /// </summary>
        /// <remarks>
        /// 入力は正数で1Mbps～300Mbpsの範囲<br/>
        /// 最大ビットレートが平均ビットレート以上となるように最大ビットレートを調整
        /// </remarks>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AveBitrateText_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                var bitrate = int.Parse(AveBitrateText.Text);
                if ((bitrate < MIN_BITRATE) || (bitrate > MAX_BITRATE))
                {
                    AveBitrateText.Text = _options.AveBitrate.ToString();
                    StatusLabel.Content = "ビットレートは1Mbps～300Mbpsの範囲として下さい。";
                }
                else
                {
                    _options.AveBitrate = bitrate;
                    AveBitrateSlider.Value = bitrate;
                    if (_options.MaxBitrate < bitrate)
                    {
                        _options.MaxBitrate = bitrate;
                        MaxBitrateText.Text = bitrate.ToString();
                        MaxBitrateSlider.Value = bitrate;
                    }
                    StatusLabel.Content = "";
                }
            }
            catch 
            {
                AveBitrateText.Text = _options.AveBitrate.ToString();
                StatusLabel.Content = "ビットレートの入力は正数として下さい。";
            }
        }

        /// <summary>
        /// 最大ビットレートの入力
        /// </summary>
        /// <remarks>
        /// 入力は正数で1Mbps～300Mbpsの範囲<br/>
        /// ただし最大ビットレートが平均ビットレート以上
        /// </remarks>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MaxBitrateText_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                var bitrate = int.Parse(MaxBitrateText.Text);
                if ((bitrate < _options.AveBitrate) || (bitrate > MAX_BITRATE))
                {
                    MaxBitrateText.Text = _options.MaxBitrate.ToString();
                    StatusLabel.Content = "ビットレートは平均ビットレート～300Mbpsの範囲として下さい。";
                }
                else
                {
                    _options.MaxBitrate = bitrate;
                    MaxBitrateSlider.Value = bitrate;
                    StatusLabel.Content = "";
                }
            }
            catch
            {
                MaxBitrateText.Text = _options.MaxBitrate.ToString();
                StatusLabel.Content = "ビットレートの入力は正数として下さい。";
            }
        }

        /// <summary>
        /// スライダーによる平均ビットレートの入力
        /// </summary>
        /// <remarks>
        /// 入力は正数で1Mbps～300Mbpsの範囲<br/>
        /// 最大ビットレートが平均ビットレート以上となるように最大ビットレートを調整
        /// </remarks>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AveBitrateSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            var bitrate = (int)AveBitrateSlider.Value;
            _options.AveBitrate = bitrate;
            if (_options.MaxBitrate < bitrate)
            {
                _options.MaxBitrate = bitrate;
                MaxBitrateText.Text = bitrate.ToString();
                MaxBitrateSlider.Value = bitrate;
            }
            AveBitrateText.Text = bitrate.ToString();
            StatusLabel.Content = "";
        }

        /// <summary>
        /// スライダーによる最大ビットレートの入力
        /// </summary>
        /// <remarks>
        /// 入力は正数で1Mbps～300Mbpsの範囲<br/>
        /// ただし最大ビットレートが平均ビットレート以上
        /// </remarks>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MaxBitrateSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            var bitrate = (int)MaxBitrateSlider.Value;
            if (_options.AveBitrate > bitrate)
            {
                bitrate = _options.AveBitrate;
                MaxBitrateSlider.Value = bitrate;
            }
            _options.MaxBitrate = bitrate;
            MaxBitrateText.Text = bitrate.ToString();
            StatusLabel.Content = "";
        }
    }
}
