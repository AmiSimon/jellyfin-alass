namespace SubtitleSync.Core.Models
{
    /// <summary>
    /// Enumeration of supported subtitle formats.
    /// </summary>
    public enum SubtitleFormat
    {
        /// <summary>
        /// SubRip Text format (.srt)
        /// </summary>
        SRT,

        /// <summary>
        /// Advanced SubStation Alpha format (.ass)
        /// </summary>
        ASS,

        /// <summary>
        /// SubStation Alpha format (.ssa)
        /// </summary>
        SSA,

        /// <summary>
        /// Web Video Text Tracks format (.vtt)
        /// </summary>
        WEBVTT,

        /// <summary>
        /// Alternative name for WebVTT
        /// </summary>
        VTT
    }
}
