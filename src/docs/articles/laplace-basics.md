# Laplace Transform Basics for Circuit Simulation

Laplace transforms can look intimidating because they come from higher math, but you do not need to become a mathematician before using `LAPLACE` sources in SPICE.

For circuit simulation, the practical idea is simple:

```text
output = transfer_function * input
```

The transfer function describes how a linear block changes a signal. It can say "amplify by 10", "roll off after 1 kHz", "remove DC", "lag the phase", or "ring like a damped resonator".

If you want the exact syntax supported by SpiceSharpParser, see [LAPLACE Transfer Sources](laplace.md). This page focuses on the intuition.

Keep one practical boundary in mind as you read: SpiceSharpParser supports a focused `LAPLACE` subset. The transfer must be used on an `E`, `G`, `F`, or `H` source; `E` and `G` use `V(node)` or `V(node1,node2)` input, while `F` and `H` use `I(source)` input. The transfer must be a proper rational polynomial in `s` with finite DC gain. Constant `M=`, `TD=`, and `DELAY=` options are supported for those source-level forms. The examples below stay inside that subset.

## Running Example: Sensor To ADC

Imagine a sensor connected to an ADC input:

```text
sensor voltage -> amplifier and filter -> ADC input
```

The real circuit might include an op-amp, resistors, capacitors, sensor capacitance, board parasitics, and an ADC input network. Early in a design, you may not want every detail. You may only need answers like:

| Question | Why it matters |
|----------|----------------|
| What is the low-frequency gain? | The ADC code must match the sensor value. |
| Where does high-frequency noise get reduced? | Noise above the useful signal band should not dominate the ADC input. |
| How fast does the ADC input settle? | The ADC should sample after the signal has settled enough. |
| Does the front end ring or overshoot? | Overshoot can create false readings or clipping. |
| How much phase shift is added? | Phase matters in control loops and sampled systems. |

A simple first model might be:

```text
H(s) = gain * wc / (s + wc)
```

For example, use `gain = 2` and `fc = 10 kHz`, where `wc = 2*pi*fc`.

That one expression says:

- At DC and low frequency, the ADC input is about twice the sensor voltage.
- Around `10 kHz`, the gain starts rolling off.
- High-frequency noise is reduced.
- In the time domain, sudden changes become smoother and slower.

Written as a small supported netlist, that first model can look like this:

```spice
* Gain of 2 with one low-pass pole at 10 kHz
.PARAM gain=2
.PARAM fc=10k
.PARAM wc={2*PI*fc}
VIN IN 0 AC 1
EAAF OUT 0 LAPLACE {V(IN)} = {gain*wc/(s+wc)}
RLOAD OUT 0 10k
.AC DEC 40 10 1MEG
.SAVE V(OUT)
.END
```

Because `VIN` uses `AC 1`, the `.AC` magnitude of `V(OUT)` reads directly as the gain from the sensor voltage to the ADC input.

This is the main trick: the same transfer function explains the frequency response and the time response of the same linear block.

## What You Can Use This For

Laplace-domain transfer functions are useful when you know the behavior you want, but do not need a detailed component-level model yet.

| Application | What the transfer function captures |
|-------------|-------------------------------------|
| ADC anti-alias or input filtering | Pass the signal band, reduce high-frequency noise. |
| Sensor front-end bandwidth | Model a sensor or conditioning circuit that cannot respond instantly. |
| Amplifier bandwidth | Keep the desired gain at low frequency, then roll off at high frequency. |
| AC coupling | Block DC while passing changing signals. |
| Control-loop blocks | Model plant, compensator, lead, lag, or finite bandwidth terms. |
| LC or mechanical resonance | Model peaking, ringing, and damping. |
| Phase shaping | Add phase lead or lag without building the full circuit yet. |

Use this style for linear, small-signal behavior. If the important behavior is clipping, saturation, slew rate, switching, startup sequencing, or device physics, use a more detailed circuit or behavioral model instead.

## Time Domain And Frequency Domain

The time domain is the waveform you would see on an oscilloscope:

