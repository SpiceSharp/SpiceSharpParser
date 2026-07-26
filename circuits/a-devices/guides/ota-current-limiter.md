# OTA Current-limiter Guide

Circuit: [`../ota-current-limiter.cir`](../ota-current-limiter.cir)

## What Is an OTA?

An operational transconductance amplifier converts an input-voltage difference
into output current. This differs from an ordinary operational amplifier,
which is normally described as producing an output voltage. The ideal relation
is `Iout = G * Vdifference`, where transconductance `G` has units of amperes per
volt.

Output current is convenient for charging capacitors and building tunable
filters, oscillators, automatic-gain controls, and analog multipliers. The
LTspice OTA A-device has two differential input pairs, so their product can
control both magnitude and polarity. Current limits keep that ideal result
inside configured source and sink capabilities.

This is a functional nonlinear current-output model. It does not reproduce a
particular OTA's transistor bias network, bandwidth, noise spectrum, input
offset drift, or temperature behavior unless those effects are represented by
supported parameters.

## What the Circuit Does

The OTA multiplies two differential inputs. The second pair is fixed at 1 V,
while `VIN1P` applies first +1 V and then -1 V to the first pair. With `G=1`,
the unconstrained result is far beyond the configured 100 uA limit, so the
output saturates cleanly in both directions.

## Reading the A-device Line

```spice
AOTA in1n in1p mulp muln 0 0 out 0 OTA G=1 Iout=100u Vhigh=5 Vlow=-5
```

The first four terminals form two differential pairs: `(in1n,in1p)` and
`(mulp,muln)`. Terminal 7 is the current output and terminal 8 is common. The
unused positions repeat common.

Setting only `Iout=100u` establishes dependent defaults of `Isrc=100u` and
`Isink=-100u`. `Vhigh` and `Vlow` keep the resulting output voltage inside
the +/-5 V compliance range.

## Why VSENSE Is Present

```spice
VSENSE out load 0
RLOAD load 0 10k
```

A zero-volt source does not change the intended voltage, but its branch
current is directly measurable. `source_current` reads about +100 uA at
1.5 ms and `sink_current` reads about -100 uA at 2.5 ms. The 10 kOhm load
converts those currents to approximately +/-1 V.

## Useful Changes

- Add `Isink=-20u Asym` to demonstrate unequal source and sink limits.
- Add `Linear` and reduce `G` to study unsaturated transconductance.
- Add `Ioffset` or `Ref` to shift the transfer curve.
- Add `Cout=100p` to explore transient output loading.

Do not set `Rout=1` for this example: OTA `Rout` is output leakage resistance,
so a small value would shunt most of the limited current to common.
