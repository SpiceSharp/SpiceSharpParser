# .OPTIONS Statement

The `.OPTIONS` statement sets simulator options that control accuracy, convergence, and behavior.

## Syntax

```
.OPTIONS <option>=<value> [<option2>=<value2> ...]
```

## Supported Options

### Tolerances

| Option | Description | Default |
|--------|-------------|---------|
| `ABSTOL=<value>` | Absolute current tolerance | 1e-12 |
| `RELTOL=<value>` | Relative tolerance | 1e-3 |
| `GMIN=<value>` | Minimum conductance | 1e-12 |

### Iteration Limits

| Option | Description |
|--------|-------------|
| `ITL1=<int>` | DC operating point max iterations |
| `ITL2=<int>` | DC sweep max iterations |
| `ITL4=<int>` | Transient max iterations |

### Temperature

| Option | Description |
|--------|-------------|
| `TEMP=<celsius>` | Simulation temperature |
| `TNOM=<celsius>` | Nominal/reference temperature for models |

### Integration Method

| Option | Description |
|--------|-------------|
| `METHOD=TRAP` | Trapezoidal integration |
| `METHOD=GEAR` | Gear integration |
| `METHOD=EULER` | Euler integration |

These options affect transient companion models and timestep history. For a
beginner-to-engine-level explanation, see
[Transient Integration Methods](transient-integration-methods.md).

### Random / Monte Carlo

| Option | Description |
|--------|-------------|
| `SEED=<int>` | Random number seed |
| `DISTRIBUTION=<name>` | Default probability distribution |

### Solver

| Option | Description |
|--------|-------------|
| `LOCALSOLVER=ON\|OFF` | Enable local solver |
| `CDFPOINTS=<int>` | CDF interpolation points (minimum 4) |
| `NORMALLIMIT=<value>` | Normal distribution limit |

## MNA View

Many `.OPTIONS` values do not change the circuit topology. They change how the
MNA solve is accepted:

| Option family | MNA effect |
|---------------|------------|
| Tolerances | Decide when Newton voltage/current changes and residuals are small enough. |
| Iteration limits | Bound how many times the matrix may be reloaded and solved. |
| `METHOD=...` | Changes transient companion-model coefficients and history terms. |
| `GMIN` | Adds or limits tiny conductance paths used to help numerical conditioning. |

For example, changing `METHOD=GEAR` does not change the netlist graph, but it
changes the matrix/RHS terms loaded by capacitors, inductors, and other dynamic
states during `.TRAN`.

For the underlying matrix algorithm, see
[How SpiceSharp Solves Circuits](spicesharp-architecture.md#modified-matrix-algorithm-step-by-step).
For a detailed explanation of Newton iteration, Jacobians, residuals, and
convergence, see
[Newton Iteration In Detail](spicesharp-architecture.md#newton-iteration-in-detail).

### Flags

| Option | Description |
|--------|-------------|
| `KEEPOPINFO` | Preserve operating point info (no value needed) |

## Examples

```spice
.OPTIONS RELTOL=1e-4 ABSTOL=1e-14
.OPTIONS TEMP=85 METHOD=GEAR
.OPTIONS ITL1=500 ITL4=100
.OPTIONS SEED=12345
```

## Typical Usage

```spice
High-accuracy simulation
V1 IN 0 1
R1 IN OUT 1k
C1 OUT 0 1u
.OPTIONS RELTOL=1e-6 ABSTOL=1e-15
.TRAN 1e-6 1e-3
.SAVE V(OUT)
.END
```
