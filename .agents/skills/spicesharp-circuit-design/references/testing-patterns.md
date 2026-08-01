# Testing Patterns

Use this reference before creating or modifying reusable test helpers, writing xUnit tests, asserting SpiceSharpParser results, or debugging parse/read/run failures.

## Contents

- Reusable test helpers
- Test writing checklist
- Assertions by artifact type
- Reference suites
- Debugging quick reference

## Reusable Test Helpers

Prefer the shared helper source linked into both test assemblies:

- `src/SpiceSharpParser.Testing/SpiceNetlistTestHelper.cs` for parse/read setup.
- `src/SpiceSharpParser.Testing/SpiceSimulationTestHelper.cs` for OP/DC/AC/TRAN export capture.
- `src/SpiceSharpParser.Testing/SpiceNetlistAssertions.cs` and `TestTolerance.cs` for validation, measurement, Fourier, and numeric assertions.

Use `SpiceNetlistTestOptions` to keep parser and reader settings aligned:

```csharp
var model = SpiceNetlistTestHelper.ParseAndRead(
    new SpiceNetlistTestOptions
    {
        Compatibility = CompatibilityOptions.LTspice,
        UseCustomComponents = true,
        WorkingDirectory = workingDirectory,
    },
    "title",
    "V1 out 0 1",
    ".op",
    ".save V(out)",
    ".end");
```

Only extend the helpers when a repeated pattern appears. Preserve this sequence:

1. Trim C# verbatim-string indentation:

```csharp
string normalized = string.Join(
    Environment.NewLine,
    netlist.Split('\n').Select(line => line.Trim()).Where(line => line.Length > 0));
```

2. Parse with `SpiceNetlistTestHelper.CreateParser(options)`:

```csharp
var parseResult = parser.ParseNetlist(normalized);
```

3. Read with `SpiceNetlistTestHelper.CreateReader(options, () => parser.Settings.WorkingDirectory)`:

```csharp
var model = reader.Read(parseResult.FinalModel);
```

4. Validate `model.ValidationResult.HasError` before running simulations.
5. Attach exports filtered by simulation: `model.Exports.Where(ex => ex.Simulation == simulation)`.
6. Run all three lines, or call `SpiceSimulationTestHelper`:

```csharp
var codes = simulation.Run(model.Circuit, -1);
codes = simulation.InvokeEvents(codes);
codes.ToArray();
```

7. Read sweep values:

```csharp
var dcValue = ((DC)simulation).GetCurrentSweepValue().Last();
var acFrequency = ((AC)simulation).Frequency;
var tranTime = ((Transient)simulation).Time;
```

8. For measurements, check `.Success` before reading `.Value`.

## Test Writing Checklist

- One descriptive test method per spec, for example `BandpassFilter_Has3dBBandwidthOf10kHz`.
- Inline the netlist as a string constant or share a clearly named local builder.
- Assert primary specs, boundary behavior, DC bias, passband/stopband, transient behavior, phase response, and sanity checks.
- Prefer `.MEAS` assertions for scalar specs.
- Use `.FOUR` result assertions for THD, harmonics, harmonic phase, and normalized dB.
- Assert `.PRINT` through `model.Prints` and `.PLOT` through `model.XyPlots` when report tables or curves are part of acceptance criteria.
- Use `WaveformAnalyzer` only when no netlist-native metric exists or for independent cross-checks.
- Add deterministic `.STEP` and `.TEMP` tests after nominal behavior passes.
- Use `.MC` with explicit `SEED=` for statistical checks.
- Run `dotnet test --logger "trx"` before final documentation.

Ten or more assertions per circuit can be normal when the circuit has several user-facing specs.

## Assertions by Artifact Type

