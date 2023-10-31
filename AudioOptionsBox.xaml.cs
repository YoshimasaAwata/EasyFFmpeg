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
        /// <value>拡張子とオーディオのコーデック、エンコーダーの辞書</value>
        private static readonly Dictionary<string, CodecLibPair> audioDic = new Dictionary<string, CodecLibPair>()
        {
            {".mp4", new CodecLibPair("aac", new []{"aac", "aac_mf"})},
            {".asf", new CodecLibPair("wmav2", new []{"wmav2"})},
            {".avi", new CodecLibPair("mp3", new []{"libmp3lame", "mp3_mf"})},
            {".swf", new CodecLibPair("mp3", new []{"libmp3lame", "mp3_mf"})},
            {".mkv", new CodecLibPair("vorbis", new []{"vorbis", "libvorbis"})},
            {".mov", new CodecLibPair("aac", new []{"aac", "aac_mf"})},
            {".ogg", new CodecLibPair("vorbis", new []{"vorbis", "libvorbis"})},
            {".ts", new CodecLibPair("mp2", new []{"mp2", "mp2fixed", "libtwolame"})},
            {".webm", new CodecLibPair("opus", new []{"opus", "libopus"})},
            {".aac", new CodecLibPair("aac", new []{"aac", "aac_mf"})},
            {".ac3", new CodecLibPair("ac3", new []{"ac3", "ac3_fixed", "ac3_mf"})},
            {".amr", new CodecLibPair("amr_nb", new []{"libopencore_amrnb"})},
            {".mp3", new CodecLibPair("mp3", new []{"libmp3lame", "mp3_mf"})},
            {".m4a", new CodecLibPair("aac", new []{"aac", "aac_mf"})},
            {".oga", new CodecLibPair("flac", new []{"flac"})},
            {".wma", new CodecLibPair("wmav2", new []{"wmav2"}) },
            {".wav", new CodecLibPair("pcm_s16le", new []{"pcm_s16le"}) },
        };

        public AudioOptionsBox()
        {
            InitializeComponent();
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
