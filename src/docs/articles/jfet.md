# J — JFET

The JFET (Junction Field-Effect Transistor) is a three-terminal semiconductor device controlled by a voltage applied to the gate-source junction.

## Syntax

```
J<name> <drain> <gate> <source> <model_name> [<area>] [OFF] [IC=<vds>[,<vgs>]]
```

| Parameter | Description |
|-----------|-------------|
| `drain` | Drain node |
| `gate` | Gate node |
| `source` | Source node |
| `model_name` | Name of a `.MODEL NJF(...)` or `.MODEL PJF(...)` definition |
| `area` | Area factor |
| `OFF` | Initial guess: device is off |
| `IC=vds[,vgs]` | Initial junction voltages for `UIC` |

## Examples

```spice
J1 DRAIN GATE SOURCE J2N3819
J2 D G S my_njfet 2.0
```

## MNA View

A JFET is nonlinear. Its drain current depends on gate-source and drain-source
voltage, and its gate junction behaves diode-like. During Newton iteration,
SpiceSharp linearizes those equations around the current voltage guess.

Conceptually, the JFET contributes:

| Term | MNA role |
|------|----------|
| Channel conductance | Matrix slope between drain and source. |
| Transconductance | Controlled-source Jacobian term from gate voltage. |
| Gate junction conductance | Diode-like matrix and RHS terms. |
| Gate capacitances | AC admittance terms or transient companion terms. |

The stamp therefore changes as the operating point moves. `.AC` uses the
small-signal derivatives from the solved operating point, while `.TRAN` also
integrates any capacitance terms.

For the deeper solver picture, see
[How SpiceSharp Solves Circuits](spicesharp-architecture.md#j-jfet).

## Model Definition

```spice
.MODEL J2N3819 NJF(VTO=-3 BETA=1.304m LAMBDA=2.25m IS=33.57f CGS=2.414p CGD=0.3p)
```

### Common Model Parameters

| Parameter | Description |
|-----------|-------------|
| `VTO` | Pinch-off voltage |
| `BETA` | Transconductance coefficient |
| `LAMBDA` | Channel-length modulation |
| `IS` | Gate junction saturation current |
| `CGS` | Gate-source capacitance |
| `CGD` | Gate-drain capacitance |
| `RD` | Drain resistance |
| `RS` | Source resistance |
