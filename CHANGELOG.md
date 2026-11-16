# Changelog

## v1.0.0

Evolve.NET's first tagged GitHub release modernizes the evolutionary algorithm library and both examples for .NET 10.

## Compatibility and migration

- Requires the .NET 10 SDK/runtime. This release cannot be loaded by .NET Framework consumers; rebuild applications for .NET 10.
- Integer initialization, the simulation entry point, and resetting mutation require `T = int`. Array-backed chromosomes and independent generic operators remain available.
- Random trajectories differ from the legacy implementation because selection and crossover were corrected. Seeds reproduce serial runs on the same runtime; the random stream is shared process-wide.
- Statistics are plain TSV, with separate output paths for each example. They are not population checkpoints or `.xls` workbooks.

See [the migration guide](https://github.com/kleberandrade/Evolve.NET/blob/v1.0.0/docs/migration.md) and [API contract](https://github.com/kleberandrade/Evolve.NET/blob/v1.0.0/docs/api.md).

## Corrections and improvements

- Preserve evaluated fitness and optimization direction when copying chromosomes and selecting tournament winners.
- Handle zero/negative fitness and minimization in roulette selection without division-by-zero or null selections.
- Copy elites independently, retain population size, and skip breeding when elitism fills the next generation.
- Validate operator probabilities, dimensions, configuration, and finite fitness values.
- Support controlled seeds and optional statistics output, with invariant decimal formatting and correct negative/minimization best scores.
- Repair the level generator's generic interfaces, include it in the solution, and replace the empty-level objective with a documented terrain heuristic.
- Make both examples noninteractive and configurable through seed, generation count, and output arguments.
- Add 26 regression test cases and English installation, usage, examples, API, and migration documentation.
- Preserve the original MIT license and authorship, with notices for development dependencies.

## Validation

Release build, all 26 test cases, and both complete examples pass on GitHub Actions for Linux, macOS, and Windows. The [validation run](https://github.com/kleberandrade/Evolve.NET/actions/runs/43789621646) covers the implementation commit.

Local macOS ARM64 validation also confirmed repeatable TSV output, CLI edge cases, and the README's runnable C# example. The seed-10 math example reaches `4294836225`; the terrain example reaches fitness `150`. The terrain heuristic does not verify playability.

## Getting started

```sh
dotnet restore Evolve.NET.sln --locked-mode
dotnet build Evolve.NET.sln -c Release --no-restore
dotnet test --solution Evolve.NET.sln -c Release --no-build --no-restore
```

This is a source release. No package has been published to NuGet.
