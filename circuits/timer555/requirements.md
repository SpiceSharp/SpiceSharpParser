# Generic 555 Timer Subcircuit Requirements

## Purpose

Provide reusable mixed-signal building blocks and a functional 555 timer
macro-model that can be instantiated from DigitalSubcircuitLibrary in pure
SpiceSharp circuits or circuits read by SpiceSharpParser.

The model is intended for topology, timing, and control-loop simulation. It is
not a transistor-level replacement for a specific NE555, TLC555, or other
vendor device.

## Required Components

| Component | Required behavior |
| --- | --- |
| Comparator | Differential comparison, supply-relative output, finite delay and output impedance |
| SR latch | Stateful set/hold/reset operation with reset-dominant simultaneous inputs |
| Open-drain driver | Pull low when enabled; high impedance when disabled |
| TIMER555 | Standard 8-pin order: GND, TRIG, OUT, RESET, CTRL, THRESH, DISCH, VCC |

## 555 Functional Rules

- The internal divider establishes nominal trigger and threshold references at
  one-third and two-thirds of the VCC-to-GND span.
- TRIG below VCC/3 sets the latch.
- THRESH above 2*VCC/3 resets the latch.
- Active-low RESET overrides trigger; trigger overrides threshold.
- Reset latch state drives OUT low and enables the low-side DISCH path.
- Set latch state drives OUT high and releases DISCH.
- Ordinary SPICE syntax is required; LTspice compatibility and custom mappings
  are not required by the built-in timer model.

## Nominal Astable Verification Circuit

| Parameter | Value |
| --- | ---: |
| VCC | 5 V |
| RA | 10 kohm |
| RB | 10 kohm |
| C | 10 nF |
| Output load | 10 kohm |
| Control bypass | 10 nF |

For ideal one-third/two-thirds thresholds:

t_high = 0.693 * (RA + RB) * C = 138.6 us

t_low = 0.693 * RB * C = 69.3 us

period = 0.693 * (RA + 2*RB) * C = 207.9 us

frequency = 1 / period = 4.81 kHz

## Acceptance Criteria

| Criterion | Target |
| --- | --- |
| Comparator polarity | All tested differential input cases correct |
| SR latch | Set, hold, reset, and reset-dominant cases correct |
| Open-drain output | Below 0.1 V enabled and above 4.9 V disabled with 10 kohm pull-up |
| Astable period | Within 5% of 207.9 us |
| Astable high time | Within 5% of 138.6 us |
| Astable low time | Within 5% of 69.3 us |
| Timing-capacitor bounds | Crosses both one-third and two-thirds supply levels |
| Active-low reset | Forces output low and discharge path on |
| Packaging | Text library remains embedded and shipped as NuGet content |
