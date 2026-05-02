# LAPLACE Transfer Sources

`LAPLACE` sources model a linear transfer function in the Laplace domain. They are useful when you know the desired gain, poles, zeros, or filter response and want to describe it directly instead of building the same behavior from resistors, capacitors, inductors, or op-amp subcircuits.

SpiceSharpParser supports voltage-controlled `E` and `G` source LAPLACE forms.

## Mental Model

A LAPLACE source applies a transfer function to an input signal:

```text
Y(s) = H(s) * X(s)
H(s) = N(s) / D(s)
```

Where:

| Term | Meaning |
|------|---------|
| `s` | Laplace-domain variable |
| `X(s)` | Input signal in the Laplace domain |
| `Y(s)` | Output signal in the Laplace domain |
| `H(s)` | Transfer function |
| `N(s)` | Numerator polynomial |
| `D(s)` | Denominator polynomial |

For DC operating point analysis, the source is evaluated at:

```text
s = 0
```

For AC frequency analysis at frequency `f` in hertz, the source is evaluated at:

```text
s = j * omega
omega = 2 * pi * f
```

So the frequency response is:

```text
H(j * omega)
```

This value is complex. Its magnitude tells you the gain at that frequency, and its angle tells you the phase shift.

## Supported Syntax

The supported input expression is a voltage probe:

```spice
V(node)
V(node1,node2)
```

`V(node)` means `V(node,0)`. `V(node1,node2)` means the differential voltage `V(node1) - V(node2)`.

### E Source

An `E` LAPLACE source is a voltage-controlled voltage source:

```text
V(out+,out-) = H(s) * V(ctrl+,ctrl-)
```

Supported spellings:

```spice
E<name> <out+> <out-> LAPLACE {V(<ctrl+>)} = {<transfer>}
E<name> <out+> <out-> LAPLACE {V(<ctrl+>)} {<transfer>}
E<name> <out+> <out-> LAPLACE = {V(<ctrl+>)} {<transfer>}
E<name> <out+> <out-> LAPLACE {V(<ctrl+>,<ctrl->)} = {<transfer>}
```

### G Source

A `G` LAPLACE source is a voltage-controlled current source:

```text
I(out+ -> out-) = H(s) * V(ctrl+,ctrl-)
```

For a grounded load resistor connected from `out` to `0`, a positive transconductance usually produces a negative output voltage because the source current is defined from `out+` to `out-`:

```text
V(out) = -Iout * Rload
```

Supported spellings:

```spice
G<name> <out+> <out-> LAPLACE {V(<ctrl+>)} = {<transfer>}
G<name> <out+> <out-> LAPLACE {V(<ctrl+>)} {<transfer>}
G<name> <out+> <out-> LAPLACE = {V(<ctrl+>)} {<transfer>}
G<name> <out+> <out-> LAPLACE {V(<ctrl+>,<ctrl->)} = {<transfer>}
```

## Transfer Polynomials

The transfer expression must be a rational polynomial in `s`:

```text
H(s) = (b0 + b1*s + b2*s^2 + ...) / (a0 + a1*s + a2*s^2 + ...)
```

SpiceSharpParser stores coefficients in ascending powers of `s`:

```text
1 + s*tau        -> [1, tau]
s^2 + 1         -> [1, 0, 1]
wc / (s + wc)   -> numerator [wc], denominator [wc, 1]
```

The transfer must be proper: the numerator degree cannot be greater than the denominator degree. This keeps the source physically usable by the runtime transfer-function behavior.

The DC gain is found by setting `s = 0`:

```text
H(0) = N(0) / D(0)
```

Examples:

```text
1/(1+s*tau)       -> H(0) = 1
wc/(s+wc)         -> H(0) = 1
s/(s+wc)          -> H(0) = 0
10*wc/(s+wc)      -> H(0) = 10
```

Transfers with singular DC gain, such as `1/s`, are rejected.

## Magnitude And Phase

For AC analysis, substitute `s = j*omega`:

```text
H(j*omega) = real + j*imag
```

Magnitude:

```text
|H(j*omega)| = sqrt(real^2 + imag^2)
```

Phase:

```text
phase = atan2(imag, real)
```

### Low-Pass Example

First-order low-pass:

```text
H(s) = 1 / (1 + s*tau)
```

The cutoff frequency is:

```text
fc = 1 / (2*pi*tau)
wc = 2*pi*fc = 1/tau
```

At cutoff:

```text
H(j*wc) = 1 / (1 + j)
        = 0.5 - j*0.5
|H|     = 1 / sqrt(2)
phase   = -pi/4
```

Equivalent form:

```text
H(s) = wc / (s + wc)
```

### High-Pass Example

First-order high-pass:

```text
H(s) = s / (s + wc)
```

At cutoff:

```text
H(j*wc) = j / (1 + j)
        = 0.5 + j*0.5
|H|     = 1 / sqrt(2)
phase   = +pi/4
```

## Worked Examples

### Unity Low-Pass E Source

