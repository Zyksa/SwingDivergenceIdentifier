# Swing & Divergence Identifier — guide utilisateur

Indicateur C# Deepcharts : structure HH/HL/LH/LL, divergences CVD et bulles
vertes/violettes issues de transactions réelles. Identité du projet : classe `SwingDivergence.SwingDivergenceIdentifier`, fichier
`SwingDivergenceIdentifier.dll`. Après migration depuis l'ancienne identité,
réajouter l'indicateur sur les graphiques concernés.

## Réglages courants

**00 À propos** affiche **by Zyksa · Beta v0.6** en lecture seule, dans le menu
de configuration. Le numéro technique du build est distinct de cette version beta.

- **Général** : afficher la structure et les divergences ; source des
  divergences ; tracer leur ligne sur le CVD et choisir son panneau.
- **Structure** : afficher les points en formation, confirmer avec **2 ou 3
  bougies directionnelles**, amplitude du retournement et affichage HH/HL/LH/LL.
- **Bulles** : décocher **Activer les bulles** pour les désactiver ; **Volume
  minimum**, valeur initiale **50**. Les anciennes options de
  source, percentile, taille, opacité, maximum et agrégation sont conservées comme
  propriétés cachées pour la compatibilité, sans encombrer cette page.
- **Setup Div Cvd** : activation du détecteur, alerte sonore, choix du son et moment
  de déclenchement via **Détection précoce sur les ticks**. Aucune prévision
  d'entrée/stop/objectif ni rectangle de setup.

Défauts v9 : **CVD seule**, **points en formation activés**, **ligne sur le CVD
activée**, panneau **2** (premier panneau sous le prix). Les points provisoires
portent `?` : ils peuvent bouger, ne sont pas des confirmations et ne produisent
ni divergence ni son. Les phrases personnalisées en haut à gauche restent masquées.

Les profils antérieurs migrent une seule fois vers ces défauts et le nouveau
profil compact des bulles. Les choix ultérieurs sont conservés au recalcul.

## Structure

Le mode principal **AlternatingLegs** suit une succession de mouvements
**haut → bas → haut → bas**. HH/LH et HL/LL sont classés une fois le mouvement
reconnu, à partir du précédent extrême confirmé du même type.

1. La progression des clôtures établit la jambe initiale. La couleur du corps ne
   décide pas à elle seule du sens : une bougie rouge peut encore clôturer plus
   haut que la précédente.
2. Dans une jambe haussière, le moteur suit le plus haut réel et la plus haute
   clôture. Un recul des clôtures amorce un retournement. Deux clôtures maintenant
   ce recul confirment le sommet ; le réglage 3 demande trois clôtures. Un petit
   rebond ou un doji peut valider le maintien du recul sans effacer le mouvement.
3. La jambe baissière applique la règle symétrique. Son creux réel est conservé,
   y compris lorsqu'il précède la bougie ayant confirmé le sommet précédent.
   La première clôture de rebond déjà disponible est conservée pour le mouvement
   suivant : les petites reprises en V ne repartent plus avec un compteur vide.
4. L'amplitude minimale est au moins deux ticks. Elle est calculée à partir de la
   volatilité médiane locale et plafonnée à un quart de l'amplitude de la jambe,
   afin qu'une petite vague ne subisse pas tout le seuil d'une grande impulsion.
5. Une mèche sans déplacement des clôtures ne déclenche aucun retournement. Un
   retrait immédiatement annulé avant validation ne produit pas de point.
6. Les points alternent et leurs indices avancent strictement. Une bougie
   extérieure prolongeant un extrême ne le confirme pas sur la même bougie ; sa
   mèche opposée n'est pas utilisée pour inventer l'ordre des événements.

**Bougies validant le retournement** est réglable à 2 ou 3. **Amplitude du
retournement** ajuste le filtre de volatilité. Le profil réactif antérieur passe
une fois au nouveau mode ; les méthodes avancées choisies par l'utilisateur
restent disponibles. **ReactiveSwings** conserve l'ancienne règle stricte des
corps, **AtrRetracement** la règle classique et **FilteredPivots** les fenêtres.

Même calcul sur les bougies closes en historique et en direct. Le suivi provisoire
est actualisé sur les ticks. Les premiers H/L sont masqués, les HH/HL sont
turquoise et les LH/LL rouges. Les tolérances classent les sommets quasi égaux en
LH et les creux quasi égaux en HL. Nouvelle session : références réinitialisées.

Correction v9 : le calcul des fenêtres n'est plus exécuté en plus du mode réactif
quand la CVD est désactivée, et il reste disponible quand la CVD est activée.
La v22 aligne les divergences CVD et les alertes sur les points de la structure
principale ; le calcul des points reste actif même si leur affichage est masqué.

