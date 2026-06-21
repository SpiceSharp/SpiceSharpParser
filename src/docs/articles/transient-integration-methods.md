# Transient Integration Methods And Engine Derivatives

Transient analysis (`.TRAN`) answers one question:

```text
What does the circuit do as time moves forward?
```

That sounds simple, but capacitors, inductors, semiconductor charges, transfer
functions, delays, and waveform sources all depend on time. The simulator cannot
put "change over time" directly into the normal modified nodal analysis matrix.
It first converts time derivatives into algebraic companion models for the
current timestep.

This article starts with the basic idea, then connects it to the derivatives and
history terms used by SpiceSharp.

## The Basic .TRAN Loop

A transient simulation is not one solve. It is many operating-point solves in a
time loop.

```text
initialize:
  solve the DC operating point, or use initial conditions with UIC
  initialize capacitor, inductor, and internal dynamic histories

while time < stop time:
  choose a candidate timestep
  build companion models for that timestep

  repeat Newton iterations:
    clear matrix and RHS
    load device equations, derivatives, and history terms
    solve the linearized matrix
    update nonlinear guesses
    check convergence

  if the point converged and integration error is acceptable:
    accept the point
    commit dynamic histories
    export saved values
  else:
    reject the point
    reduce the timestep
    try again from the last accepted time
```

The important word is **candidate**. SpiceSharp may try a timestep, decide the
result is not accurate or did not converge, reject it, and try a smaller one.
Capacitor charge, inductor flux, and other dynamic histories are committed only
after a timestep is accepted.

## Why Integration Methods Exist

Resistors are algebraic:

$$
i = Gv
$$

Capacitors and inductors are differential:

$$
i = C\frac{dv}{dt}
$$

$$
v = L\frac{di}{dt}
$$

The matrix solver wants equations that look like "coefficient times unknown plus
known right-hand-side value." It does not solve symbolic derivatives directly.
An integration method replaces a derivative with an algebraic approximation
based on:

- the current unknown value,
- the timestep size,
- previous accepted values,
- previous accepted derivatives.

For a capacitor, the engine builds a temporary relation like:

$$
i_n \approx g_{\text{eq}}v_n + i_{\text{history}}
$$

For an inductor, it builds a branch relation like:

$$
v_n \approx r_{\text{eq}}i_n + v_{\text{history}}
$$

Those temporary relations are called **companion models**. They behave like a
simple matrix stamp plus a history source for one solve attempt.

## How .TRAN Uses The Modified Matrix Algorithm

The modified nodal analysis matrix algorithm does not disappear during transient
simulation. `.TRAN` uses the same matrix idea repeatedly:

```text
choose candidate time
turn dynamic devices into companion models
clear matrix and RHS
stamp all devices
solve matrix
check Newton convergence and truncation error
accept or reject the candidate time
```

The difference is that capacitors, inductors, and other dynamic devices load
timestep-dependent stamps. Their matrix coefficients and RHS history terms can
change every candidate timestep.

### Example: RC Step Matrix For One Candidate Timestep

Use this circuit:

```spice
V1 in 0 5
R1 in out 1k
C1 out 0 1u
.OPTIONS METHOD=EULER
.TRAN 1m 10m
```

Suppose the simulation is building one backward-Euler candidate timestep:

```text
h = 1 ms
previous accepted V(out) = 2 V
current source voltage V(in) = 5 V
```

The resistor conductance is:

$$
g_R = \frac{1}{1000} = 0.001
$$

The capacitor companion coefficient is:

$$
g_C = \frac{C}{h} = \frac{1u}{1m} = 0.001
$$

Using the convention that capacitor current leaves node `out` toward ground:

$$
i_C \approx g_C V(\text{out}) + i_{\text{hist}}
$$

where:

$$
i_{\text{hist}} = -g_C V_{\text{prev}} = -0.001 \cdot 2 = -0.002
$$

The unknown vector for this candidate solve is:

$$
x =
\begin{bmatrix}
V(\text{in}) \\
V(\text{out}) \\
I(\text{V1})
\end{bmatrix}
$$

The rows mean:

```text
row V(in):   KCL at input node
row V(out):  KCL at output node
row I(V1):   voltage source constraint
```

The stamped system is:

$$
\begin{bmatrix}
g_R & -g_R & 1 \\
-g_R & g_R + g_C & 0 \\
1 & 0 & 0
\end{bmatrix}
\begin{bmatrix}
V(\text{in}) \\
V(\text{out}) \\
I(\text{V1})
\end{bmatrix}
=
\begin{bmatrix}
0 \\
-i_{\text{hist}} \\
5
\end{bmatrix}
$$

Insert the numbers:

$$
\begin{bmatrix}
0.001 & -0.001 & 1 \\
-0.001 & 0.002 & 0 \\
1 & 0 & 0
\end{bmatrix}
\begin{bmatrix}
V(\text{in}) \\
V(\text{out}) \\
I(\text{V1})
\end{bmatrix}
=
\begin{bmatrix}
0 \\
0.002 \\
5
\end{bmatrix}
$$

The third row forces:

$$
V(\text{in}) = 5
$$

The output row then says:

$$
-0.001 \cdot 5 + 0.002 V(\text{out}) = 0.002
$$

So:

$$
V(\text{out}) = 3.5\,\text{V}
$$

This one solve says: if the previous accepted capacitor voltage was 2 V, then a
1 ms backward-Euler step toward a 5 V source through 1 kOhm lands at 3.5 V.

The key matrix lesson:

| Device | Matrix contribution | RHS contribution |
|--------|---------------------|------------------|
| `R1` | `g_R` conductance terms between `in` and `out`. | None. |
| `C1` | `g_C` conductance-like term at `out`. | History current from previous accepted charge. |
| `V1` | Branch-current column and voltage-constraint row. | Source voltage `5`. |

At the next accepted timestep, `V(out) = 3.5 V` becomes the new capacitor
history. If this candidate timestep is rejected, that history is not committed.

## What Truncation Error Means

Truncation error is the error caused by replacing a real continuous-time curve
with a finite-step approximation.

The word "truncation" means the integration method keeps only part of the full
time behavior and drops the rest. Mathematically, numerical integration methods
can be understood as keeping a few terms from a local time expansion and
ignoring higher-order terms. Practically, it means this:

```text
real circuit state:       smooth curve between time points
integration method view:  approximation built from stored history
truncation error:         difference between the two
```

For a capacitor:

$$
i = \frac{dQ}{dt}
$$

Backward Euler approximates that derivative as:

$$
i_n \approx \frac{Q_n - Q_{n-1}}{h}
$$

If `Q(t)` is nearly straight over the timestep `h`, this approximation is good.
If `Q(t)` bends sharply because a source edge, diode turn-on, or switch event
just happened, the approximation may be too coarse. The omitted bending is the
truncation error.

For an inductor:

$$
v = \frac{d\Phi}{dt}
$$

The same idea applies. If flux linkage changes smoothly, a larger step may be
acceptable. If flux changes quickly, the candidate timestep may be rejected
because the flux history cannot accurately represent the curve over that whole
step.

Newton convergence and truncation-error control are two separate acceptance
checks for the same candidate timestep.

| Check | Main question | Main inputs |
|-------|---------------|-------------|
| Newton convergence | Do the equations balance at this candidate time? | KCL, KVL, device equations, residuals, and variable changes. |
| Truncation-error check | Is this time jump accurate enough? | Dynamic history, derivative estimates, timestep size, and local error estimate. |

Newton works vertically at one candidate time. It asks whether the circuit is
self-consistent at that time:

```text
given candidate time t[n]
given companion models for this h
find V(nodes) and I(branches)
make KCL/KVL/device equations balance
```

Truncation-error control works horizontally across time. It asks whether the
move from the last accepted time to this candidate time was too large:

```text
given accepted time t[n-1]
given candidate time t[n]
compare dynamic history and derivative estimates
decide whether this time jump is accurate enough
```

