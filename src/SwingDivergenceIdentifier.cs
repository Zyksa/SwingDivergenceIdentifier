using SwingDivergence.Analysis;
using VolSysAPI;
using VolSysAPI.Indicators;
using VolumetricaAPI.Chart;
using VolumetricaCore;
using Feed = VolumetricaAPI.Connection.Structure;
using static VolSysAPI.Structure;
using static VolSysAPI.ExternalStructure;

namespace SwingDivergence;

public partial class SwingDivergenceIdentifier : Indicator
{
    private const string VersionLabel = "v25";
    private const string IndicatorName = "Swing & Divergence Identifier";
    public static IndicatorDescriptionBase Register() => new()
    {
        Name = IndicatorName,
        Description = "Swing & Divergence Identifier maps HH, HL, LH and LL market structure points and regular or hidden CVD divergences. Pink lines mark divergences without a confirmed structure anchor; structured signals retain EXH/ABS labels and directional colors. Configure opacity, visibility, sounds and popups separately for both groups, with lines on price and CVD. Translucent execution bubbles track aggregated trades, filter by minimum volume and reveal centered volume labels as you zoom in.",
        Tags = new List<string> { "Structure", "Divergence", "CVD", "RSI", "MACD", "Trade bubbles", "Swing" }
    };

    private readonly List<MarketFrame> frames = new();
    private FrameBuilder builder;
    private AtrSwings majorAtr, minorAtr;
    private ReactiveSwings majorReactive, minorReactive;
    private AlternatingLegs majorLegs, minorLegs;
    private Divergences cvdDivergences;
    private FilteredSwings majorWindows, minorWindows;
    private Divergences majorDivergences, minorDivergences;
    private DeepTradeAggregator deep;
    private IAnnGroup annotations;
    private IAnnotation diagnostic;
    private int lastClosedIndex = -1, tickCount, missingCvd, sessionNumber;
    private double tickSize;
    private double? lastTradePrice;
    private bool historyFinished;
    private string lastDiagnostic;

    public override void OnSet(bool setDefault, bool themeOverride)
    {
        ClampSettings();
        OnOpenCall = OnCloseCall = OnEndCall = CallHandler.HistRT;
        // Bar totals cannot recover order sizes: historical prints make this bundle desktop-only.
        OnTickCall = ShowDeepTrades ? CallHandler.HistRT
            : WantsLiveCvd || LiveStructurePreview && ShowStructure ? CallHandler.RT : CallHandler.Off;
        if (ShowDeepTrades && DeepTradeSource == DeepTradeSource.AgregatsSDK && VAn != null)
            VAn.SetCalculateAggrTrades();
        // A zero-width caption suppresses hosts that substitute a default for null/empty.
        Description = HideChartText ? "\u200B" : $"{IndicatorName} {VersionLabel} | {Detection}";
        StatusMessage = null;
    }

    public override void OnLoad()
    {
        tradeHistory.ResetContext(TradeContext());
        cvdStructure = new(); knownCvdSignals.Clear(); notifiedCvdPairs.Clear();
        reconstructedKey = null; reconstructedSequence = 0; SeedTradeSizeScale();
        ClearLiveCvdDrawing(); liveCvdSnapshot = new();
        frames.Clear(); lastClosedIndex = -1; tickCount = missingCvd = 0;
        sessionNumber = 0; builder = null; annotations = null; deep = null; diagnostic = null; lastTradePrice = null;
        historyFinished = false; lastDiagnostic = null; alertGate = new CvdAlertGate(); ClearLivePreview();
        structureBubbles.Clear(); divergenceDrawings.Clear(); deepDrawings.Clear(); deepByKey.Clear();
        aggregatePackets = 0; nativeLastKey = null; nativeLastTrade = null;
        tickSize = Math.Abs(VAn.DoubleAddSubTicks(0, 1));
        if (!double.IsFinite(tickSize) || tickSize <= 0)
        {
            SetDiagnostic("S&D : pas de tick valide pour cet instrument.");
            return;
        }
        InitializeAnalysis();
        deep = MakeDeepAggregator();
        if (ShowDeepTrades && DeepTradeSource == DeepTradeSource.AgregatsSDK) VAn.SetCalculateAggrTrades();
        annotations = IndVars.FrontAnnList;
        annotations.Visible = true;
        diagnostic = null;
        if (ShowDiagnostics && !HideChartText)
        {
            diagnostic = VAn.CreateAnnotation(AnnotationType.Text);
            diagnostic.CoordinateXType = diagnostic.CoordinateYType = CoordinateTypeEnum.Relative;
            diagnostic.X = 0.02; diagnostic.Y = 0.90;
            diagnostic.Area = ChartAreaRef.Main; diagnostic.Visible = true;
            diagnostic.ForeColor = ColorRef.Yellow; diagnostic.FontSize = 12;
            diagnostic.Text = $"{IndicatorName} {VersionLabel} : chargement";
            VAn.AddAnnotation(annotations, diagnostic);
        }
        StatusMessage = null;
    }

