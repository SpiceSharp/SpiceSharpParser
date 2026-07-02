# .NODESET Statement

The `.NODESET` statement provides initial guesses for node voltages to help the DC operating point solver converge. Unlike `.IC`, these are suggestions rather than forced values.

## Syntax

```
.NODESET V(<node1>)=<value> [V(<node2>)=<value> ...]
```

| Parameter | Description |
|-----------|-------------|
| `V(<node>)` | Node name wrapped in `V()` |
| `value` | Suggested initial voltage |

## Example

```spice
.NODESET V(OUT)=5 V(MID)=2.5
```

## MNA View

`.NODESET` is a Newton starting hint, not a permanent matrix stamp. It gives the
operating-point solver an initial voltage guess for one or more nodes. After
Newton starts, the normal MNA equations still decide the final voltages.

That distinction matters:

| Statement | MNA effect |
|-----------|------------|
| `.NODESET` | Initial guess for Newton, solver may move away from it. |
| `.IC` with `UIC` | Initial transient state/history seed. |
| Voltage source | Actual branch equation that forces a voltage. |

Use `.NODESET` for convergence help in circuits with multiple possible
operating points, such as latches and bistable transistor circuits.

For the Newton loop, see
[How SpiceSharp Solves Circuits](spicesharp-architecture.md#example-diode-linearization).

## Difference from .IC

| Feature | .IC | .NODESET |
|---------|-----|----------|
| Effect | Forces initial voltage (with `UIC`) | Suggests starting guess for solver |
| Requires `UIC` | Yes | No |
| Use case | Transient initial conditions | DC convergence help |
| Overridden by solver | No | Yes — solver refines from guess |

## Typical Usage

Use `.NODESET` when the DC operating point solver has difficulty converging:

```spice
Bistable circuit
R1 VCC OUT1 10k
R2 VCC OUT2 10k
Q1 OUT1 OUT2 0 NPN_MODEL
Q2 OUT2 OUT1 0 NPN_MODEL
V1 VCC 0 5
.model NPN_MODEL NPN(Is=1e-14 Bf=100)
.NODESET V(OUT1)=5 V(OUT2)=0.2
.OP
.SAVE V(OUT1) V(OUT2)
.END
```
