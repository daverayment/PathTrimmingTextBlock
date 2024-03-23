using System.Collections.Concurrent;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Text;
using Microsoft.UI.Xaml.Controls;
using Windows.Foundation;

namespace DaveRayment.Controls.Helpers;

/// <summary>
/// Measure text dimensions under a specific font. Utilises caches to
/// optimise repeated measurements of the same text.
/// </summary>
public class TextMeasurement
{
	/// <summary>
	/// Holds the font-related settings used when measuring text dimensions.
	/// </summary>
	public CanvasTextFormat TextFormat { get; private set; }

	/// <summary>
	/// Shared <see cref="CanvasDevice"/> instance used for creating the text
	/// layouts to measure.
	/// </summary>
	private static readonly CanvasDevice _device =
		CanvasDevice.GetSharedDevice();

	/// <summary>
	/// Caches the pixel widths of text strings to avoid recalculation when
	/// the same text is passed in again.
	/// </summary>
	private ConcurrentDictionary<string, double> _stringToWidthCache = new();

	/// <summary>
	/// Caches the <see cref="Size"/> of text strings to avoid recalculation
	/// when the same text is passed in again.
	/// </summary>
	private ConcurrentDictionary<string, Size> _stringToSizeCache = new();

	/// <summary>
	/// Initialise a new instance of the <see cref="TextMeasurement"/> class
	/// using the font properties copied from the supplied <see cref="TextBlock"/>
	/// control.
	/// </summary>
	/// <param name="textBlock">The control from which the font properties 
	/// are to be copied./>
	internal TextMeasurement(TextBlock textBlock)
	{
		TextFormat = new CanvasTextFormat
		{
			FontSize = (float)textBlock.FontSize,
			FontFamily = textBlock.FontFamily.Source,
			FontWeight = textBlock.FontWeight,
			FontStyle = textBlock.FontStyle,
			FontStretch = textBlock.FontStretch
		};
	}

	/// <summary>
	/// Measures the width of the given text string using the current
	/// text format settings.
	/// </summary>
	/// <param name="text">The text to measure.</param>
	/// <returns>The width of the text in pixels.</returns>
	public double MeasureTextWidth(string text)
	{
		bool isCacheHit = true;
		double width = _stringToWidthCache.GetOrAdd(text, t =>
		{
			isCacheHit = false;
			using var layout = new CanvasTextLayout(_device, t, TextFormat,
				float.MaxValue, float.MaxValue);
			return layout.LayoutBounds.Width;
		});
		
		// Update metrics.
		Metrics.ControlMetricsEventSource.Instance.ReportMeasurement(isCacheHit);

		return width;
	}

	/// <summary>
	/// Measure the size of the given text string using the current text
	/// format settings.
	/// </summary>
	/// <param name="text">The text to measure.</param>
	/// <returns>The <see cref="Size"/> of the text in pixels.</returns>
	public Size MeasureText(string text)
	{
		return _stringToSizeCache.GetOrAdd(text, t =>
		{
			using var layout = new CanvasTextLayout(_device, text, TextFormat,
				 float.MaxValue, float.MaxValue);
			return new Size(layout.LayoutBounds.Width, layout.LayoutBounds.Height);
		});
	}
}

/// <summary>
/// Factory class for creating and caching <see cref="TextMeasurement"/>
/// instances based on unique combinations of font properties.
/// </summary>
public class TextMeasurementFactory
{
	/// <summary>
	/// Caches <see cref="TextMeasurement"/> instances to reuse them for
	/// identical font property combinations, optimising resource usage and
	/// performance.
	/// </summary>
	private static readonly ConcurrentDictionary<string, TextMeasurement> _cache = new();

	/// <summary>
	/// Creates or retrieves a cached <see cref="TextMeasurement"/> instance
	/// based on the font properties of the supplied <see cref="TextBlock"/>.
	/// </summary>
	/// <param name="textBlock">The <see cref="TextBlock"/> control from
	/// which to obtain the font properties.</param>
	/// <returns>A <see cref="TextMeasurement"/> instance configured with
	/// the given <see cref="TextBlock"/>'s font properties.</returns>
	public static TextMeasurement Create(TextBlock textBlock)
	{
		string key = $"{textBlock.FontSize}-{textBlock.FontFamily.Source}-{textBlock.FontWeight.Weight}-{textBlock.FontStyle}-{textBlock.FontStretch}";
		return _cache.GetOrAdd(key, _ => new TextMeasurement(textBlock));
	}
}
