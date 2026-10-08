using SwingDivergence.Structure;

namespace SwingDivergence.Analysis;

/// <summary>Confirmed local extrema for CVD pairing, independent of displayed major swings.</summary>
internal sealed class LocalCvdPivots(int left, int right, double epsilon)
{
    private readonly PivotDetector detector = new(left, right, epsilon);
    private double? high, low;
    public List<SwingChange> Add(MarketFrame bar)
    {
        var batch = detector.AddClosedBar(bar.Index, bar.High, bar.Low);
        var output = new List<SwingChange>(2);
        if (batch.High is Pivot h)
        {
            string label = !high.HasValue ? "H" : h.Price > high.Value + epsilon ? "HH" : "LH";
            output.Add(new SwingChange(new(h.BarIndex, h.ConfirmedAt, h.Price, true, label))); high = h.Price;
        }
        if (batch.Low is Pivot l)
        {
            string label = !low.HasValue ? "L" : l.Price < low.Value - epsilon ? "LL" : "HL";
            output.Add(new SwingChange(new(l.BarIndex, l.ConfirmedAt, l.Price, false, label))); low = l.Price;
        }
        return output;
    }
}
