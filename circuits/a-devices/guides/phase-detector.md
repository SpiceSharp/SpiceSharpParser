# PHASEDET Charge-pump Guide

Circuit: [`../phase-detector.cir`](../phase-detector.cir)

## What Is a Phase Detector?

A phase detector compares the timing of two repeating signals. If A arrives
first, the detector indicates that A leads; if B arrives first, it indicates
that B leads. A matching edge ends the correction interval.

Phase detectors are central to phase-locked loops (PLLs), where a reference
clock is compared with feedback from an oscillator. The signed error pulses
charge or discharge a loop filter, which adjusts the oscillator until the two
signals align in frequency and phase.

`PHASEDET` is a phase/frequency-detector and charge-pump functional model. It
represents edge ordering, retained unmatched-edge state, signed current, and
voltage compliance. It does not model analog dead zones, jitter, mismatch,
leakage, or noise of a physical PLL detector.

## What the Circuit Does

The reference input rises at 1 us and feedback rises at 3 us. During that
2 us lead interval, PHASEDET sources `Iout=1m`. The feedback edge matches the
pending reference edge and returns the output current to zero. The sequence
repeats every 20 us.

## Reading the A-device Line

```spice
APD reference feedback 0 0 0 0 error 0 PHASEDET Iout=1m Vhigh=5 Vlow=-5
```

| Position | Connection | Meaning |
| ---: | --- | --- |
| 1 | `reference` | A input |
| 2 | `feedback` | B input |
| 3–6 | `0` | Unused |
| 7 | `error` | Signed current output |
| 8 | `0` | Common node |

`RLOOP=1k` converts charge-pump current into voltage:

```text
Verror = Iout * RLOOP = 1 mA * 1 kOhm = 1 V
```

Thus `reference_leads` is approximately +1 V at 2.5 us, while
`edges_matched` is approximately 0 V at 5 us.

## Useful Changes

- Swap the source delays so feedback leads; the active level becomes -1 V.
- Change the delay difference to vary the error-pulse width.
- Replace `RLOOP` with an RC loop filter to turn pulse area into a control
  voltage for a simple PLL experiment.
- Change `Iout` to adjust loop gain without changing input timing.

The output is a current source with compliance limits, not a logic voltage
driver. Its observed voltage depends on the attached load or loop filter.
