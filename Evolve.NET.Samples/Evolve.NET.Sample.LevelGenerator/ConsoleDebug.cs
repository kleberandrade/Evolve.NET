using Evolve.NET.Core;

namespace Evolve.NET.Sample.LevelGenerator;

public class ConsoleDebug : IDebug<int>
{
    public void Log(IPopulation<int> population)
    {
        var best = Enumerable.Range(0, population.Count).Select(i => population[i])
            .OrderBy(c => c, population[0].Comparer).First();
        Console.WriteLine($"Generation {population.Generation}; best fitness: {best.Fitness}");
        Log(best);
    }

    public void Log(IChromosome<int> chromosome)
    {
        for (int row = 0; row < PatternFactory.PATTERN_LENGTH; row++)
        {
            for (int column = 0; column < chromosome.Length; column++)
                Console.Write(PatternFactory.Patterns[chromosome[column]][row]);
            Console.WriteLine();
        }
    }
}
