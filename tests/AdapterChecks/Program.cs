using VolumetricaAPI.Chart;
using SwingDivergence;
using SwingDivergence.Analysis;
using Feed = VolumetricaAPI.Connection.Structure;
using static VolSysAPI.Structure;
using static VolSysAPI.ExternalStructure;

int passed = 0;
Run("Historique : attendre les OHLC finales", () =>
{
    var host = new Host();
    var indicator = host.MakeIndicator();
    indicator.ShowDiagnostics = true;
    indicator.HideChartText = false;
    indicator.OnSet(false, false);
    double[] prices = { 5, 10, 6, 12, 7, 11, 4, 9, 5, 8, 3, 10, 5, 6 };
    foreach (double _ in prices) host.Bars.Add(new BarClass());
    indicator.OnLoad();
    for (int i = 0; i < prices.Length; i++)
    {
        host.Index = i;
        indicator.OnOpen(false);
        if (i < prices.Length - 1) indicator.OnClose(false);
    }
    Require(host.PivotLabels().Count == 0, "Historique traité trop tôt.");
    for (int i = 0; i < prices.Length; i++) host.SetBar(i, prices[i]);
    indicator.OnEnd(false);
    foreach (string label in new[] { "HH", "HL", "LH", "LL" })
        Require(host.PivotLabels().Contains(label), $"Libellé absent : {label}");
    Require(host.Annotations.Any(a => a.Text?.Contains("v25 :") == true), "Diagnostic absent.");
    Require(indicator.Description.Contains("Swing & Divergence Identifier v25"), "Version absente du titre.");
});
Run("Annotations directes, visibilité, coordonnées et tailles", () =>
{
    var host = new Host();
    var indicator = host.MakeIndicator();
    indicator.ShowDiagnostics = false;
    indicator.OnLoad();
    host.Load(new double[] { 5, 10, 6, 12, 7, 11, 4, 9, 5 });
    indicator.OnEnd(false);
    Require(host.Annotations.Count > 0, "Aucun dessin créé.");
    Require(host.ChildGroups == 0, "Sous-groupes ajoutés au premier plan.");
    var circles = host.Annotations.Where(a => a.Text == "●").ToArray();
    Require(circles.Length > 0, "Bulles absentes.");
    Require(host.Annotations.All(a => a.AnnotationType != AnnotationType.Circle), "Cercle en unités du prix réintroduit.");
    foreach (var circle in circles)
    {
        Require(circle.Visible && circle.PlotForeground, "Cercle masqué.");
        Require(Math.Abs(circle.FontSize - 16 / .55) < .001 && circle.Width == 0 && circle.Height == 0,
            "La taille de la bulle doit être fixe à l'écran.");
        Require(circle.CoordinateXType == CoordinateTypeEnum.Absolute && circle.X >= 1, "X incorrect.");
        var label = host.Annotations.Single(a => a.AnnotationType == AnnotationType.Text && a.Text != "●" && a.Text != "○" && a.X == circle.X && a.Y == circle.Y);
        Require(label.Visible && label.FontSize > 0 && label.TextAlign == TextAlignment.VCenterHCenter, "Libellé incorrect.");
    }
});
Run("Bougie ouverte exclue et clôture traitée une seule fois", () =>
{
    var host = new Host();
    var indicator = host.MakeIndicator();
    indicator.ShowDiagnostics = false;
    indicator.OnLoad();
    host.Load(new double[] { 5, 10, 6, 12, 7, 11 });
    indicator.OnEnd(false);
    int before = host.Annotations.Count;
    host.SetBar(5, 1000);
    indicator.OnEnd(true);
    Require(host.Annotations.Count == before, "Bougie ouverte incluse.");
    host.SetBar(5, 11);
    host.Index = 5;
    indicator.OnClose(true);
    int after = host.Annotations.Count;
    indicator.OnClose(true);
    host.Bars.Add(new BarClass { High = 7, Low = 5 });
    host.Index = 6;
    indicator.OnOpen(true);
    indicator.OnEnd(true);
    Require(host.Annotations.Count == after, "Pivot ajouté plusieurs fois.");
});
Run("Limite de bulles et suppression des cercles / textes / traits", () =>
{
    var host = new Host();
    var indicator = host.MakeIndicator();
    indicator.MaxBubbles = 10;
    indicator.ShowStems = true;
    indicator.ShowDiagnostics = false;
    indicator.OnSet(false, false);
    indicator.OnLoad();
    host.Load(Enumerable.Range(0, 100).Select(i => i % 2 == 0 ? 5d - i / 20d : 10d + i / 20d).ToArray());
    indicator.OnEnd(false);
    Require(host.Annotations.Count(a => a.Text == "●") == 10, "Limite incorrecte.");
    Require(host.Annotations.Count == 40, "Annotations anciennes non supprimées.");
});
Run("Recalcul et filtres des libellés", () =>
{
    var host = new Host();
    var indicator = host.MakeIndicator();
    host.Load(new double[] { 5, 10, 6, 12, 7, 11, 4, 9, 5 });
    indicator.OnLoad();
    indicator.OnEnd(false);
    string[] before = host.PivotLabels().ToArray();
    host.Annotations.Clear();
    indicator.OnLoad();
    indicator.OnEnd(false);
    Require(before.SequenceEqual(host.PivotLabels()), "Recalcul non déterministe.");
    indicator.ShowHH = false;
    indicator.OnSet(false, false);
    host.Annotations.Clear();
    indicator.OnLoad();
    indicator.OnEnd(false);
    Require(!host.PivotLabels().Contains("HH"), "Filtre HH ignoré.");
});
Run("Historique court : aucune structure inventée", () =>
{
    var host = new Host();
    var indicator = host.MakeIndicator();
    indicator.LeftBars = indicator.RightBars = 3;
    indicator.OnSet(false, false);
    indicator.OnLoad();
    host.Load(new double[] { 5, 8, 7 });
    indicator.OnEnd(false);
    Require(host.PivotLabels().Count == 0, "Pivot prématuré sur historique court.");
});
Run("Masquer les lettres conserve les disques et leur taille écran", () =>
{
    var host = new Host();
    var indicator = host.MakeIndicator();
    indicator.ShowDiagnostics = false;
    indicator.ShowLabels = false;
    indicator.BubbleSize = 30;
    indicator.OnSet(false, false);
    indicator.OnLoad();
    host.Load(new double[] { 5, 10, 6, 12, 7, 11, 4, 9, 5 });
    indicator.OnEnd(false);
    Require(host.Annotations.Count > 0 && host.Annotations.All(a => a.Text is "●" or "○"
        && a.Width == 0), "Disques masqués avec les lettres ou mauvaise taille.");
});
Run("Gros trades historiques : prints réellement agrégés et dessin du volume", () =>
{
    var host = new Host();
    var indicator = host.MakeIndicator();
    indicator.ShowDeepTrades = true;
    indicator.DeepTradeAdaptive = false;
    indicator.OnSet(false, false);
    Require(indicator.OnTickCall == CallHandler.HistRT, "Transactions historiques non demandées.");
    host.Load(new double[] { 100, 100 });
    indicator.OnLoad();
    host.Index = 0;
    var time = new DateTime(2026, 10, 7, 12, 0, 0);
    indicator.OnTick(new Feed.TickByTick { exDt = time, price = 100, Vol = 10, AggrSide = Feed.AggressorSideEnum.Ask }, false, default);
    indicator.OnTick(new Feed.TickByTick { exDt = time, price = 100.25, Vol = 8, AggrSide = Feed.AggressorSideEnum.Ask }, false, default);
    Require(host.Annotations.Any(a => a.Text == "18"), "Sweep non visible dès le franchissement du volume minimum.");
    indicator.OnTick(new Feed.TickByTick { exDt = time.AddMilliseconds(1), price = 100, Vol = 2, AggrSide = Feed.AggressorSideEnum.Bid }, false, default);
    Require(host.Annotations.Any(a => a.Text == "18"), "Volume agrégé non dessiné.");
    var tradeCircle = host.Annotations.Single(a => a.AnnotationType == AnnotationType.Circle);
    Require(tradeCircle.Width > 0 && tradeCircle.Width <= indicator.DeepBubbleMaxSize * .25 / 2 && !tradeCircle.PlotForeground,
        "Rayon des gros trades non calibré ou cercle au-dessus des bougies.");
});
Run("CVD absente : message explicite et aucune fausse divergence", () =>
{
    var host = new Host();
    var indicator = host.MakeIndicator();
    indicator.ShowCvdDivergences = true;
    indicator.ShowDivergences = true; indicator.DivergenceSource = DivergenceSource.Cvd;
    host.Load(new double[] { 5, 10, 6, 12, 7, 11, 4, 9, 5 });
    indicator.OnSet(false, false);
    indicator.OnLoad(); indicator.OnEnd(false);
    Require(indicator.LastDiagnostic?.Contains("CVD indisponible") == true && indicator.StatusMessage == null, "Diagnostic absent ou phrase encore affichée.");
    Require(!host.Annotations.Any(a => a.Text?.StartsWith("CVD ") == true), "Divergence inventée sur CVD absente.");
});
Run("Mode de secours : taille optique et correction de centrage", () =>
{
    var host = new Host();
    var indicator = host.MakeIndicator();
    indicator.Drawing = BubbleDrawing.OpticalGlyph;
    host.Load(new double[] { 5, 10, 6, 12, 7, 11, 4, 9, 5 });
    indicator.OnLoad(); indicator.OnEnd(false);
    var disks = host.Annotations.Where(a => a.Text == "●").ToArray();
    Require(disks.Length > 0 && disks.All(a => a.FontSize > indicator.BubbleSize && a.PixelMargin > 0),
        "Taille optique ou compensation verticale manquante.");
});
Run("Nom exact du bundle", () => Require(SwingDivergenceIdentifier.Register().Name == "Swing & Divergence Identifier", "Nom incorrect."));
Run("Gros trades : classement bid/ask quand le côté agressif manque", () =>
{
    var host = new Host(); var indicator = host.MakeIndicator();
    indicator.ShowDeepTrades = true; indicator.DeepTradeAdaptive = false;
    host.Load(new double[] { 100, 100 }); indicator.OnSet(false, false); indicator.OnLoad(); host.Index = 0;
    var time = new DateTime(2026, 10, 7, 12, 0, 0);
    indicator.OnTick(new Feed.TickByTick { exDt = time, bid = 100, ask = 100.25, price = 100.25,
        Vol = 20, AggrSide = Feed.AggressorSideEnum.Between }, false, default);
    indicator.OnTick(new Feed.TickByTick { exDt = time.AddMilliseconds(1), bid = 100, ask = 100.25, price = 100,
        Vol = 30, AggrSide = Feed.AggressorSideEnum.Between }, false, default);
    indicator.OnEnd(false);
    Require(host.Annotations.Any(a => a.Tooltip?.Contains("DT achat agressif") == true), "Ask non classé en achat.");
    Require(host.Annotations.Any(a => a.Tooltip?.Contains("DT vente agressive") == true), "Bid non classé en vente.");
});
Run("Aucun print historique : avertissement sans faux gros trades", () =>
{
    var host = new Host(); var indicator = host.MakeIndicator(); indicator.ShowDeepTrades = true;
    host.Load(new double[] { 100, 100 }); indicator.OnSet(false, false); indicator.OnLoad(); indicator.OnEnd(false);
    Require(indicator.LastDiagnostic?.Contains("aucun historique de transactions") == true && indicator.StatusMessage == null, "Diagnostic absent ou phrase encore affichée.");
    Require(!host.Annotations.Any(a => a.Tooltip?.StartsWith("DT ") == true), "Faux gros trade issu des volumes de bougies.");
});
Run("Nouvelle session : références de structure réinitialisées", () =>
{
    var host = new Host(); var indicator = host.MakeIndicator();
    indicator.Detection = StructureDetection.AtrRetracement; indicator.SwingAtr = .1; indicator.ShowInitialPivots = true;
    host.Load(new double[] { 100, 110, 100, 120, 100, 80, 90, 100 }); host.Bars[3].IsNewDay = true;
    indicator.OnSet(false, false); indicator.OnLoad(); indicator.OnEnd(false);
    Require(host.PivotLabels().SequenceEqual(new[] { "H", "L" }), "Structure comparée à la session précédente.");
});
Run("Migration v4 : petites bulles transparentes, puis réglages conservés", () =>
{
    var indicator = new SwingDivergenceIdentifier { AppearanceVersion = 0, BubbleSize = 32, MinorBubbleSize = 24,
        Drawing = BubbleDrawing.PixelCircle, MinorOpacity = 255, StructureOpacityPercent = 100 };
    indicator.OnSet(false, false);
    Require(indicator.AppearanceVersion == 9 && indicator.BubbleSize == 16 && indicator.MinorBubbleSize == 12
        && indicator.StructureOpacityPercent == 15 && indicator.Drawing == BubbleDrawing.OpticalGlyph,
        "Ancien rendu opaque conservé après mise à jour.");
    Require(indicator.DeepTradeMinimum == 50 && indicator.DeepTradeMaximum == 0 && !indicator.DeepTradeAdaptive,
        "Filtre manuel initial incorrect.");
    Require(indicator.DeepTradeSource == DeepTradeSource.AgregatsReconstitues, "Dépendance à l'indicateur premium par défaut.");
    indicator.BubbleSize = 20; indicator.StructureOpacityPercent = 25; indicator.OnSet(false, false);
    Require(indicator.BubbleSize == 20 && indicator.StructureOpacityPercent == 25, "Réglages utilisateur écrasés à chaque recalcul.");
});
Run("Agrégats SDK : une seule bulle mise à jour pour un identifiant", () =>
{
    var host = new Host(); var indicator = host.MakeIndicator();
    indicator.ShowDeepTrades = true; indicator.DeepTradeSource = DeepTradeSource.AgregatsSDK;
    indicator.DeepTradeMinimum = 50; host.Load(new double[] { 100, 100 });
    indicator.OnSet(false, false); indicator.OnLoad(); host.Index = 0;
    Require(host.AggregateRequests > 0, "Agrégats SDK non demandés.");
    var time = new DateTime(2026, 10, 7, 12, 0, 0);
    var tick = new Feed.TickByTick { exDt = time, price = 100, AggrID = 777, AggrSide = Feed.AggressorSideEnum.Ask };
    var packet = new AggrInfo { Vol = 60, NumberOfTrades = 6, AvgPrice = 100, TradeDir = TradeDirStatus.Ask,
        StartPrice = 100, EndPrice = 100, MaxQtyOrd = 10 };
    indicator.OnTick(tick, false, packet);
    Require(host.Annotations.Count == 2, "Premier agrégat absent.");
    packet.Vol = 120; packet.NumberOfTrades = 12; packet.AvgPrice = 100.25; tick.exDt = time.AddMilliseconds(1);
    indicator.OnTick(tick, false, packet);
    Require(host.Annotations.Count == 2 && host.Annotations.Any(a => a.Text == "120")
        && !host.Annotations.Any(a => a.Text == "60"), "Snapshots superposés au lieu d'une mise à jour.");
    indicator.OnTick(tick, false, packet);
    Require(host.Annotations.Count == 2, "Snapshot dupliqué.");
});
Run("Volumes : zoom plus exigeant sur les petites bulles", () =>
{
    var host = new Host(); var indicator = host.MakeIndicator();
    indicator.ShowDeepTrades = true; indicator.DeepTradeSource = DeepTradeSource.AgregatsSDK;
    indicator.DeepTradeMinimum = 50; host.Load(new double[] { 100, 100 });
    indicator.OnSet(false, false); indicator.OnLoad(); host.Index = 0;
    var time = new DateTime(2026, 10, 7, 12, 0, 0);
    indicator.OnTick(new Feed.TickByTick { exDt = time, AggrID = 1 }, false,
        new AggrInfo { Vol = 50, NumberOfTrades = 1, AvgPrice = 100, TradeDir = TradeDirStatus.Bid, MaxQtyOrd = 50 });
    indicator.OnTick(new Feed.TickByTick { exDt = time.AddMilliseconds(1), AggrID = 2 }, false,
        new AggrInfo { Vol = 5000, NumberOfTrades = 1, AvgPrice = 100, TradeDir = TradeDirStatus.Bid, MaxQtyOrd = 5000 });
    var small = host.Annotations.Single(a => a.Text == "50"); var large = host.Annotations.Single(a => a.Text == "5000");
    Require(small.MaxBarsViewed < large.MaxBarsViewed && large.MaxBarsViewed == indicator.DeepLabelZoomBars,
        "Volumes toujours visibles au zoom éloigné ou taille non prise en compte.");
    Require(host.Annotations.Where(a => a.AnnotationType == AnnotationType.Circle).All(a => a.Width <= indicator.DeepBubbleMaxSize * .25 / 2),
        "Rayon non borné.");
});
Run("Filtre maximum : retirer un snapshot qui dépasse la limite", () =>
{
    var host = new Host(); var indicator = host.MakeIndicator();
    indicator.ShowDeepTrades = true; indicator.DeepTradeSource = DeepTradeSource.AgregatsSDK;
    indicator.DeepTradeMinimum = 50; indicator.DeepTradeMaximum = 100;
    host.Load(new double[] { 100, 100 }); indicator.OnSet(false, false); indicator.OnLoad(); host.Index = 0;
    var tick = new Feed.TickByTick { exDt = new DateTime(2026, 10, 7), AggrID = 1 };
    var packet = new AggrInfo { Vol = 60, NumberOfTrades = 1, AvgPrice = 100, TradeDir = TradeDirStatus.Ask };
    indicator.OnTick(tick, false, packet); Require(host.Annotations.Count == 2, "Agrégat admissible absent.");
    packet.Vol = 120; indicator.OnTick(tick, false, packet);
    Require(host.Annotations.Count == 0, "Bulle conservée après dépassement du filtre max.");
});
Run("Gros trades : contour fin et fond transparent même pour les agrégats", () =>
{
    var host = new Host(); var indicator = host.MakeIndicator();
    indicator.ShowDeepTrades = true; indicator.DeepTradeMinimum = 50;
    host.Load(new double[] { 100, 100 }); indicator.OnSet(false, false); indicator.OnLoad(); host.Index = 0;
    var time = new DateTime(2026, 10, 7);
    indicator.OnTick(new Feed.TickByTick { exDt = time, price = 100, Vol = 30, AggrID = 1, AggrSide = Feed.AggressorSideEnum.Bid }, false, default);
    indicator.OnTick(new Feed.TickByTick { exDt = time.AddMilliseconds(1), price = 100, Vol = 30, AggrID = 1, AggrSide = Feed.AggressorSideEnum.Bid }, false, default);
    indicator.OnTick(new Feed.TickByTick { exDt = time.AddMilliseconds(2), price = 100, Vol = 10, AggrID = 2, AggrSide = Feed.AggressorSideEnum.Bid }, false, default);
    var circle = host.Annotations.Single(a => a.AnnotationType == AnnotationType.Circle);
    Require(circle.BackColor.Equals(indicator.DeepSellColor.WithOpacity(56)), "Opacité de la référence 20–30 % non appliquée.");
    Require(circle.LineWidth == 1 && host.Annotations.Any(a => a.Text == "60"), "Contour ou volume absent.");
});
Run("Structure : fond à 15 % et texte contrasté", () =>
{
    var host = new Host(); var indicator = host.MakeIndicator();
    host.Load(new double[] { 5, 10, 6, 12, 7, 11, 4, 9, 5 }); indicator.OnLoad(); indicator.OnEnd(false);
    foreach (var disk in host.Annotations.Where(a => a.Text == "●"))
    {
        bool up = disk.Tooltip.StartsWith("HH") || disk.Tooltip.StartsWith("HL");
        Require(disk.ForeColor.Equals((up ? indicator.BullishColor : indicator.BearishColor).WithOpacity(38)),
            "Fond de structure opaque malgré le réglage à 15 %.");
    }
    Require(host.Annotations.Where(a => a.Text is "HH" or "HL" or "LH" or "LL").All(a => a.ForeColor.Equals(indicator.TextColor)),
        "Couleur du texte incorrecte.");
});
Run("Réglages avancés : sauvegarde et aliases compatibles", () =>
{
    var settings = new DivergenceExpert { CvdTolerance = 7, RequireTechnicalValidation = false };
    var restored = System.Text.Json.JsonSerializer.Deserialize<DivergenceExpert>(System.Text.Json.JsonSerializer.Serialize(settings))!;
    var indicator = new SwingDivergenceIdentifier { AppearanceVersion = 5, DivergenceExpert = restored };
    Require(indicator.CvdTolerance == 7 && !indicator.RequireTechnicalValidation, "Réglages imbriqués non conservés.");
    indicator.ShowDivergences = false; indicator.OnSet(false, false);
    Require(!indicator.ShowCvdDivergences && !indicator.ShowTechnicalDivergences, "Interrupteur général ignoré.");
    indicator.ShowDivergences = true; indicator.DivergenceSource = DivergenceSource.CvdEtMacd; indicator.OnSet(false, false);
    Require(indicator.ShowCvdDivergences && indicator.ShowTechnicalDivergences && indicator.Oscillator == TechnicalOscillator.MacdHistogram,
        "Choix simplifié des sources non transmis au moteur.");
});
Run("Alerte CVD : historique muet, nouveau signal sonore une seule fois", () =>
{
    var (host, indicator) = CvdScenario(true, true);
    Require(host.PlayedSounds.Count == 0, "Alertes sonores au chargement historique.");
    Require(host.Messages.Count == 0, "Notification au chargement avant validation du setup.");
    int oldZones = host.Annotations.Count(a => a.AnnotationType == AnnotationType.Rectangle);
    host.Index = 5; indicator.OnClose(true);
    Require(host.PlayedSounds.SequenceEqual(new[] { "Alert 2" }), "Nouveau signal non alerté avec le son choisi.");
    indicator.OnClose(true); indicator.OnEnd(true);
    Require(host.PlayedSounds.Count == 1, "Alerte répétée pour le même setup.");
    Require(oldZones == 0 && host.Annotations.All(a => a.AnnotationType != AnnotationType.Rectangle), "Projection de setup réintroduite.");
});
Run("Alerte CVD : arrêt sonore et contrôle du replay", () =>
{
    var (mutedHost, muted) = CvdScenario(false, true);
    mutedHost.Index = 5; muted.OnClose(true);
    Require(mutedHost.PlayedSounds.Count == 0, "Option sonore désactivée ignorée.");
    var (replayHost, replay) = CvdScenario(true, true, true);
    replayHost.Index = 5; replay.OnClose(true);
    Require(replayHost.PlayedSounds.Count == 0, "Son en replay sans autorisation.");
    var (allowedHost, allowed) = CvdScenario(true, true, true);
    allowed.SetupExpert.AlertInReplay = true;
    allowedHost.Index = 5; allowed.OnClose(true);
    Require(allowedHost.PlayedSounds.Count == 1, "Option d'alerte en replay ignorée.");
});
Run("Détecteur de setup indépendant des lignes de divergence", () =>
{
    var (host, indicator) = CvdScenario(true, true, false, false);
    host.Index = 5; indicator.OnClose(true);
    Require(host.PlayedSounds.Count == 1 && host.Annotations.Any(a => a.Text?.StartsWith("CVD EXH") == true),
        "Le masquage des divergences a arrêté le détecteur CVD.");
    Require(host.Annotations.All(a => a.AnnotationType != AnnotationType.Rectangle), "Zone prédictive encore dessinée.");
});
Run("Coin supérieur gauche : aucune phrase personnalisée par défaut", () =>
{
    var host = new Host(); var indicator = host.MakeIndicator(); indicator.ShowDeepTrades = true; indicator.ShowDiagnostics = true;
    indicator.OnSet(false, false); host.Load(new double[] { 100, 100 }); indicator.OnLoad(); indicator.OnEnd(false);
    Require(indicator.Description == "\u200B" && indicator.StatusMessage == null, "Titre ou message encore visible.");
    Require(!string.IsNullOrEmpty(indicator.LastDiagnostic), "Diagnostic supprimé au lieu d'être masqué.");
    Require(!host.Annotations.Any(a => a.Text?.Contains("Swing & Divergence Identifier") == true), "Compteurs imposés sur le graphique.");
});
Run("Migration v5 : gros trades agrandis sans opacifier les structures", () =>
{
    var indicator = new SwingDivergenceIdentifier { AppearanceVersion = 5, DeepBubbleSize = 8, DeepBubbleMaxSize = 40,
        BubbleSize = 16, StructureOpacityPercent = 15 };
    indicator.OnSet(false, false);
    Require(indicator.DeepBubbleSize == 8 && indicator.DeepBubbleMaxSize == 32 && indicator.AppearanceVersion == 9,
        "Anciennes tailles de gros trades conservées.");
    Require(indicator.BubbleSize == 16 && indicator.StructureOpacityPercent == 15, "Structures agrandies ou opacifiées.");
});
Run("CVD en direct sans structure affichée, aucune projection de setup", () =>
{
    var (host, indicator) = CvdScenario(true, true);
    indicator.ShowStructure = false;
    indicator.SetupExpert.ShowZones = true; // Old templates must not restore projected rectangles.
    host.Index = 7; indicator.OnClose(true);
    Require(host.PlayedSounds.Count == 1 && host.Annotations.Any(a => a.Text?.StartsWith("CVD EXH") == true),
        "CVD encore liée à l'affichage de la structure.");
    Require(host.Annotations.All(a => a.AnnotationType != AnnotationType.Rectangle)
        && !host.Annotations.Any(a => a.Text?.Contains("R:R") == true), "Prévision de setup encore dessinée.");
});
Run("Aperçu de structure sur tick : sans gros trades et sans alerte", () =>
{
    var host = new Host(); var indicator = host.MakeIndicator();
    indicator.Detection = StructureDetection.ReactiveSwings; indicator.LiveStructurePreview = true;
    indicator.ShowInitialPivots = true; indicator.SwingAtr = 2;
    host.Load(new double[] { 100, 108, 116, 110 });
    host.Bars[1].Open = 101; host.Bars[2].Open = 109;
    indicator.OnSet(false, false); indicator.OnLoad(); indicator.OnEnd(false);
    Require(indicator.OnTickCall == CallHandler.RT, "Pas de suivi direct quand les gros trades sont désactivés.");
    host.Index = 3;
    indicator.OnTick(new Feed.TickByTick { price = 125, Vol = 1 }, true, default);
    var preview = host.Annotations.Single(a => a.Text == "H?");
    Require(preview.X == 4 && preview.Y > 125, "Point en formation non actualisé sur le tick.");
    Require(host.PlayedSounds.Count == 0, "Alerte sur un point provisoire.");
});
Run("Ancien profil : méthode réactive et limite irrévocable des 12", () =>
{
    var indicator = new SwingDivergenceIdentifier { StructureVersion = 0, Detection = StructureDetection.AtrRetracement,
        SwingAtr = 2, DivergenceMaxBars = 60 };
    indicator.OnSet(false, false);
    Require(indicator.Detection == StructureDetection.AlternatingLegs && indicator.SwingAtr == .75
        && indicator.DivergenceMaxBars == 12, "Ancienne détection ou ancien écart 60 conservé.");
    indicator.DivergenceMaxBars = 1000; indicator.OnSet(false, false);
    Require(indicator.DivergenceMaxBars == 12, "Limite des 12 contournable dans les paramètres.");
});
Run("Bulles : changement de filtre sans rejeu des transactions, puis masquage/réactivation", () =>
{
    var host = new Host(); var indicator = host.MakeIndicator();
    indicator.ShowDeepTrades = true; indicator.DeepTradeMinimum = 50;
    host.Load(new double[] { 100, 100 }); indicator.OnSet(false, false); indicator.OnLoad(); host.Index = 0;
    var time = new DateTime(2026, 10, 7, 12, 0, 0);
    indicator.OnTick(new Feed.TickByTick { exDt = time, price = 100, Vol = 70, AggrID = 1, AggrSide = Feed.AggressorSideEnum.Ask }, false, default);
    indicator.OnTick(new Feed.TickByTick { exDt = time.AddMilliseconds(1), price = 100, Vol = 30, AggrID = 2, AggrSide = Feed.AggressorSideEnum.Bid }, false, default);
    indicator.OnEnd(false);
    Require(host.Annotations.Count(a => a.AnnotationType == AnnotationType.Circle) == 1, "Filtre initial ignoré.");
    indicator.DeepTradeMinimum = 20; indicator.OnSet(false, false); host.Annotations.Clear(); indicator.OnLoad(); indicator.OnEnd(false);
    Require(host.Annotations.Count(a => a.AnnotationType == AnnotationType.Circle) == 2 && host.Annotations.Any(a => a.Text == "30"),
        "Transactions effacées au recalcul ou transactions sous le filtre perdues.");
    indicator.DeepTradeMinimum = 80; indicator.OnSet(false, false); host.Annotations.Clear(); indicator.OnLoad(); indicator.OnEnd(false);
    Require(host.Annotations.All(a => a.AnnotationType != AnnotationType.Circle), "Relèvement du filtre ignoré.");
    indicator.ShowDeepTrades = false; indicator.OnSet(false, false); host.Annotations.Clear(); indicator.OnLoad(); indicator.OnEnd(false);
    indicator.ShowDeepTrades = true; indicator.DeepTradeMinimum = 20; indicator.OnSet(false, false); host.Annotations.Clear(); indicator.OnLoad(); indicator.OnEnd(false);
    Require(host.Annotations.Count(a => a.AnnotationType == AnnotationType.Circle) == 2, "Masquage supprime les données mémorisées.");
});
Run("Structure : même séquence avec ou sans divergences CVD", () =>
{
    List<string> Sequence(bool cvd)
    {
        var host = new Host(); var indicator = host.MakeIndicator();
        indicator.Detection = StructureDetection.ReactiveSwings; indicator.ShowInitialPivots = true;
        indicator.SwingAtr = .5; indicator.LiveStructurePreview = false;
        indicator.ShowDivergences = cvd; indicator.DivergenceSource = DivergenceSource.Cvd;
        host.Load(new double[] { 100, 105, 110, 105, 100, 105, 110, 105, 100, 99 });
        for (int i = 1; i < host.Bars.Count; i++) host.Bars[i].Open = host.Bars[i - 1].Close;
        indicator.OnSet(false, false); indicator.OnLoad(); indicator.OnEnd(false);
        return host.PivotLabels();
    }
    var without = Sequence(false); var withCvd = Sequence(true);
    Require(without.Count >= 2 && without.SequenceEqual(withCvd), "Deux détecteurs de structure s'exécutent en parallèle.");
});
Run("Pivots filtrés : structure présente même avec CVD activée", () =>
{
    var host = new Host(); var indicator = host.MakeIndicator();
    indicator.ShowDivergences = true; indicator.DivergenceSource = DivergenceSource.Cvd;
    host.Load(new double[] { 5, 10, 6, 12, 7, 11, 4, 9, 5 });
    indicator.OnSet(false, false); indicator.OnLoad(); indicator.OnEnd(false);
    Require(host.PivotLabels().Count > 0, "Calcul des fenêtres désactivé par la CVD.");
});
Run("Défauts v18 : CVD seule, ligne sur le panneau CVD et points en formation", () =>
{
    var defaults = new SwingDivergenceIdentifier(); defaults.OnSet(false, false);
    Require(defaults.DivergenceSource == DivergenceSource.Cvd && !defaults.ShowTechnicalDivergences
        && defaults.LiveStructurePreview && defaults.DrawCvdLines && defaults.DirectionalBars == 2,
        "Défauts demandés absents.");
    defaults.DivergenceSource = DivergenceSource.Rsi; defaults.LiveStructurePreview = false;
    defaults.DrawCvdLines = false; defaults.OnSet(false, false);
    Require(defaults.DivergenceSource == DivergenceSource.Rsi && !defaults.LiveStructurePreview && !defaults.DrawCvdLines,
        "Migration réécrit les préférences à chaque modification.");
    var (host, indicator) = CvdScenario(false, true);
    host.Index = 5; indicator.OnClose(false);
    Require(host.Annotations.Any(a => a.AnnotationType == AnnotationType.Line && !a.Area.Equals(ChartAreaRef.Main)),
        "Ligne CVD dessinée uniquement sur le prix.");
});
Run("Setup Div Cvd précoce : notification et lignes avant clôture, sans doublon final", () =>
{
    var (host, indicator) = LiveCvdScenario();
    Require(host.PlayedSounds.Count == 0 && host.Messages.Count == 0, "Notification au chargement.");
    Require(indicator.OnTickCall == CallHandler.RT, "Ticks non demandés quand les bulles sont désactivées.");
    host.Index = 5;
    indicator.OnTick(new Feed.TickByTick { price = 102, Vol = 1 }, true, default);
    indicator.OnTick(new Feed.TickByTick { price = 103, Vol = 1 }, true, default);
    indicator.OnEnd(true);
    Require(host.PlayedSounds.Count == 1 && host.Messages.Count == 1
        && host.Messages[0].Contains("précoce (provisoire)"), "Alerte attend encore la clôture.");
    Require(host.Annotations.Any(a => a.Text?.StartsWith("CVD EXH") == true && a.Text.EndsWith(" ?"))
        && host.Annotations.Any(a => a.AnnotationType == AnnotationType.Line && !a.Area.Equals(ChartAreaRef.Main)
            && a.LineDashStyle == DashStyle.Dot), "Tracé précoce prix/CVD absent.");
    indicator.OnClose(true);
    Require(host.PlayedSounds.Count == 1 && host.Messages.Count == 1
        && host.Annotations.Any(a => a.Text?.StartsWith("CVD EXH") == true && !a.Text.EndsWith(" ?")),
        "Confirmation non promue ou notification répétée.");
    indicator.OnClose(true); indicator.OnEnd(true);
    host.Bars.Add(new BarClass()); host.SetBar(6, 103); host.Index = 6; indicator.OnOpen(true);
    Require(host.PlayedSounds.Count == 1 && host.Messages.Count == 1, "Callbacks répètent l'alerte.");
});
Run("Setup Div Cvd : les options visuelles permissives ne déclenchent pas de fausse alerte", () =>
{
    var (host, indicator) = LiveCvdScenario();
    host.Bars[3].VolTotList[0].MinDeltaVol = -9;
    indicator.RequireClearCvdLine = false; indicator.ShowWeakCvdDivergences = true;
    indicator.OnSet(false, false); indicator.OnLoad(); indicator.OnEnd(false);
    host.Index = 5; indicator.OnClose(true);
    Require(host.Messages.Count == 0 && host.PlayedSounds.Count == 0, "Ligne CVD traversée traitée comme setup.");
});
Run("Setup Div Cvd précoce : tracé au tick du pivot, retrait si invalidé, sans renotifier", () =>
{
    var (host, indicator) = CvdScenario(true, true);
    indicator.EarlyCvdDetection = true; indicator.ShowStructure = false;
    host.Bars.RemoveAt(5); host.Index = 4;
    var original = host.Bars[4].VolTotList; host.Bars[4].VolTotList = null;
    indicator.OnSet(false, false); indicator.OnLoad(); indicator.OnEnd(false);
    host.Bars[4].VolTotList = original;
    indicator.OnTick(new Feed.TickByTick { price = 89, Vol = 1 }, true, default);
    Require(host.Messages.Count == 1 && host.Annotations.Any(a => a.Text?.StartsWith("CVD EXH") == true && a.Text.EndsWith(" ?")),
        "Le pivot attend encore sa propre clôture.");
    host.Bars[4].VolTotList = new[] { new InfoVolClass { AskVol = 50, BidVol = 80, TotVol = 130, MaxDeltaVol = 0, MinDeltaVol = -30 } };
    indicator.OnTick(new Feed.TickByTick { price = 89, Vol = 1 }, true, default);
    Require(host.Messages.Count == 1 && !host.Annotations.Any(a => a.Text?.StartsWith("CVD EXH") == true && a.Text.EndsWith(" ?")),
        "Tracé invalidé conservé ou nouvelle notification.");
    host.Bars[4].VolTotList = original;
    indicator.OnTick(new Feed.TickByTick { price = 89, Vol = 1 }, true, default);
    Require(host.Messages.Count == 1 && host.PlayedSounds.Count == 1
        && host.Annotations.Any(a => a.Text?.StartsWith("CVD EXH") == true && a.Text.EndsWith(" ?")),
        "Le retour d'une même condition renotifie le mouvement.");
});
Run("Absorption baissière : mêmes HH/HL/LH que le graphique, même lorsque leur affichage est masqué", () =>
{
    var host = new Host(); var indicator = host.MakeIndicator();
    indicator.Detection = StructureDetection.AlternatingLegs; indicator.SwingAtr = .75; indicator.AtrPeriod = 14;
    indicator.DirectionalBars = 2; indicator.PriceToleranceTicks = 1; indicator.ShowStructure = false;
    indicator.ShowDivergences = true; indicator.DivergenceSource = DivergenceSource.Cvd;
    indicator.ShowCvdSetups = indicator.EarlyCvdDetection = indicator.EnableCvdSound = indicator.ShowCvdNotification = true;
    double[] o = { 100, 100, 105, 110, 114, 108, 105, 109, 112 };
    double[] h = { 101, 106, 111, 115, 114, 110, 110, 113, 114 };
    double[] l = { 99, 100, 103, 108, 104, 103, 104, 107, 110 };
    double[] c = { 100, 105, 110, 114, 108, 105, 109, 112, 113 };
    int[] delta = { 0, 8, 10, 7, -5, -2, 7, 9, 9 };
    int[] max = { 2, 10, 12, 12, 2, 4, 9, 10, 11 }, min = { -1, 0, 0, 0, -7, -4, 0, 0, 0 };
    host.Load(c);
    for (int i = 0; i < c.Length; i++)
    {
        host.Bars[i].Open = o[i]; host.Bars[i].High = h[i]; host.Bars[i].Low = l[i]; host.Bars[i].Close = c[i];
        host.Bars[i].VolTotList = new[] { new InfoVolClass { AskVol = 50 + Math.Max(delta[i], 0),
            BidVol = 50 + Math.Max(-delta[i], 0), TotVol = 100 + Math.Abs(delta[i]), MaxDeltaVol = max[i], MinDeltaVol = min[i] } };
    }
    var currentTotals = host.Bars[8].VolTotList; host.Bars[8].VolTotList = null;
    indicator.OnSet(false, false); indicator.OnLoad(); indicator.OnEnd(false);
    host.Bars[8].VolTotList = currentTotals; host.Index = 8;
    indicator.OnTick(new Feed.TickByTick { price = 113, Vol = 1 }, true, default);
    Require(host.Messages.Count == 1 && host.Messages[0].Contains("baissier (ABS)")
        && host.Annotations.Any(a => a.Text?.StartsWith("CVD ABS") == true)
        && indicator.CvdSetupStatus.Contains("ABS baissière"), "LH prix / HH CVD non détecté avec les vagues affichées.");
    indicator.OnTick(new Feed.TickByTick { price = 113.25, Vol = 1 }, true, default);
    Require(host.Messages.Count == 1 && host.PlayedSounds.Count == 1, "Absorption notifiée plusieurs fois.");
});
Run("Setup Div Cvd : notification indépendante du son et interrupteur du détecteur", () =>
{
    var (host, indicator) = LiveCvdScenario(); host.Index = 5; indicator.EnableCvdSound = false;
    indicator.OnClose(true);
    Require(host.Messages.Count == 1 && host.PlayedSounds.Count == 0, "Notification coupée avec le son.");
    var (offHost, off) = LiveCvdScenario(); off.ShowCvdSetups = false; offHost.Index = 5; off.OnClose(true);
    Require(offHost.Messages.Count == 0 && offHost.PlayedSounds.Count == 0, "Divergence brute notifiée malgré le détecteur désactivé.");
});
Run("Setup Div Cvd : replay et rechargement de l'historique muets par défaut", () =>
{
    var (host, indicator) = LiveCvdScenario(true); host.Index = 5;
    indicator.OnClose(true);
    Require(host.Messages.Count == 0 && host.PlayedSounds.Count == 0, "Notification de replay non autorisée.");
    var (histHost, history) = LiveCvdScenario();
    histHost.Bars.Add(new BarClass()); histHost.SetBar(6, 103);
    history.OnLoad(); history.OnEnd(false); histHost.Index = 5; history.OnClose(true);
    Require(histHost.Messages.Count == 0 && histHost.PlayedSounds.Count == 0, "Setup historique réalerté après rechargement.");
});
Run("Migration des alertes : setups seuls, anciens modes neutralisés et préférences conservées", () =>
{
    var indicator = new SwingDivergenceIdentifier { AlertTiming = CvdAlertTiming.DivergenceEnFormation };
    indicator.OnSet(false, false);
    Require(indicator.AlertTiming == CvdAlertTiming.SetupConfirme && indicator.AlertVersion == 19,
        "Ancien mode provisoire conservé.");
    indicator.AlertTiming = CvdAlertTiming.DivergenceDetectee; indicator.ShowCvdNotification = false; indicator.OnSet(false, false);
    Require(indicator.AlertTiming == CvdAlertTiming.SetupConfirme && !indicator.ShowCvdNotification,
        "Choix explicite réécrit à chaque recalcul.");
});
Run("Structure v18 : historique et direct identiques avec une pause entre les corps", () =>
{
    List<(string Label, double X)> Sequence(bool rt)
    {
        var host = new Host(); var indicator = host.MakeIndicator();
        indicator.Detection = StructureDetection.ReactiveSwings; indicator.ShowInitialPivots = true;
        indicator.SwingAtr = .75; indicator.LiveStructurePreview = false;
        double[] opens = { 99, 100, 105, 110, 106, 106, 103, 106, 110, 108, 108, 101, 104 };
        double[] closes = { 100, 105, 110, 106, 106, 103, 106, 110, 108, 108, 101, 104, 107 };
        host.Load(closes);
        for (int i = 0; i < closes.Length; i++)
        {
            host.Bars[i].Open = opens[i]; host.Bars[i].Close = closes[i];
            host.Bars[i].High = Math.Max(opens[i], closes[i]) + .25;
            host.Bars[i].Low = Math.Min(opens[i], closes[i]) - .25;
        }
        host.Bars[4].Low = 100;
        indicator.OnSet(false, false); indicator.OnLoad();
        if (rt)
        {
            var future = host.Bars.ToArray();
            host.Bars.Clear(); host.Bars.Add(future[0]);
            host.Index = 0; indicator.OnEnd(false);
            // Feed one bar at a time; the host exposes no future OHLC.
            for (int i = 0; i < future.Length - 1; i++)
            {
                host.Index = i; indicator.OnClose(true); indicator.OnClose(true);
                host.Bars.Add(future[i + 1]); host.Index = i + 1;
                indicator.OnOpen(true); indicator.OnEnd(true);
            }
        }
        else indicator.OnEnd(false);
        return host.Annotations.Where(a => a.Text is "H" or "L" or "HH" or "HL" or "LH" or "LL")
            .Select(a => (a.Text, a.X)).ToList();
    }
    var history = Sequence(false); var live = Sequence(true);
    Require(history.Count >= 3 && history.SequenceEqual(live), "Ordre, index ou répétitions de points différents en direct.");
    Require(history.Any(p => p.Label == "L" && p.X == 5), "Creux entouré dans la pause non dessiné.");
});
Run("Profil v11 : seuil de base corrigé une fois, seuil personnalisé conservé", () =>
{
    var indicator = new SwingDivergenceIdentifier { StructureVersion = 9, AppearanceVersion = 9, SwingAtr = 1.25 };
    indicator.OnSet(false, false);
    Require(indicator.StructureVersion == 17 && indicator.SwingAtr == .75, "Ancien seuil reste trop sélectif.");
    indicator.SwingAtr = .9; indicator.OnSet(false, false);
    Require(indicator.SwingAtr == .9, "Migration réécrit le réglage utilisateur.");
    var custom = new SwingDivergenceIdentifier { StructureVersion = 9, SwingAtr = 1.1 };
    custom.OnSet(false, false); Require(custom.SwingAtr == 1.1, "Profil personnalisé effacé.");
});
Run("Métadonnées locales : description d'assemblage absente et ID configuré présent", () =>
{
    var attributes = typeof(SwingDivergenceIdentifier).Assembly.GetCustomAttributesData();
    Require(!attributes.Any(a => a.AttributeType == typeof(System.Reflection.AssemblyDescriptionAttribute)),
        "Attribut de description d'assemblage réintroduit.");
    Require(attributes.Any(a => a.AttributeType == typeof(System.Reflection.AssemblyMetadataAttribute)
        && (string?)a.ConstructorArguments[0].Value == "DeepchartDevId"
        && !string.IsNullOrWhiteSpace((string?)a.ConstructorArguments[1].Value)),
        "Métadonnée locale DeepchartDevId absente.");
});
Run("Chargement : conteneurs d'analyse sans contrats de type réfléchis", () =>
{
    var ownTypes = typeof(SwingDivergenceIdentifier).Assembly.GetTypes();
    Require(!ownTypes.Any(t => t.GetProperty("EqualityContract", System.Reflection.BindingFlags.Instance
        | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic) != null),
        "Un record de référence génère encore un contrat System.Type.");
});
Run("Jambes alternées : même succession en historique et direct, sans bougie future", () =>
{
    List<(string Label, double X)> Sequence(bool rt)
    {
        var host = new Host(); var indicator = host.MakeIndicator();
        indicator.Detection = StructureDetection.AlternatingLegs; indicator.ShowInitialPivots = true;
        indicator.SwingAtr = .75; indicator.LiveStructurePreview = false;
        double[] opens = { 99, 100, 110, 120, 115, 114, 116, 118, 117, 116, 117 };
        double[] closes = { 100, 110, 120, 112, 113, 116, 118, 114, 115, 117, 118 };
        host.Load(closes);
        for (int i = 0; i < closes.Length; i++)
        {
            host.Bars[i].Open = opens[i]; host.Bars[i].Close = closes[i];
            host.Bars[i].High = Math.Max(opens[i], closes[i]) + .25;
            host.Bars[i].Low = Math.Min(opens[i], closes[i]) - .25;
        }
        indicator.OnSet(false, false); indicator.OnLoad();
        if (rt)
        {
            var future = host.Bars.ToArray(); host.Bars.Clear(); host.Bars.Add(future[0]);
            host.Index = 0; indicator.OnEnd(false);
            for (int i = 0; i < future.Length - 1; i++)
            {
                host.Index = i; indicator.OnClose(true); indicator.OnClose(true);
                host.Bars.Add(future[i + 1]); host.Index = i + 1; indicator.OnOpen(true); indicator.OnEnd(true);
            }
        }
        else indicator.OnEnd(false);
        return host.Annotations.Where(a => a.Text is "H" or "L" or "HH" or "HL" or "LH" or "LL")
            .Select(a => (a.Text, a.X)).ToList();
    }
    var history = Sequence(false); var live = Sequence(true);
    Require(history.SequenceEqual(live) && history.Count == 4, "Succession différente, incomplète ou répétée en direct.");
    Require(history.Select(p => p.X).SequenceEqual(new[] { 3d, 4, 7, 8 }), "Position des points incorrecte.");
});
Run("Défaut v18 : vagues alternées, ancien profil migré et choix avancé conservé", () =>
{
    var old = new SwingDivergenceIdentifier { StructureVersion = 12, Detection = StructureDetection.ReactiveSwings };
    old.OnSet(false, false);
    Require(old.StructureVersion == 17 && old.Detection == StructureDetection.AlternatingLegs, "Ancien détecteur reste actif.");
    old.Detection = StructureDetection.FilteredPivots; old.OnSet(false, false);
    Require(old.Detection == StructureDetection.FilteredPivots, "Méthode personnalisée réécrite après migration.");
    Require(SwingDivergenceIdentifier.Register().Description.Length is >= 400 and <= 500, "Description hors plage demandée.");
});
Run("CVD sans structure : rose sur prix et CVD, sans libellé ni alerte par défaut", () =>
{
    var (host, indicator) = BareCvdScenario(); host.Index = 5;
    indicator.OnTick(new Feed.TickByTick { price = 102, Vol = 1 }, true, default);
    var lines = host.Annotations.Where(a => a.AnnotationType == AnnotationType.Line).ToArray();
    Require(lines.Length == 2 && lines.All(a => a.LineColor.Equals(ColorRef.FromRgb(244, 114, 182).WithOpacity(128)))
        && lines.Any(a => !a.Area.Equals(ChartAreaRef.Main)), "Catégorie rose ou tracé CVD absent.");
    Require(!host.Annotations.Any(a => a.Text?.StartsWith("CVD ") == true)
        && host.Messages.Count == 0 && host.PlayedSounds.Count == 0, "Divergence sans structure signalée comme un setup confirmé.");
});
Run("CVD sans structure : son seul, popup seule et désactivation de la catégorie", () =>
{
    var (soundHost, sound) = BareCvdScenario(); sound.CvdWithoutStructure.PlaySound = true;
    sound.CvdWithoutStructure.ShowPopup = false; soundHost.Index = 5;
    sound.OnTick(new Feed.TickByTick { price = 102, Vol = 1 }, true, default);
    Require(soundHost.PlayedSounds.Count == 1 && soundHost.Messages.Count == 0, "Le son dépend encore de la popup.");
    var (popupHost, popup) = BareCvdScenario(); popup.CvdWithoutStructure.ShowPopup = true;
    popup.CvdWithoutStructure.PlaySound = false; popupHost.Index = 5;
    popup.OnTick(new Feed.TickByTick { price = 102, Vol = 1 }, true, default);
    Require(popupHost.Messages.Count == 1 && popupHost.PlayedSounds.Count == 0, "La popup dépend encore du son.");
    var (offHost, off) = BareCvdScenario(); off.CvdWithoutStructure.Enabled = false;
    off.CvdWithoutStructure.PlaySound = off.CvdWithoutStructure.ShowPopup = true; offHost.Index = 5;
    off.OnTick(new Feed.TickByTick { price = 102, Vol = 1 }, true, default);
    Require(offHost.Annotations.Count == 0 && offHost.Messages.Count == 0 && offHost.PlayedSounds.Count == 0,
        "La catégorie désactivée continue de tracer ou notifier.");
});
Run("CVD avec structure : reste actif quand la catégorie rose est désactivée", () =>
{
    var (host, indicator) = LiveCvdScenario(); indicator.CvdWithoutStructure.Enabled = false;
    indicator.CvdWithStructure.OpacityPercent = 70; indicator.CvdWithStructure.ShowPopup = false;
    host.Index = 5; indicator.OnTick(new Feed.TickByTick { price = 102, Vol = 1 }, true, default);
    Require(host.PlayedSounds.Count == 1 && host.Messages.Count == 0
        && host.Annotations.Any(a => a.Text?.StartsWith("CVD EXH") == true)
        && host.Annotations.Where(a => a.AnnotationType == AnnotationType.Line)
            .All(a => a.LineColor.Equals(indicator.BullishColor.WithOpacity(178))), "Les deux catégories restent couplées.");
});
Run("CVD : cible trouvée par l'identifiant du Delta Cumulative natif", () =>
{
    var (host, indicator) = LiveCvdScenario();
    host.OtherIndicators[3] = new BaseIndicatorPrms { name = "Delta Cumulative Candlestick", C_Area = 4, IndexAxis = 0 };
    host.Index = 5; indicator.OnTick(new Feed.TickByTick { price = 102, Vol = 1 }, true, default);
    Require(host.Annotations.Any(a => a.AnnotationType == AnnotationType.Line && a.Area.Equals(ChartAreaRef.Indicator(4))),
        "La ligne ne suit pas la zone de l'indicateur CVD choisi.");
});
Run("Bulles : couleur, opacité et police des captures conservées au recalcul", () =>
{
    var host = new Host(); var indicator = host.MakeIndicator(); indicator.ShowDeepTrades = true;
    indicator.DeepTradeMinimum = 50; host.Load(new double[] { 100, 100 }); indicator.OnLoad(); host.Index = 0;
    var now = new DateTime(2026, 10, 8);
    indicator.OnTick(new Feed.TickByTick { exDt = now, price = 100, Vol = 30, AggrID = 123, AggrSide = Feed.AggressorSideEnum.Ask }, false, default);
    Require(host.Annotations.Count == 0, "Bulle sous le filtre affichée.");
    indicator.OnTick(new Feed.TickByTick { exDt = now.AddMilliseconds(1), price = 100, Vol = 40, AggrID = 123, AggrSide = Feed.AggressorSideEnum.Ask }, false, default);
    Require(host.Annotations.Count(a => a.AnnotationType == AnnotationType.Circle) == 1
        && host.Annotations.Any(a => a.Text == "70" && a.FontSize == 12), "Le seuil attend encore la fin de l'ordre.");
    indicator.OnTick(new Feed.TickByTick { exDt = now.AddMilliseconds(2), price = 100, Vol = 20, AggrID = 123, AggrSide = Feed.AggressorSideEnum.Ask }, false, default);
    Require(host.Annotations.Count(a => a.AnnotationType == AnnotationType.Circle) == 1
        && host.Annotations.Any(a => a.Text == "90") && !host.Annotations.Any(a => a.Text == "70"), "Snapshots de l'ordre superposés.");
    Require(indicator.DeepBuyColor.Equals(ColorRef.FromRgb(0, 102, 22)) && indicator.TradePlot.StandardDeviation == 2.5
        && indicator.TradePlot.MinimumSize == .2 && indicator.TradePlot.MaximumSize == 25, "Profil des captures différent.");
});
Console.WriteLine($"{passed} vérifications de l'adaptateur réussies.");

