---
title: LTspice Netlist Compatibility Plan
status: Draft / Roadmap
scope: SpiceSharpParser + SpiceSharp
last_reviewed: 2026-05-03
---

# LTspice Netlist Compatibility Plan

## Summary

The first LTspice compatibility target is generated netlists: copied netlists, `.net` files, and model decks reached through `.include` / `.lib`. Schematic and symbol import for `.asc` / `.asy` files is out of scope.

The roadmap is intentionally parser-first:

1. Measure current behavior with redistributable fixtures and a compatibility matrix.
2. Add opt-in LTspice compatibility options, harmless no-op handling, and targeted diagnostics.
3. Add scalar-expression, ABM, and model-parameter tolerance only when the behavior can be represented safely.
4. Escalate to SpiceSharp runtime work only after fixtures prove parser lowering is not enough.
5. Keep README/docs support claims tied to tests and matrix rows.

## Current Repo Facts

- Arbitrary `B` voltage/current sources and `VALUE={expr}` controlled-source forms are already supported.
- Source-level `E` / `G` / `F` / `H` `LAPLACE` forms are already supported, including alternate spellings, finite constant `M=`, `TD=`, and `DELAY=` options.
- Function-style `LAPLACE(input, transfer)` is already supported inside `VALUE`, `B ... V=`, and `B ... I=` expressions, including helper lowering for mixed expressions and arbitrary scalar inputs.
- Generated C# writer parity already exists for the implemented Laplace lowering paths.
- `LAPLACE` should remain a source/lowering feature, not a normal scalar expression function.
- No explicit LTspice compatibility mode exists yet in parser or reader settings.
- Validation currently has error and warning levels only. The roadmap should use warnings for recognized LTspice no-ops until an informational level exists.
- `ValidationEntryCollection.Warnings` appears to filter `ValidationEntryLevel.Error` instead of `ValidationEntryLevel.Warning`; fix this before adding warning-based no-op behavior.
- `MathFunctions.CreateTable()` still returns `null`, making scalar `table(...)` an early expression-compatibility candidate.
- The default object mappings already register common controls such as `.param`, `.func`, `.global`, `.options`, `.temp`, `.step`, `.mc`, `.tran`, `.ac`, `.dc`, `.op`, `.noise`, `.save`, `.plot`, `.print`, `.meas`, `.measure`, `.ic`, `.nodeset`, and `.wave`, but this does not mean LTspice syntax parity is proven for every variant.
- No registered controls currently cover LTspice `.backanno`, `.tf`, `.four`, `.net`, `.ferret`, `.loadbias`, `.savebias`, or `.machine` / `.endmachine` blocks.
- `.TRAN` currently accepts traditional numeric forms and trailing `UIC`; LTspice one-argument form and modifiers such as `startup`, `steady`, `nodiscard`, and `step` still need explicit compatibility decisions.
- Source waveform mappings cover `SIN` / `SINE`, `PULSE`, `PWL`, `AM`, `SFFM`, and wave-file input, but gaps remain for `EXP(...)`, LTspice cycle-count arguments, optional wave channel defaults, and several independent-source instance options.
- MOS model generation currently covers legacy levels 1, 2, and 3. LTspice `VDMOS` and advanced monolithic levels such as BSIM/EKV/HiSIM variants are runtime or intentional-unsupported candidates.
- Distributed-line support currently starts from lossless `T`. LTspice lossy `O` / `LTRA` and uniform RC-line `URC` models need engine triage before runnable support is claimed.

## Scope

Goals:

- Make common LTspice-generated netlists parse, read, and run when their features can be represented safely by SpiceSharp and SpiceSharpBehavioral.
- Prefer parser compatibility shims and explicit diagnostics before adding runtime behavior.
- Build a small permission-safe fixture corpus; do not commit copied vendor libraries unless their license clearly permits redistribution.
- Preserve existing PSpice/ngspice-compatible behavior by default.
- Record known divergences instead of making blanket numeric-parity claims.

Non-goals:

- Importing, rendering, or simulating LTspice schematic files (`.asc`) or symbols (`.asy`).
- Depending on LTspice in CI.
- Claiming complete LTspice numeric parity across solver settings, device models, and vendor libraries.
- Duplicating SpiceSharpBehavioral runtime features in SpiceSharpParser.

## Compatibility Decisions

