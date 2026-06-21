# .DISTRIBUTION Statement

The `.DISTRIBUTION` statement defines a custom probability density function (PDF) for use with Monte Carlo analysis.

## Syntax

```
.DISTRIBUTION <name> (<x1>,<y1>) (<x2>,<y2>) [(<x3>,<y3>) ...]
```

| Parameter | Description |
|-----------|-------------|
| `name` | Name of the distribution |
| `(x, y)` | Coordinate pairs defining the PDF curve |

The PDF is defined as a piecewise-linear function through the given (x, y) points.

## Examples

```spice
* Uniform distribution from -1 to 1
.DISTRIBUTION uniform (-1,1) (1,1)

* Triangular distribution centered at 0
.DISTRIBUTION triangular (-1,0) (0,1) (1,0)
```

## Usage with Monte Carlo

Set the default distribution for `.MC` analysis:

```spice
.DISTRIBUTION my_dist (-1,0) (0,2) (1,0)
.OPTIONS DISTRIBUTION=my_dist
.MC 100 TRAN V(OUT) MAX
```

## MNA View

`.DISTRIBUTION` does not stamp MNA. It defines how Monte Carlo values are chosen.
Those chosen values can then change the device parameters that later stamp the
matrix.

For example, if a distribution varies resistor value, the MNA matrix changes
because the resistor conductance `1/R` changes in each Monte Carlo run.

## Notes

- The y-values define relative probability — the simulator normalizes automatically.
- Built-in distributions are available without explicit definition.
