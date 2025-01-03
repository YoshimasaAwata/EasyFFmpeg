﻿using System;
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

        /// <summary>
        /// ビデオのパス指定
        /// </summary>
        public enum VideoPass
        {
            /// <value>1パス</value>
            OnePass,
            /// <value>2パスファーストパス</value>
            TwoPass1st,
            /// <value>2パスセカンドパス</value>
            TwoPass2nd,
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
        private string Extension { get; set; } = ".mp4";
        /// <value>ビデオコーデックを使用する</value>
        public bool UseVideoCodec = true;
        /// <value>ビデオオプション</value>
        public VideoOptions VideoOptions { get; set; }
        /// <value>オーディオオプション</value>
        public AudioOptions AudioOptions { get; set; }
        /// <value>追加のオプション</value>
        private string _additionalOptions = "";
        public string AdditionalOptions
        {
            get => _additionalOptions;
            set
            {
                _additionalOptions = value.Trim() + " ";
            }
        }
        /// <value>引数の履歴</value>
        public StringBuilder ArgumentsHistory { get; private set; } = new StringBuilder();

        public FileList(VideoOptions video, AudioOptions audio)
        {
            VideoOptions = video;
            AudioOptions = audio;
        }

        /// <summary>
        /// ビデオの拡張子のリストからインデックスで拡張子を取得
        /// </summary>
        /// <param name="index">拡張子を指定するインデックス</param>
        /// <returns>ビデオの拡張子</returns>
        public string GetVideoExtension(int index)
        {
            return VideoExtensions[index];
        }

        /// <summary>
        /// オーディオの拡張子のリストからインデックスで拡張子を取得
        /// </summary>
        /// <param name="index">拡張子を指定するインデックス</param>
        /// <returns>オーディオの拡張子</returns>
        public string GetAudioExtension(int index)
        {
            return AudioExtensions[index];
        }

        /// <summary>
        /// 出力の拡張子を設定
        /// </summary>
        /// <param name="outputExtension">出力の拡張子</param>
        public void SetOutputExtension(string? outputExtension)
        {
            if (outputExtension != null)
            {
                UseVideoCodec = VideoExtensions.Contains(outputExtension);
                Extension = outputExtension;
            }
        }

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
        /// 引数の
        /// </summary>
        public void ClearArgumentsNote()
        {
            ArgumentsHistory.Clear();
        }

        /// <summary>
        /// ファイル結合/変換時のFFmpegの引数を作成
        /// </summary>
        /// <returns>引数</returns>
        protected string CreateArgumentsJoin()
        {
            var args = $"-hide_banner ";
            args += VideoOptions.CreateHWDecoderArgument();

            foreach (var file in FileNameList)
            {
                args += $"-i \"{file}\" ";
            }

            var firstFile = FileNameList[0];
            if (UseVideoCodec)
            {
                args += VideoOptions.CreateArguments(firstFile, VideoPass.OnePass, true);
            }
            args += AudioOptions.CreateArguments(firstFile, true);
            args += $" -filter_complex \"concat=n={FileNameList.Count}:v=1:a=1\" ";
            args += AdditionalOptions;
            args += $"{ToFileName(firstFile)}";

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

            ArgumentsHistory.Append(info.Arguments + "\n");

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
        /// <param name="pass">ビデオのパス指定</param>
        /// <returns>引数</returns>
        protected string CreateArguments(string file, VideoPass pass = VideoPass.OnePass)
        {
            var args = "-hide_banner ";
            args += VideoOptions.CreateHWDecoderArgument();
            args += $"-i \"{file}\" ";
            if (UseVideoCodec)
            {
                args += VideoOptions.CreateArguments(file, pass);
            }
            if (pass == VideoPass.TwoPass1st)
            {
                args += $"-an ";
                args += AdditionalOptions;
                args += $"-f null -";
            }
            else
            {
                args += AudioOptions.CreateArguments(file);
                args += AdditionalOptions;
                args += $"{ToFileName(file)}";
            }
            return args;
        }

        /// <summary>
        /// 実際のファイル変換
        /// </summary>
        /// <param name="file">変換元のファイル名</param>
        /// <param name="info">プロセス情報</param>
        /// <returns></returns>
        protected async Task<Code> Convert(string file, ProcessStartInfo info)
        {
            Code result = Code.OK;

            ArgumentsHistory.Append(info.Arguments + "\n");

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
        /// 1Passのファイル変換
        /// </summary>
        /// <param name="file">変換元のファイル名</param>
        /// <returns></returns>
        protected async Task<Code> Convert1Pass(string file)
        {
            ProcessStartInfo info = new ProcessStartInfo();
            info.FileName = "ffmpeg";
            info.Arguments = CreateArguments(file);
            info.UseShellExecute = false;

            Code result = await Convert(file, info);

            return result;
        }

        /// <summary>
        /// 2Passのファイル変換
        /// </summary>
        /// <param name="file">変換元のファイル名</param>
        /// <returns></returns>
        protected async Task<Code> Convert2Pass(string file)
        {
            Code result = Code.OK;

            // 1stPass
            ProcessStartInfo info = new ProcessStartInfo();
            info.FileName = "ffmpeg";
            info.Arguments = CreateArguments(file, VideoPass.TwoPass1st);
            info.UseShellExecute = false;

            result = await Convert(file, info);

            // 2ndPass
            if (result == Code.OK)
            {
                info = new ProcessStartInfo();
                info.FileName = "ffmpeg";
                info.Arguments = CreateArguments(file, VideoPass.TwoPass2nd);
                info.UseShellExecute = false;

                result = await Convert(file, info);
            }

            return result;
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

            if (VideoOptions.TwoPass)
            {
                result = await Convert2Pass(file);
            }
            else
            {
                result = await Convert1Pass(file);
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
                var file = FileNameList[index];
                FileInfo fileInfo = new FileInfo(file);
                info += $"  ファイル: {file}\n";
                info += "  コンテナ情報\n";
                info += $"    フォーマット: {fileInfo.Container}\n";
                info += $"    再生時間: {fileInfo.Duration} 秒\n";
                if (fileInfo.AudioCodec != "")
                {
                    info += "  オーディオ情報:\n";
                    info += $"    オーディオコーデック: {fileInfo.AudioCodec}\n";
                    info += $"    オーディオサンプリングレート: {fileInfo.AudioSamplingRate} Hz\n";
                    info += $"    オーディオチャンネル: {fileInfo.AudioChannel}\n";
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
