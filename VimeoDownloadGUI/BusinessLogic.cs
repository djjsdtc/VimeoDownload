namespace VimeoDownload.GUI
{
    using System;
    using System.Configuration;
    using System.IO;
    using System.Linq;
    using System.Net.Http;
    using VimeoDownload.DataContract;
    using VimeoDownload.Web;

    /// <summary>
    /// GUI 相关的业务逻辑。
    /// </summary>
    public static class BusinessLogic
    {
        public static CommandLineOption GetSavedOptions()
        {
            var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            var appSettings = config.AppSettings;
            var outputPath = appSettings.Get(SettingsConst.OutputPath);
            var proxy = appSettings.Get(SettingsConst.Proxy);
            var options = new CommandLineOption
            {
                MaxRetry = int.TryParse(appSettings.Get(SettingsConst.RetryTime), out var i) ? i : 3,
                MergerName = GetValueOrDefault(appSettings.Get(SettingsConst.Merger), SettingsConst.MergerFFMpeg, SettingsConst.MergerMkvMerge),
                Proxy = IsValidProxy(proxy) ? proxy : SettingsConst.ProxyTypeSystem,
                NoMerge = bool.TryParse(appSettings.Get(SettingsConst.MergeOutput), out var b) && !b,
                OutputPath = string.IsNullOrWhiteSpace(outputPath) ? Directory.GetCurrentDirectory() : outputPath,
                ThreadNumber = int.TryParse(appSettings.Get(SettingsConst.ThreadNumber), out i) ? i : 4,
                Timeout = int.TryParse(appSettings.Get(SettingsConst.Timeout), out i) ? i : 60,
                Download = true
            };
            return options;
        }

        public static string GetConfigurationFilePath()
            => ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None).FilePath;

        public static void SaveOptions(CommandLineOption option)
        {
            var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            var appSettings = config.AppSettings;
            appSettings.AddOrUpdate(SettingsConst.RetryTime, option.MaxRetry.ToString());
            appSettings.AddOrUpdate(SettingsConst.MergeOutput, option.MergerName);
            appSettings.AddOrUpdate(SettingsConst.Proxy, option.Proxy);
            appSettings.AddOrUpdate(SettingsConst.MergeOutput, (!option.NoMerge).ToString());
            appSettings.AddOrUpdate(SettingsConst.OutputPath, option.OutputPath);
            appSettings.AddOrUpdate(SettingsConst.ThreadNumber, option.ThreadNumber.ToString());
            appSettings.AddOrUpdate(SettingsConst.Timeout, option.Timeout.ToString());
            config.Save(ConfigurationSaveMode.Modified);
            ConfigurationManager.RefreshSection("appSettings");
        }

        private static string GetValueOrDefault(string originalValue, string defaultValue, params string[] otherValues)
            => originalValue == defaultValue || otherValues.Contains(originalValue) ? originalValue : defaultValue;

        public static bool IsValidProxy(string proxy)
        {
            try
            {
                ProgramEntrance.GetProxyHandler(proxy);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static VimeoVideo GetVideoInfo(string url, string proxy, int timeout, int maxRetry)
        {
            var proxyHandler = ProgramEntrance.GetProxyHandler(proxy);
            using (var httpClient = new HttpClient(proxyHandler) { Timeout = TimeSpan.FromSeconds(timeout) })
            {
                return WebUtility.GetVideoInfo(httpClient, url, maxRetry).Result;
            }
        }
    }
}
