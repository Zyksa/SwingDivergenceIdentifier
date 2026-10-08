namespace SwingDivergence.Analysis;

internal readonly struct DivergenceOptions : IEquatable<DivergenceOptions>
{
    public bool Cvd { get; init; }
    public bool Technical { get; init; }
    public TechnicalOscillator Oscillator { get; init; }
    public bool CvdClose { get; init; }
    public bool Hidden { get; init; }
    public bool Weak { get; init; }
    public int MaximumGap { get; init; }
    public double PriceEpsilon { get; init; }
    public double CvdEpsilon { get; init; }
    public double OscillatorEpsilon { get; init; }
    public bool ClearPrice { get; init; }
    public bool ClearCvd { get; init; }
    public bool HiddenTrend { get; init; }
    public bool ValidateTechnical { get; init; }
    public int ValidationBars { get; init; }
    public int AlignmentBars { get; init; }

    public DivergenceOptions(bool Cvd, bool Technical, TechnicalOscillator Oscillator, bool CvdClose, bool Hidden, bool Weak, int MaximumGap, double PriceEpsilon, double CvdEpsilon, double OscillatorEpsilon, bool ClearPrice, bool ClearCvd, bool HiddenTrend, bool ValidateTechnical, int ValidationBars, int AlignmentBars)
    {
        this.Cvd = Cvd;
        this.Technical = Technical;
        this.Oscillator = Oscillator;
        this.CvdClose = CvdClose;
        this.Hidden = Hidden;
        this.Weak = Weak;
        this.MaximumGap = MaximumGap;
        this.PriceEpsilon = PriceEpsilon;
        this.CvdEpsilon = CvdEpsilon;
        this.OscillatorEpsilon = OscillatorEpsilon;
        this.ClearPrice = ClearPrice;
        this.ClearCvd = ClearCvd;
        this.HiddenTrend = HiddenTrend;
        this.ValidateTechnical = ValidateTechnical;
        this.ValidationBars = ValidationBars;
        this.AlignmentBars = AlignmentBars;
    }

    public bool Equals(DivergenceOptions other) =>
        EqualityComparer<bool>.Default.Equals(Cvd, other.Cvd) &&
        EqualityComparer<bool>.Default.Equals(Technical, other.Technical) &&
        EqualityComparer<TechnicalOscillator>.Default.Equals(Oscillator, other.Oscillator) &&
        EqualityComparer<bool>.Default.Equals(CvdClose, other.CvdClose) &&
        EqualityComparer<bool>.Default.Equals(Hidden, other.Hidden) &&
        EqualityComparer<bool>.Default.Equals(Weak, other.Weak) &&
        EqualityComparer<int>.Default.Equals(MaximumGap, other.MaximumGap) &&
        EqualityComparer<double>.Default.Equals(PriceEpsilon, other.PriceEpsilon) &&
        EqualityComparer<double>.Default.Equals(CvdEpsilon, other.CvdEpsilon) &&
        EqualityComparer<double>.Default.Equals(OscillatorEpsilon, other.OscillatorEpsilon) &&
        EqualityComparer<bool>.Default.Equals(ClearPrice, other.ClearPrice) &&
        EqualityComparer<bool>.Default.Equals(ClearCvd, other.ClearCvd) &&
        EqualityComparer<bool>.Default.Equals(HiddenTrend, other.HiddenTrend) &&
        EqualityComparer<bool>.Default.Equals(ValidateTechnical, other.ValidateTechnical) &&
        EqualityComparer<int>.Default.Equals(ValidationBars, other.ValidationBars) &&
        EqualityComparer<int>.Default.Equals(AlignmentBars, other.AlignmentBars);

    public override bool Equals(object obj) => obj is DivergenceOptions other && Equals(other);
    public static bool operator ==(DivergenceOptions left, DivergenceOptions right) => left.Equals(right);
    public static bool operator !=(DivergenceOptions left, DivergenceOptions right) => !left.Equals(right);

    public override int GetHashCode()
    {
        unchecked
        {
            int hash = 17;
            hash = hash * 31 + EqualityComparer<bool>.Default.GetHashCode(Cvd);
            hash = hash * 31 + EqualityComparer<bool>.Default.GetHashCode(Technical);
            hash = hash * 31 + EqualityComparer<TechnicalOscillator>.Default.GetHashCode(Oscillator);
            hash = hash * 31 + EqualityComparer<bool>.Default.GetHashCode(CvdClose);
            hash = hash * 31 + EqualityComparer<bool>.Default.GetHashCode(Hidden);
            hash = hash * 31 + EqualityComparer<bool>.Default.GetHashCode(Weak);
            hash = hash * 31 + EqualityComparer<int>.Default.GetHashCode(MaximumGap);
            hash = hash * 31 + EqualityComparer<double>.Default.GetHashCode(PriceEpsilon);
            hash = hash * 31 + EqualityComparer<double>.Default.GetHashCode(CvdEpsilon);
            hash = hash * 31 + EqualityComparer<double>.Default.GetHashCode(OscillatorEpsilon);
            hash = hash * 31 + EqualityComparer<bool>.Default.GetHashCode(ClearPrice);
            hash = hash * 31 + EqualityComparer<bool>.Default.GetHashCode(ClearCvd);
            hash = hash * 31 + EqualityComparer<bool>.Default.GetHashCode(HiddenTrend);
            hash = hash * 31 + EqualityComparer<bool>.Default.GetHashCode(ValidateTechnical);
            hash = hash * 31 + EqualityComparer<int>.Default.GetHashCode(ValidationBars);
            hash = hash * 31 + EqualityComparer<int>.Default.GetHashCode(AlignmentBars);
            return hash;
        }
    }

    public void Deconstruct(out bool Cvd, out bool Technical, out TechnicalOscillator Oscillator, out bool CvdClose, out bool Hidden, out bool Weak, out int MaximumGap, out double PriceEpsilon, out double CvdEpsilon, out double OscillatorEpsilon, out bool ClearPrice, out bool ClearCvd, out bool HiddenTrend, out bool ValidateTechnical, out int ValidationBars, out int AlignmentBars)
    {
        Cvd = this.Cvd;
        Technical = this.Technical;
        Oscillator = this.Oscillator;
        CvdClose = this.CvdClose;
        Hidden = this.Hidden;
        Weak = this.Weak;
        MaximumGap = this.MaximumGap;
        PriceEpsilon = this.PriceEpsilon;
        CvdEpsilon = this.CvdEpsilon;
        OscillatorEpsilon = this.OscillatorEpsilon;
        ClearPrice = this.ClearPrice;
        ClearCvd = this.ClearCvd;
        HiddenTrend = this.HiddenTrend;
        ValidateTechnical = this.ValidateTechnical;
        ValidationBars = this.ValidationBars;
        AlignmentBars = this.AlignmentBars;
    }

    public override string ToString() => $"DivergenceOptions {{ Cvd = {Cvd}, Technical = {Technical}, Oscillator = {Oscillator}, CvdClose = {CvdClose}, Hidden = {Hidden}, Weak = {Weak}, MaximumGap = {MaximumGap}, PriceEpsilon = {PriceEpsilon}, CvdEpsilon = {CvdEpsilon}, OscillatorEpsilon = {OscillatorEpsilon}, ClearPrice = {ClearPrice}, ClearCvd = {ClearCvd}, HiddenTrend = {HiddenTrend}, ValidateTechnical = {ValidateTechnical}, ValidationBars = {ValidationBars}, AlignmentBars = {AlignmentBars} }}";
}

