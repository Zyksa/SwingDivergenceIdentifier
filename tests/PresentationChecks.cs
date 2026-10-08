using SwingDivergence.Analysis;

internal static class PresentationChecks
{
    public static int Run()
    {
        int passed = 0;
        void Check(string name, Action test) { test(); passed++; Console.WriteLine($"OK — {name}"); }
        var frames = Enumerable.Range(0, 12).Select(i => new MarketFrame(i, 0, 100, 110, 90, 100, 100,
            0, 20, -20, 0, 1, null, null, null)).ToArray();
        DivergenceSignal Signal(int a, int b, bool high = true) => new($"CVD:{a}:{b}", "CVD", "Hidden",
            new(a, a + 1, 110, high, "H"), new(b, b + 1, 108, high, "LH"), 10, 20,
            !high, true, false, b + 1, "Pivots confirmés");
        Check("Catégories CVD : aucune, une ou deux extrémités confirmées", () =>
        {
            var index = new CvdStructureIndex(); var signal = Signal(2, 8);
            Require(!index.HasStructure(signal), "Un pivot local est traité comme une structure confirmée.");
            index.RecordMajor(new(new(5, 7, 90, false, "HL")));
            Require(!index.HasStructure(signal), "Un creux intermédiaire classe les deux hauts comme structurés.");
            index.RecordMajor(new(new(2, 4, 110, true, "HH")));
            Require(index.HasStructure(signal), "Une seule extrémité confirmée ne suffit pas.");
            index.RecordMajor(new(new(8, 10, 108, true, "LH")));
            Require(index.HasStructure(signal), "Deux extrémités confirmées mal classées.");
        });
        Check("Catégories CVD : la mèche opposée et un point retiré ne comptent pas", () =>
        {
            var index = new CvdStructureIndex(); var signal = Signal(2, 8);
            index.RecordMajor(new(new(2, 4, 90, false, "L")));
            Require(!index.HasStructure(signal), "Un creux de la même bougie devient un sommet.");
            var point = new SwingPoint(8, 10, 108, true, "LH"); index.RecordMajor(new(point));
            Require(index.HasStructure(signal), "Point ajouté absent.");
            index.RecordMajor(new(new(9, 11, 109, true, "LH"), point));
            Require(!index.HasStructure(signal), "Un point réécrit reste confirmé.");
        });
        Check("Catégories CVD : alertes partagées entre détecteurs et extrême mobile", () =>
        {
            var index = new CvdStructureIndex();
            index.RecordMajor(new(new(2, 4, 110, true, "HH")));
            index.RecordMajor(new(new(5, 7, 90, false, "HL")));
            string first = index.AlertIdentity(Signal(2, 8), frames);
            Require(first == index.AlertIdentity(Signal(2, 9), frames), "Un haut mobile renotifie.");
            index.RecordMajor(new(new(9, 11, 108, true, "LH")));
            Require(first == index.AlertIdentity(Signal(2, 9), frames), "La confirmation renotifie.");
        });
        Check("Bulles : snapshots progressifs sans clôturer ni réinitialiser l'ordre", () =>
        {
            var aggregator = new DeepTradeAggregator(50, 99, 100, 0, false, true);
            aggregator.Feed(0, 10, 100, 30, true, 7);
            Require(aggregator.Snapshot()!.Value.Volume == 30, "Premier snapshot absent.");
            aggregator.Feed(0, 11, 101, 40, true, 7);
            var growing = aggregator.Snapshot()!.Value;
            Require(growing.Volume == 70 && growing.Prints == 2 && growing.Time == 10 && growing.OrderId == 7,
                "Le snapshot divise le même ordre.");
            Require(aggregator.Flush()!.Value == growing && aggregator.Snapshot() == null, "L'ordre est compté deux fois.");
        });
        Check("Bulles : statistiques remplacées pour un agrégat en formation", () =>
        {
            var scale = new TradeSizeScale(20);
            for (int i = 0; i < 20; i++) scale.Record(i.ToString(), 100);
            Require(scale.Count == 20 && scale.Upper(2.5) == 100, "Distribution constante incorrecte.");
            scale.Record("19", 300);
            Require(scale.Count == 20 && Math.Abs(scale.Upper(2.5) - (110 + 2.5 * Math.Sqrt(1900))) < 1e-8,
                "Un snapshot devient un nouvel ordre ou fausse l'écart-type.");
            Require(scale.Upper(1) < scale.Upper(3), "Paramètre écart-type sans effet.");
            scale.Record("20", 100);
            Require(scale.Count == 20, "Fenêtre statistique non bornée.");
        });
        Check("Bulles : bornes, couleurs d'opacité et police de la référence", () =>
        {
            var small = DeepTradeAppearance.Calibrated(50, 300, .25, .2, 25, 20, 30, 12, 300, 75);
            var large = DeepTradeAppearance.Calibrated(600, 300, .25, .2, 25, 20, 30, 12, 300, 75);
            Require(small.Size < large.Size && large.Size == 25 && large.PriceRadius == 3.125,
                "Bornes de taille incorrectes.");
            Require(small.FillOpacity >= 51 && large.FillOpacity == 76 && small.FontSize == 12 && large.FontSize == 12,
                "Opacité 20–30 % ou police 12 non respectée.");
            Require(small.LabelMaximumBars < large.LabelMaximumBars && large.LabelMaximumBars == 300,
                "Zoom des petits volumes identique à celui des gros volumes.");
        });
        Check("Bulles : correction géométrique indépendante du volume et de l'opacité", () =>
        {
            foreach (double volume in new[] { 50d, 151, 222, 600 })
            {
                var original = DeepTradeAppearance.Calibrated(volume, 300, .25, .2, 25, 20, 30, 12, 300, 75);
                var corrected = DeepTradeAppearance.Calibrated(volume, 300, .25, .2, 25, 20, 30, 12, 300, 75, 2);
                Require(corrected.PriceRadius == original.PriceRadius * 2 && corrected.Size == original.Size
                    && corrected.FillOpacity == original.FillOpacity && corrected.FontSize == original.FontSize,
                    "La correction du rayon modifie le volume, l'opacité ou la police.");
                Require(corrected.LabelMaximumBars >= original.LabelMaximumBars, "Le texte ignore le nouveau rayon.");
            }
        });
        return passed;
    }
    private static void Require(bool condition, string message) { if (!condition) throw new Exception(message); }
}
