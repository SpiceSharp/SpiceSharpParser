# R — Resistor

The resistor is the most basic passive component, opposing the flow of current according to Ohm's law.

## Syntax

```
R<name> <node+> <node-> <value> [TC=<tc1>[,<tc2>]] [M=<m>]
R<name> <node+> <node-> <model_name> [L=<length>] [W=<width>]
R<name> <node+> <node-> VALUE={<expression>}
```

| Parameter | Description |
|-----------|-------------|
| `node+`, `node-` | Positive and negative terminal nodes |
| `value` | Resistance in ohms |
| `TC=tc1[,tc2]` | Temperature coefficients (linear, quadratic) |
| `M=m` | Multiplier (parallel instances) |
| `model_name` | Reference to a `.MODEL` definition |
| `VALUE={expr}` | Behavioral expression |

## Examples

```spice
* Basic resistor
R1 IN OUT 1k

* With engineering notation
R2 A B 4.7MEG

* Temperature-dependent
R3 IN OUT 10k TC=0.001,0.0001

* Model-based with geometry
R4 IN OUT rmod L=10u W=2u

* Behavioral (expression-based)
R5 IN OUT VALUE={1k * (1 + V(CTRL))}
```

## Model Definition

```spice
.MODEL rmod R(RSH=100 NARROW=0.1u)
```

## Temperature Dependence

With temperature coefficients:

$$R(T) = R_0 \times [1 + TC_1 \times (T - T_{nom}) + TC_2 \times (T - T_{nom})^2]$$