    private void InitializeAnalysis()
    {
        double epsilon = PriceToleranceTicks * tickSize;
        builder = new FrameBuilder(AtrPeriod, RsiPeriod, MacdFast, MacdSlow, MacdSignal, TrendPeriod);
        majorAtr = new AtrSwings(SwingAtr, AtrPeriod, epsilon);
        minorAtr = new AtrSwings(MinorSwingAtr, AtrPeriod, epsilon);
        majorReactive = new ReactiveSwings(SwingAtr, AtrPeriod, epsilon, StructureExpert.MinimumReversalTicks * tickSize, DirectionalBars);
        minorReactive = new ReactiveSwings(MinorSwingAtr, AtrPeriod, epsilon, StructureExpert.MinimumReversalTicks * tickSize, DirectionalBars);
        majorLegs = new AlternatingLegs(SwingAtr, AtrPeriod, epsilon, StructureExpert.MinimumReversalTicks * tickSize, DirectionalBars);
        minorLegs = new AlternatingLegs(MinorSwingAtr, AtrPeriod, epsilon, StructureExpert.MinimumReversalTicks * tickSize, DirectionalBars);
        majorWindows = new FilteredSwings(LeftBars, RightBars, epsilon, MinimumSwingAtr);
        minorWindows = new FilteredSwings(MinorLeftBars, MinorRightBars, epsilon, MinimumSwingAtr);
        var options = new DivergenceOptions(false, ShowTechnicalDivergences, Oscillator,
            CvdAtClose, ShowHiddenDivergences, ShowWeakCvdDivergences, DivergenceMaxBars, epsilon,
            CvdTolerance, OscillatorTolerance, RequireClearPriceLine, RequireClearCvdLine,
            RequireHiddenTrend, RequireTechnicalValidation, ValidationBars, OscillatorAlignmentBars);
        majorDivergences = new Divergences(options);
        minorDivergences = new Divergences(options);
        cvdDivergences = new Divergences(options with { Cvd = NeedsCvd, Technical = false,
            AlignmentBars = 0 });
        var setupOptions = new SetupOptions(SetupConfirmation, SetupRewardRisk, SetupExpert.StopAtr,
            SetupExpert.MinimumStopTicks * tickSize, tickSize, epsilon, SetupExpert.WaitingBars,
            SetupExpert.Bullish, SetupExpert.Bearish, SetupExpert.Absorption, SetupExpert.Exhaustion);
        cvdSetups = new StructuralCvdSetups(SwingAtr, AtrPeriod,
            options with { CvdClose = false }, setupOptions, externalStructure: true);
        localCvdPivots = new LocalCvdPivots(DivergenceExpert.CvdLeftBars, DivergenceExpert.CvdRightBars, epsilon);
        localCvdDivergences = new Divergences(options with { Cvd = true, Technical = false, CvdClose = false,
            Hidden = true, Weak = false, AlignmentBars = 0, ClearPrice = true, ClearCvd = true });
        localCvdSetups = new CvdSetups(setupOptions);
    }

    private DeepTradeAggregator MakeDeepAggregator() => new(DeepTradeMinimum, DeepTradePercentile,
        DeepTradeSample, DeepAggregationMs, DeepTradeAdaptive, retainBelowThreshold: true);

