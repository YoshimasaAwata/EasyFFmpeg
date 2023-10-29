using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;

namespace EasyFFmpeg
{
    /// <summary>
    /// ファイルの情報をFFprobeで取得し、保管
    /// </summary>
    internal class FileInfo
    {
        /// <value>エラーメッセージ</value>
        public string Message { get; protected set; } = "";
        /// <value>ファイル名</value>
        public string FileName { get; protected set; }
        /// <value>ファイル名</value>
        public string FFprobeXml { get; protected set; } = "";
        /// <value>コンテナ名</value>
        public string Container { get; protected set; } = "";
        /// <value>再生時間</value>
        public string Duration { get; protected set; } = "";
        /// <value>オーディオコーデック</value>
        public string AudioCodec { get; protected set; } = "";
        /// <value>オーディオサンプリングレート</value>
        public string AudioSamplingRate { get; protected set; } = "";
        /// <value>オーディオチャンネル数</value>
        public string AudioCannel { get; protected set; } = "";
        /// <value>オーディオビットレート</value>
        public string AudioBitRate { get; protected set; } = "";
        /// <value>ビデオコーデック</value>
        public string VideoCodec { get; protected set; } = "";
        /// <value>ビデオ幅</value>
        public string VideoWidth { get; protected set; } = "";
        /// <value>ビデオ高さ</value>
        public string VideoHeight { get; protected set; } = "";
        /// <value>ビデオフレームレート</value>
        public string VideoFrameRate { get; protected set; } = "";
        /// <value>ビデオビットレート</value>
        public string VideoBitRate { get; protected set; } = "";

        /// <summary>
        /// コンストラクター
        /// </summary>
        /// <param name="file">情報を取得するファイル名</param>
        /// <exception cref="System.Exception">FFprobe実行時の不明な例外</exception>
        public FileInfo(string file)
        {
            FileName = file;

            DoFFprobe();
            AnalyzeInfo();
        }

        /// <summary>
        /// FFprobeを実行
        /// </summary>
        /// <exception cref="System.Exception">FFprobe実行時の不明な例外</exception>
        protected void DoFFprobe()
        {
            ProcessStartInfo info = new ProcessStartInfo();
            info.FileName = "ffprobe";
            info.Arguments = $"-hide_banner -v error -i \"{FileName}\" -show_streams -show_format -print_format xml";
            info.RedirectStandardOutput = true;
            info.UseShellExecute = false;
            info.CreateNoWindow = true;

            try
            {
                var ffprobe = Process.Start(info);
                if (ffprobe != null)
                {
                    FFprobeXml = ffprobe.StandardOutput.ReadToEnd();
                    ffprobe.WaitForExit();
                }
            }
            catch (Exception e)
            {
                Message = e.Message;
                throw;
            }
        }

        /// <summary>
        /// コンテナに関する情報取得
        /// </summary>
        /// <param name="doc">XMLパーサー</param>
        protected void AnalyzeContainerInfo(XmlDocument doc)
        {
            var containerFormat = doc.SelectNodes("ffprobe/format")?.Item(0);

            if (containerFormat != null)
            {
                var attr = containerFormat.Attributes?["format_name"];
                if (attr != null)
                {
                    Container = attr.Value;
                }
                attr = containerFormat.Attributes?["duration"];
                if (attr != null)
                {
                    var time = attr.Value;
                    var idx = time.IndexOf('.');
                    if (idx >= 0)
                    {
                        Duration = time.Remove(idx);
                    }
                }
            }
        }

        /// <summary>
        /// オーディオに関する情報取得
        /// </summary>
        /// <param name="doc">XMLパーサー</param>
        protected void AnalyzeAudioInfo(XmlDocument doc)
        {
            var audioStream = doc.SelectNodes("ffprobe/streams/stream[@codec_type='audio']")?.Item(0);

            if (audioStream != null)
            {
                var attr = audioStream.Attributes?["codec_name"];
                if (attr != null)
                {
                    AudioCodec = attr.Value;
                }
                attr = audioStream.Attributes?["sample_rate"];
                if (attr != null)
                {
                    AudioSamplingRate = attr.Value;
                }
                attr = audioStream.Attributes?["channel_layout"];
                if (attr != null)
                {
                    AudioCannel = attr.Value;
                }
                attr = audioStream.Attributes?["bit_rate"];
                if (attr != null)
                {
                    AudioBitRate = attr.Value;
                }
            }
        }

        /// <summary>
        /// ビデオに関する情報取得
        /// </summary>
        /// <param name="doc">XMLパーサー</param>
        protected void AnalyzeVideoInfo(XmlDocument doc)
        {
            var videoStream = doc.SelectNodes("ffprobe/streams/stream[@codec_type='video']")?.Item(0);

            if (videoStream != null)
            {
                var attr = videoStream.Attributes?["codec_name"];
                if (attr != null)
                {
                    VideoCodec = attr.Value;
                }
                attr = videoStream.Attributes?["width"];
                if (attr != null)
                {
                    VideoWidth = attr.Value;
                }
                attr = videoStream.Attributes?["height"];
                if (attr != null)
                {
                    VideoHeight = attr.Value;
                }
                attr = videoStream.Attributes?["avg_frame_rate"];
                if (attr != null)
                {
                    VideoFrameRate = attr.Value;
                }
                attr = videoStream.Attributes?["bit_rate"];
                if (attr != null)
                {
                    VideoBitRate = attr.Value;
                }
            }
        }

        /// <summary>
        /// FFprobeで取得した情報を分析
        /// </summary>
        protected void AnalyzeInfo()
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(FFprobeXml);

            AnalyzeContainerInfo(doc);
            AnalyzeAudioInfo(doc);
            AnalyzeVideoInfo(doc);
        }
    }
}
