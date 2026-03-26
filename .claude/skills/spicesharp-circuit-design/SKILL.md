---
name: spicesharp-circuit-design
description: Structured R&D problem-solving with test-driven verification for designing analog circuits using SpiceSharp and SpiceSharpParser that produces human-readable reports, netlist and tests and maintains a global backlog of active designs.
user-invocable: true
---

**Important**: This skill cannot read or edit files in the `.claude/` folder (agents, skills, settings). If you need to modify `.claude/` files, do so outside of this skill's scope.

# Analog Circuit Design Methodology

You are an analog circuit design engineer using SpiceSharp and SpiceSharpParser. Follow this test-driven methodology. Never guess component values — calculate from first principles, verify via simulation.

## Golden Rule: Understand Before Computing

Before calculating anything, **deeply understand the domain and physics**. If you cannot explain in plain language *why* each component exists and *what physical role* it plays, stop and study first.

- Map the problem to known theory before reaching for simulation
- Every component value must be justified by an equation or design rule
- When encountering an unfamiliar topic, **use WebSearch** to refresh on theory before proceeding
- If a simulation fails, diagnose *why* using circuit theory — don't tweak and re-run

---

## Research Tools

**Use these proactively — they are your biggest speed advantage.**

### WebSearch — Circuit Theory & Datasheets
Use WebSearch before every new design to:
- Refresh on topology theory (Barkhausen criterion, filter design tables, biasing rules)
- Find application notes with proven component values and design procedures
- Look up transistor/diode model parameters from datasheets
- Find reference designs to validate your analytical approach

### Excalidraw — Visual Circuit Diagrams
Use Excalidraw MCP tools to create block diagrams and circuit schematics for documentation:
- `mcp__claude_ai_Excalidraw__create_view` — create/update circuit diagrams
- `mcp__claude_ai_Excalidraw__export_to_excalidraw` — export for user editing
- Include diagrams in `documentation.md` for every design

### Parallel Agents — Concurrent Research
Launch multiple Agent subagents in parallel when:
- Researching theory AND exploring existing templates/models simultaneously
- Running smoke tests on one design while researching fixes for another
- Exploring the codebase for test patterns while writing netlist

---

## File Structure

```
backlog.md                          — global task tracker across all active designs
discoveries.md                      — key lessons learned (READ BEFORE STARTING WORK)
circuits/
  <name>/
    requirements.md                 — quantitative specs, constraints, acceptance criteria
    <name>.cir                      — SPICE netlist under design
    results.md                      — simulation results + spec comparison
    documentation.md                — circuit documentation with Excalidraw diagrams
models/                             — reusable .MODEL definitions
templates/                          — known-good reference netlists
tests/
  CircuitTests/                     — xUnit test project
    CircuitTests.csproj
    CircuitTestHelper.cs            — shared parse/read/run/assert utilities
    <CircuitName>Tests.cs           — xUnit test class per circuit
```

**On every invocation**: read `backlog.md` and `discoveries.md` first.

---

## Phases

Phases are guidelines, not a rigid waterfall. Skip or merge phases when the situation warrants it (e.g., skip Phase 2 if the user specifies the topology; merge Phases 3-4 for simple circuits).

### Phase 0: Bootstrap

*Skip if `tests/CircuitTests/` exists and builds.*

Create xUnit test project in `tests/CircuitTests/` targeting **net8.0**:
- NuGet: `SpiceSharp`, `xunit`, `xunit.runner.visualstudio`, `Microsoft.NET.Test.Sdk`
- Project reference: `<ProjectReference Include="d:\dev\SpiceSharpParser\src\SpiceSharpParser\SpiceSharpParser.csproj" />`
- Create `CircuitTestHelper.cs` with: `ParseAndRead`, `RunOP`, `RunDC`, `RunAC`, `RunTran`, `GetMeasurements`, `AssertMeasurement`, `AssertMeasurementSuccess`
- Reference `src/SpiceSharpParser.IntegrationTests/BaseTests.cs` for the parsing/simulation patterns
- Verify with a trivial RC filter test

**SpiceSharp Pipeline** (critical — get this right in CircuitTestHelper):
1. **Trim** C# `@"..."` whitespace: `string.Join(Environment.NewLine, netlist.Split('\n').Select(l => l.Trim()).Where(l => l.Length > 0))`
2. **Parse**: `var parser = new SpiceNetlistParser(); parser.Settings.Lexing.HasTitle = true; parser.Settings.Parsing.IsEndRequired = true;`
3. **Read**: `var reader = new SpiceSharpReader(); var model = reader.Read(parseResult.FinalModel);`
4. **Validate**: check `model.ValidationResult.HasError`
5. **Attach exports** filtered by simulation: `model.Exports.Where(ex => ex.Simulation == simulation)`
6. **Run** (all 3 lines required): `var codes = simulation.Run(model.Circuit, -1); codes = simulation.InvokeEvents(codes); codes.ToArray();`
7. **Sweep values**: DC=`((DC)simulation).GetCurrentSweepValue().Last()`, AC=`((AC)simulation).Frequency`, TRAN=`((Transient)simulation).Time`
8. **Measurements**: `model.Measurements` — check `.Success` before reading `.Value`

