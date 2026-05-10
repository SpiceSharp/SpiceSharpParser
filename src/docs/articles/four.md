# .FOUR Statement

The `.FOUR` statement performs Fourier analysis on transient waveform data. It is intended for measuring harmonic content, phase, normalized harmonic levels, and total harmonic distortion (THD) after a `.TRAN` simulation.

> Status: `.FOUR` is documented as the planned SpiceSharpParser behavior. The current reader must register a `.FOUR` control and expose `FourierAnalyses` before these examples run as written.

## Overview

`.FOUR` is a transient post-processing command. It does not run a new simulation by itself. Instead, it reads samples produced by `.TRAN`, takes the final settled period of each requested waveform, and decomposes that period into a fundamental component plus harmonics.

Use `.FOUR` when you want answers such as:

- How large is the 1 kHz component of `V(OUT)`?
- How much second or third harmonic distortion exists in an amplifier output?
- Did a low-pass filter reduce high-frequency harmonic content?
- Is the output mostly sinusoidal, or is there measurable THD?
- Are `V(IN)` and `V(OUT)` shifted in phase at the fundamental?

`.FOUR` is different from `.AC`. `.AC` linearizes the circuit around an operating point and computes small-signal frequency response. `.FOUR` looks at the actual transient waveform, so it can measure distortion produced by nonlinear devices, clipping, switching, or startup behavior.

## Quick Start

For a 1 kHz sine wave, the fundamental frequency is `1k`:

```spice
Pure sine Fourier analysis
V1 IN 0 SIN(0 1 1k)
R1 IN 0 1k
.TRAN 1u 10m 0 2u
.FOUR 1k V(IN)
.END
```

The transient simulation runs for `10 ms`, which is ten periods of a 1 kHz waveform. The maximum transient step is limited to `2 us`, giving enough samples for the first several harmonics.

Expected highlights:

| Result | Approximate value | Why |
|--------|-------------------|-----|
| harmonic 1 frequency | `1 kHz` | The `.FOUR` frequency is `1k`. |
| harmonic 1 magnitude | `1` | The sine source amplitude is 1 V. |
| harmonics 2 through 9 | near `0` | An ideal sine has no distortion harmonics. |
| THD | near `0%` | No higher harmonics are present. |

## Syntax

```spice
.FOUR <fundamental_frequency> <expr1> [<expr2> ...]
```

| Parameter | Description |
|-----------|-------------|
| `fundamental_frequency` | Base repeating frequency in hertz. This must be positive and finite. |
| `expr` | Signal expression to analyze, such as `V(OUT)`, `V(IN)`, or `I(V1)`. |

Examples:

```spice
.FOUR 1k V(OUT)
.FOUR 60 V(LINE) I(VLOAD)
.FOUR {freq} V(IN) V(OUT)
```

`.FOUR` requires a `.TRAN` analysis. It consumes transient samples collected during that run.

## Fundamental Frequency

The `fundamental_frequency` is the base frequency of the repeating waveform. If the waveform repeats every period `T`, then:

```text
f0 = 1 / T
T  = 1 / f0
```

For `.FOUR`, `f0` decides where harmonic 1 is. Harmonic frequencies are integer multiples of `f0`:

```text
harmonic 1 = 1 * f0
harmonic 2 = 2 * f0
harmonic 3 = 3 * f0
...
harmonic 9 = 9 * f0
```

For example:

```spice
.FOUR 1k V(OUT)
```

means:

| Harmonic | Frequency |
|----------|-----------|
| 1 | `1 kHz` |
| 2 | `2 kHz` |
| 3 | `3 kHz` |
| 4 | `4 kHz` |
| 9 | `9 kHz` |

Common choices:

| Waveform | Period | Correct `fundamental_frequency` | Notes |
|----------|--------|----------------------------------|-------|
| 1 kHz sine wave | `1 ms` | `1k` | Harmonic 1 is the sine itself. |
| 10 kHz square wave | `100 us` | `10k` | Odd harmonics appear at `30k`, `50k`, etc. |
| 20 kHz PWM-ish pulse train | `50 us` | `20k` | Use the pulse repetition frequency, not the edge rate. |
| 60 Hz mains waveform | `16.6667 ms` | `60` | Harmonics are `120 Hz`, `180 Hz`, etc. |

Choosing the wrong `f0` is one of the easiest ways to get misleading results. If the circuit produces a 1 kHz sine but the netlist asks for:

```spice
.FOUR 900 V(OUT)
```

