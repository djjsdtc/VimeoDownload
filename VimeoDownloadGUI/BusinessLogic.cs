namespace VimeoDownload.GUI
{
    using System.Configuration;
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
                Proxy = GetProxy(appSettings),
                NoMerge = bool.TryParse(appSettings.Get(SettingsConst.MergeOutput), out var b) && !b,
                OutputPath = appSettings.Get(SettingsConst.OutputPath),
                ThreadNumber = int.TryParse(appSettings.Get(SettingsConst.ThreadNumber), out i) ? i : 4,
                Timeout = int.TryParse(appSettings.Get(SettingsConst.Timeout), out i) ? i : 60
            };
            return options;
        }

        public static void SaveOptions(CommandLineOption option)
        {

        }

        private static string GetValueOrDefault(string originalValue, string defaultValue, params string[] otherValues)
            => originalValue == defaultValue || otherValues.Contains(originalValue) ? originalValue : defaultValue;

        private static string GetProxy(string proxy)
        {
            throw new System.NotImplementedException();
        }
    }
}
