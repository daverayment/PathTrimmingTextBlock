namespace PathTrimmingTextBlock.Helpers;

public static class CacheControl
{
	/// <summary>
	/// Controls whether text measurement results are cached.
	/// </summary>
	/// <remarks>Disabling caching does not clear existing caches, but they
	/// will no longer be used during the text measurement process.</remarks>
	public static bool IsCacheEnabled { get; set; } = true;
}