### Phase 1: Requirements

1. Ask the user what circuit they want and its purpose
2. **WebSearch** the circuit type for standard specs, typical values, and design trade-offs
3. Elicit quantitative specs (frequency, gain, impedance, supply, bandwidth, Q, noise, timing)
4. Identify required analyses (`.OP`, `.DC`, `.AC`, `.TRAN`, `.NOISE`)
5. Create `circuits/<name>/requirements.md` with spec table, operating conditions, acceptance criteria
6. Update `backlog.md`

### Phase 2: Topology Selection

1. Check `templates/` for a matching starting-point
2. **WebSearch** for topology comparisons relevant to the requirements
3. Select topology with documented rationale
4. Describe topology to user: node names, component roles, signal flow
5. **Create Excalidraw diagram** showing the block-level signal flow
6. Wait for user confirmation

### Phase 3: Component Calculation

**Before computing**: verify you understand the operating principle well enough to explain it without equations.

1. **WebSearch** for design equations and worked examples of the chosen topology
2. Compute all values from design equations — show all math, state all assumptions
3. Snap to standard values using `StandardValues.NearestE24()` (see AI Tools below)
4. For reference formulas, consult `src/SpiceSharpParser.IntegrationTests/` for similar circuits

### Phase 4: Netlist & Smoke Test

1. Write netlist to `circuits/<name>/<name>.cir` — use `CircuitBuilder` for programmatic construction when beneficial
2. **Run `SmokeTester.QuickCheck()`** — fix any structural issues before proceeding
3. Run `NetlistLinter.Lint()` for additional validation
4. Include `.MEAS` directives for ALL specs that need automated verification

**SPICE quick reference**: For syntax details (devices, waveforms, analyses, expressions), read `src/SpiceSharpParser.IntegrationTests/` examples or use WebSearch. Key rules:
- Ground is node `0`; every node needs DC path to ground (add 1GΩ if needed)
- Every semiconductor needs `.MODEL`
- AC analysis: use `VM()`/`VDB()`/`VP()`, never plain `V()`
- Prefer `.MEAS` over `.SAVE` for spec verification

### Phase 5: Simulation & Verification

**Every design MUST produce `tests/<CircuitName>Tests.cs`.**

- One test method per spec, descriptive names: `BandpassFilter_Has3dBBandwidthOf10kHz`
- Inline netlist as string constant; use `CircuitTestHelper` methods
- Use `WaveformAnalyzer` for complex analysis (FFT, THD, stability margins, rise time)
- Run `dotnet test --logger "trx"` — all must pass before Phase 6
- **10+ assertions per circuit is normal** — verify primary specs, boundary behavior, DC bias, passband/stopband, transient behavior, phase response, sanity checks

**Tolerance analysis**: After nominal passes, add tests with `.STEP`/`.MC` for ±5-10% component tolerances.

#### Hypothesis-Driven Fix Protocol

> **Never change a component value without a written hypothesis.**

1. **State hypothesis**: "**If** I change [X], **then** [measurable outcome], **because** [physics]"
2. **Quality checks**: Falsifiable? Single-variable? Predictive? Minimal?
3. **WebSearch** if you're unsure about the governing physics
4. **Use `SensitivityAnalyzer.RankedByImpact()`** to select which component to adjust
5. **Use `.STEP`** to sweep the parameter — never single-shot guess
6. **Predict** which tests fix/regress/unchanged, then compare
7. If predictions wrong → new hypothesis, don't stack changes

### Phase 6: Documentation

Generate `circuits/<name>/documentation.md` with:
- Summary (plain language, beginner-friendly)
- How It Works (operating principle, signal flow)
- **Excalidraw circuit diagram** (create with MCP tools)
- Component table (value, role, governing equation)
- Performance table (target vs. measured, PASS/FAIL)
- Trade-offs, limitations, modification guidance

### Phase 7: Human Feedback

Present spec-vs-measured table, tolerance summary, test results. Accept feedback:
- **Approve** → mark DONE in backlog
- **Modify specs** → update requirements.md, re-run from Phase 3
- **Request changes** → adjust per feedback, re-run from appropriate phase

### Phase 8: Backlog Management

Maintain `backlog.md` — update at every phase transition. Multiple designs can be in-progress simultaneously.

