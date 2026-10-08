namespace SwingDivergence.Analysis;

internal readonly struct CachedTrade : IEquatable<CachedTrade>
{
    public string Key { get; init; }
    public long BarTime { get; init; }
    public DeepTrade Trade { get; init; }

    public CachedTrade(string Key, long BarTime, DeepTrade Trade)
    {
        this.Key = Key;
        this.BarTime = BarTime;
        this.Trade = Trade;
    }

    public bool Equals(CachedTrade other) =>
        EqualityComparer<string>.Default.Equals(Key, other.Key) &&
        EqualityComparer<long>.Default.Equals(BarTime, other.BarTime) &&
        EqualityComparer<DeepTrade>.Default.Equals(Trade, other.Trade);

    public override bool Equals(object obj) => obj is CachedTrade other && Equals(other);
    public static bool operator ==(CachedTrade left, CachedTrade right) => left.Equals(right);
    public static bool operator !=(CachedTrade left, CachedTrade right) => !left.Equals(right);

    public override int GetHashCode()
    {
        unchecked
        {
            int hash = 17;
            hash = hash * 31 + EqualityComparer<string>.Default.GetHashCode(Key);
            hash = hash * 31 + EqualityComparer<long>.Default.GetHashCode(BarTime);
            hash = hash * 31 + EqualityComparer<DeepTrade>.Default.GetHashCode(Trade);
            return hash;
        }
    }

    public void Deconstruct(out string Key, out long BarTime, out DeepTrade Trade)
    {
        Key = this.Key;
        BarTime = this.BarTime;
        Trade = this.Trade;
    }

    public override string ToString() => $"CachedTrade {{ Key = {Key}, BarTime = {BarTime}, Trade = {Trade} }}";
}

/// <summary>Instance-local executions survive parameter recalculation, including filtered-out orders.</summary>
internal sealed class TradeHistoryCache(int capacity = 20000)
{
    private string context;
    private readonly Dictionary<string, CachedTrade> orders = new();
    private readonly Queue<string> insertionOrder = new();
    public IEnumerable<CachedTrade> Values => orders.Values;

    public void ResetContext(string next)
    {
        if (next == context) return;
        orders.Clear(); insertionOrder.Clear(); context = next;
    }

    public void Remember(string key, long barTime, DeepTrade trade)
    {
        if (!orders.ContainsKey(key)) insertionOrder.Enqueue(key);
        orders[key] = new CachedTrade(key, barTime, trade);
        while (orders.Count > capacity) orders.Remove(insertionOrder.Dequeue());
    }
}
