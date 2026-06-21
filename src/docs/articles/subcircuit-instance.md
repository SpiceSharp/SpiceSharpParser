# X — Subcircuit Instance

The `X` device statement instantiates a subcircuit defined by `.SUBCKT`. It maps external nodes to the subcircuit's interface nodes and optionally overrides default parameters.

## Syntax

```
X<name> <node1> [<node2> ...] <subcircuit_name> [<param1>=<val1> ...]
```

| Parameter | Description |
|-----------|-------------|
| `node1, node2, ...` | External nodes connected to the subcircuit's interface |
| `subcircuit_name` | Name of the `.SUBCKT` definition to instantiate |
| `param=val` | Parameter overrides |

## Examples

```spice
* Basic instantiation
X1 IN OUT VCC 0 my_amp

* With parameter overrides
X2 SIG FILT rc_filter R=10k C=100p

* Multiple instances
X1 A B VCC 0 buffer
X2 C D VCC 0 buffer
```

## MNA View

An `X` instance does not add one special "subcircuit row" to MNA. Instead, the
reader expands the subcircuit's internal devices into the parent circuit with
scoped internal node names.

After expansion, MNA sees ordinary devices:

```text
X1 rc_filter
  -> R inside X1 stamps a conductance
  -> C inside X1 stamps AC or transient dynamic terms
  -> internal node names are scoped to X1
```

So subcircuits are a hierarchy and naming feature before matrix assembly. The
matrix is still assembled from the devices inside each instance.

See [.SUBCKT](subckt.md#mna-view) for the definition-side view.

## Typical Usage

```spice
Subcircuit usage
.SUBCKT inverter IN OUT VDD VSS
M1 OUT IN VDD VDD PMOD L=1u W=4u
M2 OUT IN VSS VSS NMOD L=1u W=2u
.ENDS inverter

.MODEL PMOD PMOS(VTO=-0.7 KP=50u)
.MODEL NMOD NMOS(VTO=0.7 KP=110u)

VDD VDD 0 3.3
VIN IN 0 PULSE(0 3.3 0 1n 1n 10n 20n)

X1 IN OUT VDD 0 inverter

.TRAN 0.1n 40n
.SAVE V(IN) V(OUT)
.END
```

## Notes

- The number of nodes must match the `.SUBCKT` definition's interface node count.
- Internal nodes are automatically scoped — no naming conflicts with other instances.
- See the [.SUBCKT](subckt.md) article for defining subcircuits.
