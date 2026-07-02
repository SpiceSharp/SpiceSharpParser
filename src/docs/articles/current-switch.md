# W — Current Switch

A current-controlled switch changes between high and low resistance states based on the current through a controlling voltage source.

## Syntax

```
W<name> <node1> <node2> <Vcontrol> <model_name> [ON|OFF] [M=<m>]
```

| Parameter | Description |
|-----------|-------------|
| `node1`, `node2` | Switch terminal nodes |
| `Vcontrol` | Name of voltage source sensing the control current |
| `model_name` | Name of a `.MODEL ISWITCH(...)` definition |
| `ON` / `OFF` | Initial state |
| `M=m` | Multiplier |

## Example

```spice
W1 OUT 0 Vsense ISMOD
Vsense CTRL_A CTRL_B 0
.MODEL ISMOD ISWITCH(IT=1m IH=0.1m RON=1 ROFF=1MEG)
```

## MNA View

A current-controlled switch is also a resistor-like stamp, but its control value
is the current through a named voltage source. That current already exists as an
MNA branch-current unknown.

Conceptually:

```text
read I(Vcontrol)
choose R between RON and ROFF
stamp g = 1 / R into the node matrix
```

The switch itself has no capacitor-like or inductor-like history. Its transient
importance comes from topology changes: when the control current crosses the
threshold, the effective conductance can move quickly between `1/RON` and
`1/ROFF`.

For the shared switch stamp, see
[How SpiceSharp Solves Circuits](spicesharp-architecture.md#s-and-w-controlled-switches).

## Model Parameters

| Parameter | Description |
|-----------|-------------|
| `IT` | Threshold current |
| `IH` | Hysteresis current |
| `RON` | On-state resistance |
| `ROFF` | Off-state resistance |

## Notes

- A 0V voltage source is used as a current sensor in the control path.
- Behavior is analogous to the voltage switch (S), but controlled by current instead of voltage.
