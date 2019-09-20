namespace VimeoDownload.DataContract
{
    using Newtonsoft.Json;
    using System.Collections.Generic;

    /// <summary>
    /// 音视频文件共用的元数据。
    /// </summary>
    public abstract class MediaClip
    {
        /// <summary>
        /// 文件编号。
        /// </summary>
        [JsonProperty("id")]
        public string Id { get; set; }

        /// <summary>
        /// 文件路径。此路径为相对路径。
        /// </summary>
        [JsonProperty("base_url")]
        public string BaseUrl { get; set; }

        /// <summary>
        /// 文件格式。
        /// </summary>
        [JsonProperty("format")]
        public string Format { get; set; }

        /// <summary>
        /// 文件类型（MIME）。
        /// </summary>
        [JsonProperty("mime_type")]
        public string MimeType { get; set; }

        /// <summary>
        /// 编码方式。
        /// </summary>
        [JsonProperty("codecs")]
        public string Codecs { get; set; }

        /// <summary>
        /// 比特率。
        /// </summary>
        [JsonProperty("bitrate")]
        public int Bitrate { get; set; }

        /// <summary>
        /// 平均比特率。
        /// </summary>
        [JsonProperty("avg_bitrate")]
        public int AvgBitrate { get; set; }

        /// <summary>
        /// 时长。
        /// </summary>
        [JsonProperty("duration")]
        public double Duration { get; set; }

        /// <summary>
        /// 每个分段的时长。最后一个分段的时长可能小于该值。
        /// </summary>
        [JsonProperty("max_segment_duration")]
        public int MaxSegmentDuration { get; set; }

        /// <summary>
        /// 文件的起始段。有可能是起始段数据的 Base64 编码，也有可能是起始段的相对路径。
        /// </summary>
        [JsonProperty("init_segment")]
        public string InitSegment { get; set; }

        /// <summary>
        /// 文件的剩余分段。
        /// </summary>
        [JsonProperty("segments")]
        public IList<Segment> Segments { get; set; } = new List<Segment>();
    }
}
