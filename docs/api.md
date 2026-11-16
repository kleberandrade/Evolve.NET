# API and algorithm contract

## Chromosomes and fitness

`IChromosome<T>` exposes mutable `Genes`, an indexer, `Length`, `Fitness`, `Comparer`, and `EvaluateFitness(IFitness<T>)`. `Chromosome<T>` copies array storage on construction and copies fitness and comparer when constructed from another chromosome. Gene objects themselves and the comparer are shared references: copying is shallow for reference-type genes. Equality compares gene sequences, including length and null elements. Hash codes change if genes change; do not mutate chromosomes used as dictionary keys.

Fitness functions must return finite doubles. `EvaluateFitness` rejects NaN and infinity. Fitness is a cached value: after changing genes directly, mutating, or crossing over, call `EvaluateFitness` (or `Population.Evaluate`) before selection, sorting, or reporting. The algorithm reevaluates every chromosome after each completed generation.

Integer initialization (`new Chromosome<int>(length, min, max)`) and resetting mutation use **inclusive** lower and upper bounds, including `int.MaxValue`. Positive length and ordered bounds are required. These integer APIs, including `GeneticAlgorithm<T>.Simulate`, require `T = int`; other types fail with `NotSupportedException`. Generic array-backed chromosomes, comparers, crossover, and selection can still be used independently with custom gene types and fitness/mutation operators.

`SortFitnessMax<T>` places the highest fitness first; `SortFitnessMin<T>` places the lowest first. `CompareTo` orders by fitness, while `Equals` compares genes; these intentionally represent different notions of equivalence. Use a consistent comparer throughout a manually assembled population.

## Selection

- `TournamentSelection<T>(tournamentSize = 3)` samples with replacement. It compares the evaluated candidates and returns an independent chromosome copy, preserving fitness and comparer. Tournament size must be positive and can exceed population size.
- `RouletteSelection<T>` uses fitness-proportional nonnegative weights. Positive maximization scores retain their usual proportions. For minimization, scores are negated; negative scores are shifted by the worst normalized score. If all resulting weights are zero, selection is uniform. The worst individual can have zero probability when shifting is needed. Scaling avoids overflow for large finite scores. This selector supports numeric ascending/descending fitness comparers, not arbitrary nonnumeric preference rules.
- The original spelling `RoulleteSelectionMethod<T>` remains available with the same corrected behavior.

## Operators

`OnePointCrossoverOperator<T>(rate)` independently copies both parents. On crossover it exchanges the suffix starting at the cut. Random cuts are interior positions `1..Length-1` for lengths greater than one. For one-gene chromosomes, a cut of zero exchanges the genes. The explicit-cut overload accepts `0..Length-1`; parents must have the same nonzero length. The constructor parameter retains its legacy spelling `croosoverRate` for named-argument compatibility.

`ResetingRandomMutationOperator<int>(rate)` attempts **one reset per chromosome**, not one per gene. On mutation, it chooses one index and assigns a random integer from the inclusive bounds; the new value can equal the old value. The legacy class spelling is retained. Both operator rates must be finite and in `[0, 1]`.

## Populations and simulation

`Population<T>(count, length, min, max, comparer)` creates an integer population. The comparer is optional and defaults to maximization. `Population<T>(IEnumerable<IChromosome<T>>)` copies an existing nonempty population, preserving each chromosome's comparer and cached fitness.

`Elite(n)` copies the best `n` chromosomes to the next generation. Values from zero through population size are allowed. `AddChromosomeInNewPopulation` adds a chromosome by reference while space remains; excess offspring are ignored, so odd population sizes stay fixed. `SwapGeneration` requires a full next generation and then increments `Generation`. Caller-managed populations must avoid aliased chromosomes and mixed comparers.

`GeneticAlgorithm<int>` requires `Selection`, `Crossover`, `Mutation`, and `Fitness` before `Simulate`. It evaluates generation zero, then performs exactly `maxGeneration` transitions. A zero limit evaluates only the initial population. Full elitism carries all individuals forward without selecting or breeding. `Population` exposes the current population after initialization; after a successful run it contains the final evaluated generation. Reusing an algorithm starts a new simulation.

A non-null `Filename` produces headerless TSV with average and best fitness using invariant decimal formatting. It is a statistics log, not a checkpoint. The old `Population(string fileName)` constructor now explicitly throws `NotSupportedException` instead of silently producing an unusable object.

## Randomness and reproducibility

`Seed` resets the shared `RandomHelper` stream at the start of a simulation. `RandomHelper.SetSeed(seed)` resets it for manually composed operations. Without a seed, operations continue the current stream (initial default seed: 10).

Random draws are synchronized, but separate simulations share the stream. For reproducible experiments, run simulations serially, reset the seed before each run, and keep the runtime version and configuration fixed. Identical seeds are not a promise of identical results across .NET runtime versions. Do not reset the stream during a run. Algorithm and population objects are mutable and do not support concurrent mutation.
