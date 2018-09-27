﻿namespace Evolve.NET.Core
{
    public class SortFitnessMin : ISortFitnessComparer
    {
        public int Compare(IChromosome x, IChromosome y)
        {
            if (x.Fitness > y.Fitness) return 1;
            if (x.Fitness < y.Fitness) return -1;
            return 0;
        }
    }
}