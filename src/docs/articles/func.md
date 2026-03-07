# .FUNC Statement

The `.FUNC` statement defines user functions that can be used in expressions throughout the netlist.

## Syntax

```
.FUNC <name>(<arg1>[, <arg2>, ...]) = <expression>
```

An alternative bracket syntax is also supported:

```
.FUNC <name>[<arg1>, <arg2>] <expression>
```

## Examples

```spice
* Simple function
.FUNC square(x) = x*x

* Multi-argument function
.FUNC parallel(r1, r2) = (r1*r2)/(r1+r2)

* Multiple functions on one line
.FUNC db(x)=20*log10(x) rad(x)=x*3.14159/180
```

## Using Functions

Functions can be called in any expression context using curly braces:

```spice
.FUNC parallel(r1, r2) = (r1*r2)/(r1+r2)
R1 IN OUT {parallel(1k, 2k)}
```

## Built-in Functions

SpiceSharpParser includes standard math functions:

| Function | Description |
|----------|-------------|
| `abs(x)` | Absolute value |
| `sqrt(x)` | Square root |
| `exp(x)` | Exponential |
| `log(x)` | Natural logarithm |
| `log10(x)` | Base-10 logarithm |
| `sin(x)`, `cos(x)`, `tan(x)` | Trigonometric |
| `asin(x)`, `acos(x)`, `atan(x)` | Inverse trigonometric |
| `atan2(y, x)` | Two-argument arctangent |
| `sinh(x)`, `cosh(x)`, `tanh(x)` | Hyperbolic |
| `min(x, y)` | Minimum |
| `max(x, y)` | Maximum |
| `pow(x, y)` | Power |
| `pwr(x, y)` | `sgn(x) * pow(abs(x), y)` |
| `floor(x)` | Floor |
| `ceil(x)` | Ceiling |
| `if(cond, t, f)` | Conditional |
| `limit(x, lo, hi)` | Clamp to range |