    public override void OnOpen(bool isRt)
    {
        if (deep != null)
        {
            if (DeepTradeSource == DeepTradeSource.AgregatsReconstitues) DrawReconstructed(deep.Flush(), completed: true);
            else CompleteNativeSample();
            nativeLastKey = null; nativeLastTrade = null;
            if (VAn.BarVars[VAn.BarIndex].IsNewDay) { deep = MakeDeepAggregator(); lastTradePrice = null; }
        }
        if (isRt)
        {
            if (VAn.BarIndex - 1 > lastClosedIndex) ProcessClosedBars(VAn.BarIndex - 1, true);
            UpdateLiveCvd(VAn.BarIndex, true);
        }
    }

    public override void OnClose(bool isRt)
    {
        if (isRt) ProcessClosedBars(VAn.BarIndex, true);
    }

    public override void OnTick(Feed.TickByTick tick, bool isRt, AggrInfo aggrInfo)
    {
        if (isRt && historyFinished)
        {
            if (VAn.BarIndex - 1 > lastClosedIndex) ProcessClosedBars(VAn.BarIndex - 1, true);
            UpdateLiveCvd(VAn.BarIndex, true, tick.price);
            UpdateLivePreview(VAn.BarIndex, tick.price);
        }
        if (!ShowDeepTrades || deep == null || annotations == null) return;
        tickCount++;
        if (VAn.LastExDt != default && tick.exDt != default && tick.exDt < VAn.LastExDt.AddDays(-DeepDaysToLoad)) return;
        if (DeepTradeSource == DeepTradeSource.AgregatsSDK)
        {
            ProcessSdkTrade(tick, aggrInfo);
            return;
        }
        bool? buy = tick.AggrSide == Feed.AggressorSideEnum.Ask ? true
            : tick.AggrSide == Feed.AggressorSideEnum.Bid ? false : null;
        if (!buy.HasValue && (tick.bid != 0 || tick.ask != 0) && tick.ask >= tick.bid)
        {
            if (tick.price >= tick.ask) buy = true;
            else if (tick.price <= tick.bid) buy = false;
            else if (tick.price > (tick.bid + tick.ask) / 2) buy = true;
            else if (tick.price < (tick.bid + tick.ask) / 2) buy = false;
        }
        if (!buy.HasValue && lastTradePrice.HasValue)
        {
            if (tick.price > lastTradePrice.Value) buy = true;
            else if (tick.price < lastTradePrice.Value) buy = false;
        }
        if (double.IsFinite(tick.price) && tick.Vol > 0) lastTradePrice = tick.price;
        DrawReconstructed(deep.Feed(VAn.BarIndex, tick.exDt.Ticks / TimeSpan.TicksPerMillisecond, tick.price, tick.Vol, buy, tick.AggrID), completed: true);
        DrawReconstructed(deep.Snapshot(), completed: false);
    }

    public override void OnEnd(bool isRt)
    {
        if (builder == null) return;
        bool wasHistoryFinished = historyFinished;
        ProcessClosedBars(VAn.LastIndex() - 1, isRt);
        historyFinished = true;
        UpdateLiveCvd(VAn.LastIndex(), isRt && wasHistoryFinished);
        UpdateLivePreview(VAn.LastIndex());
        if (ShowDeepTrades && deep != null && DeepTradeSource == DeepTradeSource.AgregatsReconstitues)
            DrawReconstructed(isRt ? deep.FlushStale(VAn.LastExDt.Ticks / TimeSpan.TicksPerMillisecond) : deep.Flush(), completed: true);
        if (!isRt) { SeedTradeSizeScale(); RestoreTradeHistory(); }
        if (diagnostic != null)
            diagnostic.Text = $"S&D {VersionLabel} : {structureBubbles.Count} structures | {divergenceDrawings.Count} divergences"
                + $" | {deepDrawings.Count} DT | {frames.Count} bougies | {tickCount} trades lus";
        if (missingCvd > 0 && (ShowCvdDivergences || ShowCvdSetups || EnableCvdSound))
            SetDiagnostic($"S&D : CVD indisponible sur {missingCvd} bougies ; ces divergences ne sont pas calculées.");
        else if (ShowDeepTrades && !isRt && tickCount == 0)
            SetDiagnostic("S&D : aucun historique de transactions reçu ; les bulles apparaîtront avec le flux disponible.");
        else if (ShowDeepTrades && DeepTradeSource == DeepTradeSource.AgregatsSDK && tickCount > 0 && aggregatePackets == 0)
            SetDiagnostic("S&D : le flux ne fournit pas d'agrégats SDK. Source : AgregatsReconstitues pour la reconstitution intégrée.");
        else SetDiagnostic(null);
    }

