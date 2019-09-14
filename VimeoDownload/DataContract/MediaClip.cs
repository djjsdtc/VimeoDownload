namespace VimeoDownload.DataContract
{
    using Newtonsoft.Json;
    using System.Collections.Generic;

    public abstract class MediaClip
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("base_url")]
        public string BaseUrl { get; set; }

        [JsonProperty("format")]
        public string Format { get; set; }

        [JsonProperty("mime_type")]
        public string MimeType { get; set; }

        [JsonProperty("codecs")]
        public string Codecs { get; set; }

        [JsonProperty("bitrate")]
        public int Bitrate { get; set; }

        [JsonProperty("avg_bitrate")]
        public int AvgBitrate { get; set; }

        [JsonProperty("duration")]
        public double Duration { get; set; }

        [JsonProperty("max_segment_duration")]
        public int MaxSegmentDuration { get; set; }

        [JsonProperty("init_segment")]
        public string InitSegment { get; set; }

        [JsonProperty("segments")]
        public IList<Segment> Segments { get; set; } = new List<Segment>();
    }
}
