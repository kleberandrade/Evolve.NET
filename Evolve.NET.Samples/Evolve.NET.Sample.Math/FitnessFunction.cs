using Evolve.NET.Core;

namespace Evolve.NET.Sample.Math;

public class FitnessFunction<T> : IFitness<T>
{
    public double Evaluate(IChromosome<T> chromosome)
    {
        if (chromosome.Length > 31) throw new ArgumentException("This example supports at most 31 binary genes.");
        long value = 0;
        foreach (T gene in chromosome.Genes)
        {
            if (gene is not int bit || (bit != 0 && bit != 1))
                throw new ArgumentException("This example requires integer binary genes.");
            value = value * 2 + bit;
        }
        return (double)value * value;
    }
}
