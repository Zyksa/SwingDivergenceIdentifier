namespace SwingDivergence.Analysis;

/// <summary>Port of phidias core.divergence.SwingTracker, publishing only ended legs.</summary>
internal sealed class AtrSwings(double multiplier, int period, double epsilon)
{
    private readonly Queue<double> ranges = new();
    private double rangeTotal;
    private MarketFrame? previous, extreme;
    private bool? highLeg;
    private double? previousHigh, previousLow;
    public int LegStartedAt { get; private set; } = -1;

    // Observe the active structural leg without advancing ATR, anchors or direction.
    public List<(SwingPoint Point, int LegStart)> PreviewRunning(MarketFrame forming)
    {
        var result = new List<(SwingPoint Point, int LegStart)>();
        if (!highLeg.HasValue || !extreme.HasValue || forming.Session != extreme.Value.Session) return result;
        bool high = highLeg.Value;
        bool extending = high ? forming.High >= extreme.Value.High : forming.Low <= extreme.Value.Low;
        var candidate = extending ? forming : extreme.Value;
        result.Add((PreviewPoint(candidate, forming.Index, high), LegStartedAt));
        if (extending) return result;
        double tr = Math.Max(forming.High - forming.Low, Math.Max(Math.Abs(forming.High - previous.Value.Close),
            Math.Abs(forming.Low - previous.Value.Close)));
        double total = rangeTotal + tr - (ranges.Count == period ? ranges.Peek() : 0);
        double threshold = multiplier * total / Math.Min(period, ranges.Count + 1);
        double retreat = high ? extreme.Value.High - forming.Low : forming.High - extreme.Value.Low;
        // The next leg may already meet the reversal threshold on a live tick.
        // It is observed here but never committed before its candle closes.
        if (retreat > threshold)
            result.Add((PreviewPoint(forming, forming.Index, !high), extreme.Value.Index));
        return result;
    }

    private SwingPoint PreviewPoint(MarketFrame candidate, int at, bool high)
    {
        double price = high ? candidate.High : candidate.Low;
        double? prior = high ? previousHigh : previousLow;
        string label = !prior.HasValue ? high ? "H" : "L"
            : high ? price > prior.Value + epsilon ? "HH" : "LH"
            : price < prior.Value - epsilon ? "LL" : "HL";
        return new SwingPoint(candidate.Index, at, price, high, label);
    }

    public SwingChange? Add(MarketFrame bar)
    {
        double tr = bar.High - bar.Low;
        if (previous.HasValue)
            tr = Math.Max(tr, Math.Max(Math.Abs(bar.High - previous.Value.Close), Math.Abs(bar.Low - previous.Value.Close)));
        previous = bar;
        ranges.Enqueue(tr); rangeTotal += tr;
        if (ranges.Count > period) rangeTotal -= ranges.Dequeue();
        if (!extreme.HasValue) { extreme = bar; return null; }
        if (!highLeg.HasValue)
        {
            if (bar.High >= extreme.Value.High) { highLeg = true; extreme = bar; }
            else if (bar.Low <= extreme.Value.Low) { highLeg = false; extreme = bar; }
            return null;
        }
        bool high = highLeg.Value;
        double running = high ? extreme.Value.High : extreme.Value.Low;
        // An extending outside bar cannot also end its own leg: OHLC has no intrabar order.
        if (high ? bar.High >= running : bar.Low <= running) { extreme = bar; return null; }
        double retracement = high ? running - bar.Low : bar.High - running;
        if (retracement <= multiplier * rangeTotal / ranges.Count) return null;
        double? prior = high ? previousHigh : previousLow;
        string label = !prior.HasValue ? high ? "H" : "L"
            : high ? running > prior.Value + epsilon ? "HH" : "LH"
            : running < prior.Value - epsilon ? "LL" : "HL";
        var point = new SwingPoint(extreme.Value.Index, bar.Index, running, high, label);
        if (high) previousHigh = running; else previousLow = running;
        LegStartedAt = point.Index;
        highLeg = !high; extreme = bar;
        return new SwingChange(point);
    }
}
