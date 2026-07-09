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

Waveforms can be written in function form, for example `PULSE(...)`, or as a
waveform name followed by parameters, for example `PULSE 0 5 0 1n 1n 5u 10u`.
The same waveform syntax is available on independent current sources.

### PULSE — Rectangular Pulse Train

```
PULSE(<v1> <v2> <delay> <rise> <fall> <pulse_width> <period>)
PULSE(<v1> <v2> <delay> <rise> <fall> <pulse_width> <period> <Ncycles>)
```

```spice
V1 IN 0 PULSE(0 5 0 10n 10n 500u 1m)
V2 GATE 0 PULSE(0 1 2n 1n 1n 3n 10n 2)
```

The seven-argument form is the normal repeating pulse train. The eight-argument
LTspice finite-cycle form is available only when `CompatibilityOptions.LTspice`
is enabled. In that mode, `<Ncycles>` limits the source to that many periods and
then returns it to `<v1>`. The period and cycle count must be positive.

### SIN / SINE — Sinusoidal

```
SIN(<offset> <amplitude> <frequency> [<delay> [<damping> [<phase>]]])
SINE(<offset> <amplitude> <frequency> [<delay> [<damping> [<phase>]]])
SINE(<offset> <amplitude> <frequency> <delay> <damping> <phase> <Ncycles>)
```

```spice
V1 IN 0 SIN(0 1 1k)
V2 SIG 0 SIN(2.5 2.5 60 0 0 0)
V3 BURST 0 SINE(0 1 1k 0 0 0 3)
```

The seven-argument LTspice finite-cycle form is available only when
`CompatibilityOptions.LTspice` is enabled. In that mode, `<Ncycles>` limits the
source to that many periods after `<delay>`, then returns it to `<offset>`.
The frequency and cycle count must be positive.

### EXP — Exponential Rise/Fall

```
EXP(<v1> <v2> <td1> <tau1> <td2> <tau2>)
```

```spice
V1 IN 0 EXP(0 5 1u 100n 10u 200n)
```

`EXP` starts at `<v1>`, rises toward `<v2>` after `<td1>` with time constant
`<tau1>`, and falls back toward `<v1>` after `<td2>` with time constant
`<tau2>`. Both time constants must be positive.

### PWL — Piecewise Linear

```
PWL(<t1> <v1> <t2> <v2> ...)
PWL file = "<path>"
PWL REPEAT FOR <n> (<t1>,<v1>,...) ENDREPEAT
PWL REPEAT FOREVER (<t1>,<v1>,...) ENDREPEAT
```

```spice
V1 IN 0 PWL(0 0 1m 5 2m 5 3m 0)
V2 IN 0 PWL file = "Resources\pwl_reference.txt"
V3 IN 0 PWL REPEAT FOR 3 (1m,1,3m,3) ENDREPEAT
```

Inline PWL data uses time/value pairs. File-backed PWL data reads a local text
file with two numeric columns and an optional header row. Leading blank lines
and full-line comments beginning with `;`, `#`, `*`, or `//` are skipped.
Spaces, commas, semicolons, and tabs are recognized from the first meaningful
line. In LTspice mode, simple non-nested `REPEAT FOR <n>` and `REPEAT FOREVER`
blocks are supported with local repeat times. Nested repeats, trigger-based
PWL restarts, time/value scale factors, `SCOPEDATA`, and relative `+time`
points are not claimed yet. Missing files, empty files, missing data rows,
malformed rows, and unsupported LTspice PWL variants produce targeted `PWL`
validation diagnostics.

### SFFM — Single-Frequency FM

```
SFFM(<offset> <amplitude> <carrier_freq> <mod_index> <signal_freq> [<carrier_phase> [<signal_phase>]])
```

```spice
V1 IN 0 SFFM(0 1 1k 5 100)
```

### AM — Amplitude Modulation

```
AM(<amplitude> <offset> <modulation_freq> <carrier_freq> [<delay> [<carrier_phase> [<signal_phase>]]])
```

```spice
V1 IN 0 AM(1 0 100 10k 0 0 0)
```

### Wave-File Input

```
wavefile=<path> chan=<n> [amplitude=<scale>]
WAVE wavefile=<path> chan=<n> [amplitude=<scale>]
```

```spice
V1 IN 0 wavefile="input.wav" chan=0 amplitude=0.5
V2 IN 0 WAVE wavefile="input.wav" chan=1
```

Wave-file input converts the selected audio channel to a PWL waveform. The
channel must be explicit; LTspice channel defaults are not inferred. Missing
`wavefile=`, missing `chan=`, missing files, and invalid channel expressions
produce targeted validation diagnostics.

## LTspice Source Compatibility

With `CompatibilityOptions.LTspice`, independent sources also support
`tbl=(expr,x1,y1,...)`, which is lowered to the existing behavioral
`table(...)` expression path:

```spice
V1 OUT 0 tbl=(V(IN), 0,0, 1,5)
```

Common topology-changing LTspice source options are synthesized by the parser
as helper components instead of being passed to the core source model:

```spice
V1 OUT 0 5 Rser=0.2 Cpar=10p
I1 OUT 0 1m load=1k
```

`Rser=<value>` adds a series resistor, `Cpar=<value>` adds a capacitor across
the source terminals, and `load=<value>` adds a resistor across the source
terminals. `R=<value>` maps to series resistance on voltage sources and load
resistance on current sources.

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

## MNA View

An ideal voltage source imposes a voltage equation:

$$
V(p) - V(n) = V_{\text{source}}
$$

The current through that source is not known in advance, so modified nodal
analysis adds a branch-current unknown, for example `I(V1)`.

The source contributes:

```text
node p KCL row:      +I(V1)
node n KCL row:      -I(V1)
branch row I(V1):    V(p) - V(n) = source value
```

That branch-current unknown is also why `I(V1)` can be saved or used as the
control current for `F` and `H` sources.

See [How SpiceSharp Solves Circuits](spicesharp-architecture.md#modified-matrix-algorithm-step-by-step)
for a worked voltage-source matrix example.

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
