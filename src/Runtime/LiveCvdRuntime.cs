using SwingDivergence.Analysis;

namespace SwingDivergence;

public partial class SwingDivergenceIdentifier
{
    private LiveFrameSnapshot liveCvdSnapshot = new();
    private readonly List<DivergenceSignal> lastLiveCvdSignals = new();
    private bool WantsLiveCvd => ShowCvdSetups && EarlyCvdDetection;

    private void ClearLiveCvdDrawing()
    {
        foreach (var lastSignal in lastLiveCvdSignals)
            for (var node = divergenceDrawings.First; node != null; node = node.Next)
                if (node.Value.Signal.Id == lastSignal.Id
                    && node.Value.Signal.Validation == StructuralCvdSetups.EarlyValidation)
                {
                    RemoveItems(node.Value.Items); divergenceDrawings.Remove(node); break;
                }
        lastLiveCvdSignals.Clear();
    }

    private void UpdateLiveCvd(int index, bool live, double? tickPrice = null)
    {
        if (!WantsLiveCvd || !historyFinished || annotations == null || cvdSetups == null
            || index != frames.Count || index < 0 || index >= VAn.BarVars.Count)
        { ClearLiveCvdDrawing(); return; }
        var bar = VAn.BarVars[index];
        var totals = bar.VolTotList != null && bar.VolTotList.Length > 0 ? bar.VolTotList[0] : null;
        bool newSession = index > 0 && bar.IsNewDay;
        double? openCvd = newSession || index == 0 ? 0 : frames[^1].CvdClose;
        if (totals == null || !openCvd.HasValue)
        {
            cvdSetups.LastCheck = "Volumes Ask/Bid ou cumul CVD indisponibles sur la bougie ouverte.";
            ClearLiveCvdDrawing(); return;
        }
        double delta = (double)totals.AskVol - totals.BidVol;
        double close = tickPrice.HasValue && double.IsFinite(tickPrice.Value) ? tickPrice.Value : bar.Close;
        var current = new MarketFrame(index, newSession ? sessionNumber + 1 : sessionNumber,
            bar.Open, Math.Max(bar.High, close), Math.Min(bar.Low, close), close, totals.TotVol,
            openCvd, openCvd + Math.Max(0, Math.Max(delta, totals.MaxDeltaVol)),
            openCvd + Math.Min(0, Math.Min(delta, totals.MinDeltaVol)), openCvd + delta,
            null, null, null, null);
        var view = liveCvdSnapshot.Update(frames, current);
        var structureCandidates = MajorCvdCandidates(current, view);
        var candidates = cvdSetups.Preview(current, view, structureCandidates);
        foreach (var setup in PreviewLocalCvd(current, view))
            if (!candidates.Any(s => s.Signal.Id == setup.Signal.Id)) candidates.Add(setup);
        if (candidates.Count == 0) { ClearLiveCvdDrawing(); return; }
        foreach (var setup in candidates)
            if (localCvdSetups.Preview(setup.Signal, view).HasValue)
                SoundCvd(setup.Signal, index, live, setup.Confirmation, forming: true);
        var primary = candidates[0].Signal;
        cvdSetups.LastCheck = $"{(primary.Hidden ? "ABS" : "EXH")} {(primary.Bullish ? "haussière" : "baissière")} détectée : "
            + (cvdStructure.HasStructure(primary) ? "au moins une extrémité de structure confirmée." : "aucun point de structure confirmé (rose).");
        if (lastLiveCvdSignals.SequenceEqual(candidates.Select(s => s.Signal))) return;
        ClearLiveCvdDrawing();
        foreach (var setup in candidates)
        {
            DrawDivergence(setup.Signal, false);
            lastLiveCvdSignals.Add(setup.Signal);
        }
    }

    private IReadOnlyList<(SwingPoint Point, int LegStart)> MajorCvdCandidates(MarketFrame current, IReadOnlyList<MarketFrame> view)
    {
        if (Detection == StructureDetection.AtrRetracement) return majorAtr.PreviewRunning(current);
        if (Detection == StructureDetection.FilteredPivots)
            return cvdSetups.Candidates(LiveCvdCandidates.Find(view, LeftBars, RightBars, PriceToleranceTicks * tickSize), view, MinimumSwingAtr);
        var point = Detection == StructureDetection.AlternatingLegs ? majorLegs.Preview(current) : majorReactive.Preview(current);
        return cvdSetups.Candidates(point.HasValue ? new List<SwingPoint> { point.Value } : Array.Empty<SwingPoint>());
    }
}
