# E — Voltage-Controlled Voltage Source (VCVS)

A voltage-controlled voltage source produces an output voltage proportional to a controlling voltage.

## Syntax

### Linear

```
E<name> <out+> <out-> <ctrl+> <ctrl-> <gain>
```

### Polynomial

```
E<name> <out+> <out-> POLY(<dimension>) <ctrl_nodes...> <coefficients...>
```

### Behavioral

```
E<name> <out+> <out-> VALUE={<expression>}
E<name> <out+> <out-> TABLE={<expression>} = (<x1>,<y1>) (<x2>,<y2>) ...
```

### Laplace

```
E<name> <out+> <out-> LAPLACE {<input>} = {<transfer>} [M=<m>] [TD=<delay>|DELAY=<delay>]
E<name> <out+> <out-> LAPLACE {<input>} {<transfer>} [M=<m>] [TD=<delay>|DELAY=<delay>]
E<name> <out+> <out-> LAPLACE = {<input>} {<transfer>} [M=<m>] [TD=<delay>|DELAY=<delay>]
```

`<input>` is `V(node)` or `V(node1,node2)`. `<transfer>` is a rational polynomial in `s`. Coefficients are evaluated from constants and `.PARAM` values, and are stored in ascending powers of `s`.

Examples:

```spice
* First-order low-pass, H(s)=1/(1+s*tau)
.PARAM tau=1u
ELOW OUT 0 LAPLACE {V(IN)} = {1/(1+s*tau)}

* Differential input, H(s)=wc/(s+wc)
.PARAM fc=1k
.PARAM wc={2*PI*fc}
EDIFF OUT 0 LAPLACE {V(INP,INN)} = {wc/(s+wc)}
```

Current limitations:

- `B`, `F`, and `H` LAPLACE forms are not supported yet.
- Only input expressions `V(node)` and `V(node1,node2)` are accepted.
- Function-like `VALUE={LAPLACE(...)}` syntax is not supported yet.
- Explicit internal-state options are not supported yet.
- Transfers must be proper, finite rational polynomials in `s` with non-singular DC gain.

For LAPLACE sources, `M=` is a finite constant multiplier folded into the numerator coefficients; it may be positive, negative, or zero. `TD=` and `DELAY=` are supported aliases for a finite constant non-negative runtime delay parameter; use only one delay option and assignment syntax such as `TD=1n`.

For the transfer-function math, DC gain, frequency response, and phase examples, see [LAPLACE Transfer Sources](laplace.md).

| Parameter | Description |
|-----------|-------------|
| `out+`, `out-` | Output nodes |
| `ctrl+`, `ctrl-` | Controlling voltage nodes |
| `gain` | Voltage gain (Vout = gain × Vctrl) |

## Examples

```spice
* Linear VCVS with gain of 100
E1 OUT 0 IN 0 100

* Behavioral expression
E2 OUT 0 VALUE={V(IN)*100 + V(BIAS)}

* Polynomial
E3 OUT 0 POLY(1) IN 0 0 1 0.5

* Table lookup
E4 OUT 0 TABLE={V(IN)} = (0,0) (1,3.3) (2,5)

* Laplace low-pass
E5 OUT 0 LAPLACE {V(IN)} = {1/(1+s*1u)}

* Laplace with multiplier and delay
E5D OUTD 0 LAPLACE {V(IN)} = {1/(1+s*1u)} M=2 TD=1n

* Laplace with negative multiplier
E5INV OUTINV 0 LAPLACE {V(IN)} = {1/(1+s*1u)} M=-1

* Equivalent supported spellings
E6 OUT 0 LAPLACE {V(IN)} {1/(1+s*1u)}
E7 OUT 0 LAPLACE = {V(IN)} {1/(1+s*1u)}
```
