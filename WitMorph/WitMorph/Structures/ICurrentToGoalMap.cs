namespace WitMorph.Structures
{
    public interface ICurrentToGoalMap<T> {
        T GetGoalByCurrent(T current);
    }
}