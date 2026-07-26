# DFLOP Edge-capture Guide

Circuit: [`../edge-capture-dff.cir`](../edge-capture-dff.cir)

## What Is a D Flip-flop?

A D flip-flop is a one-bit memory controlled by a clock. The D input means
“data.” On the active clock edge, the device copies D to Q and then holds that
value until another clock edge or an asynchronous control changes it.

D flip-flops are building blocks for registers, counters, state machines, and
synchronizers. They let many parts of a digital system update at defined times
rather than reacting continuously to every input change. QB is the logical
complement of Q.

The `DFLOP` A-device captures the logical edge and applies configured output
delay and loading. It is a functional model, not a transistor-level timing
model, so it does not predict analog metastability or device-specific setup
and hold limits.

## What the Circuit Does

The first clock rising edge occurs at 10 ns while `data` is high. The second
occurs at 30 ns after `data` has returned low. Q therefore captures high, then
low; QB always provides the complementary state.

## Reading the A-device Line

```spice
ADFF data 0 clock 0 0 qb q 0 DFLOP Vhigh=5 Vlow=0 Td=1n Rout=10
```

| Position | Connection | Meaning |
| ---: | --- | --- |
| 1 | `data` | D input |
| 2 | `0` | Unused |
| 3 | `clock` | Positive-edge clock |
| 4 | `0` | Unused active-high PRE input |
| 5 | `0` | Unused active-high CLR input |
| 6 | `qb` | Complementary output |
| 7 | `q` | Main output |
| 8 | `0` | Common node |

`Td=1n` delays the visible output response by 1 ns. The measurements are at
15 ns and 35 ns, safely after that delay, and should read approximately 5 V
and 0 V respectively.

## What to Plot

Plot `V(data)`, `V(clock)`, `V(q)`, and `V(qb)`. A change on D while the clock
is low does not immediately change Q; only the next rising edge is captured.

## Useful Changes

- Drive terminal 4 high to exercise asynchronous preset.
- Drive terminal 5 high to exercise asynchronous clear; clear has priority.
- Increase `Td` and move the `.MEAS` sample times to observe propagation delay.
- Move the D transition close to a clock edge to study the functional edge
  decision.

This is a functional flip-flop model. It does not predict analog metastability
or a physical device's setup/hold-time failure probability.
