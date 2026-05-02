# LAPLACE Transfer Sources

`LAPLACE` sources model a linear transfer function in the Laplace domain. They are useful when you know the desired gain, poles, zeros, or filter response and want to describe it directly instead of building the same behavior from resistors, capacitors, inductors, or op-amp subcircuits.

SpiceSharpParser supports source-level `E`, `G`, `F`, and `H` LAPLACE forms, plus function-style `LAPLACE(input, transfer)` inside `VALUE`, `B ... V=`, and `B ... I=` behavioral expressions.

If you are new to Laplace transforms, start with [Laplace Transform Basics for Circuit Simulation](laplace-basics.md). This article is the syntax and reference page.

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

The supported input expression is a voltage or current probe:

```spice
V(node)
V(node1,node2)
I(source)
```

`V(node)` means `V(node,0)`. `V(node1,node2)` means the differential voltage `V(node1) - V(node2)`. Use voltage probes with `E` and `G` sources. `I(source)` means the current through a controlling voltage source, and is supported with `F` and `H` sources.

### E Source

An `E` LAPLACE source is a voltage-controlled voltage source:

```text
V(out+,out-) = H(s) * V(ctrl+,ctrl-)
```

Supported spellings:

```spice
E<name> <out+> <out-> LAPLACE {<input>} = {<transfer>} [M=<m>] [TD=<delay>|DELAY=<delay>]
E<name> <out+> <out-> LAPLACE {<input>} {<transfer>} [M=<m>] [TD=<delay>|DELAY=<delay>]
E<name> <out+> <out-> LAPLACE = {<input>} {<transfer>} [M=<m>] [TD=<delay>|DELAY=<delay>]
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
G<name> <out+> <out-> LAPLACE {<input>} = {<transfer>} [M=<m>] [TD=<delay>|DELAY=<delay>]
G<name> <out+> <out-> LAPLACE {<input>} {<transfer>} [M=<m>] [TD=<delay>|DELAY=<delay>]
G<name> <out+> <out-> LAPLACE = {<input>} {<transfer>} [M=<m>] [TD=<delay>|DELAY=<delay>]
```

### F Source

An `F` LAPLACE source is a current-controlled current source:

```text
I(out+ -> out-) = H(s) * I(Vsense)
```

Supported spellings:

```spice
F<name> <out+> <out-> LAPLACE {I(<source>)} = {<transfer>} [M=<m>] [TD=<delay>|DELAY=<delay>]
F<name> <out+> <out-> LAPLACE {I(<source>)} {<transfer>} [M=<m>] [TD=<delay>|DELAY=<delay>]
F<name> <out+> <out-> LAPLACE = {I(<source>)} {<transfer>} [M=<m>] [TD=<delay>|DELAY=<delay>]
```

### H Source

An `H` LAPLACE source is a current-controlled voltage source:

```text
V(out+,out-) = H(s) * I(Vsense)
```

Supported spellings:

```spice
H<name> <out+> <out-> LAPLACE {I(<source>)} = {<transfer>} [M=<m>] [TD=<delay>|DELAY=<delay>]
H<name> <out+> <out-> LAPLACE {I(<source>)} {<transfer>} [M=<m>] [TD=<delay>|DELAY=<delay>]
H<name> <out+> <out-> LAPLACE = {I(<source>)} {<transfer>} [M=<m>] [TD=<delay>|DELAY=<delay>]
```

### Function-Style LAPLACE

The same transfer can be written inside behavioral expressions:

```spice
ELOW OUT 0 VALUE={LAPLACE(V(IN), 1/(1+s*tau))}
GLOW OUT 0 VALUE={LAPLACE(V(IN), gm/(1+s*tau))}
BLOW OUT 0 V={LAPLACE(V(IN), wc/(s+wc))}
BGM OUT 0 I={LAPLACE(V(IN), gm/(1+s*tau))}
BMIX OUT 0 V={1 + 2*LAPLACE(V(IN), 1/(1+s))}
BDELAY OUT 0 V={LAPLACE(V(IN), 1/(1+s*tau), M=2, TD=1n)}
BDIFF OUT 0 V={LAPLACE(V(A)-V(B), 1/(1+s))}
BINHELP OUT 0 V={LAPLACE(2*V(IN), 1/(1+s))}
```

If the whole expression is one `LAPLACE(...)` call, SpiceSharpParser creates the matching Laplace source directly. If the expression mixes `LAPLACE(...)` with other terms, each call is lowered to an internal helper voltage source and the behavioral expression references the helper voltage. These helper entities are an implementation detail, but they can appear during low-level circuit inspection.

Function-style calls also accept arbitrary scalar input expressions. Direct probes such as `V(node)`, `V(node1,node2)`, `V(a)-V(b)`, and `I(source)` stay fast paths. Other inputs, such as `2*V(IN)` or `V(A)+I(VSENSE)*rscale`, are lowered through an internal behavioral voltage helper before the Laplace transfer.

