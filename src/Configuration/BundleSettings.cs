using System.ComponentModel;
using SwingDivergence.Analysis;
using VolumetricaAPI.Chart;
using VolumetricaControls;

namespace SwingDivergence;

public partial class SwingDivergenceIdentifier
{

    [Category("01 Général"), DisplayName("Afficher la structure")]
    public bool ShowStructure { get; set; } = true;
    [Category("01 Général"), DisplayName("Afficher les divergences")]
    public bool ShowDivergences { get; set; } = true;
    [Category("01 Général"), DisplayName("Source des divergences")]
    public DivergenceSource DivergenceSource { get; set; } = DivergenceSource.Cvd;
    [Category("03 Bulles"), DisplayName("Source des transactions")]
    public DeepTradeSource DeepTradeSource { get; set; } = DeepTradeSource.AgregatsReconstitues;
    [Category("02 Structure"), DisplayName("Opacité du fond (%)")]
    [VolCustom(MinValue = 0, MaxValue = 100, IncrementValue = 1)]
    public int StructureOpacityPercent { get; set; } = 15;
    [VolCustom(IsVisible = false)]
    public TradeFilterMode DeepFilterMode { get; set; } = TradeFilterMode.Manuel;
    [VolCustom(IsVisible = false)]
    public double DeepTradeMaximum { get; set; } = 0;
    [VolCustom(IsVisible = false)]
    public int DeepOpacityMinPercent { get; set; } = 5;
    [VolCustom(IsVisible = false)]
    public int DeepOpacityMaxPercent { get; set; } = 14;
    [VolCustom(IsVisible = false)]
    public int DeepDaysToLoad { get; set; } = 10;
    [VolCustom(IsVisible = false)]
    public int AppearanceVersion { get; set; } = 0;
    [VolCustom(IsVisible = false)]
    public int StructureVersion { get; set; } = 0;
    [Category("02 Structure"), DisplayName("Afficher les points en formation")]
    [VolCustom(SerializeIgnore = true)]
    public bool LiveStructurePreview { get => LayoutExpert.LiveStructurePreview; set => LayoutExpert.LiveStructurePreview = value; }

