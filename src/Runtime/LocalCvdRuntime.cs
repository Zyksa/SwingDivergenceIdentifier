using SwingDivergence.Analysis;

namespace SwingDivergence;

public partial class SwingDivergenceIdentifier
{
    private LocalCvdPivots localCvdPivots;
    private Divergences localCvdDivergences;
    private CvdSetups localCvdSetups;
    private CvdStructureIndex cvdStructure = new();
    private readonly Dictionary<string, DivergenceSignal> knownCvdSignals = new();
    private readonly HashSet<string> notifiedCvdPairs = new();

    private void RememberCvd(DivergenceSignal signal)
    {
        if (knownCvdSignals.TryGetValue(signal.Id, out var old)
            && old.Validation == "Structure CVD confirmée" && signal.Validation != old.Validation) return;
        knownCvdSignals[signal.Id] = signal;
        if (knownCvdSignals.Count > MaxDivergences)
        {
            var oldest = knownCvdSignals.OrderBy(p => p.Value.Second.Index).First().Key;
            knownCvdSignals.Remove(oldest);
        }
    }

    private void ProcessLocalCvd(MarketFrame frame, bool live)
    {
        foreach (var change in localCvdPivots.Add(frame))
        {
            cvdStructure.RecordLocal(change);
            foreach (var signal in localCvdDivergences.Offer(change, frames))
            {
                RememberCvd(signal);
                if (ShowCvdDivergences || ShowCvdSetups) DrawDivergence(signal, false);
                if (ShowCvdSetups)
                    foreach (var setup in localCvdSetups.Offer(signal, frames)) HandleSetup(setup, live);
            }
        }
        if (ShowCvdSetups)
            foreach (var setup in localCvdSetups.Advance(frames)) HandleSetup(setup, live);
    }

    private List<CvdSetup> PreviewLocalCvd(MarketFrame current, IReadOnlyList<MarketFrame> view)
    {
        var result = new List<CvdSetup>();
        foreach (var point in LiveCvdCandidates.Find(view, DivergenceExpert.CvdLeftBars, DivergenceExpert.CvdRightBars,
            PriceToleranceTicks * tickSize))
        foreach (var signal in localCvdDivergences.Preview(point, view))
        {
            result.Add(new CvdSetup(signal with { Validation = StructuralCvdSetups.EarlyValidation }, current.Index,
                current.Close, double.NaN, double.NaN, 0, StructuralCvdSetups.EarlyValidation));
        }
        foreach (var pending in localCvdSetups.PreviewPending(view))
            if (!result.Any(s => s.Signal.Id == pending.Signal.Id))
                result.Add(pending with { Signal = pending.Signal with { Validation = StructuralCvdSetups.EarlyValidation,
                    ConfirmedAt = current.Index }, Confirmation = StructuralCvdSetups.EarlyValidation });
        return result;
    }

    private void PromoteCvd(SwingChange change, int availableAt, bool live)
    {
        foreach (var signal in knownCvdSignals.Values.ToArray())
        {
            bool matches = signal.First.Index == change.Point.Index || signal.Second.Index == change.Point.Index;
            if (change.Removed is SwingPoint old) matches |= signal.First.Index == old.Index || signal.Second.Index == old.Index;
            if (!matches) continue;
            if (ShowCvdDivergences || ShowCvdSetups) DrawDivergence(signal, false, refresh: true);
            if (!live || !ShowCvdSetups || availableAt - signal.Second.Index > DirectionalBars) continue;
            var candidate = localCvdSetups.Preview(signal with { ConfirmedAt = availableAt }, frames);
            if (candidate.HasValue)
                SoundCvd(candidate.Value.Signal, availableAt, live, "Div Cvd avec structure");
        }
    }
}
