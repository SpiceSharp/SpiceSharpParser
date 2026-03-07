# .MEAS / .MEASURE Statement

The `.MEAS` (or `.MEASURE`) statement allows you to extract scalar measurements from simulation results — such as rise time, delay, average voltage, RMS, or custom expressions. Both `.MEAS` and `.MEASURE` are accepted as aliases.

## General Syntax

```
.MEAS <analysis_type> <name> <measurement_spec>
.MEASURE <analysis_type> <name> <measurement_spec>
```

- **analysis_type**: `TRAN`, `AC`, `DC`, `OP`, or `NOISE`
- **name**: user-defined measurement name (used to access results)
- **measurement_spec**: one of the measurement types described below

## Supported Measurement Types

### TRIG/TARG — Timing Measurements

Measures the time (or frequency/sweep) difference between a trigger event and a target event.

```
.MEAS TRAN rise_time TRIG V(out) VAL=1.0 RISE=1 TARG V(out) VAL=9.0 RISE=1
.MEAS TRAN tpd TRIG V(in) VAL=2.5 RISE=1 TARG V(out) VAL=2.5 RISE=1
.MEAS TRAN delayed TRIG V(out) VAL=1.0 RISE=1 TD=5u TARG V(out) VAL=9.0 RISE=1
```

**Qualifiers:**
- `VAL=<value>` — threshold value
- `RISE=<n>` — match the nth rising crossing
- `FALL=<n>` — match the nth falling crossing
- `CROSS=<n>` — match the nth crossing (either direction)
- `TD=<time>` — time delay before starting the search

### WHEN — Threshold Crossing

Finds the independent variable value (time, frequency, sweep) when a signal crosses a threshold.

```
.MEAS TRAN t50 WHEN V(out)=5.0
.MEAS TRAN t50 WHEN V(out) VAL=5.0 CROSS=1
.MEAS TRAN t_rise2 WHEN V(out)=2.5 RISE=2
```

Both combined syntax (`WHEN V(out)=5.0`) and separate syntax (`WHEN V(out) VAL=5.0`) are supported.

### FIND/WHEN — Value at Threshold

Finds the value of one signal at the moment another signal crosses a threshold.

```
.MEAS TRAN vout_at_cross FIND V(out) WHEN V(in)=2.5
.MEAS TRAN i_at_5v FIND I(R1) WHEN V(out)=5.0
.MEAS TRAN v_at_rise2 FIND V(out) WHEN V(in)=2.5 RISE=2
```

### MIN, MAX, AVG, RMS, PP — Statistical Measurements

Compute statistical properties of a signal over the simulation (or a windowed region).

```
.MEAS TRAN vmax MAX V(out)
.MEAS TRAN vmin MIN V(out)
.MEAS TRAN vavg AVG V(out) FROM=1u TO=9u
.MEAS TRAN vrms RMS V(out) FROM=0 TO=10u
.MEAS TRAN vpp PP V(out)
```

- `AVG` uses trapezoidal mean: integral(y dx) / (x_end - x_start)
- `RMS` uses trapezoidal integration: sqrt(integral(y² dx) / (x_end - x_start))
- `PP` computes max - min (peak-to-peak)

### INTEG — Integration

Computes the integral of a signal over time (or frequency/sweep).

```
.MEAS TRAN charge INTEG I(C1) FROM=0 TO=10u
.MEAS TRAN vt_integral INTEG V(out)
```

### DERIV — Derivative

Computes the derivative of a signal at a specific point.

```
.MEAS TRAN slope DERIV V(out) AT=5u
```

### PARAM — Computed Expression

Evaluates an expression referencing previously defined measurements.

```
.MEAS TRAN vmax MAX V(out)
.MEAS TRAN vmin MIN V(out)
.MEAS TRAN ratio PARAM='vmax/vmin'
.MEAS TRAN span PARAM='vmax-vmin'
```

PARAM measurements must appear **after** the measurements they reference.

## FROM/TO Windowing

`FROM` and `TO` restrict the measurement to a specific range of the independent variable:

```
.MEAS TRAN vmax_win MAX V(out) FROM=5u TO=15u
.MEAS TRAN vavg_win AVG V(out) FROM=2u TO=8u
```

## AC Measurements

For AC analysis, use the appropriate export types for magnitude, phase, and decibel values:

| Export | Description |
|--------|-------------|
| `VM(node)` | Voltage magnitude |
| `VDB(node)` | Voltage in decibels |
| `VP(node)` | Voltage phase |
| `VR(node)` | Voltage real part |
| `VI(node)` | Voltage imaginary part |

Example:
```
.MEAS AC bw WHEN VM(out)=0.707
.MEAS AC max_gain MAX VM(out)
.MEAS AC bw_db WHEN VDB(out)=-3
```

> **Note:** The nested function syntax `mag(V(out))` is not supported. Use `VM(out)` instead.

## Accessing Results in C#

Results are available via `SpiceSharpModel.Measurements`, which is a `ConcurrentDictionary<string, List<MeasurementResult>>`:

```csharp
var model = reader.Read(netlist);

// Run simulations
foreach (var sim in model.Simulations)
{
    sim.Execute(model.Circuit);
}

// Access measurement results
if (model.Measurements.TryGetValue("rise_time", out var results))
{
    foreach (var result in results)
    {
        Console.WriteLine($"Rise time = {result.Value} ({result.SimulationName})");
        Console.WriteLine($"Success = {result.Success}");
    }
}
```

Each `MeasurementResult` contains:
- `Name` — measurement name
- `Value` — numeric result (double.NaN if unsuccessful)
- `Success` — whether the measurement found a valid result
- `MeasurementType` — "TRIG_TARG", "WHEN", "FIND_WHEN", "MIN", "MAX", "AVG", "RMS", "PP", "INTEG", "DERIV", "PARAM"
- `SimulationName` — which simulation produced this result

## Interaction with .STEP

When `.STEP` is used, each sweep point creates a separate simulation. Each simulation produces its own measurement result, so `model.Measurements["name"]` will contain one entry per sweep point:

```
.STEP PARAM V_val LIST 2 5 10
.MEAS TRAN vmax MAX V(out)
```

This produces 3 entries in `model.Measurements["vmax"]`, one per voltage value. The `SimulationName` property identifies which sweep point produced each result.

## Known Limitations

- `mag(V(out))` nested function syntax is not supported — use `VM(out)` instead
- PARAM measurements must be declared after the measurements they reference (no dependency graph resolution)
