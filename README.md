﻿<img align="left" height="119" width="119" src="https://meodai.github.io/color-names/logo/cockatoo-fill.svg">

# Color Names

![GitHub Actions Workflow Status](https://img.shields.io/github/actions/workflow/status/vycdev/ColorNamesSharp/dotnet.yml)
![GitHub Release](https://img.shields.io/github/v/release/vycdev/colornamessharp)
![NuGet Version](https://img.shields.io/nuget/v/ColorNamesSharp)
![NuGet Downloads](https://img.shields.io/nuget/dt/ColorNamesSharp)
[![Join the chat at https://discord.gg/nU63sFMcnX](https://img.shields.io/discord/1165553796223602708?style=flat-square&logo=discord&logoColor=white&label=Discord&color=%237289DA&link=https%3A%2F%2Fdiscord.gg%2FnU63sFMcnX)](https://discord.gg/nU63sFMcnX) 

This library's primary purpose is to be able to specify a color and end up with a fitting name for that color 🌈

## Examples:<br>
`"#ffffff"` -> `White`<br>
`"#facfea"` -> `Classic Rose`<br>
`#abcdef` -> `Alphabet Blue`<br>
`#123456` -> `Incremental Blue`<br>
`#c1b2a3` -> `Balanced Beige`<br>
*...and so on*

## Where do the names come from?!
Color Names is meant to act as an easy drop-in dependency you can import and start using, meaning it's already bundled with a list of color names, a list that is maintained by another awesome open-sourced project: [meodai/color-names](https://github.com/meodai/color-names/). There's around 16.7 million sRGB colors, obviously not all of these are named, but this list provides plenty to work with, we can just extrapolate the closest color from the list.
<br>

#### Ermm actually I don't like the bundled color list 🤓🤓
Color Names remain customisable for those who'd like the extra control: you can use the `ColorNameBuilder` (usage outlined below) to fully customize the color names used, enabling you to add your own colors and even bypass the default list entirely. Alternatively, you could fork this project yourself and replace/modify the list in `ColorLists/Default.csv` with whatever you'd like. If you'd like to make changes to the default list, consider reviewing the naming rules of the open sourced list mentioned above and contributing there.

## How about performance, and how accurate is the "nearest" color?
The default color list has over 30,000 names (that's a lot). Trying to find the closest color by comparing distance in the 3D color space can be pretty computationally expensive.
<br><br>
Other libraries with similar functionality seem to often approach this by iterating over all the colors, plotting the sRGB values and calculating [Euclidian distance](https://en.wikipedia.org/wiki/Euclidean_distance) and whatever has the lowest distance is the "closest" color. This has 2 notable concerns:
<br>
1. The sRGB color space isn't all that accurate in terms of visual similarity, ie: 3 sRGB values that are equally apart in terms of raw numerical value are unlikely to be visually "different" by the same factor. Okay... so, how do we put a number on the visual similarity of colors? Fortunately, that's not my job. The [CIELAB Color Space](https://en.wikipedia.org/wiki/CIELAB_color_space) has us covered! This color space precisely revolves around positioning colors with uniform visual perception and for this reason, its used for all sorts of color correction work, and is exactly what we need. Perfect, we convert our values from the sRGB color space to the CIELAB color space, problem one solved!
2. Iterating through >30,000 vectors in a 3D space and finding the distance between all of them to a point is... a lot of calculations. But it's exactly what we need, since that's how we find the [Delta-E](https://en.wikipedia.org/wiki/Color_difference#CIELAB_%CE%94E*) variance between all our CIELAB colors to see whats the closest. So, we should really try to optimise this. For this we cache our colors in a [K-D Tree](https://en.wikipedia.org/wiki/K-d_tree) with 3 dimensions, providing us with fast [nearest neighbour searches](https://en.wikipedia.org/wiki/Nearest_neighbor_search). This takes the time complexity for searches from O(n) to O(logn). In practice, this makes a pretty substantial difference.

### Benchmarks:

> [!NOTE]
> This section is a work in progress.
> If you'd like to contribute to the benchmarks, feel free to open a PR with your results. 

<br>

## Pretty notes ✨
You can find many resources online about KD-Trees, here's some visuals made by [UwUAroze](https://github.com/UwUAroze/Color-Names?tab=readme-ov-file#pretty-notes-) to help you understand how they work:
![kdTree](https://raw.githubusercontent.com/vycdev/ColorNamesSharp/refs/heads/main/kdTree.png)
> [!NOTE]
> You can open that image in a new tab for a nicer, full-resolution view.

<br>

## Usage
You can download and install the [nuget package from here.](https://www.nuget.org/packages/ColorNamesSharp/) <br>
Or you can clone this repository and use it as a library in your project.

### Creating the instance
```csharp
ColorNames colorNames = new ColorNamesBuilder()
	.Add("Best Blue", "#3299fe") // Add your own custom colors
	.LoadDefault() // Load the default color list
	.AddFromCsv("path/to/your/colorlist.csv") // Add a custom color list from a csv file
	.BuildColorNames; // Get a new ColorNames instance that includes all the colors you've added
```

### Getting a fitting color name

```csharp
NamedColor customNamedColor = new("Best Blue", 50, 153, 254);

// You can directly get the name of the color as a string
string colorNameFromHex = colorNames.FindClosestColorName("#ffffff");
string colorNameFromRgb = colorNames.FindClosestColorName(255, 255, 255);
string colorNameFromNamedColor = colorNames.FindClosestColorName(customNamedColor);

// Or similarly you can get the NamedColor object
NamedColor namedColorFromHex = colorNames.FindClosestColorName("#ffffff");
NamedColor namedColorFromRgb = colorNames.FindClosestColorName(255, 255, 255);
NamedColor namedColorFromNamedColor = colorNames.FindClosestColorName(customNamedColor);

// Or a random color
NamedColor randomColor = colorNames.GetRandomNamedColor();
```

## Credits 

This library is a C# implementation of the original [color names library by meodai](https://github.com/meodai/color-names) and it takes inspiration from [UwUAroze's implementation in Kotlin.](https://github.com/UwUAroze/Color-Names) Huge thanks to both of them for their work!
