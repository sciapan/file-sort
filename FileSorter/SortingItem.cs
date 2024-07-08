namespace FileSorter
{
    internal readonly struct SortingItem
    {
        #region Fields

        internal int Number { get; }

        internal string Text { get; }

        /// <summary>
        /// Reader index where item is located.
        /// </summary>
        internal int ReaderIndex { get; }

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the SortingItem.
        /// </summary>
        /// <param name="line">Data line.</param>
        public SortingItem(string line) : this(line, 0)
        {
        }

        /// <summary>
        /// Initializes a new instance of the SortingItem.
        /// </summary>
        /// <param name="line">Data line.</param>
        /// <param name="readerIndex">Reader index.</param>
        public SortingItem(string line, int readerIndex)
        {
            var split = line.Split(". ");

            Number = int.Parse(split[0]);
            Text = split[1];
            ReaderIndex = readerIndex;
        }

        #endregion
    }
}