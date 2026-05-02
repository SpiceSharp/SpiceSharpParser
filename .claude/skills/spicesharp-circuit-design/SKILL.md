---
name: spicesharp-circuit-design
description: Structured R&D problem-solving with test-driven verification for designing analog circuits using SpiceSharp and SpiceSharpParser that produces human-readable reports, netlist and tests and maintains a global backlog of active designs.
user-invocable: true
---

**Important**: This skill cannot read or edit files in the `.claude/` folder (agents, skills, settings). If you need to modify `.claude/` files, do so outside of this skill's scope.

## Web Resources Preference

**Before starting any work, ask the user:**

> Would you like me to use web resources (WebSearch, web-based datasheets, application notes) during this design session?
>
> - **Yes** — I'll search for theory, reference designs, datasheets, and application notes to inform calculations and topology choices.
> - **No** — I'll work entirely offline using my built-in knowledge, existing codebase examples, and local templates/models.

Wait for the user's answer before proceeding. Store their choice and respect it throughout the entire session:
- If **Yes**: use WebSearch proactively as described in the Research Tools section below.
- If **No**: skip all WebSearch calls. Do not use WebSearch, WebFetch, or any web-based tool. Rely on built-in knowledge, local files in `templates/`, `models/`, `src/SpiceSharpParser.IntegrationTests/`, and `discoveries.md`.

---

# Analog Circuit Design Methodology

You are an analog circuit design engineer using SpiceSharp and SpiceSharpParser. Follow this test-driven methodology. Never guess component values — calculate from first principles, verify via simulation.

## Golden Rule: Understand Before Computing

Before calculating anything, **deeply understand the domain and physics**. If you cannot explain in plain language *why* each component exists and *what physical role* it plays, stop and study first.

- Map the problem to known theory before reaching for simulation
- Every component value must be justified by an equation or design rule
- When encountering an unfamiliar topic, **use WebSearch** to refresh on theory before proceeding *(if web resources enabled)*
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

#### LAPLACE Transfer Sources

Use `LAPLACE` when a block is mostly linear and the desired transfer function is known: anti-alias poles, finite-bandwidth amplifier approximations, lead/lag compensation, simplified transconductance stages, sensor front ends, and other gain/pole/zero models.

Supported source-level spellings:

In the examples below, `E/G<name>` means choose either an `E` or `G` source prefix, and `F/H<name>` means choose either an `F` or `H` source prefix.

```spice
E/G<name> <out+> <out-> LAPLACE {V(<node>)} = {<transfer>} [M=<m>] [TD=<delay>|DELAY=<delay>]
E/G<name> <out+> <out-> LAPLACE {V(<node1>,<node2>)} {<transfer>} [M=<m>] [TD=<delay>|DELAY=<delay>]
E/G<name> <out+> <out-> LAPLACE = {V(<node1>,<node2>)} {<transfer>} [M=<m>] [TD=<delay>|DELAY=<delay>]

F/H<name> <out+> <out-> LAPLACE {I(<source>)} = {<transfer>} [M=<m>] [TD=<delay>|DELAY=<delay>]
F/H<name> <out+> <out-> LAPLACE {I(<source>)} {<transfer>} [M=<m>] [TD=<delay>|DELAY=<delay>]
F/H<name> <out+> <out-> LAPLACE = {I(<source>)} {<transfer>} [M=<m>] [TD=<delay>|DELAY=<delay>]
```

Function-style forms are also supported:

```spice
ELOW OUT 0 VALUE={LAPLACE(V(IN), 1/(1+s*tau))}
BLOW OUT 0 V={LAPLACE(V(IN), wc/(s+wc))}
BGM OUT 0 I={LAPLACE(V(IN), gm/(1+s*tau))}
BMIX OUT 0 V={1 + 2*LAPLACE(V(IN), 1/(1+s))}
BDELAY OUT 0 V={LAPLACE(V(IN), 1/(1+s*tau), M=2, TD=1n)}
BINHELP OUT 0 V={LAPLACE(2*V(IN), 1/(1+s))}
```

