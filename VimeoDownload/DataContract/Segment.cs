namespace VimeoDownload.DataContract
{
    using Newtonsoft.Json;

    /// <summary>
    /// 文件分段的元数据。
    /// </summary>
    public class Segment
    {
        /// <summary>
        /// 起始时间点。
        /// </summary>
        [JsonProperty("start")]
        public double Start { get; set; }

        /// <summary>
        /// 终止时间点。
        /// </summary>
        [JsonProperty("end")]
        public double End { get; set; }

        /// <summary>
        /// 分段路径。该路径为相对路径。
        /// </summary>
        [JsonProperty("url")]
        public string Url { get; set; }
    }
}