internal readonly struct DivergenceSignal : IEquatable<DivergenceSignal>
{
    public string Id { get; init; }
    public string Source { get; init; }
    public string Kind { get; init; }
    public SwingPoint First { get; init; }
    public SwingPoint Second { get; init; }
    public double Value1 { get; init; }
    public double Value2 { get; init; }
    public bool Bullish { get; init; }
    public bool Hidden { get; init; }
    public bool Weak { get; init; }
    public int ConfirmedAt { get; init; }
    public string Validation { get; init; }
    public int ValueIndex1 { get; init; }
    public int ValueIndex2 { get; init; }

    public DivergenceSignal(string Id, string Source, string Kind, SwingPoint First, SwingPoint Second, double Value1, double Value2, bool Bullish, bool Hidden, bool Weak, int ConfirmedAt, string Validation, int ValueIndex1 = -1, int ValueIndex2 = -1)
    {
        this.Id = Id;
        this.Source = Source;
        this.Kind = Kind;
        this.First = First;
        this.Second = Second;
        this.Value1 = Value1;
        this.Value2 = Value2;
        this.Bullish = Bullish;
        this.Hidden = Hidden;
        this.Weak = Weak;
        this.ConfirmedAt = ConfirmedAt;
        this.Validation = Validation;
        this.ValueIndex1 = ValueIndex1;
        this.ValueIndex2 = ValueIndex2;
    }

    public bool Equals(DivergenceSignal other) =>
        EqualityComparer<string>.Default.Equals(Id, other.Id) &&
        EqualityComparer<string>.Default.Equals(Source, other.Source) &&
        EqualityComparer<string>.Default.Equals(Kind, other.Kind) &&
        EqualityComparer<SwingPoint>.Default.Equals(First, other.First) &&
        EqualityComparer<SwingPoint>.Default.Equals(Second, other.Second) &&
        EqualityComparer<double>.Default.Equals(Value1, other.Value1) &&
        EqualityComparer<double>.Default.Equals(Value2, other.Value2) &&
        EqualityComparer<bool>.Default.Equals(Bullish, other.Bullish) &&
        EqualityComparer<bool>.Default.Equals(Hidden, other.Hidden) &&
        EqualityComparer<bool>.Default.Equals(Weak, other.Weak) &&
        EqualityComparer<int>.Default.Equals(ConfirmedAt, other.ConfirmedAt) &&
        EqualityComparer<string>.Default.Equals(Validation, other.Validation) &&
        EqualityComparer<int>.Default.Equals(ValueIndex1, other.ValueIndex1) &&
        EqualityComparer<int>.Default.Equals(ValueIndex2, other.ValueIndex2);

    public override bool Equals(object obj) => obj is DivergenceSignal other && Equals(other);
    public static bool operator ==(DivergenceSignal left, DivergenceSignal right) => left.Equals(right);
    public static bool operator !=(DivergenceSignal left, DivergenceSignal right) => !left.Equals(right);

    public override int GetHashCode()
    {
        unchecked
        {
            int hash = 17;
            hash = hash * 31 + EqualityComparer<string>.Default.GetHashCode(Id);
            hash = hash * 31 + EqualityComparer<string>.Default.GetHashCode(Source);
            hash = hash * 31 + EqualityComparer<string>.Default.GetHashCode(Kind);
            hash = hash * 31 + EqualityComparer<SwingPoint>.Default.GetHashCode(First);
            hash = hash * 31 + EqualityComparer<SwingPoint>.Default.GetHashCode(Second);
            hash = hash * 31 + EqualityComparer<double>.Default.GetHashCode(Value1);
            hash = hash * 31 + EqualityComparer<double>.Default.GetHashCode(Value2);
            hash = hash * 31 + EqualityComparer<bool>.Default.GetHashCode(Bullish);
            hash = hash * 31 + EqualityComparer<bool>.Default.GetHashCode(Hidden);
            hash = hash * 31 + EqualityComparer<bool>.Default.GetHashCode(Weak);
            hash = hash * 31 + EqualityComparer<int>.Default.GetHashCode(ConfirmedAt);
            hash = hash * 31 + EqualityComparer<string>.Default.GetHashCode(Validation);
            hash = hash * 31 + EqualityComparer<int>.Default.GetHashCode(ValueIndex1);
            hash = hash * 31 + EqualityComparer<int>.Default.GetHashCode(ValueIndex2);
            return hash;
        }
    }

    public void Deconstruct(out string Id, out string Source, out string Kind, out SwingPoint First, out SwingPoint Second, out double Value1, out double Value2, out bool Bullish, out bool Hidden, out bool Weak, out int ConfirmedAt, out string Validation, out int ValueIndex1, out int ValueIndex2)
    {
        Id = this.Id;
        Source = this.Source;
        Kind = this.Kind;
        First = this.First;
        Second = this.Second;
        Value1 = this.Value1;
        Value2 = this.Value2;
        Bullish = this.Bullish;
        Hidden = this.Hidden;
        Weak = this.Weak;
        ConfirmedAt = this.ConfirmedAt;
        Validation = this.Validation;
        ValueIndex1 = this.ValueIndex1;
        ValueIndex2 = this.ValueIndex2;
    }

    public override string ToString() => $"DivergenceSignal {{ Id = {Id}, Source = {Source}, Kind = {Kind}, First = {First}, Second = {Second}, Value1 = {Value1}, Value2 = {Value2}, Bullish = {Bullish}, Hidden = {Hidden}, Weak = {Weak}, ConfirmedAt = {ConfirmedAt}, Validation = {Validation}, ValueIndex1 = {ValueIndex1}, ValueIndex2 = {ValueIndex2} }}";
}

