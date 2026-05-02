---
title: LTspice Netlist Compatibility Plan
status: Draft / Roadmap
scope: SpiceSharpParser + SpiceSharp
last_reviewed: 2026-05-03
---

# LTspice Netlist Compatibility Plan

## Summary

The first compatibility target is LTspice-generated netlists: copied netlists, `.net` files, and model decks reached through `.include` / `.lib`. Schematic import for `.asc` / `.asy` files is out of scope for this roadmap.

The recommended approach is phased:

1. Measure current compatibility with permission-safe fixtures.
2. Add parser-level dialect shims, aliases, no-op handling, and targeted diagnostics.
3. Escalate only proven runtime gaps into SpiceSharp engine changes.
4. Keep public support claims tied to tests and a compatibility matrix.

Current `B` source and function-style `LAPLACE(...)` support are baseline behavior, not missing work. SpiceSharpParser already supports arbitrary behavioral voltage/current sources, source-level `E` / `G` / `F` / `H` `LAPLACE`, function-style `LAPLACE(input, transfer)`, inline `M=` / `TD=` / `DELAY=` options, helper lowering for mixed expressions, and generated C# writer parity.

## Goals

- Make common LTspice-generated netlists parse, read, and run when they use features that can be represented safely by SpiceSharp and SpiceSharpBehavioral.
- Prefer compatibility shims and explicit diagnostics in SpiceSharpParser before adding new engine behavior.
- Build a small, redistributable compatibility corpus that does not copy proprietary LTspice or vendor library content without permission.
- Keep parser, generated C# writer, docs, and tests aligned.
- Record known divergences from LTspice instead of making blanket numeric-parity claims.

## Non-Goals

- Importing or rendering LTspice schematic files (`.asc`) or symbols (`.asy`).
- Depending on LTspice itself in CI.
- Claiming complete LTspice numeric parity across all device models and solver settings.
- Committing copied vendor model libraries unless their license explicitly permits redistribution.
- Duplicating SpiceSharpBehavioral features directly in SpiceSharpParser.

## Baseline To Preserve

Keep these existing compatibility surfaces working while adding LTspice-specific behavior:

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

The `LAPLACE` transfer subset remains a finite, proper rational polynomial in `s` with finite coefficients and non-singular DC gain. `LAPLACE` should stay a source/lowering feature, not a normal scalar expression function.

## Compatibility Classes

Each fixture or feature should be classified separately for parsing, reading, execution, and numeric confidence.

| Class | Meaning | Expected behavior |
| --- | --- | --- |
| Supported | Parser and engine can represent the feature | Parse/read/run tests pass |
| Parser shim | LTspice spelling can lower to existing SpiceSharp behavior | Add parser support and writer parity |
| Recognized no-op | LTspice statement is display, probing, or annotation metadata | Ignore with info/warning diagnostics |
| Engine required | The netlist parses but behavior needs a SpiceSharp runtime feature | Add engine tests before parser claims run support |
| Intentional unsupported | Feature is proprietary, out of scope, or unsafe to approximate | Emit targeted diagnostics |
| Numeric divergence | Feature runs but differs from LTspice semantics or defaults | Document tolerances and known differences |

## Roadmap

| Priority | Phase | Main outcome | Primary repo |
| --- | --- | --- | --- |
| P0 | Baseline corpus | Compatibility matrix and fixtures | SpiceSharpParser |
| P1 | Dialect/no-op infrastructure | More LTspice-generated netlists accepted | SpiceSharpParser |
| P2 | Scalar expression and ABM compatibility | Common behavioral expressions accepted | SpiceSharpParser first |
| P3 | Model and instance parameter tolerance | Vendor-style model decks fail less often and fail clearer | SpiceSharpParser first |
| P4 | Runtime gap closure | Engine support for measured blockers | SpiceSharp |
| P5 | Release integration | Parser depends on tested engine capability | Both |
| P6 | Documentation governance | Honest compatibility docs | SpiceSharpParser |

## Phase 0: Compatibility Baseline

Goal: replace guesswork with an executable, redistributable LTspice compatibility corpus.

Add fixtures under the integration-test area and group them by expected outcome:

- Parse-only: syntax that should tokenize and parse, even if no engine entity is produced.
- Read-only: netlists that should convert into a SpiceSharp model without running.
- Runnable: OP, DC, AC, TRAN, and NOISE examples with analytic or permission-safe expected values.
- Expected diagnostic: recognized LTspice constructs that should produce targeted warnings or errors.

Cover common LTspice-generated shapes:

- `.include` and `.lib`, including quoted paths, nested includes, selected sections, Windows path separators, and relative paths.
- `.param`, `.func`, `.step`, `.meas`, `.options`, `.ic`, `.nodeset`, `.temp`, and harmless generated controls such as `.backanno`.
- Behavioral `B` sources, `VALUE=`, `TABLE`, `POLY`, source-level `LAPLACE`, and function-style `LAPLACE(...)`.
- Voltage/current source waveforms: `PULSE`, `SIN`, `SINE`, `PWL`, `SFFM`, and `AM`.
- Model decks and subcircuits from synthetic, license-safe examples.

Deliverables:

- A compatibility matrix document or test data file.
- A naming convention for fixture classes, for example `LtspiceCompatibilityTests` plus feature-specific classes.
- A rule that every future compatibility claim points to a fixture.

## Phase 1: Dialect Infrastructure And No-Ops

Goal: support harmless LTspice-generated syntax without weakening diagnostics for other dialects.

Implementation direction:

- Add an explicit compatibility option, such as a `SpiceDialect` enum or flags-style `CompatibilityOptions` object.
- Prefer generic syntax improvements by default only when they do not change PSpice/ngspice behavior.
- Recognize display/probing/annotation statements that can be safely ignored, such as `.backanno`, with informational or warning diagnostics.
- Keep the existing processor pipeline intact: includes, library expansion, macros, append-model, AKO models, sweeps, and `.if` processing.
- Tighten `.include` / `.lib` tests around LTspice path conventions before changing behavior.

Candidate files:

- `src/SpiceSharpParser/SpiceNetlistParser.cs`
- `src/SpiceSharpParser/ModelReaders/Netlist/Spice/SpiceNetlistReaderSettings.cs`
- `src/SpiceSharpParser/ModelReaders/Netlist/Spice/SpiceObjectMappings.cs`
- `src/SpiceSharpParser.IntegrationTests/`

Acceptance criteria:

- LTspice-generated metadata statements no longer cause generic parse/read failures.
- Unsupported LTspice controls produce targeted diagnostics.
- Existing non-LTspice tests remain unchanged.

## Phase 2: Scalar Expression And ABM Compatibility

Goal: accept more LTspice behavioral expressions when they can be represented safely by existing runtime behavior.

Implementation direction:

- Audit LTspice scalar functions against current math functions, random functions, resolver functions, `.FUNC`, and behavioral-source support.
- Implement safe scalar aliases first.
- Treat `table()` as an early candidate because the math-function registry already exposes it as a TODO.
- Add targeted diagnostics for recognized dynamic ABM functions that need simulation state or history.
- Keep `LAPLACE(...)` on the existing lowering path rather than registering it as a scalar function.

Function categories:

| Category | Examples | Plan |
| --- | --- | --- |
| Pure scalar aliases | clamp/limit-style helpers, sign/step variants | Add parser/evaluator aliases when semantics are clear |
| Existing random/Monte Carlo | `mc`, `gauss`, `flat`, `random`, `unif` variants | Compare LTspice semantics and document differences |
| Table lookup | `table(...)` scalar form | Implement or lower to existing table behavior |
| Dynamic/stateful | delay, derivative, integral, time-history functions | Classify as engine-required unless already supported |
| Noise/random time sources | transient noise-like expressions | Engine-required or intentionally unsupported |

Candidate files:

- `src/SpiceSharpParser/ModelReaders/Netlist/Spice/Evaluation/Functions/MathFunctions.cs`
- `src/SpiceSharpParser/ModelReaders/Netlist/Spice/Evaluation/Functions/RandomFunctions.cs`
- `src/SpiceSharpParser/ModelReaders/Netlist/Spice/Evaluation/ExpressionResolver.cs`
- `src/SpiceSharpParser/ModelReaders/Netlist/Spice/Readers/EntityGenerators/Components/Sources/ArbitraryBehavioralGenerator.cs`
- `src/docs/articles/behavioral-source.md`
- `src/docs/articles/laplace.md`

