# LTspice-Style Ideal Diode

`SpiceSharpParser.CustomComponents` contains an opt-in ideal diode component for LTspice-style diode models that use parameters such as `Ron`, `Roff`, and `Vfwd`.

This component is separate from the built-in SpiceSharp diode. The built-in diode uses the normal semiconductor exponential model. The custom ideal diode uses a piecewise linear current law. It is useful for power electronics, protection clamps, rectifiers, and behavioral-level circuits where a simple on/off diode is more useful than a detailed PN junction model.

## When To Use It

Use this component when a netlist contains LTspice ideal diode parameters:

```spice
.model did D(Ron=0.1 Roff=1e9 Vfwd=0.7)
D1 out 0 did
```

Use the built-in diode when the model is a semiconductor model:

```spice
.model d4148 D(Is=2.52e-9 Rs=0.568 N=1.752 Cjo=4e-12 M=0.4 Tt=20e-9)
D1 out 0 d4148
```

The ideal diode is faster and simpler, but it does not include junction capacitance, charge storage, temperature-dependent semiconductor equations, or noise.

## Installation

Reference the custom component assembly in the application that reads the netlist:

```xml
<ProjectReference Include="..\SpiceSharpParser.CustomComponents\SpiceSharpParser.CustomComponents.csproj" />
```

When the package is published, use the package reference instead:

```bash
dotnet add package SpiceSharpParser.CustomComponents
```

## Enable Parser Mappings

The parser does not enable custom components by default. Enable them on the reader settings before calling `Read()`:

```csharp
using System;
using SpiceSharpParser;
using SpiceSharpParser.CustomComponents;

var netlist = string.Join(Environment.NewLine,
    "Ideal diode example",
    "V1 in 0 3",
    "D1 in 0 did",
    ".model did D(Ron=2 Roff=1e9 Vfwd=1)",
    ".op",
    ".end");

var parser = new SpiceNetlistParser();
parser.Settings.Lexing.HasTitle = true;
parser.Settings.Parsing.IsEndRequired = true;
var parseResult = parser.ParseNetlist(netlist);

var reader = new SpiceSharpReader();
reader.Settings.UseCustomComponents();

var spiceModel = reader.Read(parseResult.FinalModel);
```

`UseCustomComponents()` replaces the parser mappings for diode models and diode instances with custom-aware generators. Ordinary diode models still fall back to the built-in SpiceSharp diode.

Without `UseCustomComponents()`, the core parser does not know how to map LTspice ideal diode parameters to a SpiceSharp entity.

## Netlist Syntax

The instance syntax is the normal diode syntax:

```spice
D<name> <anode> <cathode> <model_name> [<area>] [ON|OFF] [M=<m>] [N=<n>]
```

The custom component is selected when the referenced `.MODEL D(...)` contains at least one ideal diode model parameter:

```spice
.model did D(Ron=2 Roff=1e9 Vfwd=1)
D1 in 0 did
```

A classic model remains a classic diode:

```spice
.model regular D(Is=1e-12 N=1)
D1 in 0 regular
```

`Rs` alone does not select the ideal diode, because `Rs` is also a classic diode model parameter. At least one of `Ron`, `Roff`, `Vfwd`, `Vrev`, `Rrev`, `Ilimit`, `RevIlimit`, `Epsilon`, or `RevEpsilon` must be present on the model.

If multiple suffixed models share a base name, instance `L=` and `W=` values are used with model `Lmin`, `Lmax`, `Wmin`, and `Wmax` selection parameters.

## Parameters

### Model Parameters

| Parameter | Description | Default |
|-----------|-------------|---------|
| `Ron` | Forward on resistance | 1 ohm |
| `Roff` | Off resistance | simulation `Gmin` if omitted |
| `Vfwd` | Forward threshold voltage | 0 V |
| `Vrev` | Reverse breakdown voltage magnitude | not enabled |
| `Rrev` | Reverse breakdown resistance | `Ron` if omitted |
| `Ilimit` | Forward current limit | not enabled |
| `RevIlimit` | Reverse current limit | not enabled |
| `Epsilon` | Forward transition smoothing width | 0 V |
| `RevEpsilon` | Reverse transition smoothing width | 0 V |
| `Rs` | Parasitic series resistance | 0 ohm |

