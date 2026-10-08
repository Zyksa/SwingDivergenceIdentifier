using System.ComponentModel;
using SwingDivergence.Analysis;
using VolumetricaControls;

namespace SwingDivergence;

public partial class SwingDivergenceIdentifier
{
    [Category("00 À propos"), DisplayName("Swing & Divergence Identifier")]
    [ReadOnly(true)]
    [VolCustom(CategoryIndex = 0, PropertyIndex = 0, SerializeIgnore = true)]
    public string IndicatorCredits
    {
        get => "by Zyksa · Beta v0.6";
        set { }
    }

    [Category("01 Général"), DisplayName("Masquer les textes du coin supérieur gauche")]
    public bool HideChartText { get; set; } = true;
    [Category("04 Setup Div Cvd"), DisplayName("Activer Setup Div Cvd")]
    public bool ShowCvdSetups { get; set; } = true;
    [Category("04 Setup Div Cvd"), DisplayName("Détection précoce sur les ticks")]
    public bool EarlyCvdDetection { get; set; } = true;
    [Category("04 Setup Div Cvd"), DisplayName("Dernier contrôle Div Cvd")]
    [ReadOnly(true)]
    [VolCustom(SerializeIgnore = true)]
    public string CvdSetupStatus
    {
        get => cvdSetups?.LastCheck ?? "En attente du calcul.";
        set { }
    }
    [VolCustom(IsVisible = false)]
    public bool EnableCvdSound { get => CvdWithStructure.PlaySound; set => CvdWithStructure.PlaySound = value; }
    [Category("04 Setup Div Cvd"), DisplayName("Son")]
    [VolCustom(IsAlert = true)]
    public DynamicList CvdSound { get; set; } = new() { SelValue = "Alert 1" };
    // Keep legacy serialized timing values readable; the early option controls tick detection.
    [VolCustom(IsVisible = false)]
    public CvdAlertTiming AlertTiming { get; set; } = CvdAlertTiming.SetupConfirme;
    [VolCustom(IsVisible = false, SerializeIgnore = true)]
    public bool ShowCvdNotification
    {
        get => CvdWithStructure.ShowPopup;
        set { CvdWithStructure.ShowPopup = value; SetupExpert.ShowNotification = value; }
    }
    [VolCustom(IsVisible = false)]
    public int AlertVersion { get; set; }
    [Category("04 Setup Div Cvd"), DisplayName("Confirmation de Setup Div Cvd")]
    public SetupConfirmation SetupConfirmation { get; set; } = SetupConfirmation.DivergenceConfirmee;
    [VolCustom(IsVisible = false, SerializeIgnore = true)]
    public double SetupRewardRisk { get; set; } = 2;
    [Category("05 Avancé"), DisplayName("Setup Div Cvd et alertes")]
    public SetupExpert SetupExpert { get; set; } = new();
    // Read-only: diagnostics remain inspectable without printing warnings over the chart.
    public string LastDiagnostic => lastDiagnostic;
}

public class SetupExpert
{
    [VolCustom(IsVisible = false)]
    public double SwingAtr { get; set; } = 2;
    [VolCustom(IsVisible = false)]
    public int AtrPeriod { get; set; } = 14;
    [DisplayName("Setups / alertes achat")]
    public bool Bullish { get; set; } = true;
    [DisplayName("Setups / alertes vente")]
    public bool Bearish { get; set; } = true;
    [DisplayName("Absorptions")]
    public bool Absorption { get; set; } = true;
    [DisplayName("Épuisements")]
    public bool Exhaustion { get; set; } = true;
    [VolCustom(IsVisible = false, SerializeIgnore = true)]
    public double StopAtr { get; set; } = .25;
    [VolCustom(IsVisible = false, SerializeIgnore = true)]
    public int MinimumStopTicks { get; set; } = 2;
    [DisplayName("Attente maximale du BOS (bougies)")]
    public int WaitingBars { get; set; } = 20;
    [VolCustom(IsVisible = false, SerializeIgnore = true)]
    public int ProjectionBars { get; set; } = 15;
    [VolCustom(IsVisible = false, SerializeIgnore = true)]
    public bool ShowZones { get; set; } = false;
    [VolCustom(IsVisible = false, SerializeIgnore = true)]
    public int MaxSetups { get; set; } = 20;
    [DisplayName("Délai minimum entre les sons (secondes)")]
    public int CooldownSeconds { get; set; } = 0;
    [VolCustom(IsVisible = false)]
    public bool AlertOnMinor { get; set; } = false;
    [DisplayName("Alerter en replay")]
    public bool AlertInReplay { get; set; } = false;
    [VolCustom(IsVisible = false)]
    public bool ShowNotification { get; set; } = true;
}
