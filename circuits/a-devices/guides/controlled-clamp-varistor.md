# VARISTOR Controlled-clamp Guide

Circuit: [`../controlled-clamp-varistor.cir`](../controlled-clamp-varistor.cir)

## What Is a Varistor?

A varistor is a voltage-dependent resistor. At ordinary voltages it has very
high resistance and draws little current. When the voltage becomes too large,
its resistance falls sharply, allowing it to divert current and limit the
voltage seen by the protected circuit.

Physical metal-oxide varistors (MOVs) are commonly placed across power inputs
to absorb short overvoltage spikes. They are passive, two-terminal parts with
a fixed characteristic. They are not precision voltage regulators, and a real
MOV can heat up, age, or fail after absorbing too much surge energy.

The LTspice `VARISTOR` A-device is a controlled functional model rather than a
literal MOV model. It separates the circuit into:

- a differential control input that sets the clamp magnitude;
- an output terminal that is protected symmetrically in both polarities; and
- a common reference terminal.

You can think of it as a programmable bidirectional limiter. In this example,
the 2 V control input means “allow the output to move normally between
approximately -2 V and +2 V; conduct strongly outside that window.”

## What the Circuit Does

`VCONTROL=2 V` sets a symmetric +/-2 V clamp threshold. `VDRIVE` first applies
+10 V and later -10 V through a 1 kOhm resistor. Outside the threshold,
`Rclamp=10` supplies the incremental clamp slope. `Roff=1T` makes leakage
inside the window negligible.

## Reading the A-device Line

```spice
AVAR control 0 0 0 0 0 out 0 VARISTOR Rclamp=10 Roff=1T
```

Terminals 1 and 2 are the differential control input, terminal 7 is the
clamped node, and terminal 8 is common. Positions 3 through 6 are unused and
therefore repeat common.

For the positive interval, Kirchhoff's current law gives:

```text
(10 - Vout) / 1000 = (Vout - 2) / 10
Vout = 2.079 V
```

The negative interval is symmetric, producing approximately -2.079 V. These
are the `positive_clamp` and `negative_clamp` measurements.

The output is slightly beyond +/-2 V because the clamp is not an ideal voltage
source. Current flowing through its finite 10 ohm `Rclamp` creates the extra
79 mV. A smaller `Rclamp` would hold the output closer to the requested
threshold.

## Useful Changes

- Sweep `VCONTROL` to create a programmable limiter.
- Increase `Rclamp` for a softer clamp or reduce it for a stiffer clamp.
- Reduce `Roff` to make inside-window leakage observable.
- Reverse the two control terminals; the magnitude remains the same because
  the threshold uses the absolute differential voltage.

This is a functional voltage-controlled clamp. It does not model surge energy,
self-heating, aging, or destruction of a physical varistor.