- Add opt-in compatibility settings shaped as a `CompatibilityOptions` object with an LTspice preset. Prefer this over a single hard-coded dialect enum so future dialect quirks can coexist.
- Add compatibility options to both parser and reader settings where behavior is split across lexing/parsing/preprocessing and SpiceSharp model generation.
- Keep default behavior stable unless a change is dialect-neutral and covered by existing tests.
- Treat recognized LTspice display/probing/annotation no-ops as warnings for now. Do not silently discard behavior-changing statements.
- Use targeted reader/parser errors for recognized unsupported LTspice constructs.
- Track parser, SpiceSharp, and SpiceSharpBehavioral minimum package requirements in the compatibility matrix before adding a versioned diagnostic framework.
- Every public compatibility claim must point to a fixture or matrix row.

## Baseline To Preserve

These examples are baseline behavior, not future work:

```spice
B1 out 0 V={V(in)*0.5}
B2 out 0 I={V(ctrl)*1m}

ELOW out 0 LAPLACE {V(in)} = {1/(1+s*tau)} M=2 TD=1n
GLOW out 0 LAPLACE {V(in)} {gm/(1+s*tau)}
FLOW out 0 LAPLACE = {I(Vsense)} {1/(1+s*tau)}
HLOW out 0 LAPLACE {I(Vsense)} = {1/(1+s*tau)}

E1 out 0 VALUE={LAPLACE(V(in), 1/(1+s*tau))}
B3 out 0 V={1 + 2*LAPLACE(V(in), 1/(1+s))}
B4 out 0 I={LAPLACE(V(a), 1/(1+s*t1)) - LAPLACE(V(b), 1/(1+s*t2))}
B5 out 0 V={LAPLACE(2*V(in), 1/(1+s*tau), M=2, TD=1n)}
```

The supported `LAPLACE` transfer subset remains a finite, proper rational polynomial in `s` with finite coefficients and non-singular DC gain.

## LTspice Feature Inventory

This inventory is a planning aid. It intentionally separates parser acceptance, SpiceSharp object generation, runtime execution, and numeric confidence. Items in the first table are not new claims unless a fixture or test already proves the LTspice spelling.

### Existing Baseline That Needs Matrix Coverage

| Area | LTspice patterns to cover | Current posture | Roadmap action |
| --- | --- | --- | --- |
| Behavioral sources | `B... V=`, `B... I=`, `VALUE={...}` on `E` / `G` / `F` / `H` | Baseline support exists | Preserve behavior and add LTspice-specific fixtures |
| Laplace sources | Source-level `LAPLACE`, function-style `LAPLACE(input, transfer)`, `M=`, `TD=`, `DELAY=` | Baseline support exists | Keep on source/lowering path, not a scalar function |
| Legacy ABM forms | `TABLE`, `POLY`, controlled-source value expressions | Parser lowering exists for source forms | Add fixtures that prove read/run behavior and writer parity |
| Parameters and functions | `.param`, `.func`, subcircuit defaults | Generic support exists | Add LTspice scoping and expression fixtures |
| Analyses | `.op`, `.dc`, `.ac`, `.tran`, `.noise` | Registered controls exist | Classify LTspice syntax variants independently |
| Outputs and measurements | `.save`, `.plot`, `.print`, `.wave`, `.meas`, `.measure` | Registered controls exist | Audit LTspice-specific syntax before claiming parity |
| Initial conditions | `.ic`, `.nodeset`, `.temp`, `.options temp`, `.options tnom` | Generic support exists | Verify LTspice generated-netlist forms |
| Includes and libraries | `.include`, `.lib`, nested files and library sections | Preprocessors exist | Tighten path, section, quoting, and Windows separator fixtures |

### Incomplete High-Yield Parser And Reader Work

