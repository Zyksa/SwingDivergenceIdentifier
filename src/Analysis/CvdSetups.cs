namespace SwingDivergence.Analysis;

internal readonly struct SetupOptions : IEquatable<SetupOptions>
{
    public SetupConfirmation Confirmation { get; init; }
    public double RewardRisk { get; init; }
    public double StopAtr { get; init; }
    public double MinimumStop { get; init; }
    public double TickSize { get; init; }
    public double PriceEpsilon { get; init; }
    public int WaitingBars { get; init; }
    public bool Bullish { get; init; }
    public bool Bearish { get; init; }
    public bool Absorption { get; init; }
    public bool Exhaustion { get; init; }

    public SetupOptions(SetupConfirmation Confirmation, double RewardRisk, double StopAtr, double MinimumStop, double TickSize, double PriceEpsilon, int WaitingBars, bool Bullish, bool Bearish, bool Absorption, bool Exhaustion)
    {
        this.Confirmation = Confirmation;
        this.RewardRisk = RewardRisk;
        this.StopAtr = StopAtr;
        this.MinimumStop = MinimumStop;
        this.TickSize = TickSize;
        this.PriceEpsilon = PriceEpsilon;
        this.WaitingBars = WaitingBars;
        this.Bullish = Bullish;
        this.Bearish = Bearish;
        this.Absorption = Absorption;
        this.Exhaustion = Exhaustion;
    }

    public bool Equals(SetupOptions other) =>
        EqualityComparer<SetupConfirmation>.Default.Equals(Confirmation, other.Confirmation) &&
        EqualityComparer<double>.Default.Equals(RewardRisk, other.RewardRisk) &&
        EqualityComparer<double>.Default.Equals(StopAtr, other.StopAtr) &&
        EqualityComparer<double>.Default.Equals(MinimumStop, other.MinimumStop) &&
        EqualityComparer<double>.Default.Equals(TickSize, other.TickSize) &&
        EqualityComparer<double>.Default.Equals(PriceEpsilon, other.PriceEpsilon) &&
        EqualityComparer<int>.Default.Equals(WaitingBars, other.WaitingBars) &&
        EqualityComparer<bool>.Default.Equals(Bullish, other.Bullish) &&
        EqualityComparer<bool>.Default.Equals(Bearish, other.Bearish) &&
        EqualityComparer<bool>.Default.Equals(Absorption, other.Absorption) &&
        EqualityComparer<bool>.Default.Equals(Exhaustion, other.Exhaustion);

    public override bool Equals(object obj) => obj is SetupOptions other && Equals(other);
    public static bool operator ==(SetupOptions left, SetupOptions right) => left.Equals(right);
    public static bool operator !=(SetupOptions left, SetupOptions right) => !left.Equals(right);

    public override int GetHashCode()
    {
        unchecked
        {
            int hash = 17;
            hash = hash * 31 + EqualityComparer<SetupConfirmation>.Default.GetHashCode(Confirmation);
            hash = hash * 31 + EqualityComparer<double>.Default.GetHashCode(RewardRisk);
            hash = hash * 31 + EqualityComparer<double>.Default.GetHashCode(StopAtr);
            hash = hash * 31 + EqualityComparer<double>.Default.GetHashCode(MinimumStop);
            hash = hash * 31 + EqualityComparer<double>.Default.GetHashCode(TickSize);
            hash = hash * 31 + EqualityComparer<double>.Default.GetHashCode(PriceEpsilon);
            hash = hash * 31 + EqualityComparer<int>.Default.GetHashCode(WaitingBars);
            hash = hash * 31 + EqualityComparer<bool>.Default.GetHashCode(Bullish);
            hash = hash * 31 + EqualityComparer<bool>.Default.GetHashCode(Bearish);
            hash = hash * 31 + EqualityComparer<bool>.Default.GetHashCode(Absorption);
            hash = hash * 31 + EqualityComparer<bool>.Default.GetHashCode(Exhaustion);
            return hash;
        }
    }

    public void Deconstruct(out SetupConfirmation Confirmation, out double RewardRisk, out double StopAtr, out double MinimumStop, out double TickSize, out double PriceEpsilon, out int WaitingBars, out bool Bullish, out bool Bearish, out bool Absorption, out bool Exhaustion)
    {
        Confirmation = this.Confirmation;
        RewardRisk = this.RewardRisk;
        StopAtr = this.StopAtr;
        MinimumStop = this.MinimumStop;
        TickSize = this.TickSize;
        PriceEpsilon = this.PriceEpsilon;
        WaitingBars = this.WaitingBars;
        Bullish = this.Bullish;
        Bearish = this.Bearish;
        Absorption = this.Absorption;
        Exhaustion = this.Exhaustion;
    }

    public override string ToString() => $"SetupOptions {{ Confirmation = {Confirmation}, RewardRisk = {RewardRisk}, StopAtr = {StopAtr}, MinimumStop = {MinimumStop}, TickSize = {TickSize}, PriceEpsilon = {PriceEpsilon}, WaitingBars = {WaitingBars}, Bullish = {Bullish}, Bearish = {Bearish}, Absorption = {Absorption}, Exhaustion = {Exhaustion} }}";
}

