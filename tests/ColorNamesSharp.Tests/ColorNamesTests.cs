using System.Collections.Concurrent;
using ColorNamesSharp.Utility;

namespace ColorNamesSharp.Tests;

public class ColorNamesTests
{
    [Fact]
    public void EmptyListReturnsNoMatchOrRandomColor()
    {
        ColorNames colors = new ColorNamesBuilder().Build();

        Assert.Null(colors.FindClosestColor("#ffffff"));
        Assert.Equal("Unknown", colors.FindClosestColorName("#ffffff"));
        Assert.Null(colors.GetRandomNamedColor());
    }

    [Fact]
    public void SearchOverloadsReturnTheExactColor()
    {
        NamedColor expected = new("Target", 12, 34, 56);
        ColorNames colors = new ColorNamesBuilder()
            .Add(new NamedColor("Other", 200, 200, 200))
            .Add(expected)
            .Build();

        Assert.Same(expected, colors.FindClosestColor(expected));
        Assert.Same(expected, colors.FindClosestColor(expected.Rgb));
        Assert.Same(expected, colors.FindClosestColor(expected.Hex));
        Assert.Same(expected, colors.FindClosestColor(expected.Lab));
    }

    [Fact]
    public void RandomColorAlwaysComesFromTheConfiguredList()
    {
        NamedColor[] configured =
        [
            new("One", 1, 2, 3),
            new("Two", 4, 5, 6),
            new("Three", 7, 8, 9)
        ];
        ColorNames colors = new ColorNamesBuilder()
            .Add(configured[0])
            .Add(configured[1])
            .Add(configured[2])
            .Build();

        for (int i = 0; i < 100; i++)
            Assert.Contains(colors.GetRandomNamedColor(), configured);
    }

    [Fact]
    public void KdTreeMatchesExhaustiveSearchForRandomQueries()
    {
        NamedColor[] configured = CreateDeterministicColors();
        ColorNames colors = Build(configured);
        Random random = new(20260716);

        for (int i = 0; i < 500; i++)
        {
            (short r, short g, short b) query = (
                (short)random.Next(256),
                (short)random.Next(256),
                (short)random.Next(256));

            NamedColor expected = FindExhaustively(configured, query);
            NamedColor actual = Assert.IsType<NamedColor>(colors.FindClosestColor(query));

            Assert.Equal(expected.Rgb, actual.Rgb);
        }
    }

    [Fact]
    public void SearchesOnOneInstanceAreThreadSafe()
    {
        NamedColor[] configured = CreateDeterministicColors();
        ColorNames colors = Build(configured);
        (short r, short g, short b)[] queries = CreateDeterministicQueries();
        NamedColor[] expected = queries
            .Select(query => FindExhaustively(configured, query))
            .ToArray();
        ConcurrentQueue<string> failures = new();

        Parallel.For(0, 2_000, i =>
        {
            int queryIndex = i % queries.Length;
            NamedColor? actual = colors.FindClosestColor(queries[queryIndex]);

            if (actual?.Rgb != expected[queryIndex].Rgb)
                failures.Enqueue($"Query {queryIndex}: expected {expected[queryIndex].Hex}, got {actual?.Hex}");
        });

        Assert.Empty(failures);
    }

    private static ColorNames Build(IEnumerable<NamedColor> configured)
    {
        ColorNamesBuilder builder = new();

        foreach (NamedColor color in configured)
            builder.Add(color);

        return builder.Build();
    }

    private static NamedColor[] CreateDeterministicColors()
    {
        Random random = new(8675309);

        return Enumerable.Range(0, 256)
            .Select(index => new NamedColor(
                $"Color {index}",
                (short)random.Next(256),
                (short)random.Next(256),
                (short)random.Next(256)))
            .ToArray();
    }

    private static (short r, short g, short b)[] CreateDeterministicQueries()
    {
        Random random = new(424242);

        return Enumerable.Range(0, 64)
            .Select(_ => (
                (short)random.Next(256),
                (short)random.Next(256),
                (short)random.Next(256)))
            .ToArray();
    }

    private static NamedColor FindExhaustively(
        IEnumerable<NamedColor> colors,
        (short r, short g, short b) query)
    {
        (float l, float a, float b) queryLab = ColorConverter.RGBToLab(query);

        return colors.MinBy(color => SquaredDistance(color.Lab, queryLab))!;
    }

    private static double SquaredDistance(
        (float l, float a, float b) left,
        (float l, float a, float b) right)
    {
        double deltaL = left.l - right.l;
        double deltaA = left.a - right.a;
        double deltaB = left.b - right.b;

        return deltaL * deltaL + deltaA * deltaA + deltaB * deltaB;
    }
}
