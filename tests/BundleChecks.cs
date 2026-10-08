using System.Text.Json;
using SwingDivergence.Analysis;

static class BundleChecks
{
    public static int Run()
    {
        int passed = 0;
        void Check(string name, Action test) { test(); passed++; Console.WriteLine("OK — " + name); }
        using var reference = JsonDocument.Parse(File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "fixtures", "reference.json")));
        var root = reference.RootElement;
        var builder = new FrameBuilder(3, 2, 3, 5, 2, 5);
        var frames = new List<MarketFrame>();
        foreach (var bar in root.GetProperty("bars").EnumerateArray())
            frames.Add(builder.Add(bar.GetProperty("index").GetInt32(), false, Number(bar, "open"), Number(bar, "high"),
                Number(bar, "low"), Number(bar, "close"), 100, Number(bar, "delta"), Number(bar, "maxDelta"), Number(bar, "minDelta")));
        Check("ATR / RSI / MACD : parité avec 600 bougies phidias", () =>
        {
            CompareSeries(root.GetProperty("atr"), frames.Select(f => f.Atr));
            CompareSeries(root.GetProperty("rsi"), frames.Select(f => f.Rsi));
            CompareSeries(root.GetProperty("macd"), frames.Select(f => f.Macd));
        });
        Check("Swings ATR : 117 confirmations identiques à SwingTracker", () =>
        {
            var detector = new AtrSwings(1.4, 3, 0.25);
            var actual = frames.Select(f => detector.Add(f)).Where(c => c.HasValue).Select(c => c!.Value.Point).ToArray();
            ComparePoints(root.GetProperty("atrSwings"), actual);
            Require(actual.Zip(actual.Skip(1)).All(p => p.First.High != p.Second.High), "Les swings n'alternent pas.");
            Require(actual.All(p => p.ConfirmedAt > p.Index), "Swing publié avant son retracement.");
        });
        Check("Pivots filtrés : parité avec _structure_for_scale", () =>
        {
            var detector = new FilteredSwings(2, 2, 0.25, 0.5);
            var final = new List<SwingPoint>();
            foreach (var frame in frames)
                foreach (var change in detector.Add(frame, frames))
                {
                    if (change.Removed.HasValue) final.RemoveAll(p => p.Index == change.Removed.Value.Index && p.High == change.Removed.Value.High);
                    final.Add(change.Point);
                }
            ComparePoints(root.GetProperty("filtered"), final);
        });
        Check("Divergences : les quatre configurations et tolérances", () =>
        {
            Require(Divergences.Classify(false, -2, 10, .25, 1, false) == "Regular", "Régulière haussière incorrecte.");
            Require(Divergences.Classify(true, 2, -10, .25, 1, false) == "Regular", "Régulière baissière incorrecte.");
            Require(Divergences.Classify(false, 2, -10, .25, 1, false) == "Hidden", "Cachée haussière incorrecte.");
            Require(Divergences.Classify(true, -2, 10, .25, 1, false) == "Hidden", "Cachée baissière incorrecte.");
            Require(Divergences.Classify(false, -.25, 1, .25, 1, true) == null, "Égalité de tolérance traitée comme divergence.");
            Require(Divergences.Classify(false, 0, 10, .25, 1, true) == "Regular", "Divergence faible incorrecte.");
            Require(Divergences.Classify(false, 0, 10, .25, 1, false) == null, "Faible signal accepté sans option.");
        });
        Check("CVD : alignement aux creux, confirmation et absence de doublons", () =>
        {
            var data = new[] { Frame(0, 11, 10, 5), Frame(1, 14, 12, 6), Frame(2, 10, 8, 15) };
            var d = new Divergences(Options());
            Require(d.Offer(new SwingChange(new(0, 1, 10, false, "L")), data).Count == 0, "Premier creux comparé à une valeur fictive.");
            var signals = d.Offer(new SwingChange(new(2, 2, 8, false, "LL")), data);
            Require(signals.Count == 1 && signals[0].Bullish && signals[0].Source == "CVD", "Divergence CVD absente.");
            Require(signals[0].Value1 == 5 && signals[0].Value2 == 15 && signals[0].ConfirmedAt == 2, "Échantillons CVD non alignés.");
            Require(d.Advance(data).Count == 0, "Signal republié.");
        });
        Check("CVD : pas de divergence sur une donnée manquante ou entre sessions", () =>
        {
            foreach (bool missing in new[] { false, true })
            {
                var data = new[] { Frame(0, 11, 10, 5), Frame(1, 14, 12, 6), Frame(2, 10, 8, 15) };
                data[2] = missing ? data[2] with { CvdLow = null } : data[2] with { Session = 1 };
                var d = new Divergences(Options());
                d.Offer(new SwingChange(new(0, 1, 10, false, "L")), data);
                Require(d.Offer(new SwingChange(new(2, 2, 8, false, "LL")), data).Count == 0, "Signal sur gap ou session différente.");
            }
        });
        Check("Ligne de divergence : traversée d'une bougie refusée", () =>
        {
            var points = new[] { Frame(0, 11, 10, 5), Frame(1, 12, 8, 7), Frame(2, 11, 10, 10) };
            var first = new SwingPoint(0, 0, 10, false, "L");
            var second = new SwingPoint(2, 2, 10, false, "HL");
            Require(!Divergences.ClearLine(first, second, 10, 10, points, false, .25), "Ligne de prix coupée acceptée.");
            points[1] = points[1] with { Low = 10 };
            Require(Divergences.ClearLine(first, second, 10, 10, points, false, .25), "Contact sur le bord refusé.");
        });
        Check("RSI : attente du BOS puis publication au bon index", () =>
        {
            var data = new List<MarketFrame> { Frame(0, 11, 10, 5) with { Rsi = 30 },
                Frame(1, 14, 12, 6), Frame(2, 10, 8, 15) with { Rsi = 40, Close = 8 } };
            var d = new Divergences(Options() with { Cvd = false, Technical = true, ValidateTechnical = true });
            d.Offer(new SwingChange(new(0, 1, 10, false, "L")), data);
            Require(d.Offer(new SwingChange(new(2, 2, 8, false, "LL")), data).Count == 0, "Signal RSI publié sans validation.");
            data.Add(Frame(3, 16, 13, 20) with { Close = 15 });
            var signals = d.Advance(data);
            Require(signals.Count == 1 && signals[0].ConfirmedAt == 3 && signals[0].Validation.Contains("BOS"), "BOS non reconnu.");
        });
        Check("CVD cumulée : gap visible et remise à zéro de session", () =>
        {
            var b = new FrameBuilder(3, 2, 3, 5, 2, 5);
            var a = b.Add(0, false, 10, 11, 9, 10, 20, -5, 2, -9);
            Require(a.CvdOpen == 0 && a.CvdClose == -5 && a.CvdHigh == 2 && a.CvdLow == -9, "CVD de bougie incorrecte.");
            Require(b.Add(1, false, 10, 11, 9, 10, 0, null, null, null).CvdClose == null, "Gap inventé comme delta zéro.");
            Require(b.Add(2, false, 10, 11, 9, 10, 10, 2, 4, -1).CvdClose == null, "Cumul continué sur une base inconnue.");
            var c = b.Add(3, true, 10, 11, 9, 10, 10, 3, 4, -2);
            Require(c.Session == 1 && c.CvdOpen == 0 && c.CvdClose == 3, "Session non réinitialisée.");
        });
        Check("Gros trades : agrégation même côté / timestamp et VWAP", () =>
        {
            var d = new DeepTradeAggregator(15, 99, 2000, 0, false);
            Require(d.Feed(0, 100, 100, 10, true) == null, "Ordre publié prématurément.");
            Require(d.Feed(0, 100, 101, 8, true) == null, "Même ordre non fusionné.");
            var t = d.Feed(0, 101, 102, 2, false)!.Value;
            Require(t.Volume == 18 && t.Prints == 2 && t.Buy && t.Low == 100 && t.High == 101, "Sweep incorrect.");
            Near(t.Price, 1808d / 18);
            Require(d.Flush() == null, "Petit ordre affiché.");
        });
        Check("Gros trades : fenêtre réglable, barres distinctes et prints tardifs", () =>
        {
            var d = new DeepTradeAggregator(1, 99, 2000, 5, false);
            d.Feed(0, 100, 100, 10, true); d.Feed(0, 104, 100, 8, true);
            Require(d.Feed(0, 103, 100, 999, true) == null, "Print tardif accepté.");
            var t = d.Feed(1, 105, 100, 2, true)!.Value;
            Require(t.Volume == 18 && t.BarIndex == 0, "Ordres fusionnés entre bougies.");
            Require(d.FlushStale(110) == null, "Ordre fermé avant expiration de fenêtre.");
            Require(d.FlushStale(111)!.Value.Volume == 2, "Ordre stagnant non fermé.");
            Require(d.Feed(1, 100, 100, 999, true) == null && d.Flush() == null, "Print tardif accepté après clôture du sweep.");
        });
        Check("Gros trades : percentile interpolé et plancher du seuil", () =>
        {
            var d = new DeepTradeAggregator(1, 90, 2000, 0, true);
            for (int i = 1; i <= 100; i++) d.Feed(0, i, 100, i, i % 2 == 0);
            var last = d.Flush()!.Value;
            Near(d.Threshold, 90.1); Near(last.Threshold, 90.1);
            var floor = new DeepTradeAggregator(1000, 90, 50, 0, true);
            for (int i = 1; i <= 100; i++) floor.Feed(0, i, 100, i, true);
            Require(floor.Flush() == null && floor.Threshold == 1000, "Seuil descendu sous le plancher.");
        });
        Check("Gros trades : identifiant d'ordre prioritaire sur le timestamp", () =>
        {
            var d = new DeepTradeAggregator(50, 99, 2000, 0, false);
            d.Feed(0, 100, 100, 30, true, 777);
            Require(d.Feed(0, 103, 100.25, 40, true, 777) == null, "Un seul ordre scindé selon les timestamps.");
            Require(d.FlushStale(200) == null, "Ordre identifié fermé arbitrairement par la fenêtre temporelle.");
            var order = d.Feed(0, 103, 101, 60, true, 778)!.Value;
            Require(order.Volume == 70 && order.Prints == 2 && order.LargestPrint == 40, "Identifiant d'ordre non respecté.");
            Require(d.Flush()!.Value.Volume == 60, "Ordres différents fusionnés au même timestamp.");
        });
        Check("Setup Div Cvd : entrée, stop au tick et objectif R:R", () =>
        {
            var data = new[] { Frame(0, 101, 100, -10), Frame(1, 105, 102, -5),
                Frame(2, 101, 99, 5) with { Atr = 1.3 }, Frame(3, 104, 101, 10) with { Close = 102 } };
            var signal = new DivergenceSignal("setup-1", "CVD", "Regular", new(0, 1, 100, false, "L"),
                new(2, 3, 99, false, "LL"), -10, 5, true, false, false, 3, "Pivots confirmés");
            var engine = new CvdSetups(new(SetupConfirmation.DivergenceConfirmee, 2, .25, .25, .25, .25, 20, true, true, true, true));
            var setups = engine.Offer(signal, data);
            Require(setups.Count == 1 && setups[0].AvailableAt == 3 && setups[0].Entry == 102, "Entrée antérieure à la confirmation.");
            Near(setups[0].Stop, 98.5); Near(setups[0].Target, 109);
            Require(engine.Offer(signal, data).Count == 0, "Setup dupliqué.");
        });
        Check("Setup Div Cvd validé : attente du BOS et expiration", () =>
        {
            var data = new List<MarketFrame> { Frame(0, 101, 100, -10), Frame(1, 105, 102, -5),
                Frame(2, 101, 99, 5) with { Close = 99 }, Frame(3, 104, 101, 10) with { Close = 103 } };
            var signal = new DivergenceSignal("setup-bos", "CVD", "Regular", new(0, 1, 100, false, "L"),
                new(2, 3, 99, false, "LL"), -10, 5, true, false, false, 3, "Pivots confirmés");
            var engine = new CvdSetups(new(SetupConfirmation.BosOuBalayage, 2, .25, .5, .25, .25, 1, true, true, true, true));
            Require(engine.Offer(signal, data).Count == 0, "Setup publié sans BOS ni balayage.");
            data.Add(Frame(4, 108, 104, 15) with { Close = 106 });
            var setups = engine.Advance(data);
            Require(setups.Count == 1 && setups[0].AvailableAt == 4 && setups[0].Confirmation.Contains("BOS"), "BOS non reconnu.");
            var expired = new CvdSetups(new(SetupConfirmation.BosOuBalayage, 2, .25, .5, .25, .25, 0, true, true, true, true));
            expired.Offer(signal, data.Take(4).ToArray());
            Require(expired.Advance(data).Count == 0, "Validation acceptée après expiration.");
        });
        Check("Alertes CVD : historique, doublons, replay et délai", () =>
        {
            var gate = new CvdAlertGate(); var now = new DateTime(2026, 10, 7, 12, 0, 0);
            Require(!gate.Accept("hist", false, false, false, 3, 3, now, 0), "Son pendant l'historique.");
            Require(!gate.Accept("hist", true, false, false, 3, 3, now, 0), "Historique rejoué en temps réel.");
            Require(gate.Accept("live", true, false, false, 4, 4, now, 10), "Nouveau signal réel ignoré.");
            Require(!gate.Accept("live", true, false, false, 4, 4, now, 0), "Signal alerté deux fois.");
            Require(!gate.Accept("cooldown", true, false, false, 5, 5, now.AddSeconds(3), 10), "Délai non respecté.");
            Require(gate.Accept("later", true, false, false, 6, 6, now.AddSeconds(11), 10), "Signal après délai ignoré.");
            Require(!gate.Accept("replay", true, true, false, 7, 7, now.AddSeconds(20), 0), "Alerte en replay sans option.");
            Require(gate.Accept("replay-on", true, true, true, 7, 7, now.AddSeconds(20), 0), "Option replay ignorée.");
            Require(!gate.Accept("stale", true, false, false, 1, 9, now, 0), "Ancien signal découvert au rattrapage alerté.");
        });
        MarketFrame Candle(int i, double open, double close, double? high = null, double? low = null)
            => Frame(i, high ?? Math.Max(open, close) + .25, low ?? Math.Min(open, close) - .25, 0)
                with { Open = open, Close = close };
        Check("Structure : deux corps directionnels, pas de retournement sur une seule bougie", () =>
        {
            var d = new ReactiveSwings(1, 3, .25, .5);
            d.Add(Candle(0, 100, 101)); d.Add(Candle(1, 101, 105)); d.Add(Candle(2, 105, 110));
            Require(d.Add(Candle(3, 110, 105)) == null, "Une seule bougie confirme encore un sommet.");
            var point = d.Add(Candle(4, 105, 100))!.Value.Point;
            Require(point.High && point.Index == 2 && point.ConfirmedAt == 4 && point.Price == 110.25,
                "Sommet mal ancré ou confirmation non causale.");
        });
        Check("Structure : les interruptions barrées ne créent ni LH ni LL", () =>
        {
            var d = new ReactiveSwings(.5, 3, .25, .5);
            var points = new List<SwingPoint>();
            var bars = new[] { Candle(0, 101, 100), Candle(1, 100, 95), Candle(2, 95, 90),
                Candle(3, 90, 90.1, 98, 89.8), Candle(4, 90.1, 85), Candle(5, 85, 88), Candle(6, 88, 92) };
            foreach (var bar in bars) if (d.Add(bar) is SwingChange change) points.Add(change.Point);
            Require(points.Count == 1 && !points[0].High && points[0].Index == 4 && points[0].Price == 84.75,
                "Petite interruption transformée en structure ou vrai creux non détecté.");
        });
        Check("Structure : une pause dans une hausse ne crée pas de HL", () =>
        {
            var d = new ReactiveSwings(.5, 3, .25, .5);
            foreach (var bar in new[] { Candle(0, 99, 100), Candle(1, 100, 105), Candle(2, 105, 110),
                Candle(3, 110, 109, 110.25, 100), Candle(4, 109, 114), Candle(5, 114, 119) })
                Require(d.Add(bar) == null, "Mèche basse ou pause de hausse transformée en HL.");
        });
        Check("Structure : réglage à trois bougies respecté", () =>
        {
            var d = new ReactiveSwings(.5, 3, .25, .5, 3);
            d.Add(Candle(0, 100, 101)); d.Add(Candle(1, 101, 105)); d.Add(Candle(2, 105, 110)); d.Add(Candle(3, 110, 115));
            Require(d.Add(Candle(4, 115, 110)) == null && d.Add(Candle(5, 110, 105)) == null, "Troisième bougie ignorée.");
            Require(d.Add(Candle(6, 105, 100))!.Value.Point.Index == 3, "Sommet confirmé au mauvais extrême.");
        });
        Check("Structure : mèches et dojis ne constituent pas une séquence directionnelle", () =>
        {
            var d = new ReactiveSwings(.5, 3, .25, .5);
            d.Add(Candle(0, 100, 101)); d.Add(Candle(1, 101, 105)); d.Add(Candle(2, 105, 110));
            Require(d.Add(Candle(3, 110, 110, 110.25, 80)) == null, "Premier doji accepté.");
            Require(d.Add(Candle(4, 110, 110, 110.25, 75)) == null, "Deux mèches déclenchent une confirmation.");
        });
        Check("Structure : aperçu ouvert sans mutation, alternance et extrêmes réels", () =>
        {
            var d = new ReactiveSwings(.5, 3, .25, .5);
            d.Add(Candle(0, 100, 101)); d.Add(Candle(1, 101, 105)); d.Add(Candle(2, 105, 110));
            Require(d.Preview(Candle(3, 110, 999))!.Value.Price == 999.25, "Aperçu non actualisé.");
            d.Add(Candle(3, 110, 105)); var high = d.Add(Candle(4, 105, 100))!.Value.Point;
            d.Add(Candle(5, 100, 105)); var low = d.Add(Candle(6, 105, 110))!.Value.Point;
            Require(high.Price == 110.25 && low.Price == 99.75 && high.High && !low.High && !low.CloseSeed,
                "Aperçu modifie une confirmation ou creux inventé à une clôture.");
        });
        Check("Structure : deux bougies sur trois et creux réel dans la pause", () =>
        {
            var d = new ReactiveSwings(.75, 5, .25, .5);
            d.Add(Candle(0, 99, 100)); d.Add(Candle(1, 100, 105)); d.Add(Candle(2, 105, 110));
            Require(d.Add(Candle(3, 110, 106)) == null, "Première bougie suffit.");
            Require(d.Add(Candle(4, 106, 106, 107, 100)) == null, "Pause traitée comme confirmation.");
            var high = d.Add(Candle(5, 106, 103))!.Value.Point;
            Require(high.High && high.Index == 2 && high.ConfirmedAt == 5, "Pause efface le retournement.");
            d.Add(Candle(6, 103, 106)); var low = d.Add(Candle(7, 106, 110))!.Value.Point;
            Require(!low.High && low.Index == 4 && low.Price == 100, "Le vrai creux de la pause est oublié.");
        });
        Check("Structure : bougie de rejet au sommet comptée sans ordre des mèches inventé", () =>
        {
            var d = new ReactiveSwings(.75, 5, .25, .5);
            d.Add(Candle(0, 99, 100)); d.Add(Candle(1, 100, 105)); d.Add(Candle(2, 105, 110));
            Require(d.Add(Candle(3, 110, 105, 112, 80)) == null, "Sommet publié sur la bougie qui l'étend.");
            var high = d.Add(Candle(4, 105, 100))!.Value.Point;
            Require(high.Index == 3 && high.Price == 112 && high.ConfirmedAt == 4, "Première bougie de rejet oubliée.");
            d.Add(Candle(5, 100, 105)); var low = d.Add(Candle(6, 105, 110))!.Value.Point;
            Require(low.Price == 99.75 && low.Index == 4, "Mèche antérieure au sommet utilisée comme futur creux.");
        });
        Check("Structure : deux pauses ou une reprise directionnelle annulent le retournement", () =>
        {
            foreach (bool resumed in new[] { false, true })
            {
                var d = new ReactiveSwings(.75, 5, .25, .5);
                d.Add(Candle(0, 99, 100)); d.Add(Candle(1, 100, 105)); d.Add(Candle(2, 105, 110));
                d.Add(Candle(3, 110, 105));
                d.Add(Candle(4, 105, resumed ? 108 : 105));
                if (!resumed) d.Add(Candle(5, 105, 105));
                Require(d.Add(Candle(6, resumed ? 108 : 105, 100)) == null, "Ancien vote de retournement réutilisé.");
            }
        });
        Check("Structure : seuil adapté après une contraction de volatilité", () =>
        {
            var d = new ReactiveSwings(.75, 5, .25, .5);
            for (int i = 0; i < 8; i++)
                d.Add(Candle(i, 100 + i * 2, 102 + i * 2, 112 + i * 2, 90 + i * 2));
            Require(d.Add(Candle(8, 116, 115)) == null, "Un corps confirme le retournement.");
            Require(d.Add(Candle(9, 115, 114)) == null, "Ancienne volatilité ignorée trop tôt.");
            var point = d.Add(Candle(10, 114, 113));
            Require(point.HasValue && point.Value.Point.Index == 7 && point.Value.Point.High,
                "L'amplitude des grandes bougies bloque les nouveaux retournements.");
        });
        Check("Structure : trois corps sur quatre, une seule pause permise", () =>
        {
            var d = new ReactiveSwings(.75, 5, .25, .5, 3);
            d.Add(Candle(0, 99, 100)); d.Add(Candle(1, 100, 105)); d.Add(Candle(2, 105, 110)); d.Add(Candle(3, 110, 115));
            d.Add(Candle(4, 115, 110)); d.Add(Candle(5, 110, 110));
            Require(d.Add(Candle(6, 110, 105)) == null, "Deux corps suffisent malgré le réglage trois.");
            Require(d.Add(Candle(7, 105, 100))!.Value.Point.Index == 3, "Trois corps avec pause non reconnus.");
        });
        Check("Jambes alternées : rebonds hésitants reconnus haut/bas/haut/bas", () =>
        {
            var detector = new AlternatingLegs(.75, 14, .25, .5, 2);
            var legacy = new ReactiveSwings(.75, 14, .25, .5);
            var data = new[] { Candle(0, 99, 100), Candle(1, 100, 110), Candle(2, 110, 120),
                Candle(3, 120, 112), Candle(4, 115, 113), Candle(5, 114, 116), Candle(6, 116, 118),
                Candle(7, 118, 114), Candle(8, 117, 115), Candle(9, 116, 117) };
            var points = new List<SwingPoint>(); int oldCount = 0;
            foreach (var bar in data)
            {
                if (detector.Add(bar) is SwingChange change) points.Add(change.Point);
                if (legacy.Add(bar).HasValue) oldCount++;
            }
            Require(points.Select(p => p.High).SequenceEqual(new[] { true, false, true, false }), "Les quatre mouvements ne sont pas reconnus.");
            Require(points.Select(p => p.Index).SequenceEqual(new[] { 2, 3, 6, 7 }), "Points déplacés hors des extrêmes des jambes.");
            Require(points.Select(p => p.Label).SequenceEqual(new[] { "H", "L", "LH", "HL" }), "Classification calculée avant les mouvements.");
            Require(points.Count > oldCount, "La succession manquée par l'ancien détecteur reste ignorée.");
        });
        Check("Jambes alternées : mêmes règles dans une baisse et dans une hausse", () =>
        {
            var up = new AlternatingLegs(.75, 14, .25, .5, 2);
            var down = new AlternatingLegs(.75, 14, .25, .5, 2);
            var data = new[] { Candle(0, 99, 100), Candle(1, 100, 110), Candle(2, 110, 120),
                Candle(3, 120, 112), Candle(4, 115, 113), Candle(5, 114, 116), Candle(6, 116, 118),
                Candle(7, 118, 114), Candle(8, 117, 115), Candle(9, 116, 117) };
            int count = 0;
            foreach (var frame in data)
            {
                var a = up.Add(frame);
                var b = down.Add(frame with { Open = -frame.Open, High = -frame.Low, Low = -frame.High, Close = -frame.Close });
                Require(a.HasValue == b.HasValue, "Détection asymétrique selon le sens.");
                if (!a.HasValue) continue;
                count++;
                Require(a.Value.Point.Index == b!.Value.Point.Index && a.Value.Point.High != b.Value.Point.High
                    && a.Value.Point.ConfirmedAt == b.Value.Point.ConfirmedAt, "Indices ou confirmation différents en miroir.");
                Near(a.Value.Point.Price, -b.Value.Point.Price);
            }
            Require(count == 4, "Succession de mouvements incomplète.");
        });
        Check("Jambes alternées : une mèche ou un retour immédiat ne confirme pas de vague", () =>
        {
            foreach (bool wick in new[] { false, true })
            {
                var d = new AlternatingLegs(.75, 14, .25, .5, 2);
                d.Add(Candle(0, 99, 100)); d.Add(Candle(1, 100, 110)); d.Add(Candle(2, 110, 120));
                Require(d.Add(wick ? Candle(3, 120, 120, 140, 80) : Candle(3, 120, 112)) == null, "Un seul retrait suffit.");
                Require(d.Add(wick ? Candle(4, 120, 120, 139, 75) : Candle(4, 112, 122)) == null,
                    "Mèche ou retrait annulé transformé en swing.");
            }
        });
        Check("Jambes alternées : confirmation à trois clôtures et maintien du déplacement", () =>
        {
            var d = new AlternatingLegs(.75, 14, .25, .5, 3);
            d.Add(Candle(0, 99, 100)); d.Add(Candle(1, 100, 110)); d.Add(Candle(2, 110, 120)); d.Add(Candle(3, 120, 125));
            Require(d.Add(Candle(4, 125, 115)) == null, "Une clôture suffit.");
            Require(d.Add(Candle(5, 115, 115)) == null, "Deux clôtures ignorent le réglage trois.");
            Require(d.Add(Candle(6, 118, 116))!.Value.Point.Index == 3, "Consolidation après retrait ignorée.");
        });
        Check("Jambes alternées : bougie extérieure, aucun ordre intrabougie inventé", () =>
        {
            var d = new AlternatingLegs(.75, 14, .25, .5, 2);
            d.Add(Candle(0, 99, 100)); d.Add(Candle(1, 100, 110)); d.Add(Candle(2, 110, 120));
            Require(d.Add(Candle(3, 120, 112, 124, 80)) == null, "Sommet confirmé sur sa bougie d'extension.");
            var high = d.Add(Candle(4, 112, 112))!.Value.Point;
            Require(high.Index == 3 && high.Price == 124, "Sommet de la bougie extérieure perdu.");
            d.Add(Candle(5, 112, 116)); var low = d.Add(Candle(6, 116, 118))!.Value.Point;
            Require(!low.High && low.Index == 4 && low.Price == 111.75, "Ancienne mèche de 80 utilisée comme creux suivant.");
        });
        Check("Jambes alternées : aperçu causal sans mutation et points immuables", () =>
        {
            var a = new AlternatingLegs(.75, 14, .25, .5, 2); var b = new AlternatingLegs(.75, 14, .25, .5, 2);
            var data = new[] { Candle(0, 99, 100), Candle(1, 100, 110), Candle(2, 110, 120),
                Candle(3, 120, 112), Candle(4, 115, 113), Candle(5, 114, 116), Candle(6, 116, 118),
                Candle(7, 118, 114), Candle(8, 117, 115), Candle(9, 116, 117) };
            var points = new List<SwingPoint>();
            foreach (var bar in data)
            {
                var expected = a.Add(bar);
                b.Preview(bar with { High = 9999, Low = -9999 });
                var actual = b.Add(bar);
                Require(actual == expected, "L'aperçu change la confirmation.");
                if (actual.HasValue) points.Add(actual.Value.Point);
            }
            Require(points.Zip(points.Skip(1)).All(pair => pair.First.High != pair.Second.High
                && pair.First.Index < pair.Second.Index), "Alternance ou chronologie incohérente.");
            Require(points.All(p => p.ConfirmedAt > p.Index), "Confirmation précoce ou même bougie aux deux extrêmes.");
        });
        Check("Divergences : 12 accepté, 13 refusé pour CVD / RSI / MACD", () =>
        {
            foreach (string source in new[] { "CVD", "RSI", "MACD" })
            foreach (int distance in new[] { 12, 13 })
            {
                var data = Enumerable.Range(0, 16).Select(i => Frame(i, 12, 10, 5) with { Rsi = 30, Macd = -2 }).ToArray();
                int second = 1 + distance;
                data[second] = data[second] with { Low = 8, CvdLow = 15, Rsi = 40, Macd = 2 };
                var opts = Options() with { MaximumGap = 500, Cvd = source == "CVD", Technical = source != "CVD",
                    Oscillator = source == "MACD" ? TechnicalOscillator.MacdHistogram : TechnicalOscillator.Rsi };
                var d = new Divergences(opts);
                d.Offer(new SwingChange(new(1, 2, 10, false, "L")), data);
                var signals = d.Offer(new SwingChange(new(second, second, 8, false, "LL")), data);
                Require(signals.Count == (distance == 12 ? 1 : 0), $"Borne des 12 incorrecte pour {source}, distance {distance}.");
            }
        });
        Check("CVD locale : absorption détectée sans grand swing confirmé", () =>
        {
            double[] highs = { 100, 101, 110, 105, 106, 105, 107, 108, 107, 106 };
            var data = new List<MarketFrame>(); var local = new LocalCvdPivots(2, 1, .25);
            var major = new AtrSwings(10, 3, .25); var divergence = new Divergences(Options());
            var signals = new List<DivergenceSignal>(); int structures = 0;
            for (int i = 0; i < highs.Length; i++)
            {
                var frame = Frame(i, highs[i], highs[i] - 20, i == 7 ? 13 : 3); data.Add(frame);
                if (major.Add(frame).HasValue) structures++;
                foreach (var point in local.Add(frame)) signals.AddRange(divergence.Offer(point, data));
            }
            Require(structures == 0, "Témoin majeur trop sensible.");
            Require(signals.Any(s => s.Hidden && !s.Bullish && s.First.Index == 2 && s.Second.Index == 7),
                "CVD encore dépendante des étiquettes de structure majeures.");
        });
        Check("Alignement CVD : indices réels et aucune lecture après confirmation", () =>
        {
            var data = Enumerable.Range(0, 9).Select(i => Frame(i, 100, 90, 0)).ToArray();
            data[2] = data[2] with { High = 110, CvdHigh = 5 }; data[3] = data[3] with { CvdHigh = 10 };
            data[7] = data[7] with { High = 108, CvdHigh = 9 }; data[8] = data[8] with { CvdHigh = 20 };
            var opts = Options() with { AlignmentBars = 1 };
            var d = new Divergences(opts); d.Offer(new SwingChange(new(2, 3, 110, true, "H")), data);
            var signals = d.Offer(new SwingChange(new(7, 8, 108, true, "LH")), data);
            Require(signals.Count == 1 && signals[0].ValueIndex1 == 3 && signals[0].ValueIndex2 == 8, "Échantillons CVD mal ancrés.");
            var early = new Divergences(opts); early.Offer(new SwingChange(new(2, 3, 110, true, "H")), data);
            Require(early.Offer(new SwingChange(new(7, 7, 108, true, "LH")), data).Count == 0, "CVD future lue avant confirmation.");
        });
        Check("Bulles : tailles compactes, progression du volume et zoom gradué", () =>
        {
            var appearances = new[] { 50d, 118, 184, 307, 595, 5000 }
                .Select(v => DeepTradeAppearance.Calculate(v, 50, .25, 8, 32, 5, 14, 300, 75)).ToArray();
            Require(appearances.Zip(appearances.Skip(1)).All(pair => pair.First.Size < pair.Second.Size), "Progression du volume perdue.");
            Require(appearances[0].PriceRadius == 1 && appearances[^1].PriceRadius == 4, "Borne compacte non respectée.");
            Require(appearances[4].PriceRadius < 3.5 && appearances.All(a => a.FillOpacity <= 36), "Bulle moyenne encore trop grande ou opaque.");
            Require(appearances[0].LabelMaximumBars < appearances[^1].LabelMaximumBars
                && appearances[^1].LabelMaximumBars == 300, "Petits volumes affichés au zoom éloigné.");
        });
        Check("Bulles : mémoire conservée au recalcul, snapshots dédupliqués et instrument isolé", () =>
        {
            var cache = new TradeHistoryCache(2); cache.ResetContext("NQ:30s");
            var order = new DeepTrade(0, 100, true, 100, 60, 1, 100, 100, 50);
            cache.Remember("one", 123, order); cache.Remember("one", 123, order with { Volume = 120 });
            cache.ResetContext("NQ:30s");
            Require(cache.Values.Single().Trade.Volume == 120, "Recalcul vide le cache ou snapshots doublés.");
            cache.Remember("two", 456, order); cache.Remember("three", 789, order);
            Require(cache.Values.Count() == 2 && cache.Values.All(c => c.Key != "one"), "Cache non borné.");
            cache.ResetContext("ES:30s"); Require(!cache.Values.Any(), "Transactions mélangées entre instruments.");
        });
        Check("Bulles : exécutions sous le filtre mémorisables pour un futur seuil inférieur", () =>
        {
            var all = new DeepTradeAggregator(50, 99, 100, 0, false, retainBelowThreshold: true);
            all.Feed(0, 1, 100, 20, true, 7);
            var trade = all.Flush();
            Require(trade.HasValue && trade.Value.Volume == 20 && trade.Value.OrderId == 7, "Données sous le filtre perdues.");
            var filtered = new DeepTradeAggregator(50, 99, 100, 0, false);
            filtered.Feed(0, 1, 100, 20, true);
            Require(filtered.Flush() == null, "Ancien contrat du filtre modifié.");
        });
        Check("CVD précoce : aperçu compare sans transformer le candidat en pivot confirmé", () =>
        {
            var data = new[] { Frame(0, 11, 10, 5), Frame(1, 14, 12, 6), Frame(2, 10, 8, 15) };
            var d = new Divergences(Options());
            d.Offer(new SwingChange(new(0, 1, 10, false, "L")), data);
            var candidate = new SwingPoint(2, 2, 8, false, "L?");
            Require(d.Preview(candidate, data).Single().Validation == "CVD en formation", "Aperçu CVD absent.");
            Require(d.Preview(candidate, data).Count == 1, "L'aperçu modifie ses ancres.");
            Require(d.Offer(new SwingChange(candidate), data).Count == 1, "Aperçu consomme le futur signal confirmé.");
        });
        Check("CVD précoce : limite des 12 et sessions également appliquées", () =>
        {
            var data = Enumerable.Range(0, 16).Select(i => Frame(i, 11, 10, 5)).ToArray();
            var d = new Divergences(Options()); d.Offer(new SwingChange(new(0, 1, 10, false, "L")), data);
            data[12] = Frame(12, 10, 8, 15); data[13] = Frame(13, 10, 8, 15);
            Require(d.Preview(new(12, 12, 8, false, "L?"), data).Count == 1, "12 rejeté prématurément.");
            Require(d.Preview(new(13, 13, 8, false, "L?"), data).Count == 0, "13 accepté en temps réel.");
            data[12] = data[12] with { Session = 1 };
            Require(d.Preview(new(12, 12, 8, false, "L?"), data).Count == 0, "Session traversée par l'alerte précoce.");
        });
        Check("CVD précoce : candidat courant et extrême attendant la droite", () =>
        {
            var closed = new[] { Frame(0, 10, 8, 1), Frame(1, 14, 12, 2), Frame(2, 9, 7, 3) };
            var current = Frame(3, 12, 10, 4);
            var snapshot = new LiveFrameSnapshot();
            var view = snapshot.Update(closed, current);
            Require(view.Count == 4 && closed.Length == 3 && view[3] == current, "Snapshot muté dans l'historique.");
            Require(LiveCvdCandidates.Find(view, 1, 1, .25).Any(p => !p.High && p.Index == 2), "Creux attend encore une clôture à droite.");
            current = current with { Low = 6 };
            Require(LiveCvdCandidates.Find(snapshot.Update(closed, current), 1, 1, .25).Any(p => !p.High && p.Index == 3),
                "Nouveau creux intrabougie non détecté.");
        });
        Check("Structures classiques : copie partielle, null explicite et original préservé", () =>
        {
            var original = Frame(1, 12, 10, 5);
            var copy = original with { CvdLow = null, Close = 11.5, Session = 2 };
            Require(original.CvdLow == 5 && original.Session == 0 && original.Close == 11,
                "Une copie modifie les données originales.");
            Require(copy.CvdLow == null && copy.Close == 11.5 && copy.Session == 2
                && copy.High == original.High && copy.CvdHigh == original.CvdHigh,
                "La copie perd les autres champs ou ne permet plus null.");
        });
        Check("Structures classiques : égalité, nullables et déduplication des exécutions", () =>
        {
            var a = new DeepTrade(1, 2, true, 100, 50, 3, 99, 101, 20, 25, 7);
            var b = new DeepTrade(1, 2, true, 100, 50, 3, 99, 101, 20, 25, 7);
            Require(a == b && a.Equals((object)b) && a.GetHashCode() == b.GetHashCode(), "Égalité par valeur perdue.");
            Require(a != (b with { Volume = 51 }) && a != (b with { OrderId = 8 })
                && a != (b with { LargestPrint = 26 }), "Champ d'exécution ignoré par la comparaison.");
            SwingChange? first = new SwingChange(new SwingPoint(1, 3, 100, true, "HH"));
            SwingChange? second = new SwingChange(new SwingPoint(1, 3, 100, true, "HH"));
            Require(first == second && first != null, "Comparaison nullable des points perdue.");
            var preview1 = (Major: (SwingPoint?)first.Value.Point, Minor: (SwingPoint?)null);
            var preview2 = (Major: (SwingPoint?)second.Value.Point, Minor: (SwingPoint?)null);
            Require(preview1 == preview2, "Déduplication des points en formation perdue.");
        });
        Check("Structures classiques : collections et valeurs flottantes particulières", () =>
        {
            var a = Frame(1, 12, 10, 5) with { Rsi = double.NaN, CvdLow = null };
            var b = Frame(1, 12, 10, 5) with { Rsi = double.NaN, CvdLow = null };
            Require(a == b && a.GetHashCode() == b.GetHashCode(), "Comparaison NaN/null différente de celle des données précédentes.");
            var values = new Dictionary<MarketFrame, int> { [a] = 3 };
            Require(values.TryGetValue(b, out int stored) && stored == 3, "Clé par valeur non retrouvée.");
            Require(new HashSet<MarketFrame> { a, b }.Count == 1, "Donnée identique dupliquée dans une collection.");
            _ = default(DeepTrade).GetHashCode();
            _ = default(SwingPoint).GetHashCode();
        });
        Check("Structures classiques : constructeurs optionnels et déconstruction", () =>
        {
            var point = new SwingPoint(1, 3, 100, true, "HH");
            var (index, at, price, high, label, closeSeed) = point;
            Require(index == 1 && at == 3 && price == 100 && high && label == "HH" && !closeSeed,
                "Signature du constructeur ou déconstruction modifiée.");
            var order = new DeepTrade(1, 2, true, 100, 50, 3, 99, 101, 20);
            Require(order.LargestPrint == 0 && order.OrderId == 0, "Valeurs optionnelles modifiées.");
            Require(point.ToString().EndsWith(" }"), "Format du diagnostic incorrect.");
        });
        return passed;
    }

    private static DivergenceOptions Options() => new(true, false, TechnicalOscillator.Rsi, false,
        true, false, 60, .25, 1, .25, false, false, false, false, 20, 0);
    private static MarketFrame Frame(int i, double high, double low, double cvd) => new(i, 0,
        (high + low) / 2, high, low, (high + low) / 2, 100, cvd, cvd + 2, cvd, cvd + 1, 1, 50, 0, 10);
    private static double Number(JsonElement e, string name) => e.GetProperty(name).GetDouble();
    private static void CompareSeries(JsonElement expected, IEnumerable<double?> actual)
    {
        var values = actual.ToArray(); int i = 0;
        foreach (var item in expected.EnumerateArray())
        {
            if (item.ValueKind == JsonValueKind.Null) Require(values[i] == null, $"Valeur initiale à {i}.");
            else { Require(values[i].HasValue, $"Valeur absente à {i}."); Near(values[i]!.Value, item.GetDouble()); }
            i++;
        }
        Require(i == values.Length, "Séries non alignées.");
    }
    private static void ComparePoints(JsonElement expected, IEnumerable<SwingPoint> actual)
    {
        var wanted = expected.EnumerateArray().ToArray(); var got = actual.ToArray();
        Require(wanted.Length == got.Length, $"Nombre de swings : attendu {wanted.Length}, obtenu {got.Length}.");
        for (int i = 0; i < got.Length; i++)
        {
            Require(got[i].Index == wanted[i].GetProperty("index").GetInt32()
                && got[i].ConfirmedAt == wanted[i].GetProperty("confirmedAt").GetInt32()
                && got[i].High == wanted[i].GetProperty("high").GetBoolean()
                && got[i].Label == wanted[i].GetProperty("label").GetString(), $"Swing différent à {i} : {got[i]}.");
            Near(got[i].Price, Number(wanted[i], "price"));
        }
    }
    private static void Near(double actual, double expected) => Require(Math.Abs(actual - expected) < 1e-8, $"Valeur attendue {expected}, obtenue {actual}.");
    private static void Require(bool value, string message) { if (!value) throw new Exception(message); }
}