```text
voltage versus time
```

In SPICE, transient analysis (`.TRAN`) is the usual time-domain view. It answers questions like:

| Time-domain question | Example in the sensor-to-ADC story |
|----------------------|-------------------------------------|
| How fast does the output rise? | The sensor voltage steps and the ADC input follows with a rounded edge. |
| How long does it take to settle? | The ADC input must be close enough before sampling. |
| Does it overshoot? | A resonant input network may go above the final value. |
| Does it ring? | A lightly damped system may oscillate before settling. |
| Is the signal smoothed? | A low-pass filter removes fast changes and noise. |

A low-pass transfer function makes sharp changes slower. That is not a bug. It is the time-domain price paid for reducing high-frequency content.

A second-order transfer function can ring. In the frequency domain that same behavior appears as a gain bump near the natural frequency.

The frequency domain is the same circuit viewed by sine waves at different frequencies:

```text
gain and phase versus frequency
```

In SPICE, AC analysis (`.AC`) is the usual frequency-domain view. It answers questions like:

| Frequency-domain question | Example in the sensor-to-ADC story |
|---------------------------|-------------------------------------|
| Which frequencies pass through? | Low-frequency sensor changes pass to the ADC. |
| Which frequencies are reduced? | High-frequency noise is attenuated. |
| What is the bandwidth? | The useful flat region ends near the cutoff frequency. |
| How much phase shift is added? | The ADC input lags the sensor signal near the cutoff. |
| Is there resonance? | A gain peak warns that the time response may overshoot or ring. |

These two views are not separate realities. They are two ways to inspect the same linear system.

## Three SPICE Views

The same `H(s)` is interpreted differently by different analyses.

| SPICE view | What it asks | How to think about `s` |
|------------|--------------|-------------------------|
| `.OP` | What is the DC operating point? | Set `s = 0`. |
| `.AC` | What is gain and phase versus frequency? | Set `s = j*omega`. |
| `.TRAN` | What waveform happens over time? | The simulator uses the equivalent dynamic behavior. |

For the sensor-to-ADC low-pass:

```text
H(s) = gain * wc / (s + wc)
```

In `.OP`, set `s = 0`:

```text
H(0) = gain * wc / wc = gain
```

So the DC sensor value is multiplied by `gain`.

In `.AC`, the simulator evaluates:

```text
H(j*omega)
```

At low frequency, `omega` is small and the gain is close to `gain`. At high frequency, `omega` is large and the gain gets smaller.

In `.TRAN`, a sudden input step does not appear instantly at the output. The output moves toward the final value with a rounded response.

## What `s` Means

A Laplace transfer function is written with a variable named `s`:

```text
H(s)
```

Think of `s` as a special placeholder for dynamic behavior. It is not a node, not a parameter you define with `.PARAM`, and not a voltage.

For operating point analysis:

```text
s = 0
```

For AC analysis at frequency `f`:

```text
s = j * omega
omega = 2 * pi * f
```

`j` means a 90 degree phase rotation. You do not need to calculate complex numbers by hand every time. The useful rule is:

- The magnitude of `H(j*omega)` is the gain at that frequency.
- The angle of `H(j*omega)` is the phase shift at that frequency.

## Units Sanity Check

Units make Laplace formulas feel less arbitrary.

| Symbol | Unit | Meaning |
|--------|------|---------|
| `f` | Hz | Cycles per second. |
| `omega`, `w`, `wc`, `wp`, `wz`, `wn` | rad/s | Angular frequency. |
| `tau` | seconds | Time constant. |
| `s` | 1/seconds | Laplace variable. |
| `gain` | usually unitless | Voltage gain for an `E` source. |
| `gm` | siemens | Transconductance for a `G` source. |

The terms added together in a transfer function must have compatible units.

In this low-pass:

```text
H(s) = 1 / (1 + s*tau)
```

`s` has units of `1/seconds`, and `tau` has units of `seconds`, so `s*tau` is unitless. That is why it can be added to `1`.

