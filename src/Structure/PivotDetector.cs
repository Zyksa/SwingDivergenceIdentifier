namespace SwingDivergence.Structure;

internal enum PivotKind { H, L, HH, HL, LH, LL, EQH, EQL }

internal readonly struct Pivot : IEquatable<Pivot>
{
    public int BarIndex { get; init; }
    public int ConfirmedAt { get; init; }
    public double Price { get; init; }
    public bool IsHigh { get; init; }
    public PivotKind Kind { get; init; }

    public Pivot(int BarIndex, int ConfirmedAt, double Price, bool IsHigh, PivotKind Kind)
    {
        this.BarIndex = BarIndex;
        this.ConfirmedAt = ConfirmedAt;
        this.Price = Price;
        this.IsHigh = IsHigh;
        this.Kind = Kind;
    }

    public bool Equals(Pivot other) =>
        EqualityComparer<int>.Default.Equals(BarIndex, other.BarIndex) &&
        EqualityComparer<int>.Default.Equals(ConfirmedAt, other.ConfirmedAt) &&
        EqualityComparer<double>.Default.Equals(Price, other.Price) &&
        EqualityComparer<bool>.Default.Equals(IsHigh, other.IsHigh) &&
        EqualityComparer<PivotKind>.Default.Equals(Kind, other.Kind);

    public override bool Equals(object obj) => obj is Pivot other && Equals(other);
    public static bool operator ==(Pivot left, Pivot right) => left.Equals(right);
    public static bool operator !=(Pivot left, Pivot right) => !left.Equals(right);

    public override int GetHashCode()
    {
        unchecked
        {
            int hash = 17;
            hash = hash * 31 + EqualityComparer<int>.Default.GetHashCode(BarIndex);
            hash = hash * 31 + EqualityComparer<int>.Default.GetHashCode(ConfirmedAt);
            hash = hash * 31 + EqualityComparer<double>.Default.GetHashCode(Price);
            hash = hash * 31 + EqualityComparer<bool>.Default.GetHashCode(IsHigh);
            hash = hash * 31 + EqualityComparer<PivotKind>.Default.GetHashCode(Kind);
            return hash;
        }
    }

    public void Deconstruct(out int BarIndex, out int ConfirmedAt, out double Price, out bool IsHigh, out PivotKind Kind)
    {
        BarIndex = this.BarIndex;
        ConfirmedAt = this.ConfirmedAt;
        Price = this.Price;
        IsHigh = this.IsHigh;
        Kind = this.Kind;
    }

    public override string ToString() => $"Pivot {{ BarIndex = {BarIndex}, ConfirmedAt = {ConfirmedAt}, Price = {Price}, IsHigh = {IsHigh}, Kind = {Kind} }}";
}

internal readonly struct PivotBatch : IEquatable<PivotBatch>
{
    public Pivot? High { get; init; }
    public Pivot? Low { get; init; }

    public PivotBatch(Pivot? High, Pivot? Low)
    {
        this.High = High;
        this.Low = Low;
    }

    public bool Equals(PivotBatch other) =>
        EqualityComparer<Pivot?>.Default.Equals(High, other.High) &&
        EqualityComparer<Pivot?>.Default.Equals(Low, other.Low);

    public override bool Equals(object obj) => obj is PivotBatch other && Equals(other);
    public static bool operator ==(PivotBatch left, PivotBatch right) => left.Equals(right);
    public static bool operator !=(PivotBatch left, PivotBatch right) => !left.Equals(right);

    public override int GetHashCode()
    {
        unchecked
        {
            int hash = 17;
            hash = hash * 31 + EqualityComparer<Pivot?>.Default.GetHashCode(High);
            hash = hash * 31 + EqualityComparer<Pivot?>.Default.GetHashCode(Low);
            return hash;
        }
    }

    public void Deconstruct(out Pivot? High, out Pivot? Low)
    {
        High = this.High;
        Low = this.Low;
    }

    public override string ToString() => $"PivotBatch {{ High = {High}, Low = {Low} }}";
}

