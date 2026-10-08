namespace SwingDivergence.Analysis;

/// <summary>Alternating legs: directional bodies confirm reversals; one neutral pause may bridge the sequence.</summary>
internal sealed class ReactiveSwings(double multiplier, int period, double epsilon,
    double minimumReversal, int directionalBars = 2)
{
    private readonly Queue<double> ranges = new();
    private MarketFrame? previous;
    private bool? highLeg, runDirection;
    private int extremeIndex, runCount, runSpan, runHighIndex, runLowIndex;
    private double extremePrice, threshold, runHigh, runLow;
    private bool runHasSeed;
    private double? lastHigh, lastLow;

    public SwingChange? Add(MarketFrame bar)
    {
        var prior = previous;
        double tr = bar.High - bar.Low;
        if (prior.HasValue)
            tr = Math.Max(tr, Math.Max(Math.Abs(bar.High - prior.Value.Close), Math.Abs(bar.Low - prior.Value.Close)));
        double priorRange = TypicalRange(tr);
        previous = bar;
        ranges.Enqueue(tr);
        if (ranges.Count > period) ranges.Dequeue();
        bool? direction = Direction(bar, prior, priorRange);
        AdvanceRun(bar, direction);
        if (!highLeg.HasValue)
        {
            if (runCount >= directionalBars)
            {
                Start(direction.Value, direction.Value ? runHighIndex : runLowIndex,
                    direction.Value ? runHigh : runLow, priorRange);
                ClearRun();
            }
            return null;
        }
        bool high = highLeg.Value;
        double candidate = high ? bar.High : bar.Low;
        if (high ? candidate > extremePrice : candidate < extremePrice)
        {
            Start(high, bar.Index, candidate, priorRange);
            // The extending bar cannot confirm the pivot. Its opposite body can
            // nevertheless be the first reversal vote, without inventing wick order.
            ClearRun();
            if (direction.HasValue && direction != high)
            {
                runDirection = direction; runCount = runSpan = 1;
                // Its opposite wick may precede the pivot: seed from subsequent bars.
                runHasSeed = false;
            }
            return null;
        }
        // Volatility contraction can lower the threshold; a later expansion never raises it.
        double contractedRange = Math.Min(priorRange, TypicalRange(tr));
        threshold = Math.Min(threshold, Math.Max(minimumReversal, contractedRange * multiplier));
        double retracement = high ? extremePrice - bar.Close : bar.Close - extremePrice;
        if (direction == high || !direction.HasValue || runCount < directionalBars || !runHasSeed || retracement < threshold)
            return null;
        var point = new SwingPoint(extremeIndex, bar.Index, extremePrice, high, Label(high, extremePrice));
        if (high) lastHigh = extremePrice; else lastLow = extremePrice;
        Start(!high, high ? runLowIndex : runHighIndex, high ? runLow : runHigh, priorRange);
        ClearRun();
        return new SwingChange(point);
    }

    private bool? Direction(MarketFrame bar, MarketFrame? prior, double typicalRange)
    {
        if (!prior.HasValue) return null;
        double localRange = Math.Min(typicalRange, bar.High - bar.Low);
        double floor = Math.Max(Math.Max(epsilon, minimumReversal / 2), localRange * .05);
        return bar.Close - bar.Open >= floor && bar.Close - prior.Value.Close >= floor ? true
            : bar.Open - bar.Close >= floor && prior.Value.Close - bar.Close >= floor ? false : null;
    }

    private void AdvanceRun(MarketFrame bar, bool? direction)
    {
        if (!direction.HasValue)
        {
            if (runCount == 0) return;
            runSpan++;
            // Only one non-directional candle may interrupt a meaningful sequence.
            if (runSpan - runCount > 1) { ClearRun(); return; }
            ExtendRun(bar);
            return;
        }
        if (direction != runDirection) ClearRun();
        runDirection = direction; runCount++; runSpan++;
        ExtendRun(bar);
    }

    private void ExtendRun(MarketFrame bar)
    {
        if (!runHasSeed)
        {
            runHigh = bar.High; runLow = bar.Low;
            runHighIndex = runLowIndex = bar.Index; runHasSeed = true;
        }
        else
        {
            if (bar.High > runHigh) { runHigh = bar.High; runHighIndex = bar.Index; }
            if (bar.Low < runLow) { runLow = bar.Low; runLowIndex = bar.Index; }
        }
    }

    private void ClearRun() { runCount = runSpan = 0; runDirection = null; runHasSeed = false; }

    private double TypicalRange(double fallback)
    {
        if (ranges.Count == 0) return fallback;
        // Robust local volatility: one impulse or isolated wick cannot dominate a whole ATR period.
        var sorted = ranges.OrderBy(value => value).ToArray();
        int mid = sorted.Length / 2;
        return sorted.Length % 2 == 0 ? (sorted[mid - 1] + sorted[mid]) / 2 : sorted[mid];
    }

    public SwingPoint? Preview(MarketFrame bar)
    {
        if (!highLeg.HasValue) return null;
        bool high = highLeg.Value;
        double candidate = high ? bar.High : bar.Low;
        bool extends = high ? candidate > extremePrice : candidate < extremePrice;
        return new SwingPoint(extends ? bar.Index : extremeIndex, -1,
            extends ? candidate : extremePrice, high, Label(high, extends ? candidate : extremePrice));
    }

    private void Start(bool high, int index, double price, double typicalRange)
    {
        highLeg = high; extremeIndex = index; extremePrice = price;
        threshold = Math.Max(minimumReversal, Math.Max(0, typicalRange) * multiplier);
    }

    private string Label(bool high, double price)
    {
        double? prior = high ? lastHigh : lastLow;
        return !prior.HasValue ? high ? "H" : "L"
            : high ? price > prior.Value + epsilon ? "HH" : "LH"
            : price < prior.Value - epsilon ? "LL" : "HL";
    }
}
