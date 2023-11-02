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
using System.Windows.Shapes;

namespace EasyFFmpeg
{
    /// <summary>
    /// AudioOptionsBox.xaml の相互作用ロジック
    /// </summary>
    public partial class AudioOptionsBox : Window
    {
        /// <value>拡張子とオーディオのコーデック辞書</value>
        private static readonly Dictionary<string, string> s_codecDic = new Dictionary<string, string>()
        {
            {".mp4", "aac"},
            {".asf", "wmav2"},
            {".avi", "mp3"},
            {".swf", "mp3"},
            {".mkv", "vorbis"},
            {".mov", "aac"},
            {".ogg", "vorbis"},
            {".ts", "mp2"},
            {".webm", "opus"},
            {".aac", "aac"},
            {".ac3", "ac3"},
            {".mp3", "mp3"},
            {".m4a", "aac"},
            {".oga", "flac"},
            {".wma", "wmav2"},
            {".wav", "pcm_s16le"},
        };

        /// <value>オーディオのコーデックとエンコーダーの辞書</value>
        private static readonly Dictionary<string, string[]> s_encoderDic = new Dictionary<string, string[]>()
        {
            {"aac", new []{"aac", "aac_mf"}},
            {"wmav2", new []{"wmav2"}},
            {"mp3", new []{"libmp3lame", "mp3_mf"}},
            {"vorbis", new []{"vorbis", "libvorbis"}},
            {"mp2", new []{"mp2", "mp2fixed", "libtwolame"}},
            {"opus", new []{"opus", "libopus"}},
            {"ac3", new []{"ac3", "ac3_fixed", "ac3_mf"}},
            {"flac", new []{"flac"}},
            {"pcm_s16le", new []{"pcm_s16le"}},
        };

        /// <value>ビットレートに意味がないコーデック</value>
        private static readonly string[] s_noBitrateCodec = new[] {"flac", "pcm_s16le",}; 

        /// <value>サンプリング周波数</value>
        private static readonly string[] s_samplingList =
        {
            "16000", "22050", "32000", "44100", "48000",
        };

        /// <value>ビットレート</value>
        private static readonly string[] s_bitrateList =
        {
            "64k", "96k", "128k", "160k", "192k", "256k", "320k", "512k",
        };

        /// <value>オーディオのオプション</value>
        private AudioOptions _options;

        /// <param name="options">オーディオのオプション</param>
        public AudioOptionsBox(AudioOptions options)
        {
            InitializeComponent();

            _options = options;

            var codec = s_codecDic[options.OutputExtension];
            var encoderList = s_encoderDic[codec];

            CopyCheck.IsChecked = _options.CopyAudio;
            EncoderStack.IsEnabled = (encoderList.Length > 1);
            EncoderCheck.IsChecked = options.SpecifyEncoder;
            EncoderLabel.IsEnabled = options.SpecifyEncoder;
            EncoderCombo.IsEnabled = options.SpecifyEncoder;
            EncoderCombo.ItemsSource = encoderList;
            EncoderCombo.SelectedIndex = (options.Encoder == "") ? 0 : Array.IndexOf(encoderList, options.Encoder);
            ChannelDefaultRadio.IsChecked = (options.Channel == 0);
            ChannelMonoRadio.IsChecked = (options.Channel == 1);
            ChannelStereoRadio.IsChecked= (options.Channel == 2);
            SamplingCheck.IsEnabled = options.SpecifySampling;
            SamplingLabel.IsEnabled = options.SpecifySampling;
            SamplingCombo.IsEnabled = options.SpecifySampling;
            SamplingCombo.ItemsSource = s_samplingList;
            SamplingCombo.SelectedIndex = (options.Sampling == "") ? -1 : Array.IndexOf(s_samplingList, options.Sampling);
            BitrateStack.IsEnabled = (Array.IndexOf(s_noBitrateCodec, codec) < 0);
            BitrateCheck.IsChecked = options.SetBitrate;
            BitrateLabel.IsEnabled = options.SetBitrate;
            BitrateCombo.IsEnabled = options.SetBitrate;
            BitrateCombo.ItemsSource = s_bitrateList;
            BitrateCombo.SelectedIndex = (options.Bitrate == "") ? -1 : Array.IndexOf(s_bitrateList, options.Bitrate);
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
        /// AudioOptionsBoxをクローズ
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
