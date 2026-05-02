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
G<name> <out+> <out-> LAPLACE {V(<ctrl+>)} = {<transfer>} [M=<m>] [TD=<delay>|DELAY=<delay>]
G<name> <out+> <out-> LAPLACE {V(<ctrl+>,<ctrl->)} = {<transfer>} [M=<m>] [TD=<delay>|DELAY=<delay>]
G<name> <out+> <out-> LAPLACE {V(<ctrl+>)} {<transfer>} [M=<m>] [TD=<delay>|DELAY=<delay>]
G<name> <out+> <out-> LAPLACE = {V(<ctrl+>)} {<transfer>} [M=<m>] [TD=<delay>|DELAY=<delay>]
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
- Function-like `VALUE={LAPLACE(...)}` and `B`-source LAPLACE syntax are not supported yet.
- Explicit internal-state options are not supported yet.
- Transfers must be proper, finite rational polynomials in `s` with non-singular DC gain.

| Parameter | Description |
|-----------|-------------|
| `out+`, `out-` | Output nodes |
| `ctrl+`, `ctrl-` | Controlling voltage nodes |
| `transconductance` | Gain in siemens (Iout = gm × Vctrl) |
| `M=m` | Multiplier. For linear `G` sources it scales the effective transconductance/current contribution, like multiple equivalent parallel instances. |

For LAPLACE sources, `M=` is folded into the numerator coefficients. `TD=` and `DELAY=` are supported aliases for a constant non-negative runtime delay parameter; use only one delay option.

For the transfer-function math, current-source sign convention, frequency response, and phase examples, see [LAPLACE Transfer Sources](laplace.md).

## Examples

```spice
* Linear VCCS: 0.1 S transconductance
G1 OUT 0 IN 0 0.1

* Behavioral
G2 OUT 0 VALUE={V(IN)*0.01}

* Laplace
G3 OUT 0 LAPLACE {V(IN)} = {1m/(1+s*1u)}

* Laplace with multiplier and delay
G3D OUTD 0 LAPLACE {V(IN)} = {1m/(1+s*1u)} M=2 DELAY=1n

* Equivalent supported spellings
G4 OUT 0 LAPLACE {V(IN)} {1m/(1+s*1u)}
G5 OUT 0 LAPLACE = {V(IN)} {1m/(1+s*1u)}
```