void Run(string name, Action test) { test(); passed++; Console.WriteLine($"OK — {name}"); }
static void Require(bool condition, string message) { if (!condition) throw new Exception(message); }

static (Host Host, SwingDivergence.SwingDivergenceIdentifier Indicator) CvdScenario(bool sound, bool setups, bool replay = false, bool lines = true)
{
    var host = new Host { Replay = replay }; var indicator = host.MakeIndicator();
    indicator.ShowDivergences = lines; indicator.DivergenceSource = DivergenceSource.Cvd;
    indicator.ShowCvdSetups = setups; indicator.EnableCvdSound = sound;
    indicator.ShowCvdNotification = true;
    indicator.EarlyCvdDetection = false;
    indicator.AlertTiming = CvdAlertTiming.SetupConfirme;
    indicator.SetupExpert.SwingAtr = .5; indicator.SetupExpert.AtrPeriod = 3;
    indicator.PriceToleranceTicks = 1;
    indicator.CvdSound.SelValue = "Alert 2";
    indicator.CvdAtClose = true; indicator.RequireClearPriceLine = false; indicator.RequireClearCvdLine = false;
    indicator.DivergenceExpert.CvdLeftBars = 1; indicator.DivergenceExpert.CvdRightBars = 1;
    host.Load(new double[] { 100, 105, 92, 106, 89, 102 });
    double[] opens = { 100, 100, 105, 92, 106, 89 };
    double[] highs = { 101, 106, 105, 108, 106, 103 };
    double[] lows = { 99, 100, 90, 92, 88, 89 };
    double[] closes = { 100, 105, 92, 106, 89, 102 };
    int[] deltas = { 0, 10, -20, 10, -8, 10 };
    int[] maxima = { 1, 10, 0, 20, 0, 13 }, minima = { -1, 0, -30, 0, -10, 0 };
    for (int i = 0; i < deltas.Length; i++)
    {
        host.Bars[i].Open = opens[i]; host.Bars[i].High = highs[i];
        host.Bars[i].Low = lows[i]; host.Bars[i].Close = closes[i];
        host.Bars[i].VolTotList = new[] { new InfoVolClass { AskVol = 50 + Math.Max(deltas[i], 0),
            BidVol = 50 + Math.Max(-deltas[i], 0), TotVol = 100 + Math.Abs(deltas[i]),
            MaxDeltaVol = maxima[i], MinDeltaVol = minima[i] } };
    }
    indicator.OnSet(false, false); indicator.OnLoad(); indicator.OnEnd(false);
    return (host, indicator);
}


