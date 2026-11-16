using Evolve.NET.Core;
using Evolve.NET.Core.CrossoverMethods;
using Evolve.NET.Core.MutationMethods;

namespace Evolve.NET.Sample.Math;

public class Program
{
    public static int Main(string[] args)
    {
        int seed = 10;
        int generations = 200;
        if (args.Length > 3 || (args.Length > 0 && !int.TryParse(args[0], out seed)) ||
            (args.Length > 1 && (!int.TryParse(args[1], out generations) || generations < 0)))
        {
            Console.Error.WriteLine("Usage: dotnet run --project Evolve.NET.Samples/Evolve.NET.Sample.Math -- [seed] [generations >= 0] [output.tsv]");
            return 1;
        }
        string output = args.Length > 2 ? args[2] : "math-results.tsv";
        var simulator = new GeneticAlgorithm<int>
        {
            Seed = seed,
            Selection = new TournamentSelection<int>(3),
            Crossover = new OnePointCrossoverOperator<int>(0.8),
            Mutation = new ResetingRandomMutationOperator<int>(0.02),
            Fitness = new FitnessFunction<int>(),
            Elitism = 3,
            Filename = output,
            Debug = new ConsoleDebug<int>(),
            DebugMask = DebugMask.Last
        };
        simulator.Simulate(200, 16, 0, 1, generations);
        Console.WriteLine($"Seed: {seed}; statistics: {output}");
        return 0;
    }
}