then `.FOUR` looks at `900 Hz`, `1.8 kHz`, `2.7 kHz`, and so on. The real 1 kHz signal no longer lands cleanly on harmonic 1, the final analyzed period does not match the waveform period, and energy can appear spread across multiple harmonics. This is called spectral leakage.

## Harmonics And THD

A harmonic is a sinusoidal component at an integer multiple of the fundamental frequency.

| Term | Meaning |
|------|---------|
| DC | Average value of the waveform over the analyzed period. |
| fundamental | Harmonic 1, at `f0`. |
| second harmonic | Harmonic 2, at `2*f0`. |
| third harmonic | Harmonic 3, at `3*f0`. |
| THD | RMS sum of harmonics 2 through 9 divided by harmonic 1. |

An ideal sine wave contains only one harmonic:

```text
x(t) = sin(2*pi*f0*t)
```

Expected `.FOUR` shape:

| Harmonic | Magnitude |
|----------|-----------|
| 1 | large |
| 2 through 9 | near zero |

A distorted sine might contain extra harmonic content:

```text
x(t) = sin(2*pi*f0*t) + 0.1*sin(2*pi*2*f0*t)
```

Expected `.FOUR` shape:

| Harmonic | Magnitude | Normalized magnitude |
|----------|-----------|----------------------|
| 1 | `1` | `1` |
| 2 | `0.1` | `0.1` |
| 3 through 9 | near `0` | near `0` |

THD ignores DC and ignores harmonic 1. It measures how much higher-harmonic content exists relative to the fundamental. In the example above, THD is about `10%`.

## How The Analyzer Works

The intended implementation is post-processing over SpiceSharp transient exports:

1. Run the `.TRAN` simulation.
2. Collect requested `.FOUR` signal samples from `EventExportData`.
3. Use the requested `fundamental_frequency` to compute one period, `T = 1 / f0`.
4. Select the last complete period available before the final transient time.
5. Resample that period onto uniformly spaced time points.
6. Compute DC and harmonics with sine/cosine correlation.
7. Normalize each harmonic relative to harmonic 1.
8. Compute THD from harmonics 2 through 9 relative to harmonic 1.
9. Store structured results for the C# API.

The final-period rule is important. In most circuits, the first few periods can include startup transients, capacitor charging, inductor current settling, oscillator buildup, or switch initial conditions. Analyzing the final complete period makes `.FOUR` more useful for steady-state distortion measurements.

## Math Background

Fourier analysis says that a repeating waveform can be represented as a DC value plus sine and cosine waves at integer multiples of a base frequency.

For a requested fundamental `f0`, the period is:

```text
T = 1 / f0
```

Over one settled period, the waveform is approximated as:

```text
x(t) = dc + sum(k = 1..H) [a_k*cos(2*pi*k*f0*t) + b_k*sin(2*pi*k*f0*t)]
```

where:

| Symbol | Meaning |
|--------|---------|
| `x(t)` | Signal being analyzed, such as `V(OUT)`. |
| `dc` | Average value of `x(t)` over one period. |
| `k` | Harmonic number. |
| `H` | Highest harmonic computed. Initial `.FOUR` support targets harmonics 0 through 9, where harmonic 0 is DC. |
| `a_k` | Cosine coefficient for harmonic `k`. |
| `b_k` | Sine coefficient for harmonic `k`. |
| `f0` | Fundamental frequency from the `.FOUR` statement. |

### Correlation In Plain Language

Sine/cosine correlation means asking, "How much does this waveform look like a sine or cosine at this exact frequency?"

For each harmonic `k`, the analyzer compares the waveform to:

```text
cos(2*pi*k*f0*t)
sin(2*pi*k*f0*t)
```

If the waveform rises and falls in the same pattern as that reference wave, the sum is large. If the waveform has unrelated shape at that frequency, positive and negative parts cancel and the sum is small.

This is why a pure 1 kHz sine gives a large harmonic 1 result when `.FOUR 1k` is used, but a very small harmonic 2 result. The waveform correlates strongly with 1 kHz and weakly with 2 kHz.

### Discrete Formulas

Transient simulations usually produce non-uniform timesteps. The intended `.FOUR` implementation first resamples the final complete period onto `M` uniformly spaced samples:

```text
x[0], x[1], x[2], ..., x[M-1]
```

For uniform samples, the coefficients are:

```text
dc  = (1 / M) * sum(n = 0..M-1, x[n])
a_k = (2 / M) * sum(n = 0..M-1, x[n] * cos(2*pi*k*n/M))
b_k = (2 / M) * sum(n = 0..M-1, x[n] * sin(2*pi*k*n/M))
```

