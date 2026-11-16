using Evolve.NET.Core;

namespace Evolve.NET.Sample.Math;

public class ConsoleDebug<T> : IDebug<T>
{
    public void Log(IPopulation<T> population)
    {
        var best = Enumerable.Range(0, population.Count).Select(i => population[i])
            .OrderBy(c => c, population[0].Comparer).First();
        Console.WriteLine($"Generation {population.Generation}; best fitness: {best.Fitness}");
        Log(best);
    }

    public void Log(IChromosome<T> chromosome)
    {
        Console.WriteLine(chromosome);
    }
}
