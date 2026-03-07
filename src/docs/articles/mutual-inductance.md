# K — Mutual Inductance

The mutual inductance statement defines magnetic coupling between two inductors.

## Syntax

```
K<name> <L1_name> <L2_name> <coupling_coefficient> [M=<m>]
```

| Parameter | Description |
|-----------|-------------|
| `L1_name` | Name of the first inductor |
| `L2_name` | Name of the second inductor |
| `coupling_coefficient` | Coupling factor k (between -1 and 1) |
| `M=m` | Multiplier |

## Examples

```spice
* Simple transformer coupling
L1 PRI_A PRI_B 10m
L2 SEC_A SEC_B 10m
K1 L1 L2 0.99

* Loosely coupled inductors
L3 A 0 100u
L4 B 0 100u
K2 L3 L4 0.5
```

## Ideal Transformer

For an ideal transformer, use a coupling coefficient close to 1.0. The turns ratio is determined by the inductance ratio:

$$n = \sqrt{\frac{L_1}{L_2}}$$

```spice
* 10:1 turns ratio transformer
L1 IN 0 100m
L2 OUT 0 1m
K1 L1 L2 0.999
```