Those checks can disagree because they measure different things.

So this situation is possible:

```text
Newton converged
but truncation error is too high
```

That means the circuit equations were solved for the attempted point, but the
attempted step was too large for the time-domain history approximation. The
candidate voltage/current values may be internally consistent, but the path from
the previous accepted point to those values is not trusted.

Example:

```text
accepted time: 10 us
try time:      12 us

Newton result at 12 us:
  V(out) = 4.8 V
  I(L1)  = 0.14 A
  KCL/KVL residuals are small
  Newton says: solved

truncation check:
  capacitor charge changed too sharply from 10 us to 12 us
  flux/current history estimate is too coarse
  truncation says: do not trust this 2 us jump

decision:
  reject 12 us
  keep history at 10 us
  retry with a smaller step, for example 11 us
```

When this happens, the simulator does not commit capacitor charge or inductor
flux history from the rejected point. It reduces the timestep and tries again
from the last accepted time.

A simple mental model:

```text
Newton failure:
  "I could not solve the equations at this candidate time."

truncation failure:
  "I solved them, but I do not trust this time jump."
```

Common reasons for high truncation error:

- sharp `PULSE` or `PWL` edges,
- diode turn-on or turn-off,
- switch state changes,
- a capacitor charging in a short current spike,
- an inductor current ramp changing too quickly,
- LC or RLC ringing that is too fast for the current timestep,
- `tmaxstep` much larger than the fastest important circuit event.

Typical fixes:

- reduce `tmaxstep`,
- smooth ideal source or behavioral transitions,
- add realistic resistance/parasitics,
- compare `METHOD=TRAP` and `METHOD=GEAR`,
- inspect whether the circuit contains impossible ideal source loops or
  discontinuous expressions.

## Method Selection

SpiceSharpParser selects the integration method with `.OPTIONS METHOD=...`.

| Netlist option | SpiceSharp method | Main character |
|----------------|-------------------|----------------|
| `METHOD=TRAP` | `Trapezoidal` | Accurate for many smooth circuits, but can show numerical ringing in stiff circuits. |
| `METHOD=TRAPEZOIDAL` | `Trapezoidal` | Same as `METHOD=TRAP`. |
| `METHOD=GEAR` | `Gear` | More numerical damping, often useful for stiff or switching circuits. |
| `METHOD=EULER` | `FixedEuler` | Fixed-timestep backward Euler; simple and damped, but usually less accurate. |

Example:

```spice
RC charging with Gear integration
V1 in 0 PULSE(0 5 0 1n 1n 5m 10m)
R1 in out 1k
C1 out 0 1u
.OPTIONS METHOD=GEAR
.TRAN 10u 5m 0 10u
.SAVE V(out)
.END
```

If no method is specified, transient construction uses SpiceSharp's normal
transient defaults or the explicit time-parameter object created by the parser.
In practice, `TRAP` is the usual first method to try, and `GEAR` is a common
second try when stiff switching behavior or trapezoidal ringing appears.

## Timestep Terms

The `.TRAN` syntax is:

```spice
.TRAN <tstep> <tstop> [<tstart> [<tmaxstep>]] [UIC]
```

| Term | Meaning |
|------|---------|
| `tstep` | Output cadence and initial timestep hint. |
| `tstop` | End time. |
| `tstart` | Time before which output is not saved. |
| `tmaxstep` | Maximum internal timestep when supplied. |
| `UIC` | Use initial conditions instead of solving the DC operating point first. |

Do not read `tstep` as "the solver will always step exactly this much." Adaptive
methods may use smaller or different internal timesteps. `tmaxstep` is the knob
that prevents the solver from jumping over events or waveform detail.

For a sharp pulse edge, prefer a small `tmaxstep` near the edge scale:

```spice
Pulse edge with tight max step
V1 in 0 PULSE(0 5 100n 1n 1n 100n 500n)
R1 in out 100
C1 out 0 10p
.TRAN 1n 1u 0 0.5n
.SAVE V(in) V(out)
.END
```

Here `tmaxstep=0.5n` keeps the internal solver from leaping across a 1 ns edge.

## Derivatives Used By The Engine

There are two different derivative ideas in transient simulation.

### Local Linearization Derivatives

Nonlinear devices are solved by Newton iteration. At each Newton guess, the
device is replaced by a local linear approximation. That approximation needs
slopes.

| Derivative | Used for |
|------------|----------|
| `dI/dV` | Nonlinear conductance in the Jacobian matrix. |
| `dQ/dV` | Incremental capacitance from charge as a function of voltage. |
| `dPhi/dI` | Incremental inductance from flux linkage as a function of current. |
| Partial derivatives such as `dI/dVgs` | Controlled-source and transistor transconductance terms. |

These derivatives say, "If this unknown changes a little, how much does this
device equation change?"

For a nonlinear capacitor:

$$
Q = Q(V)
$$

The local incremental capacitance is:

$$
C_{\text{inc}} = \frac{dQ}{dV}
$$

For a nonlinear inductor:

$$
\Phi = \Phi(I)
$$

The local incremental inductance is:

$$
L_{\text{inc}} = \frac{d\Phi}{dI}
$$

### Time Derivatives And Integration History

The transient engine also needs derivatives with respect to time:

| Time derivative | Physical meaning |
|-----------------|------------------|
| `dQ/dt` | Capacitor current. |
| `dPhi/dt` | Inductor voltage. |
| `dv/dt` | Voltage rate of change used by capacitive behavior. |
| `di/dt` | Current rate of change used by inductive behavior. |

These derivatives are not just local slopes. They depend on previous accepted
timesteps. The integration method owns that history.

SpiceSharp exposes that bridge through integration states. For charge-defined
and flux-defined custom components, the pattern is:

```csharp
var method = context.GetState<IIntegrationMethod>();
var state = method.CreateDerivative(track: true);

state.Value = currentStoredQuantity;
state.Derive();
JacobianInfo info = state.GetContributions(localDerivative, currentUnknown);
```

Conceptually:

- `Value` is the stored quantity, such as `Q` or `Phi`.
- `Derive()` computes the time derivative, such as `dQ/dt` or `dPhi/dt`.
- `GetContributions(...)` returns the Jacobian coefficient and RHS history term
  for the active integration method.

That is where the math becomes matrix data.

## How Companion Models Stamp The Matrix

For a capacitor between nodes `p` and `n`, transient analysis uses:

$$
i \approx g_{\text{eq}}v + i_{\text{history}}
$$

The conductance part contributes to the Jacobian:

| Matrix entry | Contribution |
|--------------|--------------|
| `Y[p,p]` | `+geq` |
| `Y[p,n]` | `-geq` |
| `Y[n,p]` | `-geq` |
| `Y[n,n]` | `+geq` |

The history current contributes to the RHS with opposite signs on the two
terminal nodes. The exact sign depends on the branch-current convention, but the
idea is always the same: one part is a matrix coefficient, the other part is a
known history source.

For an inductor, MNA usually introduces a branch-current variable. The transient
relation is:

$$
v \approx r_{\text{eq}}i + v_{\text{history}}
$$

The node equations connect terminal voltage to branch current, while the branch
equation receives the resistance-like coefficient and history term.

## Companion Model Families

A companion model is a temporary linear equivalent that a device loads into the
matrix for the current solve. It is "temporary" because it is valid only for:

- the current candidate timestep,
- the current Newton guess,
- the current analysis type.

The word companion is used most often for capacitors and inductors in transient
analysis, but the same idea appears in several forms.

| Family | Used by | What changes each solve |
|--------|---------|-------------------------|
| Static algebraic stamp | Resistors, linear controlled sources | Usually nothing except parameter changes. |
| Source equivalent | Independent sources, waveform sources | The RHS value may change with time. |
| Newton companion | Diodes, transistors, nonlinear expressions | Local slope and equivalent source change each Newton iteration. |
| Integration companion | Capacitors, inductors, charge/flux states | Coefficients and history sources change with timestep and accepted history. |
| Combined nonlinear dynamic companion | Semiconductor charges, `Q=`, `Flux=` | Local derivatives and integration history both matter. |

