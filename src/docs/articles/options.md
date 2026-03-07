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
