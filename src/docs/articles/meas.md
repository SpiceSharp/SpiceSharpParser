# .MEAS / .MEASURE Statement

The `.MEAS` (or `.MEASURE`) statement extracts a single scalar number from simulation waveform data — for example, "how long does the output take to rise from 1 V to 9 V?", "what is the average voltage?", or "what is the RMS current?". Both `.MEAS` and `.MEASURE` are interchangeable.

## General Syntax

```
.MEAS <analysis_type> <name> <measurement_spec>
.MEASURE <analysis_type> <name> <measurement_spec>
```

| Field | Meaning | Values |
|-------|---------|--------|
| `analysis_type` | Which simulation type to measure | `TRAN`, `AC`, `DC`, `OP`, `NOISE` |
| `name` | A label you choose — this is the key used to look up the result in C# | Any identifier (e.g. `rise_time`, `vmax`) |
| `measurement_spec` | What to measure and how (described in detail below) | One of the 12 measurement types |

## Quick Reference — All Measurement Types

| Type | What It Returns | Read More |
|------|----------------|-----------|
| `TRIG … TARG` | Time (or sweep) difference between two threshold crossings | [TRIG/TARG](#trigtarg--timing-measurements) |
| `WHEN` | Time (or sweep value) at which a signal crosses a threshold | [WHEN](#when--threshold-crossing-time) |
| `FIND … WHEN` | Value of signal A at the moment signal B crosses a threshold | [FIND/WHEN](#findwhen--value-at-a-threshold-crossing) |
| `FIND … AT` | Value of a signal at a specific point on the abscissa | [FIND/AT](#findat--value-at-a-specific-point) |
| `MAX` | Maximum value of a signal | [MAX](#max--maximum-value) |
| `MIN` | Minimum value of a signal | [MIN](#min--minimum-value) |
| `AVG` | Time-weighted average (trapezoidal mean) | [AVG](#avg--average-value) |
| `RMS` | Root mean square | [RMS](#rms--root-mean-square) |
| `PP` | Peak-to-peak (max − min) | [PP](#pp--peak-to-peak) |
| `INTEG` | Trapezoidal integral | [INTEG](#integ--integration) |
| `DERIV` | Derivative at a single point | [DERIV](#deriv--derivative-at-a-point) |
| `PARAM` | Evaluate an arithmetic expression using other measurement results | [PARAM](#param--computed-expression) |

## Common Qualifiers

These qualifiers can be combined with most measurement types:

| Qualifier | Meaning | Example |
|-----------|---------|---------|
| `VAL=<v>` | Threshold voltage/current value | `VAL=5.0` |
| `RISE=<n>` | Match the *n*th **rising** crossing | `RISE=1` (first rising edge) |
| `RISE=LAST` | Match the **last** rising crossing | `RISE=LAST` |
| `FALL=<n>` | Match the *n*th **falling** crossing | `FALL=2` (second falling edge) |
| `FALL=LAST` | Match the **last** falling crossing | `FALL=LAST` |
| `CROSS=<n>` | Match the *n*th crossing in **either** direction | `CROSS=3` |
| `CROSS=LAST` | Match the **last** crossing in either direction | `CROSS=LAST` |
| `TD=<time>` | Ignore data before this time (start searching later); works with TRIG/TARG and WHEN/FIND | `TD=5u` |
| `FROM=<x>` | Start of the measurement window | `FROM=2u` |
| `TO=<x>` | End of the measurement window | `TO=8u` |
| `AT=<x>` | Evaluate at a specific point (used in DERIV and FIND) | `AT=5u` |

---

## Measurement Types In Detail

### TRIG/TARG — Timing Measurements

**Purpose:** Measure the elapsed time (or sweep distance) between two events — a *trigger* threshold crossing and a *target* threshold crossing. This is ideal for rise time, fall time, and propagation delay.

**How it works:**

1. The simulator scans the waveform for the point where the TRIG signal crosses its `VAL` (the "start" event).
2. Then it continues scanning for the point where the TARG signal crosses its `VAL` (the "end" event).
3. The result is `time_of_target − time_of_trigger`.

**Syntax:**
```
.MEAS <analysis> <name> TRIG <signal> VAL=<v> [RISE|FALL|CROSS=<n>|LAST] [TD=<t>]
+                       TARG <signal> VAL=<v> [RISE|FALL|CROSS=<n>|LAST] [TD=<t>]
```

#### Example 1 — Rise time of an RC circuit

An RC circuit charges from 0 V toward 10 V. We measure how long it takes to go from 1 V to 9 V:

```spice
* RC circuit rise time
V1 IN 0 10.0
R1 IN OUT 10k
C1 OUT 0 1u
.IC V(OUT)=0.0
.TRAN 1e-5 50e-3

* Measure: time from V(OUT)=1V (first rise) to V(OUT)=9V (first rise)
.MEAS TRAN rise_time TRIG V(OUT) VAL=1.0 RISE=1 TARG V(OUT) VAL=9.0 RISE=1
.END
```

**Expected result:** ~21.97 ms (RC time constant = 10 kΩ × 1 µF = 10 ms; analytically $\tau \ln(9/1) \approx 21.97$ ms).

#### Example 2 — Propagation delay between input and output

A pulse drives an RC filter. We measure the delay between the input reaching 2.5 V and the output reaching 2.5 V:

```spice
* Propagation delay through RC filter
V1 IN 0 PULSE(0 5 0 1n 1n 100u 200u)
R1 IN MID 1k
C1 MID 0 10n
.IC V(MID)=0.0
.TRAN 1e-8 50e-6

* How long after V(IN) hits 2.5V does V(MID) hit 2.5V?
.MEAS TRAN tpd TRIG V(IN) VAL=2.5 RISE=1 TARG V(MID) VAL=2.5 RISE=1
.END
```

#### Example 3 — Fall time with FALL qualifier

Swap `RISE` for `FALL` to measure falling-edge timing:

```spice
* Fall time measurement
V1 IN 0 PULSE(10 0 10e-3 1n 1n 50e-3 100e-3)
R1 IN OUT 10k
C1 OUT 0 1u
.IC V(OUT)=10.0
.TRAN 1e-5 70e-3

* Time from V(OUT)=9V falling to V(OUT)=1V falling
.MEAS TRAN fall_time TRIG V(OUT) VAL=9.0 FALL=1 TARG V(OUT) VAL=1.0 FALL=1
.END
```

#### Example 4 — Using CROSS to match any edge direction

`CROSS=n` counts crossings regardless of direction (rising or falling). Useful for measuring a half-period of a repeating signal:

```spice
* Half-period of a square wave using CROSS
V1 OUT 0 PULSE(0 5 0 1n 1n 10e-6 20e-6)
R1 OUT 0 1k
.TRAN 1e-8 60e-6

* Time between the 2nd and 3rd crossing of 2.5V (= half the period)
.MEAS TRAN t_between TRIG V(OUT) VAL=2.5 CROSS=2 TARG V(OUT) VAL=2.5 CROSS=3
.END
```

**Expected result:** ~10 µs (half of the 20 µs period).

#### Example 5 — Using TD to delay the search start

`TD` (time delay) tells the simulator to skip early data. The TRIG event will only be found after `TD` has elapsed:

```spice
* Skip the first 5ms before searching for the trigger
V1 IN 0 10.0
R1 IN OUT 10k
C1 OUT 0 1u
.IC V(OUT)=0.0
.TRAN 1e-5 50e-3

.MEAS TRAN delayed TRIG V(OUT) VAL=1.0 RISE=1 TD=5e-3 TARG V(OUT) VAL=9.0 RISE=1
.END
```

---

### WHEN — Threshold Crossing Time

**Purpose:** Find the time (or frequency, or sweep value) at which a signal crosses a given threshold. Returns a single x-axis value.

**How it works:** The simulator scans the waveform looking for the point where the signal equals the threshold. Linear interpolation is used between data points for precision.

**Syntax — two forms:**
```
* Combined syntax: signal=value in one expression
.MEAS <analysis> <name> WHEN <signal>=<value> [RISE|FALL|CROSS=<n>|LAST] [TD=<t>]

* Separate syntax: signal and VAL as separate tokens
.MEAS <analysis> <name> WHEN <signal> VAL=<value> [RISE|FALL|CROSS=<n>|LAST] [TD=<t>]
```

Both forms produce the same result.

#### Example 1 — When does V(OUT) reach 5 V?

```spice
* Simple RC charge — find time to reach 50%
V1 IN 0 10.0
R1 IN OUT 10k
C1 OUT 0 1u
.IC V(OUT)=0.0
.TRAN 1e-5 50e-3

.MEAS TRAN t50 WHEN V(OUT)=5.0
.END
```

**Expected result:** ~6.93 ms ($\tau \ln 2 = 10\text{ms} \times 0.693$).

#### Example 2 — Second rising crossing of a square wave

For periodic signals, `RISE=2` skips the first rising crossing and returns the second:

```spice
* 20µs square wave — find the start of the 2nd rising edge
V1 OUT 0 PULSE(0 5 0 1n 1n 10e-6 20e-6)
R1 OUT 0 1k
.TRAN 1e-8 60e-6

.MEAS TRAN t_rise2 WHEN V(OUT)=2.5 RISE=2
.END
```

**Expected result:** ~20 µs (second period begins).

#### Example 3 — First falling crossing

```spice
* Find the time of the first falling edge
V1 OUT 0 PULSE(0 5 0 1n 1n 10e-6 20e-6)
R1 OUT 0 1k
.TRAN 1e-8 60e-6

.MEAS TRAN t_fall1 WHEN V(OUT)=2.5 FALL=1
.END
```

**Expected result:** ~10 µs.

#### Example 4 — Using TD to skip early crossings

`TD` (time delay) works with WHEN just as it does with TRIG/TARG. Only crossings after `TD` are counted:

```spice
* Sine wave crosses 0V many times. With TD=0.8ms, skip crossings before 0.8ms
V1 OUT 0 SIN(0 1 1e3)
R1 OUT 0 1k
.TRAN 1e-6 5e-3

.MEAS TRAN t_first WHEN V(OUT)=0 CROSS=1
.MEAS TRAN t_after_td WHEN V(OUT)=0 CROSS=1 TD=0.8m
.END
```

**Result:** `t_first` is the first crossing (~0.5 ms), while `t_after_td` is the first crossing after 0.8 ms (~1 ms).

#### Example 5 — Finding the LAST crossing

Use `CROSS=LAST`, `RISE=LAST`, or `FALL=LAST` to find the final matching crossing in the simulation data:

```spice
* Find the last time V(OUT) crosses 0V
V1 OUT 0 SIN(0 1 1e3)
R1 OUT 0 1k
.TRAN 1e-6 5e-3

.MEAS TRAN t_last WHEN V(OUT)=0 CROSS=LAST
.END
```

**Result:** `t_last` is near 5 ms (the last zero crossing before end of simulation).

#### Example 6 — What happens when the signal never crosses the threshold?

If the threshold is never reached, the measurement result has `Success = false` and `Value = NaN`:

```spice
* V(OUT) is constant at 5V — it never reaches 10V
V1 OUT 0 5.0
R1 OUT 0 1k
.TRAN 1e-7 10e-6

.MEAS TRAN impossible WHEN V(OUT)=10.0
.END
```

**Result:** `Success = false`, `Value = NaN`.

---

### FIND/WHEN — Value at a Threshold Crossing

**Purpose:** Find the value of **signal A** at the exact moment **signal B** crosses a threshold. This combines `FIND` (what to measure) with `WHEN` (the triggering condition).

**How it works:**

1. Scan for the time when the WHEN signal crosses its threshold (same as a regular WHEN measurement).
2. At that time, read the value of the FIND signal using linear interpolation.
3. Return that value.

**Syntax:**
```
.MEAS <analysis> <name> FIND <signal_A> WHEN <signal_B>=<value> [RISE|FALL|CROSS=<n>|LAST] [TD=<t>]
```

#### Example 1 — Output voltage when input crosses 2.5 V

A pulse drives an RC filter. We want to know V(OUT) at the moment V(IN) crosses 2.5 V:

```spice
* What is V(OUT) when V(IN) first crosses 2.5V?
V1 IN 0 PULSE(0 5 0 1e-6 1e-6 10e-6 20e-6)
R1 IN OUT 1k
C1 OUT 0 10n
.IC V(OUT)=0.0
.TRAN 1e-8 30e-6

.MEAS TRAN vout_at_cross FIND V(OUT) WHEN V(IN)=2.5
.END
```

**Expected result:** A value between 0 V and 5 V (V(OUT) lags V(IN) due to the RC filter).

#### Example 2 — Find current through a resistor when voltage reaches a threshold

```spice
* What is I(R1) when V(OUT) reaches 5V?
V1 IN 0 10.0
R1 IN OUT 10k
C1 OUT 0 1u
.IC V(OUT)=0.0
.TRAN 1e-5 50e-3

.MEAS TRAN i_at_5v FIND I(R1) WHEN V(OUT)=5.0
.END
```

#### Example 3 — Using RISE=2 to pick a specific crossing

```spice
* Find V(OUT) at the 2nd rising crossing of V(IN)
V1 IN 0 PULSE(0 5 0 1n 1n 10e-6 20e-6)
R1 IN OUT 1k
C1 OUT 0 10n
.IC V(OUT)=0.0
.TRAN 1e-8 60e-6

.MEAS TRAN v_at_rise2 FIND V(OUT) WHEN V(IN)=2.5 RISE=2
.END
```

---

### FIND/AT — Value at a Specific Point

**Purpose:** Find the value of a signal at a specific point on the abscissa (e.g., a specific time in a transient analysis, or a specific frequency in an AC analysis). The signal value is computed via linear interpolation between data points.

**Syntax:**
```
.MEAS <analysis> <name> FIND <signal> AT=<value>
```

#### Example 1 — Voltage at a specific time

```spice
* RC charge — what is V(OUT) at t=5ms?
V1 IN 0 10.0
R1 IN OUT 10k
C1 OUT 0 1u
.IC V(OUT)=0.0
.TRAN 1e-5 50e-3

.MEAS TRAN v_at_5ms FIND V(OUT) AT=5m
.END
```

**Expected result:** ~3.935 V ($10 \times (1 - e^{-5/10}) \approx 3.935$).

#### Example 2 — Constant voltage check

```spice
* Constant 5V — FIND AT any time returns 5V
V1 OUT 0 5.0
R1 OUT 0 1k
.TRAN 1e-4 10e-3

.MEAS TRAN v_check FIND V(OUT) AT=5m
.END
```

**Expected result:** 5.0 V.

---

### MAX — Maximum Value

**Purpose:** Find the largest value a signal reaches during the simulation (or within a `FROM`/`TO` window).

```spice
* RC charge toward 10V — MAX should approach 10V
V1 IN 0 10.0
R1 IN OUT 10k
C1 OUT 0 1u
.IC V(OUT)=0.0
.TRAN 1e-5 50e-3

.MEAS TRAN vmax MAX V(OUT)
.END
```

**Expected result:** > 9.9 V (the capacitor nearly fully charges in 50 ms = 5τ).

With a window — only consider data between 5 µs and 15 µs:

```spice
.MEAS TRAN vmax_window MAX V(OUT) FROM=5e-6 TO=15e-6
```

---

### MIN — Minimum Value

**Purpose:** Find the smallest value a signal reaches.

```spice
* RC charge from 0V — MIN is the starting value
V1 IN 0 10.0
R1 IN OUT 10k
C1 OUT 0 1u
.IC V(OUT)=0.0
.TRAN 1e-5 50e-3

.MEAS TRAN vmin MIN V(OUT)
.END
```

**Expected result:** < 0.1 V (the initial voltage near 0 V).

---

### AVG — Average Value

**Purpose:** Compute the time-weighted average of a signal using trapezoidal integration:

$$\text{AVG} = \frac{1}{t_{\text{end}} - t_{\text{start}}} \int_{t_{\text{start}}}^{t_{\text{end}}} V(t) \, dt$$

#### Example 1 — DC voltage across a resistor divider

A voltage divider creates a steady 5 V at node MID. The average should equal 5 V:

```spice
* Resistor divider: V(MID) = 10V * 10k / (10k + 10k) = 5V
V1 IN 0 10.0
R1 IN MID 10k
R2 MID 0 10k
.TRAN 1e-7 10e-6

.MEAS TRAN vavg_dc AVG V(MID)
.END
```

**Expected result:** 5.0 V.

#### Example 2 — Average over a window

```spice
* Constant 5V source — average over 2µs to 8µs is still 5V
V1 OUT 0 5.0
R1 OUT 0 1k
.TRAN 1e-8 10e-6

.MEAS TRAN vavg_win AVG V(OUT) FROM=2e-6 TO=8e-6
.END
```

---

### RMS — Root Mean Square

**Purpose:** Compute the RMS value of a signal using trapezoidal integration:

$$\text{RMS} = \sqrt{\frac{1}{t_{\text{end}} - t_{\text{start}}} \int_{t_{\text{start}}}^{t_{\text{end}}} V(t)^2 \, dt}$$

#### Example — RMS of a sine wave

For a sine wave with amplitude $A$, the RMS value is $A / \sqrt{2}$:

```spice
* 5V amplitude sine at 1MHz — RMS should be ~3.536V
V1 OUT 0 SIN(0 5 1MEG)
R1 OUT 0 1k
.TRAN 1e-9 10e-6

* Skip the first/last partial cycle for a clean measurement
.MEAS TRAN vrms RMS V(OUT) FROM=1e-6 TO=9e-6
.END
```

**Expected result:** ~3.536 V ($5 / \sqrt{2} \approx 3.536$).

---

### PP — Peak-to-Peak

**Purpose:** Compute `MAX − MIN` in a single measurement. Useful for measuring the full swing of a signal.

```spice
* 5V square wave — PP should be 5V
V1 OUT 0 PULSE(0 5 0 1n 1n 5e-6 10e-6)
R1 OUT 0 1k
.TRAN 1e-8 30e-6

.MEAS TRAN vpp PP V(OUT)
.END
```

**Expected result:** 5.0 V (max = 5 V, min = 0 V).

---

### INTEG — Integration

**Purpose:** Compute the trapezoidal integral of a signal over time:

$$\text{INTEG} = \int_{t_{\text{start}}}^{t_{\text{end}}} V(t) \, dt$$

The result has units of (signal unit × time unit). For example, integrating a voltage in volts over seconds gives volt-seconds.

#### Example 1 — Integral of a constant voltage

```spice
* 5V for 10µs → integral = 5V × 10µs = 50µV·s
V1 OUT 0 5.0
R1 OUT 0 1k
.TRAN 1e-8 10e-6

.MEAS TRAN vt_integral INTEG V(OUT) FROM=0 TO=10e-6
.END
```

**Expected result:** 50 × 10⁻⁶ V·s.

#### Example 2 — Integral over a sub-window

```spice
* Only integrate from 2µs to 8µs → 5V × 6µs = 30µV·s
.MEAS TRAN vt_win INTEG V(OUT) FROM=2e-6 TO=8e-6
```

---

### DERIV — Derivative at a Point

**Purpose:** Compute the instantaneous derivative (slope) of a signal at a specific point, using central-difference interpolation.

**Syntax — two forms:**
```
* At a specific point
.MEAS <analysis> <name> DERIV <signal> AT=<time>

* At the point where a condition is met
.MEAS <analysis> <name> DERIV <signal> WHEN <condition_signal>=<value> [RISE|FALL|CROSS=<n>|LAST] [TD=<t>]
```

#### Example 1 — Slope of a linear ramp

A PWL source ramps from 0 V to 10 V over 10 µs. The slope is constant at $10 / 10 \times 10^{-6} = 10^{6}$ V/s:

```spice
* Linear ramp: 0V at t=0, 10V at t=10µs → slope = 1MV/s
V1 OUT 0 PWL(0 0 10e-6 10)
R1 OUT 0 1k
.TRAN 1e-8 10e-6

.MEAS TRAN slope DERIV V(OUT) AT=5e-6
.END
```

**Expected result:** 1 × 10⁶ V/s.

#### Example 2 — Derivative at a threshold crossing (DERIV with WHEN)

Compute the slope of V(OUT) at the moment it crosses 5 V:

```spice
* RC charge — derivative when V(OUT) reaches 5V
V1 IN 0 10.0
R1 IN OUT 10k
C1 OUT 0 1u
.IC V(OUT)=0.0
.TRAN 1e-5 50e-3

.MEAS TRAN slope_at_5v DERIV V(OUT) WHEN V(OUT)=5
.END
```

**Expected result:** ~500 V/s (at $t = \tau \ln 2$, the slope is $\frac{10}{\tau} \cdot e^{-\ln 2} = \frac{10}{0.01} \cdot 0.5 = 500$).

---

### PARAM — Computed Expression

**Purpose:** Evaluate an arbitrary arithmetic expression that references the results of previously defined measurements. This lets you compute derived quantities like ratios, differences, or more complex formulas.

**Syntax:**
```
.MEAS <analysis> <name> PARAM='<expression>'
```

> **Important:** PARAM measurements must appear **after** the measurements they reference, because results are computed in order.

#### Example 1 — Ratio of max to min

```spice
* Square wave between 2V and 8V
V1 OUT 0 PULSE(2 8 0 1n 1n 5e-6 10e-6)
R1 OUT 0 1k
.TRAN 1e-8 30e-6

* First, measure max and min
.MEAS TRAN vmax MAX V(OUT)
.MEAS TRAN vmin MIN V(OUT)

* Then compute derived quantities using their names
.MEAS TRAN ratio PARAM='vmax/vmin'
.MEAS TRAN span PARAM='vmax-vmin'
.END
```

**Expected results:** `ratio` = 8/2 = 4.0, `span` = 8 − 2 = 6.0.

#### Example 2 — More complex expressions

```spice
.MEAS TRAN vmax MAX V(OUT)
.MEAS TRAN vmin MIN V(OUT)

* You can use standard math operators
.MEAS TRAN midpoint PARAM='(vmax+vmin)/2'
.MEAS TRAN pct_swing PARAM='100*(vmax-vmin)/vmax'
```

---

## FROM/TO Windowing

Any measurement type that scans waveform data (`MAX`, `MIN`, `AVG`, `RMS`, `PP`, `INTEG`, `WHEN`, `FIND/WHEN`) supports `FROM` and `TO` to restrict the x-axis range:

```
.MEAS TRAN vmax_win MAX V(OUT) FROM=5u TO=15u
.MEAS TRAN vavg_win AVG V(OUT) FROM=2u TO=8u
.MEAS TRAN t_in_window WHEN V(OUT)=2.5 RISE=1 FROM=0 TO=30e-6
```

- If only `FROM` is given, measurement runs from `FROM` to the end of the simulation.
- If only `TO` is given, measurement runs from the start to `TO`.
- If neither is given, the entire simulation range is used.

---

## AC Measurements

For AC analysis, standard `V(node)` is a complex number, so you need specific export functions:

| Export | Description | Example Use |
|--------|-------------|-------------|
| `VM(node)` | Voltage magnitude | Detecting the −3 dB point |
| `VDB(node)` | Voltage in decibels (20 log₁₀) | Gain in dB |
| `VP(node)` | Voltage phase (degrees) | Phase margin |
| `VR(node)` | Real part of complex voltage | |
| `VI(node)` | Imaginary part of complex voltage | |

#### Example — Measuring bandwidth from a frequency sweep

```spice
.MEAS AC bw_point WHEN VM(out)=0.707
.MEAS AC max_gain MAX VM(out)
.MEAS AC bw_db WHEN VDB(out)=-3
```

Both the prefix syntax and nested function syntax are supported:

| Prefix Syntax | Nested Syntax | Description |
|---------------|---------------|-------------|
| `VM(out)` | `mag(V(out))` | Voltage magnitude |
| `VDB(out)` | `db(V(out))` | Voltage in decibels |
| `VP(out)` | `phase(V(out))` or `ph(V(out))` | Voltage phase |
| `VR(out)` | `real(V(out))` or `re(V(out))` | Voltage real part |
| `VI(out)` | `imag(V(out))` or `im(V(out))` | Voltage imaginary part |
| `IM(R1)` | `mag(I(R1))` | Current magnitude |
| `IDB(R1)` | `db(I(R1))` | Current in decibels |
| `IP(R1)` | `phase(I(R1))` | Current phase |

The nested syntax also works in `.PRINT`, `.PLOT`, `.SAVE`, and `.WAVE` statements.

---

## Accessing Results in C#

Measurement results are automatically collected during simulation and stored in `SpiceSharpModel.Measurements`, a `ConcurrentDictionary<string, List<MeasurementResult>>`:

```csharp
var model = reader.Read(netlist);

// Run all simulations — measurements are computed automatically
foreach (var sim in model.Simulations)
{
    sim.Execute(model.Circuit);
}

// Look up a measurement by its name
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

| Property | Type | Description |
|----------|------|-------------|
| `Name` | `string` | The measurement name from your `.MEAS` statement |
| `Value` | `double` | The numeric result (`double.NaN` if the measurement failed) |
| `Success` | `bool` | `true` if the measurement found a valid result |
| `MeasurementType` | `string` | One of: `TRIG_TARG`, `WHEN`, `FIND_WHEN`, `FIND_AT`, `MIN`, `MAX`, `AVG`, `RMS`, `PP`, `INTEG`, `DERIV`, `PARAM` |
| `SimulationName` | `string` | Identifies which simulation produced this result |

---

## Interaction with .STEP

When `.STEP` is used, each sweep point runs as a separate simulation. Each one produces its own measurement, so the result list contains one entry per sweep point:

```spice
* Three sweep points → three measurement results
.STEP PARAM V_val LIST 2 5 10
.MEAS TRAN vmax MAX V(out)
```

This produces 3 entries in `model.Measurements["vmax"]`. Use the `SimulationName` property to identify which sweep point produced each result.

---

## Complete Example — Multiple Measurements in One Netlist

This example demonstrates several measurement types together on a single RC circuit:

```spice
* RC circuit with pulse input — comprehensive measurements
V1 IN 0 PULSE(0 10 0 1n 1n 25e-3 50e-3)
R1 IN OUT 10k
C1 OUT 0 1u
.IC V(OUT)=0.0
.TRAN 1e-5 50e-3

* Timing: how long from 1V to 9V?
.MEAS TRAN rise_time TRIG V(OUT) VAL=1.0 RISE=1 TARG V(OUT) VAL=9.0 RISE=1

* When does V(OUT) first reach 5V?
.MEAS TRAN t_half WHEN V(OUT)=5.0

* Statistics
.MEAS TRAN vmax MAX V(OUT)
.MEAS TRAN vmin MIN V(OUT)
.MEAS TRAN vavg AVG V(OUT)
.MEAS TRAN vpp PP V(OUT)

* Derived
.MEAS TRAN ratio PARAM='vmax/vmin'
.END
```

---

## Known Limitations

- PARAM measurements must be declared **after** the measurements they reference (results are computed in declaration order, not via dependency resolution)
- When a threshold crossing is not found in the data, the measurement returns `Success = false` and `Value = NaN` rather than raising an error
