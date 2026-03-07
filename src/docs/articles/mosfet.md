# M — MOSFET

The MOSFET (Metal-Oxide-Semiconductor Field-Effect Transistor) is a four-terminal semiconductor device widely used in digital and analog circuits.

## Syntax

```
M<name> <drain> <gate> <source> <bulk> <model_name> [L=<length>] [W=<width>] [M=<m>] [IC=<vds>[,<vgs>[,<vbs>]]] [OFF]
```

| Parameter | Description |
|-----------|-------------|
| `drain` | Drain node |
| `gate` | Gate node |
| `source` | Source node |
| `bulk` | Bulk/substrate node |
| `model_name` | Name of a `.MODEL NMOS(...)` or `.MODEL PMOS(...)` definition |
| `L=length` | Channel length |
| `W=width` | Channel width |
| `M=m` | Multiplier (parallel devices) |
| `IC=vds[,vgs[,vbs]]` | Initial junction voltages for `UIC` |
| `OFF` | Initial guess: device is off |

## Examples

```spice
M1 OUT IN 0 0 NMOS_MODEL L=1u W=10u
M2 OUT IN VDD VDD PMOS_MODEL L=0.5u W=20u
M3 D G S B my_nmos L=0.18u W=2u M=4
```

## Model Definition

### NMOS

```spice
.MODEL NMOS_MODEL NMOS(VTO=0.7 KP=110u GAMMA=0.4 LAMBDA=0.04 PHI=0.65)
```

### PMOS

```spice
.MODEL PMOS_MODEL PMOS(VTO=-0.7 KP=50u GAMMA=0.57 LAMBDA=0.05 PHI=0.65)
```

### Common Model Parameters

| Parameter | Description |
|-----------|-------------|
| `VTO` | Threshold voltage |
| `KP` | Transconductance parameter |
| `GAMMA` | Body-effect parameter |
| `LAMBDA` | Channel-length modulation |
| `PHI` | Surface potential |
| `TOX` | Oxide thickness |
| `CBD` | Bulk-drain junction capacitance |
| `CBS` | Bulk-source junction capacitance |

## Model Levels

SpiceSharpParser supports MOS Level 1 (Shichman-Hodges), Level 2, and Level 3 models, automatically selected based on the `LEVEL` parameter:

```spice
.MODEL my_nmos NMOS(LEVEL=1 VTO=0.7 KP=110u)
.MODEL my_nmos2 NMOS(LEVEL=2 VTO=0.7 KP=110u)
.MODEL my_nmos3 NMOS(LEVEL=3 VTO=0.7 KP=110u)
```

## Typical Usage

```spice
CMOS inverter
VDD VDD 0 3.3
VIN IN 0 PULSE(0 3.3 0 1n 1n 10n 20n)
M1 OUT IN VDD VDD PMOS_MODEL L=0.5u W=2u
M2 OUT IN 0 0 NMOS_MODEL L=0.5u W=1u
.MODEL PMOS_MODEL PMOS(VTO=-0.7 KP=50u)
.MODEL NMOS_MODEL NMOS(VTO=0.7 KP=110u)
.TRAN 0.1n 40n
.SAVE V(OUT) V(IN)
.END
```
