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
E<name> <out+> <out-> LAPLACE {V(<ctrl+>)} = {<transfer>}
E<name> <out+> <out-> LAPLACE {V(<ctrl+>,<ctrl->)} = {<transfer>}
E<name> <out+> <out-> LAPLACE {V(<ctrl+>)} {<transfer>}
E<name> <out+> <out-> LAPLACE = {V(<ctrl+>)} {<transfer>}
```

`<transfer>` is a rational polynomial in `s`. Coefficients are evaluated from constants and `.PARAM` values, and are stored in ascending powers of `s`.

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
- `M=`, `TD=`, `DELAY=`, and explicit internal-state options are not supported yet.
- Transfers must be proper, finite rational polynomials in `s` with non-singular DC gain.

`M=` normally acts as a multiplier for source/device contribution where supported. For LAPLACE sources it is recognized but not supported yet; put any multiplier directly in the transfer expression, for example `{m/(1+s*tau)}`.

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

* Equivalent supported spellings
E6 OUT 0 LAPLACE {V(IN)} {1/(1+s*1u)}
E7 OUT 0 LAPLACE = {V(IN)} {1/(1+s*1u)}
```
