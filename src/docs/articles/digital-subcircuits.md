# Digital and 555 Subcircuit Library

SpiceSharpParser.CustomComponents includes twenty-three reusable digital and mixed-signal models
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
| AddSchmittBuffer | DIG_SCHMITT_BUF | A, Y, VDD, VSS |
| AddSchmittInverter | DIG_SCHMITT_NOT | A, Y, VDD, VSS |
| AddTriStateBuffer | DIG_TRI_BUF | A, OE, Y, VDD, VSS |
| AddTriStateInverter | DIG_TRI_NOT | A, OE, Y, VDD, VSS |
| AddMultiplexer2 | DIG_MUX2 | D0, D1, S, Y, VDD, VSS |
| AddMultiplexer4 | DIG_MUX4 | D0, D1, D2, D3, S0, S1, Y, VDD, VSS |
| AddFullAdder | DIG_FULL_ADDER | A, B, CIN, SUM, COUT, VDD, VSS |
| AddDecoder2To4 | DIG_DEC2TO4 | A, B, EN, Y0, Y1, Y2, Y3, VDD, VSS |
| AddComparator | DIG_COMP | P, N, Y, VDD, VSS |
| AddOpenDrain | DIG_OPEN_DRAIN | A, Y, VDD, VSS |
| AddSetResetLatch / AddSetResetFlipFlop | DIG_SR_LATCH | S, R, Q, QB, VDD, VSS |
| AddDFlipFlop | DIG_DFF | D, CLK, PRE, CLR, Q, QB, VDD, VSS |
| AddPhaseDetector | DIG_PHASE_DETECTOR | A, B, OUT, COM |
| AddCounter | DIG_COUNTER | CLK, RESET, Q, QB, VDD, VSS |
| AddTimer555 | TIMER555 | GND, TRIG, OUT, RESET, CTRL, THRESH, DISCH, VCC |

The TIMER555 ordering matches the standard package pin numbers 1 through 8.
RESET is active low. The SR latch is active high and reset dominant.

The digital A-device-derived entries are portable functional models based on LTspice's
public special-function behavior. They expand into ordinary SpiceSharp entities;
they are not drop-in parsers for LTspice `A...` instance syntax and do not promise
solver-identical waveforms. The pin lists in the table are the supported portable
interfaces. Existing buffer, inverter, Boolean, and Schmitt models cover the
corresponding public gate functions.

Sample/hold, OTA, varistor, and modulator models are provided separately by
[AnalogSubcircuitLibrary](analog-subcircuits.md).

## Logic and Electrical Conventions

These models combine Boolean decisions with analog pins. They do not use an
HDL-style four-state logic system.

Let:

```text
VS = V(VDD,VSS)
VX = V(X,VSS)
```

For ordinary digital inputs, `X` is interpreted as logic 1 only when:

```text
VX > VTH * VS
```

Otherwise it is logic 0. The comparison is strict, so a pin exactly at the
threshold is treated as low. The default `VTH=0.5` therefore gives a 2.5 V
threshold with VDD=5 V and VSS=0 V. Thresholds follow the instantaneous local
supply span; the models assume VDD is above VSS.

An asserted logic output targets VDD and a deasserted output targets VSS. Most
outputs are not ideal voltage sources: the target first passes through the
transport delay `TPD`, then through `ROUT` into an output node with `COUT` to
VSS. Consequently:

- `TPD` determines when the internal drive changes. It does not include the
  additional RC edge time.
- With an external capacitance `CL`, the approximate 10%-to-90% edge time is
  `2.2 * ROUT * (COUT + CL)`.
- A resistive load causes realistic output droop. For example, a high output
  loaded by `RL` to VSS is approximately `VS * RL / (ROUT + RL)` above VSS.
- `RIN` gives each logic input a finite DC path to VSS. It is loading, not a
  pull-up or an undefined-input detector; an otherwise floating input tends
  low.

`BVDelay` implements transport delay. Pulses shorter than `TPD` are delayed,
not intentionally rejected as they would be by an inertial-delay model. Use a
transient maximum step comfortably below both `TPD` and the expected RC edge
time when measuring switching behavior.

