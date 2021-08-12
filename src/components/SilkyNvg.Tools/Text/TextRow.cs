namespace SilkyNvg.Text
{
    public struct TextRow
    {

        /// <summary>
        /// Input text from where row starts.
        /// </summary>
        public string Start { get; internal set; }

        /// <summary>
        /// The input text where the row ends (one past the last character).
        /// </summary>
        public string End { get; internal set; }

        /// <summary>
        /// Beginning, and rest of, the next row.
        /// </summary>
        public string Next { get; internal set; }

        /// <summary>
        /// Logical width of the row.
        /// </summary>
        public float Width { get; internal set; }

        /// <summary>
        /// Actual least X-bound of the row. Logical with and bounds can differ because of kerning and some parts over extending.
        /// </summary>
        public float MinX { get; internal set; }

        /// <summary>
        /// Actual largest X-bound of the row. Logical with and bounds can differ because of kerning and some parts over extending.
        /// </summary>
        public float MaxX { get; internal set; }

    }
}
