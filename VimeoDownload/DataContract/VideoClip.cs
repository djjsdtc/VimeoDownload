

namespace VimeoDownload.DataContract
{
    using Newtonsoft.Json;

    public class VideoClip : MediaClip
    {
        [JsonProperty("framerate")]
        public double Framerate { get; set; }

        [JsonProperty("width")]
        public int Width { get; set; }

        [JsonProperty("height")]
        public int Height { get; set; }
    }
}