    [Category("05 Avancé"), DisplayName("Structure : réglages avancés")]
    public StructureExpert StructureExpert { get; set; } = new();
    [Category("05 Avancé"), DisplayName("Affichage de la structure")]
    public LayoutExpert LayoutExpert { get; set; } = new();
    [Category("05 Avancé"), DisplayName("Divergences : réglages avancés")]
    public DivergenceExpert DivergenceExpert { get; set; } = new();
    [VolCustom(IsVisible = false)]
    public DeepExpert DeepExpert { get; set; } = new();
    [VolCustom(IsVisible = false, SerializeIgnore = true)]
    public StructureDetection Detection { get => StructureExpert.Detection; set => StructureExpert.Detection = value; }
    [Category("02 Structure"), DisplayName("Bougies validant le retournement") ]
    [VolCustom(MinValue = 2, MaxValue = 3, IncrementValue = 1)]
    public int DirectionalBars { get; set; } = 2;
    [Category("02 Structure"), DisplayName("Amplitude du retournement")]
    public double SwingAtr { get; set; } = .75;
    [VolCustom(IsVisible = false, SerializeIgnore = true)]
    public int AtrPeriod { get => StructureExpert.AtrPeriod; set => StructureExpert.AtrPeriod = value; }
    [VolCustom(IsVisible = false, SerializeIgnore = true)]
    public int LeftBars { get => StructureExpert.LeftBars; set => StructureExpert.LeftBars = value; }
    [VolCustom(IsVisible = false, SerializeIgnore = true)]
    public int RightBars { get => StructureExpert.RightBars; set => StructureExpert.RightBars = value; }
    [VolCustom(IsVisible = false, SerializeIgnore = true)]
    public double MinimumSwingAtr { get => StructureExpert.MinimumSwingAtr; set => StructureExpert.MinimumSwingAtr = value; }
    [VolCustom(IsVisible = false, SerializeIgnore = true)]
    public double PriceToleranceTicks { get => StructureExpert.PriceToleranceTicks; set => StructureExpert.PriceToleranceTicks = value; }
    [Category("02 Structure"), DisplayName("Petite structure")]
    public bool ShowMinorStructure { get; set; } = false;
    [VolCustom(IsVisible = false, SerializeIgnore = true)]
    public double MinorSwingAtr { get => StructureExpert.MinorSwingAtr; set => StructureExpert.MinorSwingAtr = value; }
    [VolCustom(IsVisible = false, SerializeIgnore = true)]
    public int MinorLeftBars { get => StructureExpert.MinorLeftBars; set => StructureExpert.MinorLeftBars = value; }
    [VolCustom(IsVisible = false, SerializeIgnore = true)]
    public int MinorRightBars { get => StructureExpert.MinorRightBars; set => StructureExpert.MinorRightBars = value; }
    [Category("02 Structure"), DisplayName("HH")]
    public bool ShowHH { get; set; } = true;
    [Category("02 Structure"), DisplayName("HL")]
    public bool ShowHL { get; set; } = true;
    [Category("02 Structure"), DisplayName("LH")]
    public bool ShowLH { get; set; } = true;
    [Category("02 Structure"), DisplayName("LL")]
    public bool ShowLL { get; set; } = true;
    [VolCustom(IsVisible = false, SerializeIgnore = true)]
    public bool ShowInitialPivots { get => StructureExpert.ShowInitialPivots; set => StructureExpert.ShowInitialPivots = value; }
    [VolCustom(IsVisible = false, SerializeIgnore = true)]
    public BubbleDrawing Drawing { get; set; } = BubbleDrawing.OpticalGlyph;
    [Category("02 Structure"), DisplayName("Taille des bulles")]
    [VolCustom(MinValue = 8, MaxValue = 40, IncrementValue = 1)]
    public int BubbleSize { get; set; } = 16;
    [VolCustom(IsVisible = false, SerializeIgnore = true)]
    public int MinorBubbleSize { get => LayoutExpert.MinorBubbleSize; set => LayoutExpert.MinorBubbleSize = value; }
    [Category("02 Structure"), DisplayName("Taille du texte")]
    [VolCustom(MinValue = 7, MaxValue = 18, IncrementValue = 1)]
    public int FontSize { get; set; } = 9;
    [VolCustom(IsVisible = false, SerializeIgnore = true)]
    public int MinorFontSize { get => LayoutExpert.MinorFontSize; set => LayoutExpert.MinorFontSize = value; }
    [VolCustom(IsVisible = false, SerializeIgnore = true)]
    public int OffsetTicks { get => LayoutExpert.OffsetTicks; set => LayoutExpert.OffsetTicks = value; }
    [VolCustom(IsVisible = false, SerializeIgnore = true)]
    public int MinorOffsetTicks { get => LayoutExpert.MinorOffsetTicks; set => LayoutExpert.MinorOffsetTicks = value; }
    [Category("02 Structure"), DisplayName("Afficher HH / HL / LH / LL")]
    public bool ShowLabels { get; set; } = true;
    [VolCustom(IsVisible = false, SerializeIgnore = true)]
    public bool ShowStems { get => LayoutExpert.ShowStems; set => LayoutExpert.ShowStems = value; }
    [VolCustom(IsVisible = false, SerializeIgnore = true)]
    public int MinorOpacity { get; set; } = 28;
    [VolCustom(IsVisible = false, SerializeIgnore = true)]
    public int MaxBubbles { get => LayoutExpert.MaxBubbles; set => LayoutExpert.MaxBubbles = value; }
    [VolCustom(IsVisible = false, SerializeIgnore = true)]
    public double GlyphCenterCorrection { get => LayoutExpert.GlyphCenterCorrection; set => LayoutExpert.GlyphCenterCorrection = value; }
    [Category("02 Structure"), DisplayName("HH / HL")]
    public ColorRef BullishColor { get; set; } = ColorRef.FromRgb(38, 166, 154);
    [Category("02 Structure"), DisplayName("LH / LL")]
    public ColorRef BearishColor { get; set; } = ColorRef.FromRgb(239, 83, 80);
    [VolCustom(IsVisible = false, SerializeIgnore = true)]
    public ColorRef NeutralColor { get; set; } = ColorRef.FromRgb(144, 164, 174);
    [Category("02 Structure"), DisplayName("Texte")]
    public ColorRef TextColor { get; set; } = ColorRef.FromRgb(240, 240, 240);
    [VolCustom(IsVisible = false, SerializeIgnore = true)]
    public bool ShowCvdDivergences { get; set; } = true;
    [VolCustom(IsVisible = false, SerializeIgnore = true)]
    public bool ShowTechnicalDivergences { get; set; } = true;
    [VolCustom(IsVisible = false, SerializeIgnore = true)]
    public TechnicalOscillator Oscillator { get; set; } = TechnicalOscillator.Rsi;
    [VolCustom(IsVisible = false, SerializeIgnore = true)]
    public bool ShowHiddenDivergences { get => DivergenceExpert.ShowHiddenDivergences; set => DivergenceExpert.ShowHiddenDivergences = value; }
    [VolCustom(IsVisible = false, SerializeIgnore = true)]
    public bool ShowWeakCvdDivergences { get => DivergenceExpert.ShowWeakCvdDivergences; set => DivergenceExpert.ShowWeakCvdDivergences = value; }
    [VolCustom(IsVisible = false, SerializeIgnore = true)]
    public bool CvdAtClose { get => DivergenceExpert.CvdAtClose; set => DivergenceExpert.CvdAtClose = value; }
    [VolCustom(IsVisible = false, SerializeIgnore = true)]
    public bool MinorDivergences { get => DivergenceExpert.MinorDivergences; set => DivergenceExpert.MinorDivergences = value; }
    [VolCustom(IsVisible = false, SerializeIgnore = true)]
    public int DivergenceMaxBars { get => DivergenceExpert.DivergenceMaxBars; set => DivergenceExpert.DivergenceMaxBars = value; }
    [VolCustom(IsVisible = false, SerializeIgnore = true)]
    public double CvdTolerance { get => DivergenceExpert.CvdTolerance; set => DivergenceExpert.CvdTolerance = value; }
    [VolCustom(IsVisible = false, SerializeIgnore = true)]
    public double OscillatorTolerance { get => DivergenceExpert.OscillatorTolerance; set => DivergenceExpert.OscillatorTolerance = value; }
    [VolCustom(IsVisible = false, SerializeIgnore = true)]
    public bool RequireClearPriceLine { get => DivergenceExpert.RequireClearPriceLine; set => DivergenceExpert.RequireClearPriceLine = value; }
    [VolCustom(IsVisible = false, SerializeIgnore = true)]
    public bool RequireClearCvdLine { get => DivergenceExpert.RequireClearCvdLine; set => DivergenceExpert.RequireClearCvdLine = value; }
    [VolCustom(IsVisible = false, SerializeIgnore = true)]
    public bool RequireHiddenTrend { get => DivergenceExpert.RequireHiddenTrend; set => DivergenceExpert.RequireHiddenTrend = value; }
    [VolCustom(IsVisible = false, SerializeIgnore = true)]
    public bool RequireTechnicalValidation { get => DivergenceExpert.RequireTechnicalValidation; set => DivergenceExpert.RequireTechnicalValidation = value; }
    [VolCustom(IsVisible = false, SerializeIgnore = true)]
    public int ValidationBars { get => DivergenceExpert.ValidationBars; set => DivergenceExpert.ValidationBars = value; }
    [VolCustom(IsVisible = false, SerializeIgnore = true)]
    public int OscillatorAlignmentBars { get => DivergenceExpert.OscillatorAlignmentBars; set => DivergenceExpert.OscillatorAlignmentBars = value; }
    [VolCustom(IsVisible = false, SerializeIgnore = true)]
    public int RsiPeriod { get => DivergenceExpert.RsiPeriod; set => DivergenceExpert.RsiPeriod = value; }
    [VolCustom(IsVisible = false, SerializeIgnore = true)]
    public int MacdFast { get => DivergenceExpert.MacdFast; set => DivergenceExpert.MacdFast = value; }
    [VolCustom(IsVisible = false, SerializeIgnore = true)]
    public int MacdSlow { get => DivergenceExpert.MacdSlow; set => DivergenceExpert.MacdSlow = value; }
    [VolCustom(IsVisible = false, SerializeIgnore = true)]
    public int MacdSignal { get => DivergenceExpert.MacdSignal; set => DivergenceExpert.MacdSignal = value; }
    [VolCustom(IsVisible = false, SerializeIgnore = true)]
    public int TrendPeriod { get => DivergenceExpert.TrendPeriod; set => DivergenceExpert.TrendPeriod = value; }
    [VolCustom(IsVisible = false, SerializeIgnore = true)]
    public int MaxDivergences { get => DivergenceExpert.MaxDivergences; set => DivergenceExpert.MaxDivergences = value; }
    [Category("01 Général"), DisplayName("Tracer les divergences sur le CVD")]
    [VolCustom(SerializeIgnore = true)]
    public bool DrawCvdLines { get => DivergenceExpert.DrawCvdLines; set => DivergenceExpert.DrawCvdLines = value; }
    [Category("01 Général"), DisplayName("Panneau CVD (2 = premier sous le prix)")]
    [VolCustom(SerializeIgnore = true)]
    public int CvdPanelNumber { get => DivergenceExpert.CvdPanelNumber; set => DivergenceExpert.CvdPanelNumber = value; }
    [Category("01 Général"), DisplayName("Identifiant du CVD (numéro entre parenthèses)")]
    public int CvdIndicatorId { get; set; } = 3;
    [Category("03 Bulles"), DisplayName("Activer les bulles")]
    public bool ShowDeepTrades { get; set; } = true;
    [Category("03 Bulles"), DisplayName("Volume minimum")]
    public double DeepTradeMinimum { get; set; } = 50;
    [VolCustom(IsVisible = false, SerializeIgnore = true)]
    public bool DeepTradeAdaptive { get; set; } = false;
    [VolCustom(IsVisible = false, SerializeIgnore = true)]
    public double DeepTradePercentile { get => DeepExpert.DeepTradePercentile; set => DeepExpert.DeepTradePercentile = value; }
    [VolCustom(IsVisible = false, SerializeIgnore = true)]
    public int DeepTradeSample { get => DeepExpert.DeepTradeSample; set => DeepExpert.DeepTradeSample = value; }
    [VolCustom(IsVisible = false, SerializeIgnore = true)]
    public int DeepAggregationMs { get => DeepExpert.DeepAggregationMs; set => DeepExpert.DeepAggregationMs = value; }
    [VolCustom(IsVisible = false)]
    public int DeepBubbleSize { get; set; } = 8;
    [VolCustom(IsVisible = false)]
    public int DeepBubbleMaxSize { get; set; } = 32;
    [VolCustom(IsVisible = false)]
    public bool DeepShowVolume { get; set; } = true;
    [VolCustom(IsVisible = false, SerializeIgnore = true)]
    public int DeepLabelZoomBars { get => DeepExpert.LabelZoomBars; set => DeepExpert.LabelZoomBars = value; }
    [VolCustom(IsVisible = false, SerializeIgnore = true)]
    public int MaxDeepTrades { get => DeepExpert.MaxDeepTrades; set => DeepExpert.MaxDeepTrades = value; }
    [VolCustom(IsVisible = false, SerializeIgnore = true)]
    public ColorRef DeepBuyColor { get => DeepExpert.DeepBuyColor; set => DeepExpert.DeepBuyColor = value; }
    [VolCustom(IsVisible = false, SerializeIgnore = true)]
    public ColorRef DeepSellColor { get => DeepExpert.DeepSellColor; set => DeepExpert.DeepSellColor = value; }
    [VolCustom(IsVisible = false, SerializeIgnore = true)]
    public bool ShowDiagnostics { get => LayoutExpert.ShowDiagnostics; set => LayoutExpert.ShowDiagnostics = value; }
    private void ClampSettings()
    {
        StructureExpert ??= new(); LayoutExpert ??= new(); DivergenceExpert ??= new(); DeepExpert ??= new();
        SetupExpert ??= new(); CvdSound ??= new() { SelValue = "Alert 1" };
        ClampCvdCategories();
        CvdIndicatorId = Math.Max(0, CvdIndicatorId);
        AlertTiming = CvdAlertTiming.SetupConfirme;
        AlertVersion = 19;
        SetupExpert.SwingAtr = double.IsFinite(SetupExpert.SwingAtr) ? Math.Clamp(SetupExpert.SwingAtr, .25, 10) : 2;
        SetupExpert.AtrPeriod = Math.Clamp(SetupExpert.AtrPeriod, 1, 100);
        if (StructureVersion < 7)
        {
            if (Detection == StructureDetection.AtrRetracement) Detection = StructureDetection.ReactiveSwings;
            if (SwingAtr == 2) SwingAtr = 1.25;
            StructureVersion = 7;
        }
        // Upgrade v4 templates once: otherwise their saved chart-space circles and opaque fills persist.
        if (AppearanceVersion < 5)
        {
            BubbleSize = 16; MinorBubbleSize = 12; FontSize = 9; MinorFontSize = 8;
            StructureOpacityPercent = 15; OffsetTicks = 4; MinorOffsetTicks = 3;
            DeepBubbleSize = 8; DeepBubbleMaxSize = 40; DeepOpacityMinPercent = 5; DeepOpacityMaxPercent = 20;
            DeepTradeMinimum = 50; DeepTradeMaximum = 0; DeepFilterMode = TradeFilterMode.Manuel;
            TextColor = ColorRef.FromRgb(240, 240, 240); AppearanceVersion = 5;
        }
        if (AppearanceVersion < 6)
        {
            if (DeepBubbleSize <= 8) DeepBubbleSize = 12;
            if (DeepBubbleMaxSize <= 40) DeepBubbleMaxSize = 52;
            AppearanceVersion = 6;
        }
        if (AppearanceVersion < 8)
        {
            DeepBubbleSize = 20; DeepBubbleMaxSize = 120;
            DeepOpacityMinPercent = 8; DeepOpacityMaxPercent = 22;
            DeepExpert.BorderOpacityPercent = 75; AppearanceVersion = 8;
        }
        if (AppearanceVersion < 9)
        {
            DeepBubbleSize = 8; DeepBubbleMaxSize = 32;
            DeepOpacityMinPercent = 5; DeepOpacityMaxPercent = 14;
            AppearanceVersion = 9;
        }
        ClampTradePlot();
        if (StructureVersion < 9)
        {
            Detection = StructureDetection.ReactiveSwings;
            DirectionalBars = 2; LiveStructurePreview = true; DrawCvdLines = true;
            if (DivergenceSource == DivergenceSource.CvdEtRsi) DivergenceSource = DivergenceSource.Cvd;
            StructureVersion = 9;
        }
        if (StructureVersion < 12)
        {
            if (Detection == StructureDetection.ReactiveSwings && SwingAtr == 1.25) SwingAtr = .75;
            StructureVersion = 12;
        }
        if (StructureVersion < 17)
        {
            if (Detection == StructureDetection.ReactiveSwings) Detection = StructureDetection.AlternatingLegs;
            StructureVersion = 17;
        }
        DirectionalBars = Math.Clamp(DirectionalBars, 2, 3);
        // Circle.Width isn't a pixel size in this renderer, regardless of UsePixelRayMarker.
        Drawing = BubbleDrawing.OpticalGlyph;
        ShowCvdDivergences = ShowDivergences && DivergenceSource is DivergenceSource.Cvd or DivergenceSource.CvdEtRsi or DivergenceSource.CvdEtMacd;
        ShowTechnicalDivergences = ShowDivergences && DivergenceSource != DivergenceSource.Cvd;
        Oscillator = DivergenceSource is DivergenceSource.Macd or DivergenceSource.CvdEtMacd ? TechnicalOscillator.MacdHistogram : TechnicalOscillator.Rsi;
        DeepTradeAdaptive = DeepFilterMode == TradeFilterMode.Percentile;
        LeftBars = Math.Clamp(LeftBars, 1, 1000); RightBars = Math.Clamp(RightBars, 1, 1000);
        MinorLeftBars = Math.Clamp(MinorLeftBars, 1, LeftBars); MinorRightBars = Math.Clamp(MinorRightBars, 1, RightBars);
        AtrPeriod = Math.Clamp(AtrPeriod, 2, 200); SwingAtr = Math.Clamp(SwingAtr, 0.1, 20);
        MinorSwingAtr = Math.Clamp(MinorSwingAtr, 0.1, SwingAtr); MinimumSwingAtr = Math.Clamp(MinimumSwingAtr, 0, 20);
        PriceToleranceTicks = Math.Clamp(PriceToleranceTicks, 0, 100);
        BubbleSize = Math.Clamp(BubbleSize, 8, 40); MinorBubbleSize = Math.Clamp(MinorBubbleSize, 6, 30);
        FontSize = Math.Clamp(FontSize, 7, Math.Max(9, (int)(BubbleSize * .6)));
        MinorFontSize = Math.Clamp(MinorFontSize, 7, Math.Max(8, (int)(MinorBubbleSize * .6)));
        StructureOpacityPercent = Math.Clamp(StructureOpacityPercent, 0, 100);
        MinorOpacity = (int)Math.Round(StructureOpacityPercent * .75 * 255 / 100);
        OffsetTicks = Math.Clamp(OffsetTicks, 1, 1000); MinorOffsetTicks = Math.Clamp(MinorOffsetTicks, 1, 1000);
        MinorOpacity = Math.Clamp(MinorOpacity, 0, 255); MaxBubbles = Math.Clamp(MaxBubbles, 10, 10000);
        GlyphCenterCorrection = Math.Clamp(GlyphCenterCorrection, -0.5, 0.5);
        DivergenceMaxBars = Math.Clamp(DivergenceMaxBars, 1, Divergences.MaximumPivotDistance); ValidationBars = Math.Clamp(ValidationBars, 0, 500);
        DivergenceExpert.CvdLeftBars = Math.Clamp(DivergenceExpert.CvdLeftBars, 1, 6);
        DivergenceExpert.CvdRightBars = Math.Clamp(DivergenceExpert.CvdRightBars, 1, 3);
        DivergenceExpert.CvdAlignmentBars = Math.Clamp(DivergenceExpert.CvdAlignmentBars, 0, DivergenceExpert.CvdRightBars);
        StructureExpert.MinimumReversalTicks = Math.Clamp(StructureExpert.MinimumReversalTicks, 1, 1000);
        RsiPeriod = Math.Clamp(RsiPeriod, 2, 200); TrendPeriod = Math.Clamp(TrendPeriod, 2, 500);
        MacdFast = Math.Clamp(MacdFast, 1, 199); MacdSlow = Math.Clamp(MacdSlow, MacdFast + 1, 500);
        MacdSignal = Math.Clamp(MacdSignal, 1, 200);
        OscillatorAlignmentBars = Math.Clamp(OscillatorAlignmentBars, 0, Math.Min(RightBars, MinorRightBars));
        CvdTolerance = Math.Max(0, CvdTolerance); OscillatorTolerance = Math.Max(0, OscillatorTolerance);
        MaxDivergences = Math.Clamp(MaxDivergences, 1, 1000); CvdPanelNumber = Math.Clamp(CvdPanelNumber, 2, 20);
        DeepTradeMinimum = Math.Max(1, DeepTradeMinimum); DeepTradePercentile = Math.Clamp(DeepTradePercentile, 0, 100);
        DeepTradeSample = Math.Clamp(DeepTradeSample, 50, 10000); DeepAggregationMs = Math.Clamp(DeepAggregationMs, 0, 1000);
        DeepBubbleSize = Math.Clamp(DeepBubbleSize, 4, 16);
        DeepBubbleMaxSize = Math.Clamp(DeepBubbleMaxSize, DeepBubbleSize, 40); MaxDeepTrades = Math.Clamp(MaxDeepTrades, 1, 5000);
        DeepExpert.BorderOpacityPercent = Math.Clamp(DeepExpert.BorderOpacityPercent, 0, 100);
        DeepOpacityMinPercent = Math.Clamp(DeepOpacityMinPercent, 0, 50);
        DeepOpacityMaxPercent = Math.Clamp(DeepOpacityMaxPercent, DeepOpacityMinPercent, 75);
        DeepTradeMaximum = Math.Max(0, DeepTradeMaximum); DeepLabelZoomBars = Math.Clamp(DeepLabelZoomBars, 30, 3000);
        DeepDaysToLoad = Math.Clamp(DeepDaysToLoad, 1, 365);
        SetupRewardRisk = Math.Clamp(SetupRewardRisk, .25, 20);
        SetupExpert.StopAtr = Math.Clamp(SetupExpert.StopAtr, 0, 10);
        SetupExpert.MinimumStopTicks = Math.Clamp(SetupExpert.MinimumStopTicks, 1, 1000);
        SetupExpert.WaitingBars = Math.Clamp(SetupExpert.WaitingBars, 0, 500);
        SetupExpert.ProjectionBars = Math.Clamp(SetupExpert.ProjectionBars, 1, 200);
        SetupExpert.MaxSetups = Math.Clamp(SetupExpert.MaxSetups, 1, 100);
        SetupExpert.CooldownSeconds = Math.Clamp(SetupExpert.CooldownSeconds, 0, 3600);
    }
}