### Instance Parameters

| Parameter | Description | Default |
|-----------|-------------|---------|
| `area` | Positive area multiplier | 1 |
| `M` | Positive parallel multiplier | 1 |
| `N` | Positive series multiplier | 1 |
| `ON` / `OFF` | Initial state hint | on |
| `Ron`, `Roff`, `Vfwd`, `Vrev`, `Rrev`, `Ilimit`, `RevIlimit`, `Epsilon`, `RevEpsilon`, `Rs` | Instance override for the same model parameter | model value |

For the ideal diode, `N` is a series multiplier. This differs from the classic diode model, where model parameter `N` is the emission coefficient.

## Parameter Scaling

The model is evaluated as one ideal diode cell. Instance multipliers then scale the result:

| Parameter | Effect |
|-----------|--------|
| `area` | Multiplies the local diode current and conductance. |
| `M` | Represents parallel diode cells. Current and conductance scale by `M`. |
| `N` | Represents series diode cells. Total voltage is divided by `N` before evaluating one cell. |
| `Rs` | Adds a series resistance between the external anode and the internal ideal diode anode. |

`area`, `M`, and `N` must be greater than zero. This avoids singular or poorly conditioned internal branch topologies, especially when `Rs` is nonzero.

For a forward-biased diode without smoothing or current limiting:

$$
\begin{aligned}
v_{\text{local}} &= \frac{V(\text{anode}, \text{cathode})}{N} \\
i_{\text{local}} &= \text{area} \cdot \frac{v_{\text{local}} - V_{\text{fwd}}}{R_{\text{on}}} \\
I_{\text{total}} &= M \cdot i_{\text{local}}
\end{aligned}
$$

So the approximate total forward threshold is:

$$
V_{\text{total}} \approx N \cdot V_{\text{fwd}}
$$

and the approximate effective forward resistance is:

$$
R_{\text{effective}} \approx \frac{R_{\text{on}} \cdot N}{\text{area} \cdot M}
$$

If `Rs` is used, it adds another effective series resistance:

$$
R_{s,\text{effective}} \approx \frac{R_s \cdot N}{\text{area} \cdot M}
$$

This matches the intended interpretation: more parallel cells carry more current, and more series cells need more voltage.

## Current And Voltage Sign

The anode is the first diode node and the cathode is the second diode node:

```spice
D1 anode cathode did
```

Positive diode voltage is:

$$
V(\text{anode}, \text{cathode})
$$

Positive diode current flows from anode to cathode. Property exports follow that sign convention:

```spice
.save @D1[v] @D1[vj] @D1[i] @D1[gd] @D1[p]
```

| Export | Meaning |
|--------|---------|
| `@D1[v]` or `@D1[vd]` | Terminal diode voltage, including any `Rs` drop |
| `@D1[vj]` or `@D1[vdiode]` | Internal ideal-diode voltage, excluding any `Rs` drop |
| `@D1[i]`, `@D1[id]`, or `@D1[c]` | Diode current |
| `@D1[gd]` | Terminal small-signal conductance at the operating point |
| `@D1[p]` or `@D1[pd]` | Terminal diode branch power |

When the diode is reverse biased, `@D1[i]` is usually negative.

## Current Law

The ideal diode current law is evaluated for one diode cell first. Instance
scaling is applied after that. Define:

$$
\begin{aligned}
v &= \text{voltage across one series cell} \\
V_f &=
\begin{cases}
V_{\text{fwd}}, & \text{if } V_{\text{fwd}} \text{ is given} \\
0, & \text{otherwise}
\end{cases} \\
g_{\text{on}} &= \frac{1}{R_{\text{on}}} \\
g_{\text{off}} &=
\begin{cases}
\frac{1}{R_{\text{off}}}, & \text{if } R_{\text{off}} \text{ is given} \\
G_{\text{min}}, & \text{otherwise}
\end{cases}
\end{aligned}
$$

The forward, off, and reverse-breakdown lines are:

$$
\begin{aligned}
i_{\text{on}}(v) &= g_{\text{on}} \cdot (v - V_f) \\
i_{\text{off}}(v) &= g_{\text{off}} \cdot v \\
i_{\text{rev}}(v) &= g_{\text{rev}} \cdot (v + V_{\text{rev}})
\end{aligned}
$$