The useful reading habit is to ask four questions:

| Question | Capacitor answer | Inductor answer |
|----------|------------------|-----------------|
| What is the current unknown? | Terminal voltage `v`. | Branch current `I(L)`. |
| What is remembered? | Charge `q`. | Flux `Phi`. |
| What becomes a matrix coefficient? | `dq/dv` times integration coefficient. | `dPhi/di` times integration coefficient. |
| What becomes RHS/history? | Old accepted charge/voltage information. | Old accepted flux/current information. |

### Capacitor Companion Model

For a linear capacitor:

$$
q = C v
$$

and:

$$
i = \frac{dq}{dt}
$$

The integration method turns that into:

$$
i_n \approx g_{\text{eq}} v_n + i_{\text{hist}}
$$

For backward Euler:

$$
g_{\text{eq}} = \frac{C}{h}
$$

and the history term contains the previous accepted capacitor voltage:

$$
i_{\text{hist}} = -\frac{C}{h}v_{n-1}
$$

So the capacitor companion looks like a conductance in parallel with a current
source. It is not physically losing its storage behavior. The storage behavior
is hidden in the history source and in the fact that `g_eq` depends on the
timestep.

Smaller timestep means larger `C/h`. That makes the companion conductance
larger, which mathematically expresses the physical idea that capacitor voltage
cannot jump easily over a very short time.

For trapezoidal and Gear methods, the same shape remains:

```text
current unknown part -> matrix conductance
accepted history     -> RHS current source
```

Only the coefficients and the amount of remembered history change.

### Inductor Companion Model

For a linear inductor:

$$
\Phi = L i
$$

and:

$$
v = \frac{d\Phi}{dt}
$$

MNA introduces a branch-current unknown, usually written here as `b = I(L)`.
The integration method turns the flux derivative into:

$$
v_n \approx r_{\text{eq}} i_n + v_{\text{hist}}
$$

For backward Euler:

$$
r_{\text{eq}} = \frac{L}{h}
$$

and the history term contains the previous accepted inductor current:

$$
v_{\text{hist}} = -\frac{L}{h}i_{n-1}
$$

The branch equation is conceptually:

$$
V(p) - V(n) \approx r_{\text{eq}} I(L) + v_{\text{hist}}
$$

That means the inductor companion is not just a two-node conductance stamp. It
uses:

- node-voltage coefficients in the branch equation,
- a branch-current unknown,
- a branch coefficient similar to a series resistance,
- a RHS/history term from previous accepted flux/current.

Smaller timestep means larger `L/h`. That expresses the physical idea that
inductor current cannot jump easily over a very short time.

### Newton Companion For A Diode

A diode does not need integration history just because it is nonlinear. It needs
Newton linearization because its current is curved:

$$
i = I_s(e^{v/(nV_t)} - 1)
$$

At one Newton guess, the diode is replaced by:

$$
i \approx g_d v + i_{\text{eq}}
$$

where:

$$
g_d = \left.\frac{di}{dv}\right|_{\text{guess}}
$$

and:

$$
i_{\text{eq}} = i(v_{\text{guess}}) - g_d v_{\text{guess}}
$$

This looks similar to a capacitor companion because it is also a conductance
plus a current source. The reason is different:

| Device | Matrix coefficient means | RHS/source term means |
|--------|--------------------------|-----------------------|
| Capacitor | Time integration coefficient from charge. | Accepted timestep history. |
| Diode | Local nonlinear slope at the Newton guess. | Linearization correction at the same guess. |

A diode with junction capacitance can have both at once: a Newton conductance
from `dI/dV`, plus dynamic charge/capacitance companion terms from `dQ/dt`.

### Switch Companion

An idealized controlled switch is usually algebraic:

```text
control says on  -> conductance near 1/Ron
control says off -> conductance near 1/Roff
```

It stamps like a resistor using the conductance selected by the current control
value. There is no stored energy and no integration history. A switch can still
make transient analysis difficult because an abrupt conductance change can force
smaller timesteps or make Newton iteration jump between very different matrices.

### Source Companion Or Equivalent

Independent current sources mostly load the RHS:

```text
rhs[p] -= I(t)
rhs[n] += I(t)
```

Independent voltage sources add a branch-current unknown and a branch equation:

```text
V(p) - V(n) = Vsource(t)
```

These are not integration companions. A `PULSE`, `SIN`, or `PWL` source changes
its value with time, and source breakpoints can pressure timestep selection, but
the source itself does not remember charge or flux.

### Nonlinear Charge And Flux Companions

The custom nonlinear passive forms make the companion idea visible.

For a nonlinear capacitor:

$$
Q = Q(V)
$$

The current is:

$$
i = \frac{dQ}{dt}
$$

At the current Newton guess, the device also needs the local derivative:

$$
C_{\text{inc}} = \frac{dQ}{dV}
$$

The integration state combines both ideas:

```text
stored quantity: Q(V)
local derivative: dQ/dV
time derivative: dQ/dt
```

So the stamp contains:

- a Jacobian term based on `dQ/dV` and the integration method,
- a RHS/history term based on accepted charge history,
- possible nonlinear correction terms because `Q(V)` is not a straight line.

For a nonlinear inductor:

$$
\Phi = \Phi(I)
$$

The voltage is:

$$
v = \frac{d\Phi}{dt}
$$

At the current Newton guess:

$$
L_{\text{inc}} = \frac{d\Phi}{dI}
$$

The stamp contains:

- a branch-row coefficient based on `dPhi/dI` and the integration method,
- a RHS/history term based on accepted flux history,
- possible nonlinear correction terms because `Phi(I)` is not a straight line.

That is why `IDerivative.GetContributions(...)` needs both a local derivative
and the current unknown value. The integration method owns the time history, but
the device still owns the local relationship between the unknown and the stored
quantity.

## Component Roles In Transient Integration

Not every component owns integration history. Some components only load
algebraic equations, some provide time-dependent source values, and some create
state that must be integrated across accepted timesteps.

| Component type | What it contributes in `.TRAN` | Integration application |
|----------------|--------------------------------|-------------------------|
| Resistor | Fixed conductance and current relation. | No integration history. It shapes time constants by damping capacitors and inductors. |
| Independent source | DC or waveform value at the current time. | No stored history; waveform edges can limit timesteps. |
| Capacitor | Companion conductance plus history current. | Stores charge. The method approximates `dQ/dt`, which is capacitor current. |
| Inductor | Branch-current equation plus history term. | Stores flux linkage. The method approximates `dPhi/dt`, which is inductor voltage. |
| Diode/semiconductor | Nonlinear current plus derivatives such as `dI/dV`. | Charge/capacitance terms add history when modeled. |
| Switch | Control-dependent conductance. | No stored energy; abrupt changes can limit timesteps. |
| Nonlinear `Q=` capacitor | `Q(V)`, `dQ/dV`, `dqdt`, and companion contribution. | `GetContributions(...)` returns Jacobian/RHS terms. |
| Nonlinear `Flux=` inductor | `Phi(I)`, `dPhi/dI`, `dfluxdt`, and branch contribution. | `GetContributions(...)` returns branch-equation terms. |

The key distinction is this:

```text
algebraic components affect the current solve
dynamic components affect the current solve and remember accepted history
```

Resistors, independent sources, and idealized switches can still make transient
simulation hard. They influence slopes, discontinuities, and convergence. But
capacitors, inductors, charge storage, flux linkage, delays, and transfer
function states are the pieces that make integration history necessary.

## Built-In Capacitor And Inductor Integration

The built-in SpiceSharp capacitor and inductor are the simplest useful examples
of transient integration. They show the same pattern that more complex devices
use: compute a stored quantity, ask the active integration method for the time
derivative, then stamp matrix and RHS contributions.

### First Mental Model

SPICE solves algebraic systems. It is comfortable with equations like:

