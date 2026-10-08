namespace SwingDivergence.Analysis;

/// <summary>Close-confirmed alternating waves, independent of candle colour; wicks only anchor prices.</summary>
internal sealed class AlternatingLegs(double multiplier, int period, double epsilon,
    double minimumReversal, int confirmationBars)
{
    private readonly Queue<double> ranges = new();
    private MarketFrame? previous;
    private bool? highLeg, initialDirection;
    private int initialCount, highIndex, lowIndex, extremeIndex, reversalCount;
    private double initialClose, initialHigh, initialLow, extremePrice, closeExtreme, legOrigin, threshold;
    private double initialCloseHigh, initialCloseLow;
    private bool hasOpposite;
    private int afterHighIndex, afterLowIndex;
    private double afterHigh, afterLow, afterCloseHigh, afterCloseLow;
    private double? lastHigh, lastLow;

    public SwingChange? Add(MarketFrame bar)
    {
        if (!previous.HasValue)
        {
            previous = bar; initialClose = bar.Close;
            initialCloseHigh = initialCloseLow = bar.Close;
            initialHigh = bar.High; initialLow = bar.Low; highIndex = lowIndex = bar.Index;
            ranges.Enqueue(bar.High - bar.Low);
            return null;
        }
        var prior = previous.Value;
        double tr = Math.Max(bar.High - bar.Low,
            Math.Max(Math.Abs(bar.High - prior.Close), Math.Abs(bar.Low - prior.Close)));
        double priorRange = TypicalRange();
        ranges.Enqueue(tr); if (ranges.Count > period) ranges.Dequeue();
        double typical = Math.Min(priorRange, TypicalRange());
        previous = bar;
        double floor = Math.Max(epsilon, minimumReversal / 2);
        if (!highLeg.HasValue)
        {
            if (bar.High > initialHigh) { initialHigh = bar.High; highIndex = bar.Index; }
            if (bar.Low < initialLow) { initialLow = bar.Low; lowIndex = bar.Index; }
            initialCloseHigh = Math.Max(initialCloseHigh, bar.Close);
            initialCloseLow = Math.Min(initialCloseLow, bar.Close);
            double displacement = bar.Close - initialClose;
            bool? direction = displacement >= minimumReversal ? true : displacement <= -minimumReversal ? false : null;
            if (!direction.HasValue) { initialCount = 0; initialDirection = null; return null; }
            if (direction != initialDirection) initialCount = 0;
            initialDirection = direction;
            initialCount++;
            if (initialCount >= confirmationBars)
            {
                bool high = direction.Value;
                Start(high, high ? highIndex : lowIndex, high ? initialHigh : initialLow,
                    high ? initialCloseHigh : initialCloseLow,
                    high ? initialLow : initialHigh, typical);
                if (bar.Index > extremeIndex) AddOpposite(bar);
            }
            return null;
        }
        bool up = highLeg.Value;
        double candidate = up ? bar.High : bar.Low;
        bool extends = up ? candidate > extremePrice : candidate < extremePrice;
        closeExtreme = up ? Math.Max(closeExtreme, bar.Close) : Math.Min(closeExtreme, bar.Close);
        double counterStep = up ? prior.Close - bar.Close : bar.Close - prior.Close;
        if (extends)
        {
            extremeIndex = bar.Index; extremePrice = candidate;
            threshold = ReversalThreshold(typical);
            hasOpposite = false; reversalCount = 0;
            // This candle can arm a rejection; only a later close can confirm it.
            if (counterStep >= floor && Retreat(bar.Close) >= floor) reversalCount = 1;
            return null;
        }
        AddOpposite(bar);
        threshold = Math.Min(threshold, ReversalThreshold(typical));
        double retreat = Retreat(bar.Close);
        if (retreat < floor) reversalCount = 0;
        else if (reversalCount > 0) reversalCount++;
        else if (counterStep >= floor) reversalCount = 1;
        if (reversalCount < confirmationBars || retreat < threshold || !hasOpposite) return null;

        var point = new SwingPoint(extremeIndex, bar.Index, extremePrice, up, Label(up, extremePrice));
        if (up) lastHigh = extremePrice; else lastLow = extremePrice;
        double origin = extremePrice;
        int oppositeIndex = up ? afterLowIndex : afterHighIndex;
        double oppositePrice = up ? afterLow : afterHigh;
        double oppositeClose = up ? afterCloseLow : afterCloseHigh;
        Start(!up, oppositeIndex, oppositePrice, oppositeClose, origin, typical);
        // A V-turn can already be rebounding when the preceding extreme is confirmed.
        // Preserve this first opposing close rather than dropping the next wave.
        if (bar.Index > extremeIndex)
            AddOpposite(bar);
        double nextCounterStep = highLeg.Value ? prior.Close - bar.Close : bar.Close - prior.Close;
        if (nextCounterStep >= floor && Retreat(bar.Close) >= floor) reversalCount = 1;
        return new SwingChange(point);
    }

    private void Start(bool high, int index, double price, double close, double origin, double typical)
    {
        highLeg = high; extremeIndex = index; extremePrice = price; closeExtreme = close; legOrigin = origin;
        hasOpposite = false; reversalCount = 0; threshold = ReversalThreshold(typical);
    }

    private double Retreat(double close) => highLeg.Value ? closeExtreme - close : close - closeExtreme;

    private double ReversalThreshold(double typical)
    {
        // A small wave must not inherit the full amplitude of the previous large impulse.
        double legPart = Math.Abs(extremePrice - legOrigin) * .25;
        return Math.Max(minimumReversal, Math.Min(typical * multiplier, legPart));
    }

    private void AddOpposite(MarketFrame bar)
    {
        if (!hasOpposite)
        {
            afterHigh = bar.High; afterLow = bar.Low; afterHighIndex = afterLowIndex = bar.Index;
            afterCloseHigh = afterCloseLow = bar.Close; hasOpposite = true;
        }
        else
        {
            if (bar.High > afterHigh) { afterHigh = bar.High; afterHighIndex = bar.Index; }
            if (bar.Low < afterLow) { afterLow = bar.Low; afterLowIndex = bar.Index; }
            afterCloseHigh = Math.Max(afterCloseHigh, bar.Close);
            afterCloseLow = Math.Min(afterCloseLow, bar.Close);
        }
    }

    private double TypicalRange()
    {
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

    private string Label(bool high, double price)
    {
        double? previousPrice = high ? lastHigh : lastLow;
        return !previousPrice.HasValue ? high ? "H" : "L"
            : high ? price > previousPrice.Value + epsilon ? "HH" : "LH"
            : price < previousPrice.Value - epsilon ? "LL" : "HL";
    }
}
