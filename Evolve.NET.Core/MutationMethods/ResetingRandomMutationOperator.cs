namespace Evolve.NET.Core.MutationMethods;

public class ResetingRandomMutationOperator<T> : IMutation<T>
{
    private readonly double mutateRate;
    public ResetingRandomMutationOperator(double mutateRate)
    {
        RandomHelper.ValidateRate(mutateRate);
        this.mutateRate = mutateRate;
    }

    // Probability is per chromosome; at most one gene is reset per call.
    public void Mutate(ref IChromosome<T> chromosome, int min, int max)
    {
        ArgumentNullException.ThrowIfNull(chromosome);
        RandomHelper.ValidateIntegerGenes<T>(min, max);
        if (RandomHelper.RandomDouble() < mutateRate)
            chromosome[RandomHelper.RandomInt(0, chromosome.Length - 1)] = (T)(object)RandomHelper.RandomInt(min, max);
    }
}
