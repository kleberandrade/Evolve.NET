namespace Evolve.NET.Core;

// Keep the original misspelled public name for source compatibility.
public class RoulleteSelectionMethod<T> : ISelection<T>
{
    public IChromosome<T> Select(IPopulation<T> population)
    {
        ArgumentNullException.ThrowIfNull(population);
        if (population.Count == 0) throw new ArgumentException("Population cannot be empty.", nameof(population));
        var candidates = Enumerable.Range(0, population.Count).Select(i => population[i]).ToList();
        candidates.Sort(candidates[0].Comparer);

        // Normalize before summing to avoid overflow. Shift negative scores to
        // nonnegative weights; negate scores when smaller fitness is better.
        double scale = candidates.Max(c => System.Math.Abs(c.Fitness));
        bool minimize = candidates[0].Fitness < candidates[^1].Fitness;
        double[] weights = candidates.Select(c => scale == 0 ? 0 :
            (minimize ? -c.Fitness : c.Fitness) / scale).ToArray();
        double offset = System.Math.Min(0, weights.Min());
        for (int i = 0; i < weights.Length; i++) weights[i] -= offset;
        if (weights.Sum() == 0) Array.Fill(weights, 1);
        double point = RandomHelper.RandomDouble() * weights.Sum();
        for (int i = 0; i < candidates.Count; i++)
        {
            point -= weights[i];
            if (point < 0) return new Chromosome<T>(candidates[i]);
        }
        return new Chromosome<T>(candidates[^1]);
    }
}

public class RouletteSelection<T> : RoulleteSelectionMethod<T> { }
