# .STEP Statement

The `.STEP` statement defines a parameter sweep, causing the simulation to run multiple times with different parameter values.

## Syntax

### Linear Sweep
```
.STEP [LIN] <variable> <start> <stop> <step>
.STEP PARAM <variable> [LIN] <start> <stop> <step>
```

### Logarithmic Sweep (Decade)
```
.STEP DEC <variable> <start> <stop> <points_per_decade>
.STEP PARAM <variable> DEC <start> <stop> <points_per_decade>
```

### Logarithmic Sweep (Octave)
```
.STEP OCT <variable> <start> <stop> <points_per_octave>
.STEP PARAM <variable> OCT <start> <stop> <points_per_octave>
```

### List of Values
```
.STEP <variable> LIST <val1> <val2> [<val3> ...]
.STEP PARAM <variable> LIST <val1> <val2> [<val3> ...]
```

| Parameter | Description |
|-----------|-------------|
| `PARAM` | Optional keyword indicating a user parameter sweep |
| `variable` | Source name or parameter name to sweep |
| `LIN`/`DEC`/`OCT`/`LIST` | Sweep type (LIN is default if omitted) |

## Examples

```spice
* Sweep a voltage source linearly
.STEP V1 1 5 0.5

* Sweep a parameter
.STEP PARAM res 1k 10k 1k

* Logarithmic sweep
.STEP DEC PARAM freq 1 1MEG 10

* List of discrete values
.STEP PARAM temp_val LIST 25 50 75 100
```

## Multiple Sweeps

Multiple `.STEP` statements create nested sweeps:

```spice
.STEP PARAM r_val LIST 1k 2k 5k
.STEP PARAM c_val LIST 1n 10n 100n
```

This runs 3 × 3 = 9 simulations.

## Typical Usage

```spice
Gain vs. feedback resistor
V1 IN 0 AC 1
R1 IN OUT {rfb}
R2 OUT 0 1k
.PARAM rfb=1k
.STEP PARAM rfb LIST 1k 2k 5k 10k
.AC DEC 10 1 1MEG
.SAVE V(OUT)
.END
```

## C# API

Each sweep iteration fires `EventExportData` events. Access exports as normal — the parameter value is updated automatically for each iteration.
