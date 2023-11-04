using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyFFmpeg
{
    /// <summary>
    /// オーディオのオプション設定を保持
    /// </summary>
    public class AudioOptions
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

        /// <value>出力ファイルの拡張子</value>
        private string _outputExtension = "";
        public string OutputExtension
        {
            get => _outputExtension;
            set
            {
                if (_outputExtension != value)
                {
                    _outputExtension = value;
                    if ((!s_codecDic.ContainsKey(value)) || (Codec != s_codecDic[value]))
                    {
                        InitializeEncoderParams();
                    }
                }
            }
        }
        /// <value>オーディオ変換時にコピーができればコピーする</value>
        public bool CopyAudio { get; set; } = true;
        /// <value>コーデックを指定する</value>
        public string Codec { get; set; }
        /// <value>エンコーダーを指定する</value>
        public bool SpecifyEncoder { get; set; } = false;
        /// <value>使用するエンコーダー</value>
        public string Encoder { get; set; } = "";
        /// <value>チャンネル指定</value>
        public int Channel { get; set; } = 0;   // 0:default, 1:mono, 2:stereo
        /// <value>サンプリングレートを指定する</value>
        public bool SpecifySampling { get; set; } = false;
        /// <value>サンプリングレート</value>
        public string Sampling { get; set; } = "";
        /// <value>ビットレート指定</value>
        public bool SetBitrate { get; set; } = false;
        /// <value>ビットレート</value>
        public string Bitrate { get; set; } = "";
        /// <value>オーディオ出力に対する引数</value>
        public string Arguments { get; set; } = "";

        /// <summary>
        /// 各プロパティを初期化
        /// </summary>
        public void InitializeEncoderParams()
        {
            // CopyAudioはエンコーダーに関係ないのでそのまま
            Codec = s_codecDic[_outputExtension];
            SpecifyEncoder = false;
            Encoder = "";
            Channel = 0;
            SpecifySampling = false;
            Sampling = "";
            SetBitrate = false;
            Bitrate = "";
        }

        /// <summary>
        /// オーディオ出力をコピーするかどうかを決めて引数を作成
        /// </summary>
        /// <param name="file">入力ファイル名</param>
        /// <returns>オーディオ出力をコピーするかどうか</returns>
        protected bool CreateCopyArgument(string file)
        {
            bool doCopy = CopyAudio;

            try
            {
                var info = new FileInfo(file);

                doCopy &= (info.AudioCodec == Codec);
                doCopy &= (Channel == 0) || (info.GetAudioChannelNum() == Channel);
                doCopy &= (!SpecifySampling) || (info.AudioSamplingRate == Sampling);
                doCopy &= (!SetBitrate) || (info.AudioBitRate == Bitrate);

                if (doCopy)
                {
                    Arguments += $"-c:a copy ";
                }
            }
            catch
            {
                doCopy = false;
            }

            return doCopy;
        }

        /// <summary>
        /// オーディオ出力の引数を作成
        /// </summary>
        /// <param name="file">入力ファイル名</param>
        /// <param name="notCopy">ビデオをコピーしない指定</param>
        /// <returns>オーディオ出力の引数</returns>
        public string CreateArguments(string file, bool notCopy = false)
        {
            Arguments = "";

            bool rc = !notCopy;
            if (rc)
            {
                rc = CreateCopyArgument(file);
            }
            if (!rc)    // オーディオをコピーしない場合には各設定を追加
            {
                if (SpecifyEncoder && (Encoder != ""))
                {
                    Arguments += $"-c:a {Encoder} ";
                }
                if (Channel != 0)
                {
                    Arguments += $"-ac {Channel} ";
                }
                if (SpecifySampling && (Sampling != ""))
                {
                    Arguments += $"-ar {Sampling} ";
                }
                if (SetBitrate && (Bitrate != ""))
                {
                    Arguments += $"-b:a {Bitrate} ";
                }
            }

            return Arguments;
        }
    }
}
