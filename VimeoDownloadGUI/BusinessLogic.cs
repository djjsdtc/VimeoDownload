namespace VimeoDownload.GUI
{
    using System.Configuration;
    using System.IO;
    using System.Linq;

    /// <summary>
    /// GUI 相关的业务逻辑。
    /// </summary>
    public static class BusinessLogic
    {
        public static CommandLineOption GetSavedOptions()
        {
            var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            var appSettings = config.AppSettings;
            var options = new CommandLineOption
            {
                MaxRetry = int.TryParse(appSettings.Get(SettingsConst.RetryTime), out var i) ? i : 3,
                MergerName = GetValueOrDefault(appSettings.Get(SettingsConst.Merger), SettingsConst.MergerFFMpeg, SettingsConst.MergerMkvMerge),
                Proxy = GetProxy(appSettings.Get(SettingsConst.Proxy)),
                NoMerge = bool.TryParse(appSettings.Get(SettingsConst.MergeOutput), out var b) && !b,
                OutputPath = appSettings.Get(SettingsConst.OutputPath) ?? Directory.GetCurrentDirectory(),
                ThreadNumber = int.TryParse(appSettings.Get(SettingsConst.ThreadNumber), out i) ? i : 4,
                Timeout = int.TryParse(appSettings.Get(SettingsConst.Timeout), out i) ? i : 60
            };
            return options;
        }

        public static void SaveOptions(CommandLineOption option)
        {
            var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            var appSettings = config.AppSettings;
            appSettings.AddOrUpdate(SettingsConst.RetryTime, option.MaxRetry.ToString());
            appSettings.AddOrUpdate(SettingsConst.MergeOutput, option.MergerName);
            appSettings.AddOrUpdate(SettingsConst.Proxy, option.Proxy);
            appSettings.AddOrUpdate(SettingsConst.MergeOutput, (!option.NoMerge).ToString());
            appSettings.AddOrUpdate(SettingsConst.OutputPath, option.OutputPath.ToString());
            appSettings.AddOrUpdate(SettingsConst.ThreadNumber, option.ThreadNumber.ToString());
            appSettings.AddOrUpdate(SettingsConst.Timeout, option.Timeout.ToString());
            config.Save(ConfigurationSaveMode.Full);
            ConfigurationManager.RefreshSection("appSettings");
        }

        private static string GetValueOrDefault(string originalValue, string defaultValue, params string[] otherValues)
            => originalValue == defaultValue || otherValues.Contains(originalValue) ? originalValue : defaultValue;

        private static string GetProxy(string proxy)
        {
            try
            {
                ProgramEntrance.GetProxyHandler(proxy);
                return proxy;
            }
            catch
            {
                return SettingsConst.ProxyTypeSystem;
            }
        }
    }
}
