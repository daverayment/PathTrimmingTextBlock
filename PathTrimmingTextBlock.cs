using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.IO;

namespace DaveRayment.Controls;

/// <summary>
/// A TextBlock which automatically applies path-ellipsis trimming to its
/// contents if the available space is too narrow to contain the full text.
/// </summary>
public class PathTrimmingTextBlock : Control
{
	/// <summary>
	/// The wrapped TextBlock control for the possibly-truncated path.
	/// </summary>
	private TextBlock _textBlock;

	/// <summary>
	/// The helper used to measure the width of the text.
	/// </summary>
	private Helpers.TextMeasurement _measurement;

	/// <summary>
	/// The Text property receives the path to be trimmed. Changes result in
	/// updates to the displayed TextBlock contents, but this property is
	/// unchanged.
	/// </summary>
	public static readonly DependencyProperty TextProperty =
		DependencyProperty.Register(
			"Text",
			typeof(string),
			typeof(PathTrimmingTextBlock),
			new PropertyMetadata(default(string), OnTextChanged));

	/// <summary>
	/// The non-filename part of the full path.
	/// </summary>
	private string _directoryPath;

	/// <summary>
	/// The filename part of the path.
	/// </summary>
	private string _filename;

	/// <summary>
	/// The full file path.
	/// </summary>
	public string Text
	{
		get => (string)GetValue(TextProperty);
		set => SetValue(TextProperty, value);
	}

	public PathTrimmingTextBlock()
	{
		this.DefaultStyleKey = typeof(PathTrimmingTextBlock);
		this.SizeChanged += OnSizeChanged;
	}

	private void OnSizeChanged(object sender, RoutedEventArgs e)
	{
		ApplyPathTrimming();
	}

	protected override void OnApplyTemplate()
	{
		base.OnApplyTemplate();
		_textBlock = GetTemplateChild("PART_PathTextBox") as TextBlock;
		if (_textBlock != null)
		{
			_measurement = Helpers.TextMeasurementFactory.Create(_textBlock);
			ApplyPathTrimming();
		}
	}

	private static void OnTextChanged(DependencyObject d,
		DependencyPropertyChangedEventArgs args)
	{
		if (d is PathTrimmingTextBlock control && args.NewValue is string newValue)
		{
			control._filename = Path.GetFileName(newValue);
			control._directoryPath = Path.GetDirectoryName(newValue) ?? "";
			control.ApplyPathTrimming();

			Metrics.ControlMetricsEventSource.Instance.ReportNewTextString();
		}
	}

	private void ApplyPathTrimming()
	{
		if (_textBlock == null || string.IsNullOrEmpty(Text) || this.ActualWidth == 0)
		{
			return;
		}

		double _availableWidth = this.ActualWidth;

		// Does the full path fit?
		if (MeasureStringWidth(Text) <= _availableWidth)
		{
			_textBlock.Text = this.Text;
			return;
		}

		// First check to see if "...\<filename>" fits.
		string filenameAndEllipsis = "...\\" + _filename;
		double filenameWidth = MeasureStringWidth(filenameAndEllipsis);

		if (filenameWidth > _availableWidth)
		{
			// The filename suffix doesn't fit, so truncate it.
			_textBlock.Text = TruncateText(_filename, _availableWidth);
			return;
		}

		// The filename suffix fits, so the next step is to prepend as much
		// of the directory portion of the path as will fit in the space left.
		_availableWidth -= filenameWidth;

		string truncatedDirectoryPath = TruncateText(_directoryPath,
			_availableWidth, "", false);

		_textBlock.Text = truncatedDirectoryPath + filenameAndEllipsis;
	}

	/// <summary>
	/// Attempts to shorten a string so that it fits within a specified width,
	/// optionally prepending ellipses to indicate truncation. It uses a binary
	/// search approach for efficiency.
	/// </summary>
	/// <param name="text">The string to truncate, e.g. "filename.txt".</param>
	/// <param name="availableWidth">The pixel width of the container within
	/// which to fit the string.</param>
	/// <param name="prefix">The prefix string to use for left-truncated text.
	/// "..." by default.
	/// <paramref name="truncateLeft"/>Whether to remove characters from the 
	/// left of the string (the default of true), or the right.</param>
	/// <returns>The longest string which fits within the available width.
	/// </returns>
	private string TruncateText(string text, double availableWidth,
		string prefix = "...", bool truncateLeft = true)
	{
		// If the prefix and text fit, return it without truncating.
		if (MeasureStringWidth(prefix + text) <= availableWidth)
		{
			return prefix + text;
		}

		// Subtract the pixel width of the prefix from the available width.
		// NB: this isn't exact, because of the space between the prefix and 
		// the rest of the text, but is a decent compromise. This is so we
		// don't have to concatenate prefix and text strings every iteration.
		if (prefix.Length > 0)
		{
			availableWidth -= MeasureStringWidth(prefix);
		}

		// The 'good' condition boundary where this number of characters have
		// been tested to fit within the available width.
		int low = 0;
		// This is the 'bad' condition boundary, representing the first guess
		// that is too long to fit.
		int high = text.Length;

		// Converge on the highest value of low that fits. Exits when low and
		// high are adjacent.
		while (low < high - 1)
		{
			// Calculate the midpoint of low and high to test next.
			int mid = low + (high - low) / 2;
			// Create the candidate string depending on whether we need to
			// remove characters from the beginning or end.
			string candidate = truncateLeft ? text[^mid..] : text[0..mid];

			// If the candidate fits within the width, move the lower bound up,
			// else shift the higher bound down.
			if (MeasureStringWidth(candidate) <= availableWidth)
			{
				low = mid;
			}
			else
			{
				high = mid;
			}
		}

		// low now represents the last length verified to fit.
		return prefix + (truncateLeft ? text[^low..] : text[0..low]);
	}

	/// <summary>
	/// Measure the length of a piece of text.
	/// </summary>
	/// <param name="text">The text to measure.</param>
	/// <returns>The width of the text as would be rendered in the control.
	/// </returns>
	private double MeasureStringWidth(string text) => _measurement.MeasureTextWidth(text);
}