public class StructureExpert
{
    [DisplayName("Méthode de détection")]
    public StructureDetection Detection { get; set; } = StructureDetection.AlternatingLegs;
    [DisplayName("Retracement minimum (ticks) — mode réactif")]
    public int MinimumReversalTicks { get; set; } = 2;
    [DisplayName("Période ATR")]
    public int AtrPeriod { get; set; } = 14;
    [DisplayName("Gauche — mode pivots filtrés")]
    public int LeftBars { get; set; } = 5;
    [DisplayName("Droite — mode pivots filtrés")]
    public int RightBars { get; set; } = 5;
    [DisplayName("Amplitude minimale — mode pivots (ATR)")]
    public double MinimumSwingAtr { get; set; } = 0.5;
    [DisplayName("Tolérance du prix (ticks)")]
    public double PriceToleranceTicks { get; set; } = 1;
    [DisplayName("Retracement secondaire (ATR)")]
    public double MinorSwingAtr { get; set; } = 1;
    [DisplayName("Gauche — mode pivots filtrés")]
    public int MinorLeftBars { get; set; } = 2;
    [DisplayName("Droite — mode pivots filtrés")]
    public int MinorRightBars { get; set; } = 2;
    [DisplayName("Premiers pivots H / L")]
    public bool ShowInitialPivots { get; set; } = false;
}

