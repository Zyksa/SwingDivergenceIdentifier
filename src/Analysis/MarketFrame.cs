namespace SwingDivergence.Analysis;

public enum StructureDetection { AtrRetracement, FilteredPivots, ReactiveSwings, AlternatingLegs }
public enum TechnicalOscillator { Rsi, MacdHistogram }
public enum BubbleDrawing { PixelCircle, OpticalGlyph }
public enum DeepTradeSource { AgregatsReconstitues, AgregatsSDK }
public enum TradeFilterMode { Manuel, Percentile }
public enum DivergenceSource { Cvd, Rsi, Macd, CvdEtRsi, CvdEtMacd }
public enum SetupConfirmation { DivergenceConfirmee, BosOuBalayage }
public enum CvdAlertTiming { DivergenceDetectee, SetupConfirme, DivergenceEnFormation }

internal readonly struct MarketFrame : IEquatable<MarketFrame>
{
    public int Index { get; init; }
    public int Session { get; init; }
    public double Open { get; init; }
    public double High { get; init; }
    public double Low { get; init; }
    public double Close { get; init; }
    public double Volume { get; init; }
    public double? CvdOpen { get; init; }
    public double? CvdHigh { get; init; }
    public double? CvdLow { get; init; }
    public double? CvdClose { get; init; }
    public double? Atr { get; init; }
    public double? Rsi { get; init; }
    public double? Macd { get; init; }
    public double? Trend { get; init; }

    public MarketFrame(int Index, int Session, double Open, double High, double Low, double Close, double Volume, double? CvdOpen, double? CvdHigh, double? CvdLow, double? CvdClose, double? Atr, double? Rsi, double? Macd, double? Trend)
    {
        this.Index = Index;
        this.Session = Session;
        this.Open = Open;
        this.High = High;
        this.Low = Low;
        this.Close = Close;
        this.Volume = Volume;
        this.CvdOpen = CvdOpen;
        this.CvdHigh = CvdHigh;
        this.CvdLow = CvdLow;
        this.CvdClose = CvdClose;
        this.Atr = Atr;
        this.Rsi = Rsi;
        this.Macd = Macd;
        this.Trend = Trend;
    }

    public bool Equals(MarketFrame other) =>
        EqualityComparer<int>.Default.Equals(Index, other.Index) &&
        EqualityComparer<int>.Default.Equals(Session, other.Session) &&
        EqualityComparer<double>.Default.Equals(Open, other.Open) &&
        EqualityComparer<double>.Default.Equals(High, other.High) &&
        EqualityComparer<double>.Default.Equals(Low, other.Low) &&
        EqualityComparer<double>.Default.Equals(Close, other.Close) &&
        EqualityComparer<double>.Default.Equals(Volume, other.Volume) &&
        EqualityComparer<double?>.Default.Equals(CvdOpen, other.CvdOpen) &&
        EqualityComparer<double?>.Default.Equals(CvdHigh, other.CvdHigh) &&
        EqualityComparer<double?>.Default.Equals(CvdLow, other.CvdLow) &&
        EqualityComparer<double?>.Default.Equals(CvdClose, other.CvdClose) &&
        EqualityComparer<double?>.Default.Equals(Atr, other.Atr) &&
        EqualityComparer<double?>.Default.Equals(Rsi, other.Rsi) &&
        EqualityComparer<double?>.Default.Equals(Macd, other.Macd) &&
        EqualityComparer<double?>.Default.Equals(Trend, other.Trend);

    public override bool Equals(object obj) => obj is MarketFrame other && Equals(other);
    public static bool operator ==(MarketFrame left, MarketFrame right) => left.Equals(right);
    public static bool operator !=(MarketFrame left, MarketFrame right) => !left.Equals(right);

    public override int GetHashCode()
    {
        unchecked
        {
            int hash = 17;
            hash = hash * 31 + EqualityComparer<int>.Default.GetHashCode(Index);
            hash = hash * 31 + EqualityComparer<int>.Default.GetHashCode(Session);
            hash = hash * 31 + EqualityComparer<double>.Default.GetHashCode(Open);
            hash = hash * 31 + EqualityComparer<double>.Default.GetHashCode(High);
            hash = hash * 31 + EqualityComparer<double>.Default.GetHashCode(Low);
            hash = hash * 31 + EqualityComparer<double>.Default.GetHashCode(Close);
            hash = hash * 31 + EqualityComparer<double>.Default.GetHashCode(Volume);
            hash = hash * 31 + EqualityComparer<double?>.Default.GetHashCode(CvdOpen);
            hash = hash * 31 + EqualityComparer<double?>.Default.GetHashCode(CvdHigh);
            hash = hash * 31 + EqualityComparer<double?>.Default.GetHashCode(CvdLow);
            hash = hash * 31 + EqualityComparer<double?>.Default.GetHashCode(CvdClose);
            hash = hash * 31 + EqualityComparer<double?>.Default.GetHashCode(Atr);
            hash = hash * 31 + EqualityComparer<double?>.Default.GetHashCode(Rsi);
            hash = hash * 31 + EqualityComparer<double?>.Default.GetHashCode(Macd);
            hash = hash * 31 + EqualityComparer<double?>.Default.GetHashCode(Trend);
            return hash;
        }
    }

    public void Deconstruct(out int Index, out int Session, out double Open, out double High, out double Low, out double Close, out double Volume, out double? CvdOpen, out double? CvdHigh, out double? CvdLow, out double? CvdClose, out double? Atr, out double? Rsi, out double? Macd, out double? Trend)
    {
        Index = this.Index;
        Session = this.Session;
        Open = this.Open;
        High = this.High;
        Low = this.Low;
        Close = this.Close;
        Volume = this.Volume;
        CvdOpen = this.CvdOpen;
        CvdHigh = this.CvdHigh;
        CvdLow = this.CvdLow;
        CvdClose = this.CvdClose;
        Atr = this.Atr;
        Rsi = this.Rsi;
        Macd = this.Macd;
        Trend = this.Trend;
    }

    public override string ToString() => $"MarketFrame {{ Index = {Index}, Session = {Session}, Open = {Open}, High = {High}, Low = {Low}, Close = {Close}, Volume = {Volume}, CvdOpen = {CvdOpen}, CvdHigh = {CvdHigh}, CvdLow = {CvdLow}, CvdClose = {CvdClose}, Atr = {Atr}, Rsi = {Rsi}, Macd = {Macd}, Trend = {Trend} }}";
}