$$
Yx = \text{rhs}
$$

Capacitors and inductors are not purely algebraic. They contain derivatives:

$$
\begin{aligned}
\text{capacitor:}\quad i &= C\frac{dv}{dt} \\
\text{inductor:}\quad v &= L\frac{di}{dt}
\end{aligned}
$$

Numerical integration is the trick that turns those derivative equations into
algebraic equations for one timestep.

The capacitor path is:

```text
voltage now -> charge now -> derivative of charge -> current now
v           -> q = C*v    -> dq/dt                -> i
```

The inductor path is:

```text
current now -> flux now  -> derivative of flux -> voltage now
i           -> Phi = L*i -> dPhi/dt            -> v
```

That is the most important mirror:

```text
capacitor remembers voltage history as charge
inductor remembers current history as flux
```

After the integration method does its work, the solver sees companion models:

| Device | What the solver receives during one timestep |
|--------|----------------------------------------------|
| Capacitor | Conductance-like matrix stamp plus history current source. |
| Inductor | Branch-current equation plus history voltage/RHS term. |

The words "history current" and "history voltage" mean: information from
previous accepted timesteps has been moved to the right-hand side or companion
source term, while the current unknown remains in the matrix.

The analysis type changes how the same physical device is interpreted:

| Analysis | Capacitor | Inductor |
|----------|-----------|----------|
| `.OP` / DC bias | Open circuit for steady-state current. | Short branch relation with unknown current. |
| `.AC` / frequency | Uses `I = s*C*V`; no timestep history. | Uses `V = s*L*I`; no timestep history. |
| `.TRAN` / time | Uses charge history to compute `dq/dt`. | Uses flux history to compute `dPhi/dt`. |

### Detailed Walkthrough: Derivative To Matrix Terms

The confusing part is usually this question:

```text
How can a derivative become a matrix stamp?
```

The answer is that the integration method separates each dynamic equation into
two parts:

```text
current unknown part -> matrix/Jacobian
accepted history     -> RHS/history source
```

Backward Euler is the simplest example because it uses one previous accepted
point. Other methods use different coefficients and more history, but they
still make the same kind of split.

For a capacitor:

$$
\begin{aligned}
q &= Cv \\
i &= \frac{dq}{dt}
\end{aligned}
$$

At timestep `n`, backward Euler estimates:

$$
i_n \approx \frac{q_n - q_{n-1}}{h}
$$

Substitute $q = Cv$:

$$
i_n \approx \frac{C v_n - C v_{n-1}}{h}
$$

Rearrange:

$$
i_n \approx \frac{C}{h}v_n - \frac{C}{h}v_{n-1}
$$

Read this equation like the engine reads it:

| Piece | Engine meaning |
|-------|----------------|
| `v_n` | Unknown capacitor voltage for the current candidate timestep. |
| `(C/h) * v_n` | Matrix/Jacobian contribution, like a conductance. |
| `v_{n-1}` | Previous accepted voltage, already known. |
| `-(C/h) * v_{n-1}` | RHS/history current contribution. |

So the capacitor is not actually becoming a resistor forever. It is only being
converted into a resistor-like companion model for this one candidate timestep.
When the timestep is accepted, the new charge becomes history for the next
timestep.

In SpiceSharp terms:

```text
v = V(p) - V(n)
_qcap.Value = C * v
_qcap.Derive()
_qcap.GetContributions(C, v)
```

`_qcap` holds the charge state. `Derive()` applies the active integration
method. `GetContributions(...)` is the bridge from stored charge history to the
matrix coefficient and RHS history current.

For an inductor:

$$
\begin{aligned}
\Phi &= Li \\
v &= \frac{d\Phi}{dt}
\end{aligned}
$$

At timestep `n`, backward Euler estimates:

$$
v_n \approx \frac{\Phi_n - \Phi_{n-1}}{h}
$$

Substitute $\Phi = Li$:

$$
v_n \approx \frac{L i_n - L i_{n-1}}{h}
$$

Rearrange:

$$
v_n \approx \frac{L}{h}i_n - \frac{L}{h}i_{n-1}
$$

Read this equation like the engine reads it:

| Piece | Engine meaning |
|-------|----------------|
| `i_n = I(L)` | Unknown branch current for the current candidate timestep. |
| `(L/h) * i_n` | Branch-row matrix coefficient, like a resistance. |
| `i_{n-1}` | Previous accepted branch current, already known. |
| `-(L/h) * i_{n-1}` | RHS/history voltage contribution. |

The inductor also needs node-voltage terms because its branch equation relates
the terminal voltage to the branch current:

$$
V(p) - V(n) \approx \frac{L}{h}I(L) - \frac{L}{h}i_{n-1}
$$

So the branch row contains current node voltages, the current inductor branch
unknown, and the known history term.

In SpiceSharp terms:

```text
i = I(L)
_flux.Value = L * i
_flux.Derive()
_flux.GetContributions(L, i)
```

`_flux` holds the flux state. `Derive()` applies the active integration method.
`GetContributions(...)` is the bridge from stored flux history to the branch
coefficient and RHS history term.

### Tiny Numerical Examples

These examples use backward Euler because it has the smallest amount of history.
They are not special SpiceSharp syntax; they are the arithmetic that happens
behind a normal `C` or `L` element during one candidate timestep.

Capacitor example:

$$
\begin{aligned}
C &= 1\,\mu\text{F} \\
h &= 1\,\text{ms} \\
v_{n-1} &= 2\,\text{V} \\
v_n &= \text{current unknown capacitor voltage}
\end{aligned}
$$

Backward Euler gives:

$$
i_n = \frac{C}{h}v_n - \frac{C}{h}v_{n-1}
$$

Compute the coefficient:

$$
\frac{C}{h} =
\frac{1\times10^{-6}}{1\times10^{-3}}
= 0.001\,\text{S}
$$

So the capacitor current equation for this candidate timestep is:

$$
\begin{aligned}
i_n &= 0.001v_n - 0.001\cdot2 \\
    &= 0.001v_n - 0.002\,\text{A}
\end{aligned}
$$

Read it as a stamp:

| Part | Meaning |
|------|---------|
| `0.001 * v[n]` | Matrix coefficient, equivalent to a 1 mS conductance. |
| `-0.002 A` | History current from the previous accepted 2 V. |

If Newton eventually solves this timestep with:

$$
v_n = 2.5\,\text{V}
$$

then:

$$
\begin{aligned}
i_n &= 0.001\cdot2.5 - 0.002 \\
    &= 0.0005\,\text{A}
\end{aligned}
$$

The charge view says the same thing:

$$
\begin{aligned}
q_{n-1} &= C\cdot2.0 = 2.0\,\mu\text{C} \\
q_n &= C\cdot2.5 = 2.5\,\mu\text{C} \\
dq &= 0.5\,\mu\text{C} \\
\frac{dq}{h} &= \frac{0.5\,\mu\text{C}}{1\,\text{ms}}
             = 0.5\,\text{mA}
\end{aligned}
$$

That is why the companion model is not a trick separate from physics. It is the
same charge law rewritten into a form the matrix solver can use.

Inductor example:

$$
\begin{aligned}
L &= 10\,\text{mH} \\
h &= 1\,\text{ms} \\
i_{n-1} &= 0.2\,\text{A} \\
i_n &= I(L)
\end{aligned}
$$

Backward Euler gives:

$$
v_n = \frac{L}{h}i_n - \frac{L}{h}i_{n-1}
$$

Compute the coefficient:

$$
\frac{L}{h} =
\frac{10\times10^{-3}}{1\times10^{-3}}
= 10\,\Omega
$$

So the branch relation for this candidate timestep is:

$$
\begin{aligned}
V(p) - V(n) &= 10I(L) - 10\cdot0.2 \\
            &= 10I(L) - 2\,\text{V}
\end{aligned}
$$

Read it as a stamp:

| Part | Meaning |
|------|---------|
| `V(p) - V(n)` | Current node-voltage unknowns in the branch row. |
| `10 * I(L)` | Matrix coefficient for the inductor branch-current unknown. |
| `-2 V` | History voltage term from the previous accepted 0.2 A. |

If Newton eventually solves this timestep with:

$$
I(L) = 0.25\,\text{A}
$$

then:

$$
\begin{aligned}
V(p) - V(n) &= 10\cdot0.25 - 2 \\
            &= 0.5\,\text{V}
\end{aligned}
$$

The flux view says the same thing:

$$
\begin{aligned}
\Phi_{n-1} &= L\cdot0.20 = 0.0020 \\
\Phi_n &= L\cdot0.25 = 0.0025 \\
d\Phi &= 0.0005 \\
\frac{d\Phi}{h} &= \frac{0.0005}{0.001} = 0.5\,\text{V}
\end{aligned}
$$

So the inductor companion model is the flux law rewritten into a branch equation
that MNA can solve.

### Built-In Capacitor Behavior Stack

For a normal `C` element, SpiceSharp 3.2.3 uses these behavior classes:

| Behavior | Used in | What it does |
|----------|---------|--------------|
| `Capacitors.Temperature` | setup/temperature | Computes the effective capacitance used by other behaviors. |
| `Capacitors.Frequency` | `.AC` | Stamps `Y = sC` into the complex matrix. |
| `Capacitors.Time` | `.TRAN`, bias load | Stores charge in `IDerivative _qcap` and stamps the transient companion model. |

There is no separate public `Capacitors.Biasing` class in this SpiceSharp
version. `Capacitors.Time` implements both `ITimeBehavior` and
`IBiasingBehavior`, so it can behave like an open circuit during DC
initialization and like a companion model during transient timesteps.

The linear capacitor state is:

$$
q = C v
$$

The current is:

$$
i = \frac{dq}{dt}
$$

Conceptually, `InitializeStates` seeds `_qcap` before the time loop:

```text
if UIC and IC= was given:
  v_initial = device IC
else:
  v_initial = operating-point voltage

_qcap.Value = C * v_initial
```

At each candidate timestep, `Load` behaves like:

```text
v = V(p) - V(n)
q = C * v

_qcap.Value = q
_qcap.Derive()
info = _qcap.GetContributions(C, v)

stamp info.Jacobian into Y[p,p], Y[p,n], Y[n,p], Y[n,n]
stamp info.Rhs into rhs[p] and rhs[n] with opposite signs
```

`info.Jacobian` is the companion conductance-like term. `info.Rhs` is the
history current term. The active integration method decides their exact values.

For backward Euler, the idea reduces to:

$$
i_n = C\frac{v_n-v_{n-1}}{h}
$$

which can be rearranged into:

$$
i_n = \frac{C}{h}v_n - \frac{C}{h}v_{n-1}
$$

So the current timestep sees a conductance-like coefficient plus a known history
source. Trapezoidal and Gear use different coefficients and more accepted
history, but the stamp shape is the same.

### Built-In Inductor Behavior Stack

For a normal `L` element, SpiceSharp 3.2.3 uses these behavior classes:

| Behavior | Used in | What it does |
|----------|---------|--------------|
| `Inductors.Temperature` | setup/temperature | Computes the effective inductance. |
| `Inductors.Biasing` | `.OP`, `.DC`, `.TRAN` bias load | Creates the branch-current unknown and ideal branch constraint. |
| `Inductors.Frequency` | `.AC` | Stamps `V = sL I` into the complex branch equation. |
| `Inductors.Time` | `.TRAN` | Stores flux in `IDerivative _flux` and stamps the transient branch companion. |

The linear inductor state is:

$$
\Phi = L i
$$

The voltage is:

$$
v = \frac{d\Phi}{dt}
$$

Unlike a capacitor, an inductor's natural dynamic unknown is current. MNA adds a
branch-current unknown `b = I(L)` and a branch equation. The node rows use `b`
in KCL, and the branch row enforces the voltage/current relation.

Conceptually, `InitializeStates` seeds `_flux` before the time loop:

```text
if UIC and IC= was given:
  i_initial = device IC
else:
  i_initial = operating-point branch current

_flux.Value = L * i_initial
```

At each candidate timestep, `Load` behaves like:

```text
i = I(L)
Phi = L * i

allow UpdateFlux handlers to adjust Phi
_flux.Value = Phi
_flux.Derive()
info = _flux.GetContributions(L, i)

stamp node/branch KCL terms for the branch current
stamp info.Jacobian into the branch row
stamp info.Rhs into the branch RHS
```

`UpdateFlux` exists so mutual inductance can contribute coupling flux before the
time-domain branch equation is loaded. In a coupled pair, the flux through one
inductor depends partly on another inductor's current, so the self inductor's
time behavior needs a hook where that flux can be modified.

For backward Euler, the inductor idea is:

$$
v_n = L\frac{i_n-i_{n-1}}{h}
$$

or:

$$
v_n = \frac{L}{h}i_n - \frac{L}{h}i_{n-1}
$$

The branch row therefore gets a resistance-like coefficient for `i_n` plus a
known history term from the previous accepted current.

### Capacitor And Inductor Compared

| Topic | Capacitor | Inductor |
|-------|-----------|----------|
| Stored state | Charge $q = Cv$ | Flux $\Phi = Li$ |
| Time derivative | `dq/dt` is current | `dPhi/dt` is voltage |
| Solver unknown it naturally uses | Node voltage | Branch current |
| DC behavior | Open circuit | Short branch constraint |
| AC behavior | `I = sC V` | `V = sL I` |
| Transient companion | Conductance plus history current | Branch coefficient plus history RHS |
| Integration object | `IDerivative _qcap` | `IDerivative _flux` |

This is why capacitor examples focus on node voltage history, while inductor
examples focus on branch current history.

## Common Formula Shape

The selectable `.TRAN` methods all do the same kind of algebraic split. First,
name the stored quantity:

| Symbol | For a capacitor | For an inductor |
|--------|-----------------|-----------------|
| `y` | Charge `q` | Flux linkage `Phi` |
| `dy/dt` | Capacitor current `i` | Inductor voltage `v` |
| Current unknown | Terminal voltage `v_n` | Branch current `i_n` |

The integration method estimates the time derivative as:

$$
\frac{dy}{dt}\bigg|_n \approx a_0 y_n + \text{history}
$$

The $a_0 y_n$ part contains the current unknown, so it becomes a matrix
coefficient. The `history` part is already known from accepted timesteps, so it
becomes a right-hand-side source term.

For a linear capacitor, $y = q = Cv$:

$$
i_n \approx a_0 C v_n + i_{\text{history}}
$$

so:

$$
g_{\text{eq}} = a_0 C
$$

For a linear inductor, $y = \Phi = Li$:

$$
v_n \approx a_0 L i_n + v_{\text{history}}
$$

so:

$$
r_{\text{eq}} = a_0 L
$$

The method-specific formulas below only change $a_0$ and what goes into the
history term.

## Backward Euler Intuition

Backward Euler estimates the current derivative from the current value and one
previous accepted value:

$$
\frac{dy}{dt}\bigg|_n \approx \frac{y_n - y_{n-1}}{h}
$$

where `h` is the current timestep. In the common formula shape:

$$
a_0 = \frac{1}{h}
$$

and:

$$
\text{history} = -\frac{1}{h}y_{n-1}
$$

This method uses only the current unknown and one previous accepted point. That
makes it simple and robust, but it also adds numerical damping.

For a capacitor:

$$
i_n = C\frac{v_n - v_{n-1}}{h}
$$

So the companion model is:

$$
g_{\text{eq}} = \frac{C}{h}
$$

$$
i_{\text{history}} = -g_{\text{eq}}v_{n-1}
$$

For an inductor:

$$
v_n = L\frac{i_n - i_{n-1}}{h}
$$

So the companion model is:

$$
r_{\text{eq}} = \frac{L}{h}
$$

