namespace WitMorph.Structures
{
    public class CurrentAndGoalPair<T>
    {
        public CurrentAndGoalPair(T goalItem, T currentItem)
        {
            Goal = goalItem;
            Current = currentItem;
        }

        public T Goal { get; set; }
        public T Current { get; set; }
    }
}