# LTspice Compatibility

Use this reference before importing, validating, or designing from LTspice-originated netlists. Compatibility is opt-in, parser-first, and evidence-scoped. Before claiming parity, read `roadmap/ltspice-compatibility-matrix.md` and the matching P0/P1/P2/P3 tests.

## Contents

- Required settings
- Supported compatibility shims
- Source and passive topology synthesis
- Custom ideal diodes
- Nonlinear Q/Flux passives
- Unsupported and engine-required gaps
- Evidence references

## Required Settings

Set LTspice compatibility on both parser and reader. Set the working directory when `.INCLUDE`, `.LIB`, or `PWL file=<path>` may resolve relative files.

```csharp
var parser = new SpiceNetlistParser();
parser.Settings.Lexing.HasTitle = true;
parser.Settings.Parsing.IsEndRequired = true;
parser.Settings.WorkingDirectory = workingDirectory;
parser.Settings.Compatibility = CompatibilityOptions.LTspice;
var parseResult = parser.ParseNetlist(netlist);

var reader = new SpiceSharpReader();
reader.Settings.Compatibility = CompatibilityOptions.LTspice;
var model = reader.Read(parseResult.FinalModel);
```

Enable custom components only when the netlist needs LTspice-style ideal diode models or nonlinear `Q=`/`Flux=` passives:

```csharp
using SpiceSharpParser.CustomComponents;

reader.Settings.Compatibility = CompatibilityOptions.LTspice;
reader.Settings.UseCustomComponents();
```

Without `UseCustomComponents()`, core LTspice mode reports targeted diagnostics for ideal-diode parameters and `Q=`/`Flux=` syntax.

## Supported Compatibility Shims

- `.backanno`: warning no-op in LTspice mode.
- LTspice output/viewer `.OPTIONS`: warning no-op for options such as `plotwinsize`, `numdgt`, and `fastaccess`.
- One-argument `.TRAN`: LTspice mode derives step and maxstep from stop time.
- `.TRAN <Tstop> UIC`: supported with derived step and `UseIc`.
- Scalar aliases and expression shims: `arccos`, `arcsin`, `arctan`, `fabs`, `sgn`, `round`, `pwr`, `pwrs`, `hypot`, `table(...)`, `tbl(...)`, `**`, boolean `!`, `&`, and `|`.
- `EXP(...)` waveform.
- Finite-cycle `PULSE(... Ncycles)` and finite-cycle `SINE(... Ncycles)`.
- Local two-column `PWL file=<path>` with optional header row and comment/blank-line handling.
- Simple non-nested `PWL REPEAT FOR <n>` and `REPEAT FOREVER` blocks, including relative `+time` values accumulated from the preceding point.
- Wave-file sources with omitted `chan=<n>` defaulting to channel 0 in LTspice mode.
- Independent source `tbl=(expr,x1,y1,...)`, lowered to behavioral `table(...)`.
- R/C model `tc=a[,b]`, lowered to `tc1`/`tc2`.
- Switch aliases `von`/`voff` and `ion`/`ioff`, lowered to midpoint/hysteresis forms.
- Metadata/rating parameters such as `mfg`, `manufacturer`, `pn`, `part`, `desc`, `description`, `V`, `Irms`, and `Ipk`: warning no-op in LTspice mode.

## Source and Passive Topology Synthesis

Topology-changing options that can be represented safely are synthesized as helper entities. Prefer native LTspice syntax when validating compatibility, and expect helper names with suffixes such as `_rser`, `_cpar`, `_load`, `_rpar`, `_lser`, and `_rlshunt`.

| Target | Supported options | Synthesis |
| --- | --- | --- |
| Voltage source | `Rser`, `R=<value>`, `Cpar`, `load` | Series resistor, parallel capacitor, load resistor |
| Current source | `load`, `R=<value>`, `Cpar` | Parallel load resistor, parallel capacitor |
| Resistor | `Rser`, `Rpar`, `Cpar` | Series resistor through internal node; parallel R/C |
| Capacitor | `Rser`, `Lser`, `Rpar`, `Cpar` | Series R/L helper chain; parallel R/C |
| Inductor | `Rser`, `Lser`, `Rpar`, `RLshunt`, `Cpar` | Series R/L helper chain; parallel R/R/C |
| Switch model | `Vser`, `Lser` | Series voltage-source / inductor helper chain from first switch terminal |