```spice
* Unity DC gain, one-pole low-pass
.PARAM tau=1u
VIN IN 0 1
ELOW OUT 0 LAPLACE {V(IN)} = {1/(1+s*tau)}
RLOAD OUT 0 1k
.OP
.SAVE V(OUT)
.END
```

At `.OP`, `s = 0`, so:

```text
H(0) = 1/(1+0*tau) = 1
V(OUT) = 1 * V(IN) = 1 V
```

### Parameterized Low-Pass AC Response

```spice
* Cutoff at 1 kHz
.PARAM fc=1k
.PARAM wc={2*PI*fc}
VIN IN 0 AC 1
ELOW OUT 0 LAPLACE {V(IN)} = {wc/(s+wc)}
RLOAD OUT 0 1k
.AC DEC 20 10 100k
.MEAS AC vm_fc FIND VM(OUT) AT=1k
.MEAS AC vp_fc FIND VP(OUT) AT=1k
.END
```

At `f = fc`, expect:

```text
VM(OUT) ~= 0.707
VP(OUT) ~= -pi/4 ~= -0.785
```

### High-Pass AC Response

```spice
.PARAM fc=1k
.PARAM wc={2*PI*fc}
VIN IN 0 AC 1
EHIGH OUT 0 LAPLACE {V(IN)} = {s/(s+wc)}
RLOAD OUT 0 1k
.AC DEC 20 10 100k
.MEAS AC vm_fc FIND VM(OUT) AT=1k
.MEAS AC vp_fc FIND VP(OUT) AT=1k
.END
```

At `f = fc`, expect:

```text
VM(OUT) ~= 0.707
VP(OUT) ~= +pi/4 ~= +0.785
```

### Inverting Low-Pass

```spice
.PARAM gain=10
.PARAM fc=1k
.PARAM wc={2*PI*fc}
VIN IN 0 AC 1
EINV OUT 0 LAPLACE {V(IN)} = {-gain*wc/(s+wc)}
RLOAD OUT 0 1k
.AC DEC 20 10 100k
.END
```

At DC:

```text
H(0) = -gain
```

The negative sign adds 180 degrees of phase inversion to the low-pass response.

### G Source Through A Load

```spice
* 1 mS low-pass transconductance into a 1 k load
.PARAM gm=1m
.PARAM tau=1u
VIN IN 0 1
GLOW OUT 0 LAPLACE {V(IN)} = {gm/(1+s*tau)}
RLOAD OUT 0 1k
.OP
.SAVE V(OUT)
.END
```

At `.OP`:

```text
Iout = gm * V(IN) = 1m * 1 = 1 mA
V(OUT) = -Iout * RLOAD = -1m * 1k = -1 V
```

The negative voltage comes from the current direction of the `G` source and the grounded load.

### Differential Input

```spice
VINP INP 0 2
VINN INN 0 0.5
EDIFF OUT 0 LAPLACE {V(INP,INN)} = {1/(1+s*1u)}
RLOAD OUT 0 1k
.OP
.SAVE V(OUT)
.END
```

At `.OP`:

```text
V(INP,INN) = V(INP) - V(INN) = 1.5 V
H(0) = 1
V(OUT) = 1.5 V
```

## Equivalent Supported Spellings

These three lines are equivalent:

```spice
E1 OUT 0 LAPLACE {V(IN)} = {1/(1+s*1u)}
E1 OUT 0 LAPLACE {V(IN)} {1/(1+s*1u)}
E1 OUT 0 LAPLACE = {V(IN)} {1/(1+s*1u)}
```

The same spelling variants are supported for `G` sources.

## Common Mistakes

| Mistake | Why it fails | Use instead |
|---------|--------------|-------------|
| `1/s` | Singular DC gain at `s = 0` | Use a finite DC-gain transfer |
| `s` | Improper transfer: numerator order is greater than denominator order | Use `s/(s+wc)` |
| `sin(s)` | Not a rational polynomial in `s` | Use polynomial/rational expressions only |
| `V(a)-V(b)` | Input expression shape is unsupported | Use `V(a,b)` |
| `I(Vsense)` | Current-controlled LAPLACE is not supported yet | Use supported `E`/`G` voltage input forms |
| `M=2` | LAPLACE multiplier option is not supported yet | Put the multiplier in `H(s)`, for example `{2/(1+s*tau)}` |
| `TD=1n` or `DELAY=1n` | Delay syntax is not supported yet | Omit delay |
| `VALUE={LAPLACE(...)}` | Function-like LAPLACE syntax is not supported yet | Use source-level `E`/`G ... LAPLACE ...` |

## Current Limitations

- Only `E` and `G` voltage-controlled LAPLACE sources are supported.
- Only `V(node)` and `V(node1,node2)` input expressions are supported.
- `B`, `F`, and `H` LAPLACE forms are not supported yet.
- Function-like `VALUE={LAPLACE(...)}` syntax is not supported yet.
- `M=`, `TD=`, `DELAY=`, and explicit internal-state options are not supported yet.
- Transfers must be finite, proper rational polynomials in `s` with non-singular DC gain.
