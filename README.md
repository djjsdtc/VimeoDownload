# VimeoDownload
下载 Vimeo 视频和托管于 Vimeo 的第三方网站视频。

## 功能
 - 根据给定的 master.json 视频元数据地址下载视频和音频
 - 使用外部工具（ffmpeg 或 mkvmerge）进行音视频合并
 - 多线程下载，并可控制线程数
 - 清晰度可选
 - 支持代理服务器
 - GUI 版本可选（仅 Windows）

## 用法
.NET Core 编译版本可跨平台使用，适用于 Windows、Linux 和 macOS。
``` dos
dotnet ./VimeoDownload.dll --download "https://skyfire.vimeocdn.com/test1/342688958/sep/video/test2/master.json?base64_init=1"
```
.NET Framework 编译版本仅可用于 Windows。
``` dos
VimeoDownload.exe -d --audio 1370478512 --merger mkvmerge --video 1370478523 "https://skyfire.vimeocdn.com/test1/342688958/sep/video/test2/master.json?base64_init=1"
```
命令行版本的更多用法和示例，请参阅 `--help` 命令中的描述。（后续也会上线 wiki 来详细描述各个参数的使用）

GUI 版本请根据界面指示进行操作。（后续也会上线 wiki 进行操作指导）

## 环境
### 编译环境
 - Visual Studio 2017 或更高版本
 - .NET Framework SDK 4.6.1 或更高版本
 - .NET Core SDK 2.2

### 运行环境
对于 .NET Framework 编译版本和 GUI 版本：
 - Windows 7 SP1 或更高版本的 Windows 操作系统
 - .NET Framework 4.6.1 或更高版本

对于 .NET Core 编译版本：
 - 支持 .NET Core 2.2 的操作系统（详见 [.NET Core 2.2 - Supported OS versions](https://github.com/dotnet/core/blob/master/release-notes/2.2/2.2-supported-os.md)）
 - .NET Core 2.2 运行时

无论使用哪种版本，如需使用音视频合并，则系统中至少应装有 [ffmpeg](https://ffmpeg.org/) 或 [mkvmerge](https://mkvtoolnix.download/) 其中之一，并确保程序所在的路径已添加到 `PATH` 环境变量中。

验证方法：在任意目录下直接执行 `ffmpeg` 或 `mkvmerge`，如出现版本号则表示配置正确。

## 相关项目
 - [Newtonsoft.Json](https://github.com/JamesNK/Newtonsoft.Json)
 - [Flurl](https://github.com/tmenier/Flurl)
 - [HttpToSocks5Proxy](https://github.com/MihaZupan/HttpToSocks5Proxy)
 - [CommandLineParser](https://github.com/commandlineparser/commandline)

## 许可协议
Copyright (C) 2019 Cookie Studio. All rights reserved.

Open-sourced under MIT License.
