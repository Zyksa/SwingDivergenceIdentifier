using SwingDivergence.Analysis;
using System.Text.Json;

internal static class SetupChecks
{
    public static int Run()
    {
        int passed = 0;
        void Check(string name, Action test) { test(); passed++; Console.WriteLine($"OK — {name}"); }

        Check("Setup structurel : parité exacte avec les setups Python sur 600 bougies", () =>
        {
            using var document = JsonDocument.Parse(File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "fixtures", "reference.json")));
            var root = document.RootElement; var builder = new FrameBuilder(14, 14, 12, 26, 9, 20);
            var data = new List<MarketFrame>(); var result = new List<CvdSetup>();
            var engine = new StructuralCvdSetups(.75, 3,
                new(true, false, TechnicalOscillator.Rsi, false, true, false, 12, .25, 1, .25,
                    true, true, false, false, 20, 0), SetupOptions());
            double N(JsonElement row, string key) => row.GetProperty(key).GetDouble();
            foreach (var row in root.GetProperty("bars").EnumerateArray())
            {
                var frame = builder.Add(data.Count, false, N(row, "open"), N(row, "high"), N(row, "low"),
                    N(row, "close"), 100, N(row, "delta"), N(row, "maxDelta"), N(row, "minDelta"));
                data.Add(frame); result.AddRange(engine.Add(frame, data));
            }
            var expected = root.GetProperty("cvdSetups").EnumerateArray().ToArray();
            Require(expected.Length > 0 && expected.Length == result.Count,
                $"Nombre de setups Python {expected.Length}, C# {result.Count}.");
            for (int i = 0; i < result.Count; i++)
            {
                var actual = result[i]; var wanted = expected[i]; var signal = actual.Signal;
                string type = $"{(signal.Bullish ? "bullish" : "bearish")}_{(signal.Hidden ? "absorption" : "exhaustion")}";
                Require(signal.First.Index == wanted.GetProperty("first").GetInt32()
                    && signal.Second.Index == wanted.GetProperty("second").GetInt32()
                    && actual.AvailableAt == wanted.GetProperty("confirmedAt").GetInt32()
                    && type == wanted.GetProperty("type").GetString()
                    && signal.First.Price == N(wanted, "price1") && signal.Second.Price == N(wanted, "price2")
                    && signal.Value1 == N(wanted, "cvd1") && signal.Value2 == N(wanted, "cvd2"),
                    $"Setup {i} différent de la référence.");
            }
            engine = new StructuralCvdSetups(2, 14,
                new(true, false, TechnicalOscillator.Rsi, false, true, false, 12, .25, 1, .25,
                    true, true, false, false, 20, 0), SetupOptions());
            var prefix = new List<MarketFrame>(); result.Clear();
            foreach (var frame in data) { prefix.Add(frame); result.AddRange(engine.Add(frame, prefix)); }
            Require(result.Count == root.GetProperty("defaultCvdSetups").GetArrayLength(),
                "Nombre de setups différent avec les paramètres par défaut Python.");
        });
        Check("Structure affichée : LH prix / HH CVD détecté malgré un retournement inférieur à 2 ATR", () =>
        {
            var data = SharedFixture(); var (engine, waves, closed) = SharedScenario();
            var coarse = new StructuralCvdSetups(2, 14, SharedOptions(), SetupOptions());
            var prefix = new List<MarketFrame>();
            for (int i = 0; i < 8; i++) { prefix.Add(data[i]); coarse.Add(data[i], prefix); }
            Require(coarse.Preview(data[8], closed.Append(data[8]).ToArray()).Count == 0,
                "La fixture ne reproduit pas le blocage du détecteur ATR indépendant.");
            var point = waves.Preview(data[8])!.Value;
            Require(point.High && point.Label == "LH", "Le rebond n'est pas un LH de la structure affichée.");
            var result = engine.Preview(data[8], closed.Append(data[8]).ToArray(), engine.Candidates(new[] { point })).Single();
            Require(!result.Signal.Bullish && result.Signal.Hidden && result.Signal.First.Index == 3
                && result.Signal.Second.Index == 8 && result.Signal.First.Price == 115 && result.Signal.Second.Price == 114
                && result.Signal.Value1 == 30 && result.Signal.Value2 == 45,
                "Absorption baissière manquée ou ancrée sur d'autres points que le graphique.");
            Require(engine.LastCheck.Contains("ABS baissière"), "Diagnostic de détection absent.");
        });
        Check("Structure affichée : identité stable pendant le rebond puis à sa confirmation", () =>
        {
            var data = SharedFixture(); var (engine, waves, closed) = SharedScenario();
            var point = waves.Preview(data[8])!.Value;
            var early = engine.Preview(data[8], closed.Append(data[8]).ToArray(), engine.Candidates(new[] { point })).Single();
            string identity = engine.AlertIdentity(early.Signal);
            var change = waves.Add(data[8]); closed.Add(data[8]);
            engine.Add(data[8], closed, change.HasValue ? new[] { change.Value } : Array.Empty<SwingChange>());
            var newer = Row(9, 113, 114.5, 111, 113.5, 43, 50, 43, 48);
            point = waves.Preview(newer)!.Value;
            early = engine.Preview(newer, closed.Append(newer).ToArray(), engine.Candidates(new[] { point })).Single();
            Require(early.Signal.Second.Index == 9 && engine.AlertIdentity(early.Signal) == identity,
                "Le déplacement du LH crée une nouvelle notification.");
            var finals = new List<CvdSetup>();
            foreach (var bar in new[] { newer, Row(10, 113.5, 114, 109, 111, 48, 49, 40, 42),
                Row(11, 111, 112, 106, 108, 42, 43, 35, 37) })
            {
                change = waves.Add(bar); closed.Add(bar);
                finals.AddRange(engine.Add(bar, closed, change.HasValue ? new[] { change.Value } : Array.Empty<SwingChange>()));
            }
            var confirmed = finals.Single(s => s.Signal.First.Index == 3 && s.Signal.Second.Index == 9);
            Require(engine.AlertIdentity(confirmed.Signal) == identity, "Confirmation du LH renotifiée.");
        });
        Check("Structure affichée : diagnostic précis quand la ligne CVD coupe une bougie", () =>
        {
            var data = SharedFixture(); var (engine, waves, closed) = SharedScenario();
            var point = waves.Preview(data[8])!.Value;
            closed[7] = closed[7] with { CvdHigh = 50 };
            Require(engine.Preview(data[8], closed.Append(data[8]).ToArray(), engine.Candidates(new[] { point })).Count == 0,
                "Une ligne CVD traversée a été acceptée.");
            Require(engine.LastCheck.Contains("Ligne CVD traversée"), "Le refus reste inexpliqué.");
        });
        Check("Structure affichée : filtres d'absorption et sessions conservés", () =>
        {
            var data = SharedFixture();
            var engine = new StructuralCvdSetups(.75, 14, SharedOptions(), SetupOptions() with { Absorption = false }, true);
            var waves = new AlternatingLegs(.75, 14, .25, .5, 2); var closed = new List<MarketFrame>();
            for (int i = 0; i < 8; i++)
            {
                var change = waves.Add(data[i]); closed.Add(data[i]);
                engine.Add(data[i], closed, change.HasValue ? new[] { change.Value } : Array.Empty<SwingChange>());
            }
            var point = waves.Preview(data[8])!.Value;
            Require(engine.Preview(data[8], closed.Append(data[8]).ToArray(), engine.Candidates(new[] { point })).Count == 0,
                "Option absorptions ignorée.");
            var newSession = data[8] with { Session = 1 };
            Require(engine.Preview(newSession, closed.Append(newSession).ToArray(), engine.Candidates(new[] { point })).Count == 0,
                "Une ancre de l'ancienne session reste active.");
        });
        Check("Structure fenêtres : alternance provisoire sans attendre la clôture du second pivot", () =>
        {
            var data = Fixture(); var closed = new List<MarketFrame>(); var waves = new FilteredSwings(1, 1, .25, 0);
            var engine = new StructuralCvdSetups(2, 14, SharedOptions(), SetupOptions(), true);
            for (int i = 0; i < 4; i++)
            {
                closed.Add(data[i]); engine.Add(data[i], closed, waves.Add(data[i], closed));
            }
            var view = closed.Append(data[4]).ToArray();
            var early = engine.Preview(data[4], view, engine.Candidates(LiveCvdCandidates.Find(view, 1, 1, .25), view)).Single();
            string identity = engine.AlertIdentity(early.Signal);
            Require(early.Signal.First.Index == 2 && early.Signal.Second.Index == 4 && early.AvailableAt == 4,
                "Les fenêtres imposent encore une clôture à droite.");
            var final = new List<CvdSetup>();
            for (int i = 4; i < 6; i++)
            {
                closed.Add(data[i]); final.AddRange(engine.Add(data[i], closed, waves.Add(data[i], closed)));
            }
            Require(engine.AlertIdentity(final.Single().Signal) == identity, "Le point provisoire crée un double signal à la clôture.");
        });

        Check("Setup structurel : quatre types CVD à la première clôture admissible", () =>
        {
            foreach (bool absorption in new[] { false, true })
                foreach (bool bearish in new[] { false, true })
                {
                    var data = Fixture(absorption, bearish);
                    var engine = Engine(); var closed = new List<MarketFrame>();
                    for (int i = 0; i < 5; i++)
                    {
                        closed.Add(data[i]);
                        Require(engine.Add(data[i], closed).Count == 0, "Alerte avant la fin du mouvement.");
                    }
                    closed.Add(data[5]);
                    var result = engine.Add(data[5], closed).Single();
                    Require(result.Signal.Bullish == !bearish && result.Signal.Hidden == absorption
                        && result.Signal.First.Index == 2 && result.Signal.Second.Index == 4
                        && result.AvailableAt == 5, "Type, ancres ou moment incorrects.");
                    Require(result.Signal.ValueIndex1 == 2 && result.Signal.ValueIndex2 == 4,
                        "CVD prélevée sur une bougie voisine.");
                    Require(engine.Add(data[5], closed).Count == 0, "Même clôture traitée deux fois.");
                }
        });
        Check("Setup précoce : quatre types tracés dès le tick de la seconde ancre", () =>
        {
            foreach (bool absorption in new[] { false, true })
            foreach (bool bearish in new[] { false, true })
            {
                var data = Fixture(absorption, bearish); var engine = Engine();
                var closed = new List<MarketFrame>();
                for (int i = 0; i < 4; i++) { closed.Add(data[i]); engine.Add(data[i], closed); }
                var live = closed.Append(data[4]).ToArray();
                var setup = engine.Preview(data[4], live).Single();
                Require(setup.AvailableAt == 4 && setup.Signal.First.Index == 2 && setup.Signal.Second.Index == 4
                    && setup.Signal.Bullish == !bearish && setup.Signal.Hidden == absorption
                    && setup.Signal.Validation == StructuralCvdSetups.EarlyValidation,
                    "Le tick attend la clôture du pivot ou du retournement suivant.");
                string identity = engine.AlertIdentity(setup.Signal);
                closed.Add(data[4]); Require(engine.Add(data[4], closed).Count == 0, "Ancre anticipée enregistrée comme confirmée.");
                engine.Preview(data[5], closed.Append(data[5]).ToArray());
                closed.Add(data[5]); var confirmed = engine.Add(data[5], closed).Single();
                Require(engine.AlertIdentity(confirmed.Signal) == identity, "La clôture crée une seconde identité d'alerte.");
            }
        });
        Check("Setup précoce : extrême mobile, une seule identité pour tout le mouvement", () =>
        {
            var data = Fixture().Take(5).ToList();
            data.Add(Row(5, 89, 91, 87, 88, -8, -5, -8, -5));
            data.Add(Row(6, 88, 90, 86, 87, -5, -3, -6, -4));
            data.Add(Row(7, 87, 104, 87, 103, -4, 5, -4, 2));
            var closed = new List<MarketFrame>(); var engine = Engine();
            var ids = new List<string>(); var gate = new CvdAlertGate(); int notifications = 0;
            var now = new DateTime(2026, 10, 8, 12, 0, 0);
            for (int i = 0; i < 7; i++)
            {
                if (i >= 4)
                {
                    var early = engine.Preview(data[i], closed.Append(data[i]).ToArray()).Single();
                    ids.Add(engine.AlertIdentity(early.Signal));
                    for (int tick = 0; tick < 3; tick++)
                        if (gate.Accept(ids[^1], true, false, false, i, i, now.AddSeconds(i), 0)) notifications++;
                }
                closed.Add(data[i]); engine.Add(data[i], closed);
            }
            closed.Add(data[7]); var final = engine.Add(data[7], closed).Single();
            Require(ids.Distinct().Count() == 1 && engine.AlertIdentity(final.Signal) == ids[0]
                && notifications == 1 && !gate.Accept(ids[0], true, false, false, 7, 7, now.AddSeconds(7), 0),
                "Chaque nouvelle bougie de l'extrême répète la notification.");
        });
        Check("Setup précoce : invalidation intrabougie et filtres stricts", () =>
        {
            var data = Fixture(); var closed = new List<MarketFrame>(); var engine = Engine();
            for (int i = 0; i < 4; i++) { closed.Add(data[i]); engine.Add(data[i], closed); }
            var valid = data[4];
            Require(engine.Preview(valid, closed.Append(valid).ToArray()).Count == 1, "Setup initial absent.");
            var invalid = valid with { CvdLow = -30 };
            Require(engine.Preview(invalid, closed.Append(invalid).ToArray()).Count == 0, "Setup invalidé toujours présent.");
            invalid = valid with { CvdLow = -19 };
            Require(engine.Preview(invalid, closed.Append(invalid).ToArray()).Count == 0, "Signal faible admis sur les ticks.");
            invalid = valid with { CvdLow = null };
            Require(engine.Preview(invalid, closed.Append(invalid).ToArray()).Count == 0, "CVD manquante remplacée.");
            invalid = valid with { Session = 1 };
            Require(engine.Preview(invalid, closed.Append(invalid).ToArray()).Count == 0, "Session précédente réutilisée.");
            closed[3] = closed[3] with { CvdLow = -19 };
            Require(engine.Preview(valid, closed.Append(valid).ToArray()).Count == 0, "Mèche traversant la ligne CVD ignorée en direct.");
        });
        Check("Setup précoce : les ticks ne changent jamais le résultat des clôtures", () =>
        {
            var data = Fixture(); var engine = Engine(); var closed = new List<MarketFrame>();
            var actual = new List<CvdSetup>();
            foreach (var frame in data)
            {
                var view = closed.Append(frame).ToArray();
                for (int tick = 0; tick < 10; tick++) engine.Preview(frame, view);
                closed.Add(frame); actual.AddRange(engine.Add(frame, closed));
            }
            Require(actual.SequenceEqual(Detect(data)), "Les previews ont modifié l'ATR ou les ancres confirmées.");
        });
        Check("Setup précoce : le BOS optionnel est détecté sur le tick qui franchit le niveau", () =>
        {
            var data = Fixture(); var closed = new List<MarketFrame>();
            var engine = Engine(SetupOptions() with { Confirmation = SetupConfirmation.BosOuBalayage, Bearish = false });
            foreach (var frame in data) { closed.Add(frame); Require(engine.Add(frame, closed).Count == 0, "BOS publié avant franchissement."); }
            var current = Row(6, 102, 110, 102, 107, 2, 8, 2, 8);
            Require(engine.Preview(current, closed.Append(current).ToArray()).Count == 0, "Seule la mèche a validé le BOS.");
            current = current with { Close = 109 };
            var early = engine.Preview(current, closed.Append(current).ToArray()).Single();
            string identity = engine.AlertIdentity(early.Signal);
            Require(early.AvailableAt == 6 && early.Signal.First.Index == 2 && early.Signal.Second.Index == 4,
                "Attente inutile de la clôture BOS.");
            closed.Add(current); var final = engine.Add(current, closed).Single();
            Require(engine.AlertIdentity(final.Signal) == identity, "BOS final crée une deuxième alerte.");
        });
        Check("Setup structurel : un creux qui évolue ne produit pas d'alertes en formation", () =>
        {
            var data = Fixture().Take(5).ToList();
            data.Add(Row(5, 89, 91, 87, 88, -8, -5, -8, -5));
            data.Add(Row(6, 88, 90, 86, 87, -5, -3, -6, -4));
            data.Add(Row(7, 87, 104, 87, 103, -4, 5, -4, 2));
            var result = Detect(data);
            Require(result.Count == 1 && result[0].Signal.Second.Index == 6 && result[0].AvailableAt == 7,
                "Les candidats intermédiaires alertent ou retardent le véritable setup.");
        });
        Check("Setup structurel : bougie extérieure, extension avant confirmation", () =>
        {
            var data = Fixture();
            data[5] = Row(5, 89, 103, 87, 102, -8, 5, -8, 2);
            Require(Detect(data).Count == 0, "La bougie extérieure confirme sa propre extension.");
            data.Add(Row(6, 102, 108, 100, 107, 2, 8, 2, 8));
            Require(Detect(data).Single().AvailableAt == 6, "Confirmation ultérieure perdue.");
        });
        Check("Setup structurel : filtres de lignes obligatoires malgré les options visuelles", () =>
        {
            var cvdCut = Fixture();
            cvdCut[3] = cvdCut[3] with { CvdLow = -19, CvdHigh = 10 };
            Require(Detect(cvdCut).Count == 0, "Traversée de mèche CVD ignorée.");
            var priceCut = Fixture();
            priceCut[3] = priceCut[3] with { Low = 88 };
            Require(Detect(priceCut).Count == 0, "Traversée de mèche de prix ignorée.");
            var edge = Fixture(); edge[3] = edge[3] with { CvdLow = -16 };
            Require(Detect(edge).Count == 1, "Contact exact de tolérance refusé.");
        });
        Check("Setup structurel : absence de CVD ou donnée non finie, sans remplacement par la clôture", () =>
        {
            foreach (double? missing in new double?[] { null, double.NaN, double.PositiveInfinity })
            {
                var endpoint = Fixture(); endpoint[4] = endpoint[4] with { CvdLow = missing };
                Require(Detect(endpoint).Count == 0, "Extrême absent remplacé par un voisin ou la clôture.");
                var interior = Fixture(); interior[3] = interior[3] with { CvdLow = missing };
                Require(Detect(interior).Count == 0, "Ligne validée avec données illisibles.");
            }
        });
        Check("Setup structurel : faible mouvement et mauvais sens ne notifient pas", () =>
        {
            var flat = Fixture(); flat[4] = flat[4] with { CvdLow = -19 };
            Require(Detect(flat).Count == 0, "Différence égale à la tolérance acceptée.");
            var sameDirection = Fixture(); sameDirection[4] = sameDirection[4] with { CvdLow = -30 };
            Require(Detect(sameDirection).Count == 0, "Prix et CVD de même sens alertent.");
        });
        Check("Setup structurel : chargement et flux donnent les mêmes ancres sans regard vers le futur", () =>
        {
            var data = Fixture(); var history = Detect(data);
            var engine = Engine(); var live = new List<CvdSetup>(); var prefix = new List<MarketFrame>();
            for (int i = 0; i < data.Count; i++)
            {
                Require(engine.Add(data[i], data).Count == 0 || i == data.Count - 1,
                    "Un tableau complet a permis un regard vers le futur.");
                // A separate engine must receive only closed prefixes.
            }
            engine = Engine();
            foreach (var bar in data) { prefix.Add(bar); live.AddRange(engine.Add(bar, prefix)); }
            Require(history.SequenceEqual(live), "Historique/direct différents.");
        });
        Check("Setup structurel : séparation des sessions et contrôle des directions", () =>
        {
            var split = Fixture();
            for (int i = 3; i < split.Count; i++) split[i] = split[i] with { Session = 1 };
            Require(Detect(split).Count == 0, "Une ancre de la session passée est réutilisée.");
            var options = SetupOptions() with { Bullish = false };
            Require(Detect(Fixture(), options).Count == 0, "Direction achat désactivée ignorée.");
            options = SetupOptions() with { Absorption = false };
            Require(Detect(Fixture(true), options).Count == 0, "Absorption désactivée ignorée.");
        });
        Check("Setup structurel : les prévisualisations et signaux faibles ne passent pas le filtre final", () =>
        {
            var data = Fixture(); var signal = Detect(data).Single().Signal;
            var gate = new CvdSetups(SetupOptions());
            Require(gate.Offer(signal with { Validation = "CVD en formation" }, data).Count == 0,
                "Prévisualisation admise comme setup.");
            Require(gate.Offer(signal with { Weak = true }, data).Count == 0, "Signal faible admis.");
            Require(gate.Offer(signal, data).Count == 1, "La prévisualisation a consommé le vrai setup.");
        });
        Check("Setup structurel : notification indépendante d'un plan de risque fictif", () =>
        {
            var data = Fixture(); var signal = Detect(data).Single().Signal;
            data[5] = data[5] with { Close = 80 };
            Require(new CvdSetups(SetupOptions()).Offer(signal, data).Count == 1,
                "Un plan de stop non demandé supprime un setup admissible.");
        });
        Check("Setup structurel : notification unique, chargement et rattrapage muets", () =>
        {
            var setup = Detect(Fixture()).Single(); var gate = new CvdAlertGate();
            var now = new DateTime(2026, 10, 8, 12, 0, 0);
            Require(gate.Accept(setup.Signal.Id, true, false, false, setup.AvailableAt, 5, now, 0), "Pas de première alerte.");
            Require(!gate.Accept(setup.Signal.Id, true, false, false, 5, 5, now.AddMilliseconds(20), 0), "Alerte répétée.");
            gate = new CvdAlertGate();
            Require(!gate.Accept(setup.Signal.Id, false, false, false, 5, 5, now, 0), "Historique sonore.");
            Require(!gate.Accept(setup.Signal.Id, true, false, false, 5, 5, now, 0), "Historique réalerté.");
            Require(!gate.Accept("catchup", true, false, false, 5, 6, now, 0), "Rattrapage ancien alerté.");
        });
        return passed;
    }

    private static StructuralCvdSetups Engine(SetupOptions? setup = null) => new(.5, 3,
        new(true, false, TechnicalOscillator.Rsi, false, false, true, 12, .25, 1, .25,
            false, false, false, false, 20, 1), setup ?? SetupOptions());
    private static DivergenceOptions SharedOptions() => new(true, false, TechnicalOscillator.Rsi, false,
        true, false, 12, .25, 1, .25, true, true, false, false, 20, 0);
    private static (StructuralCvdSetups Engine, AlternatingLegs Waves, List<MarketFrame> Closed) SharedScenario()
    {
        var data = SharedFixture(); var closed = new List<MarketFrame>();
        var waves = new AlternatingLegs(.75, 14, .25, .5, 2);
        var engine = new StructuralCvdSetups(.75, 14, SharedOptions(), SetupOptions(), true);
        for (int i = 0; i < 8; i++)
        {
            var change = waves.Add(data[i]); closed.Add(data[i]);
            engine.Add(data[i], closed, change.HasValue ? new[] { change.Value } : Array.Empty<SwingChange>());
        }
        return (engine, waves, closed);
    }
    private static List<MarketFrame> SharedFixture() => new()
    {
        Row(0, 100, 101, 99, 100, 0, 2, -1, 0),
        Row(1, 100, 106, 100, 105, 0, 10, 0, 8),
        Row(2, 105, 111, 103, 110, 8, 20, 8, 18),
        Row(3, 110, 115, 108, 114, 18, 30, 18, 25),
        Row(4, 114, 114, 104, 108, 25, 27, 18, 20),
        Row(5, 108, 110, 103, 105, 20, 24, 16, 18),
        Row(6, 105, 110, 104, 109, 18, 27, 18, 25),
        Row(7, 109, 113, 107, 112, 25, 35, 25, 34),
        Row(8, 112, 114, 110, 113, 34, 45, 34, 43)
    };
    private static SetupOptions SetupOptions() => new(SetupConfirmation.DivergenceConfirmee,
        2, .25, .5, .25, .25, 20, true, true, true, true);
    private static List<CvdSetup> Detect(List<MarketFrame> data, SetupOptions? options = null)
    {
        var engine = Engine(options); var frames = new List<MarketFrame>(); var result = new List<CvdSetup>();
        foreach (var bar in data) { frames.Add(bar); result.AddRange(engine.Add(bar, frames)); }
        return result;
    }
    private static List<MarketFrame> Fixture(bool absorption = false, bool bearish = false)
    {
        var data = new List<MarketFrame>
        {
            Row(0, 100, 101, 99, 100, 0, 1, -1, 0),
            Row(1, 100, 106, 100, 105, 0, 10, 0, 10),
            Row(2, 105, 105, 90, 92, 10, 10, -20, -10),
            Row(3, 92, 108, 92, 106, -10, 10, -10, 0),
            Row(4, 106, 106, 88, 89, 0, 0, -10, -8),
            Row(5, 89, 103, 89, 102, -8, 5, -8, 2)
        };
        if (absorption)
        {
            data[4] = Row(4, 106, 106, 92, 93, 0, 0, -30, -28);
            data[5] = Row(5, 93, 104, 93, 103, -28, 5, -28, 2);
        }
        if (bearish)
            for (int i = 0; i < data.Count; i++)
            {
                var b = data[i];
                data[i] = b with { Open = 200 - b.Open, High = 200 - b.Low, Low = 200 - b.High,
                    Close = 200 - b.Close, CvdOpen = -b.CvdOpen, CvdHigh = -b.CvdLow,
                    CvdLow = -b.CvdHigh, CvdClose = -b.CvdClose };
            }
        return data;
    }
    private static MarketFrame Row(int i, double o, double h, double l, double c,
        double co, double ch, double cl, double cc) => new(i, 0, o, h, l, c, 100,
            co, ch, cl, cc, 1, null, null, null);
    private static void Require(bool condition, string message) { if (!condition) throw new Exception(message); }
}
