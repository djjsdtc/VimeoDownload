namespace VimeoDownload.GUI
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Controls;
    using VimeoDownload.DataContract;
    using FolderBrowserDialog = System.Windows.Forms.FolderBrowserDialog;

    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        /// 当前正在进行下载的 URL。
        /// </summary>
        private string currentUrl = string.Empty;

        /// <summary>
        /// 构造函数。
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
            var writer = new ConsoleWriter(TxtOutput, Dispatcher);
            Console.SetOut(writer);
            Console.SetError(writer);
            Console.WriteLine(BusinessLogic.GetVersionInfo());
            LoadConfiguration();
        }

        /// <summary>
        /// 从 app.config 文件中加载配置。
        /// </summary>
        private void LoadConfiguration()
        {
            var config = BusinessLogic.GetSavedOptions();
            TxtMaxRetry.Text = config.MaxRetry.ToString();
            TxtTimeout.Text = config.Timeout.ToString();
            TxtThreadNum.Text = config.ThreadNumber.ToString();
            TxtWorkingDir.Text = config.OutputPath;
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

        /// <summary>
        /// 代理设置选择“none”或“system”时禁用“http”和“socks”的输入框。
        /// </summary>
        private void RtnSystemProxy_RtnNoProxy_Checked(object sender, RoutedEventArgs e)
        {
            TxtSocksHost.IsEnabled = false;
            TxtSocksPort.IsEnabled = false;
            TxtHttpHost.IsEnabled = false;
            TxtHttpPort.IsEnabled = false;
        }

        /// <summary>
        /// 代理设置选择“http”时禁用“socks”的输入框。
        /// </summary>
        private void RtnHttpProxy_Checked(object sender, RoutedEventArgs e)
        {
            TxtSocksHost.IsEnabled = false;
            TxtSocksPort.IsEnabled = false;
            TxtHttpHost.IsEnabled = true;
            TxtHttpPort.IsEnabled = true;
        }

        /// <summary>
        /// 代理设置选择“socks”时禁用“http”的输入框。
        /// </summary>
        private void RtnSocksProxy_Checked(object sender, RoutedEventArgs e)
        {
            TxtSocksHost.IsEnabled = true;
            TxtSocksPort.IsEnabled = true;
            TxtHttpHost.IsEnabled = false;
            TxtHttpPort.IsEnabled = false;
        }

        /// <summary>
        /// 勾选“merge”时允许指定 merger 和 output file。
        /// </summary>
        private void ChkMerge_Checked(object sender, RoutedEventArgs e)
        {
            OptMergerName.IsEnabled = true;
            TxtOutputFile.IsEnabled = true;
            BtnDownload.IsEnabled = IsBtnDownloadEnabled();
        }

        /// <summary>
        /// 取消勾选“merge”时禁止指定 merger 和 output file。
        /// </summary>
        private void ChkMerge_Unchecked(object sender, RoutedEventArgs e)
        {
            OptMergerName.IsEnabled = false;
            TxtOutputFile.IsEnabled = false;
            BtnDownload.IsEnabled = IsBtnDownloadEnabled();
        }

        /// <summary>
        /// 数字输入框禁止输入小于 1 的数字和非数字。（retry time 禁止小于 0）
        /// </summary>
        private void NumberTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var textBox = sender as TextBox;
            var change = e.Changes.First();
            int offset = change.Offset;
            if (change.AddedLength > 0)
            {
                if (!int.TryParse(textBox.Text, out var i) ||
                    (textBox == TxtMaxRetry && i < 0) ||
                    (textBox != TxtMaxRetry && i < 1))
                {
                    textBox.Text = textBox.Text.Remove(offset, change.AddedLength);
                    textBox.Select(offset, 0);
                }
            }
        }

        /// <summary>
        /// 单击“save settings”按钮时保存设置到 app.config。
        /// </summary>
        /// <seealso cref="BusinessLogic.SaveOptions" />
        private void BtnSaveSettings_Click(object sender, RoutedEventArgs e)
        {
            var option = GetCurrentOption();

            try
            {
                BusinessLogic.SaveOptions(option);
                MessageBox.Show(
                    $"Settings saved into {BusinessLogic.GetConfigurationFilePath()}",
                    "Save settings",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
            catch
            {
                MessageBox.Show(
                    $"Cannot save your settings.\nPlease make sure you have the permission to change the file {BusinessLogic.GetConfigurationFilePath()}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// 获取当前用户界面上的设置。
        /// </summary>
        /// <returns>当前设置。</returns>
        private CommandLineOption GetCurrentOption()
        {
            var option = new CommandLineOption
            {
                MaxRetry = int.TryParse(TxtMaxRetry.Text, out var i) ? i : 3,
                MergerName = OptMergerName.Text,
                Proxy = GetProxy(),
                NoMerge = !ChkMerge.IsChecked.Value,
                OutputPath = TxtWorkingDir.Text,
                ThreadNumber = int.TryParse(TxtThreadNum.Text, out i) ? i : 4,
                Timeout = int.TryParse(TxtTimeout.Text, out i) ? i : 60,
                Download = true,
                NotOverrideOutput = false,
                OverrideOutput = false
            };
            return option;
        }

        /// <summary>
        /// 获取当前用户界面上的代理设置。
        /// </summary>
        /// <returns>当前代理设置。</returns>
        private string GetProxy()
        {
            var proxy = SettingsConst.ProxyTypeSystem;
            if (RtnNoProxy.IsChecked.Value)
            {
                proxy = SettingsConst.ProxyTypeNone;
            }
            else if (RtnHttpProxy.IsChecked.Value)
            {
                proxy = $"{SettingsConst.ProxyTypeHttp}://{TxtHttpHost.Text}:{TxtHttpPort.Text}";
            }
            else if (RtnSocksProxy.IsChecked.Value)
            {
                proxy = $"{SettingsConst.ProxyTypeSocks}://{TxtSocksHost.Text}:{TxtSocksPort.Text}";
            }

            return proxy;
        }

        /// <summary>
        /// 单击“browse”按钮时弹出选择文件夹对话框选择保存文件的路径。
        /// </summary>
        private void BtnBrowseDir_Click(object sender, RoutedEventArgs e)
        {
            using (var dialog = new FolderBrowserDialog())
            {
                dialog.Description = "Choose the output path.";
                var result = dialog.ShowDialog();

                if (result == System.Windows.Forms.DialogResult.Cancel)
                {
                    return;
                }

                var workingDir = dialog.SelectedPath.Trim();
                TxtWorkingDir.Text = workingDir;
            }
        }

        /// <summary>
        /// 单击“fetch”按钮时获取视频信息并在音视频格式选择框中填入对应项。
        /// </summary>
        private void BtnFetch_Click(object sender, RoutedEventArgs e)
        {
            if (LockAndCheckProperties())
            {
                var source = TxtSource.Text;
                var proxy = GetProxy();
                var timeout = int.Parse(TxtTimeout.Text);
                var retry = int.Parse(TxtMaxRetry.Text);
                Task.Run(() =>
                {
                    var message = string.Empty;
                    VimeoVideo vimeoVideo = null;
                    try
                    {
                        vimeoVideo = BusinessLogic.GetVideoInfo(source, proxy, timeout, retry);
                    }
                    catch (Exception ex)
                    {
                        message = (ex is AggregateException ? ex.InnerException : ex)?.Message;
                    }

                    this.Dispatcher.Invoke(() =>
                    {
                        if (vimeoVideo != null)
                        {
                            this.currentUrl = TxtSource.Text;
                        }

                        UnlockProperties();
                        if (vimeoVideo == null)
                        {
                            MessageBox.Show(message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                            OptVideoFormat.Items.Clear();
                            OptAudioFormat.Items.Clear();
                        }
                        else
                        {
                            foreach (var mediaClip in vimeoVideo.Video)
                            {
                                OptVideoFormat.Items.Add(mediaClip);
                            }
                            foreach (var mediaClip in vimeoVideo.Audio)
                            {
                                OptAudioFormat.Items.Add(mediaClip);
                            }

                            OptVideoFormat.SelectedIndex = 0;
                            OptAudioFormat.SelectedIndex = 0;
                        }
                    });
                });
            }
        }

        /// <summary>
        /// 单击“fetch”或“download”按钮时检查设置是否正确，如正确则锁定所有输入项，准备进行网络操作。
        /// </summary>
        /// <returns>如设置正确且所有输入项已被锁定，返回 <see langword="true" />。</returns>
        private bool LockAndCheckProperties()
        {
            var message = string.Empty;
            if (string.IsNullOrEmpty(TxtTimeout.Text))
            {
                message = "Invalid timeout setting.";
            }

            if (string.IsNullOrEmpty(TxtThreadNum.Text))
            {
                message = "Invalid thread number setting.";
            }

            if (string.IsNullOrEmpty(TxtMaxRetry.Text))
            {
                message = "Invalid retry time setting.";
            }

            if (!BusinessLogic.IsValidProxy(GetProxy()))
            {
                message = "Invalid proxy setting.";
            }

            if (!string.IsNullOrEmpty(message))
            {
                MessageBox.Show(message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            foreach (var grid in new[] { this.GrdMain, this.GrdFormats, this.GrdProxy })
            {
                foreach (var uiElement in grid.Children)
                {
                    if ((uiElement is TextBox t && !t.IsReadOnly) ||
                        (uiElement is Button && uiElement != BtnSaveSettings) ||
                        uiElement is ComboBox || uiElement is RadioButton || uiElement is CheckBox)
                    {
                        (uiElement as Control).IsEnabled = false;
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// 网络操作结束后解锁所有输入项。
        /// </summary>
        private void UnlockProperties()
        {
            foreach (var grid in new[] { this.GrdMain, this.GrdFormats, this.GrdProxy })
            {
                foreach (var uiElement in grid.Children)
                {
                    if (uiElement == BtnDownload)
                    {
                        (uiElement as Control).IsEnabled = IsBtnDownloadEnabled();
                        continue;
                    }

                    if (uiElement == OptMergerName || uiElement == TxtOutputFile)
                    {
                        (uiElement as Control).IsEnabled = ChkMerge.IsChecked.Value;
                        continue;
                    }

                    if (uiElement == TxtHttpHost || uiElement == TxtHttpPort)
                    {
                        (uiElement as Control).IsEnabled = RtnHttpProxy.IsChecked.Value;
                        continue;
                    }

                    if (uiElement == TxtSocksHost || uiElement == TxtSocksPort)
                    {
                        (uiElement as Control).IsEnabled = RtnSocksProxy.IsChecked.Value;
                        continue;
                    }

                    if (uiElement is TextBox || uiElement is Button || uiElement is ComboBox ||
                        uiElement is RadioButton || uiElement is CheckBox)
                    {
                        (uiElement as Control).IsEnabled = true;
                    }
                }
            }
        }

        /// <summary>
        /// “source”文本框和“output path”文本框内容改变时判断是否允许下载。
        /// </summary>
        private void TxtSource_TxtOutputFile_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (BtnFetch != null && BtnDownload != null && TxtSource != null && TxtOutputFile != null &&
                ChkMerge != null)
            {
                BtnFetch.IsEnabled = !string.IsNullOrWhiteSpace(TxtSource.Text);
                BtnDownload.IsEnabled = IsBtnDownloadEnabled();
            }
        }

        /// <summary>
        /// 判断是否允许下载。
        /// </summary>
        /// <returns>如果允许下载，返回 <see langword="true" />。</returns>
        private bool IsBtnDownloadEnabled()
        {
            /* 允许下载的条件：
             * 1. source 文本框非空
             * 2. source 文本框和当前地址一致（即已经点过 fetch 按钮）
             * 3. 选择不进行 merge
             * 4. 选择进行 merge 且 output file name 不为空
             */
            return !string.IsNullOrWhiteSpace(this.currentUrl) &&
                   this.currentUrl == TxtSource.Text &&
                   (!ChkMerge.IsChecked.Value || !string.IsNullOrWhiteSpace(TxtOutputFile.Text));
        }

        /// <summary>
        /// 单击“download”按钮时开始下载过程。
        /// </summary>
        private void BtnDownload_Click(object sender, RoutedEventArgs e)
        {
            if (LockAndCheckProperties())
            {
                Directory.SetCurrentDirectory(TxtWorkingDir.Text);
                var option = GetCurrentOption();
                option.DownloadAddress = this.currentUrl;
                option.OutputFileName = TxtOutputFile.Text;
                option.VideoFormatId = (OptVideoFormat.SelectedItem as VideoClip).Id;
                option.AudioFormatId = (OptAudioFormat.SelectedItem as AudioClip).Id;

                Task.Run(() =>
                {
                    var message = string.Empty;
                    try
                    {
                        BusinessLogic.DownloadVideo(option, GUIOverridePromotion);
                    }
                    catch (Exception ex)
                    {
                        message = (ex is AggregateException ? ex.InnerException : ex)?.Message;
                    }

                    this.Dispatcher.Invoke(() =>
                    {
                        if (!string.IsNullOrEmpty(message))
                        {
                            MessageBox.Show(message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                        UnlockProperties();
                    });
                });
            }
        }

        /// <summary>
        /// 弹出对话框询问是否覆盖文件，单击“Yes”表示覆盖，单击“No”表示不覆盖。
        /// </summary>
        /// <param name="fileName">要覆盖的文件名。</param>
        /// <returns>如返回 <see langword="true"/> 则表示覆盖。</returns>
        private bool GUIOverridePromotion(string fileName)
        {
            var result = MessageBox.Show($"File {Path.GetFileName(fileName)} already exists. Override?",
                "File exists", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                Console.WriteLine("The file will be overwritten.");
                return true;
            }

            Console.WriteLine("The file will not be overwritten.");
            return false;
        }
    }
}
