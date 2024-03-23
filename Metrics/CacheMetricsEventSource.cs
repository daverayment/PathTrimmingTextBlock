using System.Diagnostics.Tracing;
using System.Threading;

namespace DaveRayment.Controls.Metrics;

[EventSource(Name = "PathTrimmingTextBlock-Metrics")]
public class ControlMetricsEventSource : EventSource
{
	public static readonly ControlMetricsEventSource Instance = new();

	/// <summary>
	/// The total number of paths processed. Note: a single path will likely
	/// involve several measurements to determine the optimal string size.
	/// </summary>
	private long _totalPaths = 0;

	/// <summary>
	/// Total number of cache queries which retrieved a pre-cached value.
	/// </summary>
	private long _totalCacheHits = 0;

	/// <summary>
	/// Total number of cache queries where a match was not found and the
	/// string had to be measured.
	/// </summary>
	private long _totalCacheMisses = 0;

	// Event Counters for per-period reporting.
	private readonly IncrementingEventCounter _cacheHits;
	private readonly IncrementingEventCounter _cacheMisses;
	private readonly IncrementingEventCounter _cacheQueries;

	private ControlMetricsEventSource()
	{
		_cacheHits = new IncrementingEventCounter("cache-hits", this)
		{
			DisplayName = "Cache Hits"
		};

		_cacheMisses = new IncrementingEventCounter("cache-misses", this)
		{
			DisplayName = "Cache Misses"
		};

		_cacheQueries = new IncrementingEventCounter("cache-queries", this)
		{
			DisplayName = "Cache Queries"
		};

		new PollingCounter("total-cache-hits", this, () => _totalCacheHits)
		{
			DisplayName = "Total Cache Hits"
		};

		new PollingCounter("total-cache-misses", this, () => _totalCacheMisses)
		{
			DisplayName = "Total Cache Misses"
		};

		new PollingCounter("total-cache-queries", this,
			() => _totalCacheHits + _totalCacheMisses)
		{
			DisplayName = "Total Cache Queries"
		};

		new PollingCounter("total-paths-processed", this,
			() => _totalPaths)
		{
			DisplayName = "Total Whole Paths Processed"
		};

		new PollingCounter("measurement-calls-per-text-string", this,
			() => (double)_totalCacheHits / _totalCacheMisses)
		{
			DisplayName = "Average Measurement Calls Per Text String"
		};

		new PollingCounter("percentage-cache-hits", this,
			() => (double)_totalCacheHits / (_totalCacheHits + _totalCacheMisses) * 100)
		{
			DisplayName = "Percentage Cache Hits"
		};
	}

	///// <summary>
	///// Called once per measured text string to report the number of calls it
	///// took to decide on a final width value to return.
	///// </summary>
	///// <param name="measurementCount"></param>
	//public void ReportNumberOfMeasurements(int measurementCount)
	//{
	//	Interlocked.Add(ref _totalTextMeasurementCalls, measurementCount);
	//	Interlocked.Increment(ref _totalTextStringsMeasured);
	//}

	/// <summary>
	/// Call each time a string measurement is requested. Updates the cache-
	/// related counters and totals.
	/// </summary>
	/// <param name="isCacheHit">Whether the measurement was already present in
	/// the cache and did not need to be calculated.</param>
	public void ReportMeasurement(bool isCacheHit)
	{
		// Every measurement call involves a cache check; equates to the number
		// of cache misses and hits combined.
		_cacheQueries.Increment();

		// Update running and total counters.
		if (isCacheHit)
		{
			_cacheHits.Increment();	
			Interlocked.Increment(ref _totalCacheHits);
		}
		else
		{
			_cacheMisses.Increment();
			Interlocked.Increment(ref _totalCacheMisses);
		}
	}

	/// <summary>
	/// Call each time a new path is processed. This lets us track the total
	/// number of full paths.
	/// </summary>
	public void ReportNewTextString()
	{
		Interlocked.Increment(ref _totalPaths);
	}
}
