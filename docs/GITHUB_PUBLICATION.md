# PrÃ©parer la publication GitHub

Nom suggÃ©rÃ© du dÃ©pÃ´t : **swing-divergence-identifier**. Licence : **MIT**.
Le projet prÃ©pare les sources ; aucune publication distante n'est faite par
les scripts de build ou d'export.

## VÃ©rifier le contenu

Les fichiers destinÃ©s au dÃ©pÃ´t sont les sources, tests synthÃ©tiques, documentation,
licence, scripts et workflow. `Deepchart.local.props`, les DLL, logs, builds,
archives et outils locaux sont ignorÃ©s. L'historique du projet privÃ© de rÃ©fÃ©rence
n'est pas importÃ©.

```powershell
git status --short
pwsh -File scripts/Test-PublicSource.ps1
dotnet run --project tests/StructureChecks.csproj -c Release
```

La vÃ©rification de sources inspecte **l'index Git**, donc ce qui entrera dans le
commit. AprÃ¨s toute nouvelle modification, indexer uniquement les fichiers voulus
et relancer le contrÃ´le. Il repÃ¨re le developer ID local, les chemins de compte,
les fichiers privÃ©s et quelques formats de secrets courants.

## CrÃ©er le dÃ©pÃ´t distant

Sur GitHub, crÃ©er un dÃ©pÃ´t public vide nommÃ© `swing-divergence-identifier`, sans README
ou licence supplÃ©mentaires. Depuis la racine du projet, aprÃ¨s revue du contenu :

```powershell
git commit -m "Prepare Swing & Divergence Identifier open-source release"
git remote add origin https://github.com/Zyksa/SwingDivergenceIdentifier.git
git push -u origin main
```

Ces commandes décrivent la publication et les mises à jour du dépôt ;
elles ne sont pas exÃ©cutÃ©es pendant la prÃ©paration. La CI est prÃªte Ã  vÃ©rifier
les moteurs avec .NET 10 sur Ubuntu, sans SDK propriÃ©taire ni developer ID.
La suite d'adaptateur est exÃ©cutÃ©e localement avec le SDK installÃ©.

## Source release

```powershell
pwsh -File scripts/Export-Source.ps1
```

L'archive provient de l'index Git relu, sans nÃ©cessiter de commit initial. Elle
contient un dossier `swing-divergence-identifier/`. Joindre cette archive Ã  une release
GitHub si souhaitÃ©, ou utiliser les archives de sources que GitHub crÃ©e depuis
un tag. Garder la licence et les mentions de dÃ©pendances dans la distribution.

Ne pas joindre une DLL compilÃ©e pour son propre compte : elle contient le developer
ID et n'est pas utilisable par les autres comptes. Pour l'usage local, chaque
utilisateur compile depuis les sources avec son propre ID. Le mode `PublicBuild`
produit une DLL sans cet ID, destinÃ©e Ã  l'Ã©quipe de publication Deepcharts si
elle la demande ; il ne rend pas la DLL chargeable dans Personal pour tous.
