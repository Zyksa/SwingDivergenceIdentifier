using SwingDivergence.Structure;

int passed = 0;
Run("HH et LH comparés au sommet précédent", () =>
{
    var pivots = Detect(1, 1, new double[] { 1, 4, 1, 6, 1, 5, 1 }, Enumerable.Repeat(0d, 7).ToArray());
    Equal(new[] { PivotKind.H, PivotKind.HH, PivotKind.LH }, pivots.Select(p => p.Kind).ToArray());
    Equal(new[] { 1, 3, 5 }, pivots.Select(p => p.BarIndex).ToArray());
    Equal(new[] { 2, 4, 6 }, pivots.Select(p => p.ConfirmedAt).ToArray());
});
Run("LL et HL comparés au creux précédent", () =>
{
    var pivots = Detect(1, 1, Enumerable.Repeat(10d, 7).ToArray(), new double[] { 5, 1, 5, 0, 5, 2, 5 });
    Equal(new[] { PivotKind.L, PivotKind.LL, PivotKind.HL }, pivots.Select(p => p.Kind).ToArray());
});
Run("Confirmation 3/3 au bon index", () =>
{
    var d = new PivotDetector(3, 3);
    double[] highs = { 1, 2, 3, 9, 3, 2, 1 };
    for (int i = 0; i < 6; i++)
        Require(d.AddClosedBar(i, highs[i], 0) == default, "Confirmation prématurée.");
    Pivot p = d.AddClosedBar(6, highs[6], 0).High!.Value;
    Require(p.BarIndex == 3 && p.ConfirmedAt == 6 && p.Price == 9, "Index ou prix incorrect.");
});
Run("Plateau : retenir le dernier sommet égal", () =>
{
    var result = Detect(1, 1, new double[] { 1, 4, 4, 1 }, new double[4]);
    Require(result.Count == 1 && result[0].BarIndex == 2, "Plateau dupliqué.");
});
Run("Plateau : retenir le dernier creux égal", () =>
{
    var result = Detect(1, 1, Enumerable.Repeat(10d, 4).ToArray(), new double[] { 5, 1, 1, 5 });
    Require(result.Count == 1 && result[0].BarIndex == 2, "Plateau dupliqué.");
});
Run("Pivots successifs égaux : EQH et EQL", () =>
{
    var highs = Detect(1, 1, new double[] { 1, 4, 1, 4, 1 }, new double[5]);
    var lows = Detect(1, 1, Enumerable.Repeat(10d, 5).ToArray(), new double[] { 5, 1, 5, 1, 5 });
    Equal(new[] { PivotKind.H, PivotKind.EQH }, highs.Select(p => p.Kind).ToArray());
    Equal(new[] { PivotKind.L, PivotKind.EQL }, lows.Select(p => p.Kind).ToArray());
});
Run("Bougie extérieure : sommet et creux indépendants", () =>
{
    var result = Detect(1, 1, new double[] { 3, 10, 3 }, new double[] { 2, 0, 2 });
    Require(result.Count == 2 && result.All(p => p.BarIndex == 1), "Un extrême a été perdu.");
    Require(result.Any(p => p.IsHigh) && result.Any(p => !p.IsHigh), "Sens incorrect.");
});
Run("Historique court et dernier pivot non confirmé", () =>
{
    Require(Detect(3, 3, new double[] { 1, 2, 9, 2 }, new double[4]).Count == 0, "Fenêtre incomplète acceptée.");
    Require(Detect(1, 2, new double[] { 1, 2, 9, 2 }, new double[4]).Count == 0, "Pivot de fin prématuré.");
});
Run("Prix plats : aucune structure", () =>
{
    Require(Detect(3, 3, Enumerable.Repeat(5d, 100).ToArray(), Enumerable.Repeat(5d, 100).ToArray()).Count == 0,
        "Structure artificielle sur prix plat.");
});
Run("Prix négatifs et passage à zéro", () =>
{
    var result = Detect(1, 1, new double[] { -5, -2, -5, 0, -5 }, Enumerable.Repeat(-10d, 5).ToArray());
    Equal(new[] { PivotKind.H, PivotKind.HH }, result.Select(p => p.Kind).ToArray());
});
Run("Clôture répétée sans duplication", () =>
{
    var d = new PivotDetector(1, 1);
    d.AddClosedBar(0, 1, 0);
    d.AddClosedBar(1, 4, 0);
    Require(d.AddClosedBar(2, 1, 0).High.HasValue, "Pivot absent.");
    Require(d.AddClosedBar(2, 1, 0) == default, "Clôture dupliquée.");
    d.AddClosedBar(3, 6, 0);
    Require(d.AddClosedBar(4, 1, 0).High!.Value.Kind == PivotKind.HH, "État corrompu après duplication.");
});
Run("Recalcul : résultat identique avec un nouveau détecteur", () =>
{
    double[] h = { 2, 6, 3, 8, 4, 5, 1, 7, 2 };
    double[] l = { 0, 3, 1, 4, 2, 3, -1, 3, 0 };
    Equal(Detect(1, 1, h, l).ToArray(), Detect(1, 1, h, l).ToArray());
});
Run("Paramètres et données invalides refusés", () =>
{
    Throws<ArgumentOutOfRangeException>(() => new PivotDetector(0, 1));
    Throws<ArgumentOutOfRangeException>(() => new PivotDetector(1, 1001));
    var d = new PivotDetector(1, 1);
    Throws<ArgumentException>(() => d.AddClosedBar(0, double.NaN, 0));
    Throws<ArgumentException>(() => d.AddClosedBar(0, 1, 2));
    d.AddClosedBar(0, 2, 1);
    Throws<InvalidOperationException>(() => d.AddClosedBar(2, 2, 1));
});
Run("Fenêtres asymétriques et boucle du tampon : oracle historique", () =>
{
    var random = new Random(1729);
    foreach (var (left, right) in new[] { (1, 1), (2, 3), (5, 2), (20, 30), (1000, 1000) })
    {
        int length = left == 1000 ? 3500 : 800;
        double[] h = new double[length], l = new double[length];
        for (int i = 0; i < length; i++)
        {
            l[i] = random.Next(-20, 20);
            h[i] = l[i] + random.Next(0, 20);
        }
        Equal(Oracle(left, right, h, l).ToArray(), Detect(left, right, h, l).ToArray());
    }
});
Run("Chaque préfixe produit uniquement les pivots déjà confirmés", () =>
{
    var random = new Random(42);
    double[] h = new double[120], l = new double[120];
    for (int i = 0; i < h.Length; i++) { l[i] = random.Next(-10, 10); h[i] = l[i] + random.Next(0, 12); }
    var full = Detect(2, 3, h, l);
    for (int length = 1; length <= h.Length; length++)
        Equal(full.Where(p => p.ConfirmedAt < length).ToArray(), Detect(2, 3, h[..length], l[..length]).ToArray());
});
passed += BundleChecks.Run();
passed += SetupChecks.Run();
passed += PresentationChecks.Run();
Console.WriteLine($"{passed} vérifications réussies.");

