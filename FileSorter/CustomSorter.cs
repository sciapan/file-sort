namespace FileSorter
{
    internal class CustomSorter : IComparer<SortingItem>
    {
        public int Compare(SortingItem x, SortingItem y)
        {
            var textComparison = string.Compare(x.Text, y.Text, StringComparison.Ordinal);
            if (textComparison != 0)
            {
                return textComparison;
            }

            return x.Number.CompareTo(y.Number);
        }
    }
}