internal readonly struct SwingPoint : IEquatable<SwingPoint>
{
    public int Index { get; init; }
    public int ConfirmedAt { get; init; }
    public double Price { get; init; }
    public bool High { get; init; }
    public string Label { get; init; }
    public bool CloseSeed { get; init; }

    public SwingPoint(int Index, int ConfirmedAt, double Price, bool High, string Label, bool CloseSeed = false)
    {
        this.Index = Index;
        this.ConfirmedAt = ConfirmedAt;
        this.Price = Price;
        this.High = High;
        this.Label = Label;
        this.CloseSeed = CloseSeed;
    }

    public bool Equals(SwingPoint other) =>
        EqualityComparer<int>.Default.Equals(Index, other.Index) &&
        EqualityComparer<int>.Default.Equals(ConfirmedAt, other.ConfirmedAt) &&
        EqualityComparer<double>.Default.Equals(Price, other.Price) &&
        EqualityComparer<bool>.Default.Equals(High, other.High) &&
        EqualityComparer<string>.Default.Equals(Label, other.Label) &&
        EqualityComparer<bool>.Default.Equals(CloseSeed, other.CloseSeed);

    public override bool Equals(object obj) => obj is SwingPoint other && Equals(other);
    public static bool operator ==(SwingPoint left, SwingPoint right) => left.Equals(right);
    public static bool operator !=(SwingPoint left, SwingPoint right) => !left.Equals(right);

    public override int GetHashCode()
    {
        unchecked
        {
            int hash = 17;
            hash = hash * 31 + EqualityComparer<int>.Default.GetHashCode(Index);
            hash = hash * 31 + EqualityComparer<int>.Default.GetHashCode(ConfirmedAt);
            hash = hash * 31 + EqualityComparer<double>.Default.GetHashCode(Price);
            hash = hash * 31 + EqualityComparer<bool>.Default.GetHashCode(High);
            hash = hash * 31 + EqualityComparer<string>.Default.GetHashCode(Label);
            hash = hash * 31 + EqualityComparer<bool>.Default.GetHashCode(CloseSeed);
            return hash;
        }
    }

    public void Deconstruct(out int Index, out int ConfirmedAt, out double Price, out bool High, out string Label, out bool CloseSeed)
    {
        Index = this.Index;
        ConfirmedAt = this.ConfirmedAt;
        Price = this.Price;
        High = this.High;
        Label = this.Label;
        CloseSeed = this.CloseSeed;
    }

    public override string ToString() => $"SwingPoint {{ Index = {Index}, ConfirmedAt = {ConfirmedAt}, Price = {Price}, High = {High}, Label = {Label}, CloseSeed = {CloseSeed} }}";
}
internal readonly struct SwingChange : IEquatable<SwingChange>
{
    public SwingPoint Point { get; init; }
    public SwingPoint? Removed { get; init; }

    public SwingChange(SwingPoint Point, SwingPoint? Removed = null)
    {
        this.Point = Point;
        this.Removed = Removed;
    }

    public bool Equals(SwingChange other) =>
        EqualityComparer<SwingPoint>.Default.Equals(Point, other.Point) &&
        EqualityComparer<SwingPoint?>.Default.Equals(Removed, other.Removed);

    public override bool Equals(object obj) => obj is SwingChange other && Equals(other);
    public static bool operator ==(SwingChange left, SwingChange right) => left.Equals(right);
    public static bool operator !=(SwingChange left, SwingChange right) => !left.Equals(right);

    public override int GetHashCode()
    {
        unchecked
        {
            int hash = 17;
            hash = hash * 31 + EqualityComparer<SwingPoint>.Default.GetHashCode(Point);
            hash = hash * 31 + EqualityComparer<SwingPoint?>.Default.GetHashCode(Removed);
            return hash;
        }
    }

    public void Deconstruct(out SwingPoint Point, out SwingPoint? Removed)
    {
        Point = this.Point;
        Removed = this.Removed;
    }

    public override string ToString() => $"SwingChange {{ Point = {Point}, Removed = {Removed} }}";
}

