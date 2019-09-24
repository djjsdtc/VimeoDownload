namespace VimeoDownload.GUI
{
    using System;
    using System.Collections.Generic;
    using System.IO;
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
    using VimeoDownload.DataContract;
    using FolderBrowserDialog = System.Windows.Forms.FolderBrowserDialog;
    using Path = System.IO.Path;

    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private string currentUrl = string.Empty;

        /// <summary>
        /// 构造函数。
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
            Console.SetOut(new ConsoleWriter(TxtOutput, Dispatcher));
            Console.WriteLine(BusinessLogic.GetVersionInfo());
            LoadConfiguration();
        }

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

        private void UnlockProperties()
        {
            foreach (var grid in new[] { this.GrdMain, this.GrdFormats, this.GrdProxy })
            {
                foreach (var uiElement in grid.Children)
                {
                    if (uiElement == BtnDownload || uiElement == OptAudioFormat || uiElement == OptVideoFormat)
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

        private void TxtSource_TxtOutputFile_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (BtnFetch != null && BtnDownload != null && TxtSource != null && TxtOutputFile != null &&
                ChkMerge != null)
            {
                BtnFetch.IsEnabled = !string.IsNullOrWhiteSpace(TxtSource.Text);
                BtnDownload.IsEnabled = IsBtnDownloadEnabled();
            }
        }

        private bool IsBtnDownloadEnabled()
        {
            return !string.IsNullOrWhiteSpace(this.currentUrl) &&
                   this.currentUrl == TxtSource.Text &&
                   (!ChkMerge.IsChecked.Value || !string.IsNullOrWhiteSpace(TxtOutputFile.Text));
        }

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
