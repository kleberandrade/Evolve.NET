using System.Globalization;

namespace Evolve.NET.Core;

public class Population<T> : IPopulation<T>
{
    private List<IChromosome<T>> chromosomes;
    private readonly List<IChromosome<T>> next = new();

    public IChromosome<T> this[int index]
    {
        get => chromosomes[index];
        set => chromosomes[index] = value ?? throw new ArgumentNullException(nameof(value));
    }
    public int Count => chromosomes.Count;
    public int Generation { get; private set; }
    public bool IsFullNewGeneration => next.Count == Count;

    public Population(int count, int length, int min, int max, ISortFitnessComparer<T>? comparer = null)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(count);
        chromosomes = new List<IChromosome<T>>(count);
        for (int i = 0; i < count; i++)
            chromosomes.Add(new Chromosome<T>(length, min, max) { Comparer = comparer ?? new SortFitnessMax<T>() });
    }

    public Population(IEnumerable<IChromosome<T>> chromosomes)
    {
        ArgumentNullException.ThrowIfNull(chromosomes);
        this.chromosomes = chromosomes.Select(c => (IChromosome<T>)new Chromosome<T>(c)).ToList();
        if (Count == 0) throw new ArgumentException("Population cannot be empty.", nameof(chromosomes));
    }

    public Population(string fileName) => throw new NotSupportedException("Statistics files do not contain genes and cannot restore a population.");

    public void AddChromosomeInNewPopulation(IChromosome<T> chromosome)
    {
        ArgumentNullException.ThrowIfNull(chromosome);
        if (!IsFullNewGeneration) next.Add(chromosome);
    }

    public void SwapGeneration()
    {
        if (!IsFullNewGeneration) throw new InvalidOperationException("The next generation must be full before swapping.");
        chromosomes = new List<IChromosome<T>>(next);
        next.Clear();
        Generation++;
    }

    public void Evaluate(IFitness<T> fitnessFunction)
    {
        ArgumentNullException.ThrowIfNull(fitnessFunction);
        foreach (var chromosome in chromosomes) chromosome.EvaluateFitness(fitnessFunction);
    }

    public void Elite(int elitismNumber)
    {
        if (elitismNumber < 0 || elitismNumber > Count) throw new ArgumentOutOfRangeException(nameof(elitismNumber));
        var sorted = new List<IChromosome<T>>(chromosomes);
        sorted.Sort(chromosomes[0].Comparer);
        for (int i = 0; i < elitismNumber; i++) AddChromosomeInNewPopulation(new Chromosome<T>(sorted[i]));
    }

    public void Save(string filename, bool append)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filename);
        var best = chromosomes[0];
        foreach (var candidate in chromosomes)
            if (best.Comparer.Compare(candidate, best) < 0) best = candidate;
        // Divide first to avoid overflowing when summing large finite fitness values.
        double average = chromosomes.Sum(c => c.Fitness / Count);
        using var output = new StreamWriter(filename, append);
        output.WriteLine(string.Create(CultureInfo.InvariantCulture, $"{average:R}\t{best.Fitness:R}"));
    }
}
