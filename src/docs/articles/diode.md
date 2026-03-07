# D — Diode

The diode is a two-terminal semiconductor device that allows current to flow primarily in one direction.

## Syntax

```
D<name> <anode> <cathode> <model_name> [<area>] [OFF] [IC=<vd>]
```

| Parameter | Description |
|-----------|-------------|
| `anode` | Anode node (positive terminal) |
| `cathode` | Cathode node (negative terminal) |
| `model_name` | Name of a `.MODEL D(...)` definition |
| `area` | Area factor (multiplier for current capacity) |
| `OFF` | Initial guess: device is off |
| `IC=vd` | Initial diode voltage for `UIC` |

## Examples

```spice
D1 ANODE CATHODE 1N914
D2 IN OUT MyDiode 2.0
D3 A B DMOD OFF
```

## Model Definition

```spice
.MODEL 1N914 D(Is=2.52e-9 Rs=0.568 N=1.752 Cjo=4e-12 M=0.4 tt=20e-9)
```

### Common Model Parameters

| Parameter | Description | Default |
|-----------|-------------|---------|
| `Is` | Saturation current | 1e-14 A |
| `Rs` | Series resistance | 0 Ω |
| `N` | Emission coefficient | 1 |
| `Cjo` | Zero-bias junction capacitance | 0 F |
| `M` | Grading coefficient | 0.5 |
| `Vj` | Junction potential | 1 V |
| `tt` | Transit time | 0 s |
| `BV` | Reverse breakdown voltage | ∞ |
| `IBV` | Current at breakdown | 1e-3 A |

## Typical Usage

```spice
Diode IV Characteristic
D1 OUT 0 1N914
V1 OUT 0 0
.model 1N914 D(Is=2.52e-9 Rs=0.568 N=1.752)
.DC V1 -1 1 10e-3
.SAVE I(V1)
.END
```