$$
v_{\text{history}} = -r_{\text{eq}}i_{n-1}
$$

Backward Euler is stable and damping. That damping can be helpful when a circuit
is stiff, but it can also hide or soften real oscillation if the step is too
large.

In this parser, `METHOD=EULER` maps to SpiceSharp `FixedEuler`, a fixed-timestep
backward Euler method.

## Trapezoidal Intuition

Trapezoidal integration averages the slope at the previous point and the current
point. For many smooth circuits, this gives better accuracy than backward Euler.

In the common stored-quantity form:

$$
\frac{dy}{dt}\bigg|_n \approx
\frac{2}{h}(y_n - y_{n-1}) -
\left(\frac{dy}{dt}\right)_{n-1}
$$

So:

$$
a_0 = \frac{2}{h}
$$

and:

$$
\text{history} =
-\frac{2}{h}y_{n-1} -
\left(\frac{dy}{dt}\right)_{n-1}
$$

The extra stored derivative term is what makes trapezoidal different from
backward Euler. It reuses the previous slope instead of only looking at the
previous value.

For a capacitor:

$$
i_n \approx \frac{2C}{h}v_n + i_{\text{history}}
$$

with:

$$
g_{\text{eq}} = \frac{2C}{h}
$$

$$
i_{\text{history}} =
-\frac{2C}{h}v_{n-1} - i_{n-1}
$$

Here $i_{n-1}$ is the previous accepted capacitor current, which is the previous
value of `dq/dt`.

For an inductor:

$$
v_n \approx \frac{2L}{h}i_n + v_{\text{history}}
$$

with:

$$
r_{\text{eq}} = \frac{2L}{h}
$$

$$
v_{\text{history}} =
-\frac{2L}{h}i_{n-1} - v_{n-1}
$$

Here $v_{n-1}$ is the previous accepted inductor voltage, which is the previous
value of `dPhi/dt`.

This often tracks smooth waveforms well. The tradeoff is that trapezoidal can
preserve point-to-point numerical ringing in stiff circuits, especially when
ideal switches, diodes, tiny resistances, and large storage elements create
abrupt changes.

## Gear Intuition

Gear methods are backward differentiation formula methods. They estimate the
current derivative from the current point and several previous accepted points.
In SpiceSharp 3.2.3, the Gear method used by this parser can use up to order 2.
At startup, after a rejected step, or when there is not enough history yet, the
active order may be lower.

Gear order 1 is the same derivative formula as backward Euler:

$$
\frac{dy}{dt}\bigg|_n \approx \frac{y_n - y_{n-1}}{h}
$$

Gear order 2 fits the derivative through the current point and two previous
accepted points. With variable timesteps:

$$
\frac{dy}{dt}\bigg|_n \approx
a_0y_n + a_1y_{n-1} + a_2y_{n-2}
$$

where:

$$
a_0 =
\frac{2h_n + h_{n-1}}{h_n(h_n + h_{n-1})}
$$

$$
a_1 =
-\frac{h_n + h_{n-1}}{h_n h_{n-1}}
$$

$$
a_2 =
\frac{h_n}{h_{n-1}(h_n + h_{n-1})}
$$

Here $h_n$ is the current timestep from $t_{n-1}$ to $t_n$, and $h_{n-1}$ is the
previous timestep from $t_{n-2}$ to $t_{n-1}$.

If the timesteps are equal, this reduces to the familiar constant-step Gear-2
formula:

$$
\frac{dy}{dt}\bigg|_n \approx
\frac{3y_n - 4y_{n-1} + y_{n-2}}{2h}
$$

For a capacitor in Gear order 2:

$$
g_{\text{eq}} = a_0 C
$$

$$
i_{\text{history}} =
C(a_1v_{n-1} + a_2v_{n-2})
$$

For an inductor in Gear order 2:

$$
r_{\text{eq}} = a_0 L
$$

$$
v_{\text{history}} =
L(a_1i_{n-1} + a_2i_{n-2})
$$

Compared with trapezoidal, Gear does not reuse the previous derivative as a
separate stored slope. It fits the derivative from accepted stored values. That
usually adds more numerical damping, which can calm stiff switching circuits and
reduce trapezoidal point-to-point artifacts.

The useful beginner picture is:

```text
Trapezoidal:
  accurate and energetic, but can ring numerically

Gear:
  more damped, often calmer for stiff switching circuits
```

Gear is often a good method to try for:

- rectifiers with large smoothing capacitors,
- switching power circuits,
- circuits with ideal switches,
- strongly nonlinear startup behavior,
- simulations that fail with timestep-too-small errors near abrupt events.

Do not use Gear as a magic fix for every problem. If Gear changes an expected
oscillation too much, reduce `tmaxstep`, check the circuit damping, and compare
with `TRAP`.

## How To Read The Example Tables

Each example table separates "what the component is" from "what the transient
engine does with it." A component can be essential to a transient result even if
it does not own integration history. For example, a resistor has no history, but
it sets the damping and time constant for a capacitor or inductor.

## Worked Example 1: RC Charging Step

```spice
RC charging step
V1 in 0 PULSE(0 5 0 1n 1n 5m 10m)
R1 in out 1k
C1 out 0 1u
.OPTIONS METHOD=TRAP
.TRAN 10u 5m 0 10u
.SAVE V(in) V(out)
.END
```

Components used:

| Component | What it represents | What it contributes in `.TRAN` | Integration/history term |
|-----------|--------------------|--------------------------------|--------------------------|
| `V1` | Input step source. | Time-dependent voltage at node `in`. | No stored history; pulse edges guide timestep choice. |
| `R1` | Charge path and damping. | Algebraic conductance between `in` and `out`. | None directly; sets `RC`. |
| `C1` | Stored electric energy at `out`. | Companion conductance plus history current. | Charge history and `dQ/dt`. |
| `out` node | Capacitor voltage being solved. | Unknown voltage in the matrix. | Its accepted values become capacitor history. |

The time constant is:

$$
\tau = RC = 1k \cdot 1u = 1ms
$$

Expected behavior:

- `V(out)` rises smoothly toward 5 V.
- Around `1 ms`, it reaches about 63 percent of the final value.
- Around `5 ms`, it is very close to the final value.

Engine view:

- `V1` changes the target from 0 V to 5 V.
- `R1` limits the current into `C1`; it does not use integration, but it defines
  the time constant.
- `C1` is the dynamic component. It asks the integration method for `geq` and a
  history current at every candidate step.
- At the first step, capacitor history comes from the operating point or initial
  condition. After each accepted step, the capacitor commits new charge history.

Integration application:

```text
C1 stores charge
integration method approximates dQ/dt
matrix sees temporary conductance + history current
accepted V(out) becomes the next history point
```

If the output looks too coarse, reduce `tmaxstep`:

```spice
.TRAN 10u 5m 0 2u
```

That asks for the same output cadence but prevents large internal jumps.

## Worked Example 2: RL Current Ramp

```spice
RL current ramp
V1 in 0 PULSE(0 5 0 1n 1n 5m 10m)
R1 in out 10
L1 out 0 10m
.OPTIONS METHOD=TRAP
.TRAN 10u 5m 0 10u
.SAVE V(out) I(L1)
.END
```

Components used:

| Component | What it represents | What it contributes in `.TRAN` | Integration/history term |
|-----------|--------------------|--------------------------------|--------------------------|
| `V1` | Input step source. | Time-dependent voltage at node `in`. | No stored history; pulse edge affects timestep choice. |
| `R1` | Current limiter and damping. | Algebraic conductance in series with `L1`. | None directly; sets `L/R`. |
| `L1` | Stored magnetic energy. | Branch-current unknown plus branch equation. | Flux history and `dPhi/dt`. |
| `I(L1)` | Inductor branch current. | Extra MNA unknown solved with node voltages. | Accepted current seeds future flux history. |

The inductor current does not jump instantly. The time constant is:

$$
\tau = \frac{L}{R} = \frac{10m}{10} = 1ms
$$

