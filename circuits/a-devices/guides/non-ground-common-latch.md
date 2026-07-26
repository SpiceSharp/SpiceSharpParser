# SRFLOP Non-ground-common Guide

Circuit: [`../non-ground-common-latch.cir`](../non-ground-common-latch.cir)

## What Is an SR Latch?

An SR latch is a one-bit memory with separate Set and Reset controls. Set makes
Q high, Reset makes Q low, and with neither control active the latch remembers
its previous state. QB provides the complementary value.

SR latches are used for asynchronous control, alarm retention, switch
debouncing, and as building blocks inside more complex flip-flops. Unlike a D
flip-flop, an SR latch does not need a clock edge to change state. This model is
reset-dominant, so Reset wins if Set and Reset are active together.

The `SRFLOP` A-device models logical memory, output delay, finite drive, and a
local common reference. It does not model transistor-level metastability or
the analog resolution of a forbidden simultaneous-input condition.

## What the Circuit Does

`VCOMMON` raises the device common node to 1 V above global ground. SET pulses
5 V above common at 10 ns and RESET does the same at 30 ns. Q is therefore
5 V relative to common after SET and 0 V relative to common after RESET.

## Reading the A-device Line

```spice
ASR set reset common common common common q common SRFLOP Vhigh=5 Vlow=0 Td=1n Rout=1
```

| Position | Connection | Meaning |
| ---: | --- | --- |
| 1 | `set` | Active-high SET |
| 2 | `reset` | Active-high RESET |
| 3–5 | `common` | Unused |
| 6 | `common` | Unused QB output; electrically detached |
| 7 | `q` | Main output |
| 8 | `common` | Device common node |

The repeated `common` nodes are intentional. Replacing them with `0` would
create active global-ground connections, not unused terminals.

## Understanding the Measurements

- `q_after_set = V(q,common)` is approximately 5 V.
- `q_after_reset = V(q,common)` is approximately 0 V.
- `absolute_q_after_set = V(q)` is approximately 6 V because common itself is
  at 1 V.

This distinction is the key lesson: A-device levels are common-relative.

## Useful Changes

- Change `VCOMMON` to 2 V; the high absolute Q level becomes about 7 V while
  `V(q,common)` remains 5 V.
- Connect terminal 6 to a real `qb` node and add a load to observe both outputs.
- Assert SET and RESET together to see reset-dominant behavior.
- Change `Td` and measure the interval from control edge to Q transition.

Use the actual common-node name in every unused position whenever terminal 8
is not ground.