### Options

Supported options must use assignment syntax:

| Option | Meaning |
|--------|---------|
| `M=<m>` | Finite constant multiplier. It may be positive, negative, or zero. SpiceSharpParser folds it into the numerator coefficients. |
| `TD=<delay>` | Finite constant non-negative runtime delay parameter in seconds. |
| `DELAY=<delay>` | Alias for `TD`. |

Use either `TD` or `DELAY`, not both, and specify delay only once. Bare forms such as `TD 1n` are not supported.

For function-style `LAPLACE(...)`, options may be passed inside the call:

```spice
LAPLACE(V(IN), 1/(1+s*tau), M=2)
LAPLACE(V(IN), 1/(1+s*tau), TD=1n)
LAPLACE(V(IN), 1/(1+s*tau), DELAY=1n)
```

Inline options are local to that call, so mixed expressions can give each `LAPLACE(...)` term its own multiplier and delay. Source-level `TD=` and `DELAY=` still require exactly one `LAPLACE(...)` call and conflict with inline delay. Source-level `M=` scales a direct whole-expression Laplace transfer; for mixed current-output expressions it applies to the final behavioral current expression, while mixed voltage-output expressions do not support source-level `M=`.

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

## Practical Modeling Examples

Use `LAPLACE` when the real circuit block is mostly linear and you know the intended gain, bandwidth, pole, zero, or damping. It is useful for fast system-level models before replacing a block with detailed components.

| Real-life block | Useful transfer |
|-----------------|-----------------|
| ADC input anti-alias or signal-conditioning pole | `gain*wc/(s+wc)` |
| Closed-loop amplifier with finite bandwidth | `acl*wp/(s+wp)` |
| Current-output sensor or transconductance stage | `gm*wc/(s+wc)` on a `G` source |
| Damped mechanical, LC, or control plant response | `wn*wn/(s*s + 2*zeta*wn*s + wn*wn)` |
| Lead/lag compensation block | `(1+s/wz)/(1+s/wp)` |

These examples stay within the SpiceSharpParser subset: `E` and `G` sources, voltage input probes, finite DC gain, and proper rational polynomials in `s`.

### ADC Anti-Alias Or Signal-Conditioning Pole

A front-end filter before an ADC is often modeled as a low-pass block. This example gives a gain of 2 and a 10 kHz pole:

```spice
* Signal-conditioning gain with one anti-alias pole
.PARAM gain=2
.PARAM fc=10k
.PARAM wc={2*PI*fc}
VIN IN 0 AC 1
EAAF OUT 0 LAPLACE {V(IN)} = {gain*wc/(s+wc)}
RLOAD OUT 0 10k
.AC DEC 40 10 1MEG
.SAVE V(OUT)
.END
```

At low frequency, `V(OUT)` is about `2*V(IN)`. Near `10 kHz`, the magnitude is about `2/sqrt(2)`.

### Finite-Bandwidth Amplifier Approximation

An op-amp circuit with closed-loop gain `acl` cannot keep that gain forever. A simple one-pole approximation places the closed-loop pole at:

```text
fp = gain_bandwidth / acl
```

```spice
* Closed-loop gain of 20 with a 10 MHz gain-bandwidth approximation
.PARAM acl=20
.PARAM gbw=10MEG
.PARAM fp={gbw/acl}
.PARAM wp={2*PI*fp}
VIN IN 0 AC 1
EAMP OUT 0 LAPLACE {V(IN)} = {acl*wp/(s+wp)}
RLOAD OUT 0 10k
.AC DEC 40 100 100MEG
.SAVE V(OUT)
.END
```

This is not a full op-amp macro-model. It is a compact way to include the dominant bandwidth limit in a larger signal-chain model.

### Current-Output Sensor Front End

A `G` LAPLACE source maps an input voltage to an output current. That makes it useful for transconductance stages or simplified current-output sensor blocks. In this example, `1 V` at `SENSE` represents one unit of measured signal:

```spice
* Low-pass transconductance block into a load resistor
.PARAM gm=100u
.PARAM fc=20k
.PARAM wc={2*PI*fc}
VSENSE SENSE 0 AC 1
GSENSOR OUT 0 LAPLACE {V(SENSE)} = {gm*wc/(s+wc)}
RLOAD OUT 0 10k
.AC DEC 40 100 1MEG
.SAVE V(OUT)
.END
```

At low frequency, the current is approximately `gm*V(SENSE)`. With a grounded load from `OUT` to `0`, the output voltage is negative because the `G` source current direction is from `OUT` to `0`.

### Damped Second-Order Block

Use a second-order block for a simplified LC network, mechanical resonance, or control plant:

