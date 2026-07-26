# SAMPLEHOLD Sensor-capture Guide

Circuit: [`../sample-and-hold-sensor.cir`](../sample-and-hold-sensor.cir)

## What Is a Sample-and-hold?

A sample-and-hold takes a snapshot of an analog voltage and keeps that value
available while the original signal continues to change. It is analog memory:
the stored quantity is a voltage rather than a digital zero or one.

Sample-and-hold circuits are commonly placed ahead of analog-to-digital
converters so the input remains steady during conversion. A physical circuit
usually uses an electronic switch and a capacitor. Real implementations have
acquisition time, leakage droop, charge injection, noise, and finite accuracy.

The `SAMPLEHOLD` A-device models the useful track/sample decision, differential
input, output limits, and loading. Its held integration state intentionally has
no invented long-term droop, so use a more detailed model when physical hold
error matters.

## What the Circuit Does

`VSENSOR` ramps linearly from 0 V to 4 V during the first second. At 250 ms,
the sensor is therefore at 1 V. `VSAMPLE` rises at that instant and captures
the differential input. The sensor later reaches 4 V, but `held` remains near
the captured 1 V through the end of the three-second simulation.

## Reading the A-device Line

```spice
ASH sensor 0 sample 0 0 0 held 0 SAMPLEHOLD Rout=10
```

| Position | Connection | Meaning |
| ---: | --- | --- |
| 1 | `sensor` | Positive input |
| 2 | `0` | Negative input |
| 3 | `sample` | Rising-edge sample clock |
| 4 | `0` | Track control, unused here |
| 5–6 | `0` | Unused |
| 7 | `held` | Output |
| 8 | `0` | Common node |

Because terminal 4 equals common, the device operates in edge-triggered sample
mode. Driving terminal 4 above `Ref` instead selects continuous track mode.

## What the Measurements Prove

- `held_early`, at 500 ms, is about 1 V.
- `held_late`, at 2.5 s, is still about 1 V.
- `sensor_late`, at 2.5 s, is 4 V.

The comparison between the latter two values proves that the output is held,
not merely following the input slowly. `Rout=10` and the 100 kOhm load cause
negligible loading error.

## Useful Changes

- Move the sample pulse to 500 ms to capture approximately 2 V.
- Drive terminal 4 high to build a track-and-hold example.
- Change input 2 to a nonzero reference to sample a differential signal.
- Add output capacitance or a lower load resistance to explore settling.

`SAMPLEHOLD Td` is rejected because native delay semantics are not yet
implemented; do not add it to this circuit.