This is equivalent to evaluating Fourier content only at the requested harmonic frequencies. It does not require an FFT length, and it does not require the sample count to be a power of two.

### Magnitude And Phase

The cosine and sine coefficients combine into one magnitude and one phase:

```text
magnitude_k = sqrt(a_k^2 + b_k^2)
phase_k     = atan2(-b_k, a_k) * 180 / pi
```

The planned phase convention is cosine-referenced:

| Waveform | Expected phase |
|----------|----------------|
| `cos(2*pi*f0*t)` | about `0 deg` |
| `sin(2*pi*f0*t)` | about `-90 deg` |
| `-cos(2*pi*f0*t)` | about `180 deg` or `-180 deg` |
| `-sin(2*pi*f0*t)` | about `90 deg` |

Other simulators may print phase with a different reference, wrapping, or sign convention. Exact LTspice textual phase parity is not a v1 goal.

### Normalized Magnitude And dB

Normalized magnitude compares a harmonic to the fundamental:

```text
normalized_k = magnitude_k / magnitude_1
```

Normalized dB is:

```text
normalized_db_k = 20 * log10(normalized_k)
```

Examples:

| `normalized_k` | `normalized_db_k` | Meaning |
|----------------|-------------------|---------|
| `1` | `0 dB` | Same magnitude as the fundamental. |
| `0.5` | about `-6.02 dB` | Half the fundamental magnitude. |
| `0.1` | `-20 dB` | One tenth of the fundamental magnitude. |
| `0.01` | `-40 dB` | One hundredth of the fundamental magnitude. |

### THD

THD is the RMS sum of distortion harmonics divided by the fundamental:

```text
THD percent = 100 * sqrt(magnitude_2^2 + magnitude_3^2 + ... + magnitude_9^2) / magnitude_1
```

Important details:

- DC does not count toward THD.
- Harmonic 1 does not count toward THD because it is the reference signal.
- Harmonics 2 through 9 are included in the v1 target.
- If `magnitude_1` is zero or too close to zero, THD should be reported as `NaN` because there is no useful fundamental reference.

### Worked Math Examples

#### Pure cosine

```text
x(t) = cos(2*pi*f0*t)
```

| Quantity | Value |
|----------|-------|
| `dc` | `0` |
| `a_1` | `1` |
| `b_1` | `0` |
| `magnitude_1` | `1` |
| `phase_1` | `0 deg` |
| `THD` | `0%` |

All fundamental content appears in the cosine coefficient.

#### Pure sine

```text
x(t) = sin(2*pi*f0*t)
```

| Quantity | Value |
|----------|-------|
| `dc` | `0` |
| `a_1` | `0` |
| `b_1` | `1` |
| `magnitude_1` | `1` |
| `phase_1` | `-90 deg` |
| `THD` | `0%` |

The magnitude is still `1`, but the phase is different because the output phase is measured relative to a cosine reference.

#### DC offset plus sine

```text
x(t) = 2 + sin(2*pi*f0*t)
```

| Quantity | Value |
|----------|-------|
| `dc` | `2` |
| `magnitude_1` | `1` |
| harmonics 2 through 9 | near `0` |
| `THD` | `0%` |

The DC offset is reported separately. It does not mean the signal has harmonic distortion.

#### Fundamental plus second harmonic

```text
x(t) = sin(2*pi*f0*t) + 0.1*sin(2*pi*2*f0*t)
```

| Quantity | Value |
|----------|-------|
| `magnitude_1` | `1` |
| `magnitude_2` | `0.1` |
| `normalized_2` | `0.1` |
| `normalized_db_2` | `-20 dB` |
| `THD` | `10%` |

Because the only distortion harmonic is 10 percent of the fundamental, THD is `10%`.

#### Square-wave odd harmonics

An ideal 50 percent duty-cycle square wave contains only odd harmonics. Relative to the fundamental, the ideal magnitudes are approximately:

| Harmonic | Normalized magnitude | Normalized dB |
|----------|----------------------|---------------|
| 1 | `1` | `0 dB` |
| 2 | `0` | very small |
| 3 | `1/3 = 0.333` | about `-9.54 dB` |
| 4 | `0` | very small |
| 5 | `1/5 = 0.2` | about `-13.98 dB` |
| 7 | `1/7 = 0.143` | about `-16.90 dB` |
| 9 | `1/9 = 0.111` | about `-19.08 dB` |

