# LTspice-Style Nonlinear Passives

`SpiceSharpParser.CustomComponents` contains opt-in support for LTspice-style
charge-defined capacitors and flux-defined inductors. These forms do not give a
constant `C` or `L` value. Instead, they describe the stored quantity directly:

```spice
C<name> <node+> <node-> Q=<expression> [IC=<initial_voltage>] [M=<m>] [N=<n>]
L<name> <node+> <node-> Flux=<expression> [IC=<initial_current>] [M=<m>] [N=<n>]
```

Use these forms when an LTspice netlist models a nonlinear passive by saying how
much charge or flux linkage it stores at the present terminal state.

## How It Works

During reading, `UseCustomComponents()` replaces the normal `C` and `L`
component generators with custom-aware generators. The replacement is narrow:
ordinary numeric capacitors and inductors still use the built-in SpiceSharp
components, while only `Q=` capacitors and `Flux=` inductors take the custom
path.

The expression is treated as the component's stored state:

| Component | Stored state | Independent variable $x$ | Runtime relation |
|-----------|--------------|--------------------------|------------------|
| Capacitor | Charge $Q$ | Terminal voltage $V(node+, node-)$ | $i = \frac{dQ}{dt}$ |
| Inductor | Flux linkage $\Phi$ | Branch current from `node+` to `node-` | $v = \frac{d\Phi}{dt}$ |

The simulator evaluates the expression at the current operating point and also
uses its local derivative. In transient analysis, that derivative supplies the
incremental contribution used by the solver around the present state:

$$
C_{\text{inc}} = \frac{dQ}{dV}
$$

$$
L_{\text{inc}} = \frac{d\Phi}{dI}
$$

So a linear expression is equivalent to a normal passive:

```spice
C1 out 0 Q=1u*x      ; behaves like a 1 uF capacitor
L1 in out Flux=2m*x  ; behaves like a 2 mH inductor
```

A nonlinear expression changes the incremental value as the voltage or current
changes. For example, `Flux=1m*tanh(x)` stores flux as a nonlinear function of
current, and the small-signal inductance is the slope of that curve at the
present current.

For a step-by-step explanation of how `dQ/dV`, `dPhi/dI`, `dQ/dt`, and
`dPhi/dt` become transient matrix and RHS contributions, see
[Transient Integration Methods](transient-integration-methods.md).

## MNA View

The custom nonlinear passives still participate in the same MNA system as normal
components. The difference is that their stamp is built from a stored quantity
and a local derivative.

For `Q=` capacitors:

```text
evaluate Q(V)
evaluate dQ/dV
use integration history to compute dQ/dt
stamp Jacobian coefficient into the node matrix
stamp history/correction current into RHS
```

For `Flux=` inductors:

```text
create branch-current unknown I(L)
evaluate Phi(I)
evaluate dPhi/dI
use integration history to compute dPhi/dt
stamp branch-equation coefficient and RHS history term
```

So `dQ/dV` and `dPhi/dI` are not exported only for observation. They are the
local slopes used by the MNA Jacobian at the current Newton guess.

