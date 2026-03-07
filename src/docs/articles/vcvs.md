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
```
