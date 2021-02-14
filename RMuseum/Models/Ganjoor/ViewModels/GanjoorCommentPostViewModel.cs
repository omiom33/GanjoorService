﻿namespace RMuseum.Models.Ganjoor.ViewModels
{
    /// <summary>
    /// new comment model
    /// </summary>
    public class GanjoorCommentPostViewModel
    {
        /// <summary>
        /// Poem Id
        /// </summary>
        public int PoemId { get; set; }

        /// <summary>
        /// comment
        /// </summary>
        public string HtmlComment { get; set; }

        /// <summary>
        /// in reply to
        /// </summary>
        public int? InReplyToId { get; set; }
    }
}