| Area | LTspice feature | Expected compatibility class | Notes |
| --- | --- | --- | --- |
| Generated metadata | `.backanno` | Recognized no-op | Automatically emitted by LTspice generated netlists; warning-only in LTspice mode. |
| Options | `baudrate`, `delay`, `fastaccess`, `meascplxfmt`, `measdgt`, `numdgt`, `plotwinsize`, `plotreltol`, `plotvntol`, `plotabstol`, convergence and pseudo-transient knobs | Parser shim / recognized no-op / diagnostic | Split options into mapped solver settings, output/viewer no-ops, and behavior-changing unsupported knobs. |
| Transient analysis | `.tran <Tstop>`, `UIC`, `startup`, `steady`, `nodiscard`, `step`, max-step handling | Parser shim / diagnostic | Current reader handles traditional numeric forms plus trailing `UIC` only. |
| Measurements | `FIND`, `WHEN`, `AT`, `DERIV`, `PARAM`, `TRIG` / `TARG`, `AVG`, `MAX`, `MIN`, `PP`, `RMS`, `INTEG`, stepped output, AC complex comparisons | Supported / diagnostic by case | Existing measurement code needs a syntax-by-syntax fixture audit. |
| Save/output selection | `.save V(*)`, `Id(*)`, terminal-current forms such as `Ic(Q1)`, `dialogbox` | Parser shim / no-op / diagnostic | Wildcards and GUI selection should not be treated like guaranteed exports until tested. |
| Wave output | `.wave <file> <bits> <sampleRate> V(...) ...` | Parser shim / incomplete | Existing implementation appears limited to one or two channels; LTspice allows many channels. |
| Source waveforms | `EXP(...)`, `PULSE(... Ncycles)`, `SINE(... Phi Ncycles)`, PWL file variants | Parser shim / engine if needed | Add or diagnose before claiming LTspice source-waveform parity. |
| Source instance options | Voltage-source `Rser`, `Cpar`; current-source `load`, `R=<value>`, `tbl=(...)`; `wavefile=<path> [chan=<n>]` | Parser shim / diagnostic | Some can lower to existing components; behavior-changing cases need explicit handling. |
| Scalar expressions | `table(...)`, `pwr`, `pwrs`, `hypot`, `sgn`, `round`, `fabs`, `arccos`, `arcsin`, `arctan` | Parser/runtime shim | Static functions are high-value because vendor decks use them in `.param` and ABM expressions. |
| Random functions | `flat`, `mc`, `rand`, `random`, `gauss`, `white` | Supported with divergence notes / diagnostic | Existing random behavior must be compared with LTspice semantics before numeric claims. |
| Operators | `**`, Boolean AND, OR, XOR, unary `!`, unary `~` | Parser shim / diagnostic | LTspice uses `^` as Boolean XOR in ordinary expressions and exponentiation only in Laplace expressions. |
| Smooth limits | `uplim`, `dnlim`, `limit` semantics | Parser/runtime shim | `limit` exists, but LTspice smooth limit functions need separate tests. |
| Behavioral-source options | `ic=`, `tripdv`, `tripdt`, `Rpar`, `laplace=<expr>`, `window`, `nfft`, `mtol`, `NoJacob` | Parser shim / diagnostic | Timestep-control and FFT-convolution options must not be silently ignored. |

### Unsupported Or Engine-Required Features To Classify Explicitly

| Feature | Reason to classify | Initial action |
| --- | --- | --- |
| `.tf` | SpiceSharp runtime may support related small-signal operations, but no LTspice control reader is registered | Add targeted diagnostic before implementation |
| `.four` | Post-transient Fourier reporting, not a circuit element | Add targeted diagnostic or post-processing proposal |
| `.net` | Network-parameter post-processing around `.ac` | Add targeted diagnostic; consider later AC export/post-processing support |
| `.ferret` | Downloads external files by URL | Intentional unsupported diagnostic |
| `.loadbias` / `.savebias` | Reads/writes solver state files | Diagnostic until a portable state format is designed |
| `.machine` / `.endmachine` | LTspice state-machine language with state, rule, and output statements | Engine required or intentional unsupported |
| Dynamic ABM functions | `delay`, `absdelay`, `ddt`, `idt`, `sdt`, `idtmod` need transient history or integration state | Diagnostic until SpiceSharpBehavioral/runtime support is proven |
| `VDMOS` | LTspice-specific power MOSFET behavior and capacitance model | SpiceSharp engine proposal, not parser-only support |
| Advanced MOS levels | Levels 4, 5, 6, 8, 9, 12, 14, 73 and related variants | Engine-required classification per model family |
| Lossy/distributed lines | `O` / `LTRA`, `URC` | Engine-required classification and direct SpiceSharp tests |
| MESFET and IGBT families | `NMF`, `PMF`, `NIGBT`, `PIGBT` | Diagnostic or engine proposal |

### Model And Instance Parameter Triage

Build model-family tables that classify each parameter as direct map, alias, metadata no-op, behavior-changing unsupported, or engine-required.