Analysis type matters:

- `.OP` and `.DC` calculate steady-state logic. `TPD` does not move a
  steady-state result in time.
- `.TRAN` is required to observe propagation delay, output edges, Schmitt
  retention, latch hold behavior, and 555 oscillation.
- Schmitt and latch memory is capacitor based. An operating point or an
  isolated DC sweep point has no transient history, so `RHOLD` resolves the
  stored state low when no set/reset condition is active. Use a transient ramp
  to measure a hysteresis loop.
- `.AC` and `.NOISE` linearize around one operating point; they do not simulate
  Boolean transitions or produce a meaningful digital timing response.

The VDD/VSS pins define thresholds and output levels, but these functional
behavioral models do not reproduce realistic supply current. Do not use the
current through the VDD source as an IC power-consumption prediction.

## Component Logic Reference

In the following tables, `0` and `1` are thresholded logic states, `X` means
either input state, and `Z` means the electrical high-impedance approximation
described in the tri-state section.

### Buffer and Inverter

`DIG_BUF` copies A to Y. `DIG_NOT` complements A.

| A | `DIG_BUF` Y | `DIG_NOT` Y |
| ---: | ---: | ---: |
| 0 | 0 | 1 |
| 1 | 1 | 0 |

```text
DIG_BUF: Y = A
DIG_NOT: Y = NOT A
```

Both use the ordinary threshold, delay, input resistance, and finite output
stage described above.

### Two-Input Boolean Gates

| A | B | AND | NAND | OR | NOR | XOR | XNOR |
| ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: |
| 0 | 0 | 0 | 1 | 0 | 1 | 0 | 1 |
| 0 | 1 | 0 | 1 | 1 | 0 | 1 | 0 |
| 1 | 0 | 0 | 1 | 1 | 0 | 1 | 0 |
| 1 | 1 | 1 | 0 | 1 | 0 | 0 | 1 |

```text
DIG_AND2:  Y = A AND B
DIG_NAND2: Y = NOT (A AND B)
DIG_OR2:   Y = A OR B
DIG_NOR2:  Y = NOT (A OR B)
DIG_XOR2:  Y = A XOR B
DIG_XNOR2: Y = NOT (A XOR B)
```

XOR is high when exactly one input is high. XNOR is high when the two inputs
have equal logic states.

### Schmitt Buffer and Inverter

`DIG_SCHMITT_BUF` and `DIG_SCHMITT_NOT` use two thresholds and an internal
remembered state `M`:

| Input condition | Next remembered state `M` |
| --- | ---: |
| `V(A,VSS) > VTH_RISE * VS` | 1 |
| `V(A,VSS) < VTH_FALL * VS` | 0 |
| Between or exactly on the thresholds | Previous `M` |

```text
DIG_SCHMITT_BUF: Y = M
DIG_SCHMITT_NOT: Y = NOT M
```

With the defaults, the rising and falling thresholds are 0.65 and 0.35 of the
supply span: 3.25 V and 1.75 V for a 5 V supply. A rising input must cross the
upper threshold to set the state. It can then move or become noisy anywhere in
the hysteresis band without changing the output. Only a fall below the lower
threshold clears the state.

`RSTATE` and `CMEM` control how quickly the internal state is acquired after a
threshold crossing. Their default time constant is 1 ns. `RHOLD` and `CMEM`
control retention inside the band; their default time constant is 1 second.
Retention is therefore long but not mathematically infinite. A simulation
that remains inside the band for a substantial fraction of that time can see
the stored high state decay.

An operating-point calculation with A initially inside the band has no prior
history. `RHOLD` then resolves `M=0`, so the buffer starts low and the inverter
starts high. This deterministic startup rule is a modeling choice, not a
logic-family power-up guarantee. Typed parameters require
`VTH_RISE > VTH_FALL`.

### Tri-State Buffer and Inverter

OE is active high for both components. `DIG_TRI_NOT` inverts the data but does
not invert OE.

| OE | A | `DIG_TRI_BUF` Y | `DIG_TRI_NOT` Y |
| ---: | ---: | ---: | ---: |
| 0 | X | Z | Z |
| 1 | 0 | 0 | 1 |
| 1 | 1 | 1 | 0 |