internal readonly struct CvdSetup : IEquatable<CvdSetup>
{
    public DivergenceSignal Signal { get; init; }
    public int AvailableAt { get; init; }
    public double Entry { get; init; }
    public double Stop { get; init; }
    public double Target { get; init; }
    public double RewardRisk { get; init; }
    public string Confirmation { get; init; }

    public CvdSetup(DivergenceSignal Signal, int AvailableAt, double Entry, double Stop, double Target, double RewardRisk, string Confirmation)
    {
        this.Signal = Signal;
        this.AvailableAt = AvailableAt;
        this.Entry = Entry;
        this.Stop = Stop;
        this.Target = Target;
        this.RewardRisk = RewardRisk;
        this.Confirmation = Confirmation;
    }

    public bool Equals(CvdSetup other) =>
        EqualityComparer<DivergenceSignal>.Default.Equals(Signal, other.Signal) &&
        EqualityComparer<int>.Default.Equals(AvailableAt, other.AvailableAt) &&
        EqualityComparer<double>.Default.Equals(Entry, other.Entry) &&
        EqualityComparer<double>.Default.Equals(Stop, other.Stop) &&
        EqualityComparer<double>.Default.Equals(Target, other.Target) &&
        EqualityComparer<double>.Default.Equals(RewardRisk, other.RewardRisk) &&
        EqualityComparer<string>.Default.Equals(Confirmation, other.Confirmation);

    public override bool Equals(object obj) => obj is CvdSetup other && Equals(other);
    public static bool operator ==(CvdSetup left, CvdSetup right) => left.Equals(right);
    public static bool operator !=(CvdSetup left, CvdSetup right) => !left.Equals(right);

    public override int GetHashCode()
    {
        unchecked
        {
            int hash = 17;
            hash = hash * 31 + EqualityComparer<DivergenceSignal>.Default.GetHashCode(Signal);
            hash = hash * 31 + EqualityComparer<int>.Default.GetHashCode(AvailableAt);
            hash = hash * 31 + EqualityComparer<double>.Default.GetHashCode(Entry);
            hash = hash * 31 + EqualityComparer<double>.Default.GetHashCode(Stop);
            hash = hash * 31 + EqualityComparer<double>.Default.GetHashCode(Target);
            hash = hash * 31 + EqualityComparer<double>.Default.GetHashCode(RewardRisk);
            hash = hash * 31 + EqualityComparer<string>.Default.GetHashCode(Confirmation);
            return hash;
        }
    }

    public void Deconstruct(out DivergenceSignal Signal, out int AvailableAt, out double Entry, out double Stop, out double Target, out double RewardRisk, out string Confirmation)
    {
        Signal = this.Signal;
        AvailableAt = this.AvailableAt;
        Entry = this.Entry;
        Stop = this.Stop;
        Target = this.Target;
        RewardRisk = this.RewardRisk;
        Confirmation = this.Confirmation;
    }

    public override string ToString() => $"CvdSetup {{ Signal = {Signal}, AvailableAt = {AvailableAt}, Entry = {Entry}, Stop = {Stop}, Target = {Target}, RewardRisk = {RewardRisk}, Confirmation = {Confirmation} }}";
}

internal sealed class CvdSetups(SetupOptions options)
{
    private readonly List<DivergenceSignal> waiting = new();
    private readonly HashSet<string> seen = new();

    public List<CvdSetup> Offer(DivergenceSignal signal, IReadOnlyList<MarketFrame> frames)
    {
        var output = new List<CvdSetup>();
        if (signal.Weak || signal.Validation == "CVD en formation" || signal.Validation == StructuralCvdSetups.EarlyValidation
            || signal.First.Index < 0 || signal.Second.Index >= frames.Count
            || signal.ConfirmedAt <= signal.Second.Index
            || !double.IsFinite(signal.Value1) || !double.IsFinite(signal.Value2)) return output;
        if (signal.Second.Index - signal.First.Index > Divergences.MaximumPivotDistance) return output;
        if (signal.Source != "CVD" || (signal.Bullish ? !options.Bullish : !options.Bearish)
            || (signal.Hidden ? !options.Absorption : !options.Exhaustion) || !seen.Add(signal.Id)) return output;
        var confirmed = TryConfirm(signal, frames);
        if (confirmed.HasValue) output.Add(confirmed.Value);
        else if (frames.Count - 1 <= signal.ConfirmedAt + options.WaitingBars) waiting.Add(signal);
        return output;
    }

    public CvdSetup? Preview(DivergenceSignal signal, IReadOnlyList<MarketFrame> frames)
    {
        if (signal.Source != "CVD" || signal.Weak || signal.First.Index < 0 || signal.Second.Index >= frames.Count
            || signal.Second.Index <= signal.First.Index || signal.Second.Index - signal.First.Index > Divergences.MaximumPivotDistance
            || !double.IsFinite(signal.Value1) || !double.IsFinite(signal.Value2)
            || (signal.Bullish ? !options.Bullish : !options.Bearish)
            || (signal.Hidden ? !options.Absorption : !options.Exhaustion)) return null;
        return TryConfirm(signal, frames);
    }

