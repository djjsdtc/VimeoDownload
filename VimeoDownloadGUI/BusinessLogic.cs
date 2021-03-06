﻿namespace VimeoDownload.GUI
{
    using System;
    using System.Configuration;
    using System.IO;
    using System.Linq;
    using System.Net.Http;
    using System.Reflection;
    using VimeoDownload.DataContract;
    using VimeoDownload.Web;

    /// <summary>
    /// GUI 相关的业务逻辑。
    /// </summary>
    public static class BusinessLogic
    {
        /// <summary>
        /// 从 app.config 文件中读取保存的设置参数。如果某些参数未设置或设置有误则返回默认值。
        /// </summary>
        /// <returns>保存的设置参数。</returns>
        public static CommandLineOption GetSavedOptions()
        {
            var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            var appSettings = config.AppSettings;
            var outputPath = appSettings.Get(SettingsConst.OutputPath);
            if (string.IsNullOrWhiteSpace(outputPath) || !Directory.Exists(outputPath))
            {
                outputPath = Directory.GetCurrentDirectory();
            }
            var proxy = appSettings.Get(SettingsConst.Proxy);
            var options = new CommandLineOption
            {
                MaxRetry = int.TryParse(appSettings.Get(SettingsConst.RetryTime), out var i) ? i : 3,
                MergerName = GetValueOrDefault(appSettings.Get(SettingsConst.Merger), SettingsConst.MergerFFMpeg, SettingsConst.MergerMkvMerge),
                Proxy = IsValidProxy(proxy) ? proxy : SettingsConst.ProxyTypeSystem,
                NoMerge = bool.TryParse(appSettings.Get(SettingsConst.MergeOutput), out var b) && !b,
                OutputPath = outputPath,
                ThreadNumber = int.TryParse(appSettings.Get(SettingsConst.ThreadNumber), out i) ? i : 4,
                Timeout = int.TryParse(appSettings.Get(SettingsConst.Timeout), out i) ? i : 60,
                Download = true,
                NotOverrideOutput = false,
                OverrideOutput = false
            };
            return options;
        }

        /// <summary>
        /// 获取 app.config 文件所在的完整路径。
        /// </summary>
        /// <returns>app.config 文件所在的完整路径</returns>
        public static string GetConfigurationFilePath()
            => ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None).FilePath;

        /// <summary>
        /// 将当前用户设置的参数保存到 app.config 文件中。
        /// </summary>
        /// <param name="option">当前的用户参数。</param>
        public static void SaveOptions(CommandLineOption option)
        {
            var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            var appSettings = config.AppSettings;
            appSettings.AddOrUpdate(SettingsConst.RetryTime, option.MaxRetry.ToString());
            appSettings.AddOrUpdate(SettingsConst.Merger, option.MergerName);
            appSettings.AddOrUpdate(SettingsConst.Proxy, option.Proxy);
            appSettings.AddOrUpdate(SettingsConst.MergeOutput, (!option.NoMerge).ToString());
            appSettings.AddOrUpdate(SettingsConst.OutputPath, option.OutputPath);
            appSettings.AddOrUpdate(SettingsConst.ThreadNumber, option.ThreadNumber.ToString());
            appSettings.AddOrUpdate(SettingsConst.Timeout, option.Timeout.ToString());
            config.Save(ConfigurationSaveMode.Modified);
            ConfigurationManager.RefreshSection("appSettings");
        }

        /// <summary>
        /// 如果 originalValue 在 defaultValue 和 otherValues 中则返回 originalValue，否则返回 defaultValue。
        /// </summary>
        private static string GetValueOrDefault(string originalValue, string defaultValue, params string[] otherValues)
            => originalValue == defaultValue || otherValues.Contains(originalValue) ? originalValue : defaultValue;

        /// <summary>
        /// 判断给定的代理服务器地址字符串是否合法。
        /// </summary>
        /// <param name="proxy">代理服务器地址。</param>
        /// <returns>如为合法的代理服务器地址，返回<see langword="true" />。</returns>
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

        /// <summary>
        /// 载入视频信息以获取音视频格式。
        /// </summary>
        /// <param name="url">master.json 的文件地址。</param>
        /// <param name="proxy">代理服务器地址。</param>
        /// <param name="timeout">HTTP 请求超时时间。</param>
        /// <param name="maxRetry">最大重试次数。</param>
        /// <returns></returns>
        public static VimeoVideo GetVideoInfo(string url, string proxy, int timeout, int maxRetry)
        {
            var proxyHandler = ProgramEntrance.GetProxyHandler(proxy);
            using (var httpClient = new HttpClient(proxyHandler) { Timeout = TimeSpan.FromSeconds(timeout) })
            {
                return WebUtility.GetVideoInfo(httpClient, url, maxRetry).Result;
            }
        }

        /// <summary>
        /// 执行下载操作。
        /// </summary>
        /// <param name="option">下载参数。</param>
        /// <param name="overridePromotion">交互式操作中询问用户是否覆盖已存在文件的方法。</param>
        /// <seealso cref="ProgramEntrance.Run" />
        public static void DownloadVideo(CommandLineOption option, Func<string, bool> overridePromotion)
            => ProgramEntrance.Run(option, overridePromotion).Wait();

        /// <summary>
        /// 获取程序版本信息。
        /// </summary>
        /// <returns>程序版本信息。</returns>
        public static string GetVersionInfo()
        {
            var assembly = Assembly.GetEntryAssembly() ?? Assembly.GetCallingAssembly();
            var name = assembly.GetName().Name;
            var version = assembly.GetName().Version.ToString();
            var copyright = assembly.GetCustomAttributes<AssemblyCopyrightAttribute>().FirstOrDefault()?.Copyright;

            return $"{name} {version}{Environment.NewLine}{copyright}";
        } 
    }
}
