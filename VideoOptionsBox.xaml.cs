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
    public partial class VideoOptionsBox : UserControl
    {
        /// <value>ビデオのコーデックとエンコーダーの辞書</value>
        private static readonly Dictionary<string, string[]> s_encoderDic = new Dictionary<string, string[]>()
        {
            {"h264", new []{"libx264", "libopenh264", "h264_amf", "h264_mf", "h264_nvenc", "h264_qsv", }},
            {"msmpeg4v3", new []{"msmpeg4", }},
            {"mpeg4", new []{"mpeg4", "libxvid", }},
            {"flv1", new []{"flv", }},
            {"theora", new []{"libtheora", }},
            {"mpeg2video", new []{"mpeg2video", "mpeg2_qsv", }},
            {"vp9", new []{"libvpx-vp9", "vp9_qsv", }},
        };

        /// <value>アスペクト比</value>
        private static readonly string[] s_framerateList = { "24", "30000/1001", "30", "50", "60000/1001", "60", "120", "240" };

        /// <value>スクリーンサイズ</value>
        private static readonly string[] s_sizeList =
        {
            "320x240", "640x480", "720x480", "800x600", "1024x768", "1280x720", "1280x960", "1440x1080", "1600x1200", "1920x1080", "2560x1440", "3840x2160", "7680x4320",
        };

        /// <value>アスペクト比</value>
        private static readonly string[] s_aspectList = { "4:3", "16:9", };

        /// <value>最大ビットレート(Mbps)</value>
        private const double MAX_BITRATE = 35.0;
        /// <value>最小ビットレート(Mbps)</value>
        private const double MIN_BITRATE = 0.1;

        /// <value>ビデオのオプション</value>
        public VideoOptions Options { get; set; } = new VideoOptions();

        /// <param name="outputExtension">出力の拡張子</param>
        public VideoOptionsBox(string outputExtension)
        {

            InitializeComponent();

            SetOutputExtension(outputExtension);
        }

        /// <summary>
        /// 出力の拡張子設定によるコンポーネントの初期化
        /// </summary>
        /// <param name="extension">出力の拡張子</param>
        public void SetOutputExtension(string extension)
        {
            if (Options.OutputExtension != extension)
            {
                Options.OutputExtension = extension;

                var encoderList = s_encoderDic[Options.Codec];

                OutputGroup.Header = $"エンコーダー({Options.Codec})";

                HWDecoderCheck.IsChecked = Options.UseHWAccel;
                CopyCheck.IsChecked = Options.CopyVideo;
                EncoderStack.IsEnabled = (encoderList.Length > 1);
                EncoderCheck.IsChecked = Options.SpecifyEncoder;
                EncoderLabel.IsEnabled = Options.SpecifyEncoder;
                EncoderCombo.IsEnabled = Options.SpecifyEncoder;
                EncoderCombo.ItemsSource = encoderList;
                // エンコーダーはリストの最初がお勧め
                EncoderCombo.SelectedIndex = (Options.Encoder == "") ? 0 : Array.IndexOf(encoderList, Options.Encoder);
                FramerateCheck.IsChecked = Options.SpecifyFramerate;
                FramerateLabel.IsEnabled = Options.SpecifyFramerate;
                FramerateCombo.IsEnabled = Options.SpecifyFramerate;
                FramerateCombo.ItemsSource = s_framerateList;
                // フレームレートは29.97fpsが一般的か?
                FramerateCombo.SelectedIndex = (Options.Framerate == "") ? 1 : Array.IndexOf(s_framerateList, Options.Framerate);
                FPSLabel.IsEnabled = Options.SpecifyFramerate;
                SizeCheck.IsChecked = Options.SpecifySize;
                SizeLabel.IsEnabled = Options.SpecifySize;
                SizeCombo.IsEnabled = Options.SpecifySize;
                SizeCombo.ItemsSource = s_sizeList;
                // サイズはとりあえずDVDに合わせる
                SizeCombo.SelectedIndex = (Options.Size == "") ? 2 : Array.IndexOf(s_sizeList, Options.Size);
                AspectCheck.IsChecked = Options.SpecifyAspect;
                AspectLabel.IsEnabled = Options.SpecifyAspect;
                AspectCombo.IsEnabled = Options.SpecifyAspect;
                AspectCombo.ItemsSource = s_aspectList;
                // アスペクト比は16:9を標準とする
                AspectCombo.SelectedIndex = (Options.Aspect == "") ? 1 : Array.IndexOf(s_aspectList, Options.Aspect);
                QualityDefaultRadio.IsChecked = ((!Options.SetBitrate) && (!Options.ConstantQuality));
                QualityBitrateRadio.IsChecked = Options.SetBitrate;
                QualityImageRadio.IsChecked = Options.ConstantQuality;
                AveBitrateDock.IsEnabled = Options.SetBitrate;
                MaxBitrateDock.IsEnabled = Options.SetBitrate;
                AveBitrateText.Text = Options.AveBitrate.ToString();
                MaxBitrateText.Text = Options.MaxBitrate.ToString();
                AveBitrateSlider.Value = Options.AveBitrate;
                MaxBitrateSlider.Value = Options.MaxBitrate;
                AveBitrateSlider.Maximum = MAX_BITRATE;
                AveBitrateSlider.Minimum = MIN_BITRATE;
                MaxBitrateSlider.Maximum = MAX_BITRATE;
                MaxBitrateSlider.Minimum = MIN_BITRATE;
                StatusLabel.Content = "";
            }
        }

        /// <summary>
        /// ハードウェアデコーダーを有効にする
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void HWDecoderCheck_Checked(object sender, RoutedEventArgs e)
        {
            Options.UseHWAccel = true;
        }

        /// <summary>
        /// ハードウェアデコーダーを無効にする
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void HWDecoderCheck_Unchecked(object sender, RoutedEventArgs e)
        {
            Options.UseHWAccel = false;
        }

        /// <summary>
        /// ビデオのコピーを有効にする
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CopyCheck_Checked(object sender, RoutedEventArgs e)
        {
            Options.CopyVideo = true;
        }

        /// <summary>
        /// ビデオのコピーを無効にする
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CopyCheck_Unchecked(object sender, RoutedEventArgs e)
        {
            Options.CopyVideo = false;
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

            var index = EncoderCombo.SelectedIndex;
            Options.Encoder = (index < 0) ? "" : s_encoderDic[Options.Codec][index];
            Options.SpecifyEncoder = true;
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

            Options.Encoder = "";
            Options.SpecifyEncoder = false;
        }

        /// <summary>
        /// エンコーダーの選択肢を保存する
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void EncoderCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var index = EncoderCombo.SelectedIndex;
            Options.Encoder = (index < 0) ? "" : s_encoderDic[Options.Codec][index];
        }

        /// <summary>
        /// フレームレートの選択肢を有効にする
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FramerateCheck_Checked(object sender, RoutedEventArgs e)
        {
            FramerateLabel.IsEnabled = true;
            FramerateCombo.IsEnabled = true;
            FPSLabel.IsEnabled = true;

            var index = FramerateCombo.SelectedIndex;
            Options.Framerate = (index < 0) ? "" : s_framerateList[index];
            Options.SpecifyFramerate = true;
        }

        private void FramerateCheck_Unchecked(object sender, RoutedEventArgs e)
        {
            FramerateLabel.IsEnabled = false;
            FramerateCombo.IsEnabled = false;
            FPSLabel.IsEnabled = false;

            Options.Framerate = "";
            Options.SpecifyFramerate = false;
        }

        private void FramerateCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var index = FramerateCombo.SelectedIndex;
            Options.Framerate = (index < 0) ? "" : s_framerateList[index];
        }

        /// <summary>
        /// 画像サイズの指定を有効にする
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SizeCheck_Checked(object sender, RoutedEventArgs e)
        {
            SizeLabel.IsEnabled = true;
            SizeCombo.IsEnabled = true;

            var index = SizeCombo.SelectedIndex;
            Options.Size = (index < 0) ? "" : s_sizeList[index];
            Options.SpecifySize = true;
        }

        /// <summary>
        /// 画像サイズの指定を無効にする
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SizeCheck_Unchecked(object sender, RoutedEventArgs e)
        {
            SizeLabel.IsEnabled = false;
            SizeCombo.IsEnabled = false;

            Options.Size = "";
            Options.SpecifySize = false;
        }

        /// <summary>
        /// 画像サイズの指定を保存する
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SizeCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var index = SizeCombo.SelectedIndex;
            Options.Size = (index < 0) ? "" : s_sizeList[index];
        }

        /// <summary>
        /// アスペクト比の指定を有効にする
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AspectCheck_Checked(object sender, RoutedEventArgs e)
        {
            AspectLabel.IsEnabled = true;
            AspectCombo.IsEnabled = true;

            var index = AspectCombo.SelectedIndex;
            Options.Aspect = (index < 0) ? "" : s_aspectList[index];
            Options.SpecifyAspect = true;
        }

        /// <summary>
        /// アスペクト比の指定を無効にする
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AspectCheck_Unchecked(object sender, RoutedEventArgs e)
        {
            AspectLabel.IsEnabled = false;
            AspectCombo.IsEnabled = false;

            Options.Aspect = "";
            Options.SpecifyAspect = false;
        }

        /// <summary>
        /// アスペクト比の指定を保存する
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AspectCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var index = AspectCombo.SelectedIndex;
            Options.Aspect = (index < 0) ? "" : s_aspectList[index];
        }

        /// <summary>
        /// ビットレートの指定をデフォルトに戻す
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void QualityDefaultRadio_Checked(object sender, RoutedEventArgs e)
        {
            AveBitrateDock.IsEnabled = false;
            MaxBitrateDock.IsEnabled = false;
            StatusLabel.Content = "";

            Options.ConstantQuality = false;

            Options.AveBitrate = 0;
            Options.MaxBitrate = 0;
            Options.SetBitrate = false;
        }

        /// <summary>
        /// ビットレートの指定を有効にする
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void QualityBitrateRadio_Checked(object sender, RoutedEventArgs e)
        {
            AveBitrateDock.IsEnabled = true;
            MaxBitrateDock.IsEnabled = true;
            StatusLabel.Content = "";

            Options.ConstantQuality = false;

            Options.AveBitrate = (AveBitrateText.Text == "") ? 0 : int.Parse(AveBitrateText.Text);
            Options.MaxBitrate = (MaxBitrateText.Text == "") ? 0 : int.Parse(MaxBitrateText.Text);
            Options.SetBitrate = true;
        }

        /// <summary>
        /// 画質優先の指定を有効にする
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void QualityImageRadio_Checked(object sender, RoutedEventArgs e)
        {
            AveBitrateDock.IsEnabled = false;
            MaxBitrateDock.IsEnabled = false;
            StatusLabel.Content = "";

            Options.ConstantQuality = true;

            Options.AveBitrate = 0;
            Options.MaxBitrate = 0;
            Options.SetBitrate = false;
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
        /// 入力は正数で0.1Mbps～35Mbpsの範囲<br/>
        /// 最大ビットレートが平均ビットレート以上となるように最大ビットレートを調整
        /// </remarks>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AveBitrateText_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                var mbitrate = double.Parse(AveBitrateText.Text);
                var kbitrate = (int)(mbitrate * 1000);
                if ((kbitrate < MIN_BITRATE) || (kbitrate > MAX_BITRATE))
                {
                    AveBitrateText.Text = (Options.AveBitrate / 1000).ToString();
                    StatusLabel.Content = "ビットレートは0.1Mbps～35Mbpsの範囲として下さい。";
                }
                else
                {
                    var bitrate = kbitrate / 1000;
                    Options.AveBitrate = kbitrate;
                    AveBitrateSlider.Value = bitrate;
                    if (Options.MaxBitrate < kbitrate)
                    {
                        Options.MaxBitrate = kbitrate;
                        MaxBitrateText.Text = bitrate.ToString();
                        MaxBitrateSlider.Value = bitrate;
                    }
                    StatusLabel.Content = "";
                }
            }
            catch
            {
                AveBitrateText.Text = (Options.AveBitrate / 1000).ToString();
                StatusLabel.Content = "ビットレートの入力は正数として下さい。";
            }
        }

        /// <summary>
        /// 最大ビットレートの入力
        /// </summary>
        /// <remarks>
        /// 入力は正数で0.1Mbps～35Mbpsの範囲<br/>
        /// ただし最大ビットレートが平均ビットレート以上
        /// </remarks>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MaxBitrateText_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                var mbitrate = double.Parse(MaxBitrateText.Text);
                var kbitrate = (int)(mbitrate * 1000);
                if ((kbitrate < Options.AveBitrate) || (kbitrate > MAX_BITRATE))
                {
                    MaxBitrateText.Text = (Options.MaxBitrate / 1000).ToString();
                    StatusLabel.Content = "ビットレートは平均ビットレート～35Mbpsの範囲として下さい。";
                }
                else
                {
                    Options.MaxBitrate = kbitrate;
                    MaxBitrateSlider.Value = kbitrate / 1000;
                    StatusLabel.Content = "";
                }
            }
            catch
            {
                MaxBitrateText.Text = (Options.MaxBitrate / 1000).ToString();
                StatusLabel.Content = "ビットレートの入力は正数として下さい。";
            }
        }

        /// <summary>
        /// スライダーによる平均ビットレートの入力
        /// </summary>
        /// <remarks>
        /// 入力は正数で0.1Mbps～35Mbpsの範囲<br/>
        /// 最大ビットレートが平均ビットレート以上となるように最大ビットレートを調整
        /// </remarks>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AveBitrateSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            var mbitrate = (int)AveBitrateSlider.Value;
            var kbitrate = mbitrate * 1000;
            Options.AveBitrate = kbitrate;
            if (Options.MaxBitrate < kbitrate)
            {
                Options.MaxBitrate = kbitrate;
                MaxBitrateText.Text = mbitrate.ToString();
                MaxBitrateSlider.Value = mbitrate;
            }
            AveBitrateText.Text = mbitrate.ToString();
            StatusLabel.Content = "";
        }

        /// <summary>
        /// スライダーによる最大ビットレートの入力
        /// </summary>
        /// <remarks>
        /// 入力は正数で0.1Mbps～35Mbpsの範囲<br/>
        /// ただし最大ビットレートが平均ビットレート以上
        /// </remarks>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MaxBitrateSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            var mbitrate = (int)MaxBitrateSlider.Value;
            var kbitrate = mbitrate * 1000;
            if (Options.AveBitrate > kbitrate)
            {
                kbitrate = Options.AveBitrate;
                mbitrate = kbitrate / 1000;
                MaxBitrateSlider.Value = mbitrate;
            }
            Options.MaxBitrate = kbitrate;
            MaxBitrateText.Text = mbitrate.ToString();
            StatusLabel.Content = "";
        }
    }
}
