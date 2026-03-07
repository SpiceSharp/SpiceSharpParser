# G — Voltage-Controlled Current Source (VCCS)

A voltage-controlled current source produces an output current proportional to a controlling voltage.

## Syntax

### Linear

```
G<name> <out+> <out-> <ctrl+> <ctrl-> <transconductance> [M=<m>]
```

### Polynomial

```
G<name> <out+> <out-> POLY(<dimension>) <ctrl_nodes...> <coefficients...>
```

### Behavioral

```
G<name> <out+> <out-> VALUE={<expression>}
G<name> <out+> <out-> TABLE={<expression>} = (<x1>,<y1>) (<x2>,<y2>) ...
```

| Parameter | Description |
|-----------|-------------|
| `out+`, `out-` | Output nodes |
| `ctrl+`, `ctrl-` | Controlling voltage nodes |
| `transconductance` | Gain in siemens (Iout = gm × Vctrl) |
| `M=m` | Multiplier |

## Examples

```spice
* Linear VCCS: 0.1 S transconductance
G1 OUT 0 IN 0 0.1

* Behavioral
G2 OUT 0 VALUE={V(IN)*0.01}
```
