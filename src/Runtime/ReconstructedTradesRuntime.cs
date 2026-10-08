using SwingDivergence.Analysis;

namespace SwingDivergence;

public partial class SwingDivergenceIdentifier
{
    private string reconstructedKey;
    private long reconstructedSequence;
    private TradeSizeScale tradeSizeScale;
    private double lastTradeScale;

    private void DrawReconstructed(DeepTrade? value, bool completed)
    {
        if (!value.HasValue) return;
        var trade = value.Value;
        reconstructedKey ??= trade.OrderId != 0
            ? $"raw-order:{VAn.BarVars[trade.BarIndex].exchDt.Date.Ticks}:{trade.OrderId}:{trade.Buy}"
            : $"raw-sweep:{VAn.BarVars[trade.BarIndex].exchDt.Ticks}:{trade.Time}:{trade.Buy}:{++reconstructedSequence}";
        DrawDeep(trade, reconstructedKey);
        if (completed) reconstructedKey = null;
    }

    private void SeedTradeSizeScale()
    {
        tradeSizeScale = new TradeSizeScale(DeepTradeSample);
        foreach (var cached in tradeHistory.Values.OrderBy(v => v.BarTime).ThenBy(v => v.Trade.Time))
            if (cached.Trade.Volume >= DeepTradeMinimum && (DeepTradeMaximum <= 0 || cached.Trade.Volume <= DeepTradeMaximum))
                tradeSizeScale.Record(cached.Key, cached.Trade.Volume);
        lastTradeScale = tradeSizeScale.Upper(TradePlot.StandardDeviation);
    }

    private void UpdateTradeScale(double upper)
    {
        if (lastTradeScale > 0 && Math.Abs(upper - lastTradeScale) / lastTradeScale < .05) return;
        lastTradeScale = upper;
        foreach (var drawing in deepDrawings)
        {
            var style = DeepTradeAppearance.Calibrated(drawing.Trade.Volume, upper, tickSize, TradePlot.MinimumSize,
                TradePlot.MaximumSize, TradePlot.MinimumOpacity, TradePlot.MaximumOpacity, TradePlot.TextSize,
                DeepLabelZoomBars, DeepExpert.BorderOpacityPercent, TradePlot.RadiusScale);
            var circle = drawing.Items[0];
            circle.Width = circle.Height = style.PriceRadius;
            var color = drawing.Trade.Buy ? DeepBuyColor : DeepSellColor;
            circle.BackColor = color.WithOpacity(style.FillOpacity);
            if (drawing.Items.Count > 1) drawing.Items[1].MaxBarsViewed = style.LabelMaximumBars;
        }
    }
}
