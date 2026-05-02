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
- Cover `.include` and `.lib` with quoted paths, nested includes, selected sections, Windows separators, and relative paths.
- Cover common generated controls: `.param`, `.func`, `.step`, `.meas`, `.options`, `.ic`, `.nodeset`, `.temp`, and fixture-proven no-op candidates such as `.backanno`.
- Cover behavioral forms: `B`, `VALUE=`, `TABLE`, `POLY`, source-level `LAPLACE`, and function-style `LAPLACE(...)`.
- Cover source waveforms: `PULSE`, `SIN`, `SINE`, `PWL`, `SFFM`, and `AM`.
- Cover model decks and subcircuits with synthetic, license-safe examples.

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
- Add targeted unsupported diagnostics for LTspice controls that are recognized but not safe to ignore.
- Tighten `.include` / `.lib` tests before changing path or section behavior.

Acceptance criteria:

- With LTspice compatibility enabled, recognized no-ops emit warnings and do not block reading.
- With default settings, existing non-LTspice tests keep their current behavior.
- Unsupported LTspice controls report the directive name and reason.

## P2: Scalar Expression And ABM Compatibility

Goal: accept more LTspice behavioral expressions when they can map to existing runtime behavior.

Implementation backlog:

- Audit LTspice scalar functions against existing math functions, random functions, resolver functions, `.FUNC`, and behavioral-source support.
- Implement safe scalar aliases where semantics are clear and static.
- Implement or lower scalar `table(...)`; the current `CreateTable()` TODO makes this an early candidate.
- Compare existing `mc`, `gauss`, `flat`, `random`, and `unif` behavior with LTspice semantics and document divergences.
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

Model-family priorities:

| Family | Parser action | Engine action |
| --- | --- | --- |
| R/C/L | Alias tolerances, temperature coefficients, and geometry forms | Usually existing behavior |
| Diode | Map supported parameters, diagnose unsupported recovery/noise terms | Engine changes only for measured blockers |
| BJT/JFET | Map known SPICE parameters and metadata | Engine changes only with fixtures |
| MOSFET | Separate legacy levels from LTspice/vendor power models | Advanced models likely engine-required |
| Switch | Map threshold, hysteresis, and resistance aliases | Existing behavior likely sufficient |
| Transmission line | Preserve current lossless `T` support | Lossy/distributed variants may need engine work |

Acceptance criteria:

- Each alias/ignore/error decision has a fixture.
- Behavior-changing unsupported parameters are never silently discarded.
- Docs distinguish parse tolerance from numeric equivalence.

## P4: SpiceSharp Runtime Gap Closure

Goal: add engine capabilities only after parser fixtures prove that parsing and lowering are insufficient.

Possible engine work:

- LTspice/vendor-specific MOSFET or power-device models.
- Lossy or distributed transmission-line variants.
- Dynamic behavioral functions requiring time history, derivatives, integrals, or delay buffers.
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
