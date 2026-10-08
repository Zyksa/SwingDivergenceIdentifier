using SwingDivergence.Analysis;

namespace SwingDivergence;

public partial class SwingDivergenceIdentifier
{
    private readonly TradeHistoryCache tradeHistory = new();

    private string TradeContext() => $"{VAn.SymbolName}|{VAn.ParamType}|{VAn.Param1}|{VAn.Param2}|{VAn.IsReplayData}|{DeepTradeSource}|{DeepAggregationMs}";

    private void RestoreTradeHistory()
    {
        if (!ShowDeepTrades || annotations == null) return;
        var barIndices = new Dictionary<long, int>();
        for (int i = 0; i < VAn.BarVars.Count; i++)
            barIndices[VAn.BarVars[i].exchDt.Ticks] = i;
        // Rendering must not modify the cache while it is being enumerated.
        foreach (var cached in tradeHistory.Values)
        {
            if (!barIndices.TryGetValue(cached.BarTime, out int index)) continue;
            int original = cached.Trade.BarIndex;
            if (original >= 0 && original < VAn.BarVars.Count && VAn.BarVars[original].exchDt.Ticks == cached.BarTime)
                index = original;
            if (VAn.LastExDt != default && new DateTime(cached.BarTime) < VAn.LastExDt.AddDays(-DeepDaysToLoad)) continue;
            DrawDeep(cached.Trade with { BarIndex = index }, cached.Key, remember: false, refresh: true);
        }
    }
}
