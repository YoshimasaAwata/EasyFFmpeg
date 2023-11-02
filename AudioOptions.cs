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
    }
}
