# Circuit Cookbook Plan

## Direction

SpiceSharpParser already has a broad feature surface. The next phase should
focus on proving what users can build with the existing parser and
`SpiceSharpParser.CustomComponents`, rather than adding more parser features or
component families.

The intended outcome is a curated circuit cookbook containing useful,
runnable, measured, and clearly documented designs.

## Existing Foundation

The repository contains the infrastructure needed for this direction:

- `SpiceSharpParser.AIExamples` contains 948 unique measured netlists. These
  provide candidates for human review and promotion into the cookbook.
- `circuits/a-devices` demonstrates a strong example format: runnable
  netlists, `.SAVE`, `.PLOT`, `.MEAS`, explanatory guides, and automated
  verification.
- `circuits/cookbook` contains four complete application-oriented recipes
  across the pure-SPICE and CustomComponents tracks.
- `tools/cookbook-schematic` renders deterministic, accessible SVG schematics
  from a SPICE netlist plus a compact `schematic.toml` layout. Its validator
  keeps displayed component and node names synchronized with the netlist.
- `tools/cookbook-report` provides a read-only cookbook quality gate. It checks
  required documentation, analysis and output directives, measurement-table
  coverage, layout topology, SVG accessibility, and generated schematic
  freshness. It can emit stable JSON for CI or another AI tool.
- Repository-local `cookbook-schematic` and `cookbook-report` skills document
  the corresponding AI workflows.
- The main documentation is comprehensive about syntax, analyses, and
  components, but it is not yet organized around practical things users can
  build.

The accepted AI examples should be treated as a quarry rather than published
as-is. Promoted circuits must receive an independent review of their topology,
equations, assumptions, measurements, and educational value.

## Current Implementation Status

The initial cookbook milestone is complete:

- Rectifier power supply, PWM DAC, ideal-diode power OR, and simple PLL recipes
  are present in the user-facing catalog.
- Every current recipe has a runnable `.cir`, full guide, checked-in
  `schematic.toml`, generated `schematic.svg`, and accessible `response.svg`.
- All current schematic layouts display every parsed netlist component and
  render reproducibly from their checked-in sources.
- The repository-local cookbook report passes without errors or warnings.
- The data-driven `CircuitCookbookTests` suite compiles and simulates all
  current recipes and enforces their measurement ranges.

The next circuit should be the BJT audio preamplifier. It expands the
pure-SPICE track with operating-point and AC analysis instead of adding another
transient-only example.

## Cookbook Tracks

Maintain two visible tracks.

### Pure SPICE

These circuits should use the core parser and portable SPICE constructs where
practical. They demonstrate that useful work does not require the optional
custom-components package.

### CustomComponents

These circuits may use ideal diodes, nonlinear passives, LTspice A-devices, or
the packaged analog, digital, and 555 subcircuits. They should demonstrate
complete applications rather than isolated component behavior.

## Candidate Circuit Roadmap

| Circuit | Foundation | Status | What it demonstrates |
| --- | --- | --- | --- |
| Rectifier, reservoir, and simple regulator | Pure SPICE | Complete | Diodes, ripple, load regulation, startup, and peak current |
| BJT audio preamplifier | Pure SPICE | Next | Bias point, AC gain, bandwidth, clipping, and Fourier distortion |
| PWM-to-analog converter | Pure SPICE | Complete | Switching, settling time, average output, and ripple |
| Transmission-line termination experiment | Pure SPICE | Planned | Reflections, propagation delay, and source/load matching |
| Ideal-diode redundant power input | CustomComponents | Complete | Supply OR-ing, failover, and reverse-current blocking |
| 555 monostable and PWM controller | Packaged subcircuit | Planned | Practical applications beyond the existing astable validation |
| Buck stage with a saturating inductor | Nonlinear `Flux=` inductor | Planned | Current ripple and the consequences of magnetic saturation |
| Sampled sensor alarm or data recorder | Sample-and-hold plus digital library | Planned | A complete analog-to-digital signal chain |
| Simple phase-locked loop | `PHASEDET`, loop filter, and `MODULATOR` | Complete | A mixed-signal system assembled from existing components |

## Completed Starting Sequence

The starting sequence established three complementary examples:

1. **Rectifier and filtered DC supply**
   Established the cookbook structure with an accessible, practical
   pure-SPICE circuit.

2. **PWM DAC**
   Added a compact transient example with clear measurements for output
   average, ripple, and settling time.

3. **Simple PLL**
   Demonstrated that CustomComponents can form a complete mixed-signal system,
   rather than only isolated device examples.