public class LayoutExpert
{
    [DisplayName("Afficher le point en formation (provisoire, ?)")]
    public bool LiveStructurePreview { get; set; } = true;
    [DisplayName("Afficher les compteurs de diagnostic")]
    public bool ShowDiagnostics { get; set; } = false;
    [DisplayName("Diamètre secondaire (pixels)")]
    public int MinorBubbleSize { get; set; } = 12;
    [DisplayName("Taille du texte secondaire")]
    public int MinorFontSize { get; set; } = 8;
    [DisplayName("Écart au sommet / creux (ticks)")]
    public int OffsetTicks { get; set; } = 4;
    [DisplayName("Écart secondaire (ticks)")]
    public int MinorOffsetTicks { get; set; } = 3;
    [DisplayName("Relier aux mèches")]
    public bool ShowStems { get; set; } = false;
    [DisplayName("Nombre maximal de structures")]
    public int MaxBubbles { get; set; } = 600;
    [DisplayName("Correction optique — mode caractère")]
    public double GlyphCenterCorrection { get; set; } = 0.15;
}

public class DivergenceExpert
{
    [VolCustom(IsVisible = false)]
    public int CvdLeftBars { get; set; } = 2;
    [VolCustom(IsVisible = false)]
    public int CvdRightBars { get; set; } = 1;
    [VolCustom(IsVisible = false)]
    public int CvdAlignmentBars { get; set; } = 1;
    [DisplayName("Divergences cachées / absorption")]
    public bool ShowHiddenDivergences { get; set; } = true;
    [DisplayName("Inclure les divergences CVD faibles")]
    public bool ShowWeakCvdDivergences { get; set; } = false;
    [DisplayName("CVD à la clôture (sinon extrêmes)")]
    public bool CvdAtClose { get; set; } = false;
    [DisplayName("Calcul sur la structure secondaire")]
    public bool MinorDivergences { get; set; } = false;
    [DisplayName("Écart maximal entre pivots (12 max)")]
    [VolCustom(MinValue = 1, MaxValue = 12, IncrementValue = 1)]
    public int DivergenceMaxBars { get; set; } = 12;
    [DisplayName("Tolérance CVD")]
    public double CvdTolerance { get; set; } = 1;
    [DisplayName("Tolérance RSI / MACD")]
    public double OscillatorTolerance { get; set; } = 0.25;
    [DisplayName("Ligne de prix sans traversée de bougie")]
    public bool RequireClearPriceLine { get; set; } = true;
    [DisplayName("Ligne CVD sans traversée")]
    public bool RequireClearCvdLine { get; set; } = true;
    [DisplayName("Tendance requise pour les cachées RSI/MACD")]
    public bool RequireHiddenTrend { get; set; } = true;
    [DisplayName("Validation RSI/MACD par BOS ou balayage")]
    public bool RequireTechnicalValidation { get; set; } = true;
    [DisplayName("Délai maximal de validation (bougies)")]
    public int ValidationBars { get; set; } = 20;
    [DisplayName("Alignement de l'oscillateur (bougies)")]
    public int OscillatorAlignmentBars { get; set; } = 1;
    [DisplayName("Période RSI")]
    public int RsiPeriod { get; set; } = 14;
    [DisplayName("MACD rapide")]
    public int MacdFast { get; set; } = 12;
    [DisplayName("MACD lent")]
    public int MacdSlow { get; set; } = 26;
    [DisplayName("MACD signal")]
    public int MacdSignal { get; set; } = 9;
    [DisplayName("EMA de tendance")]
    public int TrendPeriod { get; set; } = 50;
    [DisplayName("Nombre maximal de divergences")]
    public int MaxDivergences { get; set; } = 150;
    [DisplayName("Tracer les lignes sur le panneau CVD")]
    public bool DrawCvdLines { get; set; } = true;
    [DisplayName("Numéro du panneau CVD (2 = premier panneau)")]
    public int CvdPanelNumber { get; set; } = 2;
}

public class DeepExpert
{
    [DisplayName("Opacité du contour (%)")]
    public int BorderOpacityPercent { get; set; } = 75;
    [DisplayName("Zoom pour afficher les volumes (bougies visibles)")]
    public int LabelZoomBars { get; set; } = 300;
    [DisplayName("Percentile du seuil")]
    public double DeepTradePercentile { get; set; } = 99;
    [DisplayName("Ordres mémorisés pour le percentile")]
    public int DeepTradeSample { get; set; } = 2000;
    [DisplayName("Fenêtre d'agrégation (ms, 0 = même timestamp)")]
    public int DeepAggregationMs { get; set; } = 0;
    [DisplayName("Nombre maximal de bulles")]
    public int MaxDeepTrades { get; set; } = 500;
    [DisplayName("Achats agressifs")]
    public ColorRef DeepBuyColor { get; set; } = ColorRef.FromRgb(17, 117, 17);
    [DisplayName("Ventes agressives")]
    public ColorRef DeepSellColor { get; set; } = ColorRef.FromRgb(112, 35, 171);
}
