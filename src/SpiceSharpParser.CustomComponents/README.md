# SpiceSharpParser.CustomComponents

[![NuGet](https://img.shields.io/nuget/v/SpiceSharpParser.CustomComponents.svg)](https://www.nuget.org/packages/SpiceSharpParser.CustomComponents)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://github.com/SpiceSharp/SpiceSharpParser/blob/main/LICENSE)

`SpiceSharpParser.CustomComponents` is the optional companion package for
[SpiceSharpParser](https://github.com/SpiceSharp/SpiceSharpParser). It adds
LTspice-compatible components and reusable functional subcircuits while keeping
the resulting circuit compatible with the normal SpiceSharp simulation API.

The package targets .NET Standard 2.0 and .NET 8.0.

## Installation

```shell
dotnet add package SpiceSharpParser.CustomComponents
```

The package depends on `SpiceSharp-Parser`, so a separate parser package
reference is not required.

## Enable Custom Netlist Components

The original parser/reader flow keeps parsing, translation, and simulation as
explicit steps. Enable the custom mappings on `SpiceSharpReader` before calling
`Read`:

```csharp
using System.IO;
using System.Linq;
using SpiceSharpParser;
using SpiceSharpParser.CustomComponents;

string netlistText = File.ReadAllText("circuit.cir");
var compatibility = CompatibilityOptions.LTspice;

// Parse the SPICE text.
var parser = new SpiceNetlistParser();
parser.Settings.Compatibility = compatibility;
var parsed = parser.ParseNetlist(netlistText);

// Translate the parsed model into SpiceSharp objects.
var reader = new SpiceSharpReader();
reader.Settings.Compatibility = compatibility;
reader.Settings.UseCustomComponents();
var model = reader.Read(parsed.FinalModel);

// Run the analysis defined by .OP, .TRAN, .AC, or .DC in the netlist.
var simulation = model.Simulations.Single();
simulation.Execute(model.Circuit);
```

`UseCustomComponents()` adds custom-aware mappings for `A`, `C`, `D`, and `L`
elements. Call it before `Read`, and use the same compatibility settings for
the parser and reader. Ordinary diode, capacitor, and inductor syntax continues
to use the standard SpiceSharp implementations.

## Included Components

| Family | Supported forms |
| --- | --- |
| LTspice A-devices | `SRFLOP`, `DFLOP`, `PHASEDET`, `COUNTER`, `SAMPLEHOLD`, `OTA`, `VARISTOR`, and `MODULATOR`/`MODULATE` |
| Ideal diodes | LTspice-style `.MODEL D(...)` parameters such as `Ron`, `Roff`, `Vfwd`, reverse clamp, smoothing, and current limits |
| Nonlinear capacitors | Charge-defined capacitors using `Q=<expression>` |
| Nonlinear inductors | Flux-defined inductors using `Flux=<expression>` |
| Digital subcircuits | Gates, routing, latches, DFF/TFF, four-bit register, synchronous and divide-by-N counters, phase detector, and functional 555 timer |
| Analog subcircuits | Sample-and-hold, operational transconductance amplifier, voltage-controlled varistor, and frequency/amplitude modulator |

## A-device Quick Reference

Every native LTspice A-device has eight terminal positions followed by its
model name.

```spice
A<name> n1 n2 n3 n4 n5 n6 n7 n8 <model> [parameter=value ...]
```

| Model | What it does |
| --- | --- |
| `SRFLOP` | Stores one bit using asynchronous set and reset inputs |
| `DFLOP` | Captures a data bit on a clock edge |
| `PHASEDET` | Produces signed current pulses from input-edge timing error |
| `COUNTER` | Counts clock edges and generates a repeating output sequence |
| `SAMPLEHOLD` | Samples an analog voltage and holds the captured value |
| `OTA` | Converts differential input voltages into a limited output current |
| `VARISTOR` | Provides a bidirectional voltage-controlled clamp; like a programmable surge-protection varistor |
| `MODULATOR` | Generates an amplitude-controlled waveform whose frequency moves between `Space` and `Mark` |

Terminal 8 is the common node. A terminal position is unused when it repeats
that common node. If common is not ground, a literal `0` is an active connection
to global ground, not an unused terminal. Unused outputs are electrically
detached; an unused `MODULATOR` amplitude input selects the native amplitude of
1.

Example:

```spice
* Divide a clock by four
VCLOCK clock 0 PULSE(0 5 5n 100p 100p 2n 10n)
VRESET reset 0 0

ACOUNT clock reset 0 0 0 qb q 0 COUNTER Cycles=4 Duty=0.5
RQ q 0 10k
RQB qb 0 10k

.tran 100p 42n 0 100p UIC
.save V(clock) V(q) V(qb)
.plot tran V(clock) V(q) V(qb)
.meas tran q_at_30n FIND V(q) AT=30n
.print tran V(q)
.end
```

## Programmatic Subcircuit Use

The embedded libraries can also add components directly to a SpiceSharp
`Circuit`:

```csharp
using SpiceSharp;
using SpiceSharp.Components;
using SpiceSharpParser.CustomComponents.Digital;

var circuit = new Circuit(
    new VoltageSource("VDD", "vdd", "0", 5.0),
    new VoltageSource("VA", "a", "0", 5.0),
    new VoltageSource("VB", "b", "0", 5.0),
    new Resistor("RLOAD", "y", "0", 10_000.0));

var digital = DigitalSubcircuitLibrary.LoadBuiltIn();
digital.AddBinaryGate(
    circuit,
    DigitalGateKind.Nand2,
    instanceName: "XU1",
    firstInputNode: "a",
    secondInputNode: "b",
    outputNode: "y",
    positiveSupplyNode: "vdd",
    negativeSupplyNode: "0");
```

Use `AnalogSubcircuitLibrary.LoadBuiltIn()` for the four analog functional
models.

Clocked models use `DigitalSequentialParameters` for validated threshold,
delay, loading, initial state, and asynchronous-control priority overrides.

## Compatibility Notes

- A-device support is experimental and intentionally limited to the eight
  documented families.
- `MODULATOR` requires both `Mark` and `Space`; `COUNTER` requires `Cycles`.
- `SAMPLEHOLD Td` is rejected because its native delayed timing semantics are
  not implemented.
- Sweep-dependent A-device parameters are rejected instead of being silently
  frozen at one value.
- The digital and analog libraries are functional macromodels, not
  transistor-level replicas of a specific physical part.

## Documentation and Examples

- [Junior developer's science tutorial](TUTORIAL.md)
- [Custom-components architecture](ARCHITECTURE.md)
- [A-device guide](https://github.com/SpiceSharp/SpiceSharpParser/blob/main/src/docs/articles/a-devices.md)
- [Runnable A-device examples](https://github.com/SpiceSharp/SpiceSharpParser/tree/main/circuits/a-devices)
- [Digital subcircuit reference](https://github.com/SpiceSharp/SpiceSharpParser/blob/main/src/docs/articles/digital-subcircuits.md)
- [Clocked-state example and guide](https://github.com/SpiceSharp/SpiceSharpParser/tree/main/circuits/digital-milestone-b)
- [Analog subcircuit reference](https://github.com/SpiceSharp/SpiceSharpParser/blob/main/src/docs/articles/analog-subcircuits.md)
- [Ideal diode guide](https://github.com/SpiceSharp/SpiceSharpParser/blob/main/src/docs/articles/ideal-diode.md)
- [Nonlinear passive guide](https://github.com/SpiceSharp/SpiceSharpParser/blob/main/src/docs/articles/nonlinear-passives.md)

## License

MIT. See the repository [license](https://github.com/SpiceSharp/SpiceSharpParser/blob/main/LICENSE).
