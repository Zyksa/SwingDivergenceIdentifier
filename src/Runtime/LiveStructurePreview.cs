using SwingDivergence.Analysis;
using VolumetricaAPI.Chart;

namespace SwingDivergence;

public partial class SwingDivergenceIdentifier
{
    private readonly List<IAnnotation> livePreviewItems = new();
    private (SwingPoint? Major, SwingPoint? Minor)? lastPreview;

    private void ClearLivePreview()
    {
        if (annotations != null) RemoveItems(livePreviewItems);
        livePreviewItems.Clear(); lastPreview = null;
    }

    private void UpdateLivePreview(int index, double? tickPrice = null)
    {
        bool alternating = Detection == StructureDetection.AlternatingLegs;
        if (!LiveStructurePreview || !ShowStructure || (!alternating && Detection != StructureDetection.ReactiveSwings)
            || annotations == null || majorReactive == null || index <= lastClosedIndex || index < 0 || index >= VAn.BarVars.Count)
        { ClearLivePreview(); return; }
        var bar = VAn.BarVars[index];
        if (bar.IsNewDay && index > 0) { ClearLivePreview(); return; }
        double close = tickPrice.HasValue && double.IsFinite(tickPrice.Value) ? tickPrice.Value : bar.Close;
        var snapshot = new MarketFrame(index, sessionNumber, bar.Open, Math.Max(bar.High, close),
            Math.Min(bar.Low, close), close, 0, null, null, null, null, null, null, null, null);
        var major = alternating ? majorLegs.Preview(snapshot) : majorReactive.Preview(snapshot);
        var minor = ShowMinorStructure ? alternating ? minorLegs.Preview(snapshot) : minorReactive.Preview(snapshot) : null;
        if (lastPreview.HasValue && lastPreview.Value == (major, minor)) return;
        ClearLivePreview(); lastPreview = (major, minor);
        AddPreview(major, false);
        if (!major.HasValue || !minor.HasValue || major.Value.Index != minor.Value.Index || major.Value.High != minor.Value.High)
            AddPreview(minor, true);
    }

    private void AddPreview(SwingPoint? candidate, bool minor)
    {
        if (!candidate.HasValue) return;
        var point = candidate.Value;
        bool shown = point.Label switch
        { "HH" => ShowHH, "HL" => ShowHL, "LH" => ShowLH, "LL" => ShowLL, _ => ShowInitialPivots };
        if (!shown) return;
        ColorRef color = point.Label is "HH" or "HL" ? BullishColor : point.Label is "LH" or "LL" ? BearishColor : NeutralColor;
        int offset = minor ? MinorOffsetTicks : OffsetTicks;
        double y = VAn.DoubleAddSubTicks(point.Price, point.High ? offset : -offset);
        var items = DrawDisk(point.Index + 1, y, minor ? MinorBubbleSize : BubbleSize, color, 0,
            ShowLabels ? point.Label + "?" : null, 7, TextColor.WithOpacity(160),
            "Point en formation, non confirmé. Peut se déplacer ou disparaître. Aucune divergence ni alerte basée sur ce point.");
        livePreviewItems.AddRange(items);
    }
}