| Family | High-priority LTspice gaps | Initial classification rule |
| --- | --- | --- |
| Diode | Ideal diode parameters `Ron`, `Roff`, `Vfwd`, `Vrev`, `Rrev`, `Ilimit`, `Revilimit`, `Epsilon`, `Revepsilon`; metadata such as `Vpk`, `Ipk`, `Iave`, `Irms`, `diss`; extended recovery/noise terms | Metadata can warn/no-op in LTspice mode; electrical terms need mapping or diagnostic |
| Switch | `Lser`, `Vser`, `Ilimit`, `level`, `oneway`, `epsilon`, negative hysteresis semantics | Map only proven equivalents; diagnose current limiting and one-way behavior until represented |
| MOSFET | Three-terminal `VDMOS` instances, `off`, `IC=`, `temp`, geometry defaults, metadata `mfg`, `Vds`, `Ron`, `Qg`, `pchan` / `nchan` | Legacy MOS parameters can map where supported; `VDMOS` is engine-required |
| R/C/L | Temperature coefficients, geometry forms, model-level defaults, layout metadata | Mostly parser tolerance, but behavior-changing temperature forms need tests |
| Transmission line | Lossless `T` options versus `O` / `LTRA` and `URC` model cards | Preserve `T`; classify lossy/distributed lines as engine-required |

## Compatibility Matrix

Create a matrix document or test data file before broadening support claims. Classify each feature independently for parse, read, run, and numeric confidence.

| Feature | Parse | Read | Run | Numeric confidence | Diagnostics | Minimum packages | Notes |
| --- | --- | --- | --- | --- | --- | --- | --- |
| LTspice construct or netlist pattern | Accepted / rejected | SpiceSharp objects produced | OP/DC/AC/TRAN/NOISE support | Analytic / golden / smoke / divergent | Expected warning or error | Parser / engine package floor | Known limitations and migration guidance |

Compatibility classes:

| Class | Meaning | Expected behavior |
| --- | --- | --- |
| Supported | Parser and runtime can represent the feature | Parse/read/run tests pass |
| Parser shim | LTspice spelling lowers to existing behavior | Parser, reader, and writer parity where applicable |
| Recognized no-op | Display, probing, or annotation metadata | Opt-in LTspice mode emits a warning and continues |
| Engine required | Netlist parses but runtime behavior is missing | Parser matrix marks read-only or diagnostic until engine tests exist |
| Intentional unsupported | Proprietary, unsafe, or out of scope | Targeted diagnostic |
| Numeric divergence | Feature runs but differs from LTspice semantics/defaults | Document tolerances and known differences |

## Roadmap

| Priority | Phase | Outcome | Primary repo |
| --- | --- | --- | --- |
| P0 | Fixtures and matrix | Permission-safe baseline that defines support claims | SpiceSharpParser |
| P1 | Opt-in dialect/no-op infrastructure | Harmless LTspice-generated statements stop causing generic failures | SpiceSharpParser |
| P2 | Scalar expression and ABM compatibility | More common behavioral expressions parse/read with clear unsupported diagnostics | SpiceSharpParser first |
| P3 | Model and instance parameter tolerance | Vendor-style decks fail less often and fail clearer | SpiceSharpParser first |
| P4 | Runtime gap closure | Engine support for fixture-proven blockers | SpiceSharp |
| P5 | Docs and release governance | README/docs/package claims stay aligned with tested behavior | Both |

## P0: Fixtures And Matrix

Goal: replace guesswork with executable, redistributable LTspice compatibility evidence.

Implementation backlog:

- Add synthetic fixtures under the integration-test area and classify them as parse-only, read-only, runnable, or expected diagnostic.
- Seed the first matrix from the LTspice feature inventory in this document, rather than from support claims in README text.
- Cover `.include` and `.lib` with quoted paths, nested includes, selected sections, Windows separators, and relative paths.
- Cover common generated controls: `.param`, `.func`, `.step`, `.meas`, `.options`, `.ic`, `.nodeset`, `.temp`, and fixture-proven no-op candidates such as `.backanno`.
- Cover unsupported-control diagnostics for `.tf`, `.four`, `.net`, `.ferret`, `.loadbias`, `.savebias`, and `.machine` before attempting full implementations.
- Cover behavioral forms: `B`, `VALUE=`, `TABLE`, `POLY`, source-level `LAPLACE`, function-style `LAPLACE(...)`, and LTspice behavioral-source instance options.
- Cover source waveforms: `PULSE`, `SIN`, `SINE`, `PWL`, `SFFM`, `AM`, `EXP`, wave-file input, and LTspice cycle-count arguments.
- Cover model decks and subcircuits with synthetic, license-safe examples that mimic vendor decks without copying proprietary libraries.

