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

        /// <value>エンコーダーと品質指定オプションの辞書</value>
        private static readonly Dictionary<string, string> s_qualitySettings = new Dictionary<string, string>()
        {
            {"libopenh264", "" },                   // -rc_mode 0 (quality mode) default
            {"h264_amf", "-quality 2 "},            // Prefer Quality
            {"h264_mf", "-rate_control 3 "},        // Quality mode
            {"h264_nvenc", "-preset 17 "},          // slower (better quality)
            {"h264_qsv", "-preset 2 "},             // slower
            {"libx264", "-preset slower -crf 18 "},
            {"msmpeg4", "-q:v 4 "},
            {"mpeg4", "-q:v 4 "},
            {"libxvid", "-q:v 4 "},
            {"flv", "-crf 18"},                     // 怪しい "-q:v 4 "の方が良いか?
            {"libtheora", "-q:v 7"},
            {"mpeg2video", ""},
            {"mpeg2_qsv", "-preset 2 "},            // slower
            {"libvpx-vp9", "-b:v 0 -crf 30"},       // とりあえず例に上がっていた設定
            {"vp9_qsv", "-preset 2 "},              // slower
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
        /// <value>デコーダーでハードウェアアクセラレーションを使用するか</value>
        public bool UseHWAccel { get; set; } = false;
        /// <value>ビデオ変換時にコピーができればコピーする</value>
        public bool CopyVideo { get; set; } = true;
        /// <value>コーデックを指定する</value>
        public string Codec { get; set; } = "";
        /// <value>エンコーダーを指定する</value>
        public bool SpecifyEncoder { get; set; } = false;
        /// <value>使用するエンコーダー</value>
        public string Encoder { get; set; } = "";
        /// <value>フレームレート</value>
        public bool SpecifyFramerate { get; set; } = false;
        /// <value>フレームレート</value>
        public string Framerate { get; set; } = "";
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

        /// <value>一定品質指定</value>
        public bool ConstantQuality { get; set; } = false;
        /// <value>ビデオ出力に対する引数</value>
        public string Arguments { get; set; } = "";

        /// <summary>
        /// 各プロパティを初期化
        /// </summary>
        public void InitializeEncoderParams()
        {
            // UseHWAccelとCopyVideoはエンコーダーに関係ないのでそのまま
            Codec = s_codecDic[_outputExtension];
            SpecifyEncoder = false;
            Encoder = "";
            SpecifyFramerate = false;
            Framerate = "";
            SpecifySize = false;
            Size = "";
            SpecifyAspect = false;
            Aspect = "";
            ConstantQuality = false;
            SetBitrate = false;
            AveBitrate = 0;
            MaxBitrate = 0;
        }

        public string CreateHWDecoderArgument()
        {
            return (UseHWAccel) ? "-hwaccel auto " : "";
        }

        /// <summary>
        /// ビデオ出力をコピーするかどうかを決めて引数を作成
        /// </summary>
        /// <param name="file">入力ファイル名</param>
        /// <returns>ビデオ出力をコピーするかどうか</returns>
        protected bool CreateCopyArgument(string file)
        {
            bool doCopy = CopyVideo;

            try
            {
                var info = new FileInfo(file);

                doCopy &= (info.VideoCodec == Codec);
                doCopy &= (!SpecifyFramerate) || (info.VideoFrameRate == Framerate);
                var originalSize = info.VideoWidth + "x" + info.VideoHeight;
                doCopy &= (!SpecifySize) || (originalSize == Size);
                doCopy &= (!SpecifyAspect);
                doCopy &= (!ConstantQuality);
                doCopy &= (!SetBitrate);
                // アスペクト比とビットレートは設定値に関係なく指定した時点でコピー不可とする

                if (doCopy)
                {
                    Arguments += $"-c:v copy ";
                }
            }
            catch
            {
                doCopy = false;
            }

            return doCopy;
        }

        /// <summary>
        /// ビデオ出力の引数を作成
        /// </summary>
        /// <param name="file">入力ファイル名</param>
        /// <param name="notCopy">ビデオをコピーしない指定</param>
        /// <returns>ビデオ出力の引数</returns>
        public string CreateArguments(string file, bool notCopy = false)
        {
            Arguments = "";

            bool rc = !notCopy;
            if (rc)
            {
                rc = CreateCopyArgument(file);
            }
            if (!rc)    // ビデオをコピーしない場合には各設定を追加
            {
                if (SpecifyEncoder && (Encoder != ""))
                {
                    Arguments += $"-c:v {Encoder} ";
                }
                if (SpecifyFramerate && (Framerate != ""))
                {
                    Arguments += $"-r {Framerate} ";
                }
                if (SpecifySize && (Size != ""))
                {
                    Arguments += $"-s {Size} ";
                }
                if (SpecifyAspect && (Aspect != ""))
                {
                    Arguments += $"-aspect {Aspect} ";
                }
                if (ConstantQuality && (s_qualitySettings.ContainsKey(Encoder)))
                {
                    Arguments += s_qualitySettings[Encoder];
                }
                if (SetBitrate)
                {
                    Arguments += $"-b:v {AveBitrate}M -maxrate {MaxBitrate}M ";
                }
            }

            return Arguments;
        }
    }
}