Rules and gotchas:
- `E` and `G` use voltage input `V(node)` or `V(node1,node2)`; `F` and `H` use current input `I(source)`.
- Function-style `LAPLACE(...)` accepts direct probes and arbitrary scalar input expressions; non-probe inputs are lowered through internal helper sources.
- The transfer must be a finite, proper rational polynomial in `s` with non-singular DC gain.
- Use `s/(s+wc)` for high-pass behavior; bare `s` is improper and rejected.
- Avoid unsupported forms such as `1/s`, `sin(s)`, source-level `V(a)-V(b)`, and `V(node)` on `F`/`H`.
- `M=<m>` is a finite multiplier folded into the numerator. It may be positive, negative, or zero.
- `TD=<delay>` and `DELAY=<delay>` are aliases; use only one, with assignment syntax, and a non-negative value.
- Function-style calls may pass inline `M=`, `TD=`, and `DELAY=` options; inline options apply only to that call, so multiple delayed calls are supported when each delay is inline.
- Source-level delay options still require exactly one `LAPLACE(...)` call.
- `G` and `F` current is defined from `out+` to `out-`; with a grounded load this may produce inverted output voltage.
- For details and examples, read `src/docs/articles/laplace.md`, `src/docs/articles/laplace-basics.md`, and `src/SpiceSharpParser.IntegrationTests/AnalogBehavioralModeling/LaplaceTests.cs`.

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

### Tool Details and Examples

#### StandardValues — Snap to Real Component Values

Snap calculated values to E12 (±10%), E24 (±5%), or E96 (±1%) standard resistor/capacitor series. Use in Phase 3 after computing values from design equations.

**Key methods:**
- `NearestE12(value)`, `NearestE24(value)`, `NearestE96(value)` — nearest standard value
- `BracketE12(value)`, `BracketE24(value)` — returns `(Below, Above)` tuple for manual selection
- `GetValuesInRange(min, max, series)` — all standard values in a range

```csharp
// Snap a calculated 4.85k resistor to standard E24
double R = StandardValues.NearestE24(4850); // => 4700

// Get bracketing values when you need to choose direction
var (below, above) = StandardValues.BracketE24(4850); // => (4700, 5100)

// Enumerate all E24 capacitor values between 1nF and 100nF
var caps = StandardValues.GetValuesInRange(1e-9, 100e-9, StandardValues.E24Multipliers);
// => [1.0e-9, 1.1e-9, 1.2e-9, ... 82e-9, 91e-9, 100e-9]
```

---

#### CircuitBuilder — Fluent Netlist Construction

Programmatically build netlists with a chainable API. Preferable to string concatenation for complex or parameterized circuits.

**Key methods:** `Resistor()`, `Capacitor()`, `Inductor()`, `VoltageSource()`, `VoltageSourceSine()`, `VoltageSourcePulse()`, `VoltageSourcePWL()`, `CurrentSource()`, `Diode()`, `BJT()`, `MOSFET()`, `JFET()`, `VCVS()`, `VCCS()`, `BehavioralVoltageSource()`, `BehavioralCurrentSource()`, `Model()`, `ModelRaw()`, `OP()`, `DC()`, `AC()`, `Tran()`, `Save()`, `Meas()`, `Print()`, `Param()`, `IC()`, `Options()`, `RawLine()`, `SetValue()`, `RemoveComponent()`, `ToNetlist()`, `Build()`

```csharp
// Build a bandpass filter netlist
var netlist = CircuitBuilder.Create("Bandpass Filter")
    .VoltageSource("Vin", "in", "0", dc: 0, ac: 1)
    .Resistor("R1", "in", "mid", 10e3)
    .Capacitor("C1", "mid", "out", 10e-9)
    .Resistor("R2", "out", "0", 10e3)
    .Capacitor("C2", "out", "0", 10e-9)
    .AC("DEC", 100, 1, 1e6)
    .Save("VDB(out)", "VP(out)")
    .Meas("AC", "peak_gain", "MAX VDB(out)")
    .Meas("AC", "f_center", "WHEN VDB(out)=MAX")
    .ToNetlist();

// Build and parse in one step
var model = CircuitBuilder.Create("RC Filter")
    .VoltageSource("V1", "in", "0", dc: 0, ac: 1)
    .Resistor("R1", "in", "out", 1e3)
    .Capacitor("C1", "out", "0", 159e-9)
    .AC("DEC", 50, 1, 1e6)
    .Save("VDB(out)")
    .Build();  // returns SpiceSharpModel directly

// Modify an existing builder — change R1 and remove C2
builder.SetValue("R1", 15e3).RemoveComponent("C2");

// BJT amplifier with model and initial conditions
var amp = CircuitBuilder.Create("CE Amplifier")
    .ModelRaw(".MODEL 2N2222 NPN(BF=200 IS=1e-14 VAF=100)")
    .VoltageSource("VCC", "vcc", "0", dc: 12)
    .VoltageSourceSine("Vin", "in", "0", 0, 10e-3, 1e3)
    .Resistor("RC", "vcc", "col", 4.7e3)
    .Resistor("RB", "vcc", "base", 470e3)
    .BJT("Q1", "col", "base", "0", "2N2222")
    .Capacitor("Cin", "in", "base", 10e-6)
    .Tran(1e-6, 5e-3)
    .Save("V(col)", "V(base)")
    .IC("col", 6.0)
    .Options("reltol=1e-3")
    .ToNetlist();
```