Expected behavior:

- `I(L1)` ramps upward toward `5 V / 10 ohm = 0.5 A`.
- The inductor initially takes most of the voltage.
- As current settles, inductor voltage falls toward 0 V.

Engine view:

- `V1` applies the step that tries to force current through the series path.
- `R1` limits and damps the current. It has no integration state.
- `L1` creates a branch-current variable because inductor current is the natural
  dynamic unknown.
- The inductor's stored state is flux linkage. The integration method turns
  `dPhi/dt` into the branch-equation coefficient and RHS history.

Integration application:

```text
L1 stores flux linkage
integration method approximates dPhi/dt
branch equation relates voltage and current
accepted I(L1) becomes the next history point
```

The inductor is the dual of the capacitor:

| Capacitor | Inductor |
|-----------|----------|
| Stores charge `Q`. | Stores flux linkage `Phi`. |
| State depends on voltage. | State depends on current. |
| Current is `dQ/dt`. | Voltage is `dPhi/dt`. |
| Companion looks conductance-like. | Companion looks resistance-like in a branch equation. |

## Worked Example 3: RLC Ringing And Method Choice

```spice
RLC ringing comparison
V1 in 0 PULSE(0 5 0 1n 1n 1u 10m)
R1 in tank 1
L1 tank out 10u
C1 out 0 100n
Rload out 0 1k
.OPTIONS METHOD=TRAP
.TRAN 50n 200u 0 50n
.SAVE V(out) I(L1)
.END
```

Components used:

| Component | What it represents | What it contributes in `.TRAN` | Integration/history term |
|-----------|--------------------|--------------------------------|--------------------------|
| `V1` | Short excitation pulse. | Time-dependent voltage that injects energy. | No stored history; sharp edge affects timestep choice. |
| `R1` | Source resistance. | Algebraic damping and current limiting. | None directly; damps the tank. |
| `L1` | Magnetic energy in the tank. | Branch-current equation. | Flux history and `dPhi/dt`. |
| `C1` | Electric energy in the tank. | Companion conductance and history current. | Charge history and `dQ/dt`. |
| `Rload` | Output damping/load. | Algebraic conductance to ground. | None directly; controls decay. |

Run the same circuit with:

```spice
.OPTIONS METHOD=GEAR
```

Expected comparison:

- Trapezoidal usually preserves the ringing more strongly.
- Gear usually damps the ringing more.
- If Gear removes ringing that should physically be there, reduce `tmaxstep`
  and check whether the circuit has realistic resistance.
- If trapezoidal shows small alternating point-to-point artifacts, Gear may give
  a cleaner transient.

Engine view:

- The source pulse places energy into the LC network.
- `C1` and `L1` exchange energy through their charge and flux histories.
- `R1` and `Rload` dissipate energy and make the ringing decay.
- Method choice matters because numerical damping changes how strongly the
  simulated energy exchange is preserved.

Integration application:

```text
C1 history tracks capacitor voltage/charge
L1 history tracks inductor current/flux
TRAP tends to preserve oscillatory energy
GEAR tends to damp accepted history more
```

This is the classic tradeoff: trapezoidal is often more accurate for smooth
energy exchange, while Gear is often more forgiving for stiff transitions.

## Worked Example 4: Pulse Edge Through An RC

```spice
Fast pulse through small RC
V1 in 0 PULSE(0 5 100n 1n 1n 100n 500n)
R1 in out 100
C1 out 0 10p
.OPTIONS METHOD=TRAP
.TRAN 1n 1u 0 0.5n
.SAVE V(in) V(out)
.END
```

Components used:

| Component | What it represents | What it contributes in `.TRAN` | Integration/history term |
|-----------|--------------------|--------------------------------|--------------------------|
| `V1` | Fast pulse source. | Abrupt time-dependent voltage. | No stored history; edge forces small timesteps. |
| `R1` | Source resistance. | Algebraic conductance into `C1`. | None directly; sets fast `RC`. |
| `C1` | Small output capacitance. | Companion conductance and history current. | Charge history changes rapidly near edge. |
| `.TRAN ... 0.5n` | Timestep limit. | Caps internal step size. | Helps integration resolve the 1 ns edge. |

The edge is only 1 ns. Without a small `tmaxstep`, the solver may not sample the
edge well enough for the output to look right.

Engine view:

- The source changes quickly.
- The capacitor companion conductance changes if the timestep changes.
- Newton may need more iterations.
- The truncation-error check may reject a candidate step and retry smaller.

Integration application:

```text
V1 creates the fast event
C1 is the only stored-energy component
small tmaxstep limits how far integration can jump
accepted points preserve the fast charge trajectory
```

Rejected steps are normal. They mean the simulator is protecting accuracy.

## Worked Example 5: Rectifier With Smoothing Capacitor

```spice
Half-wave rectifier with smoothing capacitor
V1 ac 0 SIN(0 12 50)
D1 ac out Dmod
Rload out 0 1k
C1 out 0 100u
.MODEL Dmod D(IS=1e-14)
.OPTIONS METHOD=GEAR
.TRAN 100u 100m 0 100u
.SAVE V(ac) V(out) I(D1)
.END
```

Components used:

| Component | What it represents | What it contributes in `.TRAN` | Integration/history term |
|-----------|--------------------|--------------------------------|--------------------------|
| `V1` | Sine input source. | Time-dependent sinusoidal voltage. | No stored history. |
| `D1` | One-way charging path. | Current equation plus Newton `dI/dV`. | Turn-on affects timestep/convergence. |
| `C1` | Smoothing reservoir. | Companion conductance and history current. | Charge history and `dQ/dt`. |
| `Rload` | Discharge path/load. | Algebraic conductance to ground. | None directly; sets discharge rate. |

This circuit is harder than a simple RC:

- The diode conducts only during part of the input cycle.
- The capacitor charges quickly through the diode.
- The capacitor discharges slowly through the load.
- Around diode turn-on and turn-off, the nonlinear diode derivative changes
  quickly.

Engine view:

- `V1` moves continuously, but conduction is not continuous.
- `D1` contributes nonlinear current and `dI/dV` conductance for Newton. Near
  turn-on, small voltage changes can cause large current changes.
- `C1` contributes `geq` and history current from integration. It remembers the
  previous output charge between charging pulses.
- `Rload` slowly drains the capacitor between diode conduction windows.
- The solver may reduce timesteps near the short charging pulses.

Integration application:

```text
diode decides when C1 can charge
C1 history carries output voltage between sine peaks
Rload discharge changes charge slowly
GEAR can calm the stiff fast-charge/slow-discharge mix
```

Gear is often useful here because the circuit is stiff: a short high-current
charging interval is mixed with a long slow discharge interval.

## Worked Example 6: Nonlinear Capacitor `Q=...`

`Q=` capacitors require the custom component mappings enabled by
`UseCustomComponents()`.

```spice
Nonlinear charge-defined capacitor
V1 in 0 PULSE(0 5 0 1n 1n 5m 10m)
R1 in out 1k
C1 out 0 Q=1u*x+100n*x*x
.OPTIONS METHOD=TRAP
.TRAN 10u 5m 0 10u
.SAVE V(out) @C1[q] @C1[capacitance] @C1[dqdt]
.END
```

Components used:

| Component | What it represents | What it contributes in `.TRAN` | Integration/history term |
|-----------|--------------------|--------------------------------|--------------------------|
| `V1` | Input step source. | Time-dependent voltage at `in`. | No stored history. |
| `R1` | Charge path. | Algebraic conductance into the nonlinear capacitor. | None directly; sets drive strength. |
| `C1 Q=...` | Voltage-defined stored charge. | Evaluates `Q(V)` and local `dQ/dV`. | `dqdt` from charge history. |
| `@C1[...]` exports | Internal state observation. | Reports charge, slope, and time derivative. | Shows what integration is using. |

Here `x` is the capacitor voltage. The stored charge is:

$$
Q(V) = 1u \cdot V + 100n \cdot V^2
$$