Here Z is not a separate logical value. When disabled, the output still has
`ROFF` to VSS and `COUT` to VSS. With the default `ROFF=1 TOhm`, external
bias normally determines the output voltage. For a pull-up `RPULL` to VDD:

```text
V(Y,VSS) approximately equals VS * ROFF / (RPULL + ROFF)
```

When enabled, the selected rail drives Y through `RON`; the very weak `ROFF`
path remains present. Data and OE are thresholded and delayed independently by
the same `TPD`. A data transition while enabled and an enable/disable
transition therefore both take the configured transport delay, followed by
the output RC response.

Finite `RON` makes contention solvable. Two equal drivers commanding opposite
rails settle near half the supply span; ignoring other loads, their contention
current is approximately `VS / (RON_HIGH + RON_LOW)`. This is useful for
topology studies but is not a thermal or damage model. Typed parameters require
`ROFF > RON`.

### 2-to-1 Multiplexer

`DIG_MUX2` is a digital selector, not an analog switch. It thresholds the
selected data input and regenerates a VDD/VSS output.

| S | Selected input | Y |
| ---: | --- | ---: |
| 0 | D0 | D0 |
| 1 | D1 | D1 |

```text
Y = (NOT S AND D0) OR (S AND D1)
```

### 4-to-1 Multiplexer

S0 is the least-significant select bit and S1 is the most-significant bit.

| S1 | S0 | Selected input | Y |
| ---: | ---: | --- | ---: |
| 0 | 0 | D0 | D0 |
| 0 | 1 | D1 | D1 |
| 1 | 0 | D2 | D2 |
| 1 | 1 | D3 | D3 |

Equivalently, the selected index is `2*S1 + S0`. Like `DIG_MUX2`, this block
regenerates logic levels rather than passing an analog voltage.

### One-Bit Full Adder

`DIG_FULL_ADDER` adds A, B, and carry input CIN. SUM is the low-order result
bit and COUT is the carry into the next bit position.

```text
SUM  = A XOR B XOR CIN
COUT = (A AND B) OR (A AND CIN) OR (B AND CIN)
```

| A | B | CIN | SUM | COUT |
| ---: | ---: | ---: | ---: | ---: |
| 0 | 0 | 0 | 0 | 0 |
| 0 | 0 | 1 | 1 | 0 |
| 0 | 1 | 0 | 1 | 0 |
| 0 | 1 | 1 | 0 | 1 |
| 1 | 0 | 0 | 1 | 0 |
| 1 | 0 | 1 | 0 | 1 |
| 1 | 1 | 0 | 0 | 1 |
| 1 | 1 | 1 | 1 | 1 |

COUT is the three-input majority function: it is high when at least two inputs
are high. SUM and COUT have separate delayed, finite-impedance output stages.
In this subcircuit, `COUT` is also the name of the output-capacitance parameter.
Context distinguishes them: the fifth ordered node is the carry pin, while
`COUT=...` after the instance nodes overrides the capacitance applied to both
SUM and the carry output.

### Active-High 2-to-4 Decoder

`DIG_DEC2TO4` converts the two-bit address `(B,A)` into one of four active-high
outputs. A is the least-significant address bit. EN is active high.

| EN | B | A | Y0 | Y1 | Y2 | Y3 |
| ---: | ---: | ---: | ---: | ---: | ---: | ---: |
| 0 | X | X | 0 | 0 | 0 | 0 |
| 1 | 0 | 0 | 1 | 0 | 0 | 0 |
| 1 | 0 | 1 | 0 | 1 | 0 | 0 |
| 1 | 1 | 0 | 0 | 0 | 1 | 0 |
| 1 | 1 | 1 | 0 | 0 | 0 | 1 |

```text
Y0 = EN AND NOT B AND NOT A
Y1 = EN AND NOT B AND A
Y2 = EN AND B AND NOT A
Y3 = EN AND B AND A
```

All four outputs have independent delayed, finite-impedance stages. This is a
decoder only; there is no active-low output or demultiplexer polarity hidden
in the model.

### Differential Comparator

