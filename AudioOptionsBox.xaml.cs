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
    public partial class AudioOptionsBox : UserControl
    {
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
        private static readonly string[] s_noBitrateCodec = new[] { "flac", "pcm_s16le", };

        /// <value>サンプリング周波数</value>
        private static readonly string[] s_samplingList =
        {
            "16000", "22050", "32000", "44100", "48000",
        };

        /// <value>ビットレート</value>
        private static readonly string[] s_bitrateList =
        {
            "64k", "96k", "128k", "160k", "192k", "256k", "320k", "384k", "512k",
        };

        /// <value>オーディオのオプション</value>
        public AudioOptions Options { get; set; } = new AudioOptions();

        /// <param name="outputExtension">出力の拡張子</param>
        public AudioOptionsBox(string outputExtension)
        {

            InitializeComponent();

            SetOutputExtension(outputExtension);
        }

        public void SetOutputExtension(string extension)
        {
            if (Options.OutputExtension != extension)
            {
                Options.OutputExtension = extension;

                var encoderList = s_encoderDic[Options.Codec];

                OutputGroup.Header = $"エンコーダー({Options.Codec})";

                CopyCheck.IsChecked = Options.CopyAudio;
                EncoderStack.IsEnabled = (encoderList.Length > 1);
                EncoderCheck.IsChecked = Options.SpecifyEncoder;
                EncoderLabel.IsEnabled = Options.SpecifyEncoder;
                EncoderCombo.IsEnabled = Options.SpecifyEncoder;
                EncoderCombo.ItemsSource = encoderList;
                // エンコーダーはリストの最初がお勧め
                EncoderCombo.SelectedIndex = (Options.Encoder == "") ? 0 : Array.IndexOf(encoderList, Options.Encoder);
                ChannelDefaultRadio.IsChecked = (Options.Channel == 0);
                ChannelMonoRadio.IsChecked = (Options.Channel == 1);
                ChannelStereoRadio.IsChecked = (Options.Channel == 2);
                SamplingCheck.IsChecked = Options.SpecifySampling;
                SamplingLabel.IsEnabled = Options.SpecifySampling;
                SamplingCombo.IsEnabled = Options.SpecifySampling;
                SamplingCombo.ItemsSource = s_samplingList;
                // サンプリングレートは48kを推奨
                SamplingCombo.SelectedIndex = (Options.Sampling == "") ? 4 : Array.IndexOf(s_samplingList, Options.Sampling);
                BitrateStack.IsEnabled = (Array.IndexOf(s_noBitrateCodec, Options.Codec) < 0);
                BitrateCheck.IsChecked = Options.SetBitrate;
                BitrateLabel.IsEnabled = Options.SetBitrate;
                BitrateCombo.IsEnabled = Options.SetBitrate;
                BitrateCombo.ItemsSource = s_bitrateList;
                // ビットレートは384kbpsを推奨
                BitrateCombo.SelectedIndex = (Options.Bitrate == "") ? 7 : Array.IndexOf(s_bitrateList, Options.Bitrate);
            }
        }

        /// <summary>
        /// オーディオのコピーを有効にする
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CopyCheck_Checked(object sender, RoutedEventArgs e)
        {
            Options.CopyAudio = true;
        }

        /// <summary>
        /// オーディオのコピーを無効にする
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CopyCheck_Unchecked(object sender, RoutedEventArgs e)
        {
            Options.CopyAudio = false;
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
        /// エンコーダーのチャンネルを入力と同じとする
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ChannelDefaultRadio_Checked(object sender, RoutedEventArgs e)
        {
            Options.Channel = 0;
        }

        /// <summary>
        /// エンコーダーのチャンネルをモノラルとする
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ChannelMonoRadio_Checked(object sender, RoutedEventArgs e)
        {
            Options.Channel = 1;
        }

        /// <summary>
        /// エンコーダーのチャンネルをステレオとする
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ChannelStereoRadio_Checked(object sender, RoutedEventArgs e)
        {
            Options.Channel = 2;
        }

        /// <summary>
        /// サンプリングレートの選択肢を有効にする
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SamplingCheck_Checked(object sender, RoutedEventArgs e)
        {
            SamplingLabel.IsEnabled = true;
            SamplingCombo.IsEnabled = true;

            var index = SamplingCombo.SelectedIndex;
            Options.Sampling = (index < 0) ? "" : s_samplingList[index];
            Options.SpecifySampling = true;
        }

        /// <summary>
        /// サンプリングレートの選択肢を無効にする
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SamplingCheck_Unchecked(object sender, RoutedEventArgs e)
        {
            SamplingLabel.IsEnabled = false;
            SamplingCombo.IsEnabled = false;

            Options.Sampling = "";
            Options.SpecifySampling = false;
        }

        /// <summary>
        /// サンプリングレートの選択肢を保存する
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SamplingCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var index = SamplingCombo.SelectedIndex;
            Options.Sampling = (index < 0) ? "" : s_samplingList[index];
        }

        /// <summary>
        /// ビットレートの選択肢を有効にする
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BitrateCheck_Checked(object sender, RoutedEventArgs e)
        {
            BitrateLabel.IsEnabled = true;
            BitrateCombo.IsEnabled = true;

            var index = BitrateCombo.SelectedIndex;
            Options.Bitrate = (index < 0) ? "" : s_bitrateList[index];
            Options.SetBitrate = true;
        }

        /// <summary>
        /// ビットレートの選択肢を無効にする
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BitrateCheck_Unchecked(object sender, RoutedEventArgs e)
        {
            BitrateLabel.IsEnabled = false;
            BitrateCombo.IsEnabled = false;

            Options.Bitrate = "";
            Options.SetBitrate = false;
        }

        /// <summary>
        /// ビットレートの選択肢を保存する
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BitrateCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var index = BitrateCombo.SelectedIndex;
            Options.Bitrate = (index < 0) ? "" : s_bitrateList[index];
        }
    }
}