Acceptance criteria:

- Safe aliases have parser, evaluator, and behavioral-source tests.
- Dynamic unsupported functions produce clear diagnostics.
- Generated C# writer output remains valid for every new lowering.

## Phase 3: Model And Instance Parameter Tolerance

Goal: let common LTspice model decks map cleanly where SpiceSharp has equivalent behavior, and fail clearly where it does not.

Implementation direction:

- Build alias/ignore/error tables per model family.
- Map direct equivalents through existing model generators and parameter update paths.
- Ignore LTspice metadata or layout-only parameters with compatibility diagnostics.
- Fail for behavior-changing unsupported parameters with the component/model name and suggested fallback.
- Test model parameter expressions, subcircuit parameter defaults, geometry parameters, temperature parameters, and `.MODEL` variants.

Model families:

| Family | Parser action | Engine action |
| --- | --- | --- |
| R/C/L | Alias tolerances, temperature coefficients, geometry forms | Usually existing behavior |
| Diode | Alias supported model parameters, warn on unsupported noise/recovery terms | Add engine behavior only for measured blockers |
| BJT | Map known SPICE parameters, document divergence | Engine changes only with fixtures |
| JFET | Map known parameters and metadata | Engine changes only with fixtures |
| MOSFET | Separate legacy levels from LTspice/vendor power models | Likely engine-required for advanced models |
| Switch | Map threshold/hysteresis/resistance aliases | Existing behavior likely sufficient |
| Transmission line | Preserve current lossless `T` support | Lossy/distributed variants may need engine work |

Candidate files:

- `src/SpiceSharpParser/ModelReaders/Netlist/Spice/Readers/EntityGenerators/Components/Semiconductors/`
- `src/SpiceSharpParser/ModelReaders/Netlist/Spice/Readers/EntityGenerators/Models/`
- `src/SpiceSharpParser/ModelReaders/Netlist/Spice/Context/Updates/`
- `src/docs/articles/mosfet.md`
- `src/docs/articles/diode.md`
- `src/docs/articles/transmission-line.md`

Acceptance criteria:

- Each alias/ignore/error decision is covered by a fixture.
- Behavior-changing unsupported parameters do not silently disappear.
- Docs distinguish parse tolerance from numeric equivalence.

## Phase 4: SpiceSharp Runtime Gap Closure

Goal: add engine capabilities only after parser fixtures prove that parsing and lowering are not enough.

Possible engine work:

- LTspice/vendor-specific MOSFET or power-device models.
- Lossy/distributed transmission-line variants.
- Dynamic behavioral functions that require time history, derivatives, integrals, or delays.
- Transient compatibility improvements around timestep guidance and fast switching.
- Noise behavior differences, especially where LTspice semantics do not match SpiceSharp frequency-domain noise support.

Implementation direction:

- Add NUnit tests in the SpiceSharp repository before claiming parser runnable support.
- Reuse existing SpiceSharp patterns: component parameters, binding contexts, behavior interfaces, generated behavior factories, and biasing/frequency/time/noise behaviors.
- Avoid hidden solver-default changes unless they are measured and documented.
- Prefer parser-side warnings for timestep-sensitive generated netlists over silent numerical tuning.

Candidate files in the SpiceSharp repository:

- `SpiceSharp/Components/`
- `SpiceSharp/Simulations/`
- `SpiceSharpGenerator/BehaviorGenerator.cs`
- `SpiceSharpTest/`

Acceptance criteria:

- Engine changes have direct NUnit coverage.
- Parser fixtures reference the minimum engine package version needed.
- Runtime behavior is documented with tolerances or known divergence.

## Phase 5: Parser And Engine Release Integration

Goal: keep the parser package, engine package, and generated C# writer in lockstep.

Implementation direction:

- Land SpiceSharp engine changes first when needed.
- Update SpiceSharpParser package references only after engine tests are stable.
- Add versioned diagnostics when a parser feature requires newer SpiceSharp or SpiceSharpBehavioral packages.
- Preserve generated C# writer parity for every lowering or new entity mapping.
- Keep old behavior stable for netlists that do not opt into LTspice compatibility settings, unless the change is a safe generic parser improvement.

