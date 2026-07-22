# Digital Library Milestone A Design Notes

## Architecture

Milestone A adds eight public subcircuits to `standard-digital.lib` and raises
the built-in library from 12 to 20 definitions. Every model expands through
`SpiceSubcircuitLibrary`, so the same implementation works in a pure
SpiceSharp `Circuit`, a parser-created circuit, and a text netlist using
`.include`.

The Schmitt pair stores the accepted logic state on `CMEM`. A behavioral
current forces that state through `RSTATE` only outside the hysteresis band;
`RHOLD` provides retention and an explicit DC path. The ordinary output path
then uses transport delay, series resistance, and shunt capacitance.

The tri-state pair delays both data and active-high OE. When enabled, a
behavioral current drives the output through `RON`. An explicit `ROFF`
resistor models disabled leakage and keeps a shared bus structurally connected
for topology validation. This is electrical high impedance, not a logical Z
state.

Multiplexers, the full adder, and the decoder use supply-relative behavioral
truth tables followed by the same finite `TPD`/`ROUT`/`COUT` output stage used
by the original gates.

## Semantic Choices

- Schmitt defaults are 0.65 VDD on a rising input and 0.35 VDD on a falling
  input. Startup inside the band resolves low for the buffer and high for the
  inverter.
- Tri-state OE is active high for buffer and inverter. Only data polarity is
  inverted by `DIG_TRI_NOT`.
- `DIG_MUX2` selects D0 at S=0. `DIG_MUX4` uses S0 as its least-significant
  select bit.
- `DIG_DEC2TO4` uses A as its least-significant address bit. EN and all four
  outputs are active high; disabled outputs are low.
- The full adder exposes independent finite-impedance SUM and COUT outputs.

## Public API

`DigitalSubcircuitLibrary` exposes one named add method per new model.
Multiplexers, the adder, and decoder accept `DigitalGateParameters`.
Schmitt-trigger models accept `DigitalSchmittParameters`; tri-state models
accept `DigitalTriStateParameters`. The specialized types validate threshold
ordering and on/off resistance ordering before the target circuit is mutated.

Advanced callers can continue using `Library.AddInstance(...)` and raw SPICE
parameter dictionaries.

## Verification Boundary

These are functional mixed-signal models. They verify logic, hysteresis,
transport delay, finite loading, bus release, and contention. They do not model
logic X values, physical metastability, ESD structures, short-circuit current,
temperature curves, or a particular logic family's guaranteed data-sheet
limits.

The checked example
[`milestone-a-routing.cir`](milestone-a-routing.cir) composes a Schmitt buffer,
2:1 multiplexer, and tri-state buffer from the text library and measures both
the released and driven bus states.
