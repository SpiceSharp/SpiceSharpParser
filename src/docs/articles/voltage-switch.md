# S — Voltage Switch

A voltage-controlled switch changes between high and low resistance states based on a controlling voltage.

## Syntax

```
S<name> <node1> <node2> <ctrl+> <ctrl-> <model_name> [ON|OFF] [M=<m>]
```

| Parameter | Description |
|-----------|-------------|
| `node1`, `node2` | Switch terminal nodes |
| `ctrl+`, `ctrl-` | Controlling voltage nodes |
| `model_name` | Name of a `.MODEL VSWITCH(...)` definition |
| `ON` / `OFF` | Initial state |
| `M=m` | Multiplier |

## Example

```spice
S1 OUT 0 CTRL 0 SW_MOD
.MODEL SW_MOD VSWITCH(VT=2.5 VH=0.5 RON=1 ROFF=1MEG)
```

## MNA View

A voltage-controlled switch is a resistor whose value is controlled by another
voltage. After the control voltage chooses an on, off, or transition value, the
switch stamps like a resistor between `node1` and `node2`.

Conceptually:

```text
read V(ctrl+, ctrl-)
choose R between RON and ROFF
stamp g = 1 / R into the node matrix
```

The switch has no stored energy, so it does not own integration history. It can
still make `.TRAN` harder because a sharp control edge can change the matrix
conductance suddenly, which may force smaller timesteps or extra Newton
iterations.

For the shared switch stamp, see
[How SpiceSharp Solves Circuits](spicesharp-architecture.md#s-and-w-controlled-switches).

## Model Parameters

| Parameter | Description |
|-----------|-------------|
| `VT` | Threshold voltage |
| `VH` | Hysteresis voltage |
| `RON` | On-state resistance |
| `ROFF` | Off-state resistance |

## Behavior

- When `V(ctrl+, ctrl-) > VT + VH`, the switch turns ON (resistance = RON).
- When `V(ctrl+, ctrl-) < VT - VH`, the switch turns OFF (resistance = ROFF).
- Between these thresholds, the switch maintains its current state (hysteresis).