Acceptance criteria:

- The matrix exists and includes parse, read, run, numeric-confidence, diagnostics, notes, and minimum-package columns.
- Each new support claim has at least one fixture.
- Expected failures assert targeted diagnostics instead of generic reader/parser failures.

## P1: Opt-In Dialect And No-Ops

Goal: accept harmless LTspice-generated syntax without weakening default diagnostics.

Implementation backlog:

- Fix `ValidationEntryCollection.Warnings` before relying on warnings in compatibility tests.
- Add `CompatibilityOptions` to parser and reader settings, with an LTspice preset or factory.
- Wire opt-in LTspice behavior through preprocessing, control mapping, and reader diagnostics without changing default behavior.
- Add a recognized-no-op control path for fixture-proven display/probing/annotation statements, starting with `.backanno`.
- Add option classification tables for mapped solver options, warning no-ops, and behavior-changing unsupported LTspice options.
- Add targeted unsupported diagnostics for LTspice controls that are recognized but not safe to ignore, starting with `.tf`, `.four`, `.net`, `.ferret`, `.loadbias`, `.savebias`, and `.machine`.
- Decide how `.tran` one-argument syntax and LTspice modifiers are handled before broad transient compatibility claims.
- Tighten `.include` / `.lib` tests before changing path or section behavior.

Acceptance criteria:

- With LTspice compatibility enabled, recognized no-ops emit warnings and do not block reading.
- With default settings, existing non-LTspice tests keep their current behavior.
- Unsupported LTspice controls report the directive name and reason.

## P2: Scalar Expression And ABM Compatibility

Goal: accept more LTspice behavioral expressions when they can map to existing runtime behavior.

Implementation backlog:

- Audit LTspice scalar functions against existing math functions, random functions, resolver functions, `.FUNC`, and behavioral-source support.
- Implement safe scalar aliases where semantics are clear and static, including `arccos`, `arcsin`, `arctan`, `fabs`, `sgn`, and `round`.
- Add or diagnose LTspice operators such as `**`, Boolean `&`, `|`, `^`, and unary `!` / `~`.
- Add static functions such as `pwr`, `pwrs`, and `hypot` when their real-valued semantics can be represented.
- Implement or lower scalar `table(...)`; the current `CreateTable()` TODO makes this an early candidate.
- Compare existing `mc`, `gauss`, `flat`, `random`, `rand`, `white`, and `unif` behavior with LTspice semantics and document divergences.
- Add compatibility decisions for smooth limiting functions such as `uplim` and `dnlim`.
- Add targeted diagnostics for dynamic/stateful functions that need simulation history, derivatives, integrals, delay buffers, or transient noise semantics.
- Keep `LAPLACE(...)` on the existing lowering path and do not register it as a scalar math function.

Acceptance criteria:

- Safe aliases have evaluator, resolver, behavioral-source, and generated-writer coverage where applicable.
- `table(...)` compatibility is covered by parse/read/run tests or intentionally diagnostic fixtures.
- Dynamic unsupported functions fail with actionable diagnostics.

## P3: Model And Instance Parameter Tolerance

Goal: make common LTspice/vendor model decks map where equivalent behavior exists and fail clearly where it does not.

Implementation backlog:

- Build alias/ignore/error tables per model family.
- Map direct equivalents through existing model generators and parameter update paths.
- Warn on LTspice metadata or layout-only parameters only in LTspice compatibility mode.
- Error on unsupported behavior-changing parameters with component/model name and suggested fallback when possible.
- Test model parameter expressions, subcircuit defaults, geometry parameters, temperature parameters, and `.MODEL` variants.
- Add explicit triage for LTspice ideal diode parameters, switch current-limiting and one-way parameters, MOS metadata, and three-terminal `VDMOS` syntax.

Model-family priorities:

