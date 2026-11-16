namespace Evolve.NET.Core;

public class SortFitnessMax<T> : ISortFitnessComparer<T>
{
    public int Compare(IChromosome<T>? x, IChromosome<T>? y)
    {
        if (ReferenceEquals(x, y)) return 0;
        if (x is null) return 1;
        if (y is null) return -1;
        return y.Fitness.CompareTo(x.Fitness);
    }
}
