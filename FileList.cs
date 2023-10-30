using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Printing;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EasyFFmpeg
{

    /// <summary>
    /// 変換元および変換先のファイル名のリストの管理<br/>
    /// 実際のファイルの変換も行う
    /// </summary>
    public class FileList
    {
        /// <summary>
        /// メソッド実行結果
        /// </summary>
        public enum Code
        {
            /// <value>成功</value>
            OK,
            /// <value>失敗</value>
            NG,
            /// <value>キャンセル</value>
            Cancel,
        }

        /// <value>ビデオの変換元および変換先のファイル名のリスト</value>
        public ObservableCollection<string> FileNameList { get; } = new ObservableCollection<string>();
        /// <value>エラー等のメッセージ</value>
        public string Message { get; private set; } = "";
        /// <value>ファイルの変換先フォルダ名</value>
        public string TargetDir { get; set; } = "";
        /// <value>入力ビデオファイルの拡張子</value>
        public List<string> VideoExtensions { get; } = new List<string> {
            ".mp4", ".asf", ".avi", ".swf", ".flv", ".mkv", ".mov", ".ogv", ".ogg", ".ogx", ".ts", ".webm"
        };
        /// <value>入力オーディオファイルの拡張子</value>
        public List<string> AudioExtensions = new List<string> {
            ".aac", ".ac3", ".adpcm", ".amr", ".alac", ".fla", ".flac", ".mp1", ".mp2", ".mp3", ".m4a", ".als", ".pcm", ".qcp", ".ra", ".oga", ".wma"
        };
        /// <value>変換先拡張子</value>
        public string Extension { get; set; } = ".mp4";

        /// <summary>
        /// 変換元ファイルをセット
        /// </summary>
        /// <param name="files">変換元ファイル名</param>
        public void SetSourceFiles(IEnumerable<string> files)
        {
            foreach (var file in files)
            {
                AddSourceFile(file);
            }
        }

        /// <summary>
        /// 変換元ファイルをセット
        /// </summary>
        /// <param name="files">変換元ファイル名</param>
        public void AddSourceFile(string file)
        {
            var ext = Path.GetExtension(file); // 最初の'.'を含む
            if (VideoExtensions.Contains(ext) || AudioExtensions.Contains(ext))
            {
                FileNameList.Add(file);
            }
        }

        /// <summary>
        /// 変換元のファイル名から変換先のファイル名を作成
        /// </summary>
        /// <param name="fromFileName">変換元ファイル名</param>
        /// <returns></returns>
        protected string ToFileName(string fromFileName)
        {
            var name = Path.GetFileNameWithoutExtension(fromFileName);
            var path = (TargetDir == "") ? Path.GetDirectoryName(fromFileName) : TargetDir;
            return $"\"{path}\\{name}{Extension}\"";
        }

        /// <summary>
        /// ファイル結合/変換時のFFmpegの引数を作成
        /// </summary>
        /// <returns>引数</returns>
        protected string CreateArgumentsJoin()
        {
            var args = $"-hide_banner";

            foreach (var file in FileNameList)
            {
                args += $" -i \"{file}\"";
            }

            try
            {
                FileInfo info = new FileInfo(FileNameList[0]);
                if (info.AudioCodec != "")
                {
                    var abr = info.GetAudioBitRate();
                    args += (abr > 0) ? $" -b:a {abr}" : "";
                }
                if (info.VideoCodec != "")
                {
                    //var vbr = info.GetVideoBitRate();
                    //args += (vbr > 0) ? $" -b:v {vbr}" : "";
                    //args += (vbr > 0) ? $" -crf 10" : "";
                    if (Extension == ".mp4")
                    {
                        //args += " -c:v h264_nvenc";
                        args += " -c:v libopenh264";
                    }
                }
            }
            catch (Exception e) 
            {
                Message = e.Message;
            }

            args += $" -filter_complex \"concat=n={FileNameList.Count}:v=1:a=1\" ";
            args += $" {ToFileName(FileNameList[0])}";

            return args;
        }

        /// <summary>
        /// "FileNameList"にリストアップされた変換元ファイル名と変換先ファイル名を使いファイルを変換
        /// </summary>
        /// <remarks>
        /// 変換元ファイルは結合される
        /// </remarks>
        /// <returns>処理結果</returns>
        public async Task<Code> JoinFiles()
        {
            Code result = Code.OK;

            Message = "";

            ProcessStartInfo info = new ProcessStartInfo();
            info.FileName = "ffmpeg";
            info.Arguments = CreateArgumentsJoin();
            info.UseShellExecute = false;

            try
            {
                var ffmpeg = Process.Start(info);
                if (ffmpeg != null)
                {
                    await ffmpeg.WaitForExitAsync();
                    if (ffmpeg.ExitCode != 0)
                    {
                        if ((ffmpeg.ExitCode == -1073741510) || (ffmpeg.ExitCode == 255))
                        {
                            Message = "変換がキャンセルされました。";
                            result = Code.Cancel;
                        }
                        else
                        {
                            Message = "変換に失敗しました。";
                            result = Code.NG;
                        }
                    }
                    Debug.Write($"{ffmpeg.ExitCode}\n");
                }
            }
            catch (Exception e)
            {
                Message = "結合/変換に失敗しました\n" + e.Message;
                result = Code.NG;
            }

            return result;
        }

        /// <summary>
        /// ファイル変換時のFFmpegの引数を作成
        /// </summary>
        /// <param name="file">変換元のファイル名</param>
        /// <returns>引数</returns>
        protected string CreateArguments(string file)
        {
            var args = $"-hide_banner -i \"{file}\"";

            try
            {
                FileInfo info = new FileInfo(file);
                var ext = Path.GetExtension(file);
                if (ext == Extension)
                {
                    if (info.AudioCodec != "")
                    {
                        args += " -c:a copy";
                    }
                    if (info.VideoCodec != "")
                    {
                        args += " -c:v copy";
                    }
                }
                else
                {
                    if (info.AudioCodec != "")
                    {
                        var abr = info.GetAudioBitRate();
                        args += (abr > 0) ? $" -b:a {abr}" : "";
                    }
                    if (info.VideoCodec != "")
                    {
                        //var vbr = info.GetVideoBitRate();
                        //args += (vbr > 0) ? $" -b:v {vbr}" : "";
                        //args += (vbr > 0) ? $" -crf 10" : "";
                        if (Extension == ".mp4")
                        {
                            //args += " -c:v h264_nvenc";
                            args += " -c:v libopenh264";
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Message = e.Message;
            }

            args += $" {ToFileName(file)}";

            return args;
        }

        /// <summary>
        /// "FileNameList"にリストアップされた変換元ファイル名と変換先ファイル名を使いファイルを変換
        /// </summary>
        /// <param name="index">変換する要素のインデックス</param>
        /// <returns>処理結果</returns>
        public async Task<Code> ConvertFiles(Int32 index)
        {
            Code result = Code.OK;
            //            int fileCount = 0;
            var file = FileNameList[index];

            Message = "";

            ProcessStartInfo info = new ProcessStartInfo();
            info.FileName = "ffmpeg";
            info.Arguments = CreateArguments(file);
            info.UseShellExecute = false;

            try
            {
                var ffmpeg = Process.Start(info);
                if (ffmpeg != null)
                {
                    await ffmpeg.WaitForExitAsync();
                    if (ffmpeg.ExitCode != 0)
                    {
                        Message = Path.GetFileName(file);
                        if ((ffmpeg.ExitCode == -1073741510) || (ffmpeg.ExitCode == 255))
                        {
                            Message += "の変換がキャンセルされました。";
                            result = Code.Cancel;
                        }
                        else
                        {
                            Message += "の変換に失敗しました。";
                            result = Code.NG;
                        }
                    }
                    Debug.Write($"{ffmpeg.ExitCode}\n");
                }
            }
            catch (Exception e)
            {
                Message = Path.GetFileName(file) + "の変換に失敗しました\n" + e.Message;
                result = Code.NG;
            }

            return result;
        }

        /// <summary>
        /// "FileNameList"から指定のインデックスの要素を削除<br/>
        /// 削除後に変換先ファイル名を付けなおす
        /// </summary>
        /// <param name="index">削除する要素のインデックス</param>
        public void DeleteElement(Int32 index)
        {
            if ((index >= 0) && (index < FileNameList.Count))
            {
                FileNameList.RemoveAt(index);
            }
        }

        /// <summary>
        /// "FileNameList"の指定のインデックスの要素を上に移動<br/>
        /// 移動後に変換先ファイル名を付けなおす
        /// </summary>
        /// <remarks>
        /// 先頭の要素は移動できないので何もしない
        /// </remarks>
        /// <param name="index">移動する要素のインデックス</param>
        public void UpElement(Int32 index)
        {
            if ((index > 0) && (index < FileNameList.Count))
            {
                FileNameList.Move(index, (index - 1));
            }
        }

        /// <summary>
        /// "FileNameList"の指定のインデックスの要素を下に移動<br/>
        /// 移動後に変換先ファイル名を付けなおす
        /// </summary>
        /// <remarks>
        /// 最後の要素は移動できないので何もしない
        /// </remarks>
        /// <param name="index">移動する要素のインデックス</param>
        public void DownElement(Int32 index)
        {
            if ((index >= 0) && (index < (FileNameList.Count - 1)))
            {
                FileNameList.Move(index, (index + 1));
            }
        }

        /// <summary>
        /// 指定のインデックスのファイルを再生する
        /// </summary>
        /// <param name="index">移動する要素のインデックス</param>
        /// <returns>処理結果</returns>
        public async Task<Code> PlayFile(Int32 index)
        {
            Code result = Code.OK;

            Message = "";

            ProcessStartInfo info = new ProcessStartInfo();
            info.FileName = "ffplay";
            info.Arguments = $"-autoexit \"{FileNameList[index]}\"";
            info.UseShellExecute = false;
            info.CreateNoWindow = true;

            try
            {
                var ffmpeg = Process.Start(info);
                if (ffmpeg != null)
                {
                    await ffmpeg.WaitForExitAsync();
                    ffmpeg = null;
                }
            }
            catch (Exception e)
            {
                Message = e.Message;
                result = Code.NG;
            }

            return result;
        }

        /// <summary>
        /// 指定のインデックスのファイルの情報を得る
        /// </summary>
        /// <param name="index">移動する要素のインデックス</param>
        /// <returns>
        /// ファイルの情報</br>
        /// 取得失敗の場合にはnull
        /// </returns>
        public string? GetFileInfo(Int32 index)
        {
            string info = "ファイル情報:\n";
            Message = "";

            try
            {
                FileInfo fileInfo = new FileInfo(FileNameList[index]);
                info += "  コンテナ情報\n";
                info += $"    フォーマット: {fileInfo.Container}\n";
                info += $"    再生時間: {fileInfo.Duration} 秒\n";
                if (fileInfo.AudioCodec != "")
                {
                    info += "  オーディオ情報:\n";
                    info += $"    オーディオコーデック: {fileInfo.AudioCodec}\n";
                    info += $"    オーディオサンプリングレート: {fileInfo.AudioSamplingRate} Hz\n";
                    info += $"    オーディオチャンネル: {fileInfo.AudioCannel}\n";
                    info += $"    オーディオビットレート: {fileInfo.AudioBitRate} bps\n";
                }
                if (fileInfo.VideoCodec != "")
                {
                    info += "  ビデオ情報:\n";
                    info += $"    ビデオコーデック: {fileInfo.VideoCodec}\n";
                    info += $"    ビデオ幅: {fileInfo.VideoWidth} ピクセル\n";
                    info += $"    ビデオ高さ: {fileInfo.VideoHeight} ピクセル\n";
                    info += $"    ビデオフレームレート: {fileInfo.VideoFrameRate} fps\n";
                    info += $"    ビデオビットレート: {fileInfo.VideoBitRate} bps\n";
                }
            }
            catch (Exception e)
            {
                Message = e.Message;
                return null;
            }

            return info;
        }
    }
}
