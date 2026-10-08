using SwingDivergence.Structure;

namespace SwingDivergence.Analysis;

/// <summary>Alternation, same-kind extrema replacement and ATR noise filter from _structure_for_scale.</summary>
internal sealed class FilteredSwings(int left, int right, double epsilon, double minimumAtr)
{
    private readonly PivotDetector detector = new(left, right, epsilon);
    private readonly List<SwingPoint> tail = new();
    private Pivot? rawLast;

    public List<SwingChange> Add(MarketFrame bar, IReadOnlyList<MarketFrame> frames)
    {
        var batch = detector.AddClosedBar(bar.Index, bar.High, bar.Low);
        var changes = new List<SwingChange>(2);
        if (batch.High is Pivot h) Offer(h, frames, changes);
        if (batch.Low is Pivot l) Offer(l, frames, changes);
        return changes;
    }

    private void Offer(Pivot raw, IReadOnlyList<MarketFrame> frames, List<SwingChange> changes)
    {
        if (rawLast is Pivot lastRaw && lastRaw.IsHigh == raw.IsHigh)
        {
            bool wins = raw.IsHigh ? raw.Price > lastRaw.Price + epsilon : raw.Price < lastRaw.Price - epsilon;
            if (!wins && Math.Abs(raw.Price - lastRaw.Price) > epsilon) return;
        }
        rawLast = raw;
        SwingPoint? removed = null;
        if (tail.Count > 0)
        {
            var last = tail[^1];
            if (last.High == raw.IsHigh)
            {
                bool wins = raw.IsHigh ? raw.Price > last.Price + epsilon : raw.Price < last.Price - epsilon;
                if (!wins && Math.Abs(raw.Price - last.Price) > epsilon) return;
                removed = last;
                tail.RemoveAt(tail.Count - 1);
            }
            else if (frames[raw.BarIndex].Atr is double atr && Math.Abs(raw.Price - last.Price) < minimumAtr * atr)
                return;
        }
        double? previous = null;
        for (int i = tail.Count - 1; i >= 0; i--)
            if (tail[i].High == raw.IsHigh) { previous = tail[i].Price; break; }
        string label = !previous.HasValue ? raw.IsHigh ? "H" : "L"
            : raw.IsHigh ? raw.Price > previous.Value + epsilon ? "HH" : "LH"
            : raw.Price < previous.Value - epsilon ? "LL" : "HL";
        var point = new SwingPoint(raw.BarIndex, raw.ConfirmedAt, raw.Price, raw.IsHigh, label);
        tail.Add(point);
        if (tail.Count > 4) tail.RemoveAt(0);
        changes.Add(new SwingChange(point, removed));
    }
}