where `i_rev(v)` is used only when `Vrev` is given, and:

$$
g_{\text{rev}} = \frac{1}{R_{\text{rev}}}
$$

If `Rrev` is omitted, `Ron` is used for `Rrev`.

Without smoothing or current limiting, the raw current is the line selected by
the present voltage:

$$
i_{\text{raw}}(v) =
\begin{cases}
i_{\text{rev}}(v), & \text{if } V_{\text{rev}} \text{ is enabled and } v < v_{\text{rev,boundary}} \\
i_{\text{off}}(v), & \text{if } v < v_{\text{fwd,boundary}} \\
i_{\text{on}}(v), & \text{otherwise}
\end{cases}
$$

The small-signal conductance is the derivative of the selected line:

$$
g_{d,\text{raw}} = \frac{d i_{\text{raw}}}{d v}
$$

So the unsmoothed conductance is usually one of:

$$
g_{d,\text{raw}} \in \{g_{\text{rev}}, g_{\text{off}}, g_{\text{on}}\}
$$

The forward transition point is the intersection of the off line and the on
line:

$$
v_{\text{fwd,boundary}} = \frac{g_{\text{on}} \cdot V_f}{g_{\text{on}} - g_{\text{off}}}
$$

When `Roff` is much larger than `Ron`, `g_off` is tiny compared with `g_on`,
so this is very close to:

$$
v_{\text{fwd,boundary}} \approx V_{\text{fwd}}
$$

### Reverse Breakdown

Reverse breakdown is enabled by `Vrev`. `Vrev` is a magnitude, so this model starts reverse breakdown near:

$$
v = -V_{\text{rev}}
$$

The reverse breakdown region is approximately:

$$
i \approx \frac{v + V_{\text{rev}}}{R_{\text{rev}}}
$$

The exact reverse transition point is the intersection of the reverse line and
the off line:

$$
v_{\text{rev,boundary}} = -\frac{g_{\text{rev}} \cdot V_{\text{rev}}}{g_{\text{rev}} - g_{\text{off}}}
$$

When `Roff` is much larger than `Rrev`, this is very close to:

$$
v_{\text{rev,boundary}} \approx -V_{\text{rev}}
$$

Example:

```spice
.model clamp D(Ron=2 Roff=1e9 Vfwd=1 Vrev=2 Rrev=4)
```

At `v = -6 V`, the approximate reverse current is:

$$
i \approx \frac{-6 + 2}{4} = -1\,\text{A}
$$

### Current Limiting

`Ilimit` and `RevIlimit` smoothly limit forward and reverse current. The limiter uses a `tanh` shape, so it approaches the limit gradually instead of clipping with a hard corner.

Forward current limit:

```spice
.model did D(Ron=0.1 Roff=1e9 Vfwd=0.7 Ilimit=10)
```

Reverse current limit:

```spice
.model did D(Ron=0.1 Roff=1e9 Vfwd=0.7 Vrev=20 RevIlimit=2)
```

The smooth limiter also reduces the small-signal conductance as the current approaches the limit.

Let `i0` and `gd0` be the current and conductance after the piecewise line and
optional smoothing, but before current limiting. The forward limiter uses this
shape:

$$
\begin{aligned}
i_{\text{limited}} &= I_{\text{limit}} \cdot \tanh\left(\frac{i_0}{I_{\text{limit}}}\right) \\
g_{d,\text{limited}} &= g_{d0} \cdot \left(1 - \tanh^2\left(\frac{i_0}{I_{\text{limit}}}\right)\right)
\end{aligned}
$$

The reverse limiter uses the same equation with `RevIlimit`. Because reverse
current is negative, $\tanh(i_0 / I_{\text{rev-limit}})$ is also negative.

### Transition Smoothing

`Epsilon` and `RevEpsilon` smooth the forward and reverse transitions. A value of zero gives a sharp piecewise transition. A positive value ramps the slope over a voltage window around the transition.

```spice
.model did D(Ron=0.1 Roff=1e9 Vfwd=0.7 Epsilon=10m)
```

