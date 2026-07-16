using System.Text;

namespace ColorNamesSharp.Tests;

public class ColorNamesBuilderTests
{
    [Fact]
    public void AddSupportsHexRgbAndNamedColor()
    {
        NamedColor existing = new("Existing", 1, 2, 3);
        ColorNamesBuilder builder = new ColorNamesBuilder()
            .Add("From Hex", "#abcdef")
            .Add("From RGB", 10, 20, 30)
            .Add(existing);

        Assert.Collection(
            builder.NamedColors,
            color => Assert.Equal((171, 205, 239), color.Rgb),
            color => Assert.Equal((10, 20, 30), color.Rgb),
            color => Assert.Same(existing, color));
    }

    [Fact]
    public void AddFromCsvReadsUnicodeAndIgnoresBlankLines()
    {
        string path = Path.GetTempFileName();

        try
        {
            File.WriteAllText(
                path,
                "name,hex,good name\nZürich Blue,#248bcc,\n\nTest Red,#ff0000,x\n",
                Encoding.UTF8);

            ColorNamesBuilder builder = new ColorNamesBuilder().AddFromCsv(path);

            Assert.Collection(
                builder.NamedColors,
                color =>
                {
                    Assert.Equal("Zürich Blue", color.Name);
                    Assert.Equal("#248BCC", color.Hex);
                },
                color =>
                {
                    Assert.Equal("Test Red", color.Name);
                    Assert.Equal("#FF0000", color.Hex);
                });
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public void LoadDefaultLoadsCurrentUpstreamSnapshot()
    {
        ColorNamesBuilder builder = new ColorNamesBuilder().LoadDefault();

        Assert.Equal(31_912, builder.NamedColors.Count);
        NamedColor zurichBlue = Assert.Single(
            builder.NamedColors,
            color => color.Name == "Zürich Blue");
        Assert.Equal("#248BCC", zurichBlue.Hex);

        ColorNames colors = builder.Build();
        Assert.Equal(31_912, colors.Colors.Count);
        Assert.True(colors.TryGetByName("zürich blue", out NamedColor? byName));
        Assert.True(colors.TryGetByHex("#248bcc", out NamedColor? byHex));
        Assert.Same(zurichBlue, byName);
        Assert.Same(zurichBlue, byHex);
    }

    [Fact]
    public void BuildCreatesSearchableColorNames()
    {
        ColorNames colors = new ColorNamesBuilder()
            .Add("Red", "#ff0000")
            .Add("Blue", "#0000ff")
            .Build();

        Assert.Equal("Red", colors.FindClosestColorName("#ff0000"));
        Assert.Equal("Blue", colors.FindClosestColorName(0, 0, 250));
    }
}
