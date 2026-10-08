# Draft — publisher access request

This is a draft only. No message has been sent.

**Subject:** Publisher access request — Swing & Divergence Identifier (open-source, desktop)

Hello Deepcharts team,

I would like to distribute my indicator **Swing & Divergence Identifier** as a shared desktop
package. Its sources are prepared under the MIT license. My intended distribution
is free and open source.

The indicator draws directional HH/HL/LH/LL structure, detects price/CVD divergences
within 12 bars, provides configurable early notifications and sounds for structural setups, and plots
translucent execution bubbles filtered by minimum volume. RSI/MACD and secondary
structure are optional. It does not submit orders or project entry/stop/target zones.

Technical details: C#/.NET 10, `VolSysAPI` and `VolumetricaCore`, desktop calculation
because it uses historical trade callbacks and optional SDK aggregates. Current
assembly/class identity: `SwingDivergenceIdentifier.dll` / `SwingDivergence.SwingDivergenceIdentifier`.
Display version: Beta v0.6, by Zyksa. Build: 1.0.25. Local validation: 90 engine checks pass, including Python setup parity, separate CVD categories and growing trade snapshots. The suite of 51 SDK-adapter checks compiles; adapter execution has not been retried after Windows application control blocked it.
native rendering and audio still need platform review.

Could you confirm publisher access availability and explain:

- The submission format, upload process and required publisher metadata.
- Whether a free MIT-licensed source project can be distributed through Shared
  and the marketplace, and under which terms.
- Whether you need source, an ID-free DLL, or a build performed by your team.
- Signing requirements and how users receive package updates.

GitHub repository: https://github.com/Zyksa/SwingDivergenceIdentifier

I can provide the source archive, documentation and screenshots. The proprietary
SDK and my personal developer ID are excluded from the public source distribution.

Thank you.