Acceptance criteria:

- Full parser solution tests pass after package updates.
- Full engine solution tests pass before parser integration.
- Generated C# writer tests cover any new LTspice-compatible entity mapping.

## Phase 6: Documentation And Governance

Goal: make compatibility claims precise enough for users to trust.

Documentation updates:

- README high-level LTspice support statement.
- A compatibility matrix with separate parse, read, run, and numeric-confidence columns.
- Troubleshooting entries for common unsupported LTspice constructs.
- Migration examples showing equivalent supported syntax.
- A fixture policy for vendor libraries and license-sensitive model decks.

Compatibility matrix columns:

| Column | Purpose |
| --- | --- |
| Feature | LTspice construct or netlist pattern |
| Parse | Syntax accepted |
| Read | Converted to SpiceSharp objects |
| Run | OP/DC/AC/TRAN/NOISE execution support |
| Numeric confidence | Analytic, golden, smoke-only, or divergent |
| Diagnostics | Expected warnings/errors |
| Notes | Known limitations and migration guidance |

## Verification Strategy

Run parser and engine tests separately:

```powershell
dotnet test d:\dev\SpiceSharpParser\src\SpiceSharp-Parser.sln
dotnet test d:\dev\SpiceSharp\SpiceSharp.sln
```

For each compatibility feature:

1. Add a fixture before or with implementation.
2. Classify the fixture as parse-only, read-only, runnable, or expected diagnostic.
3. Use analytic expectations or permission-safe golden values for numeric checks.
4. Avoid CI dependency on proprietary LTspice tooling.
5. Verify generated C# writer parity when a source lowering or new entity mapping is involved.
6. Update docs and the compatibility matrix before changing support claims.

## Open Decisions

- Compatibility API shape: `SpiceDialect.LTspice`, `LtspiceCompatibility`, or a flags-style `CompatibilityOptions` object. A flags-style options object is recommended if multiple dialect quirks will coexist.
- Whether recognized LTspice no-ops should default to warnings or informational diagnostics.
- Whether LTspice compatibility mode should be opt-in only, or whether harmless no-op recognition should be enabled by default.
- How to version diagnostics for parser features that require newer SpiceSharp or SpiceSharpBehavioral packages.

## Key Files

Parser-side files:

- `README.md`
- `src/docs/articles/behavioral-source.md`
- `src/docs/articles/laplace.md`
- `src/SpiceSharpParser/SpiceNetlistParser.cs`
- `src/SpiceSharpParser/ModelReaders/Netlist/Spice/SpiceObjectMappings.cs`
- `src/SpiceSharpParser/ModelReaders/Netlist/Spice/SpiceNetlistReaderSettings.cs`
- `src/SpiceSharpParser/ModelReaders/Netlist/Spice/Evaluation/Functions/MathFunctions.cs`
- `src/SpiceSharpParser/ModelReaders/Netlist/Spice/Evaluation/Functions/RandomFunctions.cs`
- `src/SpiceSharpParser/ModelReaders/Netlist/Spice/Readers/EntityGenerators/Components/Sources/ArbitraryBehavioralGenerator.cs`
- `src/SpiceSharpParser/ModelReaders/Netlist/Spice/Readers/EntityGenerators/Components/Sources/LaplaceFunctionExpressionLowerer.cs`
- `src/SpiceSharpParser/ModelReaders/Netlist/Spice/Readers/EntityGenerators/Components/Semiconductors/`
- `src/SpiceSharpParser.IntegrationTests/`
- `src/SpiceSharpParser/SpiceSharpParser.csproj`

Engine-side files, in the sibling SpiceSharp repository:

- `SpiceSharp/Components/`
- `SpiceSharp/Simulations/`
- `SpiceSharpGenerator/BehaviorGenerator.cs`
- `SpiceSharpTest/`

## Current Decisions

- LTspice-generated netlists are the first target.
- `.asc` and `.asy` schematic import are excluded from the first roadmap.
- Parser compatibility and diagnostics come first; engine changes follow measured blockers.
- Existing `B` source and function-style `LAPLACE(...)` support are current baseline behavior.
- Copyright-sensitive vendor libraries should not be committed as fixtures unless license terms clearly allow it.
