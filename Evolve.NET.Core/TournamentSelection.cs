namespace Evolve.NET.Core;

public class TournamentSelection<T> : ISelection<T>
{
    private readonly int tournamentSize;
    public TournamentSelection(int tournamentSize = 3)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(tournamentSize);
        this.tournamentSize = tournamentSize;
    }

    public IChromosome<T> Select(IPopulation<T> population)
    {
        ArgumentNullException.ThrowIfNull(population);
        if (population.Count == 0) throw new ArgumentException("Population cannot be empty.", nameof(population));
        IChromosome<T> best = population[RandomHelper.RandomInt(0, population.Count - 1)];
        for (int i = 1; i < tournamentSize; i++)
        {
            var candidate = population[RandomHelper.RandomInt(0, population.Count - 1)];
            if (best.Comparer.Compare(candidate, best) < 0) best = candidate;
        }
        return new Chromosome<T>(best);
    }
}
