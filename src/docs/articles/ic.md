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
