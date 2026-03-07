# .SUBCKT Statement

The `.SUBCKT` statement defines a reusable subcircuit block. Subcircuits encapsulate a group of components and can be instantiated multiple times using the `X` device statement.

## Syntax

```
.SUBCKT <name> <node1> [<node2> ...] [PARAMS: <p1>=<v1> ...]
...circuit statements...
.ENDS [<name>]
```

| Parameter | Description |
|-----------|-------------|
| `name` | Subcircuit name |
| `node1, node2, ...` | External interface nodes |
| `PARAMS:` | Optional default parameter values |

## Examples

### Basic Subcircuit

```spice
.SUBCKT inverter IN OUT VDD VSS
M1 OUT IN VDD VDD pmos L=0.5u W=2u
M2 OUT IN VSS VSS nmos L=0.5u W=1u
.ENDS inverter
```

### Parameterized Subcircuit

```spice
.SUBCKT filter IN OUT PARAMS: R=1k C=1n
R1 IN OUT {R}
C1 OUT 0 {C}
.ENDS filter
```

### Instantiation

```spice
X1 A B VCC 0 inverter
X2 SIG FILT filter R=10k C=100p
```

## Nested Subcircuits

Subcircuits can contain other subcircuit definitions and instantiations:

```spice
.SUBCKT buffer IN OUT VDD VSS
X1 IN MID VDD VSS inverter
X2 MID OUT VDD VSS inverter
.ENDS buffer
```

## Parameter Override

Default parameters can be overridden at instantiation:

```spice
.SUBCKT rc_filter IN OUT PARAMS: R=1k C=1u
R1 IN OUT {R}
C1 OUT 0 {C}
.ENDS

X1 IN OUT1 rc_filter R=10k C=100n
X2 IN OUT2 rc_filter R=1k C=10n
```

## Node Scoping

Internal nodes in a subcircuit are local — they do not conflict with nodes in the parent circuit or other subcircuit instances. Use `.GLOBAL` to make a node visible across all scopes (e.g., power rails).
