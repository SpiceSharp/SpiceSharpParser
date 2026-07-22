# Digital Library Milestone A Requirements

## Scope

Implement the first delivery milestone from the digital component roadmap:

- Schmitt buffer and inverter.
- Active-high tri-state buffer and inverter.
- 2-to-1 and 4-to-1 multiplexers.
- One-bit full adder.
- Active-high 2-to-4 decoder.

All definitions must load through `SpiceSubcircuitLibrary`, work in pure
SpiceSharp circuits and parser-created circuits, and be exposed by
`DigitalSubcircuitLibrary`.

## Common Electrical Model

| Parameter | Default | Requirement |
| --- | ---: | --- |
| VDD-VSS test span | 5 V | Logic must remain supply relative |
| `TPD` | 10 ns | Transport delay for functional outputs |
| `RIN` | 1 GOhm | Finite DC input path to VSS |
| `ROUT` or `RON` | 50 Ohm | Finite enabled output drive |
| `COUT` | 5 pF | Finite output edge and load |
| Ordinary threshold `VTH` | 0.5 | High above half the local supply span |

All resistances and capacitances must be positive. Delays may be zero but not
negative. Threshold ratios must be strictly between zero and one.

## Schmitt Trigger Semantics

| Parameter | Default | 5 V equivalent |
| --- | ---: | ---: |
| `VTH_RISE` | 0.65 | 3.25 V |
| `VTH_FALL` | 0.35 | 1.75 V |
| `RSTATE` | 1 kOhm | State acquisition resistance |
| `RHOLD` | 1 TOhm | Explicit DC path and state retention |
| `CMEM` | 1 pF | State memory capacitance |

`VTH_RISE` must be greater than `VTH_FALL`. On a rising input, state changes
high only above `VTH_RISE`. On a falling input, state changes low only below
`VTH_FALL`. Between thresholds, the previous state is retained. The default
`RHOLD * CMEM` retention time is one second, much longer than the verification
transients.

The buffer output follows the retained state. The inverter output is its
complement. Ambiguous startup inside the hysteresis band resolves low for the
buffer and high for the inverter through the explicit hold resistor.

## Tri-State Semantics

The first version uses an active-high OE input. When OE is high, the output
drives the input logic level through `RON`. When OE is low, the output presents
`ROFF=1 TOhm` to VSS plus `COUT`; it is electrically high impedance rather than
a separate logical Z value. `ROFF` must be greater than `RON`.

| Criterion | Target |
| --- | --- |
| Enabled high, 10 kOhm load | Above 4.9 V |
| Enabled low, 10 kOhm pull-up | Below 0.1 V |
| Disabled with 10 kOhm pull-up | Above 4.9 V |
| Disabled with 10 kOhm pull-down | Below 0.1 V |
| Equal opposing enabled drivers | 2.4 V to 2.6 V and convergent |
| Enable/disable transition | Delayed by configured `TPD` |

The inverter reverses only the driven data polarity; OE remains active high.

## Multiplexer Semantics

- `DIG_MUX2 D0 D1 S Y VDD VSS`: S low selects D0; S high selects D1.
- `DIG_MUX4 D0 D1 D2 D3 S0 S1 Y VDD VSS`: S0 is the least-significant
  select bit and S1 is the most-significant select bit.
- All input/select combinations must match the truth table.
- Output delay and loading use the common gate parameters.

## Full-Adder Semantics

`DIG_FULL_ADDER A B CIN SUM COUT VDD VSS` implements:

```text
SUM  = A xor B xor CIN
COUT = (A and B) or (A and CIN) or (B and CIN)
```

All eight input combinations must be verified.

## Decoder Semantics

`DIG_DEC2TO4 A B EN Y0 Y1 Y2 Y3 VDD VSS` has active-high enable and outputs.
A is the least-significant address bit and B is the most-significant address
bit. When EN is low, all outputs are low. When EN is high, exactly one output
is high:

| B | A | Active output |
| ---: | ---: | --- |
| 0 | 0 | Y0 |
| 0 | 1 | Y1 |
| 1 | 0 | Y2 |
| 1 | 1 | Y3 |

## Acceptance and Packaging

- Embedded library definition count increases from 12 to 20.
- Metadata exposes every ordered pin contract and documented default.
- Typed Schmitt and tri-state parameter objects reject invalid values before
  target-circuit mutation.
- All combinational truth tables pass exhaustively.
- Schmitt rising/falling thresholds and hysteresis retention pass transient
  tests.
- Tri-state drive, release, leakage, contention, and timing pass.
- A stable representative fixture passes `NetlistLinter.Lint` and
  `SmokeTester.QuickCheck`.
- A checked-in text netlist resolves the packaged source library and runs its
  `.MEAS` assertions.
- Focused and full test suites, integration tests, both target-framework
  builds, NuGet content inspection, and `git diff --check` pass.
