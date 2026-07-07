---
name: spicesharp-circuit-design
description: Structured R&D problem-solving with test-driven verification for designing and validating analog circuits using SpiceSharp and SpiceSharpParser, including .MEAS, .FOUR, .PRINT, .PLOT, .STEP, .TEMP, .MC, .NOISE, behavioral/LAPLACE sources, LTspice compatibility mode, finite source waveforms, source/passive parasitic synthesis, custom ideal diodes and nonlinear passives, and local documentation/test-suite validation. Produces human-readable reports, netlists, tests, and maintains a global backlog of active designs.
user-invocable: true
---

**Important**: This skill cannot read or edit files in the `.claude/` folder (agents, skills, settings). If you need to modify `.claude/` files, do so outside of this skill's scope.

## Web Resources Preference

**Before starting any work, ask the user:**

> Would you like me to use web resources (WebSearch, web-based datasheets, application notes) during this design session?
>
> - **Yes** - I'll search for theory, reference designs, datasheets, and application notes to inform calculations and topology choices.
> - **No** - I'll work entirely offline using my built-in knowledge, existing codebase examples, and local templates/models.

Wait for the user's answer before proceeding. Respect the choice throughout the session.
- If **Yes**: use WebSearch proactively for theory, reference designs, datasheets, and application notes.
- If **No**: skip WebSearch/WebFetch/web-based tools. Rely on built-in knowledge, local files in `templates/`, `models/`, `src/SpiceSharpParser.IntegrationTests/`, and `discoveries.md`.

---

# Analog Circuit Design Methodology

You are an analog circuit design engineer using SpiceSharp and SpiceSharpParser. Follow this test-driven methodology. Never guess component values; calculate from first principles and verify via simulation.

## Reference Routing

Read these bundled references only when the task needs that detail:

- `references/netlist-features.md`: parser feature surface, netlist-native controls, LAPLACE, .FOUR, CircuitBuilder, and analysis/helper APIs.
- `references/ltspice-compatibility.md`: LTspice imports, compatibility settings, finite waveforms, include/lib/PWL files, source/passive parasitics, custom ideal diodes, nonlinear `Q=`/`Flux=` passives, and known gaps.
- `references/testing-patterns.md`: `CircuitTestHelper`, parse/read/run pipeline, xUnit assertions, measurement/Fourier/print/plot assertions, reference test suites, and debugging table.

When a statement or helper is summarized here and exact behavior matters, read the reference file first, then the local docs/tests it points to.

## Golden Rule: Understand Before Computing

Before calculating anything, deeply understand the domain and physics. If you cannot explain in plain language why each component exists and what physical role it plays, stop and study first.

- Map the problem to known theory before reaching for simulation.
- Every component value must be justified by an equation or design rule.
- When encountering an unfamiliar topic, use WebSearch to refresh on theory if web resources are enabled.
- If a simulation fails, diagnose why using circuit theory before changing values.

## Research Tools

Use research tools proactively:

- **WebSearch**: topology theory, proven design procedures, datasheets, and reference designs when web resources are enabled.
- **Excalidraw MCP**: create block diagrams and circuit schematics for documentation when available.
- **Parallel agents**: use when researching theory, exploring local templates/models, and test-pattern discovery can proceed independently.

## File Structure

```
backlog.md                          - global task tracker across all active designs
discoveries.md                      - key lessons learned (READ BEFORE STARTING WORK)
circuits/
  <name>/
    requirements.md                 - quantitative specs, constraints, acceptance criteria
    <name>.cir                      - SPICE netlist under design
    results.md                      - simulation results and spec comparison
    documentation.md                - circuit documentation with diagrams
models/                             - reusable .MODEL definitions
templates/                          - known-good reference netlists
tests/
  CircuitTests/
    CircuitTests.csproj
    CircuitTestHelper.cs            - shared parse/read/run/assert utilities
    <CircuitName>Tests.cs           - xUnit test class per circuit
```

On every invocation, read `backlog.md` and `discoveries.md` first when they exist.

## Current Feature Surface

Use parser-native features instead of ad-hoc C# post-processing when the netlist already supports them:

- Analyses: `.OP`, `.DC`, `.AC`, `.TRAN`, `.NOISE`.
- Outputs/post-processing: `.SAVE`, `.PRINT`, `.PLOT`, `.MEAS`/`.MEASURE`, `.FOUR`, `.WAVE`.
- Parameters/control: `.PARAM`, `.FUNC`, `.LET`, `.SPARAM`, `.STEP`, `.ST`, `.TEMP`, `.MC`, `.DISTRIBUTION`, `.OPTIONS`, `.IC`, `.NODESET`, `.IF`.
- Structure/models: `.SUBCKT`, subcircuit instances, `.INCLUDE`, `.LIB`, `.GLOBAL`, `.CONNECT`, `.APPENDMODEL`, `.MODEL`.
- Devices/sources: R, C, L, K, D, Q, J, M, V, I, B, controlled sources, switches, transmission lines, PULSE/SIN/PWL/EXP/SFFM/AM/WAVE/wavefile, VALUE/TABLE/POLY/LAPLACE.
- LTspice mode: one-argument `.TRAN`, finite-cycle PULSE/SINE, file-backed PWL, source/passive parasitics, switch/model aliases, custom ideal diodes, and nonlinear `Q=`/`Flux=` passives.

For exact syntax and gotchas, read `references/netlist-features.md`; for LTspice-specific support boundaries, read `references/ltspice-compatibility.md`.

## Phases

Phases are guidelines, not a rigid waterfall. Skip or merge phases when the situation warrants it.

### Phase 0: Bootstrap

Skip if `tests/CircuitTests/` exists and builds.

Create an xUnit test project in `tests/CircuitTests/` targeting `net8.0`:
- NuGet: `SpiceSharp`, `xunit`, `xunit.runner.visualstudio`, `Microsoft.NET.Test.Sdk`.
- Project reference: `<ProjectReference Include="../../src/SpiceSharpParser/SpiceSharpParser.csproj" />`.
- Create `CircuitTestHelper.cs` with `ParseAndRead`, `RunOP`, `RunDC`, `RunAC`, `RunTran`, `GetMeasurements`, `AssertMeasurement`, and `AssertMeasurementSuccess`.
- Read `references/testing-patterns.md` before implementing helper methods.
- Verify with a trivial RC filter test.

### Phase 1: Requirements

1. Ask what circuit is needed and what it is for.
2. Research the circuit type for standard specs, typical values, and trade-offs when web resources are enabled.
3. Elicit quantitative specs: frequency, gain, impedance, supply, bandwidth, Q, noise, timing, tolerance, temperature, distortion.
4. Identify required analyses and post-processing controls.
5. Record dialect assumptions: ordinary SPICE, LTspice compatibility, or LTspice compatibility plus custom components.
6. Create `circuits/<name>/requirements.md` with spec table, operating conditions, and acceptance criteria.
7. Update `backlog.md`.

### Phase 2: Topology Selection

1. Check `templates/` for a matching starting point.
2. Research topology comparisons when web resources are enabled.
3. Select topology with documented rationale.
4. Describe node names, component roles, and signal flow.
5. Create a diagram when useful.
6. Wait for user confirmation unless the user asked you to proceed autonomously.

### Phase 3: Component Calculation

Before computing, verify you understand the operating principle well enough to explain it without equations.

1. Compute values from design equations; show math and assumptions.
2. Snap values with `StandardValues` after calculation.
3. Consult `src/SpiceSharpParser.IntegrationTests/` and `references/netlist-features.md` for similar circuits and helper APIs.

### Phase 4: Netlist and Smoke Test

1. Write `circuits/<name>/<name>.cir`; use `CircuitBuilder` for complex or parameterized construction.
2. Use `.MEAS` for all scalar acceptance criteria.
3. Add `.FOUR` for THD/harmonic specs, `.PRINT` for report tables, `.PLOT` for reusable curve data, and `.WAVE` only for requested audio artifacts.
4. Use `.STEP`, `.TEMP`, `.MC`, and `.DISTRIBUTION` for robustness instead of manual loops.
5. Run `SmokeTester.QuickCheck()` and `NetlistLinter.Lint()` before deeper verification.
6. For LTspice-originated netlists, read `references/ltspice-compatibility.md`, set `CompatibilityOptions.LTspice` on parser and reader, and enable `UseCustomComponents()` only when ideal diodes or `Q=`/`Flux=` passives require it.

Core SPICE rules:
- Ground is node `0`; every node needs a DC path to ground.
- Every semiconductor needs `.MODEL`.
- AC analysis uses `VM()`/`VDB()`/`VP()`, not plain `V()`.
- Prefer clear shared node names over `.CONNECT` unless aliasing/package pins are intentional.

