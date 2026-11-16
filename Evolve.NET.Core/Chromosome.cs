namespace Evolve.NET.Core;

public class Chromosome<T> : IChromosome<T>
{
    public T this[int index] { get => Genes[index]; set => Genes[index] = value; }
    public T[] Genes { get; }
    public int Length => Genes.Length;
    public double Fitness { get; private set; }
    public ISortFitnessComparer<T> Comparer { get; set; } = new SortFitnessMax<T>();

    public Chromosome(int length, int min, int max)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(length);
        RandomHelper.ValidateIntegerGenes<T>(min, max);
        Genes = new T[length];
        for (int i = 0; i < length; i++)
            Genes[i] = (T)(object)RandomHelper.RandomInt(min, max);
    }

    public Chromosome(T[] genes)
    {
        ArgumentNullException.ThrowIfNull(genes);
        ArgumentOutOfRangeException.ThrowIfZero(genes.Length);
        Genes = (T[])genes.Clone();
    }

    public Chromosome(IChromosome<T> chromosome) : this(chromosome.Genes)
    {
        Fitness = chromosome.Fitness;
        Comparer = chromosome.Comparer;
    }

    public void EvaluateFitness(IFitness<T> function)
    {
        ArgumentNullException.ThrowIfNull(function);
        double fitness = function.Evaluate(this);
        if (!double.IsFinite(fitness))
            throw new InvalidOperationException("Fitness must be finite.");
        Fitness = fitness;
    }

    public override bool Equals(object? obj) => obj is Chromosome<T> other && Genes.SequenceEqual(other.Genes);
    public override int GetHashCode()
    {
        var hash = new HashCode();
        foreach (T gene in Genes) hash.Add(gene);
        return hash.ToHashCode();
    }
    public int CompareTo(IChromosome<T>? other) => other is null ? -1 : Comparer.Compare(this, other);
    public override string ToString() => "[" + string.Join("|", Genes) + "]";
    public static bool operator ==(Chromosome<T>? a, Chromosome<T>? b) => Equals(a, b);
    public static bool operator !=(Chromosome<T>? a, Chromosome<T>? b) => !Equals(a, b);
}
