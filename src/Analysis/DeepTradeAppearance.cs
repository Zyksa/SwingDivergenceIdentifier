namespace SwingDivergence.Analysis;

internal readonly struct TradeAppearance : IEquatable<TradeAppearance>
{
    public double Size { get; init; }
    public double PriceRadius { get; init; }
    public byte FillOpacity { get; init; }
    public byte BorderOpacity { get; init; }
    public int FontSize { get; init; }
    public int LabelMaximumBars { get; init; }

    public TradeAppearance(double Size, double PriceRadius, byte FillOpacity, byte BorderOpacity, int FontSize, int LabelMaximumBars)
    {
        this.Size = Size;
        this.PriceRadius = PriceRadius;
        this.FillOpacity = FillOpacity;
        this.BorderOpacity = BorderOpacity;
        this.FontSize = FontSize;
        this.LabelMaximumBars = LabelMaximumBars;
    }

    public bool Equals(TradeAppearance other) =>
        EqualityComparer<double>.Default.Equals(Size, other.Size) &&
        EqualityComparer<double>.Default.Equals(PriceRadius, other.PriceRadius) &&
        EqualityComparer<byte>.Default.Equals(FillOpacity, other.FillOpacity) &&
        EqualityComparer<byte>.Default.Equals(BorderOpacity, other.BorderOpacity) &&
        EqualityComparer<int>.Default.Equals(FontSize, other.FontSize) &&
        EqualityComparer<int>.Default.Equals(LabelMaximumBars, other.LabelMaximumBars);

    public override bool Equals(object obj) => obj is TradeAppearance other && Equals(other);
    public static bool operator ==(TradeAppearance left, TradeAppearance right) => left.Equals(right);
    public static bool operator !=(TradeAppearance left, TradeAppearance right) => !left.Equals(right);

    public override int GetHashCode()
    {
        unchecked
        {
            int hash = 17;
            hash = hash * 31 + EqualityComparer<double>.Default.GetHashCode(Size);
            hash = hash * 31 + EqualityComparer<double>.Default.GetHashCode(PriceRadius);
            hash = hash * 31 + EqualityComparer<byte>.Default.GetHashCode(FillOpacity);
            hash = hash * 31 + EqualityComparer<byte>.Default.GetHashCode(BorderOpacity);
            hash = hash * 31 + EqualityComparer<int>.Default.GetHashCode(FontSize);
            hash = hash * 31 + EqualityComparer<int>.Default.GetHashCode(LabelMaximumBars);
            return hash;
        }
    }

    public void Deconstruct(out double Size, out double PriceRadius, out byte FillOpacity, out byte BorderOpacity, out int FontSize, out int LabelMaximumBars)
    {
        Size = this.Size;
        PriceRadius = this.PriceRadius;
        FillOpacity = this.FillOpacity;
        BorderOpacity = this.BorderOpacity;
        FontSize = this.FontSize;
        LabelMaximumBars = this.LabelMaximumBars;
    }

    public override string ToString() => $"TradeAppearance {{ Size = {Size}, PriceRadius = {PriceRadius}, FillOpacity = {FillOpacity}, BorderOpacity = {BorderOpacity}, FontSize = {FontSize}, LabelMaximumBars = {LabelMaximumBars} }}";
}

/// <summary>Independent visual profile for translucent, volume-proportional trade bubbles.</summary>
internal static class DeepTradeAppearance
{
    public static TradeAppearance Calibrated(double volume, double upper, double tickSize, double minimum, double maximum,
        int minimumOpacity, int maximumOpacity, int textSize, int labelZoomBars, int borderOpacity, double radiusScale = 1)
    {
        double strength = Math.Clamp(volume / Math.Max(1, upper), 0, 1);
        double size = minimum + (maximum - minimum) * strength;
        double opacity = minimumOpacity + (maximumOpacity - minimumOpacity) * strength;
        return new TradeAppearance(size, tickSize * size / 2 * radiusScale, (byte)Math.Round(opacity * 255 / 100.0),
            (byte)Math.Round(borderOpacity * 255 / 100.0), textSize,
            Math.Max(20, (int)Math.Round(labelZoomBars * Math.Min(1, size * radiusScale / maximum))));
    }

    public static TradeAppearance Calculate(double volume, double threshold, double tickSize,
        int minimum, int maximum, int opacityMinimum, int opacityMaximum, int labelZoomBars, int borderOpacity)
    {
        double ratio = Math.Max(1, volume / Math.Max(1, threshold));
        // Area scales with volume; bounded radius prevents medium executions from
        // covering multiple candles when the price axis is zoomed in.
        double size = Math.Clamp(minimum * Math.Sqrt(ratio), minimum, maximum);
        double strength = Math.Clamp((size - minimum) / Math.Max(1, maximum - minimum), 0, 1);
        double opacity = opacityMinimum + strength * (opacityMaximum - opacityMinimum);
        return new TradeAppearance(size, tickSize * size / 2, (byte)Math.Round(opacity * 255 / 100),
            (byte)Math.Round(Math.Clamp(borderOpacity, 0, 100) * 255 / 100.0),
            (int)Math.Clamp(8 + strength * 3, 8, 11),
            Math.Max(20, (int)Math.Round(labelZoomBars * size / maximum)));
    }
}