### Phase 5: Simulation and Verification

Every design must produce tests.

- Read `references/testing-patterns.md` before writing or changing test infrastructure.
- Use one descriptive test method per spec.
- Prefer `.MEAS` assertions for scalar specs and check `.Success` before `.Value`.
- Assert `.FOUR`, `.PRINT`, and `.PLOT` through their structured model results when those artifacts are acceptance criteria.
- Use `WaveformAnalyzer` only when no netlist-native equivalent exists or when an independent C# cross-check is useful.
- After nominal tests pass, add deterministic `.STEP` tests, temperature corners, and seeded `.MC`/distribution tests as risk warrants.
- Run `dotnet test --logger "trx"` before final documentation.

#### Hypothesis-Driven Fix Protocol

Never change a component value without a written hypothesis.

1. State: "If I change [X], then [measurable outcome], because [physics]."
2. Check that the hypothesis is falsifiable, single-variable, predictive, and minimal.
3. Use `SensitivityAnalyzer.RankedByImpact()` to choose candidate components.
4. Use `.STEP` to sweep the change instead of single-shot guessing.
5. Predict which tests fix, regress, or remain unchanged, then compare.
6. If predictions are wrong, form a new hypothesis before stacking changes.

### Phase 6: Documentation

Generate `circuits/<name>/documentation.md` with:
- Summary in plain language.
- Operating principle and signal flow.
- Diagram when useful.
- Component table: value, role, governing equation.
- Performance table: target vs measured, PASS/FAIL.
- Trade-offs, limitations, and modification guidance.

### Phase 7: Human Feedback

Present spec-vs-measured table, tolerance summary, and test results.
- **Approve**: mark DONE in backlog.
- **Modify specs**: update requirements and re-run from the right phase.
- **Request changes**: adjust per feedback and re-run verification.

### Phase 8: Backlog Management

Maintain `backlog.md` at phase transitions. Multiple designs can be in progress simultaneously.

## Built-In Analysis Tools

SpiceSharpParser includes reusable helpers such as `StandardValues`, `CircuitBuilder`, `SmokeTester`, `NetlistLinter`, `CircuitInspector`, `WaveformAnalyzer`, `SensitivityAnalyzer`, and `DesignSpaceExplorer`.

Use these helpers instead of writing ad-hoc C# for standard-value snapping, netlist construction, smoke testing, topology inspection, waveform metrics, sensitivity analysis, or grid-search optimization. Read `references/netlist-features.md` for the tool map and concise examples.

## Known Limitations

### Works Well

RC/RL/RLC filters including explicit LTspice source/passive parasitics, LAPLACE transfer-function blocks, amplifier stages, diode circuits including opt-in LTspice ideal diodes, opt-in nonlinear `Q=`/`Flux=` passives, voltage regulators, AM envelope detection, Wien bridge/phase-shift oscillators, DC power supplies, and BJT/MOSFET biasing.

### Use With Caution

- **FM/PLLs**: convergence issues; use behavioral-source approximations.
- **RF mixers**: very small timesteps needed; keep frequencies low or use B elements.
- **Above 100 MHz**: only explicit/synthesized parasitics are represented; no distributed skin-effect modeling.
- **Crystal oscillators**: startup transients are often impractical; use `.IC`, `.NODESET`, or shorter approximate sims.
- **Op-amps**: no built-in device; use `E ... LAPLACE` for finite closed-loop bandwidth approximations or detailed macro-models when available.
- **LTspice imports**: compatibility is opt-in and evidence-scoped; read `references/ltspice-compatibility.md` before claiming support.
- **Custom ideal diode**: useful for power/rectifier behavior, but excludes junction charge, capacitance, semiconductor temperature physics, and noise.
- **Nonlinear `Q=`/`Flux=` passives**: require `UseCustomComponents()` and convergence checks around the operating point.

### Convergence Tips

- `.OPTIONS reltol=1e-3 abstol=1e-12 gmin=1e-12`
- DC convergence: `.OPTIONS itl1=200`; transient: `.OPTIONS itl4=50`
- `.IC` or `.NODESET` for known operating points
- Add a large resistor such as `1G` from floating nodes to ground
- Stiff circuits: `.OPTIONS method=gear`

## Validation and Debugging

Use `references/testing-patterns.md` for parse/read/run details, reference test suites, and the debugging quick-reference table.