In this equivalent low-pass:

```text
H(s) = wc / (s + wc)
```

`s` and `wc` both have units of `1/seconds`, so they can be added. The ratio is unitless.

To move between hertz and angular frequency:

```text
omega = 2 * pi * f
f = omega / (2 * pi)
```

## Gain And Phase

Gain says how much bigger or smaller the output is than the input.

```text
gain = output / input
```

Examples:

| Gain | Meaning |
|------|---------|
| `1` | Same amplitude |
| `10` | Ten times larger |
| `0.5` | Half amplitude |
| `-10` | Ten times larger and inverted |

Phase says how much the output sine wave is shifted relative to the input sine wave.

| Phase | Meaning |
|-------|---------|
| `0 deg` | Output lines up with input |
| `-45 deg` | Output lags input |
| `+45 deg` | Output leads input |
| `180 deg` | Output is inverted |

For many first-order filters, the phase shift is most noticeable around the cutoff frequency.

In the sensor-to-ADC example, phase lag means the ADC input sine wave reaches its peak later than the sensor sine wave. This can matter a lot in feedback systems and sampled control loops.

## Bode Plot Intuition

A Bode plot is just a frequency-domain plot:

- Magnitude tells you gain versus frequency.
- Phase tells you shift versus frequency.

You can understand the common shapes without drawing the plot.

| Shape | What it means |
|-------|---------------|
| Flat magnitude | The block behaves almost like a simple gain. |
| Magnitude rolls down | High frequencies are being reduced. |
| Magnitude rises | Low frequencies are blocked or a zero is adding gain. |
| Phase moves negative | The output lags the input. |
| Phase moves positive | The output leads the input. |
| Magnitude has a bump | The system may overshoot or ring in time. |

For a low-pass filter, the magnitude is flat at low frequency and slopes down after the cutoff. The phase starts changing before the cutoff, is most active near the cutoff, and then approaches its high-frequency limit.

For a resonant system, the magnitude may show a bump. In time-domain language, that bump is the same energy storage behavior that can create overshoot and ringing.

## Time Constants And Cutoff Frequency

A first-order RC low-pass has this transfer function:

```text
H(s) = 1 / (1 + s*tau)
```

`tau` is the time constant:

```text
tau = R * C
```

The cutoff frequency is:

```text
fc = 1 / (2*pi*tau)
```

At the cutoff frequency:

- The gain is about `0.707` of the low-frequency gain.
- The magnitude is down by about `3 dB`.
- The phase is about `-45 deg`.

The time-domain meaning of `tau` is also useful: after one time constant, a first-order step response has moved about 63 percent of the way toward its final value.

Smaller `tau` means faster response and higher cutoff frequency. Larger `tau` means slower response and lower cutoff frequency.

## Poles And Zeros

Poles and zeros are places where a transfer function changes behavior.

A pole usually makes gain start falling and phase start lagging:

```text
H(s) = wc / (s + wc)
```

That is a one-pole low-pass filter. Below `fc`, it passes signals. Above `fc`, it rolls off.

A zero usually makes gain start rising and phase start leading:

```text
H(s) = s / (s + wc)
```

That is a one-pole high-pass filter. It blocks DC and low frequencies, then passes high frequencies.

In real circuits:

| Circuit behavior | Typical cause |
|------------------|---------------|
| Low-pass pole | Capacitance, bandwidth limit, averaging |
| High-pass zero at origin | AC coupling, DC blocking |
| Extra pole | Op-amp bandwidth, sensor bandwidth, compensation capacitor |
| Second-order poles | RLC network, mechanical resonance, control-loop plant |

## Reading `H(s)` Without Doing Full Math

Use this quick checklist:

1. Set `s = 0` to find the DC gain.
2. Look for terms like `s + wc` or `1 + s*tau`; these are poles.
3. Look for `s` in the numerator; that often means DC is blocked or phase lead is added.
4. Look for a frequency such as `wc`, `wp`, `wz`, or `wn`; that is where behavior changes.
5. Compare the numerator degree with the denominator degree; in SpiceSharpParser, the numerator degree must not be greater.
6. Convert angular frequency to hertz with `fc = wc/(2*pi)`.