---

#### SmokeTester — Quick Parse + Lint + OP Check

One-call validation: parses the netlist, runs the linter, attempts an OP simulation, and reports node voltages and device operating regions. Use immediately after writing a netlist.

**Key types:**
- `SmokeTestResult.IsPass` — `true` if parse + lint (no errors) + OP all succeed
- `SmokeTestResult.NodeVoltages` — `Dictionary<string, double>` of DC operating point
- `SmokeTestResult.DeviceRegions` — `Dictionary<string, DeviceRegion>` (Active/Saturation/Cutoff/Linear)
- `SmokeTestResult.DiagnosticSummary()` — human-readable string report

```csharp
// Quick-check a netlist string (most common usage)
var result = SmokeTester.QuickCheck(netlist);

if (!result.IsPass)
{
    _output.WriteLine(result.DiagnosticSummary());
    // Shows parse errors, lint issues, convergence errors
}

// Inspect DC bias points
Assert.True(result.OPConverges, result.ConvergenceError);
double vCollector = result.NodeVoltages["col"];   // e.g., 6.2V
double vBase = result.NodeVoltages["base"];       // e.g., 0.7V

// Check device operating regions
Assert.Equal(DeviceRegion.Active, result.DeviceRegions["Q1"]);

// Also accepts an already-parsed model
var model = CircuitTestHelper.ParseAndRead(netlist);
var result2 = SmokeTester.QuickCheck(model);
```

---

#### NetlistLinter — Structural Validation

Detects structural problems in a parsed model: floating nodes, missing DC paths, missing `.MODEL` definitions, duplicate components, missing AC magnitudes, large capacitors without `.TRAN` maxstep, empty circuits, and missing simulation commands.

**Key types:**
- `LintResult.HasErrors` / `LintResult.HasWarnings` — quick checks
- `LintResult.Errors` / `LintResult.Warnings` — filtered issue lists
- `LintIssue.Category` — one of: `FloatingNode`, `MissingDCPath`, `MissingModel`, `DuplicateComponent`, `MissingACMagnitude`, `MissingTranMaxStep`, `EmptyCircuit`, `NoSimulation`, `NoExports`
- `LintIssue.SuggestedFix` — actionable fix suggestion (may be null)

```csharp
var model = CircuitTestHelper.ParseAndRead(netlist);
var lint = NetlistLinter.Lint(model);

// Quick pass/fail check
Assert.False(lint.HasErrors, lint.ToString());

// Inspect specific warnings
foreach (var warning in lint.Warnings)
{
    _output.WriteLine($"[{warning.Category}] {warning.Message}");
    if (warning.SuggestedFix != null)
        _output.WriteLine($"  Fix: {warning.SuggestedFix}");
}

// Filter by category
var dcPathIssues = lint.Issues
    .Where(i => i.Category == LintCategory.MissingDCPath);

// Common issue: node "out" has no DC path to ground
// LintIssue: Severity=Warning, Category=MissingDCPath,
//   Message="Node 'out' has no DC path to ground",
//   SuggestedFix="Add a large resistor (1GΩ) from 'out' to ground"
```

---

#### CircuitInspector — Topology and Bias Queries

Instance-based inspector for querying circuit structure, reading/writing component values, and checking semiconductor operating regions.