```spice
* Damped second-order response at 5 kHz
.PARAM fn=5k
.PARAM wn={2*PI*fn}
.PARAM zeta=0.7
VIN IN 0 AC 1
ERES OUT 0 LAPLACE {V(IN)} = {wn*wn/(s*s + 2*zeta*wn*s + wn*wn)}
RLOAD OUT 0 10k
.AC DEC 50 10 1MEG
.SAVE V(OUT)
.END
```

Lower `zeta` gives more peaking near `fn`; higher `zeta` gives a flatter, more damped response.

### Lead/Lag Control Block

Many control examples use pure integrators such as `1/s`, but SpiceSharpParser rejects those because their DC gain is singular. A finite lead/lag block is supported:

```spice
* Lead block: zero at 1 kHz, pole at 10 kHz
.PARAM fz=1k
.PARAM fp=10k
.PARAM wz={2*PI*fz}
.PARAM wp={2*PI*fp}
VIN ERR 0 AC 1
ELEAD CTRL 0 LAPLACE {V(ERR)} = {(1+s/wz)/(1+s/wp)}
RLOAD CTRL 0 10k
.AC DEC 40 10 1MEG
.SAVE V(CTRL)
.END
```

With `fz < fp`, this block adds phase lead and increases gain between the zero and pole. Swap the pole and zero placement for lag-style behavior.

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

The same spelling variants are supported for `G` sources and for differential inputs such as `{V(INP,INN)}`.
For `F` and `H`, use the same spelling variants with current input, such as `{I(VSENSE)}`.

## Common Mistakes

| Mistake | Why it fails | Use instead |
|---------|--------------|-------------|
| `1/s` | Singular DC gain at `s = 0` | Use a finite DC-gain transfer |
| `s` | Improper transfer: numerator order is greater than denominator order | Use `s/(s+wc)` |
| `sin(s)` | Not a rational polynomial in `s` | Use polynomial/rational expressions only |
| source-level `V(a)-V(b)` | Source-level input shape is unsupported | Use `V(a,b)` |
| `I(Vsense)` on `E` or `G` | `E` and `G` require voltage input | Use `V(node)` or switch to `F`/`H` |
| `V(node)` on `F` or `H` | `F` and `H` require current input | Use `I(Vsense)` or switch to `E`/`G` |
| `M=inf` | The multiplier must be finite | Use a finite constant expression |
| `TD=1n DELAY=2n` | Only one delay option may be used | Use either `TD` or `DELAY` |
| `TD=-1n` | Delay must be non-negative | Use `TD=0` or a positive delay |
| `TD 1n` | Options require assignment syntax | Use `TD=1n` |
| source-level delay with multiple function calls | One source-level delay cannot be shared by several calls | Move `TD=` or `DELAY=` into each `LAPLACE(...)` call |
| `LAPLACE(V(in), H(s), FOO=1)` | Unknown inline option | Use only `M=`, `TD=`, or `DELAY=` |
| nested `LAPLACE(...)` inside a function input | Dynamic Laplace calls cannot be nested | Lower the inner transfer to a separate source |

## Further Reading

These references are useful for the engineering context behind transfer-function models. Some examples use broader PSpice or LTspice syntax than SpiceSharpParser currently supports, so adapt them to the supported source-level rational-polynomial subset described above.

- [Model Transfer Functions by Applying the Laplace Transform in LTspice](https://www.analog.com/en/resources/technical-articles/model-transfer-functions-by-applying-the-laplace-transform-in-ltspice.html)
- [Cadence PSpice User Guide: Analog Behavioral Modeling](https://resources.pcb.cadence.com/pspiceuserguide/06-analog-behavioral-modeling)
- [Filter Basics: Anti-Aliasing](https://www.analog.com/en/resources/technical-articles/guide-to-antialiasing-filter-basics.html)
- [TI Low-Pass Filtered Non-Inverting Amplifier Circuit](https://www.ti.com/tool/CIRCUIT060057)
- [Low Noise Single Supply Photodiode Amplifier](https://www.analog.com/en/resources/technical-articles/low-noise-single-supply-photodiode-amplifier.html)

## Current Limitations

- `E` and `G` require `V(node)` or `V(node1,node2)` input expressions.
- `F` and `H` require `I(source)` input expressions.
- Function-style `LAPLACE(...)` accepts direct probes and arbitrary scalar input expressions by generating helpers when needed.
- Function-style options inside the call support `M=`, `TD=`, and `DELAY=`.
- Explicit internal-state options are not supported yet.
- Transient response is verified for undelayed first-order `E` / `G` low-pass sources. Delayed transient sources and current-controlled `F` / `H` transient sources are not currently claimed beyond runtime support covered by focused tests.
- Transfers must be finite, proper rational polynomials in `s` with non-singular DC gain.
