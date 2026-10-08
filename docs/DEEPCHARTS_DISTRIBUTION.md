# Publier Swing & Divergence Identifier sur Deepcharts

Mise à jour : **8 octobre 2026**, d'après le formulaire **Add package** fourni par
l'utilisateur. Cette interface est accessible sur son compte ; cela ne prouve
pas que tous les comptes ont les mêmes droits de publication.

## Publication depuis le formulaire

Le formulaire observé accepte les **sources C#**, compilées ensemble comme un
package. Il ne demande ni le projet `.csproj` ni une DLL locale. Il indique que
le calcul est actuellement disponible **côté client uniquement**.

1. **Name** : `Swing & Divergence Identifier`.
2. **Description** :

   ```text
   Swing & Divergence Identifier maps market structure points with HH, HL, LH and LL labels, detects regular and hidden CVD divergences, and displays transparent buy/sell execution bubbles. Follow alternating swing highs and lows, receive configurable visual and sound alerts as divergences form, and filter bubbles by minimum volume. Volume labels adapt to chart zoom, while optional RSI/MACD analysis and live structure previews help you customize the display.
   ```

3. **Help link** : l'URL du README GitHub, une fois le dépôt publié.
4. **Visibility** :
   - **Private** : uniquement le propriétaire du package.
   - **Public** : les utilisateurs qui ont rejoint le lien développeur.
5. Préparer le dossier à envoyer depuis la racine du projet :

   ```powershell
   pwsh -File scripts/Export-DeepchartsPackage.ps1
   ```

   Ouvrir **`dist/swing-divergence-identifier-deepcharts/`**. Dans **Source files → Parcourir**, aller
   dans ce dossier, sélectionner **les 30 fichiers `.cs` ensemble** (`Ctrl+A`),
   puis ouvrir. **`GlobalUsings.cs` doit faire partie de la sélection** : les
   imports du projet local ne sont pas automatiquement transmis avec les sources.
6. Le dossier contient uniquement les sources de production, à plat, environ
   **191 Ko**. Il est sous la limite de **50 fichiers et 2 048 KB** du formulaire.
   Aucun `.csproj`, fichier SDK, test, diagnostic ou archive ZIP ne doit être
   sélectionné dans ce champ.
7. Avant de confirmer, vérifier le nom, la visibilité et la liste des fichiers.
   Le clic sur **Confirm** publie/enregistre selon les règles de la plateforme ;
   aucune action sur ce formulaire n'est effectuée par les scripts du dépôt.
8. Vérifier ensuite le chargement du package et son affichage dans les indicateurs
   partagés, puis tester structure, lignes CVD, notifications, sons et bulles.

La classe enregistrée est `SwingDivergence.SwingDivergenceIdentifier`. Une archive de sources
GitHub contient aussi la documentation et les tests : elle ne remplace pas la
sélection des seuls fichiers `.cs` de production dans ce formulaire.

## Pourquoi un package desktop ?

Le bundle utilise `OnTick(HistRT)` pour les bulles et propose les agrégats SDK
réservés au desktop. Désactiver les bulles dans les paramètres ne retire pas ces
usages du code. Une version serveur demanderait un indicateur distinct sans ces
membres ni les transactions historiques.
[Documentation officielle](https://docs-indicators.deepcharts.com/hybrid/desktop-only)

## Différence avec l'installation locale

Une DLL personnelle est compilée avec le developer ID du compte et placée dans
`Documents\Deepchart\Indicators`. Le formulaire **Add package** montré ici prend
les sources ; il s'agit d'une autre voie de distribution. Ne pas y envoyer la
DLL compilée pour son propre compte.

[Developer ID](https://docs-indicators.deepcharts.com/hybrid/developer-id) ·
[Packages partagés](https://docs-indicators.deepcharts.com/hybrid/sharing)

La documentation publique indique encore que la publication n'est pas ouverte à
tous les développeurs. Le formulaire fourni apporte une information plus précise
pour ce compte. En cas d'erreur d'accès ou de compilation, transmettre au support
le message exact et la version de Deepcharts.

## Informations du projet

- **Nom** : Swing & Divergence Identifier.
- **Sources** : licence MIT ; SDK Deepcharts non redistribué.
- **Version affichée** : Beta v0.6, by Zyksa ; build technique 1.0.25.
- **Calcul** : client/desktop, C#/.NET 10.
- **Données** : delta Ask/Bid et transactions disponibles dans le flux utilisateur.
- **Validation locale** : les 90 tests moteur passent, dont la parité des setups
  Python, les catégories CVD et les snapshots de bulles. Les 51 tests d'adaptateur compilent ;
  leur exécution n'a pas été relancée après le blocage du contrôle d'application
  Windows. Le dossier exporté a été compilé seul avec le SDK local.
  Cela vérifie les sources sans confirmer la compilation sur la plateforme ni
  le rendu natif/audio.

[Brouillon de demande au support](DEEPCHARTS_SUBMISSION_DRAFT.md)
