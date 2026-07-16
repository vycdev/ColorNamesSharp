namespace ColorNamesSharp.Tests;

public class DataAccessTests
{
    [Fact]
    public void ColorsIsAReadOnlySnapshotInInsertionOrder()
    {
        NamedColor first = new("First", 1, 2, 3);
        NamedColor second = new("Second", 4, 5, 6);
        ColorNamesBuilder builder = new ColorNamesBuilder()
            .Add(first)
            .Add(second);

        ColorNames colors = builder.Build();
        builder.Add("Added Later", 7, 8, 9);

        Assert.Equal([first, second], colors.Colors);
        IList<NamedColor> list = Assert.IsAssignableFrom<IList<NamedColor>>(colors.Colors);
        Assert.True(list.IsReadOnly);
        Assert.Throws<NotSupportedException>(() => list.Add(new NamedColor("Mutation", 10, 11, 12)));
    }

    [Fact]
    public void TryGetByNameIsCaseInsensitive()
    {
        NamedColor expected = new("Alphabet Blue", 171, 205, 239);
        ColorNames colors = new ColorNamesBuilder()
            .Add(expected)
            .Build();

        Assert.True(colors.TryGetByName("alphabet blue", out NamedColor? actual));
        Assert.Same(expected, actual);
        Assert.False(colors.TryGetByName("Missing", out actual));
        Assert.Null(actual);
        Assert.False(colors.TryGetByName(null, out actual));
        Assert.Null(actual);
    }

    [Fact]
    public void TryGetByHexIsCaseInsensitiveAndRejectsUnknownValues()
    {
        NamedColor expected = new("Alphabet Blue", 171, 205, 239);
        ColorNames colors = new ColorNamesBuilder()
            .Add(expected)
            .Build();

        Assert.True(colors.TryGetByHex("#abcdef", out NamedColor? actual));
        Assert.Same(expected, actual);
        Assert.False(colors.TryGetByHex("#000000", out actual));
        Assert.Null(actual);
        Assert.False(colors.TryGetByHex("not-a-color", out actual));
        Assert.Null(actual);
        Assert.False(colors.TryGetByHex(null, out actual));
        Assert.Null(actual);
    }

    [Fact]
    public void ExactLookupsReturnTheFirstConfiguredDuplicate()
    {
        NamedColor first = new("Duplicate", 1, 2, 3);
        NamedColor second = new("DUPLICATE", 1, 2, 3);
        ColorNames colors = new ColorNamesBuilder()
            .Add(first)
            .Add(second)
            .Build();

        Assert.True(colors.TryGetByName("duplicate", out NamedColor? byName));
        Assert.True(colors.TryGetByHex("#010203", out NamedColor? byHex));
        Assert.Same(first, byName);
        Assert.Same(first, byHex);
    }
}
