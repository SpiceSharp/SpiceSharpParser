# How to Read and Run the A-device Examples

An LTspice A-device always has eight terminal positions, even when its model
uses only a few of them:

```text
A<name> n1 n2 n3 n4 n5 n6 n7 n8 <model> [parameter=value ...] [flag ...]
```

Terminal 8 is `common`. Voltages, thresholds, inputs, and outputs are normally
interpreted relative to this node. A terminal is unused only when its node is
the same as terminal 8. Consequently, `0` means “unused” only for a device
whose common node is global ground.

Each checked circuit has three useful layers:

1. Sources create an input sequence that makes the behavior easy to see.
2. One native `A...` line configures the device.
3. `.SAVE` selects waveforms and `.PLOT` creates XY plot data for the same
   signals.
4. `.MEAS` turns important behavior into scalar assertions
The examples use `.TRAN ... UIC` so startup is controlled by the transient
model rather than a separate operating-point solution. The automated suite
runs every file through SpiceSharpParser and native LTspice and verifies the
same documented measurements.

## Guides

- [COUNTER clock divider](clock-divider.md)
- [DFLOP edge capture](edge-capture-dff.md)
- [SAMPLEHOLD sensor capture](sample-and-hold-sensor.md)
- [OTA current limiter](ota-current-limiter.md)
- [PHASEDET charge pump](phase-detector.md)
- [VARISTOR controlled clamp](controlled-clamp-varistor.md)
- [MODULATOR frequency switch](fm-modulator.md)
- [SRFLOP with a non-ground common](non-ground-common-latch.md)

## Running an Example

From C#, enable LTspice parsing and the custom component mappings:

```csharp
var options = new SpiceCompileOptions
{
    Dialect = SpiceDialect.LTspice,
    ConfigureReader = settings => settings.UseCustomComponents(),
};

SpiceCompilationResult result =
    SpiceCompiler.CompileFile("clock-divider.cir", options);
```

For repository verification, run:

```powershell
$env:LTSPICE_EXE = 'C:\Program Files\ADI\LTspice\LTspice.exe'
dotnet test src/SpiceSharpParser.Tests/SpiceSharpParser.Tests.csproj `
  --filter 'FullyQualifiedName~LTspiceADeviceExampleTests'
```

When changing a circuit, preserve the title line, `.END`, all eight terminal
positions, and a maximum transient step small enough to resolve the relevant
edges or carrier period.
