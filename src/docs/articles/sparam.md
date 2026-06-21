# .SPARAM Statement

The `.SPARAM` statement defines scalar parameters, similar to `.PARAM` but with immediate (eager) evaluation. The expression is evaluated to a numeric value at parse time rather than stored as a symbolic expression.

## Syntax

```
.SPARAM <name>=<value> [<name2>=<value2> ...]
```

## Example

```spice
.SPARAM vdd=3.3
.SPARAM r1_val=1k r2_val=2k
```

## Difference from .PARAM

| Feature | .PARAM | .SPARAM |
|---------|--------|---------|
| Evaluation | Deferred (symbolic) | Immediate (scalar) |
| Expression support | Yes | Yes (evaluated immediately) |
| Use in `.STEP` | Can be swept | Pre-computed before sweep |

## MNA View

`.SPARAM` also does not stamp MNA by itself. It eagerly computes a scalar value
that may later be used by a component, source, model, or analysis setting.

The MNA impact happens only through the statement that consumes the value. For
example, if `.SPARAM r1=1k` is used by `R1 IN OUT {r1}`, the resistor stamps
`1/1k` into the matrix.

Use `.SPARAM` when you need a parameter to be fully resolved before any simulation setup occurs.
