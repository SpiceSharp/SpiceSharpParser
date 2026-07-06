---
title: LTspice Netlist Compatibility Plan
status: Draft / Roadmap
scope: SpiceSharpParser + SpiceSharp
last_reviewed: 2026-07-06
---

# LTspice Netlist Compatibility Plan

## Summary

The first LTspice compatibility target is generated netlists: copied netlists, `.net` files, and model decks reached through `.include` / `.lib`. Schematic and symbol import for `.asc` / `.asy` files is out of scope.

This roadmap is intentionally parser-first and evidence-first:

1. Measure current behavior with redistributable fixtures and a compatibility matrix.
2. Add opt-in LTspice compatibility settings, harmless no-op handling, and targeted diagnostics.
3. Add scalar-expression, ABM, waveform, and model-parameter tolerance only when behavior can be represented safely.
4. Escalate to SpiceSharp runtime work only after fixtures prove parser lowering is not enough.
5. Keep README/docs support claims tied to tests and matrix rows.

## Current Repo Facts

- Arbitrary `B` voltage/current sources and `VALUE={expr}` controlled-source forms are already supported.
- Source-level `E` / `G` / `F` / `H` `LAPLACE` forms are already supported, including alternate spellings, finite constant `M=`, `TD=`, and `DELAY=` options.
- Function-style `LAPLACE(input, transfer)` is already supported inside `VALUE`, `B ... V=`, and `B ... I=` expressions, including helper lowering for mixed expressions and arbitrary scalar inputs.
- Generated C# writer parity already exists for the implemented Laplace lowering paths.
- `LAPLACE` must remain a source/lowering feature, not a normal scalar expression function.
- P1 adds explicit `CompatibilityOptions` presets in parser and reader settings; both default to `CompatibilityOptions.None`.
- Validation currently has error and warning levels only. Use warnings for recognized LTspice no-ops until an informational level exists.
- `ValidationEntryCollection.Warnings` filters warning entries and is covered by warning/error separation tests.
- Scalar `table(...)` and `tbl(...)` are supported through SpiceSharpBehavioral defaults and covered by LTspice P2 fixtures; `MathFunctions.CreateTable()` now returns a parser-owned interpolation helper for direct factory callers.
- The default mappings register many common controls and devices, but registration does not prove LTspice syntax parity for every variant.
- `.FOUR` transient Fourier post-processing is implemented as dialect-neutral output support. Results are exposed through `SpiceSharpModel.FourierAnalyses`, with coverage for multiple signals, current signals, parameterized frequencies, stepped transient runs, and targeted failure diagnostics.
- Default reader behavior still rejects LTspice `.backanno`, `.tf`, `.net`, `.ferret`, `.loadbias`, `.savebias`, and `.machine` / `.endmachine` with targeted diagnostics.
- In LTspice mode, `.backanno` is a warning no-op. The other known unsupported LTspice controls remain targeted errors.
- Include/lib path evidence covers quoted paths, Windows and slash separators, nested `.include` resolution relative to the including file, selected `.lib` sections, and nested selected `.lib` sections resolved relative to the parent library file.
- `.TRAN` accepts traditional numeric forms and trailing `UIC`; P1 LTspice mode also accepts `.tran <Tstop>` and `.tran <Tstop> UIC` by deriving `step = Tstop / 50.0`. LTspice `startup`, `steady`, `nodiscard`, and `step` modifiers remain targeted errors.
- LTspice output/viewer `.options` such as `plotwinsize`, `plotreltol`, `plotvntol`, `plotabstol`, `numdgt`, `measdgt`, `meascplxfmt`, `baudrate`, and `fastaccess` are warning no-ops only in LTspice mode.
- LTspice behavior-changing `.options` such as `cshunt`, `gshunt`, `srcsteps`, `gminsteps`, `trtol`, `chgtol`, `pivrel`, `pivtol`, and `ptrantau` remain targeted errors.
- Source waveform mappings cover `SIN` / `SINE`, `PULSE`, `EXP`, `PWL`, `AM`, `SFFM`, and wave-file input. LTspice finite-cycle `PULSE(... Ncycles)` and `SINE(... Ncycles)` are supported, PWL file parsing supports optional header rows, leading blank/comment lines, and space/comma/semicolon/tab delimiters, unsupported PWL repeat syntax produces targeted diagnostics, wave-file channel defaults are not inferred, and topology-changing independent-source instance options remain targeted errors.
- MOS model generation currently covers legacy levels 1, 2, and 3. LTspice `VDMOS` and advanced monolithic levels such as BSIM/EKV/HiSIM variants are runtime or intentional-unsupported candidates.
- Distributed-line support currently starts from lossless `T`. LTspice lossy `O` / `LTRA` and uniform RC-line `URC` models need engine triage before runnable support is claimed.
- P3 LTspice mode maps R/C model `tc=a[,b]`, switch `von`/`voff`, and current-switch `ion`/`ioff` aliases where they lower to existing parameters.
- P3 LTspice mode warns on recognized metadata/rating parameters and emits targeted errors for topology-changing passive parasitics, ideal-diode parameters when custom mappings are not enabled, switch current-limiting/series options, `VDMOS`, high MOS levels, three-terminal power-MOS syntax, and `O` / `LTRA` / `U` / `URC` line families. The optional custom component package adds runnable `IdealDiode` support for LTspice-style diode parameters plus `NonlinearCapacitor` / `NonlinearInductor` support for LTspice-style capacitor `Q=` and inductor `Flux=` forms when `UseCustomComponents()` is enabled, with optional LTspice-backed DC/AC/transient golden comparisons when `LTSPICE_EXE` is configured.