internal sealed class Divergences(DivergenceOptions options)
{
    internal const int MaximumPivotDistance = 12;
    private readonly List<SwingPoint> highs = new(), lows = new();
    private readonly List<DivergenceSignal> pending = new();
    public string LastCheck { get; private set; } = "En attente d'ancres de structure.";

    public List<DivergenceSignal> Offer(SwingChange change, IReadOnlyList<MarketFrame> frames)
    {
        var points = change.Point.High ? highs : lows;
        if (change.Removed is SwingPoint old)
        {
            points.RemoveAll(p => p.Index == old.Index);
            pending.RemoveAll(s => s.First.Index == old.Index || s.Second.Index == old.Index);
        }
        var output = new List<DivergenceSignal>();
        if (points.Count > 0)
        {
            var first = points[^1]; var second = change.Point;
            if (second.Index > first.Index && second.Index - first.Index <= Math.Clamp(options.MaximumGap, 1, MaximumPivotDistance)
                && frames[first.Index].Session == frames[second.Index].Session)
            {
                if (options.Cvd) Check("CVD", first, second, frames, output);
                if (options.Technical)
                    Check(options.Oscillator == TechnicalOscillator.Rsi ? "RSI" : "MACD", first, second, frames, output);
            }
        }
        points.Add(change.Point);
        if (points.Count > 3) points.RemoveAt(0);
        return output;
    }

