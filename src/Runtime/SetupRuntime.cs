using SwingDivergence.Analysis;

namespace SwingDivergence;

public partial class SwingDivergenceIdentifier
{
    private StructuralCvdSetups cvdSetups;
    private CvdAlertGate alertGate;

    private void HandleDivergence(DivergenceSignal signal, bool minor, bool live)
    {
        if (signal.Source == "CVD") RememberCvd(signal);
        if (signal.Source == "CVD" ? ShowCvdDivergences || ShowCvdSetups : ShowTechnicalDivergences)
            DrawDivergence(signal, minor);
    }

    private void HandleSetup(CvdSetup setup, bool live)
    {
        SoundCvd(setup.Signal, setup.AvailableAt, live, setup.Confirmation);
        RememberCvd(setup.Signal);
        DrawDivergence(setup.Signal, false);
    }

    private void SoundCvd(DivergenceSignal signal, int availableAt, bool live, string reason,
        bool forming = false)
    {
        var category = CvdCategory(cvdStructure.HasStructure(signal));
        if (!ShowCvdSetups || !category.Enabled || (!category.PlaySound && !category.ShowPopup) || signal.Second.Index - signal.First.Index > Divergences.MaximumPivotDistance
            || (signal.Bullish ? !SetupExpert.Bullish : !SetupExpert.Bearish)
            || (signal.Hidden ? !SetupExpert.Absorption : !SetupExpert.Exhaustion)) return;
        DateTime now = VAn.LastExDt;
        if (now == default && availableAt >= 0 && availableAt < VAn.BarVars.Count)
            now = VAn.BarVars[availableAt].exchDt;
        if (now == default) now = DateTime.UtcNow;
        if (!alertGate.Accept(cvdStructure.AlertIdentity(signal, frames), live, VAn.IsReplayData, SetupExpert.AlertInReplay,
            availableAt, forming ? frames.Count : frames.Count - 1, now, SetupExpert.CooldownSeconds)) return;
        if (!notifiedCvdPairs.Add(signal.Id)) return;
        string sound = CvdSound.SelValue;
        if (string.IsNullOrWhiteSpace(sound) && CvdSound.ListElements?.Count > 0) sound = CvdSound.ListElements[0];
        if (category.PlaySound && !string.IsNullOrWhiteSpace(sound)) VAn.PlayAlert(sound);
        if (category.ShowPopup)
            VAn.ShowMessage($"{VAn.SymbolName} — {reason} {(signal.Bullish ? "haussier" : "baissier")} ({(signal.Hidden ? "ABS" : "EXH")})", IndicatorName);
    }
}
