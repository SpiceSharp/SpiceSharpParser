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

### Laplace

```
G<name> <out+> <out-> LAPLACE {V(<ctrl+>)} = {<transfer>}
G<name> <out+> <out-> LAPLACE {V(<ctrl+>,<ctrl->)} = {<transfer>}
```

`<transfer>` is a rational polynomial in `s`. It maps the controlling voltage to output current, so its units are transconductance.

Examples:

```spice
* First-order transconductance low-pass
.PARAM gm=1m
.PARAM fc=1k
.PARAM wc={2*PI*fc}
GLOW OUT 0 LAPLACE {V(IN)} = {gm*wc/(s+wc)}

* Differential input
GDIFF OUT 0 LAPLACE {V(INP,INN)} = {gm/(1+s*1u)}
```

Current limitations:

- Only input expressions `V(node)` and `V(node1,node2)` are accepted.
- Alternate LAPLACE syntaxes without the expression assignment form are not supported yet.
- `M=`, `TD=`, `DELAY=`, and explicit internal-state options are not supported yet.
- Transfers must be proper, finite rational polynomials in `s` with non-singular DC gain.

| Parameter | Description |
|-----------|-------------|
| `out+`, `out-` | Output nodes |
| `ctrl+`, `ctrl-` | Controlling voltage nodes |
| `transconductance` | Gain in siemens (Iout = gm × Vctrl) |
| `M=m` | Multiplier. For linear `G` sources it scales the effective transconductance/current contribution, like multiple equivalent parallel instances. |

For LAPLACE sources, `M=` is recognized but not supported yet. Until it is implemented, put the multiplier directly in the transfer expression, for example `{m*gm*wc/(s+wc)}`.

## Examples

```spice
* Linear VCCS: 0.1 S transconductance
G1 OUT 0 IN 0 0.1

* Behavioral
G2 OUT 0 VALUE={V(IN)*0.01}

* Laplace
G3 OUT 0 LAPLACE {V(IN)} = {1m/(1+s*1u)}
```
