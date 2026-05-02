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

* Laplace low-pass voltage output
B6 OUT 0 V={LAPLACE(V(IN), 1/(1+s*tau))}

* Mixed expression with an internal Laplace helper
B7 OUT 0 V={1 + 2*LAPLACE(V(IN), 1/(1+s))}
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
| `F ... LAPLACE {I(...)} = {H(s)}` | Current-controlled current transfer function |
| `H ... LAPLACE {I(...)} = {H(s)}` | Current-controlled voltage transfer function |
| `E/G/F/H ... LAPLACE {input} {H(s)}` | Alternate supported LAPLACE spelling |
| `E/G/F/H ... LAPLACE = {input} {H(s)}` | Alternate supported LAPLACE spelling |
| `VALUE={LAPLACE(input,H(s))}` | Function-style Laplace transfer |
| `B ... V={LAPLACE(input,H(s))}` | Function-style voltage-output Laplace transfer |
| `B ... I={LAPLACE(input,H(s))}` | Function-style current-output Laplace transfer |

`LAPLACE` support covers source-level `E` and `G` voltage-controlled sources with `V(node)` or `V(node1,node2)` input, plus `F` and `H` current-controlled sources with `I(source)` input. Function-style `LAPLACE(input, transfer)` is also supported in `VALUE`, `B ... V=`, and `B ... I=` expressions with `V(node)`, `V(node1,node2)`, or `I(source)` input.

When `LAPLACE(...)` is part of a larger expression, SpiceSharpParser creates internal helper voltage sources and rewrites the behavioral expression to reference those helper node voltages. The helpers are implementation details, but may be visible through low-level circuit inspection APIs.

For LAPLACE sources, `M=` is a finite constant multiplier and may be positive, negative, or zero. `TD=` and `DELAY=` are supported aliases for a finite constant non-negative runtime delay parameter; use only one delay option and assignment syntax. For function-style expressions, delay options require exactly one `LAPLACE(...)` call.

For the transfer-function math, DC gain, frequency response, and worked examples, see [LAPLACE Transfer Sources](laplace.md).
