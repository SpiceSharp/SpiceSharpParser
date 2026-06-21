# .TEMP Statement

The `.TEMP` statement specifies one or more simulation temperatures. When multiple temperatures are given, the simulation runs at each temperature.

## Syntax

```
.TEMP <temp1> [<temp2> ...]
```

Temperatures are specified in degrees Celsius.

## Examples

```spice
* Single temperature
.TEMP 85

* Multiple temperatures — simulation runs at each
.TEMP -40 25 85 125
```

## Interaction with .OPTIONS TEMP

If both `.TEMP` and `.OPTIONS TEMP=` are specified, `.TEMP` takes precedence and overrides any previous `.OPTIONS TEMP` setting.

## MNA View

Temperature does not add its own MNA row. It changes device parameters before
the devices load their stamps.

Examples:

| Device | Temperature can change |
|--------|------------------------|
| Resistor | Effective resistance and conductance. |
| Diode/BJT | Saturation currents, junction voltages, and derivatives. |
| MOSFET/JFET | Model currents and local small-signal slopes. |

If multiple temperatures are requested, SpiceSharp runs the simulation at each
temperature. Each run reloads the MNA matrix using the parameter values for that
temperature.

For the matrix-loading picture, see
[How SpiceSharp Solves Circuits](spicesharp-architecture.md#modified-matrix-algorithm-step-by-step).

## Typical Usage

```spice
Temperature sweep
V1 IN 0 1
R1 IN OUT 1k
C1 OUT 0 1u
.TEMP -40 0 25 85 125
.TRAN 1e-6 10e-3
.SAVE V(OUT)
.END
```

## Notes

- The internal simulation uses Kelvin (Celsius + 273.15).
- Temperature affects device model parameters (e.g., diode saturation current, resistor temperature coefficients).
- For a continuous temperature sweep, use `.STEP PARAM temp_val` with `.OPTIONS TEMP={temp_val}`.
