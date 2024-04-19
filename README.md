# PathTrimmingTextBlock

A custom WinUI control that automatically applies path-ellipsis trimming to its contents when the available space is too narrow to contain the full text.

## Illustration

![Mid-Squash Image](https://raw.githubusercontent.com/daverayment/ProjectMedia/main/Mid-Squash%2050.png?raw=true)

[Animated Version](https://html-preview.github.io/?url=https://github.com/daverayment/ProjectMedia/blob/main/SquashAnim.html)

## Compatibility
The control targets the Windows 10 SDK, version 1809 (10.0.17763) as a minimum. This corresponds to the October 2018 update of Windows 10.

## Overview
The `PathTrimmingTextBlock` is a WinUI control that displays a file path or any other string of text. If the available width is not sufficient to display the full text, it intelligently truncates the string, prioritising the display of the file name and showing as much of the directory path as possible, followed by an ellipsis and directory separator character (`...\`).

For example, given the path `C:\Very\Long\Directory\Path\filename.txt` and a narrow available width, the control might display `C:\Very\Lo...\filename.txt`.

## Usage
To use the `PathTrimmingTextBlock` in your own WinUI project, start by simply adding the NuGet package as a dependency:

```
Install-Package DaveRayment.Controls.PathTrimmingTextBlock
```

and then declare it in your XAML layout:

```xml
<Page
    x:Class="MyViews.MainPage"
    xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
    xmlns:controls="using:DaveRayment.Controls">

    <Grid>
        <controls:PathTrimmingTextBlock Text="C:\Very\Long\Directory\Path\filename.txt" />
    </Grid>
</Page>
```

By default, the control will display the full text if it fits within the available width. If not, it will intelligently truncate the text to fit, preserving the filename and as much of the directory path as possible.

## Styling and Model Binding
The `PathTrimmingTextBlock` includes a default template that displays the trimmed text with a tooltip showing the full text.

The control wraps a generic WinUI `TextBlock`, so any TextBlock-related styles may be set or bound to as normal. For example:

```xml
<controls:PathTrimmingTextBlock
    FontSize="24"
    Text="{x:Bind ViewModel.ChosenPath, Mode=OneWay}" />
```
(Note: the `controls` prefix here must have been previously declared to reference the `DaveRayment.Controls` namespace, as in the earlier example.)

## Implementation Details
The `PathTrimmingTextBlock` is implemented as a WinUI custom control that wraps a `TextBlock` control for maximum compatibility. It uses a `TextMeasurement` class to efficiently measure the width of text strings and determine the optimal truncation strategy.

`TextMeasurement` uses a `Microsoft.Graphics.Canvas.CanvasDevice` internally to render and measure the text strings. This introduces a dependency on the `Microsoft.Graphics.Win2D` package.

The `TextMeasurement` class caches the measured widths of text strings to avoid redundant calculations. The `TextMeasurementFactory` class manages the caching of `TextMeasurement` instances, ensuring that shared font properties result in shared measurement instances for optimal performance.

A binary chop method is used when truncating candidate strings to fit in the available space. This is efficient and the majority of paths are fully calculated in 6 measurement passes or fewer.

## Cache Control
Although it is recommended to always use the cache, it may be disabled with the following:

```csharp
PathTrimmingTextBlock.Helpers.CacheControl.IsCacheEnabled = false;
```
Likewise, setting this back to `true` will re-enable usage of the cache. Already-cached values are retained when the cache is disabled, so re-enabling it will use them again.

If you want to ensure the cache is never used, you should set `IsCacheEnabled` to `false` before rendering a form containing `PathTrimmingTextBlock` instances.

More fine-grained control over the cache may be included in future releases. Please let me know which options would be useful for you by [raising an issue](https://github.com/daverayment/PathTrimmingTextBlock/issues).

## Performance Metrics
The `PathTrimmingTextBlock` includes a built-in `EventSource` called "PathTrimmingTextBlock-Metrics" that provides performance metrics related to text measurement and caching. These metrics are currently present for debugging and development purposes, but may be useful for monitoring and optimising the control's performance in your own application. The control's default implementation should, however, be suitable for the vast majority of applications.

To access the performance metrics, you can use the the `dotnet-counters` tool:

```pwsh
dotnet-counters monitor --name "MyApplicationName" PathTrimmingTextBlock-Metrics
```

More information on the `dotnet-counters` tool is available at <https://learn.microsoft.com/en-us/dotnet/core/diagnostics/dotnet-counters>.


## Issues and Contributing

Please use the [Issues](https://github.com/daverayment/PathTrimmingTextBlock/issues) page for bug reporting and suggestions.

Contributions are welcome. Please raise an issue first.

## Licence
MIT Licence. See [LICENSE](https://github.com/daverayment/PathTrimmingTextBlock/blob/master/LICENSE).