Examples:

| Transfer | What it does |
|----------|--------------|
| `1/(1+s*tau)` | Unity-gain low-pass |
| `wc/(s+wc)` | Same low-pass, written with angular cutoff |
| `s/(s+wc)` | High-pass that blocks DC |
| `10*wc/(s+wc)` | Gain of 10 with one-pole bandwidth |
| `(1+s/wz)/(1+s/wp)` | Lead or lag block, depending on pole/zero placement |

## Choose Your Transfer Function

Start with the behavior you need, then choose a simple transfer function.

| Goal | Try this transfer | Time-domain intuition | Frequency-domain intuition |
|------|-------------------|-----------------------|----------------------------|
| Smooth noise or limit bandwidth | `wc/(s+wc)` | Edges become rounded. | Flat, then rolls off. |
| Add gain and bandwidth limit | `gain*wp/(s+wp)` | Output follows with finite speed. | Gain is `gain` at low frequency, then rolls off. |
| Block DC | `s/(s+wc)` | A step causes a temporary response that returns toward zero. | Low frequencies are reduced, high frequencies pass. |
| Model peaking or ringing | `wn*wn/(s*s + 2*zeta*wn*s + wn*wn)` | May overshoot or ring before settling. | May show a bump near `fn`. |
| Shape phase in a loop | `(1+s/wz)/(1+s/wp)` | Can improve or slow loop response. | Adds lead or lag between zero and pole. |

For the running sensor-to-ADC example, the first or second row is usually the starting point.

## What Changes As Frequency Increases?

This table is a quick way to read common blocks.

| Block | Low frequency | Near cutoff or natural frequency | High frequency |
|-------|---------------|----------------------------------|----------------|
| Low-pass | Passes signal with normal gain. | Gain drops and phase lags. | Signal is reduced. |
| High-pass | DC and slow changes are reduced. | Gain rises and phase changes. | Signal passes. |
| Finite-bandwidth amplifier | Acts like a normal gain block. | Gain starts falling. | Amplifier cannot keep up. |
| Damped resonance | Often near normal gain. | May peak and shift phase quickly. | Usually rolls off. |
| Lead block | Starts at lower gain. | Adds positive phase over a band. | Ends at higher gain. |
| Lag block | Starts at higher gain. | Adds negative phase over a band. | Ends at lower gain. |

When you see a strong frequency-domain change, expect a related time-domain effect. Rolloff means smoothing and slower edges. Peaking means possible overshoot. Heavy phase shift means delay-like behavior for sine waves.

## Common Physical Blocks

### RC Low-Pass

A resistor feeding a capacitor to ground is a low-pass filter:

```text
H(s) = 1 / (1 + s*R*C)
```

Use it for simple bandwidth limits, anti-alias filters, smoothing, and sensor front ends.

In the time domain, it slows edges and smooths noise. In the frequency domain, it passes low frequencies and reduces high frequencies.

### AC Coupling High-Pass

A series capacitor with a resistor to ground blocks DC:

```text
H(s) = s / (s + wc)
```

Use it when you want changes or AC content, but not the DC level.

In the time domain, a step appears as a temporary pulse-like response. In the frequency domain, low frequencies are reduced and high frequencies pass.

### Finite Op-Amp Bandwidth

An ideal gain block has the same gain forever, but a real amplifier loses gain at high frequency. A simple closed-loop approximation is:

```text
H(s) = gain * wp / (s + wp)
```

where:

```text
fp = gain_bandwidth / gain
wp = 2*pi*fp
```

This is not a full op-amp macro-model. It is a compact way to include a dominant bandwidth limit.

### Damped Resonance

Some systems have a natural frequency and damping:

```text
H(s) = wn*wn / (s*s + 2*zeta*wn*s + wn*wn)
```

where:

| Term | Meaning |
|------|---------|
| `wn` | Natural angular frequency |
| `zeta` | Damping ratio |

