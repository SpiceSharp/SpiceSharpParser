# T — Lossless Transmission Line

The lossless transmission line models a two-port distributed element with a specified characteristic impedance and propagation delay.

## Syntax

```
T<name> <port1+> <port1-> <port2+> <port2-> Z0=<impedance> TD=<delay>
```

| Parameter | Description |
|-----------|-------------|
| `port1+`, `port1-` | Port 1 (input) nodes |
| `port2+`, `port2-` | Port 2 (output) nodes |
| `Z0` | Characteristic impedance (ohms) |
| `TD` | Propagation delay (seconds) |

## Examples

```spice
* 50-ohm line with 1ns delay
T1 IN 0 OUT 0 Z0=50 TD=1e-9

* Impedance matching
T2 A 0 B 0 Z0=75 TD=5e-9
```

## MNA View

A lossless transmission line is a delayed two-port, not a single lumped
resistor, capacitor, or inductor. Its MNA behavior depends on characteristic
impedance and propagation delay.

In transient analysis, the line uses accepted waveform history. Present port
voltages and currents are related to wave values that left the other port one
delay earlier:

```text
present port relation
  = characteristic-impedance term
  + delayed history contribution
```

That makes the line similar in spirit to a companion model: the current solve
sees algebraic matrix/RHS terms, but some RHS terms come from stored accepted
history. If the timestep is rejected, the delayed history is not advanced.

For the deeper solver picture, see
[How SpiceSharp Solves Circuits](spicesharp-architecture.md#t-lossless-transmission-line).

## Typical Usage

```spice
Transmission line with matched load
V1 IN 0 PULSE(0 1 0 0.1n 0.1n 5n 20n)
RS IN LINE_IN 50
T1 LINE_IN 0 LINE_OUT 0 Z0=50 TD=1n
RL LINE_OUT 0 50
.TRAN 0.01n 10n
.SAVE V(LINE_IN) V(LINE_OUT)
.END
```

## Notes

- The lossless model assumes zero series resistance and zero shunt conductance.
- For accurate high-frequency modeling, ensure the time step is smaller than the propagation delay.