For any smoothed transition, let the left line be:

$$
i_{\text{left}}(v) = a_{\text{left}} \cdot v + b_{\text{left}}
$$

and the right line be:

$$
i_{\text{right}}(v) = a_{\text{right}} \cdot v + b_{\text{right}}
$$

For the forward transition, the left line is `i_off(v)` and the right line is
`i_on(v)`. For the reverse transition, the left line is `i_rev(v)` and the
right line is `i_off(v)`.

With smoothing width `e`, the smoothing window is:

$$
\begin{aligned}
v_{\text{start}} &= v_{\text{boundary}} - \frac{e}{2} \\
v_{\text{end}} &= v_{\text{boundary}} + \frac{e}{2}
\end{aligned}
$$

Inside that window:

$$
\begin{aligned}
d &= v - v_{\text{start}} \\
i(v) &= i_{\text{left}}(v_{\text{start}})
    + a_{\text{left}} \cdot d
    + \frac{(a_{\text{right}} - a_{\text{left}}) \cdot d^2}{2e} \\
g_d(v) &= a_{\text{left}} + \frac{(a_{\text{right}} - a_{\text{left}}) \cdot d}{e}
\end{aligned}
$$

Outside the window, the model uses the normal left or right line.

Use smoothing when a hard corner causes convergence problems in DC or transient operating-point iterations.

## How The Parser Selects The Component

The parser bridge has two pieces:

| Class | Role |
|-------|------|
| `IdealDiodeModelGenerator` | Reads `.MODEL ... D(...)`. If the model has ideal diode parameters, it creates an `IdealDiodeModel`; otherwise it delegates to the built-in diode model generator. |
| `IdealDiodeGenerator` | Reads `D...` instances. If the referenced model is an `IdealDiodeModel`, it creates an `IdealDiode`; otherwise it delegates to the built-in diode generator. |

The flow is:

```text
reader.Settings.UseCustomComponents()
  -> replace D model generator
  -> replace D component generator

.model did D(Ron=...)
  -> IdealDiodeModelGenerator detects Ron
  -> creates IdealDiodeModel
  -> stores model parameters

D1 in 0 did
  -> IdealDiodeGenerator resolves model did
  -> sees IdealDiodeModel
  -> creates IdealDiode
  -> binds live model parameters
  -> applies instance overrides
```

Classic diode models are delegated back to the existing parser behavior:

```text
.model regular D(Is=...)
  -> no ideal diode parameter
  -> built-in DiodeModel
  -> built-in Diode
```

## How The SpiceSharp Component Works

`IdealDiode` is a normal two-pin SpiceSharp component:

```text
pin 0 = anode
pin 1 = cathode
```

During simulation setup, it creates behavior objects:

| Behavior | Used by | Purpose |
|----------|---------|---------|
| `Biasing` | `.OP`, `.DC`, and operating-point parts of transient | Evaluates diode current and conductance, then stamps the real-valued matrix. |
| `Frequency` | `.AC` | Uses the operating-point conductance for small-signal AC. |

The component is memoryless. It does not add dynamic state for charge or capacitance.

### Biasing Stamp

At each biasing load, the behavior:

1. Reads the present internal diode voltage.
2. Divides it by `N` to get the voltage across one series cell.
3. Evaluates current `i` and local conductance `gd`.
4. Scales by `area` and `M`.
5. Stamps the internal diode conductance into the matrix.
6. Stamps the equivalent current into the right-hand side.
7. Stamps the series branch equation between the external and internal anodes.

The local linear form is:

$$
\begin{aligned}
i &\approx g_d \cdot v + i_{\text{eq}} \\
i_{\text{eq}} &= i - g_d \cdot v
\end{aligned}
$$

The conductance stamp is resistor-like:

| Matrix entry | Added value |
|--------------|-------------|
| `Y[internal_anode, internal_anode]` | `+gd` |
| `Y[cathode, cathode]` | `+gd` |
| `Y[internal_anode, cathode]` | `-gd` |
| `Y[cathode, internal_anode]` | `-gd` |

The right-hand side receives the equivalent current source contribution.

### Series Resistance

The component always creates a private internal anode and a series branch:

```text
external anode -- Rs -- internal anode -- ideal diode -- cathode
```

