# Migration to Swing & Divergence Identifier

Version 1.0.16 renames the project identity:

| Item | New name |
| --- | --- |
| Displayed indicator/package | Swing & Divergence Identifier |
| C# project | SwingDivergenceIdentifier.csproj |
| Assembly | SwingDivergenceIdentifier.dll |
| Namespace | SwingDivergence |
| Indicator class | SwingDivergenceIdentifier |
| GitHub repository/source archive root | swing-divergence-identifier |

Add the renamed indicator again to charts using the earlier assembly/class.
Local installation backs up the previous DLL before removing it. Workspace
references and saved chart instances are not automatically rewritten.

The checkout folder can be named `SwingDivergenceIdentifier`. If Windows says
that the folder is in use, close the editors, terminals and applications using
it before renaming it from its parent directory. The code and scripts use paths
relative to the checkout root, so changing its folder name needs no source edits.