The local incremental capacitance is:

$$
C_{\text{inc}} = \frac{dQ}{dV} = 1u + 200n \cdot V
$$

Engine view:

```text
evaluate voltage V
evaluate charge Q(V)
evaluate local derivative dQ/dV
store Q in an IDerivative state
call Derive() to compute dQ/dt
call GetContributions(dQ/dV, V)
stamp Jacobian and RHS history terms
```

Integration application:

```text
Q(V) is the stored state
dQ/dV is the local capacitance used in the Jacobian
dQ/dt is the current produced by integration history
GetContributions(...) returns the matrix/RHS numbers for this step
```

The exported values mean:

| Export | Meaning |
|--------|---------|
| `@C1[q]` | Present stored charge. |
| `@C1[capacitance]` | Present incremental capacitance `dQ/dV`. |
| `@C1[dqdt]` | Time derivative of charge, which is capacitor current. |

This example is a good way to see that transient analysis does not treat the
expression after `Q=` as a capacitance. It treats it as stored charge.

## Worked Example 7: Nonlinear Inductor `Flux=...`

`Flux=` inductors also require `UseCustomComponents()`.

```spice
Nonlinear flux-defined inductor
V1 in 0 PULSE(0 5 0 1n 1n 5m 10m)
R1 in out 10
L1 out 0 Flux=10m*tanh(x)
.OPTIONS METHOD=TRAP
.TRAN 10u 5m 0 10u
.SAVE I(L1) @L1[flux] @L1[inductance] @L1[dfluxdt]
.END
```

Components used:

| Component | What it represents | What it contributes in `.TRAN` | Integration/history term |
|-----------|--------------------|--------------------------------|--------------------------|
| `V1` | Input step source. | Time-dependent voltage at `in`. | No stored history. |
| `R1` | Current limiter and damping. | Algebraic conductance in series. | None directly; sets drive strength. |
| `L1 Flux=...` | Current-defined stored flux. | Evaluates `Phi(I)` and local `dPhi/dI`. | `dfluxdt` from flux history. |
| `I(L1)` | Branch-current unknown. | MNA variable solved with node voltages. | Accepted current seeds future flux. |
| `@L1[...]` exports | Internal state observation. | Reports flux, slope, and time derivative. | Shows what integration is using. |

Here `x` is the inductor branch current. The stored flux linkage is:

$$
\Phi(I) = 10m \cdot \tanh(I)
$$

The local incremental inductance is:

$$
L_{\text{inc}} = \frac{d\Phi}{dI}
$$

As current grows, `tanh(I)` flattens, so the slope can become smaller. That means
the incremental inductance changes during the transient.

Engine view:

```text
evaluate branch current I
evaluate flux Phi(I)
evaluate local derivative dPhi/dI
store Phi in an IDerivative state
call Derive() to compute dPhi/dt
call GetContributions(dPhi/dI, I)
stamp the branch equation
```

Integration application:

```text
Phi(I) is the stored state
dPhi/dI is the local inductance used in the branch Jacobian
dPhi/dt is the voltage produced by integration history
GetContributions(...) returns the branch coefficient/RHS for this step
```

The exported values mean:

| Export | Meaning |
|--------|---------|
| `@L1[flux]` | Present stored flux linkage. |
| `@L1[inductance]` | Present incremental inductance `dPhi/dI`. |
| `@L1[dfluxdt]` | Time derivative of flux, which is inductor voltage. |

## Worked Example 8: UIC Initial Conditions

Initial conditions matter because they seed integration history.

```spice
Capacitor starts charged
V1 in 0 0
R1 in out 1k
C1 out 0 1u IC=5
.TRAN 10u 5m 0 10u UIC
.SAVE V(out)
.END
```

With `UIC`, SpiceSharp skips the DC operating point and initializes the
capacitor from `IC=5`. The first accepted transient point starts from stored
charge consistent with 5 V.

For an inductor:

```spice
Inductor starts with current
V1 in 0 0
R1 in out 10
L1 out 0 10m IC=100m
.TRAN 10u 5m 0 10u UIC
.SAVE I(L1)
.END
```

Components used across the two `UIC` cases:

| Component | What it represents | What it contributes in `.TRAN` | Integration/history term |
|-----------|--------------------|--------------------------------|--------------------------|
| `V1` | Zero source/reference drive. | Keeps the external drive fixed at 0 V. | No stored history. |
| `R1` | Discharge path or damping path. | Algebraic conductance. | None directly; sets decay rate. |
| `C1 IC=5` | Initially charged capacitor. | Starts with terminal voltage from `IC`. | Initial charge history. |
| `L1 IC=100m` | Initially energized inductor. | Starts with branch current from `IC`. | Initial flux history. |
| `UIC` | Initial-condition mode. | Skips DC operating point initialization. | Uses device `.IC` values to seed dynamic state. |

Engine view:

- `UIC` changes initialization, not the later timestep loop.
- `C1 IC=5` gives the capacitor an initial terminal voltage, so its first charge
  history is not zero.
- `L1 IC=100m` gives the inductor an initial branch current, so its first flux
  history is not zero.
- `R1` has no history, but it controls how quickly the stored energy decays.

Without `UIC`, the simulator first solves the DC operating point. Device `IC=`
values may still be parameters, but `.IC` node-voltage statements are only used
as transient initial conditions when `UIC` is specified.

Integration application:

```text
UIC skips the usual operating-point history seed
capacitor IC becomes initial charge history
inductor IC becomes initial flux history
first accepted timestep builds from those stored states
```

## Practical Method Guide

| Situation | First try | If it looks wrong or fails |
|-----------|-----------|----------------------------|
| Smooth RC/RL settling | `METHOD=TRAP` | Reduce `tmaxstep`; compare with `GEAR`. |
| Resonant LC/RLC behavior | `METHOD=TRAP` | Add realistic resistance; reduce `tmaxstep`; compare with `GEAR`. |
| Rectifiers and large capacitors | `METHOD=GEAR` | Reduce `tmaxstep` around charging pulses. |
| Ideal switches or abrupt behavioral expressions | `METHOD=GEAR` | Smooth the expression or add realistic parasitics. |
| Need predictable fixed stepping | `METHOD=EULER` | Use a small enough `.TRAN` step and check accuracy carefully. |
| Timestep too small | `METHOD=GEAR` | Also inspect discontinuities, impossible ideal loops, and extreme values. |

Good transient simulation is usually a balance:

- choose a method that matches the circuit behavior,
- keep `tmaxstep` small enough to resolve the fastest important event,
- avoid unrealistic ideal discontinuities when possible,
- compare methods when the numerical method might affect the conclusion.

## Common Misunderstandings

| Misunderstanding | Better mental model |
|------------------|---------------------|
| `.TRAN 1u 1m` means every internal step is exactly 1 us. | `1u` is an output/initial step hint; adaptive methods may choose other internal steps. |
| A capacitor is just a resistor during transient. | It is a companion conductance plus history current for one solve attempt. |
| Gear is always more correct. | Gear is more damped; that can help stiff circuits but can also damp real oscillation. |
| Trapezoidal ringing is always physical. | Some point-to-point ringing can be numerical, especially in stiff idealized circuits. |
| `Q=` means capacitance. | `Q=` means stored charge; capacitance is the derivative `dQ/dV`. |
| `Flux=` means inductance. | `Flux=` means stored flux linkage; inductance is the derivative `dPhi/dI`. |
| Rejected timesteps are failures. | Rejected timesteps are normal accuracy control. |

## Related Articles

- [.TRAN](tran.md) - transient statement syntax.
- [.OPTIONS](options.md) - selecting `METHOD=TRAP`, `METHOD=GEAR`, or `METHOD=EULER`.
- [LTspice-Style Nonlinear Passives](nonlinear-passives.md) - `Q=` and `Flux=` custom components.
- [How SpiceSharp Solves Circuits](spicesharp-architecture.md) - MNA, solver loops, and architecture details.
