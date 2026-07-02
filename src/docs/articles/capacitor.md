# C — Capacitor

A capacitor stores energy in an electric field between two conductors.

## Syntax

```
C<name> <node+> <node-> <value> [IC=<initial_voltage>] [M=<m>]
C<name> <node+> <node-> <model_name> [L=<length>] [W=<width>] [IC=<v>]
C<name> <node+> <node-> VALUE={<expression>}
C<name> <node+> <node-> Q=<expression> [IC=<initial_voltage>] [M=<m>] [N=<n>]
```

| Parameter | Description |
|-----------|-------------|
| `node+`, `node-` | Positive and negative terminal nodes |
| `value` | Capacitance in farads |
| `IC=v` | Initial voltage across the capacitor (for `UIC`) |
| `TC=tc1[,tc2]` | Temperature coefficients |
| `M=m` | Multiplier |
| `model_name` | Reference to a `.MODEL` definition |
| `VALUE={expr}` | Behavioral expression |
| `Q=expr` | LTspice-style charge expression; requires `UseCustomComponents()` |

## Examples

```spice
* Basic capacitor
C1 OUT 0 1u

* With initial condition
C2 OUT 0 100n IC=5

* Picofarads
C3 A B 10p

* Model-based
C4 IN OUT cmod L=10u W=1u IC=1

* Behavioral
C5 IN OUT VALUE={M*N*1p}

* LTspice-style charge-defined capacitor
C6 OUT 0 Q=1u*x
```

See [LTspice-Style Nonlinear Passives](nonlinear-passives.md) for `Q=<expr>`
capacitors.

## MNA View

A capacitor is dynamic, so its matrix contribution depends on the analysis:

| Analysis | Matrix role |
|----------|-------------|
| `.OP` / DC bias | Ideal open circuit, except initial/history setup. |
| `.AC` | Complex admittance stamp `Y = sC`. |
| `.TRAN` | Companion conductance plus RHS history current. |

In transient analysis, the integration method rewrites:

$$
i = \frac{dQ}{dt}
$$

into a temporary algebraic companion model:

$$
i \approx g_{\text{eq}}V + i_{\text{history}}
$$

The `g_eq` part is stamped into the MNA matrix like a conductance. The history
current is stamped into the RHS. The capacitor commits new charge history only
after a timestep is accepted.

For the detailed SpiceSharp `Capacitors.Time` behavior and examples, see
[Transient Integration Methods](transient-integration-methods.md#built-in-capacitor-behavior-stack).

## Model Definition

```spice
.MODEL cmod C(CJ=1e-12)
```
