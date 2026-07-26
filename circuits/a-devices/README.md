# Runnable LTspice A-device Examples

These circuits use LTspice's native eight-terminal `A...` syntax and are
checked by `LTspiceADeviceExampleTests`. They are small enough to modify while
showing patterns that are useful in larger mixed-signal designs.

| Example | Pattern demonstrated | Guide |
| --- | --- | --- |
| [`clock-divider.cir`](clock-divider.cir) | Divide-by-four clock with complementary outputs | [COUNTER guide](guides/clock-divider.md) |
| [`edge-capture-dff.cir`](edge-capture-dff.cir) | Capture high and low data on successive clock edges | [DFLOP guide](guides/edge-capture-dff.md) |
| [`sample-and-hold-sensor.cir`](sample-and-hold-sensor.cir) | Capture a ramp and retain the sample for seconds | [SAMPLEHOLD guide](guides/sample-and-hold-sensor.md) |
| [`ota-current-limiter.cir`](ota-current-limiter.cir) | Symmetric `Iout` limiting measured through a zero-volt sense source | [OTA guide](guides/ota-current-limiter.md) |
| [`phase-detector.cir`](phase-detector.cir) | Convert input-edge skew into a signed charge-pump voltage | [PHASEDET guide](guides/phase-detector.md) |
| [`controlled-clamp-varistor.cir`](controlled-clamp-varistor.cir) | Clamp positive and negative excursions to a controlled magnitude | [VARISTOR guide](guides/controlled-clamp-varistor.md) |
| [`fm-modulator.cir`](fm-modulator.cir) | Switch between SPACE and MARK frequencies with the AM input unused | [MODULATOR guide](guides/fm-modulator.md) |
| [`non-ground-common-latch.cir`](non-ground-common-latch.cir) | Use a non-ground common node and detach an unused output correctly | [SRFLOP guide](guides/non-ground-common-latch.md) |

Start with [How to Read and Run the Examples](guides/README.md) if native
A-device terminal ordering is new to you.

Compile one from C# with custom-component mappings enabled:

```csharp
using SpiceSharpParser;
using SpiceSharpParser.CustomComponents;

var options = new SpiceCompileOptions
{
    Dialect = SpiceDialect.LTspice,
    ConfigureReader = settings => settings.UseCustomComponents(),
};

SpiceCompilationResult result =
    SpiceCompiler.CompileFile("clock-divider.cir", options);
```

Every file contains `.SAVE`/`.PLOT` waveform requests and `.MEAS` statements so it can be used as a simulation smoke
test as well as a starting point. Remember these syntax rules when adapting an
example:

- Terminal 8 is the device common node. Repeat that node in every unused
  terminal position; use `0` only when common really is global ground.
- `MODULATOR` requires both `Mark` and `Space`. Connecting its AM terminal to
  common marks AM as unused and selects the native default amplitude of 1.
- `COUNTER` requires `Cycles`.
- `SAMPLEHOLD Td` is intentionally rejected because its native timing
  semantics are not implemented.

For a terminal-by-terminal introduction to every supported family, see
[`src/docs/articles/a-devices.md`](../../src/docs/articles/a-devices.md).
