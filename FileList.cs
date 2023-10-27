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
    internal class FileList
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
        /// <value>ファイル変換キャンセル用</value>
        private CancellationTokenSource? tokenSource = null;
        /// <value>ロック用オブジェクト</value>
        private readonly object balanceLock = new object();
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
        /// <value>変換時に結合するかどうか</value>
        public bool Join { get; set; } = false;
        protected Process? ffmpeg = null;

        /// <summary>
        /// 変換元ファイルをセット
        /// </summary>
        /// <param name="files">変換元ファイル名</param>
        public void SetSourceFiles(IEnumerable<string> files)
        {
            foreach (var file in files)
            {
                var ext = Path.GetExtension(file); // 最初の'.'を含む
                if (VideoExtensions.Contains(ext) || AudioExtensions.Contains(ext))
                {
                    FileNameList.Add(file);
                }
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
            return $"{path}\\{name}{Extension}";
        }

        /// <summary>
        /// "FileNameList"にリストアップされた変換元ファイル名と変換先ファイル名を使いファイルを変換
        /// </summary>
        /// <remarks>
        /// 変換元ファイルは結合される
        /// </remarks>
        /// <param name="progress">プログレスバーダイアログ</param>
        /// <returns>処理結果</returns>
        public async Task<Code> JoinFiles(FileConversionProgress progress)
        {
            Code result = Code.OK;

            Message = "";

            lock (balanceLock)
            {
                tokenSource = new CancellationTokenSource();
            }

            ProcessStartInfo info = new ProcessStartInfo();
            info.FileName = "ffmpeg";
            info.Arguments = $"-i ";

            foreach (var file in FileNameList)
            {
                info.Arguments += $"\"{file.ToString()}\" ";
            }

            info.Arguments += $"\"{ToFileName(FileNameList[0])}\"";

            try
            {
                var ffmpeg = Process.Start(info);
                if (ffmpeg != null)
                {
                    await ffmpeg.WaitForExitAsync(tokenSource.Token);
                }
            }
            catch (Exception e)
            {
                Message = e.Message;
                result = tokenSource.IsCancellationRequested ? Code.Cancel : Code.NG;
            }

            lock (balanceLock)
            {
                tokenSource.Dispose();
                tokenSource = null;
            }

            return result;
        }

        /// <summary>
        /// "FileNameList"にリストアップされた変換元ファイル名と変換先ファイル名を使いファイルを変換
        /// </summary>
        /// <remarks>
        /// 変換元ファイルは個別に変換される
        /// </remarks>
        /// <param name="file">変換するファイル</param>
        /// <returns>処理結果</returns>
        public Code ConvertFiles(string file)
        {
            Code result = Code.OK;
//            int fileCount = 0;

            Message = "";

                ProcessStartInfo info = new ProcessStartInfo();
                info.FileName = "ffmpeg";
                info.Arguments = $"-i \"{file}\" \"{ToFileName(file)}\"";
                info.RedirectStandardError = true;
                info.UseShellExecute = false;
                info.CreateNoWindow = true;

//                progress.SetFileNameProgress(Path.GetFileName(file), (++fileCount * 100 / FileNameList.Count));

                try
                {
                    var ffmpeg = Process.Start(info);
                    if (ffmpeg != null)
                    {
                        string fileInfo = ffmpeg.StandardError.ReadToEnd();
                        ffmpeg.WaitForExit();
                        Debug.Write(fileInfo);
                    }
                }
                catch (Exception e)
                {
                    Message = e.Message;
                //                    result = tokenSource.IsCancellationRequested ? Code.Cancel : Code.NG;
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
        public Code PlayFile(Int32 index)
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
                ffmpeg = Process.Start(info);
                if (ffmpeg != null)
                {
                    ffmpeg.WaitForExit();
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
        public String GetFileInfo(Int32 index)
        {
            String fileInfo = "";

            Message = "";

            ProcessStartInfo info = new ProcessStartInfo();
            info.FileName = "ffprobe";
            info.Arguments = $"-hide_banner \"{FileNameList[index]}\"";
            info.RedirectStandardError = true;
            info.UseShellExecute = false;
            info.CreateNoWindow = true;

            try
            {
                ffmpeg = Process.Start(info);
                if (ffmpeg != null)
                {
                    fileInfo = ffmpeg.StandardError.ReadToEnd();
                    ffmpeg.WaitForExit();
                    ffmpeg = null;
                }
            }
            catch (Exception e)
            {
                Message = e.Message;
                return null;
            }

            return fileInfo;
        }

        /// <summary>
        /// ファイルの変換を中断
        /// </summary>
        public void CancelFFmpeg()
        {
            if (ffmpeg != null)
            {
                ffmpeg.Kill(true);
                ffmpeg = null;
            }
            lock (balanceLock)
            {
                tokenSource?.Cancel();
            }
        }
    }
}