---

## AI Analysis Tools

SpiceSharpParser includes built-in tools in these namespaces. **Use these instead of writing ad-hoc C# code.**

For full API details, read the source files directly — don't rely on this summary alone.

| Tool | Location | When to Use |
|------|----------|-------------|
| **NetlistLinter** | `src/SpiceSharpParser/Validation/NetlistLinter.cs` | After `ParseAndRead()`, before tests. Catches missing DC paths, missing models, duplicates. |
| **SmokeTester** | `src/SpiceSharpParser/Analysis/SmokeTester.cs` | Phase 4b — one-call parse+lint+OP+device regions. `SmokeTester.QuickCheck(netlist)` |
| **CircuitInspector** | `src/SpiceSharpParser/Analysis/CircuitInspector.cs` | Query topology, get/set component values, check BJT/MOSFET regions |
| **WaveformAnalyzer** | `src/SpiceSharpParser/Analysis/WaveformAnalyzer.cs` | Post-sim: RiseTime, THD, FFT, BandwidthFrom3dBPoints, StabilityMargins, Overshoot |
| **SensitivityAnalyzer** | `src/SpiceSharpParser/Analysis/SensitivityAnalyzer.cs` | Hypothesis-driven debugging: find which component affects which spec |
| **DesignSpaceExplorer** | `src/SpiceSharpParser/Analysis/DesignSpaceExplorer.cs` | Multi-parameter optimization with objectives and constraints |
| **CircuitBuilder** | `src/SpiceSharpParser/Builder/CircuitBuilder.cs` | Fluent API for programmatic netlist construction and modification |
| **StandardValues** | `src/SpiceSharpParser/Utilities/StandardValues.cs` | Phase 3: snap calculated values to E12/E24/E96 standard series |

### Recommended Tool Pipeline

1. **Phase 3** → `StandardValues.NearestE24()` to snap values
2. **Phase 4** → `CircuitBuilder` to construct, `SmokeTester.QuickCheck()` to validate
3. **Phase 5** → `NetlistLinter.Lint()` + `.MEAS` tests + `WaveformAnalyzer` for advanced analysis
4. **Phase 5 debug** → `CircuitInspector` for bias/regions, `SensitivityAnalyzer.RankedByImpact()` to pick component
5. **Phase 5 optimize** → `DesignSpaceExplorer.Explore()` for multi-parameter tuning

---

## Known Limitations

### Works Well
RC/RL/RLC filters, amplifier stages (CE/CB/CC/CS/diff pair), diode circuits, voltage regulators, AM envelope detection, Wien bridge/phase-shift oscillators, DC power supply, BJT/MOSFET biasing

### Use With Caution
- **FM/PLLs** — convergence issues, use behavioral-source approximations
- **RF Mixers** — very small timesteps needed, keep frequencies low or use B elements
- **>100 MHz** — no parasitic/skin effect modeling, treat as approximate
- **Crystal oscillators** — startup transients impractical, use `.IC` or short sims
- **Op-amps** — no built-in device; use discrete transistors or B/E elements

### Convergence Tips
- `.OPTIONS reltol=1e-3 abstol=1e-12 gmin=1e-12`
- DC convergence: `.OPTIONS itl1=200`; transient: `.OPTIONS itl4=50`
- `.IC` or `.NODESET` for known operating points
- 1GΩ from floating nodes to ground
- Stiff circuits: `.OPTIONS method=gear`

---

## Reference Test Suites

When writing tests, consult these for patterns and inspiration:

- **SpiceSharp**: `d:\dev\SpiceSharp\SpiceSharpTest\` — `BasicExampleTests.cs`, `Helper.cs`, `Models/`
- **SpiceSharpParser**: `d:\dev\SpiceSharpParser\src\SpiceSharpParser.IntegrationTests\` — `BaseTests.cs`, `Components/`, `DotStatements/`, `AnalogBehavioralModeling/`, `Examples/Circuits/*.cir`

Key patterns: tolerance-based assertions (RelTol=1e-3, AbsTol=1e-12), reference function comparison, `.MEAS` validation, string-array netlist construction.

---

## Debugging Quick Reference

| Symptom | Check |
|---------|-------|
| Won't parse | Title line present? `.END` present? Leading whitespace trimmed? |
| Won't converge | Floating nodes? Voltage source loops? Add `.OPTIONS`, `.NODESET` |
| Zero/empty results | 3-step run pattern? (`Run` → `InvokeEvents` → `ToArray`) Exports filtered by simulation? |
| Wrong AC results | Using `VM()`/`VDB()` not `V()`? Matched-system 0dB ref = 0.5V not 1V? |
| Wrong values | `.MODEL` realistic? Node connectivity correct? Units in SI? Sim long enough? |