    private void ProcessClosedBars(int throughIndex, bool isRt)
    {
        if (builder == null || annotations == null || throughIndex < 0) return;
        if (throughIndex < lastClosedIndex && VAn.IsPartialRecalculation) { VAn.SetFullRecalculation(); return; }
        throughIndex = Math.Min(throughIndex, VAn.BarVars.Count - 1);
        while (lastClosedIndex < throughIndex)
        {
            int index = lastClosedIndex + 1;
            ClearLiveCvdDrawing();
            ClearLivePreview();
            var bar = VAn.BarVars[index];
            if (index > 0 && bar.IsNewDay) { sessionNumber++; InitializeAnalysis(); }
            var totals = bar.VolTotList != null && bar.VolTotList.Length > 0 ? bar.VolTotList[0] : null;
            double? delta = totals == null ? null : (double)totals.AskVol - totals.BidVol;
            var frame = builder.Add(index, false, bar.Open, bar.High, bar.Low, bar.Close,
                totals?.TotVol ?? 0, delta, totals?.MaxDeltaVol, totals?.MinDeltaVol) with { Session = sessionNumber };
            frames.Add(frame);
            bool live = isRt && historyFinished && index == throughIndex;
            if (!frame.CvdClose.HasValue) missingCvd++;
            var primaryChanges = AdvanceStructure(frame, false);
            foreach (var update in primaryChanges) cvdStructure.RecordMajor(update);
            foreach (var update in primaryChanges)
                if (update.Removed is SwingPoint old) ForgetSwing(old, false);
            if (ShowCvdSetups)
                foreach (var setup in cvdSetups.Add(frame, frames, primaryChanges)) HandleSetup(setup, live);
            foreach (var change in primaryChanges) Offer(change, false, live);
            if (ShowMinorStructure)
                foreach (var change in AdvanceStructure(frame, true)) Offer(change, true, live);
            if (NeedsCvd) ProcessLocalCvd(frame, live);
            foreach (var change in primaryChanges) PromoteCvd(change, index, live);
            foreach (var signal in majorDivergences.Advance(frames)) HandleDivergence(signal, false, live);
            if (MinorDivergences && ShowMinorStructure)
                foreach (var signal in minorDivergences.Advance(frames)) HandleDivergence(signal, true, live);
            lastClosedIndex = index;
        }
    }

    private IReadOnlyList<SwingChange> AdvanceStructure(MarketFrame frame, bool minor)
    {
        if (Detection == StructureDetection.FilteredPivots)
            return (minor ? minorWindows : majorWindows).Add(frame, frames);
        SwingChange? change = Detection == StructureDetection.AlternatingLegs ? (minor ? minorLegs : majorLegs).Add(frame)
            : Detection == StructureDetection.ReactiveSwings ? (minor ? minorReactive : majorReactive).Add(frame)
            : (minor ? minorAtr : majorAtr).Add(frame);
        return change.HasValue ? new List<SwingChange> { change.Value } : Array.Empty<SwingChange>();
    }

    private void Offer(SwingChange? change, bool minor, bool live)
    {
        if (!change.HasValue) return;
        var update = change.Value;
        if (minor && update.Removed is SwingPoint old)
        {
            ForgetSwing(old, minor);
        }
        DrawStructure(update.Point, minor);
        if (!minor && NeedsCvd)
            foreach (var signal in cvdDivergences.Offer(update, frames)) HandleDivergence(signal, false, live);
        if (!minor || MinorDivergences)
            foreach (var signal in (minor ? minorDivergences : majorDivergences).Offer(update, frames)) HandleDivergence(signal, minor, live);
    }

    private void SetDiagnostic(string message)
    {
        lastDiagnostic = message;
        StatusMessage = HideChartText ? null : message;
    }
    private bool NeedsCvd => ShowCvdDivergences || ShowCvdSetups || EnableCvdSound || ShowCvdNotification;
}
