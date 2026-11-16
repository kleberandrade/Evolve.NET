using Evolve.NET.Core;

namespace Evolve.NET.Sample.LevelGenerator;

// A terrain heuristic, not a physics or playability validator.
public class FitnessFunction : IFitness<int>
{
    public double Evaluate(IChromosome<int> chromosome)
    {
        double fitness = 0;
        for (int i = 0; i < chromosome.Length; i++)
        {
            string pattern = PatternFactory.Patterns[chromosome[i]];
            fitness += pattern[^1] == PatternFactory.PATTERN_GROUND ? 2 : -3;
            if (pattern.Contains(PatternFactory.PATTERN_BLOCK_COIN) || pattern.Contains(PatternFactory.PATTERN_BLOCK_ITEM))
                fitness += 1;
            if (i > 0)
            {
                string previous = PatternFactory.Patterns[chromosome[i - 1]];
                int height = pattern.Count(c => c == PatternFactory.PATTERN_GROUND);
                int previousHeight = previous.Count(c => c == PatternFactory.PATTERN_GROUND);
                fitness -= System.Math.Max(0, System.Math.Abs(height - previousHeight) - 1);
                if (chromosome[i] == chromosome[i - 1]) fitness -= 1;
            }
        }
        return fitness;
    }
}