The ideal-diode redundant power input was the first follow-on application. The
next addition is the BJT audio preamplifier.

## Current Repository Structure and Growth Path

Keep the user-oriented catalog beside the source while leaving historical and
milestone directories intact.

```text
circuits/
  README.md
  cookbook/
    pure-spice/
      rectifier-power-supply/
        README.md
        rectifier-power-supply.cir
        schematic.toml
        schematic.svg
        response.svg
      pwm-dac/
        README.md
        pwm-dac.cir
        schematic.toml
        schematic.svg
        response.svg
    custom-components/
      ideal-diode-power-or/
        README.md
        ideal-diode-power-or.cir
        schematic.toml
        schematic.svg
        response.svg
      simple-pll/
        README.md
        simple-pll.cir
        schematic.toml
        schematic.svg
        response.svg
tools/
  cookbook-schematic/
  cookbook-report/
.agents/skills/
  cookbook-schematic/
  cookbook-report/
```

The root `circuits/README.md` is the searchable cookbook catalog. It should
classify circuits by application, required package, analysis type, and
difficulty.

Implementation-oriented examples such as `digital-milestone-a` can remain as
regression and design-history material. The catalog should present new
application-oriented names to users.

If the cookbook eventually grows beyond roughly 15 to 20 substantial designs,
or its visual assets begin to dominate the repository, consider moving it to a
dedicated examples repository or documentation site. Until then, keeping the
circuits beside the source makes regression testing and compatibility review
easier.

## Standard Circuit Documentation

Every circuit `README.md` should use the same order.

1. **Purpose**
   State the practical problem solved by the circuit.

2. **Design targets**
   List supply range, input range, target gain or timing, load, frequency, and
   other important constraints.

3. **Requirements and compatibility**
   Identify the package, dialect, analyses, difficulty, and known simulator
   compatibility.

4. **Schematic**
   Include a readable, accessible SVG with component and node names matching
   the netlist. Keep layout intent in `schematic.toml`, generate
   `schematic.svg` with `cookbook-schematic`, and do not hand-edit the generated
   SVG.

5. **Runnable netlist**
   Link to the complete `.cir` file. Include only the most instructive excerpt
   in the guide when the full netlist is long.

6. **How it works**
   Explain the circuit by functional block rather than line-by-line syntax.

7. **Design calculations**
   Show the equations, approximations, and expected values used to select the
   components.

8. **Simulation setup**
   Explain the chosen analyses, timestep or sweep settings, saved signals, and
   relevant initial conditions.

9. **Expected behavior and measurements**
   Identify the important waveforms and compare `.MEAS` results with calculated
   targets or documented acceptance ranges. Include an accessible
   `response.svg` that summarizes the signals and scalar results discussed in
   the guide.

10. **Experiments**
    Suggest safe parameter changes that help users understand or adapt the
    circuit.

11. **Model boundary**
    State what the example does not predict, such as thermal behavior,
    parasitics, component tolerances, vendor-specific behavior, or absolute
    accuracy.

12. **Running and testing**
    Provide exact CLI and C# instructions for compiling and simulating the
    circuit.

## Suggested Metadata Header

Each guide should begin with a compact table similar to this:

| Property | Value |
| --- | --- |
| Requires | SpiceSharpParser core |
| Dialect | Portable SPICE |
| Analyses | OP, AC, TRAN |
| Difficulty | Intermediate |
| Verified with | SpiceSharpParser 3.4.x |

For custom circuits, `Requires` should explicitly name
`SpiceSharpParser.CustomComponents` and the component families used.

## Verification Standard

A cookbook circuit is complete only when it satisfies all of the following:

- It solves or clearly models a recognizable circuit-design problem.
- It uses public, released APIs and supported netlist syntax.
- It compiles without unexpected diagnostics.
- All simulations complete successfully.
- Its `.MEAS` statements express the important scalar outcomes.
- Automated tests check those outcomes against meaningful tolerances.
- The documentation compares simulation with theory or an independent
  expectation.
- Saved waveforms and plots correspond to the signals discussed in the guide.
- `schematic.toml` passes topology and netlist-reference validation.
- `schematic.svg` is a fresh renderer output and uses matching component and
  node names.
- Both SVG assets have useful accessible title and description metadata.
- The model boundary and significant limitations are explicit.
- Another user can run it by following only the documented instructions.

Run the fast, read-only structural and generated-asset gate first:

```powershell
.\tools\cookbook-schematic\.venv\Scripts\cookbook-report
```