Real simulated square waves have finite rise and fall times, so high harmonics are usually smaller than the ideal values.

#### Filtered harmonic content

For a first-order RC low-pass filter:

```text
|H(f)| = 1 / sqrt(1 + (f / fc)^2)
fc = 1 / (2*pi*R*C)
```

With `R = 1k` and `C = 159.154943n`, the cutoff frequency is about `1 kHz`.

At cutoff:

```text
|H(fc)| = 1 / sqrt(2) = 0.707
```

At three times cutoff:

```text
|H(3*fc)| = 1 / sqrt(1 + 3^2) = 0.316
```

If the input is:

```text
V(MID) = 1.0*sin(2*pi*1k*t) + 0.2*sin(2*pi*3k*t)
```

then the low-pass output is roughly:

```text
fundamental output magnitude = 1.0 * 0.707 = 0.707
third harmonic output magnitude = 0.2 * 0.316 = 0.063
```

Input THD is about `20%`. Output THD is roughly:

```text
100 * 0.063 / 0.707 = 8.9%
```

This is why low-pass filtering can reduce measured THD when the distortion is mostly high-frequency harmonic content.

## Reading The Results

Each `.FOUR` signal is intended to produce one result object containing a list of harmonic rows.

| Field | Meaning |
|-------|---------|
| `SignalName` | Signal expression being analyzed, such as `V(OUT)`. |
| `FundamentalFrequency` | Frequency passed to `.FOUR`. |
| `TotalHarmonicDistortionPercent` | THD percentage computed from harmonics 2 through 9. |
| `Harmonics` | Harmonic rows, including DC and harmonics 1 through 9. |

Each harmonic row contains:

| Field | Meaning |
|-------|---------|
| `HarmonicNumber` | Harmonic index. Harmonic 0 is DC; harmonic 1 is the fundamental. |
| `Frequency` | Harmonic frequency, equal to `HarmonicNumber * FundamentalFrequency`. |
| `Magnitude` | Harmonic magnitude in signal units, such as volts or amps. |
| `PhaseDegrees` | Harmonic phase in degrees using the planned cosine-reference convention. |
| `NormalizedMagnitude` | Magnitude divided by harmonic 1 magnitude. |
| `NormalizedMagnitudeDecibels` | `20 * log10(NormalizedMagnitude)`. |

Interpretation notes:

- `Magnitude` is the amplitude of that harmonic component, not RMS unless the source waveform and future API explicitly define RMS output.
- DC is useful for detecting offset, but it is not part of THD.
- `NormalizedMagnitude` compares harmonics within the same signal. To compare a filter input and output, compare the absolute `Magnitude` values for `V(IN)` and `V(OUT)`.
- `0 dB` normalized means "equal to the fundamental of this same signal."
- Negative normalized dB means "smaller than the fundamental."
- `NaN` THD usually means the fundamental magnitude is too small to be a meaningful denominator.

## Choosing `.TRAN` Settings

Fourier accuracy depends heavily on the transient data. Good `.FOUR` results start with a good `.TRAN`.

### Simulate Enough Periods

Run long enough for startup behavior to settle before the final time. For a 1 kHz waveform, one period is `1 ms`, so this transient runs for ten periods:

```spice
.TRAN 1u 10m 0 2u
```

If the circuit has slow capacitors, inductors, feedback loops, or oscillators, ten periods may not be enough. Increase the stop time until the final period is representative of steady state.

### Use A Small Enough Maximum Step

The fourth argument of `.TRAN` is commonly used as the maximum timestep:

```spice
.TRAN <tstep> <tstop> [tstart] [tmaxstep]
```

If v1 computes through harmonic 9, a `.FOUR 1k` analysis inspects content up to `9 kHz`. The 9 kHz period is about `111 us`. A `2 us` maximum step gives more than 50 samples per 9th-harmonic period:

```text
111 us / 2 us = 55.5 samples
```

Practical starting points:

| Goal | Suggested `tmaxstep` |
|------|----------------------|
| Rough THD estimate | At least 20 samples per highest harmonic period. |
| Better harmonic magnitudes | 50 to 100 samples per highest harmonic period. |
| Sharp switching edges | Use a smaller step and verify convergence/runtime. |

### Avoid Startup Transients

`.FOUR` analyzes the final complete period, but that period can still contain startup behavior if the simulation ends too early. Signs of this problem include:

- Fundamental magnitude changes when `tstop` is increased.
- THD changes greatly when `tstop` is increased.
- The waveform is visibly drifting during the final cycle.
- A filter output has not reached steady amplitude.

