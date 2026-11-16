namespace Evolve.NET.Core.CrossoverMethods;

public class OnePointCrossoverOperator<T> : ICrossover<T>
{
    private readonly double crossoverRate;
    public OnePointCrossoverOperator(double croosoverRate)
    {
        RandomHelper.ValidateRate(croosoverRate);
        crossoverRate = croosoverRate;
    }

    public void Crossover(IChromosome<T> parent1, IChromosome<T> parent2,
        out IChromosome<T> offspring1, out IChromosome<T> offspring2)
    {
        ArgumentNullException.ThrowIfNull(parent1);
        int cutoff = parent1.Length > 1 ? RandomHelper.RandomInt(1, parent1.Length - 1) : 0;
        Crossover(parent1, parent2, out offspring1, out offspring2, cutoff);
    }

    public void Crossover(IChromosome<T> parent1, IChromosome<T> parent2,
        out IChromosome<T> offspring1, out IChromosome<T> offspring2, int cutoffPoint)
    {
        ArgumentNullException.ThrowIfNull(parent1);
        ArgumentNullException.ThrowIfNull(parent2);
        if (parent1.Length != parent2.Length) throw new ArgumentException("Parents must have equal lengths.");
        if (cutoffPoint < 0 || cutoffPoint >= parent1.Length) throw new ArgumentOutOfRangeException(nameof(cutoffPoint));
        offspring1 = new Chromosome<T>(parent1);
        offspring2 = new Chromosome<T>(parent2);
        if (RandomHelper.RandomDouble() < crossoverRate)
            for (int i = cutoffPoint; i < parent1.Length; i++)
                (offspring1[i], offspring2[i]) = (parent2[i], parent1[i]);
    }
}
