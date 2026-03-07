# I — Independent Current Source

An independent current source provides a specified current between two nodes. It supports DC, AC, and time-domain waveforms, similar to voltage sources.

## Syntax

```
I<name> <node+> <node-> [DC <dc_value>] [AC <mag> [<phase>]] [<waveform>] [M=<m>]
```

| Parameter | Description |
|-----------|-------------|
| `node+`, `node-` | Current flows from node+ through the source to node- |
| `DC <value>` | DC current value |
| `AC <mag> [phase]` | AC magnitude and phase |
| `waveform` | Time-domain waveform (PULSE, SIN, PWL, SFFM, AM) |
| `M=m` | Multiplier (equivalent to m parallel sources) |

## Examples

```spice
* DC current source: 1mA
I1 VCC OUT DC 1m

* AC current source
I2 IN 0 AC 1m 0

* Pulse current source
I3 A B PULSE(0 10m 0 1n 1n 5u 10u)

* Sinusoidal current
I4 IN 0 SIN(0 5m 1k)

* With multiplier
I5 A B DC 1m M=4
```

## Waveform Types

Current sources support the same waveform types as voltage sources:

- `PULSE` — Rectangular pulse train
- `SIN` / `SINE` — Sinusoidal
- `PWL` — Piecewise linear
- `SFFM` — Single-frequency FM
- `AM` — Amplitude modulation

See the [V — Voltage Source](voltage-source.md) article for waveform syntax details.

## Behavioral Current Source

```spice
I1 OUT 0 VALUE={V(CTRL)*1m}
```
