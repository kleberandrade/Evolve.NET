# Migrating from the .NET Framework version

## Toolchain

The library previously targeted .NET Framework 3.5, with examples on .NET Framework 4.6.1. All projects now target .NET 10 using SDK-style project files. Install the .NET 10 SDK and rebuild consumers for .NET 10; the new assembly cannot be loaded by .NET Framework applications. No compatibility facade or published NuGet release is included in this upgrade.

`Directory.Build.props` centralizes the target framework, nullable analysis, deterministic compilation, and warnings-as-errors. `global.json` accepts supported stable .NET 10 feature bands and selects Microsoft Testing Platform for tests. Legacy `App.config`, explicit framework references, ClickOnce settings, and redundant assembly-info files have been removed. Copyright and authorship are retained in the license and project metadata.

## Behavioral corrections

- Tournament selection compares evaluated candidates instead of copying them with fitness zero. Copies retain both fitness and optimization direction.
- Elites are copied independently. Full elitism skips breeding, and odd population sizes retain the requested count. Swapping an incomplete population now throws.
- Configure minimization with `GeneticAlgorithm<int>.Comparer = new SortFitnessMin<int>()`; the comparer propagates to descendants and affects selection, elites, and reported best fitness.
- Roulette now handles zero, signed, and minimization scores with normalized/shifted weights and a uniform fallback, avoiding division by zero and null returns. `RouletteSelection<T>` is the correctly spelled alias; the old `RoulleteSelectionMethod<T>` name remains usable.
- Random crossover chooses an interior cut for multi-gene parents; an explicit zero cut is still supported. Rates and dimensions are validated.
- Invalid counts, lengths, bounds, fitness values, and incomplete algorithm configuration now fail early with descriptive exceptions.
- Integer initialization and resetting mutation explicitly require `T = int`. The original generic cast did not safely support arbitrary gene types.
- Chromosome equality is length-aware and null-safe; array storage is copied. Direct gene edits still require fitness reevaluation.
- `Filename` is optional. Statistics correctly report negative best fitness and minimization, using invariant TSV formatting. The unsupported file-loading constructor throws instead of leaving an uninitialized population.
- Use `Seed` or `RandomHelper.SetSeed` for reproducibility. Fixed seeds produce different trajectories from the legacy implementation because selection, crossover cuts, and integer random draws were corrected.

## Examples and tests

The level generator is now part of the solution and uses the actual generic interfaces. Its heuristic rewards ground and varied terrain instead of favoring an empty map. Both programs terminate without `Console.ReadKey`, show only the final result, accept seed/generation/output arguments, and use separate `.tsv` output paths. The original tile patterns are preserved.

Run tests with `dotnet test --solution Evolve.NET.sln -c Release`. The `--solution` syntax is the .NET 10 Microsoft Testing Platform interface, rather than the old positional VSTest command. Test dependencies are pinned in `Evolve.NET.Tests/packages.lock.json`; use `dotnet restore Evolve.NET.sln --locked-mode` for reproducible restores.
