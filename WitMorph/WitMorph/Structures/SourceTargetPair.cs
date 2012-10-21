namespace WitMorph.Structures
{
    public class SourceTargetPair<T>
    {
        public SourceTargetPair(T sourceItem, T targetItem)
        {
            Source = sourceItem;
            Target = targetItem;
        }

        public T Source { get; set; }
        public T Target { get; set; }
    }
}