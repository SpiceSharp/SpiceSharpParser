# Circuit Cookbook Plan

## Direction

SpiceSharpParser already has a broad feature surface. The next phase should
focus on proving what users can build with the existing parser and
`SpiceSharpParser.CustomComponents`, rather than adding more parser features or
component families.

The intended outcome is a curated circuit cookbook containing useful,
runnable, measured, and clearly documented designs.

## Existing Foundation

The repository already contains most of the infrastructure needed for this
direction:

- `SpiceSharpParser.AIExamples` contains 948 unique measured netlists. These
  provide candidates for human review and promotion into the cookbook.
- `circuits/a-devices` demonstrates a strong example format: runnable
  netlists, `.SAVE`, `.PLOT`, `.MEAS`, explanatory guides, and automated
  verification.
- `circuits/timer555` compares calculated behavior with simulated results and
  documents the model boundary.
- The main documentation is comprehensive about syntax, analyses, and
  components, but it is not yet organized around practical things users can
  build.

The accepted AI examples should be treated as a quarry rather than published
as-is. Promoted circuits must receive an independent review of their topology,
equations, assumptions, measurements, and educational value.

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

| Circuit | Foundation | What it demonstrates |
| --- | --- | --- |
| Rectifier, reservoir, and simple regulator | Pure SPICE | Diodes, ripple, load regulation, startup, and peak current |
| BJT audio preamplifier | Pure SPICE | Bias point, AC gain, bandwidth, clipping, and Fourier distortion |
| PWM-to-analog converter | Pure SPICE | Switching, settling time, average output, and ripple |
| Transmission-line termination experiment | Pure SPICE | Reflections, propagation delay, and source/load matching |
| Ideal-diode redundant power input | CustomComponents | Supply OR-ing, failover, and reverse-current blocking |
| 555 monostable and PWM controller | Packaged subcircuit | Practical applications beyond the existing astable validation |
| Buck stage with a saturating inductor | Nonlinear `Flux=` inductor | Current ripple and the consequences of magnetic saturation |
| Sampled sensor alarm or data recorder | Sample-and-hold plus digital library | A complete analog-to-digital signal chain |
| Simple phase-locked loop | `PHASEDET`, loop filter, and `MODULATOR` | A mixed-signal system assembled from existing components |

## Recommended Starting Sequence

1. **Rectifier and filtered DC supply**
   Establish the cookbook structure with an accessible, practical pure-SPICE
   circuit.

2. **PWM DAC**
   Add a compact transient example with clear measurements for output average,
   ripple, and settling time.

3. **Simple PLL**
   Demonstrate that CustomComponents can form a complete mixed-signal system,
   rather than only isolated device examples.

This sequence covers nonlinear analog behavior, switching behavior, and a
composed custom-component application without requiring new library features.

## Proposed Repository Structure

Add a user-oriented catalog while initially leaving existing historical and
milestone directories intact.

```text
circuits/
  README.md
  cookbook/
    pure-spice/
      rectifier-power-supply/
        README.md
        rectifier-power-supply.cir
        schematic.svg
        response.svg
    custom-components/
      ideal-diode-power-or/
        README.md
        ideal-diode-power-or.cir
        schematic.svg
        response.svg
```

The root `circuits/README.md` should be the searchable cookbook catalog. It
should classify circuits by application, required package, analysis type, and
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
   Include a readable SVG with node names matching the netlist.

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
   targets or documented acceptance ranges.

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
- The schematic and netlist use matching names.
- The model boundary and significant limitations are explicit.
- Another user can run it by following only the documented instructions.

Portable-SPICE claims should be checked with another simulator when practical.
Native LTspice comparison can continue for LTspice A-device examples.

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
