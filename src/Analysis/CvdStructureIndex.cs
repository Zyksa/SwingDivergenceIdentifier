namespace SwingDivergence.Analysis;

/// <summary>Structure confluence is measured at the divergence's two price anchors.</summary>
internal sealed class CvdStructureIndex
{
    private readonly HashSet<(int Index, bool High)> confirmed = new();
    private readonly List<SwingPoint> major = new(), local = new();

    public void RecordMajor(SwingChange change)
    {
        if (change.Removed is SwingPoint old)
        {
            confirmed.Remove((old.Index, old.High));
            major.RemoveAll(p => p.Index == old.Index && p.High == old.High);
        }
        confirmed.Add((change.Point.Index, change.Point.High));
        major.Add(change.Point);
    }

    public void RecordLocal(SwingChange change) => local.Add(change.Point);

    public bool HasStructure(DivergenceSignal signal)
        => confirmed.Contains((signal.First.Index, signal.First.High))
            || confirmed.Contains((signal.Second.Index, signal.Second.High));

    public string AlertIdentity(DivergenceSignal signal, IReadOnlyList<MarketFrame> frames)
    {
        int session = frames[signal.First.Index].Session;
        bool structured = HasStructure(signal);
        var points = structured ? major : local;
        int origin = -1, first = signal.First.Index;
        for (int i = points.Count - 1; i >= 0; i--)
        {
            var point = points[i];
            if (point.Index >= signal.Second.Index || point.Index >= frames.Count || frames[point.Index].Session != session) continue;
            if (point.High != signal.Second.High && origin < 0) origin = point.Index;
            if (structured && point.High == signal.Second.High) { first = point.Index; break; }
        }
        // Local extrema can move while their intervening leg stays the same.
        return structured ? $"{session}:CVD:{(signal.Second.High ? "H" : "L")}:{first}:LEG:{origin}"
            : $"{session}:CVD-local:{(signal.Second.High ? "H" : "L")}:LEG:{origin}";
    }
}
