namespace SwingDivergence.Analysis;

internal readonly struct DeepTrade : IEquatable<DeepTrade>
{
    public int BarIndex { get; init; }
    public long Time { get; init; }
    public bool Buy { get; init; }
    public double Price { get; init; }
    public double Volume { get; init; }
    public int Prints { get; init; }
    public double Low { get; init; }
    public double High { get; init; }
    public double Threshold { get; init; }
    public double LargestPrint { get; init; }
    public ulong OrderId { get; init; }

    public DeepTrade(int BarIndex, long Time, bool Buy, double Price, double Volume, int Prints, double Low, double High, double Threshold, double LargestPrint = 0, ulong OrderId = 0)
    {
        this.BarIndex = BarIndex;
        this.Time = Time;
        this.Buy = Buy;
        this.Price = Price;
        this.Volume = Volume;
        this.Prints = Prints;
        this.Low = Low;
        this.High = High;
        this.Threshold = Threshold;
        this.LargestPrint = LargestPrint;
        this.OrderId = OrderId;
    }

    public bool Equals(DeepTrade other) =>
        EqualityComparer<int>.Default.Equals(BarIndex, other.BarIndex) &&
        EqualityComparer<long>.Default.Equals(Time, other.Time) &&
        EqualityComparer<bool>.Default.Equals(Buy, other.Buy) &&
        EqualityComparer<double>.Default.Equals(Price, other.Price) &&
        EqualityComparer<double>.Default.Equals(Volume, other.Volume) &&
        EqualityComparer<int>.Default.Equals(Prints, other.Prints) &&
        EqualityComparer<double>.Default.Equals(Low, other.Low) &&
        EqualityComparer<double>.Default.Equals(High, other.High) &&
        EqualityComparer<double>.Default.Equals(Threshold, other.Threshold) &&
        EqualityComparer<double>.Default.Equals(LargestPrint, other.LargestPrint) &&
        EqualityComparer<ulong>.Default.Equals(OrderId, other.OrderId);

    public override bool Equals(object obj) => obj is DeepTrade other && Equals(other);
    public static bool operator ==(DeepTrade left, DeepTrade right) => left.Equals(right);
    public static bool operator !=(DeepTrade left, DeepTrade right) => !left.Equals(right);

    public override int GetHashCode()
    {
        unchecked
        {
            int hash = 17;
            hash = hash * 31 + EqualityComparer<int>.Default.GetHashCode(BarIndex);
            hash = hash * 31 + EqualityComparer<long>.Default.GetHashCode(Time);
            hash = hash * 31 + EqualityComparer<bool>.Default.GetHashCode(Buy);
            hash = hash * 31 + EqualityComparer<double>.Default.GetHashCode(Price);
            hash = hash * 31 + EqualityComparer<double>.Default.GetHashCode(Volume);
            hash = hash * 31 + EqualityComparer<int>.Default.GetHashCode(Prints);
            hash = hash * 31 + EqualityComparer<double>.Default.GetHashCode(Low);
            hash = hash * 31 + EqualityComparer<double>.Default.GetHashCode(High);
            hash = hash * 31 + EqualityComparer<double>.Default.GetHashCode(Threshold);
            hash = hash * 31 + EqualityComparer<double>.Default.GetHashCode(LargestPrint);
            hash = hash * 31 + EqualityComparer<ulong>.Default.GetHashCode(OrderId);
            return hash;
        }
    }

    public void Deconstruct(out int BarIndex, out long Time, out bool Buy, out double Price, out double Volume, out int Prints, out double Low, out double High, out double Threshold, out double LargestPrint, out ulong OrderId)
    {
        BarIndex = this.BarIndex;
        Time = this.Time;
        Buy = this.Buy;
        Price = this.Price;
        Volume = this.Volume;
        Prints = this.Prints;
        Low = this.Low;
        High = this.High;
        Threshold = this.Threshold;
        LargestPrint = this.LargestPrint;
        OrderId = this.OrderId;
    }

    public override string ToString() => $"DeepTrade {{ BarIndex = {BarIndex}, Time = {Time}, Buy = {Buy}, Price = {Price}, Volume = {Volume}, Prints = {Prints}, Low = {Low}, High = {High}, Threshold = {Threshold}, LargestPrint = {LargestPrint}, OrderId = {OrderId} }}";
}

/// <summary>Same-side/time sweep reconstruction and rolling percentile from phidias core.market.</summary>
internal sealed class DeepTradeAggregator(double floor, double percentile, int sampleSize, int windowMs, bool adaptive,
    bool retainBelowThreshold = false)
{
    private readonly Queue<double> sizes = new();
    private int samplesSinceUpdate;
    private double threshold = floor;
    private bool? side;
    private int barIndex, prints;
    private long start, last;
    private long? watermark;
    private double volume, priceVolume, low, high, largestPrint;
    private ulong activeOrderId;

    public double Threshold => threshold;

    public DeepTrade? Snapshot() => side.HasValue ? new DeepTrade(barIndex, start, side.Value, priceVolume / volume,
        volume, prints, low, high, threshold, largestPrint, activeOrderId) : null;

    public DeepTrade? Feed(int index, long time, double price, double size, bool? buy, ulong orderId = 0)
    {
        if (!double.IsFinite(price) || !double.IsFinite(size) || size <= 0) return null;
        if (watermark.HasValue && time < watermark.Value) return null;
        watermark = time;
        bool sameOrder = orderId != 0 || activeOrderId != 0
            ? orderId != 0 && orderId == activeOrderId : time - last <= windowMs;
        if (side.HasValue && buy.HasValue && buy == side && index == barIndex && sameOrder)
        {
            volume += size; priceVolume += price * size; prints++; last = time;
            low = Math.Min(low, price); high = Math.Max(high, price);
            largestPrint = Math.Max(largestPrint, size);
            return null;
        }
        var ended = Flush();
        if (buy.HasValue)
        {
            side = buy; barIndex = index; start = last = time; prints = 1;
            activeOrderId = orderId;
            volume = size; priceVolume = price * size; low = high = price; largestPrint = size;
        }
        return ended;
    }

    public DeepTrade? FlushStale(long time) => side.HasValue && activeOrderId == 0 && time - last > windowMs ? Flush() : null;

    public DeepTrade? Flush()
    {
        if (!side.HasValue) return null;
        bool buy = side.Value; side = null;
        sizes.Enqueue(volume);
        while (sizes.Count > sampleSize) sizes.Dequeue();
        samplesSinceUpdate++;
        if (adaptive && samplesSinceUpdate >= 50 && sizes.Count >= 50)
        {
            samplesSinceUpdate = 0;
            var sorted = sizes.OrderBy(v => v).ToArray();
            double rank = percentile / 100 * (sorted.Length - 1);
            int lo = (int)Math.Floor(rank), hi = Math.Min(lo + 1, sorted.Length - 1);
            threshold = Math.Max(floor, sorted[lo] + (sorted[hi] - sorted[lo]) * (rank - lo));
        }
        return retainBelowThreshold || volume >= threshold ? new DeepTrade(barIndex, start, buy, priceVolume / volume,
            volume, prints, low, high, threshold, largestPrint, activeOrderId) : null;
    }
}
