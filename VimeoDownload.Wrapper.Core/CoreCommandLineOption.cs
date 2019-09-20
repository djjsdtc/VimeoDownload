namespace VimeoDownload.Wrapper.Core
{
    using System.Collections.Generic;
    using CommandLine.Text;

    /// <summary>
    /// <see cref="CommandLineOption" /> 的 .NET Core 版本重载。
    /// </summary>
    class CoreCommandLineOption : CommandLineOption
    {
        /// <inheritdoc />
        [Usage(ApplicationAlias = "dotnet VimeoDownload.dll")]
        public new static IEnumerable<Example> Examples => CommandLineOption.Examples;
    }
}
