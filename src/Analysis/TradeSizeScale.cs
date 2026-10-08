namespace SwingDivergence.Analysis;

/// <summary>Rolling order-volume distribution; growing snapshots replace their own sample.</summary>
internal sealed class TradeSizeScale(int capacity)
{
    private readonly Dictionary<string, double> samples = new();
    private readonly Queue<string> order = new();
    private double sum, squares;
    public int Count => samples.Count;

    public void Record(string key, double volume)
    {
        if (!double.IsFinite(volume) || volume <= 0) return;
        if (samples.TryGetValue(key, out double previous)) { sum -= previous; squares -= previous * previous; }
        else order.Enqueue(key);
        samples[key] = volume; sum += volume; squares += volume * volume;
        while (samples.Count > Math.Max(2, capacity))
        {
            string oldest = order.Dequeue(); double value = samples[oldest];
            samples.Remove(oldest); sum -= value; squares -= value * value;
        }
    }

    public double Upper(double deviations)
    {
        if (samples.Count < 20) return 300;
        double mean = sum / samples.Count;
        double deviation = Math.Sqrt(Math.Max(0, squares / samples.Count - mean * mean));
        return Math.Max(1, mean + Math.Max(0, deviations) * deviation);
    }
}
