namespace VimeoDownload.Wrapper.Windows
{
    using System.Collections.Generic;
    using CommandLine.Text;

    /// <summary>
    /// <see cref="CommandLineOption" /> 的 .NET Framework 版本重载。
    /// </summary>
    public class WinCommandLineOption : CommandLineOption
    {
        /// <inheritdoc />
        [Usage(ApplicationAlias = "VimeoDownload")]
        public new static IEnumerable<Example> Examples => CommandLineOption.Examples;
    }
}