**Key methods:**
- `GetNodes()` — all node names
- `GetComponentNames()` — all component names
- `GetComponentsConnectedTo(node)` — components touching a node
- `GetComponentInfo(name)` — full details (type, nodes, parameters, model)
- `GetComponentValue(name)` / `SetComponentValue(name, value)` — R/L/C values
- `GetBJTRegion(name, nodeVoltages)` — Active/Saturation/Cutoff
- `GetMOSFETRegion(name, nodeVoltages, vth)` — Cutoff/Linear/Saturation
- `GetComponentCounts()` — summary by type

```csharp
var model = CircuitTestHelper.ParseAndRead(netlist);
var inspector = new CircuitInspector(model);

// Enumerate topology
var nodes = inspector.GetNodes();           // ["0", "in", "out", "vcc", "base", "col"]
var components = inspector.GetComponentNames(); // ["V1", "R1", "R2", "C1", "Q1", ...]

// What's connected to the output node?
var atOutput = inspector.GetComponentsConnectedTo("out");
// => ["R2", "C1"]

// Read component details
var info = inspector.GetComponentInfo("R1");
// info.Type = "Resistor", info.Nodes = ["in", "mid"],
// info.Parameters = { "resistance": 10000.0 }

// Read/write passive values
double rVal = inspector.GetComponentValue("R1"); // 10000.0
inspector.SetComponentValue("R1", 12000.0);      // modify in-place

// Check BJT region using node voltages from SmokeTester
var smoke = SmokeTester.QuickCheck(netlist);
var region = inspector.GetBJTRegion("Q1", smoke.NodeVoltages);
// => DeviceRegion.Active

// MOSFET region with custom threshold
var mosRegion = inspector.GetMOSFETRegion("M1", smoke.NodeVoltages, vth: 1.0);
// => DeviceRegion.Saturation

// Circuit summary
var counts = inspector.GetComponentCounts();
// => { "Resistor": 4, "Capacitor": 2, "BJT": 1, "VoltageSource": 2 }
```

---

#### WaveformAnalyzer — Post-Simulation Metrics

All-static methods for analyzing simulation output waveforms. Data is passed as `List<(double, double)>` tuples collected from simulation exports.

**Time-domain methods:**
- `RiseTime(data, lowPct=0.1, highPct=0.9)` — 10%-90% rise time
- `FallTime(data, highPct=0.9, lowPct=0.1)` — 90%-10% fall time
- `SettlingTime(data, finalValue, tolerancePct=0.02)` — time to stay within ±2% of final value
- `Overshoot(data, finalValue)` — percentage overshoot
- `PeakToPeak(data, fromTime, toTime)` — peak-to-peak in optional time window
- `RMS(data, fromTime, toTime)` — RMS value (trapezoidal integration)
- `Average(data, fromTime, toTime)` — average value
- `DCOffset(data)` — average over entire waveform

**Frequency-domain methods:**
- `FFT(data)` — returns `List<(Frequency, Magnitude)>` (Cooley-Tukey radix-2)
- `THD(data, fundamentalFreq, numHarmonics=10)` — total harmonic distortion as %
- `SNR(data, signalFreq)` — signal-to-noise ratio in dB

**AC response methods:**
- `BandwidthFrom3dBPoints(data)` — -3dB bandwidth from gain-vs-frequency data
- `StabilityMargins(gain, phase)` — returns `(GainMarginDb, PhaseMarginDeg)`
- `GainAt(data, frequency)` — interpolated gain at a frequency
- `PhaseAt(data, frequency)` — interpolated phase at a frequency
- `FrequencyAtGain(data, gainDb)` — frequency where gain equals a value

**Utilities:**
- `InterpolateAt(data, x)` — linear interpolation
- `FindCrossing(data, threshold, occurrence=1)` — Nth threshold crossing

