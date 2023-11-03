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
        /// <value>出力ファイルの拡張子</value>
        private string _outputExtension;
        public string OutputExtension {
            get => _outputExtension;
            set 
            {
                if (_outputExtension != value)
                {
                    _outputExtension = value;
                    Initialize();
                }
            } 
        }
        /// <value>オーディオ変換時にコピーができればコピーする</value>
        public bool CopyAudio { get; set; } = true;
        /// <value>コーデックを指定する</value>
        public string Codec { get; set; } = "";
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

        /// <param name="Extension">出力ファイルの拡張子</param>
        public AudioOptions(string Extension) 
        {
            _outputExtension = Extension;
        }

        /// <summary>
        /// 各プロパティを初期化
        /// </summary>
        public void Initialize()
        {
            CopyAudio = true;
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
        /// <param name="info">入力ファイル情報</param>
        /// <returns>オーディオ出力をコピーするかどうか</returns>
        protected bool CreateCopyArgument(FileInfo info)
        {
            bool doCopy = CopyAudio;

            doCopy &= (info.AudioCodec == Codec);
            doCopy &= (Channel == 0) || (info.GetAudioChannelNum() == Channel);
            doCopy &= (!SpecifySampling) || (info.AudioSamplingRate == Sampling);
            doCopy &= (!SetBitrate) || (info.AudioBitRate == Bitrate);

            if (doCopy)
            {
                Arguments += $"-c:a Copy ";
            }

            return doCopy;
        }

        /// <summary>
        /// オーディオ出力の引数を作成
        /// </summary>
        /// <param name="info">入力ファイル情報</param>
        /// <returns>オーディオ出力の引数</returns>
        public string CreateArguments(FileInfo info)
        {
            Arguments = "";

            bool rc = CreateCopyArgument(info);
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
