using System.Reflection;
using SwingDivergence;
using SwingDivergence.Analysis;
using VolSysAPI;
using VolumetricaAPI.Chart;
using static VolSysAPI.Structure;
using static VolSysAPI.ExternalStructure;

// Simulates host calls through the real SDK interfaces; does not emulate native pixel rendering.
sealed class Host
{
    public List<BarClass> Bars = new();
    public List<IAnnotation> Annotations = new();
    public int Index, ChildGroups, AggregateRequests;
    public bool Replay;
    public readonly List<string> PlayedSounds = new();
    public readonly List<string> Messages = new();
    public readonly Dictionary<int, BaseIndicatorPrms> OtherIndicators = new();
    public IAnnGroup Group;
    public Host()
    {
        Group = PropertyProxy.Make<IAnnGroup>((method, args) => method.Name switch
        {
            "get_AnnItems" => Annotations,
            "get_AnnGroupList" => Array.Empty<IAnnGroup>(),
            "AddGroup" => AddChild(),
            "RemoveAnnotation" => Annotations.Remove((IAnnotation)args![0]!),
            "Clear" => Clear(),
            _ => PropertyProxy.Unhandled
        });
    }
    private object? AddChild() { ChildGroups++; return null; }
    private object? Clear() { Annotations.Clear(); return null; }
    public void SetBar(int index, double price)
    {
        Bars[index].High = price + 2;
        Bars[index].Low = price;
        Bars[index].Open = Bars[index].Close = price + 1;
        Bars[index].Index = index;
        Bars[index].exchDt = new DateTime(2026, 10, 7, 12, 0, 0).AddSeconds(index * 30);
    }
    public void Load(double[] prices)
    {
        Bars.Clear();
        for (int i = 0; i < prices.Length; i++) { Bars.Add(new BarClass()); SetBar(i, prices[i]); }
        Index = Bars.Count - 1;
    }
    public List<string> PivotLabels() => Annotations
        .Where(a => a.AnnotationType == AnnotationType.Text && a.CoordinateXType == CoordinateTypeEnum.Absolute
            && a.Text is "HH" or "HL" or "LH" or "LL" or "H" or "L")
        .Select(a => a.Text).ToList();
    public SwingDivergenceIdentifier MakeIndicator()
    {
        var indicator = new TestIndicator { LeftBars = 1, RightBars = 1, Detection = StructureDetection.FilteredPivots,
            AppearanceVersion = 9, StructureVersion = 17, AlertVersion = 10, MinimumSwingAtr = 0, PriceToleranceTicks = 0, ShowDeepTrades = false,
            ShowDivergences = false, DeepTradeSource = DeepTradeSource.AgregatsReconstitues, DeepTradeMinimum = 15,
            ShowCvdSetups = false, EnableCvdSound = false, ShowCvdNotification = false,
            ShowCvdDivergences = false, ShowTechnicalDivergences = false };
        var variables = PropertyProxy.Make<IIndicatorVariables>((method, args) =>
            method.Name is "get_FrontAnnList" or "get_Ann_List" ? Group : PropertyProxy.Unhandled);
        var api = PropertyProxy.Make<IMethodCloudAPI>((method, args) => method.Name switch
        {
            "get_BarVars" => Bars,
            "get_BarIndex" => Index,
            "LastIndex" => Bars.Count - 1,
            "SetCalculateAggrTrades" => RequestAggregates(),
            "get_IsReplayData" => Replay,
            "GetParamsById" => OtherIndicators.TryGetValue((int)args![0]!, out var parameters) ? parameters : null,
            "PlayAlert" => Play(args!),
            "ShowMessage" => Message(args!),
            "CreateAnnotation" => NewAnnotation((AnnotationType)args![0]!),
            "CreateAnnGroup" => throw new Exception("Sous-groupe inattendu."),
            "AddAnnotation" => AddAnnotation(args!),
            "DoubleAddSubTicks" => (double)args![0]! + Convert.ToDouble(args[1]) * 0.25,
            _ => PropertyProxy.Unhandled
        });
        indicator.Attach(variables, api);
        indicator.OnSet(false, false);
        return indicator;
    }
    private object? RequestAggregates() { AggregateRequests++; return null; }
    private object? Play(object?[] args) { PlayedSounds.Add((string)args[0]!); return null; }
    private object? Message(object?[] args) { Messages.Add((string)args[0]!); return null; }
    private IAnnotation NewAnnotation(AnnotationType type)
    {
        var annotation = PropertyProxy.Make<IAnnotation>();
        ((PropertyProxy)(object)annotation).Values["AnnotationType"] = type;
        return annotation;
    }
    private object? AddAnnotation(object?[] args)
    {
        if (!ReferenceEquals(args[0], Group)) throw new Exception("Groupe inconnu.");
        Annotations.Add((IAnnotation)args[1]!);
        return null;
    }
}

sealed class TestIndicator : SwingDivergenceIdentifier
{
    public void Attach(IIndicatorVariables variables, IMethodCloudAPI api)
    {
        IndVars = variables;
        VAn = api;
    }
}

public class PropertyProxy : DispatchProxy
{
    public static readonly object Unhandled = new();
    public Dictionary<string, object?> Values = new();
    public Func<MethodInfo, object?[]?, object?>? Handler;
    public static T Make<T>(Func<MethodInfo, object?[]?, object?>? handler = null) where T : class
    {
        var result = Create<T, PropertyProxy>();
        ((PropertyProxy)(object)result).Handler = handler;
        return result;
    }
    protected override object? Invoke(MethodInfo? method, object?[]? args)
    {
        object? handled = Handler is null ? Unhandled : Handler(method!, args);
        if (!ReferenceEquals(handled, Unhandled)) return handled;
        string name = method!.Name;
        if (name.StartsWith("set_")) { Values[name[4..]] = args![0]; return null; }
        if (name.StartsWith("get_") && Values.TryGetValue(name[4..], out var value)) return value;
        return method.ReturnType == typeof(void) || !method.ReturnType.IsValueType
            ? null : Activator.CreateInstance(method.ReturnType);
    }
}