## Scope And Policy

Goals:

- Make common LTspice-generated netlists parse, read, and run when their features can be represented safely by SpiceSharp and SpiceSharpBehavioral.
- Prefer parser compatibility shims and explicit diagnostics before adding runtime behavior.
- Preserve existing PSpice/ngspice-compatible behavior by default.
- Build a small permission-safe fixture corpus; do not commit copied vendor libraries unless their license clearly permits redistribution.
- Record known divergences instead of making blanket numeric-parity claims.

Non-goals:

- Importing, rendering, or simulating LTspice schematic files (`.asc`) or symbols (`.asy`).
- Depending on LTspice in CI.
- Claiming complete LTspice numeric parity across solver settings, device models, and vendor libraries.
- Silently discarding behavior-changing statements or parameters.
- Duplicating SpiceSharpBehavioral runtime features in SpiceSharpParser.

Compatibility rules:

- Default behavior remains unchanged unless a change is dialect-neutral and covered by existing tests.
- Compatibility mode is opt-in.
- Recognized LTspice display, probing, annotation, or GUI metadata no-ops emit warnings in LTspice mode.
- Behavior-changing unsupported constructs emit targeted errors that name the directive, component, model, option, or parameter.
- Every public compatibility claim must point to a fixture or matrix row.

## Public API Contract

Add compatibility settings as an explicit options object, not as scattered booleans:

```csharp
public sealed class CompatibilityOptions
{
    public static CompatibilityOptions None { get; }
    public static CompatibilityOptions LTspice { get; }
    public bool IsLTspice { get; }
}
```

Implementation expectations:

- Add `CompatibilityOptions Compatibility` to `SpiceNetlistParserSettings`.
- Add `CompatibilityOptions Compatibility` to `SpiceNetlistReaderSettings`.
- Default both settings to `CompatibilityOptions.None`.
- Preserve compatibility settings in `SpiceNetlistReaderSettings.Clone()`.
- Flow parser compatibility through lexing, parsing, preprocessing, include/lib handling, and validation where needed. P1 exposes the setting even though implemented behavior is reader-focused.
- Flow reader compatibility through control mapping, waveform generation, expression evaluation, model generation, and diagnostics where needed. P1 currently uses it for `.backanno`, LTspice option classification, and `.tran <Tstop>` lowering.
- Do not use compatibility settings to hide existing errors outside the explicitly classified LTspice paths.

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