For the general matrix assembly algorithm, see
[How SpiceSharp Solves Circuits](spicesharp-architecture.md#modified-matrix-algorithm-step-by-step).

## Enable Parser Mappings

The custom mappings are not enabled by default. Reference the custom component
assembly, then enable the mappings before calling `Read()`:

```csharp
using System;
using SpiceSharpParser;
using SpiceSharpParser.CustomComponents;

var netlist = string.Join(Environment.NewLine,
    "Nonlinear passive example",
    "V1 in 0 10",
    "R1 in out 10k",
    "C1 out 0 Q=1u*x",
    ".tran 1e-8 10e-6",
    ".end");

var parser = new SpiceNetlistParser();
parser.Settings.Lexing.HasTitle = true;
parser.Settings.Parsing.IsEndRequired = true;
var parseResult = parser.ParseNetlist(netlist);

var reader = new SpiceSharpReader();
reader.Settings.UseCustomComponents();

var spiceModel = reader.Read(parseResult.FinalModel);
```

`UseCustomComponents()` installs the custom-aware `C` and `L` generators used by
the article examples. Ordinary constant-valued capacitors and inductors still
fall back to the built-in SpiceSharp components.

Without `UseCustomComponents()`, the core LTspice compatibility reader reports
`Q=` and `Flux=` as unsupported charge/flux-defined passive syntax.

## Expression Variable

Both forms use LTspice's `x` variable convention, but `x` means a different
terminal quantity for each component. Capacitor expressions are voltage-based:
`x` is `V(node+, node-)`. Inductor expressions are current-based: `x` is the
branch current from `node+` to `node-`.

## Capacitor Details

The nonlinear capacitor is implemented in the custom component package. The
expression represents total charge as a function of terminal voltage:

```spice
C1 out 0 Q=1u*x
```

Transient analysis does not use the expression as a capacitance value. It first
evaluates charge from the present voltage, then differentiates that charge over
time:

$$
i = \frac{dQ(V)}{dt}
$$

For nonlinear expressions, the effective capacitance around the present voltage
is the local slope of the charge curve:

$$
C_{\text{inc}} = \frac{dQ}{dV}
$$

For DC biasing, the capacitor behaves as an open circuit, like an ordinary ideal
capacitor. For AC analysis, the component uses the operating-point incremental
capacitance:

$$
C_{\text{inc}} = \frac{dQ}{dV}
$$

$$
Y_{\text{ac}} = s C_{\text{inc}}
$$

The `IC=` parameter sets the initial terminal voltage when transient analysis
runs with `UIC`. The initial charge state is computed from that voltage.

The component exposes property exports for voltage, current, charge,
incremental capacitance, power, and `dqdt`.

## Inductor Details

The nonlinear inductor is implemented in the custom component package. The
expression represents total flux linkage as a function of branch current:

```spice
L1 in out Flux=2m*x IC=10m
```

The inductor creates a branch current variable. During transient analysis, it
evaluates flux from that branch current and asks the active integration method
for the derivative contribution. The branch equation is:

$$
v = \frac{d\Phi(I)}{dt}
$$

For DC biasing, the inductor behaves as a voltage-short branch, like an ordinary
ideal inductor. For AC analysis, the component uses the operating-point
incremental inductance:

$$
L_{\text{inc}} = \frac{d\Phi}{dI}
$$

$$
Z_{\text{ac}} = s L_{\text{inc}}
$$

The `IC=` parameter sets the initial branch current when transient analysis runs
with `UIC`. The initial flux state is computed from that current.

The component exposes property exports for voltage, current, flux, incremental
inductance, power, and `dfluxdt`.

## Scaling

`M` represents parallel cells and `N` represents series cells. The expression is
written for one cell; the generated component scales the stored quantity to the
whole instance.

For capacitors, the component multiplies the single-cell charge by `M` and
divides it by `N`:

$$
Q_{\text{total}}(V) = \frac{M}{N} Q_{\text{cell}}(V)
$$

For inductors, branch current is split across parallel cells and flux linkage is
accumulated across series cells:

$$
\Phi_{\text{total}}(I) = N \Phi_{\text{cell}}\left(\frac{I}{M}\right)
$$

$$
L_{\text{inc,total}}(I) = \frac{N}{M} \frac{d\Phi_{\text{cell}}}{dI}
$$

## Limitations

- `UseCustomComponents()` is required.
- Expression functions must be supported by the parser and `SpiceSharpBehavioral`.
- Unsupported trailing parameters on `Q=` / `Flux=` instances produce validation
  errors instead of being ignored.

## Related Articles

- [Transient Integration Methods](transient-integration-methods.md)
- [.TRAN](tran.md)
- [.OPTIONS](options.md)
