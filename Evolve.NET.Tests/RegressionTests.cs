using System.Globalization;
using Evolve.NET.Core;
using Evolve.NET.Core.CrossoverMethods;
using Evolve.NET.Core.MutationMethods;
using Xunit;

[assembly: Xunit.v3.Parallelization(Mode = Xunit.Sdk.ParallelMode.None)]

namespace Evolve.NET.Tests;

public class RegressionTests
{
    private sealed class Fitness(Func<IChromosome<int>, double> evaluate) : IFitness<int>
    {
        public double Evaluate(IChromosome<int> chromosome) => evaluate(chromosome);
    }

    private static Chromosome<int> Scored(int gene, bool minimize = false)
    {
        var c = new Chromosome<int>(new[] { gene });
        if (minimize) c.Comparer = new SortFitnessMin<int>();
        c.EvaluateFitness(new Fitness(x => x[0]));
        return c;
    }

    private static GeneticAlgorithm<int> Algorithm(int elite = 1, bool minimize = false) => new()
    {
        Seed = 42,
        Selection = new TournamentSelection<int>(),
        Crossover = new OnePointCrossoverOperator<int>(0.8),
        Mutation = new ResetingRandomMutationOperator<int>(0.5),
        Fitness = new Fitness(c => c.Genes.Sum()),
        Elitism = elite,
        Comparer = minimize ? new SortFitnessMin<int>() : new SortFitnessMax<int>()
    };

    [Fact]
    public void ChromosomeCopiesGenesFitnessAndOptimizationDirection()
    {
        var original = Scored(-7, true);
        var copy = new Chromosome<int>(original);
        Assert.Equal(-7, copy.Fitness);
        Assert.IsType<SortFitnessMin<int>>(copy.Comparer);
        copy[0] = 99;
        Assert.Equal(-7, original[0]);
    }

