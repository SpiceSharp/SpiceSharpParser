# Transient Integration Methods

Transient analysis (`.TRAN`) answers one question:

```text
What does the circuit do as time moves forward?
```

That sounds simple, but capacitors, inductors, semiconductor charges, transfer
functions, delays, and waveform sources all depend on time. SpiceSharp cannot
put "change over time" directly into the modified nodal analysis (MNA) matrix.
For each candidate timestep it first converts time derivatives into algebraic
companion models: matrix coefficients plus known history terms.

This guide explains that conversion from the outside in:

| If you want to understand... | Start with |
|------------------------------|------------|
| The transient solve loop | [The Basic .TRAN Loop](#the-basic-tran-loop) |
| Why timesteps can be rejected | [Candidate Steps And Truncation Error](#candidate-steps-and-truncation-error) |
| How dynamic devices become MNA stamps | [Companion Model Families](#companion-model-families) |
| What `IIntegrationMethod` and `IDerivative` do | [Derivatives And Integration History](#derivatives-and-integration-history) |
| How ordinary `C` and `L` elements work | [Built-In Capacitor And Inductor Integration](#built-in-capacitor-and-inductor-integration) |
| What method and timestep to choose | [Method And Timestep Selection](#method-and-timestep-selection) |

The main mental model is:

```text
stored charge/flux history
  -> integration method
  -> current matrix coefficient + known RHS history term
  -> Newton/MNA solve for this candidate time
```

## The Basic .TRAN Loop

A transient simulation is not one solve. It is many biasing solves inside a time
loop. Each point may require several Newton iterations, and some attempted
points are rejected before the simulator moves on.

```text
initialize:
  solve the DC operating point, or use initial conditions with UIC
  initialize capacitor, inductor, waveform, and internal dynamic histories

while time < stop time:
  choose a candidate timestep
  prepare dynamic states and waveform values for that candidate time

  repeat Newton iterations:
    clear matrix and RHS
    load device equations, local derivatives, and history terms
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
    retry from the last accepted time
```

The important word is **candidate**. SpiceSharp may try a timestep, solve the
MNA equations at that candidate time, decide the step was too inaccurate, reject
it, and try again with a smaller step. Capacitor charge, inductor flux,
waveform state, and other dynamic histories are committed only after a timestep
is accepted.

## Candidate Steps And Truncation Error

Newton convergence and timestep accuracy are separate checks.

| Check | Main question | Main inputs |
|-------|---------------|-------------|
| Newton convergence | Do the equations balance at this candidate time? | KCL, KVL, device equations, residuals, and variable changes. |
| Truncation-error check | Was this time jump accurate enough? | Dynamic history, derivative estimates, timestep size, and local error estimates. |

Newton works vertically at one candidate time:

```text
given candidate time t[n]
given companion models for this h
find V(nodes) and I(branches)
make KCL/KVL/device equations balance
```

Truncation-error control works horizontally across time:

```text
given accepted time t[n-1]
given candidate time t[n]
compare dynamic history and derivative estimates
decide whether this time jump is accurate enough
```

Those checks can disagree. This is possible:

```text
Newton converged
but truncation error is too high
```

That means the candidate voltages and currents are internally consistent, but
the path from the previous accepted point to those values is not trusted. The
simulator rejects the candidate, keeps the previous accepted histories, reduces
the timestep, and tries again.

Common causes of high truncation error include:

- sharp `PULSE` or `PWL` edges,
- diode turn-on or turn-off,
- switch state changes,
- short capacitor charging current spikes,
- inductor current ramps that change slope quickly,
- LC or RLC ringing that is too fast for the current timestep,
- `tmaxstep` much larger than the fastest important circuit event.

A simple mental model:

```text
Newton failure:
  "I could not solve the equations at this candidate time."

truncation failure:
  "I solved them, but I do not trust this time jump."
```

Rejected timesteps are normal. They are part of the accuracy control loop, not
exported simulation results.

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

The matrix solver wants equations that look like:

```text
coefficient * current unknown + known RHS value
```

It does not solve symbolic time derivatives directly. An integration method
replaces a derivative with an algebraic approximation based on:

- the current unknown value,
- the current timestep size,
- previous accepted values,
- previous accepted derivatives or stored history.

For a capacitor, the engine builds a temporary relation like:

$$
i_n \approx g_{\text{eq}}v_n + i_{\text{history}}
$$

For an inductor, it builds a branch relation like:

$$
v_n \approx r_{\text{eq}}i_n + v_{\text{history}}
$$

Those temporary relations are **companion models**. They are valid for one
solve attempt and are rebuilt when the timestep, accepted history, or Newton
guess changes.

## How .TRAN Uses The Modified Matrix Algorithm

The MNA matrix algorithm does not disappear during transient simulation.
`.TRAN` uses the same matrix idea repeatedly:

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

The signs in this example follow the stated branch-current convention. Other
device implementations may use an opposite branch orientation, but the split is
the same: current unknowns contribute to the matrix, accepted history
contributes to the RHS.

## Companion Model Families

A companion model is a temporary linear equivalent that a device loads into the
matrix for the current solve. It is temporary because it is valid only for:

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
| What becomes a matrix coefficient? | `dq/dv` times the integration coefficient. | `dPhi/di` times the integration coefficient. |
| What becomes RHS/history? | Previous accepted charge/voltage information. | Previous accepted flux/current information. |

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
source. The capacitor has not become a permanent resistor. Its storage behavior
is represented by the history source and by the fact that `g_eq` depends on the
timestep.

For a capacitor between nodes `p` and `n`, the conductance part contributes to
the Jacobian like this:

| Matrix entry | Contribution |
|--------------|--------------|
| `Y[p,p]` | `+geq` |
| `Y[p,n]` | `-geq` |
| `Y[n,p]` | `-geq` |
| `Y[n,n]` | `+geq` |

The history current contributes to the RHS with opposite signs on the two
terminal nodes. The exact signs depend on the branch-current convention.

### Inductor Companion Model

For a linear inductor:

$$
\Phi = L i
$$

and:

$$
v = \frac{d\Phi}{dt}
$$

The integration method turns that into:

$$
v_n \approx r_{\text{eq}} i_n + v_{\text{hist}}
$$

For backward Euler:

$$
r_{\text{eq}} = \frac{L}{h}
$$

and:

$$
v_{\text{hist}} = -\frac{L}{h}i_{n-1}
$$

MNA usually introduces a branch-current variable for an inductor. The node
equations connect terminal voltage to branch current, while the branch equation
receives the resistance-like coefficient and history term.

### Nonlinear And Source Companions

Transient integration is not the only place companion thinking appears.

| Device or behavior | Companion idea |
|--------------------|----------------|
| Diode | Newton replaces the nonlinear curve with local `dI/dV` conductance plus an equivalent current source. |
| Switch | The effective conductance changes with control state; abrupt changes can force smaller timesteps. |
| Waveform source | The source value is known at the candidate time and loads into the RHS or source constraint. |
| `Q=` capacitor | Local `dQ/dV` and integration history both contribute. |
| `Flux=` inductor | Local `dPhi/dI` and integration history both contribute. |

The common shape is always:

```text
current unknown sensitivity -> matrix/Jacobian
known value or accepted history -> RHS
```

## Derivatives And Integration History

There are two different derivative ideas in transient simulation.

### Local Linearization Derivatives

Nonlinear devices are solved by Newton iteration. At each Newton guess, a
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

### Time Derivatives And Integration States

The transient engine also needs derivatives with respect to time:

| Time derivative | Physical meaning |
|-----------------|------------------|
| `dQ/dt` | Capacitor current. |
| `dPhi/dt` | Inductor voltage. |
| `dv/dt` | Voltage rate of change used by capacitive behavior. |
| `di/dt` | Current rate of change used by inductive behavior. |

These derivatives are not just local slopes. They depend on previous accepted
timesteps. The integration method owns that history.

SpiceSharp exposes the bridge through integration states. For charge-defined
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

### Common Formula Shape

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

The method-specific formulas change $a_0$ and what goes into the history term.

### Backward Euler Intuition

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

In this parser, `METHOD=EULER` maps to SpiceSharp `FixedEuler`, a fixed-step
backward Euler method.

### Trapezoidal Intuition

Trapezoidal integration averages the slope at the previous point and the
current point. For many smooth circuits, this gives better accuracy than
backward Euler.

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
backward Euler. It often tracks smooth waveforms well, but it can preserve
point-to-point numerical ringing in stiff idealized circuits.

### Gear Intuition

Gear methods are backward differentiation formula methods. They estimate the
current derivative from the current point and accepted stored values. In
SpiceSharp 3.2.3, the Gear method used by this parser can use up to order 2.
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

The exact coefficients depend on the current and previous timestep sizes. With
equal timesteps, this reduces to:

$$
\frac{dy}{dt}\bigg|_n \approx
\frac{3y_n - 4y_{n-1} + y_{n-2}}{2h}
$$

Compared with trapezoidal, Gear does not reuse the previous derivative as a
separate stored slope. It fits the derivative from accepted stored values. That
usually adds more numerical damping, which can calm stiff switching circuits
and reduce trapezoidal point-to-point artifacts.

## Method And Timestep Selection

Method choice and timestep limits control different parts of the transient
solve. The method decides how derivative history becomes matrix/RHS data.
`tmaxstep` decides how far the internal solver is allowed to jump.

### Method Selection

SpiceSharpParser selects the integration method with `.OPTIONS METHOD=...`.

| Netlist option | SpiceSharp method | Main character |
|----------------|-------------------|----------------|
| `METHOD=TRAP` | `Trapezoidal` | Accurate for many smooth circuits, but can show numerical ringing in stiff circuits. |
| `METHOD=TRAPEZOIDAL` | `Trapezoidal` | Same as `METHOD=TRAP`. |
| `METHOD=GEAR` | `Gear` | More numerical damping, often useful for stiff or switching circuits. |
| `METHOD=EULER` | `FixedEuler` | Fixed-step backward Euler; simple and damped, but usually less accurate. |

Example:

```spice
RC charging with Gear integration
V1 in 0 PULSE(0 5 0 1n 1n 5m 10m)
R1 in out 1k
C1 out 0 1u
.OPTIONS METHOD=GEAR
.TRAN 10u 5m 10u
.SAVE V(out)
.END
```

If no method is specified, transient construction uses SpiceSharp's normal
transient defaults or the explicit time-parameter object created by the parser.
In practice, `TRAP` is the usual first method to try, and `GEAR` is a common
second try when stiff switching behavior or trapezoidal ringing appears.

### Timestep Terms

SpiceSharpParser supports these ordinary `.TRAN` numeric forms:

```spice
.TRAN <tstep> <tstop> [UIC]
.TRAN <tstep> <tstop> <tmaxstep> [UIC]
.TRAN <tstep> <tstop> <tstart> <tmaxstep> [UIC]
```

| Term | Meaning |
|------|---------|
| `tstep` | Output cadence and initial timestep hint. |
| `tstop` | End time. |
| `tmaxstep` | Maximum internal timestep when supplied. |
| `tstart` | Time before which output is not saved; available only in the 4-number form. |
| `UIC` | Use initial conditions instead of solving the DC operating point first. |

The three-number form is `tstep`, `tstop`, `tmaxstep`. It is not `tstart`.
Use the four-number form when you need both `tstart` and `tmaxstep`.

Do not read `tstep` as "the solver will always step exactly this much."
Adaptive methods may use smaller or different internal timesteps. `tmaxstep` is
the knob that prevents the solver from jumping over events or waveform detail.

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

Here `tstart=0` and `tmaxstep=0.5n`, so the internal solver is kept from
leaping across a 1 ns edge.

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

### Detailed Walkthrough: Derivative To Matrix Terms

Backward Euler is the simplest example because it uses one previous accepted
point. Other methods use different coefficients and more history, but they
still make the same split:

```text
current unknown part -> matrix/Jacobian
accepted history     -> RHS/history source
```

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

Substitute $q = Cv$ and rearrange:

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

Substitute $\Phi = Li$ and rearrange:

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
terminal voltage to branch current:

$$
V(p) - V(n) \approx \frac{L}{h}I(L) - \frac{L}{h}i_{n-1}
$$

### Tiny Numerical Examples

These examples use backward Euler because it has the smallest amount of
history. They are not special SpiceSharp syntax; they are the arithmetic that
happens behind a normal `C` or `L` element during one candidate timestep.

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

### Built-In Inductor Behavior Stack

For a normal `L` element, SpiceSharp 3.2.3 uses these behavior classes:

| Behavior | Used in | What it does |
|----------|---------|--------------|
| `Inductors.Temperature` | setup/temperature | Computes the effective inductance. |
| `Inductors.Biasing` | `.OP`, `.DC`, `.TRAN` bias load | Creates the branch-current unknown and ideal branch constraint. |
| `Inductors.Frequency` | `.AC` | Stamps `V = sL I` into the complex branch equation. |
| `Inductors.Time` | `.TRAN` | Stores flux in `IDerivative _flux` and stamps the transient branch companion. |

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
time-domain branch equation is loaded.

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

## Custom Charge And Flux Components

The same integration pattern appears in SpiceSharpParser custom nonlinear
passives.

For `Q=` capacitors, the component evaluates:

```text
voltage V
charge Q(V)
local derivative dQ/dV
time derivative dQ/dt from integration history
```

The local derivative `dQ/dV` becomes incremental capacitance in the Jacobian.
The time derivative `dQ/dt` is capacitor current.

For `Flux=` inductors, the component evaluates:

```text
branch current I
flux Phi(I)
local derivative dPhi/dI
time derivative dPhi/dt from integration history
```

The local derivative `dPhi/dI` becomes incremental inductance in the branch
Jacobian. The time derivative `dPhi/dt` is inductor voltage.

The implementation pattern is the same as the built-in devices:

```csharp
state.Value = storedQuantity;
state.Derive();
JacobianInfo info = state.GetContributions(localDerivative, currentUnknown);
```

## Worked Examples

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
.TRAN 10u 5m 10u
.SAVE V(in) V(out)
.END
```

| Component | What it represents | What it contributes in `.TRAN` | Integration/history term |
|-----------|--------------------|--------------------------------|--------------------------|
| `V1` | Input step source. | Time-dependent voltage at node `in`. | No stored history; pulse edges guide timestep choice. |
| `R1` | Charge path and damping. | Algebraic conductance between `in` and `out`. | None directly; sets `RC`. |
| `C1` | Stored electric energy at `out`. | Companion conductance plus history current. | Charge history and `dQ/dt`. |
| `out` node | Capacitor voltage being solved. | Unknown voltage in the matrix. | Accepted values become capacitor history. |

The time constant is:

$$
\tau = RC = 1k \cdot 1u = 1ms
$$

Expected behavior:

- `V(out)` rises smoothly toward 5 V.
- Around `1 ms`, it reaches about 63 percent of the final value.
- Around `5 ms`, it is very close to the final value.

Integration application:

```text
C1 stores charge
integration method approximates dQ/dt
matrix sees temporary conductance + history current
accepted V(out) becomes the next history point
```

## Worked Example 2: RL Current Ramp

```spice
RL current ramp
V1 in 0 PULSE(0 5 0 1n 1n 5m 10m)
R1 in out 10
L1 out 0 10m
.OPTIONS METHOD=TRAP
.TRAN 10u 5m 10u
.SAVE V(out) I(L1)
.END
```

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

## Worked Example 3: RLC Ringing And Method Choice

```spice
RLC ringing comparison
V1 in 0 PULSE(0 5 0 1n 1n 1u 10m)
R1 in tank 1
L1 tank out 10u
C1 out 0 100n
Rload out 0 1k
.OPTIONS METHOD=TRAP
.TRAN 50n 200u 50n
.SAVE V(out) I(L1)
.END
```

Run the same circuit with:

```spice
.OPTIONS METHOD=GEAR
```

Expected comparison:

- Trapezoidal usually preserves the ringing more strongly.
- Gear usually damps the ringing more.
- If Gear removes ringing that should physically be there, reduce `tmaxstep`
  and check whether the circuit has realistic resistance.
- If trapezoidal shows small alternating point-to-point artifacts, Gear may
  give a cleaner transient.

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

The edge is only 1 ns. Without a small `tmaxstep`, the solver may not sample the
edge well enough for the output to look right.

Engine view:

- The source changes quickly.
- The capacitor companion conductance changes if the timestep changes.
- Newton may need more iterations.
- The truncation-error check may reject a candidate step and retry smaller.

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
.TRAN 100u 100m 100u
.SAVE V(ac) V(out) I(D1)
.END
```

This circuit is harder than a simple RC:

- The diode conducts only during part of the input cycle.
- The capacitor charges quickly through the diode.
- The capacitor discharges slowly through the load.
- Around diode turn-on and turn-off, the nonlinear diode derivative changes
  quickly.

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
.TRAN 10u 5m 10u
.SAVE V(out) @C1[q] @C1[capacitance] @C1[dqdt]
.END
```

Here `x` is the capacitor voltage. The stored charge is:

$$
Q(V) = 1u \cdot V + 100n \cdot V^2
$$

The local incremental capacitance is:

$$
C_{\text{inc}} = \frac{dQ}{dV} = 1u + 200n \cdot V
$$

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
.TRAN 10u 5m 10u
.SAVE I(L1) @L1[flux] @L1[inductance] @L1[dfluxdt]
.END
```

Here `x` is the inductor branch current. The stored flux linkage is:

$$
\Phi(I) = 10m \cdot \tanh(I)
$$

The local incremental inductance is:

$$
L_{\text{inc}} = \frac{d\Phi}{dI}
$$

As current grows, `tanh(I)` flattens, so the slope can become smaller. That
means the incremental inductance changes during the transient.

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
.TRAN 10u 5m 10u UIC
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
.TRAN 10u 5m 10u UIC
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

`UIC` changes initialization, not the later timestep loop. Without `UIC`, the
simulator first solves the DC operating point. Device `IC=` values may still be
parameters, but `.IC` node-voltage statements are only used as transient
initial conditions when `UIC` is specified.

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
| `.TRAN 1u 1m 100n` sets `tstart=100n`. | In this parser, the three-number form sets `tmaxstep=100n`; use four numbers for `tstart`. |
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
