using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyFFmpeg
{
    /// <summary>
    /// ビデオのオプション設定を保持
    /// </summary>
    public class VideoOptions
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
        /// <value>デコーダーでハードウェアアクセラレーションを使用するか</value>
        public bool UseHWAccel { get; set; } = false;
        /// <value>ビデオ変換時にコピーができればコピーする</value>
        public bool CopyVideo { get; set; } = true;
        /// <value>エンコーダーを指定する</value>
        public bool SpecifyEncoder { get; set; } = false;
        /// <value>使用するエンコーダー</value>
        public string Encoder { get; set; } = "";
        /// <value>画像サイズを指定する</value>
        public bool SpecifySize { get; set; } = false;
        /// <value>画像サイズ(幅x高さ)</value>
        public string Size { get; set; } = "";
        /// <value>アスペクト比を指定する</value>
        public bool SpecifyAspect { get; set; } = false;
        /// <value>アスペクト比(横:縦)</value>
        public string Aspect { get; set; } = "";
        /// <value>CBR指定</value>
        public bool SetBitrate { get; set; } = false;
        /// <value>平均ビットレート</value>
        public int AveBitrate { get; set; } = 1;
        /// <value>最大ビットレート</value>
        public int MaxBitrate { get; set; } = 1;

        /// <param name="Extension">出力ファイルの拡張子</param>
        public VideoOptions(string Extension) 
        {
            _outputExtension = Extension;
        }

        /// <summary>
        /// 各プロパティを初期化
        /// </summary>
        public void Initialize()
        {
            UseHWAccel = false;
            CopyVideo = true;
            SpecifyEncoder = false;
            Encoder = "";
            SpecifySize = false;
            Size = "";
            SpecifyAspect = false;
            Aspect = "";
            SetBitrate = false;
            AveBitrate = 0;
            MaxBitrate = 0;
        }
    }
}