Smaller `zeta` gives more peaking and ringing. Larger `zeta` gives a flatter, more damped response.

## Common Misconceptions

| Misconception | Better way to think about it |
|---------------|------------------------------|
| `s` is a normal variable I can set with `.PARAM`. | `s` is reserved for Laplace behavior inside the transfer expression. |
| `AC 1` on a source means the input is always 1 V in every simulation. | `AC 1` sets that source's small-signal AC magnitude; it is mainly a convenient way to read gain directly during `.AC` analysis. |
| A Laplace source is a perfect replacement for a real op-amp. | It is a linear approximation of selected behavior, such as gain and bandwidth. |
| A good `.AC` plot guarantees every transient behavior is good. | Nonlinear effects, clipping, slew rate, and startup behavior may still matter. |
| Higher cutoff is always better. | Higher cutoff is faster, but it also lets more high-frequency noise through. |
| More poles and zeros always make a better model. | Use the simplest model that answers the design question. |

## Good Uses Of LAPLACE Sources

`LAPLACE` sources are best for linear, small-signal behavior:

- Filters with known poles and zeros.
- Sensor or amplifier bandwidth limits.
- Approximate transimpedance or transconductance stages.
- Control-system blocks.
- Comparing an ideal transfer function with a detailed circuit.

They are not a good fit for behavior that is strongly nonlinear:

- Clipping or saturation.
- Slew-rate limiting.
- Startup sequencing.
- Switching ripple.
- Digital logic.
- Temperature-dependent device physics.

For those, use detailed circuit models, behavioral expressions, or device models that directly represent the nonlinear behavior.

## Small Exercises

These are quick checks for reading transfer functions.

| Question | Answer |
|----------|--------|
| What is the DC gain of `10*wc/(s+wc)`? | Set `s = 0`, so the gain is `10`. |
| Does `s/(s+wc)` pass DC? | No. At `s = 0`, the numerator is `0`. |
| If `fc` increases in `wc/(s+wc)`, does the block get faster or slower? | Faster. Higher cutoff means shorter time constant. |
| What does `AC 1` on an input source help with in `.AC` analysis? | It sets the input magnitude to 1, so the output magnitude is directly the transfer gain from that source. |
| What does lower `zeta` usually mean in a second-order block? | More peaking and more ringing. |
| Why avoid a pure integrator such as `1/s` here? | Its DC gain is singular, so it is outside the supported finite-DC-gain subset. |

## Intuition Checklist

When you see a transfer function, ask:

1. What is the DC gain when `s = 0`?
2. Does it pass or block low frequencies?
3. Where does the gain start changing?
4. Does the output lead or lag the input?
5. Would a step be smooth, fast, slow, or ringing?
6. Is this linear approximation enough, or do nonlinear details matter?

For the sensor-to-ADC example, this checklist says: the DC gain is `gain`, low-frequency sensor signals pass, high-frequency noise is reduced above `fc`, phase lag appears near the cutoff, and time-domain edges become smoother.

## SpiceSharpParser Subset

SpiceSharpParser intentionally supports the practical subset introduced near the start:

- `E`, `G`, `F`, and `H` LAPLACE sources.
- Voltage input expressions for `E` / `G`: `V(node)` and `V(node1,node2)`.
- Current input expressions for `F` / `H`: `I(source)`.
- Proper rational polynomials in `s`.
- Finite DC gain.
- Constant `M=`, `TD=`, and `DELAY=` options on supported source-level forms.
- Function-style `LAPLACE(input, transfer, ...)` in behavioral expressions, including inline `M=`, `TD=`, and `DELAY=` options.
- Arbitrary function-style input expressions, lowered through internal helpers when they are not direct probes.

That means many web examples need adaptation before they are valid here. Avoid unsupported forms such as `exp()`, `sqrt()`, pure `1/s`, nested `LAPLACE(...)` calls inside function inputs, ideal delay forms outside the supported `TD=` / `DELAY=` options, and explicit internal-state options.