`DIG_COMP` does not use `VTH`. Its decision is based directly on the
differential input voltage:

```text
Y = 1 when V(P,N) > VOFF
Y = 0 otherwise
```

`VOFF` is in volts and defaults to zero. At exact equality the output is low.
Positive `VOFF` means P must exceed N by more than that amount; negative
`VOFF` shifts the decision in the opposite direction. The output is still
regenerated to VDD/VSS and uses `TPD`, `ROUT`, and `COUT`.

The comparator has no hysteresis. Use a Schmitt block or external feedback if
a noisy or slowly moving input must not chatter around the decision point.

### Active-High Open-Drain Driver

`DIG_OPEN_DRAIN` can pull Y toward VSS but cannot drive it high.

| A | Output stage | Expected Y with an external pull-up |
| ---: | --- | --- |
| 0 | Released through `ROFF` | High |
| 1 | Pull-down through `RON` | Low |

An external pull-up or another driving element is required to establish a high
level. With a pull-up `RPULL` to VDD, the enabled low is approximately:

```text
V(Y,VSS) approximately equals VS * RON / (RPULL + RON)
```

Multiple open-drain outputs can share a pull-up to form wired-AND logic in
positive logic: the bus is high only when every driver is released. This model
has no `TPD` parameter; its input decision changes the conductance immediately,
while `COUT` and the external network determine the voltage transition.

### Reset-Dominant SR Latch

S and R are active high. R has priority when both controls are asserted.

| S | R | Next Q | Next QB | Meaning |
| ---: | ---: | ---: | ---: | --- |
| 0 | 0 | Previous Q | Previous QB | Hold |
| 1 | 0 | 1 | 0 | Set |
| 0 | 1 | 0 | 1 | Reset |
| 1 | 1 | 0 | 1 | Reset dominates |

Q and QB are generated as complements of one internal remembered state and
then pass through matching delayed output stages. `RSTATE`/`CMEM` control state
acquisition, while `RHOLD`/`CMEM` control retention. With the defaults, those
time constants are 1 ns and 1 second respectively.

If an operating-point calculation starts with S=R=0 and no prior state,
`RHOLD` resolves Q low and QB high. As with the Schmitt state, a held high is
not retained forever: it decays on the very long `RHOLD*CMEM` scale. The model
is deterministic and functional; it does not model metastability or an
indeterminate forbidden state.

### Functional 555 Timer

`TIMER555` is composed from the comparator, reset-dominant SR latch, buffer,
and open-drain primitives. Its ordered pins match the standard eight-pin
package:

| Position | Pin | Function in this model |
| ---: | --- | --- |
| 1 | GND | Local reference for all thresholds and outputs |
| 2 | TRIG | Sets the latch when below the lower reference |
| 3 | OUT | Buffered Q output: high while the latch is set |
| 4 | RESET | Active when below half of the VCC-to-GND span |
| 5 | CTRL | Upper threshold reference and divider tap |
| 6 | THRESH | Resets the latch when above CTRL |
| 7 | DISCH | Open-drain pull-down, enabled while OUT is low |
| 8 | VCC | Positive supply and upper output rail |

Three equal `RDIV` resistors form the default references. With CTRL unloaded,
CTRL settles at two-thirds of VCC and the internal lower reference `REFLO`
settles at one-third. Driving CTRL externally moves the upper threshold and,
through the lower divider resistors, the trigger threshold. With an ideal stiff
CTRL voltage, `REFLO` is approximately half of CTRL.

The functional control priority is:

| Priority | Analog condition | Latch action | OUT | DISCH |
| ---: | --- | --- | --- | --- |
| 1 | `V(RESET,GND) < 0.5 * V(VCC,GND)` | Reset | Low | Pulls low |
| 2 | RESET inactive and `V(TRIG,GND) < V(REFLO,GND)` | Set | High | Released |
| 3 | No trigger and `V(THRESH,GND) > V(CTRL,GND)` | Reset | Low | Pulls low |
| 4 | None of the above | Hold | Previous | Complement of OUT state |

