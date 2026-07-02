# L — Inductor

An inductor stores energy in a magnetic field created by current flowing through a coil.

## Syntax

```
L<name> <node+> <node-> <value> [IC=<initial_current>]
L<name> <node+> <node-> Flux=<expression> [IC=<initial_current>] [M=<m>] [N=<n>]
```

| Parameter | Description |
|-----------|-------------|
| `node+`, `node-` | Positive and negative terminal nodes |
| `value` | Inductance in henries |
| `IC=i` | Initial current through the inductor (for `UIC`) |
| `Flux=expr` | LTspice-style flux-linkage expression; requires `UseCustomComponents()` |

## Examples

```spice
* Basic inductor
L1 IN OUT 10u

* With initial current
L2 A B 1m IC=100m

* Nanohenries
L3 IN OUT 47n

* LTspice-style flux-defined inductor
L4 IN OUT Flux=2m*x IC=10m
```

See [LTspice-Style Nonlinear Passives](nonlinear-passives.md) for
`Flux=<expr>` inductors.

## Mutual Inductance

Inductors can be magnetically coupled using the `K` statement:

```spice
L1 A 0 10u
L2 B 0 10u
K1 L1 L2 0.95
```

See the [K — Mutual Inductance](mutual-inductance.md) article.

## MNA View

An inductor's natural dynamic variable is current, so MNA uses a branch-current
unknown such as `I(L1)`.

| Analysis | Matrix role |
|----------|-------------|
| `.OP` / DC bias | Ideal short branch relation with unknown current. |
| `.AC` | Branch equation using `V = sL I`. |
| `.TRAN` | Branch companion coefficient plus RHS history term. |

In transient analysis, the integration method rewrites:

$$
v = \frac{d\Phi}{dt}
$$

into a temporary branch equation:

$$
v \approx r_{\text{eq}}I(L1) + v_{\text{history}}
$$

The node rows connect the branch current into KCL. The branch row connects node
voltage, branch current, and the known history term.

For the detailed SpiceSharp `Inductors.Time` behavior and examples, see
[Transient Integration Methods](transient-integration-methods.md#built-in-inductor-behavior-stack).
