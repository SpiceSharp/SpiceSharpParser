# MODULATOR Frequency-switch Guide

Circuit: [`../fm-modulator.cir`](../fm-modulator.cir)

## What Is a Modulator?

A modulator changes a carrier waveform according to another signal. Frequency
modulation (FM) changes how quickly the carrier oscillates, while amplitude
modulation (AM) changes its height. These ideas are used in communications,
signal generation, tone synthesis, sweeps, and voltage-controlled oscillators.

The `MODULATOR` A-device combines both operations. FM selects or interpolates
between the `Space` and `Mark` frequencies, and AM multiplies the sine-wave
amplitude. “Space” and “mark” are traditional names for the two signaling
frequencies used to represent different states.

This model is an ideal phase-continuous signal generator with finite output
loading. It does not model a radio transmitter, spectral noise, distortion,
bandwidth limits, or demodulation at a receiver.

## What the Circuit Does

`VFM` holds FM at 0 V until 3 ms and then moves it to 1 V. The MODULATOR maps
those endpoints to `Space=1k` and `Mark=2k`:

```text
FM = 0 V -> 1 kHz -> 1 ms period
FM = 1 V -> 2 kHz -> 0.5 ms period
```

The phase accumulator remains continuous while frequency changes, so the
carrier does not restart at 3 ms.

## Reading the A-device Line

```spice
AMOD fm 0 0 0 0 0 carrier 0 MODULATOR mark=2k space=1k Rout=1
```

Terminal 1 is FM, terminal 2 is AM, terminal 7 is output, and terminal 8 is
common. Terminal 2 repeats common, which marks AM as unused and selects the
native default amplitude of 1. The other unused positions also repeat common.

## What the Measurements Prove

- `space_period` is approximately 1 ms before the FM transition.
- `mark_period` is approximately 0.5 ms afterward.
- `default_amplitude` is approximately 1 V despite no driven AM source.

`.OPTIONS plotwinsize=0` preserves waveform samples for reliable period and
peak measurements.

## Useful Changes

- Add `VAM am 0 2` and connect terminal 2 to `am` for a 2 V amplitude.
- Set FM to 0.5 V for 1.5 kHz linear interpolation.
- Apply a ramp or sine to FM for chirp or continuous-FM experiments.
- Use values outside 0–1 V to observe frequency extrapolation.

Both `Mark` and `Space` are required. Omitting either is diagnosed rather than
silently replaced by a hidden frequency.
