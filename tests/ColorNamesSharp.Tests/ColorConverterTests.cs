using ColorNamesSharp.Utility;

namespace ColorNamesSharp.Tests;

public class ColorConverterTests
{
    [Theory]
    [InlineData("#000000", 0, 0, 0)]
    [InlineData("#ffffff", 255, 255, 255)]
    [InlineData("#AbCdEf", 171, 205, 239)]
    public void HexToRgbParsesSixDigitHex(string hex, short r, short g, short b)
    {
        Assert.Equal((r, g, b), ColorConverter.HexToRgb(hex));
    }

    [Theory]
    [InlineData("")]
    [InlineData("ffffff")]
    [InlineData("#fff")]
    [InlineData("#00000000")]
    public void HexToRgbRejectsInvalidShape(string hex)
    {
        Assert.Throws<ArgumentException>(() => ColorConverter.HexToRgb(hex));
    }

    [Fact]
    public void RgbToLabConvertsReferenceColors()
    {
        AssertLabNear((0f, 0f, 0f), ColorConverter.RGBToLab((0, 0, 0)), 0.001f);
        AssertLabNear((100f, 0f, 0f), ColorConverter.RGBToLab((255, 255, 255)), 0.02f);
        AssertLabNear((53.24f, 80.09f, 67.20f), ColorConverter.RGBToLab((255, 0, 0)), 0.03f);
    }

    private static void AssertLabNear(
        (float l, float a, float b) expected,
        (float l, float a, float b) actual,
        float tolerance)
    {
        Assert.InRange(actual.l, expected.l - tolerance, expected.l + tolerance);
        Assert.InRange(actual.a, expected.a - tolerance, expected.a + tolerance);
        Assert.InRange(actual.b, expected.b - tolerance, expected.b + tolerance);
    }
}
