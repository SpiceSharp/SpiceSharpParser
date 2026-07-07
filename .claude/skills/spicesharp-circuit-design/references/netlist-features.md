# Netlist Features and Helper APIs

Use this reference before using advanced parser syntax, dot statements, LAPLACE, .FOUR, or built-in analysis helpers. For exact behavior, read `src/docs/index.md`, the specific article under `src/docs/articles/`, and matching tests under `src/SpiceSharpParser.IntegrationTests/`.

## Contents

- Feature surface
- Netlist-native controls
- LAPLACE transfer sources
- .FOUR Fourier and THD
- CircuitBuilder and analysis helpers

## Feature Surface

| Category | Supported features | Primary local references |
| --- | --- | --- |
| Analyses | `.OP`, `.DC`, `.AC`, `.TRAN`, `.NOISE` | `op.md`, `dc.md`, `ac.md`, `tran.md`, `noise.md` |
| Output/post-processing | `.SAVE`, `.PRINT`, `.PLOT`, `.MEAS`/`.MEASURE`, `.FOUR`, `.WAVE` | `save.md`, `print.md`, `plot.md`, `meas.md`, `four.md` |
| Parameters/expressions | `.PARAM`, `.FUNC`, `.LET`, `.SPARAM`, nested exports such as `db(V(out))`, `mag(I(R1))` | `param.md`, `func.md`, `let.md`, `sparam.md`, `meas.md` |
| Sweeps/control/statistics | `.STEP`, `.ST`, `.TEMP`, `.MC`, `.DISTRIBUTION`, `.OPTIONS`, `.IC`, `.NODESET`, `.IF` | `step.md`, `st.md`, `temp.md`, `mc.md`, `distribution.md`, `options.md`, `ic.md`, `nodeset.md`, `if.md` |
| Structure/models | `.SUBCKT`, `X...`, `.INCLUDE`, `.LIB`, `.GLOBAL`, `.CONNECT`, `.APPENDMODEL`, `.MODEL` | `subckt.md`, `subcircuit-instance.md`, `include.md`, `lib.md`, `global.md`, `appendmodel.md` |
| Sources/devices | R, C, L, K, D, Q, J, M, V, I, B, E/F/G/H, S/W, T, BVDelay, PULSE/SIN/PWL/EXP/SFFM/AM/WAVE/wavefile, VALUE/TABLE/POLY/LAPLACE | matching component articles and tests |

## Netlist-Native Controls

- Use `.MEAS` for scalar acceptance criteria: `TRIG/TARG`, `WHEN`, `FIND`, `MAX`, `MIN`, `AVG`, `RMS`, `PP`, `INTEG`, `DERIV`, and `PARAM`.
- Use `.FOUR` for settled-period harmonic magnitude, phase, normalized dB, and THD from transient waveforms.
- Use `.PRINT` for tabular exported data and `.PLOT` for structured XY plot data when reports need curves.
- Use `.WAVE` only when a transient waveform must be exported to a WAV file artifact.
- Use `.STEP`, `.TEMP`, `.MC`, and `.DISTRIBUTION` for sweeps, corners, and statistical robustness.
- Use `.LET` for reusable dynamic expressions such as power or gain, `.FUNC` for reusable equations, and `.SPARAM` only when immediate scalar evaluation is required before setup/sweeps.

When `CircuitBuilder` has no dedicated helper, use `RawLine()` with the exact statement, for example:

```csharp
builder.RawLine(".FOUR 1k V(OUT)");
builder.RawLine(".STEP PARAM r LIST 1k 2k 5k");
builder.RawLine(".PLOT TRAN V(OUT) merge");
```

## LAPLACE Transfer Sources

Use `LAPLACE` when a block is mostly linear and the desired transfer function is known: anti-alias poles, finite-bandwidth amplifier approximations, lead/lag compensation, simplified transconductance stages, and sensor front ends.

Supported source-level forms:

```spice
E1 out 0 LAPLACE {V(in)} = {1/(1+s*tau)}
G1 out 0 LAPLACE {V(in)} {gm/(1+s*tau)}
F1 out 0 LAPLACE {I(Vsense)} = {1/(1+s*tau)}
H1 out 0 LAPLACE = {I(Vsense)} {1/(1+s*tau)}
```

Supported function-style forms:

```spice
BLOW out 0 V={LAPLACE(V(in), wc/(s+wc))}
BDELAY out 0 V={LAPLACE(V(in), 1/(1+s*tau), M=2, TD=1n)}
```

Rules:

