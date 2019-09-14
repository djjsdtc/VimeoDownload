namespace VimeoDownload.DataContract
{
    using Newtonsoft.Json;

    public class AudioClip : MediaClip
    {
        [JsonProperty("channels")]
        public int Channels { get; set; }

        [JsonProperty("sample_rate")]
        public int SampleRate { get; set; }
    }
}