RESET therefore overrides TRIG, and TRIG overrides THRESH if both comparator
conditions are true. Comparisons are strict. A floating RESET is pulled toward
GND by `RIN`, so tie RESET high when the timer should operate normally.

OUT is a push-pull functional output with `ROUT` and `COUT`. DISCH is an
open-drain output using `RDIS` when enabled and `ROFF` when released. The timer
parameter `TPD` is passed to multiple internal stages; it is not a single
guaranteed pin-to-pin delay. A trigger or threshold event passes through a
comparator, latch, and possibly output buffer, so the observed OUT delay can be
several `TPD` intervals plus state and output RC response.

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
two-input kinds. Dedicated methods expose the Schmitt, tri-state, multiplexer,
adder, decoder, comparator, open-drain, latch, and timer contracts. The Library
property exposes the underlying SpiceSubcircuitLibrary, including pins and
default parameters.

## Milestone A Routing Example

This example conditions a slow or noisy input, selects it, and drives a shared
bus only while the active-high enable is asserted:

```csharp
var digital = DigitalSubcircuitLibrary.LoadBuiltIn();

digital.AddSchmittBuffer(
    circuit,
    "XINPUT",
    "sensor",
    "conditioned",
    "vdd",
    "0",
    new DigitalSchmittParameters
    {
        RisingThresholdRatio = 0.65,
        FallingThresholdRatio = 0.35,
    });

digital.AddMultiplexer2(
    circuit,
    "XROUTE",
    "conditioned",
    "alternate",
    "select",
    "routed",
    "vdd",
    "0");

digital.AddTriStateBuffer(
    circuit,
    "XBUS",
    "routed",
    "enable",
    "bus",
    "vdd",
    "0");
```

The directly executable netlist is
[`circuits/digital-milestone-a/milestone-a-routing.cir`](../../../circuits/digital-milestone-a/milestone-a-routing.cir).
It includes the shipped source library and verifies conditioned, disabled-bus,
and enabled-bus voltages with `.MEAS`.

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

Multiplexers, the full adder, and the decoder use these same gate parameters.
`DIG_MUX2` selects D0 for S=0 and D1 for S=1. `DIG_MUX4` treats S0 as the
least-significant select bit. The decoder uses A as its least-significant
address bit, has an active-high EN input, and drives all outputs low while
disabled.

## Schmitt Parameters and Startup

Pass `DigitalSchmittParameters` to either Schmitt add method:

| Property | SPICE parameter | Default | Meaning |
| --- | --- | ---: | --- |
| RisingThresholdRatio | VTH_RISE | 0.65 | Low-to-high threshold fraction |
| FallingThresholdRatio | VTH_FALL | 0.35 | High-to-low threshold fraction |
| PropagationDelay | TPD | 10 ns | Transport delay |
| InputResistance | RIN | 1 GOhm | Input loading to VSS |
| OutputResistance | ROUT | 50 ohms | Series output resistance |
| OutputCapacitance | COUT | 5 pF | Output capacitance to VSS |
| StateResistance | RSTATE | 1 kOhm | State acquisition resistance |
| HoldResistance | RHOLD | 1 TOhm | State retention and DC path |
| StateCapacitance | CMEM | 1 pF | State storage capacitance |

The rising ratio must be greater than the falling ratio. Between thresholds,
the model retains its previous state. If an operating-point calculation starts
inside the band without prior history, RHOLD resolves the buffer low and the
inverter high. This is deterministic functional initialization, not a physical
power-up guarantee.

## Tri-State Parameters and Bus Semantics

Pass `DigitalTriStateParameters` to either tri-state add method:

| Property | SPICE parameter | Default | Meaning |
| --- | --- | ---: | --- |
| LogicThresholdRatio | VTH | 0.5 | Data and OE switching fraction |
| PropagationDelay | TPD | 10 ns | Data and enable transport delay |
| InputResistance | RIN | 1 GOhm | Input loading to VSS |
| OnResistance | RON | 50 ohms | Enabled drive resistance |
| OffResistance | ROFF | 1 TOhm | Disabled leakage resistance |
| OutputCapacitance | COUT | 5 pF | Output capacitance to VSS |