### Compare Input And Output For Filters

For filters, run `.FOUR` on both sides:

```spice
.FOUR 1k V(IN) V(OUT)
```

This lets you compare:

- Fundamental attenuation from input to output.
- Harmonic attenuation from input to output.
- THD before and after filtering.
- Phase shift at the fundamental and harmonics.

### Understand Spectral Leakage

Spectral leakage happens when the analyzed time window does not contain an integer number of waveform periods. Planned `.FOUR` support reduces this by selecting one complete period based on `f0`, but leakage can still appear if:

- The chosen `f0` does not match the real waveform frequency.
- The source frequency has been parameterized incorrectly.
- The waveform is not periodic during the final period.
- The simulation step size is too coarse near sharp edges.

## Circuit Examples

### Pure Sine

```spice
Pure sine Fourier analysis
V1 IN 0 SIN(0 1 1k)
R1 IN 0 1k
.TRAN 1u 10m 0 2u
.FOUR 1k V(IN)
.END
```

Expected highlights:

| Result | Approximate value |
|--------|-------------------|
| `V(IN)` harmonic 1 magnitude | `1` |
| `V(IN)` harmonics 2 through 9 | near `0` |
| `V(IN)` THD | near `0%` |

### Sine Plus Second Harmonic

```spice
Second harmonic distortion
V1 A 0 SIN(0 1 1k)
V2 A OUT SIN(0 0.1 2k)
R1 OUT 0 1k
.TRAN 1u 10m 0 2u
.FOUR 1k V(OUT)
.END
```

This waveform contains a 1 kHz fundamental and a smaller 2 kHz second harmonic.

Expected highlights:

| Result | Approximate value |
|--------|-------------------|
| harmonic 1 magnitude | `1` |
| harmonic 2 magnitude | `0.1` |
| harmonic 2 normalized magnitude | `0.1` |
| harmonic 2 normalized dB | `-20 dB` |
| THD | `10%` |

### Square Wave Odd Harmonics

```spice
Square wave Fourier analysis
V1 OUT 0 PULSE(-1 1 0 100n 100n 500u 1m)
R1 OUT 0 1k
.TRAN 1u 10m 0 2u
.FOUR 1k V(OUT)
.END
```

The pulse repeats every `1 ms`, so the fundamental is `1 kHz`. A near-ideal 50 percent duty-cycle square wave mostly contains odd harmonics.

Expected highlights:

| Result | Approximate value |
|--------|-------------------|
| harmonic 1 | dominant |
| harmonic 2 | near `0` |
| harmonic 3 normalized magnitude | about `0.333` |
| harmonic 5 normalized magnitude | about `0.2` |
| harmonic 7 normalized magnitude | about `0.143` |
| harmonic 9 normalized magnitude | about `0.111` |

Finite rise and fall times reduce high harmonic magnitudes, so simulated values may be lower than the ideal ratios.

### RC Low-Pass At Cutoff

```spice
RC low-pass Fourier analysis
V1 IN 0 SIN(0 1 1k)
R1 IN OUT 1k
C1 OUT 0 159.154943n
.TRAN 1u 10m 0 2u
.FOUR 1k V(IN) V(OUT)
.END
```

The capacitor value places the cutoff frequency near `1 kHz`:

```text
fc = 1 / (2*pi*1k*159.154943n) = 1 kHz
```

At cutoff, a first-order low-pass output is about `0.707` of the input and about `-45 deg` shifted.

Expected highlights:

| Result | Approximate value |
|--------|-------------------|
| `V(IN)` harmonic 1 magnitude | `1` |
| `V(OUT)` harmonic 1 magnitude | `0.707` |
| output/input fundamental ratio | `0.707` |
| `V(OUT)` phase shift | about `-45 deg` |

### RC High-Pass At Cutoff

```spice
RC high-pass Fourier analysis
V1 IN 0 SIN(0 1 1k)
C1 IN OUT 159.154943n
R1 OUT 0 1k
.TRAN 1u 10m 0 2u
.FOUR 1k V(IN) V(OUT)
.END
```

This is the high-pass counterpart to the low-pass example. With the same `R` and `C`, the cutoff is again about `1 kHz`.

For a first-order RC high-pass filter:

```text
|H(f)| = (f / fc) / sqrt(1 + (f / fc)^2)
```

At cutoff:

```text
|H(fc)| = 1 / sqrt(2) = 0.707
```

Expected highlights:

