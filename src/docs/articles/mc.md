# .MC Statement

The `.MC` statement configures Monte Carlo analysis, running the simulation multiple times with random parameter variations to assess statistical behavior.

## Syntax

```
.MC <runs> <analysis_type> <output_variable> <function> [SEED=<value>]
```

| Parameter | Description |
|-----------|-------------|
| `runs` | Number of Monte Carlo iterations |
| `analysis_type` | `OP`, `DC`, `TRAN`, `AC`, or `NOISE` |
| `output_variable` | Signal to monitor |
| `function` | Statistical analysis function |
| `SEED=<value>` | Optional random seed for reproducibility |

## Example

```spice
Monte Carlo analysis
V1 IN 0 1
R1 IN OUT 1k
C1 OUT 0 1u
.MC 100 TRAN V(OUT) MAX SEED=42
.TRAN 1e-6 10e-3
.SAVE V(OUT)
.END
```

## Parameter Tolerances

Monte Carlo analysis varies parameters that have defined tolerances. Tolerances are specified in model parameters or via distribution definitions (see `.DISTRIBUTION`).

## Notes

- The `SEED` parameter ensures repeatable results across runs.
- Use `.DISTRIBUTION` to define custom probability density functions.
- Use `.OPTIONS DISTRIBUTION=<name>` to set the default distribution.
