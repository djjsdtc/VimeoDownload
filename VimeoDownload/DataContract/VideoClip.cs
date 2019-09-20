namespace VimeoDownload.DataContract
{
    using Newtonsoft.Json;

    /// <summary>
    /// 视频文件的元数据。
    /// </summary>
    public class VideoClip : MediaClip
    {
        /// <summary>
        /// 视频帧率。
        /// </summary>
        [JsonProperty("framerate")]
        public double Framerate { get; set; }

        /// <summary>
        /// 视频宽度。
        /// </summary>
        [JsonProperty("width")]
        public int Width { get; set; }

        /// <summary>
        /// 视频高度。
        /// </summary>
        [JsonProperty("height")]
        public int Height { get; set; }
    }
}