internal sealed class WilderAverage(int period)
{
    private int count;
    private double total;
    private double? value;
    public double? Add(double input)
    {
        if (value.HasValue) value = (value.Value * (period - 1) + input) / period;
        else if (++count >= period) value = (total + input) / period;
        else total += input;
        return value;
    }
}

internal sealed class Ema(int period)
{
    private int count;
    private double total;
    private double? value;
    public double? Add(double input)
    {
        if (value.HasValue) value += 2.0 / (period + 1) * (input - value.Value);
        else if (++count >= period) value = (total + input) / period;
        else total += input;
        return value;
    }
}

internal sealed class FrameBuilder(int atrPeriod, int rsiPeriod, int fast, int slow, int signal, int trendPeriod)
{
    private readonly WilderAverage atr = new(atrPeriod), gains = new(rsiPeriod), losses = new(rsiPeriod);
    private readonly Ema fastEma = new(fast), slowEma = new(slow), signalEma = new(signal), trendEma = new(trendPeriod);
    private double? previousClose;
    private double? cvd = 0;
    private int session;

    public MarketFrame Add(int index, bool newSession, double open, double high, double low, double close,
        double volume, double? delta, double? maxDelta, double? minDelta)
    {
        if (newSession && index > 0) { session++; cvd = 0; }
        double tr = high - low;
        double? rsi = null;
        if (previousClose.HasValue)
        {
            tr = Math.Max(tr, Math.Max(Math.Abs(high - previousClose.Value), Math.Abs(low - previousClose.Value)));
            double change = close - previousClose.Value;
            double? gain = gains.Add(Math.Max(change, 0)), loss = losses.Add(Math.Max(-change, 0));
            if (gain.HasValue && loss.HasValue)
                rsi = loss.Value == 0 ? gain.Value > 0 ? 100 : 50 : 100 - 100 / (1 + gain.Value / loss.Value);
        }
        previousClose = close;
        double? a = fastEma.Add(close), b = slowEma.Add(close), macd = null;
        if (a.HasValue && b.HasValue)
        {
            double line = a.Value - b.Value;
            double? sig = signalEma.Add(line);
            if (sig.HasValue) macd = line - sig.Value;
        }
        double? cvdOpen = cvd, cvdHigh = null, cvdLow = null, cvdClose = null;
        if (cvd.HasValue && delta.HasValue)
        {
            cvdClose = cvd.Value + delta.Value;
            if (maxDelta.HasValue && minDelta.HasValue)
            {
                cvdHigh = cvd.Value + Math.Max(0, Math.Max(delta.Value, maxDelta.Value));
                cvdLow = cvd.Value + Math.Min(0, Math.Min(delta.Value, minDelta.Value));
            }
            cvd = cvdClose;
        }
        else { cvd = null; cvdOpen = null; }
        return new MarketFrame(index, session, open, high, low, close, volume, cvdOpen,
            cvdHigh, cvdLow, cvdClose, atr.Add(tr), rsi, macd, trendEma.Add(close));
    }
}
