namespace VimeoDownload.DataContract
{
    using Newtonsoft.Json;

    public class Segment
    {
        [JsonProperty("start")]
        public double Start { get; set; }

        [JsonProperty("end")]
        public double End { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; }
    }
}