    public List<CvdSetup> PreviewPending(IReadOnlyList<MarketFrame> frames)
    {
        var result = new List<CvdSetup>();
        foreach (var signal in waiting)
        {
            if (frames[^1].Session != frames[signal.Second.Index].Session) continue;
            var candidate = TryConfirm(signal, frames);
            if (candidate.HasValue && candidate.Value.AvailableAt == frames.Count - 1)
                result.Add(candidate.Value);
        }
        return result;
    }

    public List<CvdSetup> Advance(IReadOnlyList<MarketFrame> frames)
    {
        var output = new List<CvdSetup>();
        for (int i = waiting.Count - 1; i >= 0; i--)
        {
            var signal = waiting[i];
            var setup = TryConfirm(signal, frames);
            if (setup.HasValue) { output.Add(setup.Value); waiting.RemoveAt(i); }
            else if (frames.Count - 1 > signal.ConfirmedAt + options.WaitingBars
                || frames[^1].Session != frames[signal.Second.Index].Session)
                waiting.RemoveAt(i);
        }
        return output;
    }

    public void Forget(int index)
    {
        waiting.RemoveAll(s => s.First.Index == index || s.Second.Index == index);
        // Already reported identities remain consumed; a window-pivot rewrite cannot re-alert an old setup.
    }

    private CvdSetup? TryConfirm(DivergenceSignal signal, IReadOnlyList<MarketFrame> frames)
    {
        if (signal.ConfirmedAt >= frames.Count) return null;
        int at = signal.ConfirmedAt;
        string reason = "Setup Div Cvd validé";
        if (options.Confirmation == SetupConfirmation.BosOuBalayage)
        {
            var pivotBar = frames[signal.Second.Index];
            bool sweep = signal.Bullish
                ? signal.Second.Price < signal.First.Price - options.PriceEpsilon && pivotBar.Close > signal.First.Price
                : signal.Second.Price > signal.First.Price + options.PriceEpsilon && pivotBar.Close < signal.First.Price;
            if (sweep) reason = "Balayage de liquidité";
            else
            {
                double level = signal.Bullish ? double.NegativeInfinity : double.PositiveInfinity;
                for (int i = signal.First.Index + 1; i < signal.Second.Index; i++)
                    level = signal.Bullish ? Math.Max(level, frames[i].High) : Math.Min(level, frames[i].Low);
                if (!double.IsFinite(level)) return null;
                at = -1;
                int end = Math.Min(frames.Count - 1, signal.ConfirmedAt + options.WaitingBars);
                for (int i = signal.Second.Index + 1; i <= end; i++)
                {
                    if (frames[i].Session != frames[signal.Second.Index].Session) break;
                    double buffer = Math.Max(options.PriceEpsilon, (frames[i].Atr ?? 0) * 0.05);
                    if (signal.Bullish ? frames[i].Close > level + buffer : frames[i].Close < level - buffer)
                    { at = Math.Max(i, signal.ConfirmedAt); break; }
                }
                if (at < 0) return null;
                reason = "BOS confirmé";
            }
        }
        double entry = frames[at].Close;
        double bufferStop = Math.Max(options.MinimumStop, (frames[signal.Second.Index].Atr ?? 0) * options.StopAtr);
        double stop = signal.Second.Price + (signal.Bullish ? -bufferStop : bufferStop);
        if (options.TickSize > 0)
            stop = (signal.Bullish ? Math.Floor(stop / options.TickSize) : Math.Ceiling(stop / options.TickSize)) * options.TickSize;
        double risk = signal.Bullish ? entry - stop : stop - entry;
        // Alert eligibility follows the structural setup, not a projected trade plan.
        if (!double.IsFinite(risk) || risk <= 0)
        {
            if (options.Confirmation == SetupConfirmation.BosOuBalayage) return null;
            return new CvdSetup(signal, at, entry, stop, double.NaN, options.RewardRisk, reason);
        }
        double target = entry + (signal.Bullish ? 1 : -1) * risk * options.RewardRisk;
        if (options.TickSize > 0) target = Math.Round(target / options.TickSize) * options.TickSize;
        return new CvdSetup(signal, at, entry, stop, target, options.RewardRisk, reason);
    }
}

internal sealed class CvdAlertGate
{
    private readonly HashSet<string> seen = new();
    private DateTime? lastSound;
    public bool Accept(string identity, bool live, bool replay, bool allowReplay, int availableAt,
        int latestClosed, DateTime now, int cooldownSeconds)
    {
        if (!seen.Add(identity)) return false;
        if (!live || replay && !allowReplay || availableAt != latestClosed) return false;
        if (lastSound.HasValue && now >= lastSound.Value && (now - lastSound.Value).TotalSeconds < cooldownSeconds) return false;
        lastSound = now;
        return true;
    }
}
