﻿using System;
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
        /// <value>ビデオ出力に対する引数</value>
        public string Arguments { get; set; } = "";

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
            Codec = "";
            SpecifyEncoder = false;
            Encoder = "";
            SpecifyFramerate = false;
            Framerate = "";
            SpecifySize = false;
            Size = "";
            SpecifyAspect = false;
            Aspect = "";
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
        /// <param name="info">入力ファイル情報</param>
        /// <returns>ビデオ出力をコピーするかどうか</returns>
        protected bool CreateCopyArgument(FileInfo info)
        {
            bool doCopy = CopyVideo;

            doCopy &= (info.VideoCodec == Codec);
            doCopy &= (!SpecifyFramerate) || (info.VideoFrameRate == Framerate);
            var originalSize = info.VideoWidth + "x" + info.VideoHeight;
            doCopy &= (!SpecifySize) || (originalSize == Size);
            doCopy &= (!SpecifyAspect);
            doCopy &= (!SetBitrate);
            // アスペクト比とビットレートは設定値に関係なく指定した時点でコピー不可とする

            if (doCopy)
            {
                Arguments += $"-c:v Copy ";
            }

            return doCopy;
        }

        /// <summary>
        /// ビデオ出力の引数を作成
        /// </summary>
        /// <param name="info">入力ファイル情報</param>
        /// <returns>ビデオ出力の引数</returns>
        public string CreateArguments(FileInfo info)
        {
            Arguments = "";

            bool rc = CreateCopyArgument(info);
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
                if (SetBitrate)
                {
                    Arguments += $"-b:v {AveBitrate}M -maxrate {MaxBitrate}M ";
                }
            }

            return Arguments;
        }
    }
}
