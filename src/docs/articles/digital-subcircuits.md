# Digital and 555 Subcircuit Library

SpiceSharpParser.CustomComponents includes twelve reusable mixed-signal models
backed by SpiceSubcircuitLibrary. The definitions live in the embedded
standard-digital.lib text library and expand into ordinary SpiceSharp entities,
so the same blocks work in pure programmatic circuits and in circuits read by
SpiceSharpParser.

## Included Components

| API or kind | Subcircuit | Ordered pins |
| --- | --- | --- |
| DigitalGateKind.Buffer | DIG_BUF | A, Y, VDD, VSS |
| DigitalGateKind.Inverter | DIG_NOT | A, Y, VDD, VSS |
| DigitalGateKind.And2 | DIG_AND2 | A, B, Y, VDD, VSS |
| DigitalGateKind.Nand2 | DIG_NAND2 | A, B, Y, VDD, VSS |
| DigitalGateKind.Or2 | DIG_OR2 | A, B, Y, VDD, VSS |
| DigitalGateKind.Nor2 | DIG_NOR2 | A, B, Y, VDD, VSS |
| DigitalGateKind.Xor2 | DIG_XOR2 | A, B, Y, VDD, VSS |
| DigitalGateKind.Xnor2 | DIG_XNOR2 | A, B, Y, VDD, VSS |
| AddComparator | DIG_COMP | P, N, Y, VDD, VSS |
| AddOpenDrain | DIG_OPEN_DRAIN | A, Y, VDD, VSS |
| AddSetResetLatch | DIG_SR_LATCH | S, R, Q, QB, VDD, VSS |
| AddTimer555 | TIMER555 | GND, TRIG, OUT, RESET, CTRL, THRESH, DISCH, VCC |

The TIMER555 ordering matches the standard package pin numbers 1 through 8.
RESET is active low. The SR latch is active high and reset dominant.

## Programmatic SpiceSharp Use

```csharp
using SpiceSharp;
using SpiceSharp.Components;
using SpiceSharpParser.CustomComponents.Digital;

var circuit = new Circuit(
    new VoltageSource("VCC", "vcc", "0", 5.0),
    new Resistor("RA", "vcc", "discharge", 10_000.0),
    new Resistor("RB", "discharge", "timing", 10_000.0),
    new Capacitor("CT", "timing", "0", 10e-9),
    new Capacitor("CCTRL", "control", "0", 10e-9),
    new Resistor("RLOAD", "out", "0", 10_000.0));

var digital = DigitalSubcircuitLibrary.LoadBuiltIn();
digital.AddTimer555(
    circuit,
    instanceName: "XU1",
    groundNode: "0",
    triggerNode: "timing",
    outputNode: "out",
    resetNode: "vcc",
    controlNode: "control",
    thresholdNode: "timing",
    dischargeNode: "discharge",
    positiveSupplyNode: "vcc");
```

AddBuffer and AddInverter are unary shortcuts. AddBinaryGate handles the six
two-input kinds. AddComparator, AddOpenDrain, AddSetResetLatch, and AddTimer555
add the higher-level primitives. The Library property exposes the underlying
SpiceSubcircuitLibrary, including pins and default parameters.

## Gate Parameters

Pass DigitalGateParameters to the gate add methods:

```csharp
digital.AddBuffer(
    circuit,
    "XBUF",
    "input",
    "output",
    "vcc",
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
| LogicThresholdRatio | VTH | 0.5 | Threshold fraction of the VDD-to-VSS span |
| PropagationDelay | TPD | 10 ns | Transport delay |
| InputResistance | RIN | 1 GOhm | Resistance from each input to VSS |
| OutputResistance | ROUT | 50 ohms | Series output resistance |
| OutputCapacitance | COUT | 5 pF | Output capacitance to VSS |

For a positive supply span, an input is high when V(A,VSS) is greater than
VTH times V(VDD,VSS). With a capacitive load CL, the approximate 10%-to-90%
edge time is 2.2 times ROUT times (COUT + CL).

## Comparator, Latch, Open-Drain, and Timer Parameters

These APIs accept an IReadOnlyDictionary<string, string> of raw SPICE values.
For example:

```csharp
digital.AddComparator(
    circuit,
    "XCMP",
    "sense",
    "reference",
    "comparison",
    "vcc",
    "0",
    new Dictionary<string, string>
    {
        ["VOFF"] = "10m",
        ["TPD"] = "25n",
    });
