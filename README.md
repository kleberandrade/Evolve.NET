# Evolve.NET

A small C# evolutionary algorithm library with integer chromosomes, tournament and roulette selection, one-point crossover, resetting mutation, and elitism. Includes runnable binary optimization and terrain generation examples.

## Requirements

- [.NET 10 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/10.0), a supported LTS release ([support policy](https://dotnet.microsoft.com/en-us/platform/support/policy)). `global.json` accepts stable 10.0 SDK feature bands from 10.0.100 onward.
- Windows, macOS, or Linux supported by .NET 10. No Visual Studio or .NET Framework installation is required.
- Network access to NuGet for the test dependencies on the first restore. The core library and examples have no external package dependencies.

## Get started

```sh
git clone https://github.com/kleberandrade/Evolve.NET.git
cd Evolve.NET
dotnet restore Evolve.NET.sln --locked-mode
dotnet build Evolve.NET.sln -c Release --no-restore
dotnet test --solution Evolve.NET.sln -c Release --no-build --no-restore
```

Run the examples from the repository root:

```sh
dotnet run --project Evolve.NET.Samples/Evolve.NET.Sample.Math -c Release --no-build -- 10 200 math-results.tsv
dotnet run --project Evolve.NET.Samples/Evolve.NET.Sample.LevelGenerator -c Release --no-build -- 10 300 level-results.tsv
```

Each example accepts optional positional arguments: `[seed] [generations >= 0] [output.tsv]`. Defaults are seed `10`, 200 generations for Math, 300 for LevelGenerator, and separate `math-results.tsv` / `level-results.tsv` files. They print the final best chromosome or map and exit without waiting for keyboard input. Invalid argument syntax exits with code 1. Output directories must already exist.

Math maximizes the square of the unsigned integer encoded by 16 binary genes. With seed 10 and 200 generations, the validated run reaches sixteen ones and fitness `4294836225`. The fitness helper accepts up to 31 binary integer genes.

LevelGenerator evolves 50 columns selected from 26 original tile patterns. It rewards grounded columns and coin/item patterns, and penalizes gaps, abrupt height changes, and repeated adjacent patterns. The resulting ASCII map is a terrain demonstration; its fitness does not prove that the level is playable. See [examples and statistics](docs/examples.md).

## Use the library

Reference the project from a .NET 10 application:

```sh
dotnet add path/to/YourApp.csproj reference Evolve.NET.Core/Evolve.NET.Core.csproj
```

The following is a complete `Program.cs`:

```csharp
using System;
using System.Linq;
using Evolve.NET.Core;
using Evolve.NET.Core.CrossoverMethods;
using Evolve.NET.Core.MutationMethods;

var algorithm = new GeneticAlgorithm<int>
{
    Seed = 42,
    Selection = new TournamentSelection<int>(3),
    Crossover = new OnePointCrossoverOperator<int>(0.8),
    Mutation = new ResetingRandomMutationOperator<int>(0.1),
    Fitness = new CountOnes(),
    Elitism = 2
};
algorithm.Simulate(count: 50, length: 16, min: 0, max: 1, maxGeneration: 100);
var population = algorithm.Population!;
var best = Enumerable.Range(0, population.Count)
    .Select(i => population[i])
    .OrderBy(c => c, algorithm.Comparer).First();
Console.WriteLine($"{best}: {best.Fitness}");

sealed class CountOnes : IFitness<int>
{
    public double Evaluate(IChromosome<int> chromosome) => chromosome.Genes.Sum();
}
```

Larger fitness is better by default. Set `Comparer = new SortFitnessMin<int>()` on the algorithm to minimize instead. `Filename` is optional; when supplied, it writes average and best fitness for generation zero and every completed generation, overwriting the previous run. Use `IDebug<T>` with `DebugMask.First`, `Step`, or `Last` for callbacks.

See the [API and algorithm contract](docs/api.md) for probability semantics, cloning, population management, randomness, and custom gene types, and the [migration guide](docs/migration.md) for changes from the .NET Framework version.

## Tests and continuous integration

Tests use xUnit.net v3 with Microsoft Testing Platform, selected by `global.json`. The .NET 10 test command uses `--solution`:

```sh
dotnet test --solution Evolve.NET.sln -c Release
```

Regression tests cover fitness-preserving selection, roulette with zero/negative fitness, crossover and mutation boundaries, chromosome equality and copying, generation size, full/partial elitism, minimization, reproducible seeds, invariant statistics, invalid inputs, and both sample fitness functions. Tests run serially because the random stream is shared. The NuGet lock file pins test dependencies.

The GitHub Actions workflow builds, tests, and runs both examples on Linux, macOS, and Windows. Local validation was performed with SDK 10.0.404 on macOS ARM64; hosted CI requires pushing this commit and has not been run as part of the local upgrade. See the [local validation record](docs/validation.md) for commands, results, and limits.

## License

[MIT](LICENSE), copyright (c) 2018 Kléber de Oliveira Andrade. Original authorship is preserved. See [third-party notices](docs/third-party-notices.md) for development dependencies.
