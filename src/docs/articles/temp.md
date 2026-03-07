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
