using SwingDivergence.Analysis;
using Feed = VolumetricaAPI.Connection.Structure;
using static VolSysAPI.Structure;

namespace SwingDivergence;

public partial class SwingDivergenceIdentifier
{
    private int aggregatePackets;
    private string nativeLastKey;
    private DeepTrade? nativeLastTrade;

    private void ProcessSdkTrade(Feed.TickByTick tick, AggrInfo info)
    {
        if (info == null || info.Vol <= 0 || !double.IsFinite(info.AvgPrice)) return;
        bool? buy = info.TradeDir is TradeDirStatus.Ask or TradeDirStatus.AboveAsk ? true
            : info.TradeDir is TradeDirStatus.Bid or TradeDirStatus.BelowBid ? false
            : tick.AggrSide == Feed.AggressorSideEnum.Ask ? true
            : tick.AggrSide == Feed.AggressorSideEnum.Bid ? false : null;
        if (!buy.HasValue) return;
        aggregatePackets++;
        long time = tick.exDt.Ticks / TimeSpan.TicksPerMillisecond;
        double price = info.AvgPrice != 0 || tick.price == 0 ? info.AvgPrice : tick.price;
        string key;
        if (tick.AggrID != 0) key = $"SDK:{tick.exDt.Date.Ticks}:{tick.AggrID}:{buy.Value}";
        else if (nativeLastTrade is DeepTrade previous && previous.BarIndex == VAn.BarIndex
            && previous.Buy == buy && previous.Time == time && info.Vol >= previous.Volume)
            key = nativeLastKey;
        else key = $"SDK-event:{VAn.BarVars[VAn.BarIndex].exchDt.Ticks}:{time}:{buy.Value}";
        if (nativeLastKey != null && key != nativeLastKey) CompleteNativeSample();
        var order = new DeepTrade(VAn.BarIndex, time, buy.Value, price, info.Vol,
            Math.Max(1, info.NumberOfTrades), Math.Min(info.StartPrice, info.EndPrice),
            Math.Max(info.StartPrice, info.EndPrice), DeepTradeAdaptive ? deep.Threshold : DeepTradeMinimum, info.MaxQtyOrd);
        nativeLastKey = key; nativeLastTrade = order;
        // AggrInfo can be an evolving snapshot: update one marker per aggregate ID,
        // rather than layering one new bubble for every raw execution of that order.
        DrawDeep(order, key);
    }

    private void CompleteNativeSample()
    {
        if (nativeLastTrade is not DeepTrade order || deep == null) return;
        deep.Feed(order.BarIndex, order.Time, order.Price, order.Volume, order.Buy);
        deep.Flush(); // Only updates percentile statistics; the snapshot already has its marker.
    }
}