## Compatibility Matrix

Create a matrix document or test data file before broadening support claims. Classify each feature independently for parse, read, run, numeric confidence, and diagnostics.

| Feature | Parse | Read | Run | Numeric confidence | Diagnostics | Minimum packages | Notes |
| --- | --- | --- | --- | --- | --- | --- | --- |
| LTspice construct or netlist pattern | Accepted / rejected | SpiceSharp objects produced | OP/DC/AC/TRAN/NOISE support | Analytic / golden / smoke / divergent | Expected warning or error | Parser / engine package floor | Known limitations and migration guidance |

Compatibility classes:

| Class | Meaning | Expected behavior |
| --- | --- | --- |
| Supported | Parser and runtime can represent the feature | Parse/read/run tests pass |
| Parser shim | LTspice spelling lowers to existing behavior | Parser, reader, and writer parity where applicable |
| Recognized no-op | Display, probing, annotation, or GUI metadata | LTspice mode emits a warning and continues |
| Targeted diagnostic | Known unsupported construct | Validation error names the construct and reason |
| Engine required | Parser can recognize the feature but runtime behavior is missing | Matrix marks non-runnable until engine tests exist |
| Numeric divergence | Feature runs but differs from LTspice semantics/defaults | Document tolerances and known differences |

Fixture rules:

- Prefer synthetic, redistributable fixtures that mimic common generated or vendor-deck patterns.
- Classify every fixture as parse-only, read-only, runnable, or expected diagnostic.
- Add expected-failure fixtures before implementing broad shims so unsupported LTspice forms stop falling through to generic errors.
- Do not require proprietary LTspice tooling in CI.

## Roadmap

| Priority | Phase | Outcome | Primary repo |
| --- | --- | --- | --- |
| P0 | Fixtures and matrix | Permission-safe evidence baseline that defines support claims | SpiceSharpParser |
| P1 | Opt-in dialect and diagnostics | Harmless LTspice-generated statements stop causing generic failures | SpiceSharpParser |
| P2 | Scalar expression, ABM, and waveform compatibility | More common expressions and sources parse/read with clear unsupported diagnostics | SpiceSharpParser first |
| P3 | Model and instance parameter tolerance | Vendor-style decks fail less often and fail clearer | SpiceSharpParser first |
| P4 | Runtime gap closure | Engine support for fixture-proven blockers | SpiceSharp |
| P5 | Docs and release governance | README/docs/package claims stay aligned with tested behavior | Both |

## P0: Fixtures And Matrix

Goal: replace guesswork with executable, redistributable LTspice compatibility evidence.

Implementation backlog:

- Add the compatibility matrix with the columns listed above.
- Add an LTspice compatibility fixture area in integration tests.
- Seed matrix rows from current repo facts and this roadmap, not from broad README support language.
- Add fixtures for current baseline behavior: `B`, `VALUE=`, source-level `TABLE`, `POLY`, source-level `LAPLACE`, function-style `LAPLACE(...)`, `.param`, `.func`, `.include`, `.lib`, `.tran`, common output directives, and `.FOUR` transient post-processing.
- Add expected-diagnostic fixtures for `.tf`, `.net`, `.ferret`, `.loadbias`, `.savebias`, and `.machine` / `.endmachine`.
- Add no-op candidate fixtures for generated metadata, starting with `.backanno`.
- Add initial syntax audit fixtures for `.options`, `.tran`, `.wave`, source waveforms, scalar `table(...)`, and model parameters.
- Fix `ValidationEntryCollection.Warnings` before adding warning-based assertions.

Acceptance criteria:

- The matrix exists and each row has a compatibility class.
- Every new support claim has at least one fixture.
- Expected failures assert targeted diagnostics instead of generic reader/parser failures.
- Default-mode tests and LTspice-mode tests are separate where behavior differs.

## P1: Opt-In Dialect And Diagnostics

Goal: accept harmless LTspice-generated syntax without weakening default diagnostics.

Implemented P1 behavior:

- Added `CompatibilityOptions` and wired it into parser and reader settings.
- Preserved default behavior with `CompatibilityOptions.None`.
- Added `CompatibilityOptions.LTspice` with only fixture-backed behavior enabled.
- Added a recognized-no-op control path for `.backanno` in LTspice mode.
- Kept targeted unsupported diagnostics for `.tf`, `.net`, `.ferret`, `.loadbias`, `.savebias`, and `.machine` / `.endmachine`.
- Added option classification tables for warning no-ops and behavior-changing unsupported LTspice options.
- Added include/lib path fixtures for quoted paths, Windows and slash separators, nested includes, selected library sections, and nested selected library sections.
- Lowered LTspice-mode `.tran <Tstop>` and `.tran <Tstop> UIC` to an explicit compatibility policy: `step = Tstop / 50.0`, with `maxStep = step`.
- Classified LTspice `.tran` modifiers: `UIC` is supported, while `startup`, `steady`, `nodiscard`, and `step` produce targeted diagnostics.

Remaining follow-up:

- Add new `.include` / `.lib` path fixtures only when real-world decks expose additional variants or diagnostics gaps.

Acceptance criteria:

- With LTspice compatibility enabled, recognized no-ops emit warnings and do not block reading.
- With default settings, existing non-LTspice tests keep their current behavior.
- Unsupported LTspice controls report the directive name and reason.
- `SpiceNetlistReaderSettings.Clone()` preserves compatibility settings.

## P2: Scalar Expression, ABM, And Waveform Compatibility

Goal: accept more LTspice behavioral syntax when it maps cleanly to existing runtime behavior.

Implemented P2 behavior:

- Added dialect-neutral expression support for `fabs(x)`, one-argument `round(x)`, unary `!`, and single-character boolean `&` / `|`.
- Added fixture-backed evidence for `arccos`, `arcsin`, `arctan`, `sgn`, `pwr`, `pwrs`, `hypot`, scalar `table(...)` / `tbl(...)`, and `**`.
- Kept `^` as the existing exponent operator; LTspice boolean XOR is deferred to avoid changing current semantics.
- Added LTspice-mode targeted diagnostics for `uplim(...)`, `dnlim(...)`, and unary `~`.
- Added six-argument `EXP(v1 v2 td1 tau1 td2 tau2)` source waveform support with argument-count and positive-tau diagnostics.
- Added LTspice-mode finite-cycle `PULSE(... Ncycles)` and `SINE(... Ncycles)` support with targeted diagnostics for invalid period/frequency and cycle-count arguments.
- Added PWL file fixtures for supported local two-column text variants with optional header rows, leading blank/comment lines, and space/comma/semicolon/tab delimiters, plus targeted diagnostics for missing files, empty files, missing data rows, malformed rows, and unsupported LTspice repeat syntax.
- Added LTspice-mode `tbl=(expr,x1,y1,...)` independent-source lowering to the existing behavioral `table(...)` path.
- Improved `wavefile=<path> chan=<n> [amplitude=<value>]` validation so missing `wavefile`, missing `chan`, missing files, and invalid `chan` values produce targeted diagnostics.
- Added LTspice-mode parser synthesis for topology-changing independent-source options: `Rser` adds a series resistor, `Cpar` adds a shunt capacitor, `load` adds a shunt resistor, and `R=<value>` maps to series resistance on voltage sources or load resistance on current sources.

Remaining follow-up:

