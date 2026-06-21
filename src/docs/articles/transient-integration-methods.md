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

```text
matrix * unknowns = right-hand side
```

Capacitors and inductors are not purely algebraic. They contain derivatives:

```text
capacitor: i = C * dv/dt
inductor:  v = L * di/dt
```

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

```text
q = C * v
i = dq/dt
```

At timestep `n`, backward Euler estimates:

$$
i_n \approx \frac{q_n - q_{n-1}}{h}
$$

Substitute `q = C*v`:

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

```text
Phi = L * i
v = dPhi/dt
```

At timestep `n`, backward Euler estimates:

$$
v_n \approx \frac{\Phi_n - \Phi_{n-1}}{h}
$$

Substitute `Phi = L*i`:

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

```text
C = 1 uF
h = 1 ms
previous accepted capacitor voltage = 2 V
current unknown capacitor voltage = v[n]
```

Backward Euler gives:

$$
i_n = \frac{C}{h}v_n - \frac{C}{h}v_{n-1}
$$

Compute the coefficient:

```text
C / h = 1e-6 / 1e-3 = 0.001 S
```

So the capacitor current equation for this candidate timestep is:

```text
i[n] = 0.001 * v[n] - 0.001 * 2
i[n] = 0.001 * v[n] - 0.002 A
```

Read it as a stamp:

| Part | Meaning |
|------|---------|
| `0.001 * v[n]` | Matrix coefficient, equivalent to a 1 mS conductance. |
| `-0.002 A` | History current from the previous accepted 2 V. |

If Newton eventually solves this timestep with:

```text
v[n] = 2.5 V
```

then:

```text
i[n] = 0.001 * 2.5 - 0.002
i[n] = 0.0005 A
```

The charge view says the same thing:

```text
q[n-1] = C * 2.0 = 2.0 uC
q[n]   = C * 2.5 = 2.5 uC
dq     = 0.5 uC
dq / h = 0.5 uC / 1 ms = 0.5 mA
```

That is why the companion model is not a trick separate from physics. It is the
same charge law rewritten into a form the matrix solver can use.

Inductor example:

```text
L = 10 mH
h = 1 ms
previous accepted inductor current = 0.2 A
current unknown branch current = i[n] = I(L)
```

Backward Euler gives:

$$
v_n = \frac{L}{h}i_n - \frac{L}{h}i_{n-1}
$$

Compute the coefficient:

```text
L / h = 10e-3 / 1e-3 = 10 ohm
```

So the branch relation for this candidate timestep is:

```text
V(p) - V(n) = 10 * I(L) - 10 * 0.2
V(p) - V(n) = 10 * I(L) - 2 V
```

Read it as a stamp:

| Part | Meaning |
|------|---------|
| `V(p) - V(n)` | Current node-voltage unknowns in the branch row. |
| `10 * I(L)` | Matrix coefficient for the inductor branch-current unknown. |
| `-2 V` | History voltage term from the previous accepted 0.2 A. |

If Newton eventually solves this timestep with:

```text
I(L) = 0.25 A
```

then:

```text
V(p) - V(n) = 10 * 0.25 - 2
V(p) - V(n) = 0.5 V
```

The flux view says the same thing:

```text
Phi[n-1] = L * 0.20 = 0.0020
Phi[n]   = L * 0.25 = 0.0025
dPhi     = 0.0005
dPhi / h = 0.0005 / 0.001 = 0.5 V
```

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
| Stored state | Charge `q = C*v` | Flux `Phi = L*i` |
| Time derivative | `dq/dt` is current | `dPhi/dt` is voltage |
| Solver unknown it naturally uses | Node voltage | Branch current |
| DC behavior | Open circuit | Short branch constraint |
| AC behavior | `I = sC V` | `V = sL I` |
| Transient companion | Conductance plus history current | Branch coefficient plus history RHS |
| Integration object | `IDerivative _qcap` | `IDerivative _flux` |

This is why capacitor examples focus on node voltage history, while inductor
examples focus on branch current history.

## Backward Euler Intuition

Backward Euler estimates the current derivative from the current value and one
previous accepted value:

$$
\frac{dx}{dt}\bigg|_n \approx \frac{x_n - x_{n-1}}{h}
$$

where `h` is the timestep.

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

Backward Euler is stable and damping. That damping can be helpful when a circuit
is stiff, but it can also hide or soften real oscillation if the step is too
large.

In this parser, `METHOD=EULER` maps to SpiceSharp `FixedEuler`, a fixed-timestep
backward Euler method.

## Trapezoidal Intuition

Trapezoidal integration averages the slope at the previous point and the current
point. For many smooth circuits, this gives better accuracy than backward Euler.

For a capacitor, the current can be written conceptually as:

$$
i_n \approx \frac{2C}{h}v_n + i_{\text{history}}
$$

where the history term includes the previous voltage and previous capacitor
current. Compared with backward Euler, trapezoidal uses more information from
the last accepted point.

This often tracks smooth waveforms well. The tradeoff is that trapezoidal can
show numerical ringing in stiff circuits, especially when ideal switches,
diodes, tiny resistances, and large storage elements create abrupt changes.

## Gear Intuition

Gear methods are backward differentiation formula methods. They estimate the
current derivative from the current point and several previous accepted points.
First-order Gear behaves like backward Euler; higher-order Gear uses more
history.

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
