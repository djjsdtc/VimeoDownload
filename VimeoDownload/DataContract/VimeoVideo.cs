namespace VimeoDownload.DataContract
{
    using Newtonsoft.Json;
    using System.Collections.Generic;

    public class VimeoVideo
    {
        [JsonProperty("clip_id")]
        public string ClipId { get; set; }

        [JsonProperty("base_url")]
        public string BaseUrl { get; set; }

        [JsonProperty("video")]
        public IList<VideoClip> Video { get; set; }

        [JsonProperty("audio")]
        public IList<AudioClip> Audio { get; set; }
    }
}
