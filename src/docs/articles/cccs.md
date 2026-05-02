# F — Current-Controlled Current Source (CCCS)

A current-controlled current source produces an output current proportional to a controlling current (measured through a voltage source).

## Syntax

### Linear

```
F<name> <out+> <out-> <Vcontrol> <gain> [M=<m>]
```

### Polynomial

```
F<name> <out+> <out-> POLY(<dimension>) <Vcontrols...> <coefficients...>
```

### Laplace

```
F<name> <out+> <out-> LAPLACE {I(<Vcontrol>)} = {<transfer>} [M=<m>] [TD=<delay>|DELAY=<delay>]
F<name> <out+> <out-> LAPLACE {I(<Vcontrol>)} {<transfer>} [M=<m>] [TD=<delay>|DELAY=<delay>]
F<name> <out+> <out-> LAPLACE = {I(<Vcontrol>)} {<transfer>} [M=<m>] [TD=<delay>|DELAY=<delay>]
```

| Parameter | Description |
|-----------|-------------|
| `out+`, `out-` | Output nodes (current flows from out+ to out-) |
| `Vcontrol` | Name of a voltage source sensing the control current |
| `gain` | Current gain (Iout = gain × Ictrl) |
| `M=m` | Multiplier |

## Examples

```spice
* Simple current mirror with gain 2
F1 OUT 0 Vsense 2

* Frequency-shaped current mirror
F2 OUT 0 LAPLACE {I(Vsense)} = {2/(1+s*1u)}

* Requires a 0V sense source in the control path
Vsense CTRL_A CTRL_B 0
```

## Notes

- The controlling current is the current through the named voltage source.
- A 0V voltage source is commonly used as a current sensor (ammeter).
- For LAPLACE sources, the transfer must be a proper rational polynomial in `s` with finite DC gain. See [LAPLACE Transfer Sources](laplace.md).
