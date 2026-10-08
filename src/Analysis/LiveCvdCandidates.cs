namespace SwingDivergence.Analysis;

internal static class LiveCvdCandidates
{
    public static List<SwingPoint> Find(IReadOnlyList<MarketFrame> frames, int left, int right, double epsilon)
    {
        var output = new List<SwingPoint>();
        int last = frames.Count - 1;
        // The open bar and the recent extrema still waiting for right-hand closes.
        for (int index = Math.Max(left, last - right); index <= last; index++)
        {
            var bar = frames[index];
            if (bar.Session != frames[last].Session) continue;
            bool high = true, low = true;
            for (int i = index - left; i <= Math.Min(last, index + right); i++)
            {
                if (i == index) continue;
                if (frames[i].Session != bar.Session) { high = low = false; break; }
                // Last equal retest wins, consistently with the confirmed window detector.
                if (i < index ? frames[i].High > bar.High + epsilon : frames[i].High >= bar.High - epsilon) high = false;
                if (i < index ? frames[i].Low < bar.Low - epsilon : frames[i].Low <= bar.Low + epsilon) low = false;
            }
            if (high) output.Add(new SwingPoint(index, last, bar.High, true, "H?"));
            if (low) output.Add(new SwingPoint(index, last, bar.Low, false, "L?"));
        }
        return output;
    }
}

/// <summary>A separate reusable snapshot; no iterator state machine or non-generic collections.</summary>
internal sealed class LiveFrameSnapshot
{
    private readonly List<MarketFrame> view = new();
    private int closedCount = -1;
    public IReadOnlyList<MarketFrame> Update(IReadOnlyList<MarketFrame> closed, MarketFrame forming)
    {
        if (closedCount != closed.Count)
        {
            view.Clear();
            for (int i = 0; i < closed.Count; i++) view.Add(closed[i]);
            view.Add(forming); closedCount = closed.Count;
        }
        else view[^1] = forming;
        return view;
    }
}