Les modes avancés `AtrRetracement` (référence Python classique) et
`FilteredPivots` (fenêtres, alternance et filtre ATR) restent disponibles. Le mode alterné est sélectionné par défaut.
Les petites bulles de structure gardent un diamètre nominal fixe à l'écran :
16 principal, 12 secondaire, fond 15 %, texte 9/8, distance aux mèches 4/3 ticks.
Le rendu par glyphes utilise une compensation optique ; son diamètre exact dépend
également de la police et de l'échelle Windows.

## Divergences CVD

CVD calculée depuis Ask moins Bid et les excursions delta intrabougie. Une donnée
manquante interrompt le cumul jusqu'à la prochaine session ; aucun delta fictif.
Les divergences sont séparées en deux catégories. **Avec structure** : au moins
une de leurs deux extrémités correspond à un point confirmé de la structure
principale. **Sans structure** : aucun de leurs deux points n'est confirmé par
cette structure ; les pivots locaux CVD permettent de tracer ces cas en rose.
Un point intermédiaire ou une autre mèche de la même bougie ne suffit pas.

- EXH haussier : prix LL, CVD plus haut ; EXH baissier : prix HH, CVD plus bas.
- ABS haussier : prix HL, CVD plus bas ; ABS baissier : prix LH, CVD plus haut.
- **12 intervalles maximum** entre les deux pivots, même avec une ancienne valeur
  enregistrée supérieure. Un réglage plus court reste possible.
- Même session, contrôles de traversée de lignes et alignement sur des bougies
  déjà closes. La ligne du panneau CVD utilise les indices réels des échantillons.

Le panneau sélectionné doit contenir un CVD avec le même cumul de session et des
volumes Ask/Bid sans filtre. RSI/MACD restent sélectionnables en option.

Dans **04 Div Cvd**, les groupes **Avec au moins un point de structure**
et **Sans point de structure — rose** ont chacun : activation, affichage des
tracés, opacité, son et popup. Désactiver une catégorie coupe ses dessins et ses
alertes. Masquer ses tracés ne coupe pas un son explicitement activé. Son et
popup sont indépendants : une popup désactivée n'empêche pas le son.
Le groupe rose est affiché à **50 %**, avec **son et popup désactivés** par défaut.
Le groupe avec structure conserve les couleurs directionnelles et les libellés
EXH/ABS + flèche, avec une opacité initiale de **100 %**.

Les lignes sont tracées sur le prix et sur le CVD lorsque **Tracer les divergences
sur le CVD** est activé. **Identifiant du CVD** vaut **3** pour le CVD de la capture.
Le SDK lit la zone de cet indicateur ; si cette cible ne peut pas être identifiée,
le numéro de panneau configuré reste utilisé comme secours.
## Notifications de Setup Div Cvd

La **Détection précoce sur les ticks** est activée par défaut. Le détecteur
compare le point extrême du mouvement en cours au point structurel précédent
de même type, sans attendre la clôture du pivot ou deux bougies de retournement.
La v22 réutilise les vagues qui produisent les points affichés : un HH puis un HL
du graphique peuvent donc servir d'ancre et d'origine au LH en formation. Elle
n'impose plus un second retournement indépendant de 2 ATR. Les ancres des bougies
closes restent inchangées par les ticks.
Une divergence locale visible ou un point de structure provisoire ne déclenche
pas directement l'alerte : tous les filtres du setup doivent être remplis.

La classification et les contrôles CVD s'appuient sur le moteur de référence,
avec des ancres issues de la structure sélectionnée :

- Mouvements alternés de la méthode principale sélectionnée dans **Structure**.
  Le défaut conserve les retournements validés sur **2 bougies closes** et un
  seuil d'amplitude local. Les candidats restent provisoires jusqu'à confirmation.
- Un sommet est comparé au sommet structurel immédiatement précédent, un creux
  au creux précédent. Les deux ancres sont dans la même session, à 12 intervalles
  maximum. Aucune recherche d'une ancienne paire plus attractive.
- CVD extrême prise sur **la même bougie que chaque point de prix**, sans
  alignement sur une bougie voisine ni remplacement par la clôture.
- Mouvements de prix et de CVD opposés, dépassant leurs tolérances. Les signaux
  faibles sont exclus. Les lignes de prix et de CVD ne doivent pas traverser les
  ranges des bougies intermédiaires, mèches comprises. Une donnée CVD manquante
  ou non finie bloque la validation.

L'alerte part **sur le premier tick qui satisfait les conditions du setup**.
Le tracé apparaît immédiatement sur le prix et le CVD : ligne pointillée et
libellé **EXH/ABS ?**, avec un message **« Setup Div Cvd précoce (provisoire) »**.
Le tracé suit les nouvelles valeurs et disparaît si la condition s'invalide.
La notification déjà émise ne peut pas être annulée. Une seule notification est
autorisée pour le même mouvement, même si son extrême change de bougie, si la
condition disparaît puis revient, ou si elle est ensuite confirmée à la clôture.
Après confirmation, le tracé perd son `?` et reprend le style normal.

