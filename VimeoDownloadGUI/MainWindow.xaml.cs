namespace VimeoDownload.GUI
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Data;
    using System.Windows.Documents;
    using System.Windows.Input;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;
    using System.Windows.Navigation;
    using System.Windows.Shapes;

    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        /// 构造函数。
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
            LoadConfiguration();
        }

        private void LoadConfiguration()
        {
            var config = BusinessLogic.GetSavedOptions();
            TxtMaxRetry.Text = config.MaxRetry.ToString();
            TxtTimeout.Text = config.Timeout.ToString();
            TxtThreadNum.Text = config.ThreadNumber.ToString();
            TxtWorkingDir.Text = config.OutputPath.ToString();
            ChkMerge.IsChecked = !config.NoMerge;
            switch (config.MergerName)
            {
                case SettingsConst.MergerFFMpeg:
                    OptMergerName.SelectedIndex = 0;
                    break;
                case SettingsConst.MergerMkvMerge:
                    OptMergerName.SelectedIndex = 1;
                    break;
            }
            switch (config.Proxy)
            {
                case SettingsConst.ProxyTypeNone:
                    RtnNoProxy.IsChecked = true;
                    break;
                case SettingsConst.ProxyTypeSystem:
                    RtnSystemProxy.IsChecked = true;
                    break;
                default:
                    var uri = new Uri(config.Proxy);
                    switch (uri.Scheme)
                    {
                        case SettingsConst.ProxyTypeHttp:
                            RtnHttpProxy.IsChecked = true;
                            TxtHttpHost.Text = uri.Host;
                            TxtHttpPort.Text = uri.Port.ToString();
                            TxtSocksHost.Text = string.Empty;
                            TxtSocksHost.Text = string.Empty;
                            break;
                        case SettingsConst.ProxyTypeSocks:
                            RtnSocksProxy.IsChecked = true;
                            TxtSocksHost.Text = uri.Host;
                            TxtSocksHost.Text = uri.Port.ToString();
                            TxtHttpHost.Text = string.Empty;
                            TxtHttpHost.Text = string.Empty;
                            break;
                    }
                    break;
            }
        }

        private void RtnSystemProxy_RtnNoProxy_Checked(object sender, RoutedEventArgs e)
        {
            TxtSocksHost.IsEnabled = false;
            TxtSocksPort.IsEnabled = false;
            TxtHttpHost.IsEnabled = false;
            TxtHttpPort.IsEnabled = false;
        }

        private void RtnHttpProxy_Checked(object sender, RoutedEventArgs e)
        {
            TxtSocksHost.IsEnabled = false;
            TxtSocksPort.IsEnabled = false;
            TxtHttpHost.IsEnabled = true;
            TxtHttpPort.IsEnabled = true;
        }

        private void RtnSocksProxy_Checked(object sender, RoutedEventArgs e)
        {
            TxtSocksHost.IsEnabled = true;
            TxtSocksPort.IsEnabled = true;
            TxtHttpHost.IsEnabled = false;
            TxtHttpPort.IsEnabled = false;
        }

        private void ChkMerge_Checked(object sender, RoutedEventArgs e)
        {
            OptMergerName.IsEnabled = true;
            TxtOutputFile.IsEnabled = true;
        }

        private void ChkMerge_Unchecked(object sender, RoutedEventArgs e)
        {
            OptMergerName.IsEnabled = false;
            TxtOutputFile.IsEnabled = false;
        }
    }
}
