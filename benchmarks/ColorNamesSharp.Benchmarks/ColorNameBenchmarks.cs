using BenchmarkDotNet.Attributes;
using ColorNamesSharp.Utility;

namespace ColorNamesSharp.Benchmarks;

[MemoryDiagnoser]
[RankColumn]
[Orderer(BenchmarkDotNet.Order.SummaryOrderPolicy.FastestToSlowest)]
public class LookupBenchmarks
{
    private readonly (short R, short G, short B) query = (250, 207, 234);
    private ColorNames colorNames = null!;
    private (NamedColor Color, (float L, float A, float B) Lab)[] linearColors = null!;

    [GlobalSetup]
    public void Setup()
    {
        colorNames = new ColorNamesBuilder().LoadDefault().Build();
        linearColors = colorNames.Colors
            .Select(color => (color, ColorConverter.RGBToLab(color.Rgb)))
            .ToArray();
    }

    [Benchmark]
    public NamedColor? ExactNameLookup()
    {
        colorNames.TryGetByName("Classic Rose", out NamedColor? color);
        return color;
    }

    [Benchmark]
    public NamedColor? ExactHexLookup()
    {
        colorNames.TryGetByHex("#FBCCE7", out NamedColor? color);
        return color;
    }

    [Benchmark]
    public NamedColor? KdTreeNearestLookup() => colorNames.FindClosestColor(query);

    [Benchmark]
    public NamedColor? LinearNearestLookup()
    {
        (float queryL, float queryA, float queryB) = ColorConverter.RGBToLab(query);
        NamedColor? nearest = null;
        double minimumDistance = double.PositiveInfinity;

        foreach ((NamedColor color, (float l, float a, float b)) in linearColors)
        {
            double deltaL = queryL - l;
            double deltaA = queryA - a;
            double deltaB = queryB - b;
            double distanceSquared = deltaL * deltaL + deltaA * deltaA + deltaB * deltaB;

            if (distanceSquared < minimumDistance)
            {
                minimumDistance = distanceSquared;
                nearest = color;
            }
        }

        return nearest;
    }
}

[MemoryDiagnoser]
[RankColumn]
[Orderer(BenchmarkDotNet.Order.SummaryOrderPolicy.FastestToSlowest)]
public class ConstructionBenchmarks
{
    [Benchmark]
    public ColorNames LoadAndBuildDefaultList() => new ColorNamesBuilder().LoadDefault().Build();
}