| Artifact | Assertion pattern |
| --- | --- |
| Parse/read validation | Assert no errors; inspect warnings when LTspice warning no-ops are expected |
| OP/DC/AC/TRAN export | Attach export to matching simulation and collect through event callbacks |
| `.MEAS` | Find by name; assert `Success`; compare `.Value` with tolerance |
| `.FOUR` | Inspect `model.FourierAnalyses`; assert `Success`, `SignalName`, `SimulationName`, `TotalHarmonicDistortionPercent`, and harmonic rows |
| `.PRINT` | Assert printed rows/columns in `model.Prints` |
| `.PLOT` | Assert curve data in `model.XyPlots` |
| LTspice helper synthesis | Assert expected helper entity names such as `_rser`, `_cpar`, `_rlshunt`; verify subcircuit scoping when repeated |
| Custom components | Assert `UseCustomComponents()` was enabled and concrete custom entities/models exist |

## Reference Suites

Consult these before inventing a new test pattern:

- `src/SpiceSharpParser.Testing/`
- `src/SpiceSharpParser.Tests/Testing/SpiceNetlistTestHelperTests.cs`
- `src/SpiceSharpParser.IntegrationTests/BaseTests.cs`
- `src/SpiceSharpParser.IntegrationTests/Components/`
- `src/SpiceSharpParser.IntegrationTests/DotStatements/`
- `src/SpiceSharpParser.IntegrationTests/AnalogBehavioralModeling/`
- `src/SpiceSharpParser.IntegrationTests/Examples/Circuits/*.cir`
- `src/SpiceSharpParser.IntegrationTests/LTspiceCompatibility/LTspiceCompatibilityP0Tests.cs`
- `src/SpiceSharpParser.IntegrationTests/LTspiceCompatibility/LTspiceCompatibilityP1Tests.cs`
- `src/SpiceSharpParser.IntegrationTests/LTspiceCompatibility/LTspiceCompatibilityP2Tests.cs`
- `src/SpiceSharpParser.IntegrationTests/LTspiceCompatibility/LTspiceCompatibilityP3Tests.cs`
- `src/SpiceSharpParser.IntegrationTests/LTspiceCompatibility/LTspiceIdealDiodeIntegrationTests.cs`
- `src/SpiceSharpParser.Tests/CustomComponents/*Tests.cs`
- Sibling SpiceSharp tests when available: `../SpiceSharp/SpiceSharpTest/`

## Debugging Quick Reference

| Symptom | Check |
| --- | --- |
| Won't parse | Title line present? `.END` present? Leading whitespace trimmed? |
| Reader validation errors | Correct parser and reader compatibility settings? Custom components enabled only when needed? |
| Won't converge | Floating nodes? Voltage source loops? Add `.OPTIONS`, `.IC`, or `.NODESET`. |
| Zero/empty results | Three-step run pattern used? Exports filtered by simulation? |
| Wrong AC results | Using `VM()`/`VDB()`/`VP()` rather than `V()`? AC magnitude present? |
| Wrong values | `.MODEL` realistic? Node connectivity correct? Units in SI? Simulation long enough? |
| LTspice syntax rejected | Set `CompatibilityOptions.LTspice` on both parser and reader? Check `roadmap/ltspice-compatibility-matrix.md`. |
| `Q=`/`Flux=` or ideal diode rejected | Referenced `SpiceSharpParser.CustomComponents` and called `reader.Settings.UseCustomComponents()`? |
| `.INCLUDE`/`.LIB`/PWL file missing | `WorkingDirectory` set? Paths relative to including file or current working directory? Fixture file present? |
| Unexpected `_rser`/`_cpar` helpers | LTspice source/passive parasitics synthesize helper entities; inspect helper names and internal node scoping. |
| `.FOUR` missing/failed | `.TRAN` present? Complete final period? Signal expression valid? Check `Success`/`ErrorMessage`. |
| `.FOUR` THD odd | Fundamental frequency correct? Max step small enough? Final cycles settled? |
| `.STEP`/`.TEMP` count unexpected | Measurements and Fourier results produce one result per generated simulation; inspect `SimulationName`. |
