# Q — Bipolar Junction Transistor (BJT)

The BJT is a three-terminal semiconductor device used for amplification and switching.

## Syntax

```
Q<name> <collector> <base> <emitter> <model_name> [<area>] [OFF] [IC=<vbe>[,<vce>]]
```

| Parameter | Description |
|-----------|-------------|
| `collector` | Collector node |
| `base` | Base node |
| `emitter` | Emitter node |
| `model_name` | Name of a `.MODEL NPN(...)` or `.MODEL PNP(...)` definition |
| `area` | Area factor |
| `OFF` | Initial guess: device is off |
| `IC=vbe[,vce]` | Initial junction voltages for `UIC` |

## Examples

```spice
Q1 C B E 2N3904
Q2 OUT BASE 0 NPN_MODEL 2.0
Q3 C B E PNP_MOD IC=0.7,5.0
```

## Model Definition

### NPN

```spice
.MODEL 2N3904 NPN(Is=6.734f Bf=416.4 Br=0.7374 Cje=3.638p Cjc=4.493p)
```

### PNP

```spice
.MODEL 2N3906 PNP(Is=1.41f Bf=180 Br=4 Cje=9.7p Cjc=18p)
```

### Common Model Parameters

| Parameter | Description |
|-----------|-------------|
| `Is` | Transport saturation current |
| `Bf` | Ideal forward current gain (β) |
| `Br` | Ideal reverse current gain |
| `Nf` | Forward current emission coefficient |
| `Nr` | Reverse current emission coefficient |
| `Cje` | Base-emitter zero-bias capacitance |
| `Cjc` | Base-collector zero-bias capacitance |
| `Vaf` | Forward Early voltage |
| `Var` | Reverse Early voltage |
| `Rb` | Base resistance |
| `Rc` | Collector resistance |
| `Re` | Emitter resistance |

## Typical Usage

```spice
Common-emitter amplifier
VCC VCC 0 12
V1 IN 0 DC 0 AC 1m
R1 VCC OUT 4.7k
R2 IN BASE 100k
R3 BASE 0 22k
Q1 OUT BASE 0 2N3904
.model 2N3904 NPN(Is=6.734f Bf=416.4)
.AC DEC 10 10 10MEG
.SAVE V(OUT)
.END
```