void Run(string name, Action test)
{
    test();
    passed++;
    Console.WriteLine($"OK — {name}");
}

static List<Pivot> Detect(int left, int right, double[] high, double[] low)
{
    var detector = new PivotDetector(left, right);
    var result = new List<Pivot>();
    for (int i = 0; i < high.Length; i++)
    {
        var batch = detector.AddClosedBar(i, high[i], low[i]);
        if (batch.High is Pivot h) result.Add(h);
        if (batch.Low is Pivot l) result.Add(l);
    }
    return result;
}

// Independent batch oracle: max/min and their last positions in an entire window.
static List<Pivot> Oracle(int left, int right, double[] high, double[] low)
{
    var result = new List<Pivot>();
    double? previousHigh = null, previousLow = null;
    for (int candidate = left; candidate + right < high.Length; candidate++)
    {
        int start = candidate - left, length = left + right + 1;
        double[] h = high.Skip(start).Take(length).ToArray();
        double[] l = low.Skip(start).Take(length).ToArray();
        if (Array.LastIndexOf(h, h.Max()) == left)
        {
            var kind = !previousHigh.HasValue ? PivotKind.H
                : high[candidate] > previousHigh ? PivotKind.HH
                : high[candidate] < previousHigh ? PivotKind.LH : PivotKind.EQH;
            result.Add(new Pivot(candidate, candidate + right, high[candidate], true, kind));
            previousHigh = high[candidate];
        }
        if (Array.LastIndexOf(l, l.Min()) == left)
        {
            var kind = !previousLow.HasValue ? PivotKind.L
                : low[candidate] > previousLow ? PivotKind.HL
                : low[candidate] < previousLow ? PivotKind.LL : PivotKind.EQL;
            result.Add(new Pivot(candidate, candidate + right, low[candidate], false, kind));
            previousLow = low[candidate];
        }
    }
    return result;
}

static void Equal<T>(T[] expected, T[] actual)
{
    Require(expected.SequenceEqual(actual), $"Attendu : {string.Join(", ", expected)} ; obtenu : {string.Join(", ", actual)}");
}

static void Require(bool condition, string message)
{
    if (!condition) throw new Exception(message);
}

static void Throws<T>(Action action) where T : Exception
{
    try { action(); }
    catch (T) { return; }
    throw new Exception($"Exception {typeof(T).Name} attendue.");
}