Repeated subcircuits scope internal helper nodes. Parameterized parasitic values are supported in the P3 fixtures.

## Custom Ideal Diodes

Use `SpiceSharpParser.CustomComponents` when a `.MODEL D(...)` contains ideal diode parameters:

```spice
.model did D(Ron=0.1 Roff=1e9 Vfwd=0.7 Ilimit=10 Epsilon=10m)
D1 out 0 did
```

Ideal-diode selectors include `Ron`, `Roff`, `Vfwd`, `Vrev`, `Rrev`, `Ilimit`, `RevIlimit`, `Epsilon`, and `RevEpsilon`.

Behavior:

- OP/DC uses LTspice-style region-wise-linear current law.
- AC uses the operating-point derivative.
- TRAN is memoryless and re-evaluated at each time point.
- Supports LTspice-style `M` parallel scaling and `N` series scaling.
- Accepts `area`, `OFF`, `L`, `W`, and `Rs`; `area` and `Rs` are ignored electrically for ideal-diode parity.
- Does not model junction capacitance, charge storage, semiconductor temperature physics, or noise.

Read `src/docs/articles/ideal-diode.md`, `src/SpiceSharpParser.Tests/CustomComponents/IdealDiodeTests.cs`, and `src/SpiceSharpParser.IntegrationTests/LTspiceCompatibility/LTspiceIdealDiodeIntegrationTests.cs`.

## Nonlinear Q/Flux Passives

Use custom components for LTspice-style charge-defined capacitors and flux-defined inductors:

```spice
C1 out 0 Q=1u*x+100n*x*x
L1 in out Flux=1m*x+100u*x*x
```

Rules:

- `C... Q=<expr>` uses `x = V(node+,node-)`.
- `L... Flux=<expr>` uses `x = I(L)` from node+ to node-.
- AC uses the operating-point slope: `dQ/dV` or `dFlux/dI`.
- TRAN uses stored quantity plus integration history.
- `IC=`, `M=`, and `N=` are supported.

Read `src/docs/articles/nonlinear-passives.md` and `src/SpiceSharpParser.Tests/CustomComponents/NonlinearPassiveTests.cs`.

## Unsupported and Engine-Required Gaps

Unsupported constructs should produce targeted diagnostics rather than silent fallbacks.

Known gaps include:

- LTspice schematic and symbol import: `.asc` / `.asy`.
- `.TF`, `.NET`, `.FERRET`, `.LOADBIAS`, `.SAVEBIAS`, `.MACHINE` / `.ENDMACHINE`.
- `.TRAN startup`, `steady`, `nodiscard`, and `step`.
- Behavior-changing solver options such as `cshunt`, `gshunt`, `srcsteps`, `gminsteps`, `trtol`, `chgtol`, `pivrel`, `pivtol`, and `ptrantau`.
- Nested/combined PWL repeat blocks, trigger restarts, time/value scale factors, and `SCOPEDATA`.
- `uplim(...)`, `dnlim(...)`, and unary `~`.
- Switch `Ilimit`.
- MOS levels outside 1, 2, and 3.
- Three-terminal LTspice MOS / power-MOS syntax, `VDMOS`, `O`/`LTRA`, and `U`/`URC`.

## Evidence References

- Compatibility matrix: `roadmap/ltspice-compatibility-matrix.md`
- Plan: `roadmap/ltspice-netlist-compatibility-plan.md`
- Tests: `src/SpiceSharpParser.IntegrationTests/LTspiceCompatibility/LTspiceCompatibilityP0Tests.cs`
- Tests: `src/SpiceSharpParser.IntegrationTests/LTspiceCompatibility/LTspiceCompatibilityP1Tests.cs`
- Tests: `src/SpiceSharpParser.IntegrationTests/LTspiceCompatibility/LTspiceCompatibilityP2Tests.cs`
- Tests: `src/SpiceSharpParser.IntegrationTests/LTspiceCompatibility/LTspiceCompatibilityP3Tests.cs`
- Ideal diode integration: `src/SpiceSharpParser.IntegrationTests/LTspiceCompatibility/LTspiceIdealDiodeIntegrationTests.cs`
