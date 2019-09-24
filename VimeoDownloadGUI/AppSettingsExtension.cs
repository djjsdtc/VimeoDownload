namespace VimeoDownload.GUI
{
    using System.Configuration;
    using System.Linq;

    /// <summary>
    /// AppSettings 读写扩展方法。
    /// </summary>
    public static class AppSettingsExtension
    {
        /// <summary>
        /// 判断 <see cref="AppSettingsSection"/> 中是否包含指定的键。
        /// </summary>
        /// <param name="appSettings"><see cref="AppSettingsSection"/> 对象。</param>
        /// <param name="key">键名。</param>
        /// <returns>如果包含指定名称的键，返回 <see langword="true"/>。</returns>
        public static bool ContainsKey(this AppSettingsSection appSettings, string key)
            => appSettings.Settings.AllKeys.Contains(key);

        /// <summary>
        /// 根据给定的键名获取对应的值。
        /// </summary>
        /// <param name="appSettings"><see cref="AppSettingsSection"/> 对象。</param>
        /// <param name="key">键名。</param>
        /// <returns>对应的值，如键不存在则返回 <see langword="null"/>。</returns>
        public static string Get(this AppSettingsSection appSettings, string key)
        {
            if (!appSettings.ContainsKey(key))
            {
                return null;
            }

            return appSettings.Settings[key].Value;
        }

        /// <summary>
        /// 更新给定的键到对应的值。
        /// </summary>
        /// <param name="appSettings"><see cref="AppSettingsSection"/> 对象。</param>
        /// <param name="key">键名。</param>
        /// <param name="value">要设定的值。</param>
        public static void AddOrUpdate(this AppSettingsSection appSettings, string key, string value)
        {
            if (appSettings.ContainsKey(key))
            {
                appSettings.Settings[key].Value = value;
            }
            else
            {
                appSettings.Settings.Add(key, value);
            }
        }
    }
}