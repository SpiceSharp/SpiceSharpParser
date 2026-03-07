# .IF / .ELSE / .ENDIF Statements

The `.IF` / `.ELSE` / `.ENDIF` statements provide conditional inclusion of netlist sections based on parameter expressions evaluated at parse time.

## Syntax

```
.IF (<condition>)
  ...statements included when condition is true...
.ELSE
  ...statements included when condition is false...
.ENDIF
```

The `.ELSE` block is optional.

## Examples

### Simple Condition

```spice
.PARAM use_fast=1

.IF (use_fast == 1)
R1 IN OUT 100
.ELSE
R1 IN OUT 10k
.ENDIF
```

### Without .ELSE

```spice
.PARAM add_cap=1

.IF (add_cap > 0)
C1 OUT 0 100p
.ENDIF
```

### Nested (with Parameters)

```spice
.PARAM version=2

.IF (version == 1)
.INCLUDE "model_v1.lib"
.ELSE
.INCLUDE "model_v2.lib"
.ENDIF
```

## Condition Expressions

The condition can use any expression that evaluates to a numeric value:

- Non-zero = true
- Zero = false

Standard comparison operators (`==`, `!=`, `>`, `<`, `>=`, `<=`) and logical operators are supported.

## Notes

- Conditions are evaluated at parse time using the current parameter values.
- The conditional blocks can contain any valid SPICE statements: components, models, subcircuits, analysis commands, etc.