    // Non-mutating CVD comparison: a live candidate never becomes a confirmed anchor.
    public List<DivergenceSignal> Preview(SwingPoint candidate, IReadOnlyList<MarketFrame> frames)
    {
        var output = new List<DivergenceSignal>();
        if (!options.Cvd) return output;
        var points = candidate.High ? highs : lows;
        if (points.Count == 0) { LastCheck = "Aucun point précédent de même type confirmé."; return output; }
        var first = points[^1];
        if (candidate.Index <= first.Index || candidate.Index - first.Index > Math.Clamp(options.MaximumGap, 1, MaximumPivotDistance)
            || frames[first.Index].Session != frames[candidate.Index].Session)
        { LastCheck = "Ancres hors session, dans le mauvais ordre ou au-delà de l'écart maximal."; return output; }
        Check("CVD", first, candidate, frames, output);
        for (int i = 0; i < output.Count; i++) output[i] = output[i] with { Validation = "CVD en formation" };
        return output;
    }

    private void Check(string source, SwingPoint first, SwingPoint second,
        IReadOnlyList<MarketFrame> frames, List<DivergenceSignal> output)
    {
        var sample1 = Sample(source, first, frames); var sample2 = Sample(source, second, frames);
        if (!sample1.HasValue || !sample2.HasValue || sample2.Value.Index <= sample1.Value.Index
            || sample2.Value.Index - sample1.Value.Index > Math.Clamp(options.MaximumGap, 1, MaximumPivotDistance))
        { LastCheck = $"Mesures {source} indisponibles ou ancres hors de l'écart maximal."; return; }
        double value1 = sample1.Value.Value, value2 = sample2.Value.Value;
        double eps = source == "CVD" ? options.CvdEpsilon : options.OscillatorEpsilon;
        string kind = Classify(second.High, second.Price - first.Price, value2 - value1,
            options.PriceEpsilon, eps, options.Weak && source == "CVD");
        if (kind == null) { LastCheck = "Prix et CVD sans mouvement opposé suffisant (tolérances)."; return; }
        bool hidden = kind.StartsWith("Hidden");
        if (hidden && !options.Hidden) { LastCheck = "Absorptions / divergences cachées désactivées."; return; }
        if (hidden && source != "CVD" && options.HiddenTrend)
        {
            var a = frames[first.Index]; var b = frames[second.Index];
            if (!a.Trend.HasValue || !b.Trend.HasValue) return;
            if (second.High ? b.Trend >= a.Trend || b.Close > b.Trend : b.Trend <= a.Trend || b.Close < b.Trend) return;
        }
        if (options.ClearPrice && !ClearLine(first, second, first.Price, second.Price, frames, false, options.PriceEpsilon))
        { LastCheck = "Ligne de prix traversée par une bougie intermédiaire (mèches comprises)."; return; }
        if (source == "CVD" && options.ClearCvd && !ClearLine(first with { Index = sample1.Value.Index },
            second with { Index = sample2.Value.Index }, value1, value2, frames, true, eps))
        { LastCheck = "Ligne CVD traversée par une bougie intermédiaire ou données CVD manquantes."; return; }
        LastCheck = $"{(hidden ? "ABS" : "EXH")} {(second.High ? "baissière" : "haussière")} admissible.";
        bool weak = Math.Abs(second.Price - first.Price) <= options.PriceEpsilon || Math.Abs(value2 - value1) <= eps;
        var signal = new DivergenceSignal($"{source}:{(second.High ? "H" : "L")}:{kind}:{first.Index}:{second.Index}", source, kind,
            first, second, value1, value2, !second.High, hidden, weak, second.ConfirmedAt, "Pivots confirmés",
            sample1.Value.Index, sample2.Value.Index);
        if (source != "CVD" && options.ValidateTechnical)
        {
            var validated = Validate(signal, frames);
            if (validated.HasValue) output.Add(validated.Value);
            else if (frames.Count - 1 <= second.Index + options.ValidationBars) pending.Add(signal);
        }
        else output.Add(signal);
    }

