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
