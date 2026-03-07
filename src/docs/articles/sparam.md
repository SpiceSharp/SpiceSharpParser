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

Use `.SPARAM` when you need a parameter to be fully resolved before any simulation setup occurs.
