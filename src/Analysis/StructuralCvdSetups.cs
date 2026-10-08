namespace SwingDivergence.Analysis;

/// <summary>Shared structure (or standalone reference ATR) -> price-led CVD pair -> eligible setup.</summary>
internal sealed class StructuralCvdSetups(double swingAtr, int atrPeriod,
    DivergenceOptions divergenceOptions, SetupOptions setupOptions, bool externalStructure = false)
{
    private AtrSwings swings;
    private Divergences divergences;
    private CvdSetups setups;
    private int lastIndex = -1, session = -1;
    private readonly Dictionary<string, string> alertIdentities = new();
    private readonly Dictionary<string, string> previewIdentities = new();
    internal const string EarlyValidation = "Setup Div Cvd précoce (provisoire)";
    private readonly List<SwingPoint> structureTail = new();
    public string LastCheck { get; set; } = "En attente de points de structure.";
    private int CurrentLegStart => externalStructure ? (structureTail.Count > 0 ? structureTail[^1].Index : -1) : swings.LegStartedAt;

    public string AlertIdentity(DivergenceSignal signal)
    {
        if (signal.Validation == EarlyValidation && previewIdentities.TryGetValue(signal.Id, out string earlyIdentity))
            return earlyIdentity;
        return alertIdentities.TryGetValue(signal.Id, out string identity) ? identity : LegIdentity(signal, CurrentLegStart);
    }

    private string LegIdentity(DivergenceSignal signal, int start)
        => $"{session}:CVD:{(signal.Second.High ? "H" : "L")}:{signal.First.Index}:LEG:{start}";

    public List<CvdSetup> Preview(MarketFrame forming, IReadOnlyList<MarketFrame> view,
        IReadOnlyList<(SwingPoint Point, int LegStart)> structureCandidates = null)
    {
        var result = new List<CvdSetup>();
        previewIdentities.Clear();
        if (swings == null || forming.Session != session || forming.Index != lastIndex + 1
            || forming.Index != view.Count - 1 || CurrentLegStart < 0)
        { LastCheck = "En attente de structure confirmée dans la session courante."; return result; }
        var candidates = externalStructure ? structureCandidates : swings.PreviewRunning(forming);
        LastCheck = "Aucun point de structure en formation à comparer.";
        if (candidates != null)
        foreach (var candidate in candidates)
        {
            var signals = divergences.Preview(candidate.Point with { ConfirmedAt = forming.Index }, view);
            LastCheck = divergences.LastCheck;
            foreach (var signal in signals)
            {
                var setup = setups.Preview(signal with { Validation = EarlyValidation }, view);
                if (setup.HasValue)
                {
                    previewIdentities[signal.Id] = LegIdentity(signal, candidate.LegStart);
                    result.Add(setup.Value with { Confirmation = EarlyValidation });
                }
                else LastCheck = "Direction/type désactivé ou confirmation BOS/balayage non satisfaite.";
            }
        }
        foreach (var pending in setups.PreviewPending(view))
        {
            if (result.Any(s => s.Signal.Id == pending.Signal.Id)) continue;
            previewIdentities[pending.Signal.Id] = AlertIdentity(pending.Signal);
            result.Add(pending with
            {
                Signal = pending.Signal with { Validation = EarlyValidation, ConfirmedAt = forming.Index },
                Confirmation = EarlyValidation
            });
        }
        if (result.Count > 0)
        {
            var signal = result[0].Signal;
            LastCheck = $"{(signal.Hidden ? "ABS" : "EXH")} {(signal.Bullish ? "haussière" : "baissière")} détectée : bougies {signal.First.Index + 1} → {signal.Second.Index + 1} (provisoire).";
        }
        return result;
    }

    public List<CvdSetup> Add(MarketFrame closed, IReadOnlyList<MarketFrame> frames,
        IReadOnlyList<SwingChange> structureChanges = null)
    {
        // Only closes commit state; Preview observes a separate snapshot on ticks.
        var output = new List<CvdSetup>();
        if (closed.Index <= lastIndex || closed.Index != frames.Count - 1) return output;
        lastIndex = closed.Index;
        if (swings == null || session != closed.Session)
        {
            session = closed.Session;
            alertIdentities.Clear();
            previewIdentities.Clear();
            structureTail.Clear();
            swings = new AtrSwings(swingAtr, atrPeriod, divergenceOptions.PriceEpsilon);
            divergences = new Divergences(divergenceOptions with
            {
                Cvd = true, Technical = false, Hidden = true, Weak = false,
                AlignmentBars = 0, ClearPrice = true, ClearCvd = true
            });
            setups = new CvdSetups(setupOptions);
        }
        if (externalStructure)
        {
            if (structureChanges != null)
            foreach (var update in structureChanges)
            {
                if (update.Removed is SwingPoint old)
                {
                    structureTail.RemoveAll(p => p.Index == old.Index && p.High == old.High);
                    setups.Forget(old.Index);
                }
                Collect(update, CurrentLegStart, frames, output);
                structureTail.Add(update.Point);
                if (structureTail.Count > 4) structureTail.RemoveAt(0);
            }
        }
        else
        {
            int endedLeg = swings.LegStartedAt;
            var change = swings.Add(closed);
            if (change.HasValue) Collect(change.Value, endedLeg, frames, output);
        }
        output.AddRange(setups.Advance(frames));
        return output;
    }

    private void Collect(SwingChange change, int endedLeg, IReadOnlyList<MarketFrame> frames, List<CvdSetup> output)
    {
        var signals = divergences.Offer(change, frames);
        LastCheck = divergences.LastCheck;
        foreach (var signal in signals)
        {
            alertIdentities[signal.Id] = LegIdentity(signal, endedLeg);
            output.AddRange(setups.Offer(signal with { Validation = "Structure CVD confirmée" }, frames));
        }
    }

    public List<(SwingPoint Point, int LegStart)> Candidates(IReadOnlyList<SwingPoint> points,
        IReadOnlyList<MarketFrame> frames = null, double minimumAtr = 0)
    {
        var result = new List<(SwingPoint Point, int LegStart)>();
        if (structureTail.Count == 0) return result;
        var origin = structureTail[^1];
        foreach (var point in points)
        {
            if (point.High == origin.High || point.Index <= origin.Index) continue;
            if (frames != null && frames[point.Index].Atr is double atr
                && Math.Abs(point.Price - origin.Price) < minimumAtr * atr) continue;
            result.Add((point, origin.Index));
            // A window may already outline the next two alternating legs.
            // Observe their chain without storing either provisional point.
            origin = point;
        }
        return result;
    }
}