Use deterministic JSON when CI or an AI agent needs to consume the result:

```powershell
.\tools\cookbook-schematic\.venv\Scripts\cookbook-report `
  --format json --output artifacts\cookbook-report.json
```

Then run the compile, simulation, plot, and measurement regression suite:

```powershell
dotnet test src\SpiceSharpParser.Tests\SpiceSharpParser.Tests.csproj `
  --filter 'FullyQualifiedName~CircuitCookbookTests'
```

Portable-SPICE claims should be checked with another simulator when practical.
Native LTspice comparison can continue for LTspice A-device examples.

## Tooling Roadmap

Develop the cookbook tooling in small, independently useful layers.

### Complete: schematic source and rendering

`cookbook-schematic` supplies a versioned TOML layout, netlist-aware
validation, deterministic accessible SVG rendering, and a documented AI
workflow. Continue extending its symbol vocabulary only when a real cookbook
circuit needs a conventional symbol that cannot be expressed clearly today.

### Complete: read-only cookbook report

`cookbook-report` discovers recipes and produces deterministic text or JSON. It
checks the static cookbook contract and compares each checked-in schematic with
a fresh render without overwriting repository files.

### Next: recipe manifest and data-driven coverage

Add one small versioned manifest per recipe containing its netlist, required
dialect or custom mappings, analysis types, measurement acceptance ranges, and
catalog metadata. Migrate `CircuitCookbookTests` from hard-coded cases to these
manifests so adding a recipe does not require editing a central C# list.

The report should then compare manifest measurement names with `.MEAS`
statements and the guide's Verified Measurements table.

### Later: structured simulation reports

Add a thin .NET runner that compiles each manifest with the correct dialect and
custom mappings, executes its analyses, and returns versioned JSON containing
diagnostics, plot names, measurement values, and convergence status. Integrate
that output into `cookbook-report` only after the JSON contract is stable.

### Later: deterministic response summaries and catalog generation

Generate `response.svg` from reviewed plot specifications plus structured
simulation results, while retaining visual review as a required step. Generate
the catalog table from recipe manifests once the manifest schema has proved
stable across several additional circuits.

## Immediate Next Steps

1. Add the BJT audio preamplifier with OP, AC, and transient measurements.
2. Define and trial the recipe manifest on the BJT recipe and one existing
   pure-SPICE recipe.
3. Migrate the remaining cookbook test cases after the manifest format is
   validated.
4. Implement the structured .NET simulation runner and add an opt-in
   simulation mode to `cookbook-report`.
5. Add deterministic `response.svg` generation after structured simulation
   output and plot specifications are available.
6. Cross-check portable-SPICE recipes with an independent simulator where
   practical.

## Keeping the Project Small

Use the following constraints to prevent the cookbook effort from becoming
feature growth:

- A proposed circuit must fit the existing public feature surface.
- Unsupported syntax is a documented limitation, not an automatic parser
  feature request.
- Fix defects in already-supported behavior when a circuit exposes them, but
  do not expand syntax or add component families merely to complete an
  example.
- Keep cookbook examples and visual assets out of NuGet packages unless they
  are required at runtime.
- Store compact SVG schematics and plots; do not commit large raw waveform
  datasets.
- Prefer one data-driven test harness for cookbook circuits over substantial
  new test code for every example.
- Use `.MEAS` as the netlist-level source of truth for important outcomes.
- Avoid embedding dated whole-suite pass counts in circuit documents.
- Document expected values and tolerances; let CI report current test counts.
- Prefer one authoritative circuit guide instead of separate requirements,
  documentation, and results files unless the design is genuinely complex.
- Keep generated files reproducible from compact, reviewable source files; do
  not introduce a new generator without a corresponding freshness check.
- Keep report and validation commands read-only unless their command name and
  documentation explicitly promise generation or repair.

## Selection Criteria

When choosing a candidate from the AI example suite or designing a new one,
score it against these questions:

1. Is it useful outside parser testing?
2. Does it demonstrate a capability not already obvious from another cookbook
   circuit?
3. Can its important behavior be measured objectively?
4. Can the theory be explained without relying on simulator internals?
5. Is the result stable enough for regression testing?
6. Can it be adapted by changing a small set of parameters?
7. Can its limitations be stated honestly and clearly?
8. Does it run in a reasonable amount of time?

Prefer a small, diverse set of excellent examples over a large catalog of
minor variations.

## Guiding Rule

Every new circuit must **teach something, solve something, and prove
something**, without introducing a new parser capability.
