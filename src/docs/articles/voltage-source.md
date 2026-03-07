# V — Independent Voltage Source

An independent voltage source provides a specified voltage between two nodes. It supports DC, AC, and various time-domain waveforms.

## Syntax

```
V<name> <node+> <node-> [DC <dc_value>] [AC <mag> [<phase>]] [<waveform>]
```

| Parameter | Description |
|-----------|-------------|
| `node+`, `node-` | Positive and negative terminal nodes |
| `DC <value>` | DC voltage (used for `.OP` and `.DC`) |
| `AC <mag> [phase]` | AC magnitude and phase in degrees (used for `.AC`) |
| `waveform` | Time-domain waveform (used for `.TRAN`) |

## DC Source

```spice
V1 VCC 0 5
V2 IN 0 DC 3.3
```

## AC Source

```spice
V1 IN 0 DC 0 AC 1 0
V1 IN 0 AC 1
```

## Waveform Types

### PULSE — Rectangular Pulse Train

```
PULSE(<v1> <v2> <delay> <rise> <fall> <pulse_width> <period>)
```

```spice
V1 IN 0 PULSE(0 5 0 10n 10n 500u 1m)
```

### SIN — Sinusoidal

```
SIN(<offset> <amplitude> <frequency> [<delay> [<damping> [<phase>]]])
```

```spice
V1 IN 0 SIN(0 1 1k)
V2 SIG 0 SIN(2.5 2.5 60 0 0 0)
```

### PWL — Piecewise Linear

```
PWL(<t1> <v1> <t2> <v2> ... [repeat])
```

```spice
V1 IN 0 PWL(0 0 1m 5 2m 5 3m 0)
```

### SFFM — Single-Frequency FM

```
SFFM(<offset> <amplitude> <carrier_freq> <mod_index> <signal_freq> [<carrier_phase> [<signal_phase>]])
```

```spice
V1 IN 0 SFFM(0 1 1k 5 100)
```

### AM — Amplitude Modulation

Available for amplitude-modulated waveforms.

## Behavioral Voltage Source

```spice
V1 OUT 0 VALUE={V(IN)*0.5}
V2 OUT 0 TABLE={V(CTRL)} = (0,0) (1,3.3) (2,5)
```

## Current Measurement

The current through a voltage source can be measured with `I(Vname)`:

```spice
.SAVE I(V1)
```

## Typical Usage

```spice
Pulse response
V1 IN 0 PULSE(0 5 1u 10n 10n 5u 10u)
R1 IN OUT 1k
C1 OUT 0 1n
.TRAN 1n 20u
.SAVE V(OUT) V(IN) I(V1)
.END
```