```csharp
// === Time-domain analysis (transient simulation) ===
// Collect data from simulation exports
var tranData = new List<(double Time, double Value)>();
// ... populate from simulation run ...

double tRise = WaveformAnalyzer.RiseTime(tranData);         // 10%-90% rise time
double tFall = WaveformAnalyzer.FallTime(tranData);         // 90%-10% fall time
double tSettle = WaveformAnalyzer.SettlingTime(tranData, finalValue: 3.3, tolerancePct: 0.02);
double overshoot = WaveformAnalyzer.Overshoot(tranData, finalValue: 3.3);  // percent
double vpp = WaveformAnalyzer.PeakToPeak(tranData, fromTime: 1e-3, toTime: 5e-3);
double vRms = WaveformAnalyzer.RMS(tranData, fromTime: 1e-3, toTime: 5e-3);

// === Frequency-domain analysis (from time-domain data) ===
var spectrum = WaveformAnalyzer.FFT(tranData);
double thd = WaveformAnalyzer.THD(tranData, fundamentalFreq: 1000, numHarmonics: 5);
double snr = WaveformAnalyzer.SNR(tranData, signalFreq: 1000);

// === AC response analysis (from AC simulation) ===
var gainData = new List<(double Freq, double GainDb)>();
var phaseData = new List<(double Freq, double PhaseDeg)>();
// ... populate from AC simulation exports (VDB, VP) ...

double bw = WaveformAnalyzer.BandwidthFrom3dBPoints(gainData);  // -3dB bandwidth in Hz
var (gm, pm) = WaveformAnalyzer.StabilityMargins(gainData, phaseData);
// gm = gain margin in dB, pm = phase margin in degrees (positive = stable)

double gainAt1k = WaveformAnalyzer.GainAt(gainData, 1000);      // gain at 1kHz in dB
double phaseAt1k = WaveformAnalyzer.PhaseAt(phaseData, 1000);   // phase at 1kHz in degrees
double fUnity = WaveformAnalyzer.FrequencyAtGain(gainData, 0);  // unity-gain frequency

// === General utilities ===
double tCross = WaveformAnalyzer.FindCrossing(tranData, threshold: 2.5, occurrence: 3);
// Time of 3rd crossing of 2.5V threshold
```

---

#### SensitivityAnalyzer — Find Which Component Matters

Computes normalized sensitivity of a `.MEAS` result to each passive component using central finite differences (±perturbation). A sensitivity of 1.0 means a 1% change in the component causes a 1% change in the spec.

**Key types:**
- `ComponentSensitivity` — `.Sensitivity` (normalized), `.SpecAtMinus`, `.SpecAtPlus`, `.NominalComponentValue`
- `SensitivityResult` — `.NominalValue`, `.Sensitivities` dict, `.RankedByImpact()` sorted list

```csharp
// Netlist must include .MEAS directives for the specs you want to analyze
string netlist = @"
    Bandpass Filter
    V1 in 0 DC 0 AC 1
    R1 in mid 10k
    C1 mid out 10n
    R2 out 0 10k
    C2 out 0 10n
    .AC DEC 100 1 1MEG
    .MEAS AC peak_gain MAX VDB(out)
    .MEAS AC center_freq WHEN VDB(out)=MAX
    .END";

var analyzer = new SensitivityAnalyzer(netlist);

// Which components most affect center frequency?
var result = analyzer.ComputeSensitivity("center_freq", perturbationPct: 1.0);

_output.WriteLine($"Nominal center freq: {result.NominalValue} Hz");
foreach (var (comp, sens) in result.RankedByImpact())
{
    _output.WriteLine($"  {comp}: sensitivity = {sens:F3}");
}
// Example output:
//   Nominal center freq: 15915 Hz
//   C1: sensitivity = -1.002   (1% increase in C1 => ~1% decrease in freq)
//   R1: sensitivity = -0.498
//   C2: sensitivity = -0.501
//   R2: sensitivity = 0.003    (negligible effect)

// Single-component check
double dGain_dR1 = analyzer.ComputePartialDerivative("peak_gain", "R1", perturbationPct: 2.0);
```

---

#### DesignSpaceExplorer — Multi-Parameter Optimization

Grid-search optimizer that sweeps component values, evaluates `.MEAS` results, and finds the best feasible design point. Uses logarithmic spacing for ranges > 1 decade.

**Fluent API:**
- `AddParameter(componentName, min, max, steps=10)` — parameter to sweep
- `AddObjective(measurementName, target, weight=1.0)` — minimize distance to target
- `AddConstraint(measurementName, min, max)` — hard constraint on result
- `Explore()` — run grid search, returns `ExplorationResult`

**Key types:**
- `DesignPoint` — `.ComponentValues`, `.MeasurementValues`, `.ObjectiveScore`, `.ConstraintsSatisfied`
- `ExplorationResult` — `.Best` (best feasible point), `.AllPoints`, `.FeasibleCount`, `.BestValues`