Dans **Setup Div Cvd**, `Activer Setup Div Cvd` active le détecteur et ses alertes.
`Notification visuelle` et `Alerte sonore` sont indépendants. La méthode et
l'amplitude se règlent dans **Structure** et s'appliquent également aux setups.
Dans **Avancé → Setup Div Cvd et alertes**, les filtres de direction, EXH/ABS,
replay et délai entre alertes restent disponibles. La confirmation optionnelle
`BosOuBalayage` ajoute une condition : les ticks doivent aussi satisfaire ce
franchissement ou balayage. Désactiver **Détection précoce sur les ticks** permet
de revenir aux seuls mouvements confirmés sur bougies closes. Le calcul sur
bougies closes utilise les points confirmés de la méthode sélectionnée ; le mode
précoce est une anticipation intrabougie qui peut s'invalider. Le mode structure
`AtrRetracement`, avec amplitude 2 et période 14, reproduit le suivi ATR classique
de la référence Python.

**Setup Div Cvd → Dernier contrôle Div Cvd** est une information en lecture seule :
détection EXH/ABS, absence d'ancre, données manquantes, écart, tolérance ou ligne
traversée. Aucun texte de diagnostic n'est imposé sur le graphique.

Les réglages permissifs des divergences affichées ne désactivent pas les critères
du détecteur de setups. Les lignes validées sont également dessinées sur le prix
et le panneau CVD choisi. Aucun rectangle, entrée, stop ou objectif projeté.
Aucune notification au chargement historique ou lors d'un recalcul ; replay muet
par défaut. Une condition déjà présente au chargement est affichée mais ne
déclenche pas une nouvelle notification au premier tick. Ces alertes identifient
les conditions du détecteur, sans prédire
le résultat d'une position.


## Bulles de transactions

Agrégation indépendante sur les transactions reçues par le SDK. Identifiant
agresseur prioritaire ; sans identifiant, même sens et timestamp. Prix moyen
pondéré arrondi au tick. Les volumes de bougies ne fabriquent jamais des ordres.
Les bulles ne dépendent pas de l'ajout de l'indicateur premium.

Dans **03 Bulles**, la case **Activer les bulles** permet de masquer les bulles de
transactions et leurs chiffres. La structure et les alertes de setups restent
actives selon leurs propres réglages. Réactiver la case restitue les groupes
déjà mémorisés dans le cache de l'instance.

Dans **Bulles → Rendu des bulles**, la v23 reprend les réglages des captures :
écart-type **2,5**, taille **0,20–25**, opacité **20–30 %**, texte blanc **12**,
cercles pleins et aucune zone. Les couleurs sont reprises des sélecteurs :
achat `(0, 102, 22)`, vente `(77, 15, 148)`.

Un ordre en formation apparaît dès qu'il franchit le volume minimum, puis une
seule bulle et un seul chiffre sont mis à jour. Les petites bulles demandent
davantage de zoom pour afficher leur volume (`MaxBarsViewed`). Les cercles sont
derrière les bougies, les chiffres sont centrés au-dessus. Limite initiale :
500 bulles. Les zones restent désactivées.

La taille est calibrée sur la distribution des volumes d'ordres reçus : moyenne
plus 2,5 écarts-types, dans une fenêtre de 2 000 groupes. Les snapshots d'un
même ordre remplacent leur valeur précédente. Avant 20 groupes, une borne de
300 contrats stabilise le démarrage. Rayon de l'annotation = taille × tick / 2.
Ce calcul indépendant reprend les paramètres visuels fournis ; la formule
interne du rendu propriétaire n'est pas connue et la parité exacte doit encore
être comparée au même zoom.

**Source des transactions** permet d'utiliser les agrégats SDK lorsque le flux
les fournit, ou les sweeps reconstitués. Sans données d'agrégation natives, les
groupes reconstitués ne garantissent pas un découpage d'ordres identique.

Un **cache en mémoire de 20 000 groupes** conserve les transactions déjà reçues,
y compris celles sous le filtre. Lors d'un changement de paramètres, Deepcharts
vide les dessins mais le bundle redessine les groupes mémorisés avec le nouveau
seuil : ils ne dépendent plus d'un rejeu de l'historique. Les snapshots SDK et le
rejeu d'ordres identifiés mettent à jour la même bulle au lieu de la dupliquer.

Ce cache appartient à l'instance de l'indicateur : retirer l'indicateur, remplacer
la DLL ou redémarrer l'application le perd. Un changement d'instrument, résolution,
mode replay, source ou fenêtre d'agrégation le réinitialise. Il ne récupère pas les
transactions jamais fournies par le flux. Les groupes anciens dépassant la capacité
sont évincés. Le bundle utilise `OnTick(HistRT)` et se calcule donc sur le desktop.