That fixed topology lets `Rs` be stepped across zero. When `Rs=0`, the series branch is an exact zero-volt constraint rather than an artificial small resistor. When `Rs` is greater than zero, the same branch equation represents the parasitic resistance without requiring the parser to synthesize an extra resistor component.

The standard `v`, `gd`, and `p` exports are terminal quantities. Use `vj` when
you need the internal ideal-diode voltage without the `Rs` drop.

### Convergence Check

The biasing behavior implements a convergence check using the diode current predicted from the previous local linearization:

$$
i_{\text{predicted}} = i_{\text{old}} + g_{d,\text{old}} \cdot \Delta v
$$

If the predicted current differs from the actual loaded current by more than the simulation tolerances, the iteration is marked as not converged and SpiceSharp continues iterating.

### AC Behavior

AC analysis uses the operating-point conductance. The AC stamp uses the
internal diode conductance and optional `Rs`, while property exports report
terminal voltage, terminal current, and terminal complex power:

$$
y_{\text{ac}} = g_d
$$

There is no frequency-dependent capacitance in this model. That means the AC response of the diode itself is resistive and frequency independent. Any frequency response must come from surrounding capacitors, inductors, sources, or other dynamic components.

## Direct Code Usage

You can also create the component directly without parsing a netlist:

```csharp
using SpiceSharp;
using SpiceSharp.Components;
using SpiceSharp.Simulations;
using SpiceSharpParser.CustomComponents;

var diode = new IdealDiode("D1", "in", "0");
diode.Parameters.OnResistance = 2.0;
diode.Parameters.OffResistance = 1e9;
diode.Parameters.ForwardVoltage = 1.0;

var circuit = new Circuit(
    new VoltageSource("V1", "in", "0", 3.0),
    diode);

var op = new OP("op");
var current = new RealPropertyExport(op, "D1", "i");

foreach (int _ in op.Run(circuit))
{
    Console.WriteLine(current.Value);
}
```

This prints approximately `1 A`, because:

$$
\frac{3\,\text{V} - 1\,\text{V}}{2\,\Omega} = 1\,\text{A}
$$

## Worked Examples

### Forward Conduction

```spice
Forward ideal diode
V1 in 0 3
D1 in 0 did
.model did D(Ron=2 Roff=1e9 Vfwd=1)
.op
.save @D1[i] @D1[v]
.end
```

The expected current is approximately:

$$
i \approx \frac{3 - 1}{2} = 1\,\text{A}
$$

### Off Leakage

```spice
Off leakage
V1 in 0 0.5
D1 in 0 did
.model did D(Ron=2 Roff=1e9 Vfwd=1)
.op
.save @D1[i]
.end
```

The diode is below its forward region. The expected current is approximately:

$$
i \approx \frac{0.5}{10^9} = 0.5\,\text{nA}
$$

### Reverse Clamp

```spice
Reverse clamp
V1 in 0 -6
D1 in 0 clamp
.model clamp D(Ron=2 Roff=1e9 Vfwd=1 Vrev=2 Rrev=4)
.op
.save @D1[i]
.end
```

The expected reverse current is approximately:

$$
i \approx \frac{-6 + 2}{4} = -1\,\text{A}
$$

### Series And Parallel Multipliers

```spice
Series and parallel ideal diode
V1 in 0 3
D1 in 0 did M=2 N=2
.model did D(Ron=2 Roff=1e9 Vfwd=1)
.op
.save @D1[i]
.end
```

The local diode voltage is:

$$
v_{\text{local}} = \frac{3}{2} = 1.5\,\text{V}
$$

The local current through one diode cell is:

$$
i_{\text{local}} = \frac{1.5 - 1}{2} = 0.25\,\text{A}
$$

With `M=2`, total current is:

$$
I_{\text{total}} = 2 \cdot 0.25 = 0.5\,\text{A}
$$

### Current-Limited Clamp

```spice
Current-limited clamp
V1 in 0 5
R1 in out 10
D1 out 0 did
.model did D(Ron=0.1 Roff=1e9 Vfwd=0.7 Ilimit=10 Epsilon=10m)
.op
.save V(out) @D1[i] @D1[gd]
.end
```

