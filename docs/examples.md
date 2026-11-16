# Examples and statistics

Build once from the repository root:

```sh
dotnet restore Evolve.NET.sln --locked-mode
dotnet build Evolve.NET.sln -c Release --no-restore
```

## Binary math optimization

```sh
dotnet run --project Evolve.NET.Samples/Evolve.NET.Sample.Math -c Release --no-build -- 10 200 math-results.tsv
```

The 200-member population contains 16-bit chromosomes. Fitness is the square of the decoded binary integer. Tournament size is 3, elitism is 3, crossover probability is 0.8, and mutation probability is 0.02 per chromosome. The verified seed-10 run reaches the maximum `65535 * 65535 = 4294836225` at generation 200. This is an optimization example, not a guarantee of convergence for every seed or configuration.

## Terrain columns

```sh
dotnet run --project Evolve.NET.Samples/Evolve.NET.Sample.LevelGenerator -c Release --no-build -- 10 300 level-results.tsv
```

The 100-member population contains 50 pattern indices per chromosome. Each index selects a 13-tile column from `PatternFactory.Patterns`. Tournament size is 3, elitism is 3, crossover probability is 0.8, and mutation probability is 0.2 per chromosome. The verified seed-10 run reaches fitness 150 at generation 300.

Fitness gives +2 for each column ending in ground, -3 otherwise, and +1 for a column containing coins or items. Each adjacent pair loses one point if identical, and loses `max(0, abs(groundHeightDifference) - 1)` for an abrupt terrain change. Ground height counts `#` tiles. This heuristic does not model movement, collision, enemy difficulty, or reachability.

The map prints these original symbols directly so redirected output remains readable:

- `.`: empty space
- `#`: ground
- `@`: block
- `?`: coin block
- `!`: item block
- `M`: enemy

## Output files

Each statistics file has two tab-separated columns with no header: `averageFitness` and `bestFitness`. Row one describes generation zero, so a run of 200 generations writes 201 rows. A new run replaces the previous file; each subsequent generation appends one row. Decimal points are independent of system locale. The files are plain TSV, readable by text editors, Excel, and other spreadsheet tools. They are not binary `.xls` workbooks and cannot restore a simulation.

Use different paths to compare seeds:

```sh
dotnet run --project Evolve.NET.Samples/Evolve.NET.Sample.Math -c Release --no-build -- 42 200 math-seed42.tsv
```

For a quick smoke check, use five generations. For initialization only, use zero. Output paths may be absolute or relative to the current working directory; create parent directories first.
