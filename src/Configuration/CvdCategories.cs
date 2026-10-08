using System.ComponentModel;
using VolumetricaControls;

namespace SwingDivergence;

public partial class SwingDivergenceIdentifier
{
    [Category("04 Div Cvd"), DisplayName("Avec au moins un point de structure")]
    public CvdCategorySettings CvdWithStructure { get; set; } = new();
    [Category("04 Div Cvd"), DisplayName("Sans point de structure — rose")]
    public CvdCategorySettings CvdWithoutStructure { get; set; } = new()
        { OpacityPercent = 50, PlaySound = false, ShowPopup = false };
    [VolCustom(IsVisible = false)]
    public int CvdCategoryVersion { get; set; }

    private CvdCategorySettings CvdCategory(bool withStructure) => withStructure ? CvdWithStructure : CvdWithoutStructure;

    private void ClampCvdCategories()
    {
        CvdWithStructure ??= new();
        CvdWithoutStructure ??= new() { OpacityPercent = 50, PlaySound = false, ShowPopup = false };
        if (CvdCategoryVersion < 23)
        {
            if (!SetupExpert.ShowNotification) CvdWithStructure.ShowPopup = false;
            CvdCategoryVersion = 23;
        }
        SetupExpert.ShowNotification = CvdWithStructure.ShowPopup;
        CvdWithStructure.OpacityPercent = Math.Clamp(CvdWithStructure.OpacityPercent, 0, 100);
        CvdWithoutStructure.OpacityPercent = Math.Clamp(CvdWithoutStructure.OpacityPercent, 0, 100);
    }
}

public class CvdCategorySettings
{
    [DisplayName("Activer cette catégorie")]
    public bool Enabled { get; set; } = true;
    [DisplayName("Afficher les tracés")]
    public bool ShowLines { get; set; } = true;
    [DisplayName("Opacité des tracés (%)")]
    [VolCustom(MinValue = 0, MaxValue = 100, IncrementValue = 1)]
    public int OpacityPercent { get; set; } = 100;
    [DisplayName("Alerte sonore")]
    public bool PlaySound { get; set; } = true;
    [DisplayName("Popup de notification")]
    public bool ShowPopup { get; set; } = true;
}
