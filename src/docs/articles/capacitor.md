# C — Capacitor

A capacitor stores energy in an electric field between two conductors.

## Syntax

```
C<name> <node+> <node-> <value> [IC=<initial_voltage>] [M=<m>]
C<name> <node+> <node-> <model_name> [L=<length>] [W=<width>] [IC=<v>]
C<name> <node+> <node-> VALUE={<expression>}
```

| Parameter | Description |
|-----------|-------------|
| `node+`, `node-` | Positive and negative terminal nodes |
| `value` | Capacitance in farads |
| `IC=v` | Initial voltage across the capacitor (for `UIC`) |
| `TC=tc1[,tc2]` | Temperature coefficients |
| `M=m` | Multiplier |
| `model_name` | Reference to a `.MODEL` definition |
| `VALUE={expr}` | Behavioral expression |

## Examples

```spice
* Basic capacitor
C1 OUT 0 1u

* With initial condition
C2 OUT 0 100n IC=5

* Picofarads
C3 A B 10p

* Model-based
C4 IN OUT cmod L=10u W=1u IC=1

* Behavioral
C5 IN OUT VALUE={M*N*1p}
```

## Model Definition

```spice
.MODEL cmod C(CJ=1e-12)
```
