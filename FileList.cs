﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EasyFFmpeg
{

    /// <summary>
    /// 変換元および変換先のファイル名のリストの管理</br>
    /// 実際のファイルの変換も行う
    /// </summary>
    internal class FileList
    {
        /// <summary>
        /// 変換元および変換先のファイル名の対
        /// </summary>
        public class FileNames
        {
            /// <value>変換元ファイル名</value>
            public string FromFile { get; set; } = "";
            /// <value>変換先ファイル名</value>
            public string ToFile { get; set; } = "";
        }

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

        /// <value>変換元および変換先のファイル名のリスト</value>
        public ObservableCollection<FileNames> FileNameList { get; } = new ObservableCollection<FileNames>();
        /// <value>エラー等のメッセージ</value>
        public string Message { get; private set; } = "";
        /// <value>ファイルの変換先フォルダ名</value>
        public string TargetDir { get; set; } = "";
        /// <value>ファイル変換キャンセル用</value>
        private CancellationTokenSource? tokenSource = null;
        /// <value>オーディオターゲットかどうか</value>
        public bool AudioTarget { get; set; } = false;
        /// <value>ロック用オブジェクト</value>
        private readonly object balanceLock = new object();
        /// <value>入力ビデオファイルの拡張子</value>
        protected readonly List<string> video_extensions = new List<string>() {
            ".asf", ".avi", "swf", "flv", ".mkv", ".mov", ".mp4", ".ogv", ".ogg", ".ogx", ".ts", ".webm"
        };
        /// <value>入力オーディオファイルの拡張子</value>
        protected readonly List<string> audio_extensions = new List<string>() {
            ".aac", ".ac3", ".adpcm", ".amr", ".alac", ".fla", ".flac", ".mp1", ".mp2", ".mp3", ".als", ".pcm", ".qcp", ".ra", ".oga", "wma"
        };
        /// <value>変換先拡張子</value>
        public string Extension { get; set; } = ".mp4";
        /// <value>変換時に結合するかどうか</value>
        public bool Join { get; set; } = false;

        /// <summary>
        /// ファイルの変換元フォルダをセット</br>
        /// 同時に変換するファイル名をリストに登録
        /// </summary>
        /// <param name="dir">ファイルの変換元フォルダ名</param>
        /// <returns>処理結果</returns>
        public Code SetSourceDir(string dir)
        {
            Code result = Code.OK;

            try
            {
                var files = Directory.GetFiles(dir);
                FileNameList.Clear();

                foreach (var file in files)
                {
                    var ext = Path.GetExtension(file); // 最初の'.'を含む
                    if (video_extensions.Contains(ext) || (AudioTarget && audio_extensions.Contains(ext)))
                    {
                        FileNameList.Add(new FileNames() { FromFile = file });
                    }
                }
 
                this.MakeToFilesList();
            }
            catch (Exception e)
            {
                Message = e.Message;
                result = Code.NG;
            }

            return result;
        }

        /// <summary>
        /// "FileNameList"内にセットした変換元ファイル名と拡張子を元に変換先ファイル名を作成
        /// </summary>
        protected void MakeToFilesList()
        {
            foreach (var file in FileNameList)
            {
                var name = Path.GetFileNameWithoutExtension(file.FromFile);
                var path = Path.GetDirectoryName(file.FromFile);
                if (path != TargetDir)
                {
                    path = TargetDir;
                }
                file.ToFile = $"{path}\\{name}{Extension}";
            }
        }

        /// <summary>
        /// "FileNameList"にリストアップされた変換元ファイル名と変換先ファイル名を使いファイルを変換
        /// </summary>
        /// <param name="progress">プログレスバーダイアログ</param>
        /// <returns>処理結果</returns>
        public async Task<Code> ConvertFiles(FileConversionProgress progress)
        {
            Code result = Code.OK;
            int fileCount = 0;

            lock (balanceLock)
            {
                tokenSource = new CancellationTokenSource();
            }

            foreach (var file in FileNameList)
            {
                ProcessStartInfo info = new ProcessStartInfo();
                info.FileName = "ffmpeg";
                info.Arguments = $"-i {file.FromFile} {file.ToFile}";

                progress.SetFileNameProgress(Path.GetFileName(file.FromFile), (++fileCount * 100 / FileNameList.Count));

                try
                {
                    var ffmpeg = Process.Start(info);
                    if (ffmpeg != null)
                    {
                        ffmpeg.WaitForExit();
                    }
                }
                catch (Exception e)
                {
                    Message = e.Message;
                    result = tokenSource.IsCancellationRequested ? Code.Cancel : Code.NG;
                    break;
                }
            }

            lock (balanceLock)
            {
                tokenSource.Dispose();
                tokenSource = null;
            }

            return result;
        }

        /// <summary>
        /// "FileNameList"から指定のインデックスの要素を削除</br>
        /// 削除後に変換先ファイル名を付けなおす
        /// </summary>
        /// <param name="index">削除する要素のインデックス</param>
        public void DeleteElement(Int32 index)
        {
            if ((index >= 0) && (index < FileNameList.Count))
            {
                FileNameList.RemoveAt(index);
                MakeToFilesList();
            }
        }

        /// <summary>
        /// "FileNameList"の指定のインデックスの要素を上に移動</br>
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
                MakeToFilesList();
            }
        }

        /// <summary>
        /// "FileNameList"の指定のインデックスの要素を下に移動</br>
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
                MakeToFilesList();
            }
        }

        /// <summary>
        /// ファイルの変換を中断
        /// </summary>
        public void CancelCopy()
        {
            lock (balanceLock)
            {
                tokenSource?.Cancel();
            }
        }
    }
}
