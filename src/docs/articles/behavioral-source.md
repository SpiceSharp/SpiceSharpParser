# B — Arbitrary Behavioral Source

The behavioral source creates a voltage or current source whose value is defined by a mathematical expression. This is part of analog behavioral modeling (ABM).

## Syntax

```
B<name> <node+> <node-> V={<expression>}
B<name> <node+> <node-> I={<expression>}
```

| Parameter | Description |
|-----------|-------------|
| `node+`, `node-` | Terminal nodes |
| `V={expr}` | Behavioral voltage source |
| `I={expr}` | Behavioral current source |

## Examples

```spice
* Voltage source: half the input
B1 OUT 0 V={V(IN)*0.5}

* Current source: proportional to voltage
B2 OUT 0 I={V(CTRL)*1m}

* Full-wave rectifier
B3 OUT 0 V={abs(V(IN))}

* Conditional (comparator)
B4 OUT 0 V={if(V(IN)>2.5, 5, 0)}

* Multiplier
B5 OUT 0 V={V(A)*V(B)}
```

## Expressions

The expression can use:

- Node voltages: `V(node)`, `V(node1, node2)`
- Branch currents: `I(Vsource)`
- Mathematical functions: `abs()`, `sqrt()`, `exp()`, `log()`, `sin()`, `cos()`, `if()`, `min()`, `max()`, etc.
- Arithmetic operators: `+`, `-`, `*`, `/`, `**` (power)
- Parameters defined with `.PARAM`

## Supported ABM Forms

SpiceSharpParser supports these analog behavioral modeling constructs:

| Form | Description |
|------|-------------|
| `VALUE={expr}` | Expression-based source (equivalent to `V=` or `I=`) |
| `TABLE={expr}` | Lookup table with piecewise-linear interpolation |
| `POLY(n)` | Polynomial transfer function |
| `E ... LAPLACE {V(...)} = {H(s)}` | Voltage-controlled voltage transfer function |
| `G ... LAPLACE {V(...)} = {H(s)}` | Voltage-controlled current transfer function |
| `E/G ... LAPLACE {V(...)} {H(s)}` | Alternate supported LAPLACE spelling |
| `E/G ... LAPLACE = {V(...)} {H(s)}` | Alternate supported LAPLACE spelling |

`LAPLACE` support is currently limited to `E` and `G` voltage-controlled sources with `V(node)` or `V(node1,node2)` input. `B`, `F`, `H`, function-like `VALUE={LAPLACE(...)}` syntax, `M=`, `TD=`, and `DELAY=` are not supported yet.

`M=` is a multiplier on sources/devices where supported, usually equivalent to multiple parallel instances or a scaled effective contribution. For LAPLACE sources, put that factor directly in `H(s)` for now.