    public List<DivergenceSignal> Advance(IReadOnlyList<MarketFrame> frames)
    {
        var output = new List<DivergenceSignal>();
        for (int i = pending.Count - 1; i >= 0; i--)
        {
            var signal = pending[i];
            var validated = Validate(signal, frames);
            if (validated.HasValue) { output.Add(validated.Value); pending.RemoveAt(i); }
            else if (frames.Count - 1 > signal.Second.Index + options.ValidationBars) pending.RemoveAt(i);
        }
        return output;
    }

    private DivergenceSignal? Validate(DivergenceSignal signal, IReadOnlyList<MarketFrame> frames)
    {
        var second = signal.Second; var first = signal.First;
        var pivotBar = frames[second.Index];
        bool sweep = second.High ? second.Price > first.Price + options.PriceEpsilon && pivotBar.Close < first.Price
            : second.Price < first.Price - options.PriceEpsilon && pivotBar.Close > first.Price;
        if (sweep) return signal with { Validation = "Balayage de liquidité", ConfirmedAt = second.ConfirmedAt };
        double level = second.High ? double.PositiveInfinity : double.NegativeInfinity;
        for (int i = first.Index + 1; i < second.Index; i++)
            level = second.High ? Math.Min(level, frames[i].Low) : Math.Max(level, frames[i].High);
        if (!double.IsFinite(level)) return null;
        int end = Math.Min(frames.Count - 1, second.Index + options.ValidationBars);
        for (int i = second.Index + 1; i <= end; i++)
        {
            double buffer = Math.Max(options.PriceEpsilon, (frames[i].Atr ?? 0) * 0.05);
            if (second.High ? frames[i].Close < level - buffer : frames[i].Close > level + buffer)
                return signal with { Validation = "BOS confirmé", ConfirmedAt = Math.Max(i, second.ConfirmedAt) };
        }
        return null;
    }