The 10 ohm resistor keeps the operating current far below the 10 A limit, so
the limiter is almost inactive in this operating point. Once the diode is above
the smoothed transition region, the approximate equations are:

$$
\begin{aligned}
I &\approx \frac{5 - V(\text{out})}{10} \\
I &\approx \frac{V(\text{out}) - 0.7}{0.1}
\end{aligned}
$$

Solving them gives:

$$
\begin{aligned}
I &\approx 0.426\,\text{A} \\
V(\text{out}) &\approx 0.743\,\text{V}
\end{aligned}
$$

If the surrounding circuit tried to drive much more current, the forward
limiter would use:

$$
I = 10 \cdot \tanh\left(\frac{(v - 0.7) / 0.1}{10}\right)
$$

## Unsupported Or Ignored Parameters

When an ideal diode model is selected, the custom parser keeps the ideal-diode parameters and ignores classic diode model parameters such as:

```text
Is, Tnom, N, Tt, Cjo, Cj0, Vj, M, Eg, Xti, Fc, BV, IBV, Kf, Af
```

The instance parameters `temp` and `ic` are ignored for the ideal diode. Unsupported ideal diode instance parameters produce validation errors.

Metadata-style LTspice parameters such as `mfg`, `pn`, `description`, and ratings such as `irms` or `ipk` are ignored.

## Limitations

- The component is memoryless: it does not model junction capacitance or charge storage.
- It does not provide noise behavior.
- It does not model temperature-dependent semiconductor behavior.
- Classic diode parameters such as `Is`, `Cjo`, `Tt`, and `BV` are ignored when an ideal diode model is selected.
- `UseCustomComponents()` is required. Without it, the core parser treats LTspice ideal diode parameters as unsupported in LTspice compatibility mode.

## Troubleshooting

| Symptom | Likely cause | Fix |
|---------|--------------|-----|
| `Ron` or `Vfwd` is reported as unsupported | Custom mappings are not enabled | Call `reader.Settings.UseCustomComponents()` before `Read()`. |
| A classic diode unexpectedly behaves like an ideal diode | The model contains at least one ideal diode parameter | Remove `Ron`, `Roff`, `Vfwd`, `Vrev`, `Rrev`, `Ilimit`, `RevIlimit`, `Epsilon`, or `RevEpsilon` from the model. |
| `N` does not behave like emission coefficient | In the ideal diode, `N` is the series multiplier | Use a classic diode model for emission-coefficient behavior. |
| AC result has no diode capacitance effect | The ideal diode has no capacitance model | Add explicit capacitors or use a classic diode model. |
| DC or transient convergence is rough near switching | The transition is too sharp | Add `Epsilon` or `RevEpsilon`, or reduce extreme `Ron`/`Roff` ratios. |

## Complete Example

```spice
Ideal diode clamp
V1 in 0 5
R1 in out 10
D1 out 0 did
.model did D(Ron=0.1 Roff=1e9 Vfwd=0.7 Ilimit=10)
.op
.save V(out) @D1[i]
.end
```

This uses the same current law:

$$
\begin{aligned}
g_{\text{on}} &= \frac{1}{0.1} = 10\,\text{S} \\
g_{\text{off}} &= \frac{1}{10^9} = 1\,\text{nS} \\
i_{\text{on}}(v) &= 10 \cdot (v - 0.7) \\
i_{\text{off}}(v) &= 10^{-9} \cdot v
\end{aligned}
$$

Because `Roff` is so large, the forward boundary is approximately:

$$
v_{\text{fwd,boundary}} \approx 0.7\,\text{V}
$$

With the 10 ohm source resistor, the operating point is approximately:

$$
\begin{aligned}
I &\approx \frac{5 - V(\text{out})}{10} \\
I &\approx \frac{V(\text{out}) - 0.7}{0.1} \\
I &\approx 0.426\,\text{A} \\
V(\text{out}) &\approx 0.743\,\text{V}
\end{aligned}
$$

The `Ilimit=10` parameter is still part of the model. It matters when the
surrounding circuit tries to push the pre-limit forward current toward 10 A:

$$
i_{\text{limited}} = 10 \cdot \tanh\left(\frac{i_0}{10}\right)
$$
