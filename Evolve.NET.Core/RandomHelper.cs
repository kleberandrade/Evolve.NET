namespace Evolve.NET.Core;

public static class RandomHelper
{
    private static Random random = new(10);
    private static readonly object sync = new();

    // This stream is shared. Reset only between simulations, not during a run.
    public static void SetSeed(int seed)
    {
        lock (sync) random = new Random(seed);
    }

    // Both bounds are inclusive, including int.MaxValue.
    public static int RandomInt(int min, int max)
    {
        if (min > max) throw new ArgumentOutOfRangeException(nameof(max));
        lock (sync) return (int)random.NextInt64(min, (long)max + 1);
    }

    public static double RandomDouble()
    {
        lock (sync) return random.NextDouble();
    }

    internal static void ValidateIntegerGenes<T>(int min, int max)
    {
        if (typeof(T) != typeof(int))
            throw new NotSupportedException("Integer initialization and resetting mutation require T = int. Use the array constructor and custom operators for other gene types.");
        if (min > max) throw new ArgumentOutOfRangeException(nameof(max));
    }

    internal static void ValidateRate(double rate)
    {
        if (!double.IsFinite(rate) || rate < 0 || rate > 1)
            throw new ArgumentOutOfRangeException(nameof(rate), "Probability must be between 0 and 1.");
    }
}