/// <summary>
/// Consumes closed bars only. A pivot is emitted once the right-hand window has
/// closed. Equal extrema within a window are resolved in favor of the rightmost bar.
/// </summary>
internal sealed class PivotDetector
{
    private readonly struct Bar : IEquatable<Bar>
    {
        public int Index { get; init; }
        public double High { get; init; }
        public double Low { get; init; }

        public Bar(int Index, double High, double Low)
        {
            this.Index = Index;
            this.High = High;
            this.Low = Low;
        }

        public bool Equals(Bar other) =>
            EqualityComparer<int>.Default.Equals(Index, other.Index) &&
            EqualityComparer<double>.Default.Equals(High, other.High) &&
            EqualityComparer<double>.Default.Equals(Low, other.Low);

        public override bool Equals(object obj) => obj is Bar other && Equals(other);
        public static bool operator ==(Bar left, Bar right) => left.Equals(right);
        public static bool operator !=(Bar left, Bar right) => !left.Equals(right);

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 31 + EqualityComparer<int>.Default.GetHashCode(Index);
                hash = hash * 31 + EqualityComparer<double>.Default.GetHashCode(High);
                hash = hash * 31 + EqualityComparer<double>.Default.GetHashCode(Low);
                return hash;
            }
        }

        public void Deconstruct(out int Index, out double High, out double Low)
        {
            Index = this.Index;
            High = this.High;
            Low = this.Low;
        }

        public override string ToString() => $"Bar {{ Index = {Index}, High = {High}, Low = {Low} }}";
    }
    private readonly Bar[] window;
    private readonly int left, right;
    private readonly double epsilon;
    private int next, count;
    private int? lastIndex;
    private double? previousHigh, previousLow;

    public PivotDetector(int left, int right, double epsilon = 0)
    {
        if (left < 1 || left > 1000)
            throw new ArgumentOutOfRangeException(nameof(left));
        if (right < 1 || right > 1000)
            throw new ArgumentOutOfRangeException(nameof(right));
        this.left = left;
        this.right = right;
        this.epsilon = Math.Max(0, epsilon);
        window = new Bar[left + right + 1];
    }

    public PivotBatch AddClosedBar(int index, double high, double low)
    {
        if (index < 0 || !double.IsFinite(high) || !double.IsFinite(low) || high < low)
            throw new ArgumentException("Invalid closed bar.");
        if (lastIndex == index)
            return default; // Some hosts deliver the same close more than once.
        if (lastIndex.HasValue && index != lastIndex.Value + 1)
            throw new InvalidOperationException("Closed bars must be supplied in order; reset on reload.");

        window[next] = new Bar(index, high, low);
        next = (next + 1) % window.Length;
        count = Math.Min(count + 1, window.Length);
        lastIndex = index;
        if (count < window.Length)
            return default;

        // Once full, 'next' points to the oldest bar; the candidate is 'left' bars after it.
        Bar candidate = At(left);
        bool isHigh = true, isLow = true;
        for (int i = 0; i < left; i++)
        {
            Bar other = At(i);
            if (other.High > candidate.High + epsilon) isHigh = false;
            if (other.Low < candidate.Low - epsilon) isLow = false;
        }
        for (int i = left + 1; i <= left + right; i++)
        {
            Bar other = At(i);
            if (other.High >= candidate.High - epsilon) isHigh = false;
            if (other.Low <= candidate.Low + epsilon) isLow = false;
        }

        Pivot? highPivot = null, lowPivot = null;
        if (isHigh)
        {
            PivotKind kind = !previousHigh.HasValue ? PivotKind.H
                : candidate.High > previousHigh.Value ? PivotKind.HH
                : candidate.High < previousHigh.Value ? PivotKind.LH : PivotKind.EQH;
            highPivot = new Pivot(candidate.Index, index, candidate.High, true, kind);
            previousHigh = candidate.High;
        }
        if (isLow)
        {
            PivotKind kind = !previousLow.HasValue ? PivotKind.L
                : candidate.Low > previousLow.Value ? PivotKind.HL
                : candidate.Low < previousLow.Value ? PivotKind.LL : PivotKind.EQL;
            lowPivot = new Pivot(candidate.Index, index, candidate.Low, false, kind);
            previousLow = candidate.Low;
        }
        return new PivotBatch(highPivot, lowPivot);
    }

    private Bar At(int offset) => window[(next + offset) % window.Length];
}
