using MaterialDesignThemes.Wpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using static EasyFFmpeg.FileList;
using static Microsoft.WindowsAPICodePack.Shell.PropertySystem.SystemProperties.System;

namespace EasyFFmpeg
{
    /// <summary>
    /// FileConversionProgress.xaml の相互作用ロジック
    /// </summary>
    public partial class FileConversionProgress : UserControl
    {
        public FileConversionProgress()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 表示するファイル名とプログレスバーが表示する値を設定
        /// </summary>
        /// <remarks>
        /// 他スレッドから使用できるようにデリゲートを使用しディスパッチャーを通してアップデート
        /// </remarks>
        /// <param name="fileName">表示するファイル名</param>
        /// <param name="value">プログレスバーで表示する値</param>
        public void SetFileNameProgress(string fileName, int value)
        {
            this.Dispatcher.Invoke(new Action(() =>
            {
                FileNameLabel.Content = fileName;
                ConversionProgress.Value = value;
                CircleProgress.Value = value;
            }));
        }
    }
}
