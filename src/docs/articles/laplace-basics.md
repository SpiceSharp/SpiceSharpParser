# Laplace Transform Basics for Circuit Simulation

Laplace transforms can look intimidating because they come from higher math, but you do not need to become a mathematician before using `LAPLACE` sources in SPICE.

For circuit simulation, the practical idea is simple:

```text
output = transfer_function * input
```

The transfer function describes how a linear block changes a signal. It can say "amplify by 10", "roll off after 1 kHz", "remove DC", "lag the phase", or "ring like a damped resonator".

If you want the exact syntax supported by SpiceSharpParser, see [LAPLACE Transfer Sources](laplace.md). This page focuses on the intuition.

## Time Domain And Frequency Domain

The time domain is the waveform you would see on an oscilloscope:

```text
voltage versus time
```

It answers questions like:

| Question | Example |
|----------|---------|
| How fast does the output rise? | Step response |
| Does it overshoot? | Damped resonance |
| Does it settle? | Filter or control loop response |

The frequency domain is the same circuit viewed by sine waves at different frequencies:

```text
gain and phase versus frequency
```

It answers questions like:

| Question | Example |
|----------|---------|
| Which frequencies pass through? | Low-pass filter |
| Which frequencies are blocked? | High-pass coupling |
| How much delay or phase shift is added? | Op-amp bandwidth |

SPICE `.AC` analysis is frequency-domain analysis. It tries many frequencies and reports the output magnitude and phase.

## What `s` Means

A Laplace transfer function is written with a variable named `s`:

```text
H(s)
```

Think of `s` as a placeholder that lets one expression describe both DC and AC behavior.

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

## Poles And Zeros

Poles and zeros are the frequencies where a transfer function changes behavior.

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
3. Look for `s` in the numerator; that often means DC is blocked.
4. Compare the numerator degree with the denominator degree; in SpiceSharpParser, the numerator degree must not be greater.
5. Convert `wc` to hertz with `fc = wc/(2*pi)`.

Examples:

| Transfer | What it does |
|----------|--------------|
| `1/(1+s*tau)` | Unity-gain low-pass |
| `wc/(s+wc)` | Same low-pass, written with angular cutoff |
| `s/(s+wc)` | High-pass that blocks DC |
| `10*wc/(s+wc)` | Gain of 10 with one-pole bandwidth |
| `(1+s/wz)/(1+s/wp)` | Lead or lag block, depending on pole/zero placement |

## Common Physical Blocks

### RC Low-Pass

A resistor feeding a capacitor to ground is a low-pass filter:

```text
H(s) = 1 / (1 + s*R*C)
```

Use it for simple bandwidth limits, anti-alias filters, smoothing, and sensor front ends.

### AC Coupling High-Pass

A series capacitor with a resistor to ground blocks DC:

```text
H(s) = s / (s + wc)
```

Use it when you want changes or AC content, but not the DC level.

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

## SpiceSharpParser Subset

SpiceSharpParser intentionally supports a practical subset:

- `E` and `G` LAPLACE sources.
- Voltage input expressions: `V(node)` and `V(node1,node2)`.
- Proper rational polynomials in `s`.
- Finite DC gain.

That means many web examples need adaptation before they are valid here. Avoid unsupported forms such as `exp()`, `sqrt()`, pure `1/s`, ideal delay, `TD=`, `DELAY=`, and function-like `VALUE={LAPLACE(...)}` syntax.
