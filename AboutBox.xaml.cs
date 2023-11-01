using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace EasyFFmpeg
{
    /// <summary>
    /// AboutBox.xaml の相互作用ロジック
    /// </summary>
    public partial class AboutBox : Window
    {
        /// <summary>
        /// 使用しているNuGetパッケージ
        /// </summary>
        protected class Package
        {
            /// <value>パッケージ名</value>
            public string Name { get; set; } = "";
            /// <value>パッケージのライセンス</value>
            public string License { get; set; } = "";
            /// <value>パッケージのライセンスのURL</value>
            public string Url { get; set; } = "";
        }

        /// <value>使用しているNuGetパッケージのリスト</value>
        private static readonly ReadOnlyObservableCollection<Package> s_packages = new ReadOnlyObservableCollection<Package>(
            new ObservableCollection<Package>() {
            new Package {Name = "WindowsAPICodePack-Core", License = "Custom", Url = "https://github.com/aybe/Windows-API-Code-Pack-1.1/blob/master/LICENCE"},
            new Package {Name = "WindowsAPICodePack-Shell", License = "Custom", Url = "https://github.com/aybe/Windows-API-Code-Pack-1.1/blob/master/LICENCE"},
            new Package {Name = "MahApps.Metro", License = "MIT", Url = "https://github.com/MahApps/MahApps.Metro/blob/develop/LICENSE"},
            new Package {Name = "MahApps.Metro.IconPacks", License = "MIT", Url = "https://github.com/MahApps/MahApps.Metro.IconPacks/blob/develop/LICENSE"},
            new Package {Name = "ControlzEx", License = "MIT", Url = "https://github.com/ControlzEx/ControlzEx/blob/develop/LICENSE"},
            new Package {Name = "Microsoft.Xaml.Behaviors.Wpf", License = "MIT", Url = "https://github.com/microsoft/XamlBehaviorsWpf/blob/master/LICENSE"},
            new Package {Name = "System.Text.Json", License = "MIT", Url = "https://www.nuget.org/packages/System.Text.Json/4.7.2/license"},
            new Package {Name = "MaterialDesignThemes", License = "MIT", Url = "https://github.com/MaterialDesignInXAML/MaterialDesignInXamlToolkit/blob/master/LICENSE"},
            new Package {Name = "MaterialDesignColors", License = "MIT", Url = "https://github.com/MaterialDesignInXAML/MaterialDesignInXamlToolkit/blob/master/LICENSE"},
            new Package {Name = "MaterialDesignThemes.MahApps", License = "MIT", Url = "https://github.com/MaterialDesignInXAML/MaterialDesignInXamlToolkit/blob/master/LICENSE"},
        });

        public AboutBox()
        {
            InitializeComponent();

            PackageList.ItemsSource = s_packages;
            PackageList.SelectedIndex = 0;

            this.WindowStartupLocation = WindowStartupLocation.CenterScreen;
        }

        /// <summary>
        /// ウィンドウをドラッグして移動
        /// </summary>
        /// <remarks>
        /// マウスボタンがリリースされた後にコールされる場合があるのでマウスボタンが押されている事をチェック
        /// </remarks>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed)
            {
                this.DragMove();
            }
        }

        /// <summary>
        /// パッケージ表示のListViewで選択したら警告表示をクリア
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PackageList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            AlartTextBox.Text = "";
        }

        /// <summary>
        /// About表示時にパッケージ表示のListViewのパッケージ名の列幅を計算
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            PackageColumn.Width = PackageList.ActualWidth - LicenseColumn.ActualWidth - SystemParameters.VerticalScrollBarWidth;
        }

        /// <summary>
        /// ライセンス表示ボタンをクリックしたらパッケージ表示のListViewで選択したパッケージのライセンスURLをブラウザで表示
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LicenseButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var startInfo = new System.Diagnostics.ProcessStartInfo(s_packages[PackageList.SelectedIndex].Url);
                startInfo.UseShellExecute = true;
                System.Diagnostics.Process.Start(startInfo);
                AlartTextBox.Text = "";
            }
            catch (System.ComponentModel.Win32Exception noBrowser)
            {
                if (noBrowser.ErrorCode == -2147467259)
                {
                    AlartTextBox.Text = noBrowser.Message;
                }
            }
            catch (System.Exception other)
            {
                AlartTextBox.Text = other.Message;
            }
        }

        /// <summary>
        /// AboutBoxをクローズ
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
