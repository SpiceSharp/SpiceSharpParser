# H — Current-Controlled Voltage Source (CCVS)

A current-controlled voltage source produces an output voltage proportional to a controlling current (measured through a voltage source).

## Syntax

### Linear

```
H<name> <out+> <out-> <Vcontrol> <transimpedance>
```

### Polynomial

```
H<name> <out+> <out-> POLY(<dimension>) <Vcontrols...> <coefficients...>
```

### Laplace

```
H<name> <out+> <out-> LAPLACE {I(<Vcontrol>)} = {<transfer>} [M=<m>] [TD=<delay>|DELAY=<delay>]
H<name> <out+> <out-> LAPLACE {I(<Vcontrol>)} {<transfer>} [M=<m>] [TD=<delay>|DELAY=<delay>]
H<name> <out+> <out-> LAPLACE = {I(<Vcontrol>)} {<transfer>} [M=<m>] [TD=<delay>|DELAY=<delay>]
```

| Parameter | Description |
|-----------|-------------|
| `out+`, `out-` | Output nodes |
| `Vcontrol` | Name of a voltage source sensing the control current |
| `transimpedance` | Gain in ohms (Vout = Rm × Ictrl) |

## Examples

```spice
* Transimpedance of 500 ohms
H1 OUT 0 Vsense 500

* Frequency-shaped transimpedance
H2 OUT 0 LAPLACE {I(Vsense)} = {500/(1+s*1u)}

* Requires a 0V sense source
Vsense A B 0
```

## Notes

- Like the `F` source, the controlling current is measured through a named voltage source.
- A 0V voltage source acts as an ideal ammeter.
- For LAPLACE sources, the transfer must be a proper rational polynomial in `s` with finite DC gain. See [LAPLACE Transfer Sources](laplace.md).
