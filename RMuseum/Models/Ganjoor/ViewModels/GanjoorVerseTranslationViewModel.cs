﻿namespace RMuseum.Models.Ganjoor.ViewModels
{
    /// <summary>
    /// verse translation view mode
    /// </summary>
    public class GanjoorVerseTranslationViewModel
    {
        /// <summary>
        /// language id
        /// </summary>
        public int LanguageId { get; set; }

        /// <summary>
        /// verse order
        /// </summary>
        public int VOrder { get; set; }

        /// <summary>
        /// translated text
        /// </summary>
        public string TText { get; set; }
    }
}
