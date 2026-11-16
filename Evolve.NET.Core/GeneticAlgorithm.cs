using Evolve.NET.Core.CrossoverMethods;
using Evolve.NET.Core.MutationMethods;

namespace Evolve.NET.Core;

public class GeneticAlgorithm<T>
{
    public DebugMask DebugMask { get; set; }
    public ISelection<T>? Selection { get; set; }
    public ICrossover<T>? Crossover { get; set; }
    public IMutation<T>? Mutation { get; set; }
    public IFitness<T>? Fitness { get; set; }
    public int Elitism { get; set; }
    public IDebug<T>? Debug { get; set; }
    public string? Filename { get; set; }
    public int? Seed { get; set; }
    public ISortFitnessComparer<T> Comparer { get; set; } = new SortFitnessMax<T>();
    public IPopulation<T>? Population { get; private set; }

    public void Simulate(int count, int length, int min, int max, int maxGeneration)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(maxGeneration);
        if (Elitism < 0 || Elitism > count) throw new ArgumentOutOfRangeException(nameof(Elitism));
        if (Selection is null || Crossover is null || Mutation is null || Fitness is null || Comparer is null)
            throw new InvalidOperationException("Selection, crossover, mutation, fitness and comparer must be configured.");
        if (Seed.HasValue) RandomHelper.SetSeed(Seed.Value);
        var population = new Population<T>(count, length, min, max, Comparer);
        Population = population;
        population.Evaluate(Fitness);
        if (Filename is not null) population.Save(Filename, false);
        if ((DebugMask & DebugMask.First) != 0) Debug?.Log(population);

        while (population.Generation < maxGeneration)
        {
            population.Elite(Elitism);
            while (!population.IsFullNewGeneration)
            {
                var parent1 = Selection.Select(population);
                var parent2 = Selection.Select(population);
                Crossover.Crossover(parent1, parent2, out var offspring1, out var offspring2);
                Mutation.Mutate(ref offspring1, min, max);
                Mutation.Mutate(ref offspring2, min, max);
                offspring1.Comparer = Comparer;
                offspring2.Comparer = Comparer;
                population.AddChromosomeInNewPopulation(offspring1);
                population.AddChromosomeInNewPopulation(offspring2);
            }
            population.SwapGeneration();
            population.Evaluate(Fitness);
            if (Filename is not null) population.Save(Filename, true);
            if ((DebugMask & DebugMask.Step) != 0) Debug?.Log(population);
        }
        if ((DebugMask & DebugMask.Last) != 0) Debug?.Log(population);
    }
}
