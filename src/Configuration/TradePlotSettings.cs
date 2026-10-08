using System.ComponentModel;
using VolumetricaControls;

namespace SwingDivergence;

public partial class SwingDivergenceIdentifier
{
    [Category("03 Bulles"), DisplayName("Rendu des bulles")]
    public TradePlotSettings TradePlot { get; set; } = new();

    private void ClampTradePlot()
    {
        TradePlot ??= new();
        TradePlot.StandardDeviation = double.IsFinite(TradePlot.StandardDeviation) ? Math.Clamp(TradePlot.StandardDeviation, .1, 10) : 2.5;
        TradePlot.MinimumSize = double.IsFinite(TradePlot.MinimumSize) ? Math.Clamp(TradePlot.MinimumSize, .01, 100) : .2;
        TradePlot.MaximumSize = double.IsFinite(TradePlot.MaximumSize) ? Math.Clamp(TradePlot.MaximumSize, TradePlot.MinimumSize, 100) : 25;
        TradePlot.MinimumOpacity = Math.Clamp(TradePlot.MinimumOpacity, 0, 100);
        TradePlot.MaximumOpacity = Math.Clamp(TradePlot.MaximumOpacity, TradePlot.MinimumOpacity, 100);
        TradePlot.TextSize = Math.Clamp(TradePlot.TextSize, 6, 24);
        TradePlot.RadiusScale = double.IsFinite(TradePlot.RadiusScale) ? Math.Clamp(TradePlot.RadiusScale, .1, 5) : 1;
        if (AppearanceVersion < 23)
        {
            DeepBuyColor = VolumetricaAPI.Chart.ColorRef.FromRgb(0, 102, 22);
            DeepSellColor = VolumetricaAPI.Chart.ColorRef.FromRgb(77, 15, 148);
            AppearanceVersion = 23;
        }
    }
}

public class TradePlotSettings
{
    [DisplayName("Écart-type de taille")]
    public double StandardDeviation { get; set; } = 2.5;
    [DisplayName("Taille minimale")]
    public double MinimumSize { get; set; } = .2;
    [DisplayName("Taille maximale")]
    public double MaximumSize { get; set; } = 25;
    [DisplayName("Correction du rayon (×)")]
    public double RadiusScale { get; set; } = 1;
    [DisplayName("Opacité minimale (%)")]
    public int MinimumOpacity { get; set; } = 20;
    [DisplayName("Opacité maximale (%)")]
    public int MaximumOpacity { get; set; } = 30;
    [DisplayName("Afficher les volumes")]
    public bool ShowText { get; set; } = true;
    [DisplayName("Taille du texte")]
    public int TextSize { get; set; } = 12;
}