| Family | Parser action | Engine action |
| --- | --- | --- |
| R/C/L | Alias tolerances, temperature coefficients, and geometry forms | Usually existing behavior |
| Diode | Map supported parameters, classify ideal-diode terms, diagnose unsupported recovery/noise terms | Engine changes only for measured blockers |
| BJT/JFET | Map known SPICE parameters and metadata | Engine changes only with fixtures |
| MOSFET | Separate legacy levels from LTspice/vendor power models, especially `VDMOS` | Advanced models likely engine-required |
| Switch | Map threshold, hysteresis, and resistance aliases; diagnose `Lser`, `Vser`, `Ilimit`, `oneway`, and `epsilon` until represented | Engine work may be needed for current limiting and one-way behavior |
| Transmission line | Preserve current lossless `T` support | `O` / `LTRA` and `URC` need engine work |

Acceptance criteria:

- Each alias/ignore/error decision has a fixture.
- Behavior-changing unsupported parameters are never silently discarded.
- Docs distinguish parse tolerance from numeric equivalence.

## P4: SpiceSharp Runtime Gap Closure

Goal: add engine capabilities only after parser fixtures prove that parsing and lowering are insufficient.

Possible engine work:

- LTspice/vendor-specific MOSFET or power-device models, especially `VDMOS`.
- Advanced monolithic MOS levels beyond existing levels 1, 2, and 3.
- Lossy or distributed transmission-line variants such as `O` / `LTRA` and `URC`.
- Dynamic behavioral functions requiring time history, derivatives, integrals, delay buffers, or transient noise.
- State-machine support for `.machine` blocks if it is ever considered in scope.
- MESFET and IGBT device families if fixtures prove they are common blockers.
- Transient robustness around timestep-sensitive switching.
- Noise behavior differences where LTspice semantics do not match SpiceSharp frequency-domain noise support.

Acceptance criteria:

- Engine changes land with direct SpiceSharp tests before parser runnable support is claimed.
- Parser fixtures reference the minimum engine package version needed.
- Runtime behavior is documented with tolerances or known divergence.
- Solver-default changes are avoided unless measured and documented.

## P5: Docs And Release Governance

Goal: keep package references, generated C# writer behavior, docs, and README claims aligned.

Implementation backlog:

- Update README LTspice language only when matrix rows and fixtures support the claim.
- Add troubleshooting entries for common unsupported LTspice constructs.
- Add migration examples showing equivalent supported syntax.
- Preserve generated C# writer parity for every new lowering or entity mapping.
- Update SpiceSharpParser package references only after needed SpiceSharp changes have stable tests.
- Keep vendor-library fixture policy explicit: synthetic examples by default, copied libraries only with clear redistribution permission.

Acceptance criteria:

- Full parser solution tests pass after parser changes.
- Full engine solution tests pass before package integration when engine work is involved.
- Docs and matrix are updated in the same change as compatibility behavior.

## Implementation Map

Use these areas as the starting map, not as an exhaustive file checklist:

- Settings and dialect flow: parser settings, reader settings, object mappings, control reader, preprocessing pipeline.
- Diagnostics: validation collection, targeted reader/parser errors, warning-based no-op controls.
- Expressions and ABM: math/random functions, expression resolver, behavioral source generator, C# writer parity.
- Models and parameters: model generators, component generators, parameter update paths, semiconductor docs.
- Fixtures and docs: integration tests, README, LTspice compatibility matrix, behavioral-source and Laplace docs.

## Verification Strategy

For each compatibility feature:

1. Add or update a matrix row.
2. Add a fixture before or with implementation.
3. Classify the fixture as parse-only, read-only, runnable, or expected diagnostic.
4. Use analytic expectations or permission-safe golden values for numeric checks.
5. Avoid CI dependency on proprietary LTspice tooling.
6. Verify generated C# writer parity when a source lowering or new entity mapping is involved.
7. Update docs before broadening public support claims.

Run parser and engine tests separately when code changes require them:

```powershell
dotnet test d:\dev\SpiceSharpParser\src\SpiceSharp-Parser.sln
dotnet test d:\dev\SpiceSharp\SpiceSharp.sln
```

## Resolved Decisions

- LTspice-generated netlists are the first target.
- `.asc` and `.asy` schematic import are excluded from this roadmap.
- Compatibility mode is opt-in.
- `CompatibilityOptions` with an LTspice preset is the recommended API shape.
- Recognized LTspice no-ops produce warnings until an informational diagnostic level exists.
- Parser compatibility and diagnostics come first; engine changes follow measured blockers.
- Existing `B`, `VALUE`, and `LAPLACE(...)` support is baseline behavior to preserve.
- Compatibility claims must be backed by fixtures and matrix rows.