## Compiler et installer

### Installation depuis le ZIP GitHub

1. **Code → Download ZIP**, puis extraire tout le dossier.
2. Double-cliquer sur **Installer.bat** à la racine.
3. Saisir son propre developer ID si aucune configuration locale n'existe.
4. Redémarrer Deepcharts et ajouter **Personal → Swing & Divergence Identifier**.

L'installateur détecte le dossier du SDK Deepcharts, vérifie PowerShell 7 et le
SDK .NET 10, puis compile et copie la DLL personnelle. Il sauvegarde la version
installée avant remplacement. Les bibliothèques du SDK ne sont pas téléchargées :
elles doivent venir de l'installation Deepcharts.

Si un prérequis Microsoft manque, son installation via WinGet est proposée et
demande une réponse explicite. Windows peut demander une élévation pour installer
ces prérequis. Sans WinGet, l'installateur indique quoi installer manuellement.
Le developer ID est enregistré dans **Deepchart.local.props**, ignoré par Git,
et n'est pas affiché dans les sorties de vérification. Aucune règle Defender
ou politique globale Windows n'est modifiée.

Vérification seule : `Installer.bat -CheckOnly -NonInteractive`. Pour une
installation automatisée, le script accepte `-DeveloperId`,
`-DeepchartDirectory` et `-NonInteractive`. Le choix `-InstallPrerequisites`
autorise explicitement l'installation des paquets manquants sans question.
`-DestinationDirectory` permet une installation isolée, notamment pour les tests.

.NET 10 et références `VolSysAPI.dll` / `VolumetricaCore.dll` de l'installation
Deepcharts. Le developer ID est configuré dans `Deepchart.local.props`, exclu de Git.

```powershell
pwsh -File .\scripts\Build-Indicator.ps1 -Install
```

DLL installée dans `Documents\Deepchart\Indicators\SwingDivergenceIdentifier.dll`.
Après une mise à jour, recharger l'instance **Personal → Swing & Divergence Identifier**.
Si l'application conserve l'ancienne DLL, redémarrer Deepcharts.

## Vérification v12

**51 vérifications du moteur et 41 de l'adaptateur SDK passent.** Tests : cas de
pauses/mèches, règle 2/3 bougies, extrêmes et aperçu causal, structure identique
avec/sans CVD, ligne du panneau CVD, recalcul sans transactions rejouées, abaissement
et relèvement du seuil, cache borné, snapshots, sons et absence de projections.
Parité des modes classiques sur 600 bougies Python : ATR/RSI/MACD, 117 swings
ATR et 121 pivots filtrés. Les tests SDK simulent l'hôte, ils ne valident ni le
rendu natif ni l'audio physique. La politique de sécurité Windows reste inchangée.

```powershell
dotnet run --project .\tests\StructureChecks.csproj -c Release
dotnet run --project .\tests\AdapterChecks\AdapterChecks.csproj -c Release
```

## Références

- [API Deepcharts](https://docs-indicators.deepcharts.com/introduction)
- [Annotations](https://docs-indicators.deepcharts.com/concepts/annotations)
- [Transactions](https://docs-indicators.deepcharts.com/concepts/market-data)
- [Alertes](https://docs-indicators.deepcharts.com/concepts/alerts)

## Compatibilité de chargement v11

La v10 introduisait des itérateurs `yield` et un indexeur personnalisé. Leur code
compilé référençait `System.Environment.get_CurrentManagedThreadId`, des collections
non génériques et `System.Reflection.DefaultMemberAttribute`, hors des usages
ordinaires documentés pour les indicateurs. Ces constructions ont été remplacées
par des listes génériques et un snapshot réutilisable. Les alertes intrabougie
conservent le même comportement ; la copie des bougies closes est reconstruite
uniquement quand leur nombre change, puis seul le snapshot ouvert est actualisé.

`Build-Indicator.ps1` lance désormais `Test-IndicatorCompatibility.ps1` avant la
copie. Ce contrôle lit uniquement les métadonnées, repère les références sensibles
et détecte la régression v10 ; il ne remplace pas le validateur complet Deepcharts.
Il ne charge ni n'exécute la DLL. Les 51 tests du moteur et 41 de l'adaptateur
passent avec la v12. Le retour effectif de la catégorie Personal doit être vérifié
dans l'application.

Windows Code Integrity a également bloqué des outils de développement locaux.
Aucune protection Windows, règle de contrôle d'application ou exclusion antivirus
n'a été modifiée. Les diagnostics de chargement Deepcharts et les blocages Windows
sont distincts ; une DLL refusée par Windows nécessite une signature autorisée par
la politique applicable, plutôt qu'une modification de cette politique.
