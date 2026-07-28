# Digital Library Milestone B Requirements

## Scope

Milestone B completes the first clocked-state layer:

- `DIG_D_LATCH`: active-high transparent D latch.
- `DIG_DFF`: positive-edge D flip-flop promoted to the typed sequential API.
- `DIG_TFF`: positive-edge toggle flip-flop.
- `DIG_REG4`: four-bit positive-edge register with asynchronous clear and
  active-high output enable.
- `DIG_COUNTER4_UP`: four-bit synchronous up counter with enable, asynchronous
  clear, rollover, and carry indication.

The models must work through direct `.include` netlists and through
`DigitalSubcircuitLibrary`.

## Deterministic Semantics

- `PRE` and `CLR` are active high. Clear wins by default when both are high;
  `PRE_PRIORITY=1` selects preset priority on one-bit models.
- DFF, TFF, register, and counter react to the positive clock edge only.
- The D latch is transparent while `EN` is high and holds while it is low.
- `IC` is 0 or 1 for one-bit models and 0 through 15 for four-bit models.
- Register and counter bit zero is the least-significant bit.
- Register outputs are electrically high impedance while `OE` is low, using
  finite `ROFF` leakage rather than a logical Z state.
- Version one is functional. It does not claim physical setup, hold, or
  metastability behavior.

## Common Parameters

| Parameter | Default | Meaning |
| --- | ---: | --- |
| `VTH` | 0.5 | Supply-relative input threshold |
| `TPD` | 10 ns | Transport propagation delay |
| `RIN` | 1 GOhm | Finite input path to VSS |
| `ROUT` | 50 Ohm | Enabled output resistance |
| `COUT` | 5 pF | Output capacitance |
| `CMEM` | 1 pF | Ideal state-storage capacitance |
| `IC` | 0 | Deterministic initial state |

`DIG_REG4` also defaults `ROFF` to 1 TOhm. State forcing resistance defaults
to 10 Ohm for the latch, DFF, TFF, and register and 1 mOhm for the counter.

## Acceptance

- Pin metadata and defaults load without diagnostics.
- Latch transparent, hold, and asynchronous behavior pass.
- DFF and TFF respond once per positive edge across threshold, rise-time, and
  maximum-step variations.
- Register capture, hold, output release, re-enable, and clear pass.
- Counter enable, carry, rollover, and the complete default modulo-16 sequence
  pass.
- Invalid typed parameters fail before circuit mutation.
- State remains stable for 30-second UIC and non-UIC simulations.
- The checked direct netlist compiles and passes its `.MEAS` values.
