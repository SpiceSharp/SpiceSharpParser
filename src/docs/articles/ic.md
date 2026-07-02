# .IC Statement

The `.IC` statement sets initial node voltages for transient analysis. These values are used when the `UIC` (Use Initial Conditions) keyword is specified on the `.TRAN` statement.

## Syntax

```
.IC V(<node1>)=<value> [V(<node2>)=<value> ...]
```

| Parameter | Description |
|-----------|-------------|
| `V(<node>)` | Node name wrapped in `V()` |
| `value` | Initial voltage at that node |

## Examples

```spice
.IC V(OUT)=0 V(MID)=2.5

.IC V(1)=5 V(2)=0 V(3)=3.3
```

## MNA View

`.IC` is not a normal device stamp. It changes how transient analysis starts.
With `.TRAN ... UIC`, the listed node voltages seed the initial solution and
the initial history used by dynamic devices connected to those nodes.

For an RC circuit, `.IC V(OUT)=0` means the capacitor voltage starts from
`0 V`. The first transient companion model is then built from that initial
charge state instead of from a DC operating-point result.

Without `UIC`, SpiceSharp computes the DC operating point first. In that case,
`.IC` does not force the final DC MNA solution; it is mainly the `UIC` startup
path that makes `.IC` visible.

See [.TRAN](tran.md#mna-view) and
[Transient Integration Methods](transient-integration-methods.md#worked-example-8-uic-initial-conditions)
for how initial conditions seed transient history.

## Typical Usage

```spice
Capacitor charging
R1 IN OUT 10k
C1 OUT 0 1u
V1 IN 0 10
.IC V(OUT)=0
.TRAN 1e-6 50e-3 UIC
.SAVE V(OUT)
.END
```

## Notes

- `.IC` values are only applied when `UIC` is specified on the `.TRAN` line.
- Without `UIC`, the simulator computes an initial DC operating point instead.
- For initial guesses that assist DC convergence (without `UIC`), use `.NODESET` instead.
- Device-level initial conditions can also be set with the `IC=` parameter on individual components (e.g., `C1 1 0 1u IC=5`).