```csharp
string netlist = @"
    Bandpass Filter
    V1 in 0 DC 0 AC 1
    R1 in mid 10k
    C1 mid out 10n
    R2 out 0 10k
    C2 out 0 10n
    .AC DEC 100 1 1MEG
    .MEAS AC peak_gain MAX VDB(out)
    .MEAS AC bw TRIG VDB(out) VAL=-3 RISE=1 TARG VDB(out) VAL=-3 FALL=1
    .END";

var explorer = new DesignSpaceExplorer(netlist);

var result = explorer
    .AddParameter("R1", 1e3, 100e3, steps: 10)   // sweep R1 from 1k to 100k
    .AddParameter("C1", 1e-9, 100e-9, steps: 10)  // sweep C1 from 1nF to 100nF
    .AddObjective("bw", target: 5000, weight: 1.0) // want 5kHz bandwidth
    .AddConstraint("peak_gain", min: -6, max: 0)   // gain between -6dB and 0dB
    .Explore();

if (result.Best != null)
{
    _output.WriteLine($"Best design (score={result.Best.ObjectiveScore:F4}):");
    foreach (var (comp, val) in result.BestValues)
        _output.WriteLine($"  {comp} = {val}");
    foreach (var (meas, val) in result.Best.MeasurementValues)
        _output.WriteLine($"  {meas} = {val}");
}
_output.WriteLine($"Feasible points: {result.FeasibleCount} / {result.AllPoints.Count}");

// Use the best values in the final design
double bestR1 = StandardValues.NearestE24(result.BestValues["R1"]);
double bestC1 = StandardValues.NearestE24(result.BestValues["C1"]);
```

---

### Typical Workflow: Combining Tools

```csharp
// 1. Build netlist programmatically
var netlist = CircuitBuilder.Create("Low-Pass Filter")
    .VoltageSource("V1", "in", "0", dc: 0, ac: 1)
    .Resistor("R1", "in", "out", StandardValues.NearestE24(1590))  // snap to E24
    .Capacitor("C1", "out", "0", StandardValues.NearestE24(100e-9))
    .AC("DEC", 100, 1, 1e6)
    .Meas("AC", "f3db", "WHEN VDB(out)=-3")
    .Save("VDB(out)", "VP(out)")
    .ToNetlist();

// 2. Smoke test — parse, lint, and check OP convergence
var smoke = SmokeTester.QuickCheck(netlist);
Assert.True(smoke.IsPass, smoke.DiagnosticSummary());

// 3. Inspect topology and bias
var inspector = new CircuitInspector(smoke.Model);
Assert.Equal(2, inspector.GetComponentsConnectedTo("out").Count);

// 4. Run AC simulation and analyze response
// ... (run simulation, collect gain/phase data) ...
double bw = WaveformAnalyzer.BandwidthFrom3dBPoints(gainData);

// 5. If spec not met, find which component to adjust
var sensitivity = new SensitivityAnalyzer(netlist);
var impact = sensitivity.ComputeSensitivity("f3db");
var topComponent = impact.RankedByImpact().First().Component;

// 6. Optimize the most impactful components
var explorer = new DesignSpaceExplorer(netlist)
    .AddParameter("R1", 1e3, 10e3, steps: 15)
    .AddParameter("C1", 10e-9, 1e-6, steps: 15)
    .AddObjective("f3db", target: 1000)
    .Explore();
```

---

## Known Limitations

### Works Well
RC/RL/RLC filters, LAPLACE transfer-function blocks, amplifier stages (CE/CB/CC/CS/diff pair), diode circuits, voltage regulators, AM envelope detection, Wien bridge/phase-shift oscillators, DC power supply, BJT/MOSFET biasing

### Use With Caution
- **FM/PLLs** — convergence issues, use behavioral-source approximations
- **RF Mixers** — very small timesteps needed, keep frequencies low or use B elements
- **>100 MHz** — no parasitic/skin effect modeling, treat as approximate
- **Crystal oscillators** — startup transients impractical, use `.IC` or short sims
- **Op-amps** — no built-in device; use `E ... LAPLACE` for finite closed-loop bandwidth approximations, or use detailed behavioral/transistor models for full macro-model behavior

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
- **LAPLACE**: `src/docs/articles/laplace.md`, `src/docs/articles/laplace-basics.md`, `src/SpiceSharpParser.IntegrationTests/AnalogBehavioralModeling/LaplaceTests.cs`

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
