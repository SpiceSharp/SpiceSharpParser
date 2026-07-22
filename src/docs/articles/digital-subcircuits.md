# Digital Gate Subcircuit Library

`SpiceSharpParser.CustomComponents` includes eight generic combinational gate
models backed by `SpiceSubcircuitLibrary`. The models are stored as an embedded
SPICE text library and expanded into ordinary SpiceSharp entities when added to
a circuit.

## Included Gates

| `DigitalGateKind` | Subcircuit | Ordered pins |
| --- | --- | --- |
| `Buffer` | `DIG_BUF` | A, Y, VDD, VSS |
| `Inverter` | `DIG_NOT` | A, Y, VDD, VSS |
| `And2` | `DIG_AND2` | A, B, Y, VDD, VSS |
| `Nand2` | `DIG_NAND2` | A, B, Y, VDD, VSS |
| `Or2` | `DIG_OR2` | A, B, Y, VDD, VSS |
| `Nor2` | `DIG_NOR2` | A, B, Y, VDD, VSS |
| `Xor2` | `DIG_XOR2` | A, B, Y, VDD, VSS |
| `Xnor2` | `DIG_XNOR2` | A, B, Y, VDD, VSS |

## Programmatic SpiceSharp Use

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

`AddBuffer` and `AddInverter` are shortcuts for the unary gates.
`AddGate` accepts an ordered input-node collection when gate kinds are chosen
dynamically. The `Library` property exposes the underlying
`SpiceSubcircuitLibrary` and its pin/default-parameter metadata.

## Electrical Parameters

Pass a `DigitalGateParameters` object to any add method:

```csharp
digital.AddBuffer(
    circuit,
    "XBUF",
    "input",
    "output",
    "vdd",
    "0",
    new DigitalGateParameters
    {
        LogicThresholdRatio = 0.7,
        PropagationDelay = 6e-9,
        InputResistance = 1e9,
        OutputResistance = 25.0,
        OutputCapacitance = 4e-12,
    });
```

| Property | SPICE parameter | Default | Meaning |
| --- | --- | --- | --- |
| `LogicThresholdRatio` | `VTH` | 0.5 | Threshold fraction of the local VDD-to-VSS span |
| `PropagationDelay` | `TPD` | 10 ns | Transport delay |
| `InputResistance` | `RIN` | 1 GOhm | Resistance from each input to VSS |
| `OutputResistance` | `ROUT` | 50 ohms | Series output resistance |
| `OutputCapacitance` | `COUT` | 5 pF | Intrinsic output capacitance to VSS |

For a positive supply span, an input is high when

$$
V(A,VSS) > VTH \cdot V(VDD,VSS).
$$

The behavioral logic result is passed through SpiceSharp's voltage-delay
element, then through `ROUT` to the output. With a capacitive load
`C_L`, the approximate 10%-to-90% edge time is

$$
t_r \approx 2.2 \cdot ROUT \cdot (COUT + C_L).
$$

The defaults are generic simulation values, not a model of a named logic
family. For family-specific work, set threshold, delay, and loading from the
manufacturer's operating conditions. For example, the
[TI CD74HC04 data sheet](https://www.ti.com/lit/ds/symlink/cd74hc04.pdf)
specifies supply- and load-dependent input limits and propagation delays.

## Mixing with SpiceSharpParser Custom Components

The digital models themselves use standard parser components and need no
additional mapping. They can be added after a netlist containing optional
custom components has been read:

```csharp
var settings = new SpiceNetlistReaderSettings();
settings.UseCustomComponents();
var model = new SpiceNetlistReader(settings).Read(parsedNetlist);

var digital = DigitalSubcircuitLibrary.LoadBuiltIn();
digital.AddBuffer(model.Circuit, "XBUF", "in", "out", "vdd", "0");
```

For a user-authored digital library that itself contains custom ideal diodes or
nonlinear passives, pass matching reader configuration to
`SpiceSubcircuitLibrary.LoadFile`:

```csharp
var options = new SpiceCompileOptions
{
    ConfigureReader = settings => settings.UseCustomComponents(),
};

var library = SpiceSubcircuitLibrary.LoadFile("my-digital.lib", options);
```

## Scope and Limitations

These models intentionally provide two-state combinational behavior for mixed
analog/digital simulations. They do not currently model unknown states,
metastability, tri-state/high-impedance outputs, supply current, protection
structures, temperature drift, noise, or family-specific input/output curves.
The delay is a transport delay, while edge rate and load interaction come from
the output resistance and capacitance. Use transistor-level or vendor
macro-model subcircuits when those effects are acceptance criteria.

The underlying primitives are supported directly by SpiceSharp:
[behavioral sources](https://spicesharp.github.io/SpiceSharp/api/index.html)
and
[voltage delay](https://spicesharp.github.io/SpiceSharp/api/SpiceSharp.Components.VoltageDelay.html).
