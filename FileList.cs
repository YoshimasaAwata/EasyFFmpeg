using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

        /// <value>フィルタリング用拡張子を複数指定する際の区切り文字</value>
        private static readonly char[] Delimiter = { ',', ' ', '.', ';', ':' };

        /// <value>変換元および変換先のファイル名のリスト</value>
        public ObservableCollection<FileNames> FileNameList { get; } = new ObservableCollection<FileNames>();
        /// <value>エラー等のメッセージ</value>
        public string Message { get; private set; } = "";
        /// <value>ファイルの変換先フォルダ名</value>
        public string TargetDir { get; set; } = "";
        /// <value>ファイルの変換元フォルダ名</value>
        public string SourceDir { get; private set; } = "";
        private string baseFileName = "new-file-name";
        /// <value>変換先ファイル名の共通部分</value>
        public string BaseFileName 
        { 
            get { return baseFileName; } 
            set 
            { 
                baseFileName = value;
                this.MakeToFilesList();     // 変更の度にファイル名リストを更新
            } 
        }
        /// <value>フィルタリング用拡張子のリスト</value>
        private List<string>? extensions = null;
        /// <value>ファイル変換キャンセル用</value>
        private CancellationTokenSource? tokenSource = null;
        /// <value>ロック用オブジェクト</value>
        private readonly object balanceLock = new object();

        /// <summary>
        /// 指定のファイル名を"FileNameList"に追加</br>
        /// 指定により隠しファイルやシステムファイルは除外する
        /// </summary>
        /// <param name="file">追加するファイル名</param>
        /// <param name="exclude">隠しファイルやシステムファイルを除外するかどうか</param>
        protected void AddFromFile(string file, bool? exclude)
        {
            if (exclude == false)
            {
                FileNameList.Add(new FileNames() { FromFile = file });
            }
            else    // exclude == null or true
            {
                var attr = File.GetAttributes(file);
                if ((attr & (FileAttributes.Hidden | FileAttributes.System)) == 0)
                {
                    FileNameList.Add(new FileNames() { FromFile = file });
                }
            }
        }

        /// <summary>
        /// ファイルの変換元フォルダをセット</br>
        /// 同時に変換するファイル名をリストに登録
        /// </summary>
        /// <param name="dir">ファイルの変換元フォルダ名</param>
        /// <param name="exclude">隠しファイルやシステムファイルを除外するかどうか</param>
        /// <returns>処理結果</returns>
        public Code SetSourceDir(string dir, bool? exclude)
        {
            Code result = Code.OK;

            try
            {
                var files = Directory.GetFiles(dir);
                SourceDir = dir;
                FileNameList.Clear();

                if ((extensions != null) && (extensions.Count > 0))
                {
                    foreach (var file in files)
                    {
                        var ext = Path.GetExtension(file).Substring(1); // 最初の'.'を除く
                        if (extensions.Contains(ext))
                        {
                            AddFromFile(file, exclude);
                        }
                    }
                }
                else
                {
                    foreach (var file in files)
                    {
                        AddFromFile(file, exclude);
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
        /// "FileNameList"内にセットした変換元ファイル名と"baseFileName"を元に連番を付加して変換先ファイル名を作成
        /// </summary>
        protected void MakeToFilesList()
        {
            int num = 0;
            foreach (var file in FileNameList)
            {
                var ext = Path.GetExtension(file.FromFile);
                var newFileName = $"{baseFileName}{num++:d3}{ext}";
                file.ToFile = newFileName;
            }
        }

        /// <summary>
        /// "FileNameList"にリストアップされた変換元ファイル名と変換先ファイル名、変換先フォルダ名を使いファイルを変換
        /// </summary>
        /// <param name="progress">プログレスバーダイアログ</param>
        /// <returns>処理結果</returns>
        public async Task<Code> CopyFiles(FileConversionProgress progress)
        {
            Code result = Code.OK;
            int fileCount = 0;

            // 変換先フォルダの指定がない場合は変換元フォルダに変換
            var dir = (TargetDir == "") ? SourceDir : TargetDir;

            lock (balanceLock)
            {
                tokenSource = new CancellationTokenSource();
            }

            foreach (var file in FileNameList)
            {
                var targetFileName = Path.Join(dir, file.ToFile);

                progress.SetFileNameProgress(Path.GetFileName(file.FromFile), (++fileCount * 100 / FileNameList.Count));

                try
                {
                    using (var source = File.OpenRead(file.FromFile))
                    {
                        using (var target = File.Open(targetFileName, FileMode.CreateNew))
                        {
                            await source.CopyToAsync(target, tokenSource.Token).ConfigureAwait(false);
                        }
                    }
                }
                catch (IOException)
                {
                    // ファイルが既に存在する等のエラーはそのまま続行する
                    continue;
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
        /// 変換元ファイルのフィルタリング用拡張子のリストを作成</br>
        /// 拡張子は区切り文字',', ' ', '.', ';', ':'を用いて複数指定できる
        /// </summary>
        /// <param name="ext">拡張子を記述した文字列</param>
        public void SetExtensions(string ext)
        {
            var extList = ext.Split(Delimiter, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            extensions = (extList.Length > 0) ? new List<string>(extList) : null;
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