- `E` and `G` use voltage probes; `F` and `H` use current probes.
- Function-style `LAPLACE(...)` accepts direct probes and arbitrary scalar input expressions; non-probe inputs are lowered through internal helper sources.
- The transfer must be a finite, proper rational polynomial in `s` with non-singular DC gain.
- Use `s/(s+wc)` for high-pass behavior; bare `s` is improper.
- Avoid unsupported forms such as `1/s`, `sin(s)`, source-level `V(a)-V(b)`, and `V(node)` on `F`/`H`.
- `M=` is a finite multiplier folded into the numerator.
- `TD=` and `DELAY=` are aliases; use only one with assignment syntax and a non-negative value.
- `G` and `F` current is defined from `out+` to `out-`; a grounded load may invert output voltage.

Read `src/docs/articles/laplace.md`, `src/docs/articles/laplace-basics.md`, and `src/SpiceSharpParser.IntegrationTests/AnalogBehavioralModeling/LaplaceTests.cs` for exact behavior.

## .FOUR Fourier and THD

Use `.FOUR` when requirements include THD, harmonic content, distortion cleanup, harmonic phase, square-wave harmonic ratios, or before/after filter harmonic comparison.

```spice
V1 OUT 0 SIN(0 1 1k)
R1 OUT 0 1k
.TRAN 1u 10m 0 2u
.FOUR 1k V(OUT) I(V1)
```

Rules:

- `.FOUR` requires `.TRAN`.
- It analyzes the last complete period; run long enough for startup to settle.
- Choose the true repetition frequency.
- Use a small max step; aim for 20+ samples per highest harmonic period, and 50-100 samples for good magnitudes.
- Results are stored in `model.FourierAnalyses`, one result per signal per transient simulation or `.STEP` point.
- Check `Success` and `ErrorMessage` before reading results.
- Harmonics cover DC and harmonics 1-9; THD is computed from harmonics 2-9.

Read `src/docs/articles/four.md` and `src/SpiceSharpParser.IntegrationTests/DotStatements/FourTests.cs`.

## Analysis Helper APIs

Use these helpers before writing custom code:

| Helper | Location | Use |
| --- | --- | --- |
| `StandardValues` | `src/SpiceSharpParser/Utilities/StandardValues.cs` | E12/E24/E96 snapping and ranges |
| `CircuitBuilder` | `src/SpiceSharpParser/Builder/CircuitBuilder.cs` | Fluent programmatic netlist construction |
| `SmokeTester` | `src/SpiceSharpParser/Analysis/SmokeTester.cs` | Parse, lint, OP convergence, node voltages, device regions |
| `NetlistLinter` | `src/SpiceSharpParser/Validation/NetlistLinter.cs` | Floating nodes, missing DC paths, missing models, duplicates |
| `CircuitInspector` | `src/SpiceSharpParser/Analysis/CircuitInspector.cs` | Topology, component values, BJT/MOSFET regions |
| `WaveformAnalyzer` | `src/SpiceSharpParser/Analysis/WaveformAnalyzer.cs` | Rise/fall/settling, overshoot, RMS, FFT, THD, SNR, bandwidth, margins |
| `SensitivityAnalyzer` | `src/SpiceSharpParser/Analysis/SensitivityAnalyzer.cs` | Rank component impact on `.MEAS` results |
| `DesignSpaceExplorer` | `src/SpiceSharpParser/Analysis/DesignSpaceExplorer.cs` | Grid-search optimization with objectives and constraints |

Recommended pipeline:

1. Use `StandardValues.NearestE24()` after calculating values.
2. Use `CircuitBuilder` for complex or parameterized netlists.
3. Use `SmokeTester.QuickCheck()` and `NetlistLinter.Lint()` before deeper tests.
4. Use netlist `.MEAS`/`.FOUR` first, then `WaveformAnalyzer` for metrics with no netlist-native equivalent.
5. Use `SensitivityAnalyzer` before changing component values.
6. Use `DesignSpaceExplorer` for multi-parameter optimization.

Concise example:

```csharp
var netlist = CircuitBuilder.Create("Low-Pass Filter")
    .VoltageSource("V1", "in", "0", dc: 0, ac: 1)
    .Resistor("R1", "in", "out", StandardValues.NearestE24(1590))
    .Capacitor("C1", "out", "0", StandardValues.NearestE24(100e-9))
    .AC("DEC", 100, 1, 1e6)
    .Meas("AC", "f3db", "WHEN VDB(out)=-3")
    .Save("VDB(out)", "VP(out)")
    .ToNetlist();

var smoke = SmokeTester.QuickCheck(netlist);
Assert.True(smoke.IsPass, smoke.DiagnosticSummary());
```
