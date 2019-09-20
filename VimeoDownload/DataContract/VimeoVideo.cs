namespace VimeoDownload.DataContract
{
    using Newtonsoft.Json;
    using System.Collections.Generic;

    /// <summary>
    /// Vimeo 视频的总元数据。
    /// </summary>
    public class VimeoVideo
    {
        /// <summary>
        /// 视频 ID。
        /// </summary>
        [JsonProperty("clip_id")]
        public string ClipId { get; set; }

        /// <summary>
        /// 视频文件的总目录地址。该路径为相对路径。
        /// </summary>
        [JsonProperty("base_url")]
        public string BaseUrl { get; set; }

        /// <summary>
        /// 所有视频文件。每个视频文件对应不同清晰度。
        /// </summary>
        [JsonProperty("video")]
        public IList<VideoClip> Video { get; set; }

        /// <summary>
        /// 所有音频文件。每个音频文件对应不同比特率。
        /// </summary>
        [JsonProperty("audio")]
        public IList<AudioClip> Audio { get; set; }

        /// <summary>
        /// 指示音频和视频文件的起始分段表示为 Base64 还是文件路径。
        /// </summary>
        [JsonIgnore]
        public bool IsBase64Init { get; set; }

        /// <inheritdoc />
        public override string ToString() => JsonConvert.SerializeObject(this);
    }
}
