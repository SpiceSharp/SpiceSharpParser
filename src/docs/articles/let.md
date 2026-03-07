# .LET Statement

The `.LET` statement defines a named expression that can be referenced later in the netlist. Unlike `.PARAM`, `.LET` stores the expression itself (not just the evaluated value), making it useful for deferred calculations.

## Syntax

```
.LET <name> <expression>
```

| Parameter | Description |
|-----------|-------------|
| `name` | Name of the expression |
| `expression` | A mathematical expression (may use `{}` delimiters) |

## Examples

```spice
.LET power {V(OUT)*I(V1)}
.LET gain {V(OUT)/V(IN)}
```

## Difference from .PARAM

- `.PARAM` stores a parameter value that is evaluated at parse time and used for component values.
- `.LET` stores a named expression that can be evaluated dynamically during simulation, often used with `.MEAS` or other post-processing.
