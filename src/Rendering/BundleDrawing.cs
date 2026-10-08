using SwingDivergence.Analysis;
using VolumetricaAPI.Chart;

namespace SwingDivergence;

public partial class SwingDivergenceIdentifier
{
    private readonly struct StructureDrawing : IEquatable<StructureDrawing>
    {
        public SwingPoint Point { get; init; }
        public bool Minor { get; init; }
        public List<IAnnotation> Items { get; init; }

        public StructureDrawing(SwingPoint Point, bool Minor, List<IAnnotation> Items)
        {
            this.Point = Point;
            this.Minor = Minor;
            this.Items = Items;
        }

        public bool Equals(StructureDrawing other) =>
            EqualityComparer<SwingPoint>.Default.Equals(Point, other.Point) &&
            EqualityComparer<bool>.Default.Equals(Minor, other.Minor) &&
            EqualityComparer<List<IAnnotation>>.Default.Equals(Items, other.Items);

        public override bool Equals(object obj) => obj is StructureDrawing other && Equals(other);
        public static bool operator ==(StructureDrawing left, StructureDrawing right) => left.Equals(right);
        public static bool operator !=(StructureDrawing left, StructureDrawing right) => !left.Equals(right);

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 31 + EqualityComparer<SwingPoint>.Default.GetHashCode(Point);
                hash = hash * 31 + EqualityComparer<bool>.Default.GetHashCode(Minor);
                hash = hash * 31 + EqualityComparer<List<IAnnotation>>.Default.GetHashCode(Items);
                return hash;
            }
        }

        public void Deconstruct(out SwingPoint Point, out bool Minor, out List<IAnnotation> Items)
        {
            Point = this.Point;
            Minor = this.Minor;
            Items = this.Items;
        }

        public override string ToString() => $"StructureDrawing {{ Point = {Point}, Minor = {Minor}, Items = {Items} }}";
    }
    private readonly struct SignalDrawing : IEquatable<SignalDrawing>
    {
        public DivergenceSignal Signal { get; init; }
        public bool Minor { get; init; }
        public List<IAnnotation> Items { get; init; }

        public bool HasStructure { get; init; }

        public SignalDrawing(DivergenceSignal Signal, bool Minor, List<IAnnotation> Items, bool HasStructure)
        {
            this.Signal = Signal;
            this.Minor = Minor;
            this.Items = Items;
            this.HasStructure = HasStructure;
        }

        public bool Equals(SignalDrawing other) =>
            EqualityComparer<DivergenceSignal>.Default.Equals(Signal, other.Signal) &&
            EqualityComparer<bool>.Default.Equals(Minor, other.Minor) &&
            EqualityComparer<List<IAnnotation>>.Default.Equals(Items, other.Items) && HasStructure == other.HasStructure;

        public override bool Equals(object obj) => obj is SignalDrawing other && Equals(other);
        public static bool operator ==(SignalDrawing left, SignalDrawing right) => left.Equals(right);
        public static bool operator !=(SignalDrawing left, SignalDrawing right) => !left.Equals(right);

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 31 + EqualityComparer<DivergenceSignal>.Default.GetHashCode(Signal);
                hash = hash * 31 + EqualityComparer<bool>.Default.GetHashCode(Minor);
                hash = hash * 31 + EqualityComparer<List<IAnnotation>>.Default.GetHashCode(Items);
                hash = hash * 31 + HasStructure.GetHashCode();
                return hash;
            }
        }

        public void Deconstruct(out DivergenceSignal Signal, out bool Minor, out List<IAnnotation> Items)
        {
            Signal = this.Signal;
            Minor = this.Minor;
            Items = this.Items;
        }

        public override string ToString() => $"SignalDrawing {{ Signal = {Signal}, Minor = {Minor}, Items = {Items} }}";
    }
    private readonly struct DeepDrawing : IEquatable<DeepDrawing>
    {
        public string Key { get; init; }
        public DeepTrade Trade { get; init; }
        public List<IAnnotation> Items { get; init; }

        public DeepDrawing(string Key, DeepTrade Trade, List<IAnnotation> Items)
        {
            this.Key = Key;
            this.Trade = Trade;
            this.Items = Items;
        }

        public bool Equals(DeepDrawing other) =>
            EqualityComparer<string>.Default.Equals(Key, other.Key) &&
            EqualityComparer<DeepTrade>.Default.Equals(Trade, other.Trade) &&
            EqualityComparer<List<IAnnotation>>.Default.Equals(Items, other.Items);

        public override bool Equals(object obj) => obj is DeepDrawing other && Equals(other);
        public static bool operator ==(DeepDrawing left, DeepDrawing right) => left.Equals(right);
        public static bool operator !=(DeepDrawing left, DeepDrawing right) => !left.Equals(right);

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 31 + EqualityComparer<string>.Default.GetHashCode(Key);
                hash = hash * 31 + EqualityComparer<DeepTrade>.Default.GetHashCode(Trade);
                hash = hash * 31 + EqualityComparer<List<IAnnotation>>.Default.GetHashCode(Items);
                return hash;
            }
        }

        public void Deconstruct(out string Key, out DeepTrade Trade, out List<IAnnotation> Items)
        {
            Key = this.Key;
            Trade = this.Trade;
            Items = this.Items;
        }

        public override string ToString() => $"DeepDrawing {{ Key = {Key}, Trade = {Trade}, Items = {Items} }}";
    }
    private readonly LinkedList<StructureDrawing> structureBubbles = new();
    private readonly LinkedList<SignalDrawing> divergenceDrawings = new();
    private readonly LinkedList<DeepDrawing> deepDrawings = new();
    private readonly Dictionary<string, LinkedListNode<DeepDrawing>> deepByKey = new();

    private void DrawStructure(SwingPoint point, bool minor)
    {
        if (!ShowStructure) return;
        bool show = point.Label switch
        {
            "HH" => ShowHH, "HL" => ShowHL, "LH" => ShowLH, "LL" => ShowLL, _ => ShowInitialPivots
        };
        if (!show) return;
        for (var node = structureBubbles.First; node != null; node = node.Next)
        {
            if (node.Value.Point.Index != point.Index || node.Value.Point.High != point.High) continue;
            if (minor || !node.Value.Minor) return;
            RemoveItems(node.Value.Items); structureBubbles.Remove(node); break;
        }
        ColorRef color = point.Label is "HH" or "HL" ? BullishColor
            : point.Label is "LH" or "LL" ? BearishColor : NeutralColor;
        int offset = minor ? MinorOffsetTicks : OffsetTicks;
        double x = point.Index + 1;
        double y = VAn.DoubleAddSubTicks(point.Price, point.High ? offset : -offset);
        string tooltip = $"{point.Label} — {(minor ? "secondaire" : "principale")}\nPrix : {point.Price}"
            + $"\nPivot : bougie {point.Index + 1}\nConnu à la clôture de la bougie {point.ConfirmedAt + 1}";
        var items = new List<IAnnotation>();
        if (ShowStems)
            items.Add(AddLine(x, point.Price, x, y, color.WithOpacity(100), DashStyle.Solid, tooltip));
        items.AddRange(DrawDisk(x, y, minor ? MinorBubbleSize : BubbleSize, color,
            minor ? (byte)MinorOpacity : (byte)Math.Round(StructureOpacityPercent * 255 / 100.0), ShowLabels ? point.Label : null,
            minor ? MinorFontSize : FontSize, TextColor, tooltip));
        structureBubbles.AddLast(new StructureDrawing(point, minor, items));
        while (structureBubbles.Count > MaxBubbles)
        {
            RemoveItems(structureBubbles.First.Value.Items); structureBubbles.RemoveFirst();
        }
    }

    private List<IAnnotation> DrawDisk(double x, double y, int diameter, ColorRef color,
        byte opacity, string text, int fontSize, ColorRef textColor, string tooltip)
    {
        var result = new List<IAnnotation>();
        // These are screen-size font glyphs. Never use native Circle.Width: the
        // installed chart renderer scales it in price units, even with its pixel-marker flag.
        var disk = VAn.CreateAnnotation(AnnotationType.Text);
        Place(disk, x, y);
        disk.Text = "●"; disk.TextAlign = TextAlignment.VCenterHCenter;
        disk.FontSize = (float)(diameter / 0.55);
        disk.ForeColor = color.WithOpacity(opacity); disk.Visible = opacity > 0;
        disk.Margin = GlyphCenterCorrection >= 0 ? MarginEnum.Y : MarginEnum.Y2;
        disk.PixelMargin = (float)(Math.Abs(GlyphCenterCorrection) * Math.Max(0, disk.FontSize - fontSize));
        disk.BackColor = disk.LineColor = ColorRef.FromArgb(0, 0, 0, 0);
        disk.Tooltip = tooltip;
        VAn.AddAnnotation(annotations, disk); result.Add(disk);
        var outline = VAn.CreateAnnotation(AnnotationType.Text); Place(outline, x, y);
        outline.Text = "○"; outline.TextAlign = TextAlignment.VCenterHCenter;
        outline.FontSize = (float)(diameter / 0.72); outline.ForeColor = color.WithOpacity(160);
        outline.Margin = disk.Margin;
        outline.PixelMargin = (float)(Math.Abs(GlyphCenterCorrection) * Math.Max(0, outline.FontSize - fontSize));
        outline.BackColor = outline.LineColor = ColorRef.FromArgb(0, 0, 0, 0);
        outline.Tooltip = tooltip;
        VAn.AddAnnotation(annotations, outline); result.Add(outline);
        if (!string.IsNullOrEmpty(text))
        {
            var label = AddText(x, y, text, textColor, fontSize, tooltip);
            result.Add(label);
        }
        return result;
    }

    private void DrawDivergence(DivergenceSignal signal, bool minor, bool refresh = false)
    {
        bool cvd = signal.Source == "CVD";
        bool withStructure = !cvd || cvdStructure.HasStructure(signal);
        var categorySettings = cvd ? CvdCategory(withStructure) : null;
        bool shown = !cvd || categorySettings.Enabled && categorySettings.ShowLines;
        for (var node = divergenceDrawings.First; node != null; node = node.Next)
        {
            var existing = node.Value;
            if (existing.Minor != minor || existing.Signal.Id != signal.Id) continue;
            if (shown && !refresh && existing.Signal == signal && existing.HasStructure == withStructure) return;
            if (existing.Signal.Validation == "Structure CVD confirmée"
                && signal.Validation != "Structure CVD confirmée" && shown && !refresh) return;
            RemoveItems(existing.Items); divergenceDrawings.Remove(node); break;
        }
        if (!shown) return;
        ColorRef color = !withStructure ? ColorRef.FromRgb(244, 114, 182) : signal.Bullish ? BullishColor : BearishColor;
        if (cvd) color = color.WithOpacity((byte)Math.Round(categorySettings.OpacityPercent * 255 / 100.0));
        var items = new List<IAnnotation>();
        bool early = signal.Validation == StructuralCvdSetups.EarlyValidation;
        string category = signal.Source == "CVD" ? signal.Hidden ? "ABS" : "EXH" : signal.Hidden ? "HID" : "REG";
        string text = $"{signal.Source} {category} {(signal.Bullish ? "↑" : "↓")}{(signal.Weak || early ? " ?" : "")}";
        string tooltip = $"{text}\nPrix : {signal.First.Price} → {signal.Second.Price}"
            + $"\n{signal.Source} : {signal.Value1:0.##} → {signal.Value2:0.##}"
            + $"\n{signal.Validation}\n{(early ? "Détecté pendant" : "Disponible à la clôture de")} la bougie {signal.ConfirmedAt + 1}";
        DashStyle style = signal.Weak || early ? DashStyle.Dot : signal.Hidden ? DashStyle.Dash : DashStyle.Solid;
        items.Add(AddLine(signal.First.Index + 1, signal.First.Price, signal.Second.Index + 1,
            signal.Second.Price, color, style, tooltip));
        double y = VAn.DoubleAddSubTicks(signal.Second.Price, signal.Second.High ? OffsetTicks + 12 : -OffsetTicks - 12);
        if (withStructure) items.Add(AddText(signal.Second.Index + 1, y, text, color, 12, tooltip));
        if (signal.Source == "CVD" && DrawCvdLines)
        {
            var line = VAn.CreateAnnotation(AnnotationType.Line);
            Place(line, (signal.ValueIndex1 >= 0 ? signal.ValueIndex1 : signal.First.Index) + 1, signal.Value1);
            line.X2 = (signal.ValueIndex2 >= 0 ? signal.ValueIndex2 : signal.Second.Index) + 1; line.Y2 = signal.Value2;
            line.Area = ResolveCvdArea();
            line.LineWidth = 2; line.LineColor = color; line.LineDashStyle = style; line.Tooltip = tooltip;
            VAn.AddAnnotation(annotations, line); items.Add(line);
        }
        divergenceDrawings.AddLast(new SignalDrawing(signal, minor, items, withStructure));
        while (divergenceDrawings.Count > MaxDivergences)
        {
            RemoveItems(divergenceDrawings.First.Value.Items); divergenceDrawings.RemoveFirst();
        }
    }

    private void DrawDeep(DeepTrade? trade, string key = null, bool remember = true, bool refresh = false)
    {
        if (!ShowDeepTrades || !trade.HasValue || annotations == null) return;
        var order = trade.Value;
        long barTime = VAn.BarVars[order.BarIndex].exchDt.Ticks;
        key ??= order.OrderId != 0 ? $"raw:{barTime}:{order.Time}:{order.OrderId}:{order.Buy}"
            : $"raw:{barTime}:{order.Time}:{order.Buy}:{order.Price:R}:{order.Volume:R}:{order.Prints}";
        if (remember) tradeHistory.Remember(key, barTime, order);
        LinkedListNode<DeepDrawing> existing = null;
        if (key != null) deepByKey.TryGetValue(key, out existing);
        double threshold = DeepTradeAdaptive ? order.Threshold : DeepTradeMinimum;
        if (order.Volume < threshold || DeepTradeMaximum > 0 && order.Volume > DeepTradeMaximum)
        {
            if (existing != null) RemoveDeep(existing);
            return;
        }
        if (existing != null)
        {
            order = order with { BarIndex = existing.Value.Trade.BarIndex, Time = existing.Value.Trade.Time };
            if (existing.Value.Trade == order && !refresh) return;
            RemoveDeep(existing);
        }
        tradeSizeScale.Record(key, order.Volume);
        double upper = tradeSizeScale.Upper(TradePlot.StandardDeviation);
        UpdateTradeScale(upper);
        var appearance = DeepTradeAppearance.Calibrated(order.Volume, upper, tickSize,
            TradePlot.MinimumSize, TradePlot.MaximumSize, TradePlot.MinimumOpacity, TradePlot.MaximumOpacity,
            TradePlot.TextSize, DeepLabelZoomBars, DeepExpert.BorderOpacityPercent, TradePlot.RadiusScale);
        double price = Math.Round(order.Price / tickSize) * tickSize;
        string tooltip = $"DT {(order.Buy ? "achat agressif" : "vente agressive")}\nVolume : {order.Volume:0.##}"
            + $"\n{order.Prints} transactions\nPrix : {order.Low} → {order.High}\nSeuil : {order.Threshold:0.##}";
        var color = order.Buy ? DeepBuyColor : DeepSellColor;
        var circle = VAn.CreateAnnotation(AnnotationType.Circle);
        Place(circle, order.BarIndex + 1, price);
        circle.PlotForeground = false;
        // Native Circle.Width is a PRICE radius, not pixels. Tick-based radii are
        // intentional for trades: they scale with the price-axis zoom like the reference.
        circle.Width = circle.Height = appearance.PriceRadius;
        circle.UsePixelRayMarker = false;
        circle.LineWidth = 1;
        circle.LineColor = color.WithOpacity(appearance.BorderOpacity);
        circle.BackColor = color.WithOpacity(appearance.FillOpacity);
        circle.Tooltip = tooltip;
        // Circles below candles, numbers above: a large execution must not cover
        // the candle bodies or previously drawn volume labels.
        VAn.AddAnnotation(IndVars.Ann_List, circle);
        var items = new List<IAnnotation> { circle };
        if (DeepShowVolume && TradePlot.ShowText)
        {
            var label = AddText(circle.X, circle.Y, order.Volume.ToString("0.#"),
                ColorRef.FromRgb(255, 255, 255), appearance.FontSize, tooltip);
            label.FontBold = false;
            label.TextOnlyInside = false;
            label.MaxBarsViewed = appearance.LabelMaximumBars;
            items.Add(label);
        }
        var node = deepDrawings.AddLast(new DeepDrawing(key, order, items)); deepByKey[key] = node;
        while (deepDrawings.Count > MaxDeepTrades) RemoveDeep(deepDrawings.First);
    }

    private ChartAreaRef ResolveCvdArea()
    {
        var target = VAn.GetParamsById(CvdIndicatorId);
        if (target != null && !string.IsNullOrWhiteSpace(target.name)
            && (target.name.Contains("CVD", StringComparison.OrdinalIgnoreCase)
                || target.name.Contains("Delta", StringComparison.OrdinalIgnoreCase) && target.name.Contains("Cumul", StringComparison.OrdinalIgnoreCase)))
            return ChartAreaRef.Indicator(target.C_Area, target.IsVerticalCA, target.IsStackedCA, target.IndexAxis);
        return ChartAreaRef.Indicator(CvdPanelNumber - 2);
    }

    private void RemoveDeep(LinkedListNode<DeepDrawing> node)
    {
        RemoveItems(node.Value.Items); deepByKey.Remove(node.Value.Key); deepDrawings.Remove(node);
    }

    private void ForgetSwing(SwingPoint point, bool minor)
    {
        for (var node = structureBubbles.First; node != null;)
        {
            var next = node.Next;
            if (node.Value.Minor == minor && node.Value.Point.Index == point.Index)
            { RemoveItems(node.Value.Items); structureBubbles.Remove(node); }
            node = next;
        }
        for (var node = divergenceDrawings.First; node != null;)
        {
            var next = node.Next;
            if (node.Value.Minor == minor
                && (node.Value.Signal.First.Index == point.Index || node.Value.Signal.Second.Index == point.Index))
            { RemoveItems(node.Value.Items); divergenceDrawings.Remove(node); }
            node = next;
        }
    }

    private IAnnotation AddText(double x, double y, string text, ColorRef color, int size, string tooltip)
    {
        var label = VAn.CreateAnnotation(AnnotationType.Text); Place(label, x, y);
        label.Text = text; label.TextAlign = TextAlignment.VCenterHCenter;
        label.FontSize = size; label.FontBold = true; label.ForeColor = color;
        label.Margin = MarginEnum.None; label.Padding = PaddingEnum.None;
        label.BackColor = label.LineColor = ColorRef.FromArgb(0, 0, 0, 0);
        label.Tooltip = tooltip;
        VAn.AddAnnotation(annotations, label); return label;
    }

    private IAnnotation AddLine(double x, double y, double x2, double y2, ColorRef color, DashStyle style, string tooltip)
    {
        var line = VAn.CreateAnnotation(AnnotationType.Line); Place(line, x, y);
        line.X2 = x2; line.Y2 = y2; line.LineWidth = 2; line.LineColor = color;
        line.LineDashStyle = style; line.IsExtended = false; line.Tooltip = tooltip;
        VAn.AddAnnotation(annotations, line); return line;
    }

    private static void Place(IAnnotation annotation, double x, double y)
    {
        annotation.X = x; annotation.Y = y; annotation.Area = ChartAreaRef.Main;
        annotation.CoordinateXType = annotation.CoordinateYType = CoordinateTypeEnum.Absolute;
        annotation.Visible = true; annotation.PlotForeground = true;
        annotation.MaxBarsViewed = int.MaxValue;
    }
    private void RemoveItems(IEnumerable<IAnnotation> items)
    {
        foreach (var item in items)
        {
            annotations.RemoveAnnotation(item);
            IndVars.Ann_List.RemoveAnnotation(item);
        }
    }
}