OE is active high for both models; the inverter changes data polarity only.
The disabled output is represented electrically by ROFF and COUT. External
bias therefore controls the bus, and two enabled opposing drivers settle to a
finite contention voltage. `OffResistance` must be greater than
`OnResistance`.

Both typed parameter objects validate ratios, finite values, positive passive
values, and parameter relationships before the target circuit is changed.

## Model Parameter Defaults

Gate, Schmitt, and tri-state facade methods use the typed parameter classes
described above. Comparator, latch, open-drain, and timer methods accept an
`IReadOnlyDictionary<string, string>` of raw SPICE values. For example:

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
| DIG_SCHMITT_BUF, DIG_SCHMITT_NOT | VTH_RISE=0.65, VTH_FALL=0.35, TPD=10n, RIN=1G, ROUT=50, COUT=5p, RSTATE=1k, RHOLD=1T, CMEM=1p |
| DIG_TRI_BUF, DIG_TRI_NOT | VTH=0.5, TPD=10n, RIN=1G, RON=50, ROFF=1T, COUT=5p |
| DIG_MUX2, DIG_MUX4, DIG_FULL_ADDER, DIG_DEC2TO4 | VTH=0.5, TPD=10n, RIN=1G, ROUT=50, COUT=5p |
| DIG_COMP | VOFF=0, TPD=10n, RIN=1G, ROUT=50, COUT=5p |
| DIG_OPEN_DRAIN | VTH=0.5, RIN=1G, RON=10, ROFF=1T, COUT=5p |
| DIG_SR_LATCH | VTH=0.5, TPD=10n, RIN=1G, ROUT=50, COUT=5p, RSTATE=1k, RHOLD=1T, RINIT=100G, CMEM=1p, IC=0 |
| DIG_DFF | VTH=0.5, TPD=10n, RIN=1G, ROUT=50, COUT=5p, RSTATE=10, RHOLD=1T, RINIT=100G, CMEM=1p, IC=0 |
| DIG_PHASE_DETECTOR | REF=0.5, IOUT=1m, VHIGH=10, VLOW=-10, RIN=1G, ROUT=1T, RCLAMP=1, COUT=1p, RSTATE=10, RHOLD=1T, CMEM=1p |
| DIG_COUNTER | CYCLES=2, DUTY=0.5, VTH=0.5, RIN=1G, ROUT=50, COUT=5p, RHOLD=1T, CMEM=10p, RWRAP=1, CWRAP=1p |
| TIMER555 | TPD=100n, RIN=1G, ROUT=20, COUT=2n, RDIS=10, ROFF=1T, RDIV=5k |

VOFF is the comparator differential offset. RON and RDIS set enabled
pull-down resistance; ROFF sets leakage in the released state. RSTATE and CMEM
set latch acquisition dynamics, while RHOLD and CMEM determine the functional
state-retention time constant. TIMER555's COUT intentionally shapes the
otherwise ideal behavioral output edge; it is not a physical pin capacitance.

## 555 Astable Configuration and Timing

The component logic reference above describes the timer's divider, control
priority, staged delays, output, and discharge behavior. For the usual astable
connection, the ideal timing estimates are:

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

## LTspice A-device Compatibility

The optional `LTspiceADeviceCompatibilityGoldenTests` suite runs native
LTspice A-devices and these portable subcircuits with the same stimuli. Its
digital cases cover `SRFLOP`, `DFLOP`, `PHASEDET`, and `COUNTER`. Set
`LTSPICE_EXE` to the LTspice executable path to enable the tests; they are
skipped when it is unset.

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
Schmitt and tri-state functional contracts are informed by the
[TI SN74HC14 data sheet](https://www.ti.com/lit/ds/symlink/sn74hc14.pdf) and
[TI SN74HC125 data sheet](https://www.ti.com/lit/ds/symlink/sn74hc125.pdf).
These generic models do not claim either device's exact voltage or timing
limits. The
underlying primitives are supported by SpiceSharp's
[behavioral components](https://spicesharp.github.io/SpiceSharp/api/index.html)
and
[voltage delay](https://spicesharp.github.io/SpiceSharp/api/SpiceSharp.Components.VoltageDelay.html).