    private (int Index, double Value)? Sample(string source, SwingPoint point, IReadOnlyList<MarketFrame> frames)
    {
        var frame = frames[point.Index];
        if (source == "CVD" && (options.CvdClose || point.CloseSeed))
            return frame.CvdClose.HasValue && double.IsFinite(frame.CvdClose.Value) ? (point.Index, frame.CvdClose.Value) : null;
        double? best = null;
        int bestIndex = -1;
        int end = Math.Min(point.ConfirmedAt, Math.Min(frames.Count - 1, point.Index + options.AlignmentBars));
        for (int i = Math.Max(0, point.Index - options.AlignmentBars); i <= end; i++)
        {
            if (frames[i].Session != frame.Session) continue;
            double? sample = source == "CVD" ? point.High ? frames[i].CvdHigh : frames[i].CvdLow
                : source == "RSI" ? frames[i].Rsi : frames[i].Macd;
            if (sample.HasValue && double.IsFinite(sample.Value) && (!best.HasValue || (point.High ? sample > best : sample < best)
                || sample == best && Math.Abs(i - point.Index) <= Math.Abs(bestIndex - point.Index)))
            { best = sample; bestIndex = i; }
        }
        return best.HasValue ? (bestIndex, best.Value) : null;
    }

    internal static string Classify(bool high, double priceChange, double valueChange, double priceEps, double valueEps, bool weak)
    {
        bool pu = priceChange > priceEps, pd = priceChange < -priceEps;
        bool vu = valueChange > valueEps, vd = valueChange < -valueEps;
        if (high ? pu && vd : pd && vu) return "Regular";
        if (high ? pd && vu : pu && vd) return "Hidden";
        if (!weak || (!pu && !pd && !vu && !vd)) return null;
        if (!pu && !pd) return high ? vd ? "Regular" : vu ? "Hidden" : null : vu ? "Regular" : vd ? "Hidden" : null;
        if (!vu && !vd) return high ? pu ? "Regular" : pd ? "Hidden" : null : pd ? "Regular" : pu ? "Hidden" : null;
        return null;
    }

    internal static bool ClearLine(SwingPoint first, SwingPoint second, double value1, double value2,
        IReadOnlyList<MarketFrame> frames, bool cvd, double epsilon)
    {
        for (int i = first.Index + 1; i < second.Index; i++)
        {
            double? low = cvd ? frames[i].CvdLow : frames[i].Low;
            double? high = cvd ? frames[i].CvdHigh : frames[i].High;
            if (!low.HasValue || !high.HasValue || !double.IsFinite(low.Value) || !double.IsFinite(high.Value)) return false;
            double value = value1 + (value2 - value1) * (i - first.Index) / (second.Index - first.Index);
            if (low.Value + epsilon < value && value < high.Value - epsilon) return false;
        }
        return true;
    }
}
