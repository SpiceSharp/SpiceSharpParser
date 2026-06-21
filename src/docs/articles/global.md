# .GLOBAL Statement

The `.GLOBAL` statement declares nodes as global, making them visible across subcircuit boundaries. Normally, nodes inside a subcircuit are local and isolated from the parent circuit.

## Syntax

```
.GLOBAL <node1> [<node2> ...]
```

## Examples

```spice
* Make power rails global
.GLOBAL VCC GND VDD VSS
```

## MNA View

`.GLOBAL` affects node identity before MNA assembly. It does not add a stamp by
itself.

Without `.GLOBAL`, a node named `VCC` inside a subcircuit is scoped to that
subcircuit instance. With `.GLOBAL VCC`, all references to `VCC` point to the
same MNA node unknown. That can connect power rails, clocks, or references
across otherwise isolated subcircuit scopes.

Ground node `0` is already global and is not included as an unknown in the MNA
solution vector.

## Typical Usage

```spice
Global power rails
.GLOBAL VCC

V1 VCC 0 5

.SUBCKT amp IN OUT
R1 IN BASE 10k
Q1 VCC BASE OUT NPN_MODEL
.ENDS amp

X1 SIG OUTPUT amp
.model NPN_MODEL NPN(Is=1e-14 Bf=100)
.OP
.SAVE V(OUTPUT)
.END
```

Without `.GLOBAL VCC`, the `VCC` node inside the subcircuit would be a separate local node. With `.GLOBAL`, it refers to the same node as the top-level `VCC`.

## Notes

- Ground (node `0`) is always global — it does not need to be declared.
- Global nodes are useful for power supply rails, clocks, resets, and other signals shared across the entire design.