    [Fact]
    public void EqualityHandlesNullLengthsAndNullGenes()
    {
        var c = new Chromosome<string?>(new string?[] { null, "x" });
        var equal = new Chromosome<string?>(new string?[] { null, "x" });
        Assert.True(c == equal);
        Assert.Equal(c.GetHashCode(), equal.GetHashCode());
        Assert.False(c == null);
        Assert.True((Chromosome<int>?)null == null);
        Assert.False(new Chromosome<int>(new[] { 1 }).Equals(new Chromosome<int>(new[] { 1, 2 })));
        Assert.Equal("[|x]", c.ToString());
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void TournamentChoosesBestSampledFitnessAndReturnsIndependentCopy(bool minimize)
    {
        var population = new Population<int>(new[] { Scored(-10, minimize), Scored(2, minimize), Scored(30, minimize) });
        RandomHelper.SetSeed(19);
        var sampled = Enumerable.Range(0, 12).Select(_ => population[RandomHelper.RandomInt(0, 2)]).ToList();
        sampled.Sort(population[0].Comparer);
        RandomHelper.SetSeed(19);
        var selected = new TournamentSelection<int>(12).Select(population);
        Assert.Equal(sampled[0].Fitness, selected.Fitness);
        selected[0] = 999;
        Assert.DoesNotContain(Enumerable.Range(0, 3), i => population[i][0] == 999);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void RouletteHandlesSignedFitnessAndFavorsBetterScores(bool minimize)
    {
        var population = new Population<int>(new[] { Scored(-10, minimize), Scored(0, minimize), Scored(10, minimize) });
        RandomHelper.SetSeed(3);
        var counts = new Dictionary<int, int> { [-10] = 0, [0] = 0, [10] = 0 };
        var selector = new RouletteSelection<int>();
        for (int i = 0; i < 6000; i++) counts[selector.Select(population)[0]]++;
        int best = minimize ? -10 : 10;
        Assert.True(counts[best] > counts[0]);
        Assert.True(counts[0] > counts[-best]);
    }

    [Fact]
    public void RouletteHandlesAllZeroFitnessWithoutReturningNull()
    {
        var population = new Population<int>(new[] { Scored(0), Scored(0) });
        for (int i = 0; i < 100; i++) Assert.Equal(0, new RoulleteSelectionMethod<int>().Select(population).Fitness);
    }

    [Fact]
    public void RoulettePreservesPositiveProportionsAndHandlesExtremeFitness()
    {
        var population = new Population<int>(new[] { Scored(1), Scored(3) });
        var selector = new RouletteSelection<int>();
        RandomHelper.SetSeed(321);
        int higher = Enumerable.Range(0, 8000).Count(_ => selector.Select(population)[0] == 3);
        Assert.InRange(higher, 5800, 6200);
        population.Evaluate(new Fitness(c => c[0] == 1 ? -double.MaxValue : double.MaxValue));
        Assert.Equal(double.MaxValue, selector.Select(population).Fitness);
    }

    [Fact]
    public void RouletteSelectsEveryCandidateWhenAllFitnessValuesAreZero()
    {
        var population = new Population<int>(new[] { Scored(1), Scored(2), Scored(3) });
        population.Evaluate(new Fitness(_ => 0));
        RandomHelper.SetSeed(98);
        var selector = new RouletteSelection<int>();
        var selected = Enumerable.Range(0, 300).Select(_ => selector.Select(population)[0]).ToHashSet();
        Assert.Equal(3, selected.Count);
    }

    [Fact]
    public void CrossoverExchangesSuffixAndDoesNotAliasParents()
    {
        var a = new Chromosome<int>(new[] { 1, 2, 3, 4 }) { Comparer = new SortFitnessMin<int>() };
        var b = new Chromosome<int>(new[] { 5, 6, 7, 8 });
        new OnePointCrossoverOperator<int>(1).Crossover(a, b, out var x, out var y, 2);
        Assert.Equal(new[] { 1, 2, 7, 8 }, x.Genes);
        Assert.Equal(new[] { 5, 6, 3, 4 }, y.Genes);
        Assert.IsType<SortFitnessMin<int>>(x.Comparer);
        x[0] = 9;
        Assert.Equal(1, a[0]);
        new OnePointCrossoverOperator<int>(0).Crossover(a, b, out x, out y, 1);
        Assert.Equal(a.Genes, x.Genes);
        Assert.NotSame(a.Genes, x.Genes);
        Assert.Throws<ArgumentException>(() => new OnePointCrossoverOperator<int>(1).Crossover(a, Scored(0), out _, out _));
        Assert.Throws<ArgumentOutOfRangeException>(() => new OnePointCrossoverOperator<int>(1).Crossover(a, b, out _, out _, 4));
    }

    [Fact]
    public void SingleGeneCrossoverIsDefined()
    {
        new OnePointCrossoverOperator<int>(1).Crossover(Scored(1), Scored(2), out var a, out var b);
        Assert.Equal(2, a[0]);
        Assert.Equal(1, b[0]);
    }

    [Fact]
    public void MutationResetsAtMostOneGeneAndHonorsZeroProbability()
    {
        IChromosome<int> c = new Chromosome<int>(new[] { 1, 1, 1, 1 });
        new ResetingRandomMutationOperator<int>(0).Mutate(ref c, 9, 9);
        Assert.Equal(new[] { 1, 1, 1, 1 }, c.Genes);
        new ResetingRandomMutationOperator<int>(1).Mutate(ref c, 9, 9);
        Assert.Equal(1, c.Genes.Count(x => x == 9));
        Assert.Equal(3, c.Genes.Count(x => x == 1));
    }

    [Fact]
    public void RandomSeedReplaysAndInclusiveBoundsSupportIntMaxValue()
    {
        RandomHelper.SetSeed(123);
        var a = Enumerable.Range(0, 30).Select(_ => RandomHelper.RandomInt(-3, 3)).ToArray();
        RandomHelper.SetSeed(123);
        Assert.Equal(a, Enumerable.Range(0, 30).Select(_ => RandomHelper.RandomInt(-3, 3)).ToArray());
        Assert.Equal(int.MaxValue, RandomHelper.RandomInt(int.MaxValue, int.MaxValue));
        Assert.Equal(int.MinValue, RandomHelper.RandomInt(int.MinValue, int.MinValue));
    }

    [Fact]
    public void ElitesAreIndependentAndIncompleteGenerationCannotSwap()
    {
        var original = Scored(10);
        var population = new Population<int>(new[] { original, Scored(-2) });
        Assert.Throws<InvalidOperationException>(population.SwapGeneration);
        var previousBest = population[0];
        population.Elite(1);
        population.AddChromosomeInNewPopulation(Scored(-9));
        population.AddChromosomeInNewPopulation(Scored(99));
        population.SwapGeneration();
        Assert.Equal(2, population.Count);
        Assert.Equal(1, population.Generation);
        Assert.Equal(10, population[0].Fitness);
        population[0][0] = 88;
        Assert.Equal(10, original[0]);
        Assert.Equal(10, previousBest[0]);
    }

    private sealed class Trace : IDebug<int>
    {
        public List<double> Best { get; } = new();
        public void Log(IPopulation<int> population) => Best.Add(Enumerable.Range(0, population.Count)
            .Select(i => population[i]).OrderBy(c => c, population[0].Comparer).First().Fitness);
        public void Log(IChromosome<int> chromosome) { }
    }

    [Theory]
    [InlineData(1, 0, false)]
    [InlineData(1, 1, true)]
    [InlineData(5, 0, false)]
    [InlineData(5, 2, false)]
    [InlineData(6, 6, true)]
    [InlineData(7, 3, true)]
    public void EvolutionKeepsPopulationSizeAndElitismAcrossGenerations(int count, int elite, bool minimize)
    {
        var algorithm = Algorithm(elite, minimize);
        var trace = new Trace();
        algorithm.Debug = trace;
        algorithm.DebugMask = DebugMask.First | DebugMask.Step;
        algorithm.Simulate(count, 8, -3, 3, 12);
        Assert.Equal(count, algorithm.Population!.Count);
        Assert.Equal(12, algorithm.Population.Generation);
        Assert.Equal(13, trace.Best.Count);
        if (elite > 0)
            for (int i = 1; i < trace.Best.Count; i++)
                Assert.True(minimize ? trace.Best[i] <= trace.Best[i - 1] : trace.Best[i] >= trace.Best[i - 1]);
        for (int i = 0; i < count; i++)
        {
            Assert.Equal(algorithm.Population[i].Genes.Sum(), algorithm.Population[i].Fitness);
            Assert.All(algorithm.Population[i].Genes, gene => Assert.InRange(gene, -3, 3));
        }
    }

    [Fact]
    public void RepeatedSimulationWithSameSeedReproducesFinalPopulation()
    {
        var algorithm = Algorithm();
        algorithm.Simulate(9, 10, 0, 1, 20);
        var expected = Enumerable.Range(0, 9).Select(i => algorithm.Population![i].ToString()).ToArray();
        algorithm.Simulate(9, 10, 0, 1, 20);
        Assert.Equal(expected, Enumerable.Range(0, 9).Select(i => algorithm.Population![i].ToString()).ToArray());
    }

    [Theory]
    [InlineData(false, -1.5)]
    [InlineData(true, -2.5)]
    public void StatisticsUseInvariantCultureAndCorrectNegativeBest(bool minimize, double best)
    {
        var population = new Population<int>(new[] { Scored(-3, minimize), Scored(-5, minimize) });
        population.Evaluate(new Fitness(c => c[0] / 2.0));
        string file = Path.GetTempFileName();
        var previousCulture = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("pt-BR");
            population.Save(file, false);
            population.Save(file, true);
            Assert.Equal(new[] { $"-2\t{best.ToString(CultureInfo.InvariantCulture)}", $"-2\t{best.ToString(CultureInfo.InvariantCulture)}" }, File.ReadAllLines(file));
        }
        finally { CultureInfo.CurrentCulture = previousCulture; File.Delete(file); }
    }

    [Fact]
    public void ZeroGenerationsEvaluatesInitialPopulationAndWritesOneRow()
    {
        string file = Path.GetTempFileName();
        try
        {
            var a = Algorithm();
            a.Filename = file;
            a.Simulate(5, 4, 1, 1, 0);
            Assert.Equal(0, a.Population!.Generation);
            Assert.Equal("4\t4", Assert.Single(File.ReadAllLines(file)));
        }
        finally { File.Delete(file); }
    }

    [Fact]
    public void InvalidConfigurationFailsWithClearExceptions()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new Population<int>(0, 3, 0, 1));
        Assert.Throws<ArgumentOutOfRangeException>(() => new Chromosome<int>(0, 0, 1));
        Assert.Throws<ArgumentOutOfRangeException>(() => new Chromosome<int>(3, 2, 1));
        Assert.Throws<NotSupportedException>(() => new Chromosome<double>(3, 0, 1));
        Assert.Throws<NotSupportedException>(() => new Population<int>("statistics.tsv"));
        Assert.Throws<ArgumentOutOfRangeException>(() => new TournamentSelection<int>(0));
        foreach (double rate in new[] { -0.1, 1.1, double.NaN, double.PositiveInfinity })
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new OnePointCrossoverOperator<int>(rate));
            Assert.Throws<ArgumentOutOfRangeException>(() => new ResetingRandomMutationOperator<int>(rate));
        }
        Assert.Throws<InvalidOperationException>(() => new GeneticAlgorithm<int>().Simulate(5, 3, 0, 1, 1));
        Assert.Throws<ArgumentOutOfRangeException>(() => Algorithm(6).Simulate(5, 3, 0, 1, 1));
        Assert.Throws<ArgumentOutOfRangeException>(() => Algorithm().Simulate(5, 3, 0, 1, -1));
        Assert.Throws<InvalidOperationException>(() => Scored(0).EvaluateFitness(new Fitness(_ => double.NaN)));
    }

    [Fact]
    public void SampleFitnessFunctionsMatchTheirDocumentedObjectives()
    {
        var math = new Evolve.NET.Sample.Math.FitnessFunction<int>();
        Assert.Equal(25, math.Evaluate(new Chromosome<int>(new[] { 1, 0, 1 })));
        var level = new Evolve.NET.Sample.LevelGenerator.FitnessFunction();
        Assert.True(level.Evaluate(new Chromosome<int>(new[] { 1, 9, 1 })) >
                    level.Evaluate(new Chromosome<int>(new[] { 0, 0, 0 })));
        Assert.All(Evolve.NET.Sample.LevelGenerator.PatternFactory.Patterns, pattern => Assert.Equal(13, pattern.Length));
    }
}