| Result | Approximate value |
|--------|-------------------|
| `V(IN)` harmonic 1 magnitude | `1` |
| `V(OUT)` harmonic 1 magnitude | `0.707` |
| output/input fundamental ratio | `0.707` |
| `V(OUT)` phase shift | about `+45 deg` |

### Low-Pass Harmonic Cleanup

```spice
Low-pass harmonic cleanup
V1 IN 0 SIN(0 1 1k)
V3 IN MID SIN(0 0.2 3k)
R1 MID OUT 1k
C1 OUT 0 159.154943n
.TRAN 1u 10m 0 2u
.FOUR 1k V(MID) V(OUT)
.END
```

`V(MID)` contains a 1 kHz fundamental and a 3 kHz harmonic. The low-pass filter attenuates the 3 kHz harmonic more strongly than the 1 kHz fundamental, so `V(OUT)` should have lower THD than `V(MID)`.

Expected highlights:

| Result | Approximate value |
|--------|-------------------|
| `V(MID)` harmonic 1 magnitude | `1` |
| `V(MID)` harmonic 3 magnitude | `0.2` |
| `V(MID)` THD | about `20%` |
| `V(OUT)` harmonic 1 magnitude | about `0.707` |
| `V(OUT)` harmonic 3 magnitude | about `0.063` |
| `V(OUT)` THD | about `9%` |

### Wrong Fundamental Frequency

```spice
Wrong fundamental frequency example
V1 IN 0 SIN(0 1 1k)
R1 IN 0 1k
.TRAN 1u 10m 0 2u
.FOUR 900 V(IN)
.END
```

The source is 1 kHz, but `.FOUR` asks for 900 Hz. The analyzer therefore checks `900 Hz`, `1.8 kHz`, `2.7 kHz`, and so on. The real 1 kHz sine does not line up with harmonic 1.

Expected behavior:

| Result | Why it is misleading |
|--------|----------------------|
| harmonic 1 magnitude is not close to `1` | Harmonic 1 is checking `900 Hz`, not `1 kHz`. |
| higher harmonics may not be near zero | The analyzed period does not match the sine period. |
| THD may look nonzero | Leakage can be mistaken for distortion. |

The fix is:

```spice
.FOUR 1k V(IN)
```

## C# API

After running the simulations, Fourier results are intended to be available from `model.FourierAnalyses`:

```csharp
RunSimulations(model);

foreach (var result in model.FourierAnalyses)
{
    Console.WriteLine($"{result.SignalName}: THD = {result.TotalHarmonicDistortionPercent}%");

    foreach (var harmonic in result.Harmonics)
    {
        Console.WriteLine($"{harmonic.HarmonicNumber}: {harmonic.Magnitude}");
    }
}
```

Each `.FOUR` signal should produce a separate `FourierAnalysisResult`, including enough context to identify the simulation, the signal name, the requested fundamental frequency, harmonic rows, and THD.

## Troubleshooting

| Symptom | Likely cause | What to check |
|---------|--------------|---------------|
| `.FOUR` has no result | No `.TRAN` analysis was run. | Add a transient analysis; `.FOUR` is transient post-processing. |
| Error about insufficient data | The transient did not include one complete final period. | Increase `.TRAN` stop time or choose the correct `f0`. |
| Harmonics appear in a pure sine | Wrong `fundamental_frequency`, coarse timestep, or unsettled waveform. | Match `f0` to the source and reduce `tmaxstep`. |
| THD changes when `tstop` changes | The final waveform is not settled. | Simulate more periods before the final time. |
| High harmonics are too small or unstable | Timestep is too large for the highest harmonic. | Use a smaller `tmaxstep`. |
| THD is `NaN` | Fundamental magnitude is near zero. | Confirm that harmonic 1 is the intended reference frequency. |
| Phase does not match another simulator | Different phase reference or wrapping convention. | Compare relative phase and verify the convention used. |
| Filter output ratio is unexpected | Cutoff frequency, component value, or timestep is wrong. | Recompute `fc = 1 / (2*pi*R*C)` and compare `.FOUR` on input and output. |

## Limitations

- Requires a `.TRAN` analysis.
- Requires at least one complete period of transient data.
- Initial support targets harmonics 0 through 9, where harmonic 0 is DC.
- THD is computed from harmonics 2 through 9 relative to harmonic 1.
- The planned v1 API exposes structured results rather than reproducing LTspice's exact textual report format.
- Accuracy depends on transient step size, waveform settling, interpolation quality, and final simulation time.
- Phase uses the planned cosine-reference convention and may not match another simulator's printed phase exactly.
