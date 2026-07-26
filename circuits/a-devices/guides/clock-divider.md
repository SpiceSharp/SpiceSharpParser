# COUNTER Clock-divider Guide

Circuit: [`../clock-divider.cir`](../clock-divider.cir)

## What Is a Counter?

A digital counter remembers how many clock edges have occurred. Each rising
edge advances its internal state by one; after a chosen number of states, the
sequence wraps around and starts again.

Counters are used for event counting, timers, sequencers, and frequency
division. If an output repeats once for every four input edges, its frequency
is one quarter of the clock frequency. The `COUNTER` A-device provides that
repeating output directly instead of exposing a multi-bit binary count.

The functional model remembers edge count and produces finite-voltage Q and QB
outputs. It does not model the individual flip-flops, propagation ripple,
metastability, or maximum clock rate of a physical counter IC.

## What the Circuit Does

`VCLOCK` produces a 5 V clock with a 10 us period, so its frequency is
100 kHz. The COUNTER uses `Cycles=4`, making the output period four input
periods:

```text
Toutput = 4 * 10 us = 40 us
foutput = 100 kHz / 4 = 25 kHz
```

`Duty=0.5` keeps `divided` high for two count states and low for two. The
`divided_b` output is its complement. Both outputs have 10 kOhm loads so they
are electrically observable and have a DC path.

## Reading the A-device Line

```spice
ACOUNT clock reset 0 0 0 divided_b divided 0 COUNTER cycles=4 duty=0.5 Vhigh=5 Vlow=0 Rout=10
```

| Position | Connection | Meaning |
| ---: | --- | --- |
| 1 | `clock` | Rising-edge clock input |
| 2 | `reset` | Active-high reset |
| 3–5 | `0` | Unused; equal to common |
| 6 | `divided_b` | Complementary output |
| 7 | `divided` | Main output |
| 8 | `0` | Common node |

`Vhigh` and `Vlow` establish 5 V logic. `Rout=10` gives each output a finite
10 ohm drive resistance.

## What to Plot and Measure

Plot `V(clock)`, `V(divided)`, and `V(divided_b)`. Every four clock edges,
the main output sequence repeats. `output_period` measures 40 us between
rising crossings. `output_low` samples at 25 us and should be near 0 V.

## Useful Changes

- Change `Cycles=8` to obtain 12.5 kHz.
- Change `Duty=0.25` with `Cycles=4` for one high state and three low states.
- Pulse `reset` high to restart the count sequence.

`Cycles` is required. Use an integer of at least 2, and retain loads on outputs
that you want structural linting and simulation to treat as connected.