```

| Subcircuit | Parameters and defaults |
| --- | --- |
| DIG_COMP | VOFF=0, TPD=10n, RIN=1G, ROUT=50, COUT=5p |
| DIG_OPEN_DRAIN | VTH=0.5, RIN=1G, RON=10, ROFF=1T, COUT=5p |
| DIG_SR_LATCH | VTH=0.5, TPD=10n, RIN=1G, ROUT=50, COUT=5p, RSTATE=1k, RHOLD=1T, CMEM=1p |
| TIMER555 | TPD=100n, RIN=1G, ROUT=20, COUT=2n, RDIS=10, ROFF=1T, RDIV=5k |

VOFF is the comparator differential offset. RON and RDIS set enabled
pull-down resistance; ROFF sets leakage in the released state. RSTATE and CMEM
set latch acquisition dynamics, while RHOLD and CMEM determine the functional
state-retention time constant. TIMER555's COUT intentionally shapes the
otherwise ideal behavioral output edge; it is not a physical pin capacitance.

## 555 Behavior and Astable Timing

The timer contains a three-resistor divider, two comparators, a reset-dominant
SR latch, a finite-impedance output stage, and an open-drain discharge stage.
TRIG below one-third VCC sets the latch. THRESH above two-thirds VCC resets it.
The enforced priority is active-low RESET, then TRIG, then THRESH.

For the usual astable connection, the ideal timing estimates are:

```text
t_high = 0.693 * (RA + RB) * C
t_low  = 0.693 * RB * C
period = 0.693 * (RA + 2*RB) * C
```

With RA=10 kohm, RB=10 kohm, and C=10 nF, the expected high time is 138.6 us,
low time is 69.3 us, period is 207.9 us, and frequency is about 4.81 kHz. The
checked example is in circuits/timer555/timer555-astable.cir.

Use a transient maximum step comfortably below TPD and the output edge time.
The example uses Gear integration and a 10 ns maximum step. When UIC is used,
initializing the control bypass capacitor near two-thirds VCC avoids an
artificial startup ramp.

## Mixing with SpiceSharpParser Custom Components

The built-in digital models use standard parser components and require no
optional mappings. Add them after reading a netlist containing custom
components:

```csharp
var settings = new SpiceNetlistReaderSettings();
settings.UseCustomComponents();
var model = new SpiceNetlistReader(settings).Read(parsedNetlist);

var digital = DigitalSubcircuitLibrary.LoadBuiltIn();
digital.AddTimer555(
    model.Circuit,
    "XU1",
    "0",
    "trigger",
    "out",
    "reset",
    "control",
    "threshold",
    "discharge",
    "vcc");
```

For a user-authored library that contains custom ideal diodes or nonlinear
passives, configure SpiceSubcircuitLibrary.LoadFile with
settings.UseCustomComponents().

## Scope and Limitations

These are functional mixed-signal models. TIMER555 is suitable for topology,
timing, priority, and control-loop experiments, but it is not a transistor-level
model of a particular NE555 or TLC555. It does not reproduce supply current,
output-current limits, saturation curves, exact input bias, temperature drift,
noise, recovery behavior, or every data-sheet tolerance. The digital blocks do
not model unknown states, metastability, protection structures, or logic-family
input/output curves. Use a vendor macro-model or transistor model when those
effects are acceptance criteria.

The architecture and one-third/two-thirds thresholds follow the
[TI NE555 data sheet](https://www.ti.com/lit/ds/symlink/ne555.pdf) and the
[TI TLC555 data sheet](https://www.ti.com/lit/ds/symlink/tlc555m.pdf). The
underlying primitives are supported by SpiceSharp's
[behavioral components](https://spicesharp.github.io/SpiceSharp/api/index.html)
and
[voltage delay](https://spicesharp.github.io/SpiceSharp/api/SpiceSharp.Components.VoltageDelay.html).
