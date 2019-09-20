namespace VimeoDownload.DataContract
{
    using Newtonsoft.Json;

    /// <summary>
    /// 音频文件的元数据。
    /// </summary>
    public class AudioClip : MediaClip
    {
        /// <summary>
        /// 音频通道数。
        /// </summary>
        [JsonProperty("channels")]
        public int Channels { get; set; }

        /// <summary>
        /// 音频采样率。
        /// </summary>
        [JsonProperty("sample_rate")]
        public int SampleRate { get; set; }
    }
}