- Compare existing random functions with LTspice semantics before making numeric claims.
- Decide whether LTspice boolean XOR can be added without breaking existing `^` exponent behavior.
- Defer LTspice PWL repeat variants until fixture-backed runtime behavior is specified.

Acceptance criteria:

- Safe aliases have evaluator/resolver and behavioral-source coverage where applicable.
- Dynamic or stateful unsupported functions fail with actionable diagnostics.
- Waveform claims distinguish parser acceptance from runtime equivalence.

## P3: Model And Instance Parameter Tolerance

Goal: make common LTspice/vendor model decks map where equivalent behavior exists and fail clearly where they do not.

Implemented P3 behavior:

- Added centralized LTspice model/component parameter classification tables with comments explaining alias, warning no-op, and targeted-error decisions.
- Mapped R/C model `tc=a[,b]` aliases to `tc1` / `tc2`.
- Mapped switch `von` / `voff` and current-switch `ion` / `ioff` aliases to midpoint/hysteresis parameters.
- Warned on recognized LTspice metadata and rating parameters only in LTspice mode.
- Added targeted diagnostics for topology-changing passive parasitics, LTspice ideal-diode parameters when custom mappings are not enabled, switch `Lser` / `Vser` / `Ilimit`, high MOS levels, `VDMOS`, three-terminal power-MOS syntax, and `O` / `LTRA` / `U` / `URC` line families. LTspice ideal-diode parameters, capacitor `Q=`, and inductor `Flux=` remain targeted diagnostics in core LTspice mode but are runnable through `SpiceSharpParser.CustomComponents` as `IdealDiode`, `NonlinearCapacitor`, and `NonlinearInductor`, with optional LTspice-backed DC/AC/transient golden evidence.

Remaining follow-up:

- Broaden model-family alias tables only when direct SpiceSharp equivalents are confirmed by fixtures.
- Add engine work before claiming runnable support for `VDMOS`, advanced MOS families, lossy transmission lines, uniform RC lines, or synthesized passive parasitics.
- Keep vendor-library import tests synthetic unless redistribution is clearly permitted.

Initial model-family priorities:

| Family | Parser action | Engine action |
| --- | --- | --- |
| R/C/L | Alias tolerance, temperature coefficients, geometry forms, metadata classification | Usually existing behavior |
| Diode | Map supported parameters; classify metadata; route LTspice ideal-diode terms through `SpiceSharpParser.CustomComponents` when enabled | Engine changes only for measured blockers outside the custom ideal-diode model |
| BJT/JFET | Map known SPICE parameters and metadata | Engine changes only with fixtures |
| MOSFET | Separate legacy levels from LTspice/vendor power models, especially `VDMOS` | Advanced models likely engine-required |
| Switch | Map threshold, hysteresis, and resistance aliases; diagnose current limiting and one-way behavior | Engine work may be needed |
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

Recent evidence refresh:

- `SpiceSharpParser.AIExamples` adds 948 unique manifest-backed gold/ok accepted examples from DeepSeek/local sources using `accepted_examples_manifest.json` and `accepted_examples_fixture.jsonl`. Treat this as supplemental regression and evidence metadata, not an LTspice compatibility claim unless individual cases are later classified into the compatibility matrix.

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
- Waveforms and outputs: waveform generators, `.wave`, exporters, save/plot/print/meas controls.
- Models and parameters: model generators, component generators, parameter update paths, semiconductor docs.
- Fixtures and docs: integration tests, README, compatibility matrix, behavioral-source and Laplace docs.

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
- Compatibility mode is opt-in and defaults off.
- `CompatibilityOptions` with an LTspice preset is the recommended API shape.
- Recognized LTspice no-ops produce warnings until an informational diagnostic level exists.
- Parser compatibility and diagnostics come first; engine changes follow measured blockers.
- Existing `B`, `VALUE`, `TABLE`, `POLY`, and `LAPLACE(...)` support is baseline behavior to preserve.
- Compatibility claims must be backed by fixtures and matrix rows.