static (Host Host, SwingDivergence.SwingDivergenceIdentifier Indicator) LiveCvdScenario(bool replay = false)
{
    var (host, indicator) = CvdScenario(true, true, replay);
    indicator.EarlyCvdDetection = true;
    // Seed historical callbacks without an already-valid live candidate.
    var currentTotals = host.Bars[5].VolTotList;
    host.Bars[5].VolTotList = null;
    indicator.ShowStructure = false; indicator.SwingAtr = 10; indicator.ShowDeepTrades = false;
    indicator.ShowDivergences = true; indicator.DivergenceSource = DivergenceSource.Cvd;
    indicator.ShowCvdSetups = true; indicator.EnableCvdSound = true; indicator.ShowCvdNotification = true;
    indicator.AlertTiming = CvdAlertTiming.DivergenceEnFormation;
    indicator.CvdAtClose = true; indicator.RequireClearPriceLine = false; indicator.RequireClearCvdLine = false;
    indicator.DivergenceExpert.CvdLeftBars = indicator.DivergenceExpert.CvdRightBars = 1;
    indicator.OnSet(false, false); indicator.OnLoad(); indicator.OnEnd(false);
    host.Bars[5].VolTotList = currentTotals;
    return (host, indicator);
}

static (Host Host, SwingDivergence.SwingDivergenceIdentifier Indicator) BareCvdScenario()
{
    var (host, indicator) = CvdScenario(true, true);
    indicator.Detection = StructureDetection.ReactiveSwings; indicator.SwingAtr = 10; indicator.DirectionalBars = 3;
    indicator.ShowStructure = false; indicator.EarlyCvdDetection = true;
    var totals = host.Bars[5].VolTotList; host.Bars[5].VolTotList = null;
    indicator.OnSet(false, false); indicator.OnLoad(); indicator.OnEnd(false); host.Bars[5].VolTotList = totals;
    return (host, indicator);
}
