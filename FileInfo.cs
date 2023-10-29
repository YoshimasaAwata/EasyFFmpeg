using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

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
        public string FFprobeInfo { get; protected set; } = "";
        /// <value>コンテナ名</value>
        public string Container { get; protected set; } = "";
        /// <value>再生時間</value>
        public string Duration { get; protected set; } = "";
        /// <value>オーディオコーデック</value>
        public string AudioCodec { get; protected set; } = "";

        /// <summary>
        /// コンストラクター
        /// </summary>
        /// <param name="file">情報を取得するファイル名</param>
        /// <exception cref="System.Exception">FFprobe実行時の不明な例外</exception>
        public FileInfo(string file)
        {
            FileName = file;

            DoFFprobe();
        }

        /// <summary>
        /// FFprobeを実行
        /// </summary>
        /// <exception cref="System.Exception">FFprobe実行時の不明な例外</exception>
        protected void DoFFprobe()
        {
            ProcessStartInfo info = new ProcessStartInfo();
            info.FileName = "ffprobe";
            info.Arguments = $"-hide_banner \"{FileName}\"";
            info.RedirectStandardError = true;
            info.UseShellExecute = false;
            info.CreateNoWindow = true;

            try
            {
                var ffprobe = Process.Start(info);
                if (ffprobe != null)
                {
                    FFprobeInfo = ffprobe.StandardError.ReadToEnd();
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
        /// FFprobeで取得した情報を分析
        /// </summary>
        protected void AnalyzeInfo()
        {
            var GetContent = (string src) => src.Substring(src.IndexOf(":") + 1).Trim();

            var lines = FFprobeInfo.Split('\n');
            var majorLine = Array.Find<string>(lines, (x => x.Contains("major_brand")));
            var dulationLine = Array.Find<string>(lines, (x => x.Contains("Duration:")));
            var audioLine = Array.Find<string>(lines, (x => (x.Contains("Stream #0") && x.Contains("Audio:"))));
            var VideoLine = Array.Find<string>(lines, (x => (x.Contains("Stream #0") && x.Contains("Video:"))));

            if (majorLine != null)
            {
                Container = GetContent(majorLine);
            }

            if (dulationLine != null)
            {
                var dLine = Regex.Match(dulationLine, @"Duration:\s*[0-9:]*").Value;
                Duration = Regex.Match(dLine, @"[0-9:]*").Value; 
            }

            if (audioLine != null)
            {
                var aLine = Regex.Match(audioLine, @"Audio:\s*\S+").Value;
                AudioCodec = GetContent(aLine);

            }
        }
    }
}
