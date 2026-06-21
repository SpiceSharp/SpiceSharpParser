# How SpiceSharp Solves Circuits: Junior Developer Guide

This guide is written for a developer who can read C# but does not yet feel comfortable
with circuit math or electronics vocabulary. You do not need an electrical engineering
background to follow it. The goal is to explain what SpiceSharp is doing, why it needs a
matrix, and how the pieces fit together in software terms.

SpiceSharpParser reads a SPICE netlist, but the numerical circuit solution is done by
SpiceSharp. This article explains the handoff and then looks inside the engine:
modified nodal analysis, sparse matrix solving, Newton iteration, transient integration,
and the matrix contribution of each supported component category.

The short version is:

```text
SPICE text
  -> SpiceNetlistParser.ParseNetlist()
  -> parse-tree model
  -> SpiceSharpReader.Read()
  -> SpiceSharp Circuit + Simulations + Exports
  -> simulation.Execute(circuit)
  -> SpiceSharp builds and solves sparse matrix equations
```

SpiceSharpParser creates the `Circuit`, simulation objects such as `OP`, `DC`, `AC`, and
`Transient`, and export objects. The sparse matrix, Newton loops, timestep loops, and
linear algebra live in the SpiceSharp dependency.

## How To Read This Guide

If you are new to electronics, read in this order:

1. **Vocabulary**: learn the words that appear everywhere.
2. **Main Objects**: map those words to SpiceSharp classes.
3. **Modified Nodal Analysis**: understand why a circuit becomes equations.
4. **Sparse Matrix And Solver Internals**: understand how those equations are stored and solved.
5. **Component Stamp Atlas**: use it as a reference, not as something to memorize.

The formulas are there because the simulator ultimately solves numbers. You can still
understand the architecture if you treat each formula as a recipe for where a component
adds values into a table.

## Tiny Electronics Vocabulary

| Term | Plain meaning |
|------|---------------|
| Circuit | A graph of components connected by wires. |
| Node | A wire connection point. All points on the same ideal wire have the same voltage. |
| Ground / `0` | The reference node. Its voltage is defined as 0 V. |
| Voltage | Electrical "level" at a node, measured relative to ground or another node. |
| Current | Flow through a component branch. |
| Resistance | How strongly a resistor opposes current. Higher resistance means less current. |
| Conductance | The inverse of resistance: $g = 1 / R$. Higher conductance means more current. |
| Source | A component that injects a known voltage or current. |
| Operating point | The steady DC solution before time-varying or AC effects. |
| Nonlinear | A device whose behavior is not a straight-line relation. Diodes and transistors are nonlinear. |
| Newton iteration | A repeated guessing process for solving nonlinear equations. |
| Jacobian | A matrix of local slopes/derivatives used by Newton iteration. |
| Transient | Time-domain simulation: what happens as time moves forward. |
| AC | Small-signal frequency-domain simulation. |
| Matrix | A table of coefficients used to solve many equations at once. |
| RHS | Right-hand side vector: the known values in the equation system. |
| Stamp | A small component contribution added into the big global matrix/RHS. |

The most important mental model:

```text
circuit components
  -> many small local rules
  -> one big table of equations
  -> solver finds node voltages and branch currents
```

If you know basic software data structures, think of the circuit as a graph and the
solver matrix as an indexed table built from that graph.

## Main Objects

| Concept | Role | Software analogy |
|---------|------|------------------|
| `Circuit` | Collection of SpiceSharp entities created from netlist components and models. | Object graph. |
| `Simulation` | Analysis runner: operating point, DC, AC, transient, or noise. | Job/command object. |
| Behavior | Analysis-specific implementation attached to an entity. | Strategy object. |
| Simulation state | Shared numerical state: variables, solver, current solution, history. | Runtime context. |
| Solver | Sparse linear system used for modified nodal analysis. | Specialized database/table plus equation solver. |
| Export | Reader for voltages, currents, properties, plots, prints, and measurements. | Query over current runtime state. |

SpiceSharp uses behavior interfaces to separate component data from numerical work. A
resistor entity can have one set of parameters, but its biasing behavior knows how to
load a DC conductance stamp, its frequency behavior knows how to load a complex AC
stamp, and dynamic devices can also have time-domain behavior.

In other words, SpiceSharp is not one giant `switch` statement over component names.
It is closer to a plugin system: each component provides behaviors, and the simulation
asks for the behaviors it needs.

## Source Code Reading Map

When reading SpiceSharp source code, it helps to separate **what is being
simulated** from **who is doing the simulation**.

| Thing to find | What it usually means |
|---------------|-----------------------|
| Entity/component class | User-facing circuit object and parameters. |
| Behavior class | Analysis-specific numerical work for that entity. |
| Simulation class | The outer algorithm that decides which behaviors to call and when. |
| Simulation state | Shared runtime data: solver, variables, current solution, time, and history. |
| Integration method | Time-step controller and derivative/history calculator for `.TRAN`. |
| Export | Reader that observes solved values without owning the solve. |

The lifecycle of one solve point usually looks like this:

```text
bind:
  create behaviors
  allocate variables
  cache matrix/RHS locations

prepare:
  set analysis mode, time, frequency, swept value, or temperature

load:
  clear numeric matrix/RHS
  ask active behaviors to stamp equations

solve:
  factor and solve the matrix
  write the solution into simulation variables

check:
  update nonlinear guesses
  test convergence or timestep accuracy

accept/export:
  commit accepted state
  let exports read the current solution
```

Different analyses wrap that lifecycle differently:

| Analysis | Extra loop around the load/solve/check lifecycle |
|----------|--------------------------------------------------|
| `.OP` | Newton loop until the bias point converges. |
| `.DC` | Sweep loop; each point runs a biasing solve. |
| `.AC` | Frequency loop after an operating point and small-signal linearization. |
| `.TRAN` | Time loop; each candidate time may run Newton, then accept or reject history. |

If you are reading engine files, this rough map is useful:

| Source area | What to look for |
|-------------|------------------|
| `BiasingSimulation` | Real-valued load/solve/Newton iteration used by `.OP`, `.DC`, and `.TRAN`. |
| `Transient` | Time loop around biasing iteration: probe, solve, truncate/evaluate, accept, export. |
| `IIntegrationMethod` | Current time, timestep, accepted history, derivative/integral states, and reject/accept control. |
| `IAcceptBehavior` | Device state that is committed only after a successful solve point. |
| `ITruncatingBehavior` | Device or helper logic that limits timestep size. |
| Waveform classes / `IWaveform` | Source values as a function of simulation time. |

The most important reading habit is to ask:

```text
Is this code defining the circuit,
loading equations for the current solve,
checking whether the solve is acceptable,
or committing/exporting state after acceptance?
```

## Modified Nodal Analysis

SpiceSharp solves circuits with modified nodal analysis (MNA). The phrase sounds
academic, but the idea is developer-friendly:

```text
turn the circuit into a set of equations
put those equations into a matrix
ask a solver for the unknown values
```

The unknown values are usually node voltages and a few special branch currents. Once
the solver finds those values, SpiceSharp can answer questions such as `V(out)`,
`I(V1)`, or "what is the current through this resistor?"

MNA is the numerical language that lets resistors, capacitors, sources,
semiconductors, behavioral devices, and subcircuits all contribute to one shared system
of equations.

The basic idea is:

1. Use node voltages as the main unknowns.
2. Add extra current unknowns when a device imposes a voltage equation.
3. Let every component stamp its local equations into one global matrix.
4. Solve the global matrix repeatedly until the circuit state is known.

$$
Yx = \mathrm{rhs}
$$

where:

$$
\begin{aligned}
x &= \text{unknown solution vector} \\
Y &= \text{admittance/Jacobian matrix} \\
\mathrm{rhs} &= \text{right-hand-side vector}
\end{aligned}
$$

Read this like a programming problem:

$$
\begin{aligned}
Y &= \text{table of coefficients} \\
x &= \text{values we want to find} \\
\mathrm{rhs} &= \text{known values}
\end{aligned}
$$

The solver receives `Y` and `rhs`, then computes `x`.

In pure linear DC circuits, `Y` can be read as an admittance matrix. In real SpiceSharp
simulations, it is more general:

- in nonlinear DC analysis, `Y` is the Newton Jacobian,
- in AC analysis, `Y` is complex and frequency-dependent,
- in transient analysis, `Y` includes companion-model terms from the integration method,
- for ideal voltage-defined devices, `Y` also contains constraint equations.

So the safest name is "the MNA matrix" or "the Jacobian matrix", even though many rows
look like ordinary conductance/admittance rows.

### Modified Matrix Algorithm Step By Step

If someone says "the modified matrix algorithm" in a SPICE context, they usually mean
the MNA matrix assembly algorithm:

```text
circuit graph + device equations
  -> unknown vector x
  -> matrix Y
  -> RHS vector
  -> solve Y*x = rhs
```

The algorithm has two phases.

Setup phase:

```text
1. Collect all non-ground node voltages.
2. Add branch-current unknowns for voltage-defined devices.
3. Assign each unknown a solver index.
4. Let each behavior cache the matrix/RHS locations it will touch.
```

Load/solve phase:

```text
1. Clear numeric matrix and RHS values.
2. Ask each active behavior to load its stamp.
3. Sum all stamps into the shared matrix and RHS.
4. Factor and solve the sparse linear system.
5. Store the solution back into node and branch variables.
6. Check convergence, timestep accuracy, or export conditions.
```

For `.OP`, this may be one linear solve or many Newton iterations. For `.AC`, the
matrix is complex and frequency-dependent. For `.TRAN`, this load/solve phase repeats
for every candidate timestep, and dynamic devices load companion-model stamps.

The important implementation idea is:

```text
components do not solve the circuit themselves
components only add their local equations to the shared system
```

#### Matrix Assembly Trace

Consider:

```spice
V1 in 0 10
R1 in out 1k
R2 out 0 2k
.OP
```

The unknown vector is:

$$
x =
\begin{bmatrix}
V(\text{in}) \\
V(\text{out}) \\
I(\text{V1})
\end{bmatrix}
$$

Let:

$$
g_1 = 0.001,\quad g_2 = 0.0005
$$

Start with zeros:

$$
Y =
\begin{bmatrix}
0 & 0 & 0 \\
0 & 0 & 0 \\
0 & 0 & 0
\end{bmatrix},
\quad
\mathrm{rhs} =
\begin{bmatrix}
0 \\
0 \\
0
\end{bmatrix}
$$

`R1 in out 1k` stamps a two-node conductance:

```text
Y[in,in]   += g1
Y[in,out]  -= g1
Y[out,in]  -= g1
Y[out,out] += g1
```

Now:

$$
Y =
\begin{bmatrix}
g_1 & -g_1 & 0 \\
-g_1 & g_1 & 0 \\
0 & 0 & 0
\end{bmatrix}
$$

`R2 out 0 2k` adds conductance from `out` to ground:

```text
Y[out,out] += g2
```

Now:

$$
Y =
\begin{bmatrix}
g_1 & -g_1 & 0 \\
-g_1 & g_1 + g_2 & 0 \\
0 & 0 & 0
\end{bmatrix}
$$

`V1 in 0 10` adds a branch-current unknown and a voltage constraint:

```text
Y[in,I(V1)] += 1
Y[I(V1),in] += 1
rhs[I(V1)]  += 10
```

Final system:

$$
\begin{bmatrix}
g_1 & -g_1 & 1 \\
-g_1 & g_1 + g_2 & 0 \\
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
0 \\
10
\end{bmatrix}
$$

The solver finds:

```text
V(in)  = 10 V
V(out) = 6.666... V
I(V1)  = current needed by the voltage source
```

That is the modified matrix algorithm in miniature: every device contributes a
small pattern, the patterns are summed, then the solver finds the unknown vector.

### Why MNA Is Modified

Plain nodal analysis uses one equation per non-ground node. Each equation is Kirchhoff's
current law (KCL). You do not need to memorize the physics to understand the software
shape. KCL means:

```text
for every node, the current going in must match the current going out
```

That works well for resistors and current sources, because their currents can be written
directly in terms of node voltages.

For example, a resistor between `a` and `b` has:

$$
i(a \to b) = g\left(V(a) - V(b)\right)
$$

That current can be expanded into coefficients on `V(a)` and `V(b)`, so it fits directly
into KCL rows.

Example 1: resistor to ground:

```spice
R1 out 0 1k
```

There is one unknown node voltage: `V(out)`. The resistor value is known, so the
conductance is known:

$$
g = \frac{1}{1000}
$$

The node rule is:

$$
\begin{aligned}
\text{current through R1} &= 0 \\
gV(\text{out}) &= 0
\end{aligned}
$$

That is already a plain nodal equation. No extra unknown is needed. The matrix can have
one row and one column:

$$
\begin{bmatrix} g \end{bmatrix}
\begin{bmatrix} V(\text{out}) \end{bmatrix}
=
\begin{bmatrix} 0 \end{bmatrix}
$$

Example 2: current source plus resistor:

```spice
I1 out 0 2m
R1 out 0 1k
```

The current source value is known: `2mA`. The resistor current is still
`g * V(out)`. The node equation can be written directly:

$$
gV(\text{out}) = -2\,\text{mA}
$$

Depending on sign convention the RHS may appear as `+2mA` or `-2mA`, but the important
point is the same: the current source contributes a known number. It does not create a
new unknown.

Matrix form:

$$
\begin{bmatrix} g \end{bmatrix}
\begin{bmatrix} V(\text{out}) \end{bmatrix}
=
\begin{bmatrix} -2\,\text{mA} \end{bmatrix}
$$

The exact sign depends on the direction assigned to `I1`. The structural lesson is what
matters here: the current source changes the RHS, not the unknown vector.

Ideal current sources are easy for nodal analysis: they inject a known current, so they
go straight into the RHS vector. They do not add a new unknown.

Ideal voltage sources are different. They are the reason MNA is "modified". A voltage
source tells the solver:

$$
V(p) - V(n) = \text{value}
$$

but it does not tell the solver its current in advance. That is like having a method
that returns one property but hides another property you still need. MNA fixes this by
adding a new unknown for the current through the voltage-defined branch. The source then
contributes:

- KCL terms in the endpoint node rows,
- one extra row that enforces the voltage constraint,
- one extra column for the source branch current.

Example 3: voltage source plus resistor:

```spice
V1 in 0 10
R1 in 0 1k
```

The voltage source tells us:

$$
V(\text{in}) = 10
$$

The resistor current is:

$$
I(\text{R1}) = gV(\text{in})
$$

But what is the current through `V1`? It depends on the rest of the circuit. The source
will provide whatever current is necessary to keep `V(in)` at `10 V`. Plain nodal
analysis has nowhere to put that unknown source current.

MNA adds one extra unknown:

$$
x =
\begin{bmatrix}
V(\text{in}) \\
I(\text{V1})
\end{bmatrix}
$$

Now the equations can say two things:

$$
\begin{aligned}
\text{row } V(\text{in}):\quad gV(\text{in}) + I(\text{V1}) &= 0 \\
\text{row } I(\text{V1}):\quad V(\text{in}) &= 10
\end{aligned}
$$

Matrix form:

$$
\begin{bmatrix}
g & 1 \\
1 & 0
\end{bmatrix}
\begin{bmatrix}
V(\text{in}) \\
I(\text{V1})
\end{bmatrix}
=
\begin{bmatrix}
0 \\
10
\end{bmatrix}
$$

This is the "modified" part in one picture: the voltage source added a branch-current
unknown and a voltage-constraint row.

Example 4: voltage source between two non-ground nodes:

```spice
V1 p n 5
```

The source says:

$$
V(p) - V(n) = 5
$$

Again, its current is unknown. MNA adds `I(V1)`:

$$
x =
\begin{bmatrix}
V(p) \\
V(n) \\
I(\text{V1})
\end{bmatrix}
$$

The source contributes this shape:

$$
\begin{aligned}
\text{row } V(p):\quad &+I(\text{V1}) \\
\text{row } V(n):\quad &-I(\text{V1}) \\
\text{row } I(\text{V1}):\quad &V(p) - V(n) = 5
\end{aligned}
$$

Matrix contribution:

$$
\begin{bmatrix}
0 & 0 & 1 \\
0 & 0 & -1 \\
1 & -1 & 0
\end{bmatrix}
\begin{bmatrix}
V(p) \\
V(n) \\
I(\text{V1})
\end{bmatrix}
=
\begin{bmatrix}
0 \\
0 \\
5
\end{bmatrix}
$$

Other components connected to `p` and `n` add their own terms into the first two rows.
The voltage source itself provides the branch-current column and constraint row.

This same pattern appears for independent voltage sources, voltage-controlled voltage
sources, current-controlled voltage sources, inductors in many formulations, and some
dynamic or behavioral devices.

### Unknowns In The Solution Vector

The solution vector `x` is an ordered list of values the simulator is trying to find.
Think of it as an array:

```csharp
double[] x = solver.Solve();
```

The most common values in that array are node voltages. Some components add extra
current values. For a simple circuit with node voltages `V(in)`, `V(out)`, and one
ideal voltage source branch current `I(V1)`, the unknown vector may look like:

$$
x =
\begin{bmatrix}
V(\text{in}) \\
V(\text{out}) \\
I(\text{V1})
\end{bmatrix}
$$

SpiceSharp assigns these variables to solver indices during setup. Components refer to
variables through references created by the binding context and simulation state. Once
the references are bound, behavior code can load the matrix by index instead of looking
up node names on every iteration.

Common variable kinds are:

| Variable kind | Example | Why it exists |
|---------------|---------|---------------|
| Node voltage | `V(out)` | Main MNA unknown for a non-ground node. |
| Voltage-source current | `I(V1)` | Required because ideal voltage sources impose voltage, not current. |
| Inductor current | `I(L1)` | Often used so the inductor can express $v = L\,di/dt$. |
| Controlled-source current | `I(E1)`, `I(H1)` | Required for voltage-output controlled sources. |
| Internal model variable | device-specific | Used by some dynamic, nonlinear, or expanded models. |

The row and column with the same index refer to different roles:

```text
column k = coefficient multiplying unknown x[k]
row k    = equation that must be satisfied for variable k
```

For a node-voltage variable, the row is usually a KCL equation. For a branch-current
variable, the row is usually a voltage constraint or device constitutive equation.

### Ground And Index 0

Ground, usually node `0`, is the reference node. Its voltage is not solved because it is
defined to be `0 V`.

SpiceSharp sparse matrix APIs reserve index 0 as a special throw-away location for
ignored ground terms. Real solver equations use positive indices. This lets device code
stamp a uniform pattern without special-casing every grounded terminal.

For a resistor from `out` to ground, the full two-node stamp still looks like:

```text
Y[out,out] += g
Y[out,0]   -= g
Y[0,out]   -= g
Y[0,0]     += g
```

Only the `Y[out,out]` term participates in the real solve. The ground terms are routed
to the special index-0 location.

This convention is small but important. It lets component behaviors stamp the same
shape whether a pin is grounded or not. That keeps device code simple and avoids many
conditional branches inside hot solver paths.

### Rows Are Equations

Think of each matrix row as one rule the simulator must satisfy.

For node rows, the rule is usually:

```text
sum of currents leaving node = sum of independent current injections
```

For a resistor from `out` to ground:

$$
gV(\text{out}) = 0
$$

For a resistor from `in` to `out`:

$$
\begin{aligned}
g\left(V(\text{in}) - V(\text{out})\right) &\to \text{row in} \\
g\left(V(\text{out}) - V(\text{in})\right) &\to \text{row out}
\end{aligned}
$$

For a voltage source branch row:

$$
V(p) - V(n) = \text{source voltage}
$$

After stamping, all of those local equations are added together into one matrix. If two
components touch the same matrix location, their coefficients are summed. This is why
the operation is called stamping: each behavior contributes its local stencil to the
global matrix.

### Columns Are Unknowns

Each matrix column represents one unknown value from `x`. A non-zero entry means:

```text
this equation depends on this unknown
```

If column `out` has a coefficient in row `in`, that means the equation for node `in`
depends on `V(out)`. Resistors, capacitors, controlled sources, and nonlinear device
Jacobians all create these cross-couplings.

Branch-current columns are different but follow the same rule. If a voltage source
current unknown `I(V1)` appears in the KCL row for node `p`, then `Y[p,b] += 1` says:

```text
the current through V1 participates in node p's KCL equation
```

### RHS Is The Known Side

The RHS vector stores known values. If `Y` is "what multiplies the unknowns", `rhs` is
"what the equations must equal".

Examples:

| Contribution | Typical RHS effect |
|--------------|--------------------|
| Independent current source | Adds current to endpoint node rows. |
| Independent voltage source | Adds voltage value to the branch constraint row. |
| Linearized diode | Adds equivalent current source from Newton linearization. |
| Capacitor transient companion | Adds history current from previous accepted timesteps. |
| Inductor transient companion | Adds history voltage/current term. |
| Behavioral expression | Adds current, voltage, or residual value after evaluation. |

In nonlinear analysis, the RHS is not just "independent sources". It also contains the
constant part of each local linear approximation. That is what lets the linear system
represent a nonlinear circuit at the current Newton guess.

### Setup Versus Load In SpiceSharp

SpiceSharp separates structural setup from numeric loading. This is very similar to
separating allocation from per-frame/per-iteration work in normal software.

Setup/binding answers questions like:

- Which variables exist?
- Which solver indices do those variables use?
- Which matrix locations does this behavior need?
- Which RHS entries does this behavior need?
- Which simulation states are available?

Loading answers questions like:

- What conductance does this resistor have right now?
- What current does this source inject at this time?
- What small-signal derivatives does this diode have at the current guess?
- What equivalent conductance does this capacitor get for this timestep?

In simplified form:

```text
setup:
  create variables
  create behaviors
  bind behaviors to solver indices
  allocate matrix/RHS element references

each solve attempt:
  clear numeric values
  call Load() on each active behavior
  factor and solve
```

This separation is central to SpiceSharp performance. Topology and matrix locations are
mostly stable, while numeric values change constantly. A transient simulation can reuse
the same sparse matrix structure across many timesteps, only changing values such as
source waveforms, nonlinear derivatives, and integration coefficients.

The software pattern is:

```text
bind once
load many times
```

### Behavior Loading

For MNA, the most important behavior method is the load step. Biasing behaviors load
real-valued DC/Newton contributions. Frequency behaviors load complex-valued AC
contributions. Time behaviors prepare dynamic state and companion-model values used by
the biasing load during transient analysis.

Conceptually, a behavior does this:

```text
read present parameters and simulation state
compute local coefficients
add coefficients to Y
add known terms to rhs
```

For a linear resistor, the local coefficient is simply $g = 1 / R$.

For a nonlinear diode, the local coefficients depend on the previous solution guess:

$$
\begin{aligned}
g_d &= \text{local derivative } di/dv \\
i_{\text{eq}} &= \text{residual current term}
\end{aligned}
$$

For a transient capacitor, the local coefficients depend on timestep and history:

$$
\begin{aligned}
g_{\text{eq}} &= \text{integration coefficient} \\
i_{\text{history}} &= \text{contribution from previous accepted states}
\end{aligned}
$$

### Linear Circuit Example

Consider this small circuit:

```spice
V1 in 0 10
R1 in out 1k
R2 out 0 2k
.OP
```

Unknown vector:

$$
x =
\begin{bmatrix}
V(\text{in}) \\
V(\text{out}) \\
I(\text{V1})
\end{bmatrix}
$$

Let:

$$
\begin{aligned}
g_1 &= \frac{1}{1000} \\
g_2 &= \frac{1}{2000}
\end{aligned}
$$

The resulting MNA system is:

$$
\begin{aligned}
\text{row } V(\text{in}):\quad g_1V(\text{in}) - g_1V(\text{out}) + I(\text{V1}) &= 0 \\
\text{row } V(\text{out}):\quad -g_1V(\text{in}) + (g_1 + g_2)V(\text{out}) &= 0 \\
\text{row } I(\text{V1}):\quad V(\text{in}) &= 10
\end{aligned}
$$

Matrix form:

$$
\begin{bmatrix}
g_1 & -g_1 & 1 \\
-g_1 & g_1 + g_2 & 0 \\
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
0 \\
10
\end{bmatrix}
$$

The voltage source adds the branch-current unknown and the final constraint row. The
resistors add the conductance terms. Solving gives `V(in) = 10 V` and
`V(out) = 6.666... V`.

If the matrix still looks intimidating, focus on the rows:

- first row: KCL at node `in`,
- second row: KCL at node `out`,
- third row: voltage source says `V(in)` must be `10`.

The matrix is just the compact table form of those three rules.

### Nonlinear Circuit Example

For a diode connected from `out` to ground, the real equation is nonlinear:

$$
i = I_s\left(e^{V(\text{out})/(nV_t)} - 1\right)
$$

SpiceSharp does not solve that exponential equation directly in one matrix solve.
Instead, each Newton iteration replaces the diode with a local linear approximation:

$$
i \approx g_dV(\text{out}) + i_{\text{eq}}
$$

Then the diode loads:

```text
Y[out,out] += gd
rhs[out]   -= ieq
```

After solving, the diode checks whether its current and voltage changed enough to count
as converged. If not, it recomputes `gd` and `ieq` around the new guess and loads the
matrix again. This repeats until all nonlinear convergence checks pass or the iteration
limit is reached.

Here is a more concrete picture. Imagine this circuit:

```spice
V1 in 0 5
R1 in out 1k
D1 out 0 DMOD
.MODEL DMOD D(IS=1e-14)
.OP
```

The resistor is linear. Its stamp is always based on:

$$
g_R = \frac{1}{1000}
$$

The diode is nonlinear. Its current depends exponentially on `V(out)`, so SpiceSharp
cannot stamp one fixed conductance for the whole solve. Instead it uses the current
guess for `V(out)`.

At one Newton iteration, suppose the current guess is:

$$
V_{\text{guess}}(\text{out}) = 0.60\,\text{V}
$$

The diode behavior evaluates its model at that guess:

$$
\begin{aligned}
i_d &= \text{diode current at } 0.60\,\text{V} \\
g_d &= \text{diode slope at } 0.60\,\text{V} \\
i_{\text{eq}} &= i_d - g_d \cdot 0.60
\end{aligned}
$$

Plain English:

```text
id  = where the diode curve is at this guess
gd  = how steep the diode curve is at this guess
ieq = correction term so the straight-line approximation touches the curve
```

For this one iteration, the diode pretends to be:

$$
\text{diode current} \approx g_dV(\text{out}) + i_{\text{eq}}
$$

So it stamps:

```text
Y[out,out] += gd
rhs[out]   -= ieq
```

Then the solver solves the whole circuit and may return:

$$
V_{\text{new}}(\text{out}) = 0.67\,\text{V}
$$

Now the diode checks the change:

$$
\text{change} = \left|0.67 - 0.60\right|
$$

If that change is larger than the allowed tolerance, the diode says "not converged".
SpiceSharp starts another Newton iteration using the new guess:

$$
V_{\text{guess}}(\text{out}) = 0.67\,\text{V}
$$

The diode recomputes:

$$
\begin{aligned}
i_d &= \text{diode current at } 0.67\,\text{V} \\
g_d &= \text{diode slope at } 0.67\,\text{V} \\
i_{\text{eq}} &= i_d - g_d \cdot 0.67
\end{aligned}
$$

Then it reloads the matrix with the new `gd` and `ieq`. The resistor stamp stays the
same, but the diode stamp changes because the diode operating point changed.

A simplified iteration log might look like:

| Iteration | Guess before load | Solved value | Change | Converged? |
|-----------|-------------------|--------------|--------|------------|
| 1 | `0.10 V` | `0.58 V` | large | no |
| 2 | `0.58 V` | `0.66 V` | smaller | no |
| 3 | `0.66 V` | `0.674 V` | smaller | no |
| 4 | `0.674 V` | `0.675 V` | tiny | yes |

The numbers are illustrative, not exact SpiceSharp output. The important idea is that
each iteration replaces the curved diode equation with a straight-line approximation at
the current guess. When the next solved value is close enough to the previous guess,
the approximation and the real nonlinear device agree well enough to stop.

This is also why bad nonlinear circuits can fail to converge. If the guess jumps around,
or if the diode model creates extreme slopes, each new straight-line approximation may
point the solver somewhere unhelpful. Then the iteration limit is reached before the
changes become small enough.

### Dynamic Circuit Example

For a capacitor, the original equation is differential:

$$
i = C\frac{dv}{dt}
$$

MNA needs algebraic equations, so transient analysis uses an integration method to turn
the capacitor into a companion model at each timestep:

$$
i \approx g_{\text{eq}}v + i_{\text{history}}
$$

That looks like a resistor plus a current source for one solve attempt. When the
timestep changes, or when a timestep is rejected and retried, `geq` and `ihistory` are
recomputed. When a timestep is accepted, the capacitor's integration history is
accepted too.

This is why transient analysis is a loop around the MNA solve:

```text
choose timestep
build companion models for that timestep
solve nonlinear MNA system
accept or reject timestep
update history if accepted
```

### Real And Complex MNA

SpiceSharp uses the same MNA idea for real and complex systems.

| Analysis | Matrix value type | Meaning |
|----------|-------------------|---------|
| `.OP` | real | DC operating-point equations. |
| `.DC` | real | Repeated DC operating-point equations over swept values. |
| `.TRAN` | real | Time-domain companion-model equations. |
| `.AC` | complex | Small-signal frequency-domain equations. |
| `.NOISE` | complex plus noise data | Small-signal transfer and noise propagation. |

In AC, capacitors and inductors do not use timestep history. They stamp frequency-domain
admittances and impedances using $s = j\omega = j\,2\pi f$. The topology is familiar, but the
numbers are complex.

### What MNA Does Not Do Alone

MNA is the equation format, not the entire simulator. A full SpiceSharp simulation also
needs:

- model evaluation,
- temperature updates,
- parameter sweeps,
- Newton iteration,
- convergence checks,
- sparse ordering and factorization,
- timestep control,
- truncation-error control,
- export events.

MNA is the common place where all device physics and simulation control eventually meet:
every active behavior must express its local effect as matrix and RHS contributions.

## Stamp Notation

A stamp is a component's small contribution to the big global matrix. If the matrix is
a spreadsheet, stamping means "add these numbers to these cells."

For example, a resistor says:

```text
add +g to cell (p,p)
add -g to cell (p,n)
add -g to cell (n,p)
add +g to cell (n,n)
```

Many components may add to the same cell. The final cell value is the sum of all their
contributions.

The tables below use these symbols:

| Symbol | Meaning |
|--------|---------|
| `p`, `n` | Positive and negative output terminals. |
| `cp`, `cn` | Positive and negative control terminals. |
| `b` | Extra branch-current equation/unknown for an ideal voltage-defined branch. |
| `bc` | Branch current of a controlling voltage source. |
| `Y[r,c] += a` | Add `a` to the matrix row `r`, column `c`. |
| `rhs[r] += a` | Add `a` to the right-hand-side vector row `r`. |
| $v(p,n)$ | $V(p) - V(n)$. |
| $s$ | Laplace variable. In AC, $s = j\omega = j\,2\pi f$. |

The sign convention used here is the common SPICE convention: a current source from
`p` to `n` removes current from `p` and injects current into `n`:

```text
rhs[p] -= I
rhs[n] += I
```

Exact sign details can differ in internal code depending on whether a behavior writes
an equivalent residual or source form, but these stamps describe the resulting MNA
equations.

## Sparse Matrix And Solver Internals

This section explains the "spreadsheet" that SpiceSharp builds and solves. You do not
need to know advanced linear algebra first. The key idea is:

```text
most circuit matrix cells are zero,
so SpiceSharp stores only the cells that matter.
```

### Dense Versus Sparse

A dense matrix stores every cell:

```text
5 x 5 dense table

[ ? ? ? ? ? ]
[ ? ? ? ? ? ]
[ ? ? ? ? ? ]
[ ? ? ? ? ? ]
[ ? ? ? ? ? ]
```

A sparse matrix stores only cells that are actually used:

```text
same logical table, but only these cells exist:

(1,1), (1,2), (2,1), (2,2), (3,5), (5,3)
```

Circuit matrices are naturally sparse because most components touch only a few nodes.
A resistor between `a` and `b` touches four cells. It does not care about every other
node in the circuit.

Example: if a circuit has 10,000 unknowns, a dense matrix has:

```text
10,000 * 10,000 = 100,000,000 cells
```

But each resistor still touches only four cells. Storing all 100 million cells would be
wasteful.

### The Mental Model

Think of the solver matrix as a large dictionary keyed by `(row, column)`:

```text
matrix[(row, column)] = value
```

In a real sparse solver it is more specialized than a normal dictionary, but the mental
model is useful:

```text
if a component needs a cell:
  create or find that cell
  keep a fast reference to it
  add values to it during every load
```

Rows are equations. Columns are unknowns. A matrix cell answers:

```text
how much does this unknown contribute to this equation?
```

### Variable Mapping

The netlist uses names:

```spice
R1 in out 1k
V1 in 0 10
```

The solver needs indexes:

```text
"in"     -> solver index 1
"out"    -> solver index 2
"I(V1)"  -> solver index 3
```

After setup, component behaviors work with indexes and cached references, not with
strings. That is important because `Load()` can be called many times:

```text
OP solve:        maybe many Newton iterations
DC sweep:        many sweep points
Transient solve: many timesteps, each with Newton iterations
AC solve:        many frequency points
```

String lookup in every load would be unnecessary overhead.

### SparseMatrix

`SparseMatrix<T>` is SpiceSharp's sparse matrix type. It is still logically a square
matrix, but it only stores allocated elements.

It keeps elements connected by row and by column:

```text
row view:
  row 2 -> cells (2,1), (2,2), (2,5)

column view:
  col 2 -> cells (1,2), (2,2), (4,2)
```

Why both views? Because solving and factoring a matrix need to walk through rows and
columns efficiently. A plain list of cells would be too slow for large circuits.

For a junior developer, the important part is:

```text
SparseMatrix<T> stores only useful cells
and lets the solver navigate those cells efficiently.
```

Index `0` is special. Grounded terms go to a throw-away index-0 area, because ground is
defined as `0 V` and does not need a real solver equation.

### MatrixLocation

`MatrixLocation` is just a coordinate:

```text
MatrixLocation(row, column)
```

Read it as:

```text
MatrixLocation(which equation, which unknown)
```

For a resistor between `p` and `n`, the behavior needs four matrix locations:

```text
(p,p)
(p,n)
(n,p)
(n,n)
```

If `n` is ground, then locations involving `n` use index `0` and are ignored by the real
solve.

### ElementSet

`ElementSet<T>` is a helper for cached matrix and RHS entries.

Without caching, a behavior would repeatedly do this:

```text
find Y[p,p]
find Y[p,n]
find Y[n,p]
find Y[n,n]
add values
```

With caching, setup does the lookup once:

```text
setup:
  remember references to Y[p,p], Y[p,n], Y[n,p], Y[n,n]
```

Then loading is cheap:

```text
load:
  add +g, -g, -g, +g to the cached cells
```

Software analogy:

```text
ElementSet<T> = precomputed references to the cells a component will edit
```

This is the same performance idea as caching a resolved method, compiled expression, or
array index instead of rediscovering it inside a hot loop.

### One Solve Pass

A simplified solve pass looks like this:

```text
1. Clear previous numeric values from Y and rhs.
2. Ask every active behavior to Load().
3. Each behavior adds values to cached matrix/RHS cells.
4. Factor the matrix.
5. Solve for x.
6. Copy x back into simulation variables.
7. Let nonlinear devices check convergence.
```

Example with one resistor and one current source:

```spice
I1 out 0 2m
R1 out 0 1k
```

Unknown:

$$
x =
\begin{bmatrix}
V(\text{out})
\end{bmatrix}
$$

After loading:

$$
Y =
\begin{bmatrix}
g
\end{bmatrix},
\qquad
\mathrm{rhs} =
\begin{bmatrix}
-2\,\text{mA}
\end{bmatrix}
$$

The solver computes:

$$
V(\text{out}) = \frac{\mathrm{rhs}}{g}
$$

The real solver is more general, but this tiny example shows the same pattern:

```text
load known component contributions,
then solve for unknown circuit values.
```

### What The Solver Knows

The solver does not know what a resistor, diode, capacitor, or MOSFET is. By the time
the solver runs, all electronics meaning has been translated into numbers.

The solver receives:

$$
\begin{aligned}
Y &= \text{matrix of coefficients} \\
\mathrm{rhs} &= \text{known values}
\end{aligned}
$$

The solver returns:

$$
x = \text{solved unknown values}
$$

That is it.

For example, the solver does not see:

```text
R1 in out 1k
D1 out 0 DMOD
```

It sees something more like:

```text
Y[1,1] += ...
Y[1,2] += ...
rhs[1] += ...
```

This separation is important. Component behaviors understand circuit physics. The
solver understands equation systems.

```text
components/behaviors:
  "What numbers should be added to the matrix?"

solver:
  "Given this matrix, what is x?"
```

### Tiny Numeric Solve Example

Here is a plain math example, not a full circuit. Suppose loading produced:

$$
Y =
\begin{bmatrix}
3 & 1 \\
1 & 2
\end{bmatrix},
\qquad
\mathrm{rhs} =
\begin{bmatrix}
7 \\
5
\end{bmatrix}
$$

The solver is trying to find:

$$
x =
\begin{bmatrix}
x_1 \\
x_2
\end{bmatrix}
$$

So the equations are:

$$
\begin{aligned}
3x_1 + x_2 &= 7 \\
x_1 + 2x_2 &= 5
\end{aligned}
$$

One possible way to solve by hand:

$$
\begin{aligned}
x_1 + 2x_2 &= 5 \\
x_1 &= 5 - 2x_2 \\
3(5 - 2x_2) + x_2 &= 7 \\
15 - 6x_2 + x_2 &= 7 \\
15 - 5x_2 &= 7 \\
x_2 &= 1.6 \\
x_1 &= 1.8
\end{aligned}
$$

So:

$$
x =
\begin{bmatrix}
1.8 \\
1.6
\end{bmatrix}
$$

For a circuit, `x1` and `x2` might be voltages or branch currents. The solver does not
care. It just solves the equations.

### Why A Special Sparse Solver Is Needed

The tiny example above is easy by hand. Real circuits are not tiny:

```text
small example: 2 unknowns
real circuit:  thousands or millions of unknowns
```

A general dense solver would waste time on zero cells. A sparse solver tries to do work
only around cells that exist.

That is why the flow is:

```text
behaviors create sparse matrix entries
solver factors only the sparse structure
solver computes the solution vector
simulation reads solution values back
```

The solver is a performance-critical data structure plus algorithm. It is not just a
formula.

### Solver State

`ISolverSimulationState<T>` is the simulation state that owns the solver and solution
vector.

The generic `T` is the number type:

| Analysis | Typical `T` |
|----------|-------------|
| `.OP`, `.DC`, `.TRAN` | `double` |
| `.AC`, `.NOISE` | complex number |

The solver state is where behaviors get access to:

- the sparse solver,
- the current solution vector,
- variable mappings,
- matrix/RHS entries.

Plain English:

```text
solver state = the shared numerical workspace for this simulation
```

### Factorization

Once the matrix is loaded, SpiceSharp must solve:

$$
Yx = \mathrm{rhs}
$$

The direct way to think about this is:

```text
given the table Y and known values rhs,
find the unknown values x
```

Internally, sparse solvers commonly factor the matrix into easier pieces. You may see
this written as:

$$
Y = LU
$$

You do not need to implement LU factorization to read SpiceSharp code, but it helps to
know what problem it solves.

Suppose the solver receives:

$$
\begin{bmatrix}
2 & 1 \\
4 & 3
\end{bmatrix}
\begin{bmatrix}
x_1 \\
x_2
\end{bmatrix}
=
\begin{bmatrix}
5 \\
11
\end{bmatrix}
$$

The equations are:

$$
\begin{aligned}
2x_1 + x_2 &= 5 \\
4x_1 + 3x_2 &= 11
\end{aligned}
$$

One hand-solving strategy is elimination:

$$
\begin{aligned}
\mathrm{row}_2 &\leftarrow \mathrm{row}_2 - 2\mathrm{row}_1 \\
\text{before:}\quad 2x_1 + x_2 &= 5 \\
4x_1 + 3x_2 &= 11 \\
\text{after:}\quad 2x_1 + x_2 &= 5 \\
x_2 &= 1
\end{aligned}
$$

Now the answer is easy:

$$
\begin{aligned}
x_2 &= 1 \\
2x_1 + 1 &= 5 \\
x_1 &= 2
\end{aligned}
$$

Factorization is the solver's organized version of this idea. It prepares the matrix so
the solution can be found by simpler forward/backward steps.

The useful mental model is:

```text
factorization prepares the matrix
substitution uses that prepared matrix to get the answer
```

Factorization is usually the expensive part. Solving after factorization is cheaper.

You can think of factorization like preparing an index before running a query:

```text
without preparation:
  solving is hard every time

after factorization:
  the matrix is reorganized into a form that is easier to solve
```

For one loaded matrix, the solver typically does:

```text
factor:
  analyze/reorder/factor Y

solve:
  use the factored form to compute x
```

The `L` and `U` names come from splitting the prepared matrix into two triangular
systems:

$$
\begin{aligned}
L &= \text{lower triangular, values mostly on/below the diagonal} \\
U &= \text{upper triangular, values mostly on/above the diagonal}
\end{aligned}
$$

Triangular systems are easier to solve because one variable can be found at a time.

Example upper-triangular system:

$$
\begin{bmatrix}
2 & 1 \\
0 & 1
\end{bmatrix}
\begin{bmatrix}
x_1 \\
x_2
\end{bmatrix}
=
\begin{bmatrix}
5 \\
1
\end{bmatrix}
$$

The second row gives `x2 = 1`. Then the first row gives `x1 = 2`.

In circuit simulation, this is repeated many times because device values change. The
matrix structure may be mostly the same, but the numeric values inside the cells change.

Example:

```text
Newton iteration 1:
  diode gd = 0.001
  factor and solve

Newton iteration 2:
  diode gd = 0.004
  same cell locations, different values
  factor and solve again
```

The expensive part is not finding the diode's cells again. Those were cached. The
expensive part is solving the changed numeric system.

### Pivoting, Ordering, And Fill-In

Sparse solvers have to choose a good processing order. Three words appear often:
`pivoting`, `ordering`, and `fill-in`.

Pivoting means choosing stable values to divide by during factorization. A bad pivot is
like dividing by a number that is too close to zero: the result becomes unreliable.

#### Pivoting Example

```text
bad first pivot:

[ 0.000000001  1 ]
[ 1            2 ]
```

If elimination starts at the top-left value, it must divide by `0.000000001`. That is
dangerous because tiny rounding errors can become huge.

A safer internal order is:

```text
swap row 1 and row 2:

[ 1            2 ]
[ 0.000000001  1 ]
```

Now the first pivot is `1`, which is much safer. The mathematical answer is the same;
the solver just changed the internal route to get there.

For circuit simulation, weak pivots often happen when the circuit is nearly singular or
badly scaled. Examples:

- extremely large and extremely small component values mixed together,
- almost-floating nodes,
- ideal source loops,
- very weak conductance paths.

#### Ordering Example

Ordering means rearranging internal rows and columns to make factorization safer or
faster. This does not change the circuit answer; it changes how the solver gets there.

Think of ordering like choosing the order to process tasks with dependencies. Some
orders create less temporary work than others.

Suppose `x1`, `x2`, `x3`, and `x4` are unknowns, and `x1` is connected to all others:

```text
star-like matrix pattern:

    x1 x2 x3 x4
r1  X  X  X  X
r2  X  X  .  .
r3  X  .  X  .
r4  X  .  .  X
```

If the solver eliminates `x1` first, it can create new connections between `x2`, `x3`,
and `x4`. That creates extra stored cells.

If it eliminates the leaf-like variables first (`x2`, `x3`, `x4`), it may create fewer
extra cells.

The circuit answer is unchanged. Only the internal work changes.

#### Fill-In Example

Fill-in means factorization creates extra non-zero cells that were not present in the
original matrix.

Start with this sparse pattern, where `X` means a stored non-zero and `.` means zero:

```text
before factorization:

    x1 x2 x3
r1  X  X  .
r2  X  X  X
r3  .  X  X
```

During factorization, eliminating `x2` can create a new relationship between `x1` and
`x3`:

```text
after fill-in:

    x1 x2 x3
r1  X  X  F
r2  X  X  X
r3  F  X  X
```

`F` means fill-in: a cell that was zero in the original matrix but becomes needed by the
factorization process.

Fill-in is not wrong. It is temporary mathematical bookkeeping. But it costs memory and
time, so sparse solvers try to choose an ordering that keeps fill-in low.

Developer analogy:

```text
ordering = choose a good execution plan
pivoting = avoid unstable divide-by-nearly-zero steps
fill-in  = temporary/intermediate data created by the execution plan
```

Some fill-in is normal. Too much fill-in makes the solve slower and uses more memory.

#### Why Circuit Matrices Care So Much

Circuit matrices can be very sparse:

```text
10,000 unknowns
dense matrix cells: 100,000,000
actual useful cells: maybe tens or hundreds of thousands
```

If factorization creates too much fill-in, the solver loses some of the sparse advantage.
Good ordering protects performance. Good pivoting protects numerical stability.

In one sentence:

```text
ordering tries to keep the work small;
pivoting tries to keep the math stable;
fill-in is the extra work created along the way.
```

### Why Reuse Matters

In many simulations, the circuit topology stays the same while numbers change.

Examples:

- a diode changes `gd` and `ieq` each Newton iteration,
- a capacitor changes `geq` and history current each timestep,
- an AC analysis changes frequency-dependent values at each frequency,
- a DC sweep changes source values at each sweep point.

The matrix cells are mostly the same cells. Only their numeric values change.

That is why SpiceSharp separates:

```text
structure:
  which cells exist?

values:
  what numbers are in those cells right now?
```

This is one of the main reasons sparse matrix solving can be fast enough for circuit
simulation.

### Singular Matrix Causes

A singular matrix means the equations do not determine one unique solution.

Read it as:

```text
the simulator was asked a question that does not have one unique answer
```

Common causes:

- a floating node with no DC path to ground,
- two ideal voltage sources fighting with different voltages in parallel,
- an inductor loop or voltage source loop with no resistance path,
- a capacitor-only island in operating point,
- a zero-valued resistor, inductor, or source parameter that removes an expected path,
- a model that creates an impossible operating point.

Example floating node:

```spice
C1 floating 0 1u
.OP
```

In operating point, an ideal capacitor is open. The node `floating` has no DC rule that
sets its voltage, so the solver cannot find one unique answer.

One common fix is to add a large resistor to ground:

```spice
Rleak floating 0 1G
```

That gives the node a weak DC path and gives the matrix a real equation for the node.

## Behavior Architecture

SpiceSharp entities are not the matrix solver. An entity represents a component or
model object, while behaviors represent what that object does in a particular analysis.

This is one of the most developer-friendly parts of SpiceSharp. You can understand it
with normal object-oriented design:

```text
entity = data and identity
behavior = analysis-specific implementation
```

That distinction is important:

```text
Entity
  owns names, pins, parameters, model references

Behavior
  owns analysis-specific numerical work
```

A resistor entity may expose resistance and temperature parameters. Its biasing
behavior loads a real conductance stamp. Its frequency behavior loads an AC stamp. A
capacitor has time behavior because it needs integration history. A diode has
convergence behavior because its nonlinear current must be checked after each Newton
iteration.

Common SpiceSharp behavior interfaces:

| Interface | Purpose |
|-----------|---------|
| `IBehavior` | Base behavior contract. |
| `ITemperatureBehavior` | Computes temperature-dependent parameters before solving. |
| `IBiasingBehavior` | Loads real-valued DC/Newton matrix and RHS entries. |
| `IBiasingUpdateBehavior` | Updates device state after a biasing solve iteration. |
| `IConvergenceBehavior` | Checks whether a nonlinear device has converged. |
| `IFrequencyBehavior` | Loads complex small-signal matrix and RHS entries for AC/noise. |
| `IFrequencyUpdateBehavior` | Updates frequency-domain state after a frequency solve. |
| `ITimeBehavior` | Initializes or prepares time-domain state for transient analysis. |
| `ITruncatingBehavior` | Helps estimate timestep truncation error. |
| `IAcceptBehavior` | Accepts state after a successful timestep or solve point. |
| `INoiseBehavior` | Contributes noise sources and noise density data. |

The same component can implement several of these. For example, a diode typically needs
temperature behavior, biasing behavior, convergence behavior, frequency behavior, and
possibly time behavior for capacitances. A simple resistor may only need temperature,
biasing, and frequency behavior.

Do not worry if the behavior list feels long. Most analyses only ask for the behavior
types they need. A DC operating-point simulation does not care about noise behavior.
An AC simulation does care about frequency behavior.

### Behavior Interface Cheat Sheet

The easiest way to read a behavior class is to ask when the simulation calls it.

| Interface or method | Called when | What to look for in the code |
|---------------------|-------------|------------------------------|
| `ITemperatureBehavior.Temperature()` | Before solving, and when temperature changes. | Effective parameter calculation, such as resistance, capacitance, or model constants. |
| `IBiasingBehavior.Load()` | Every real-valued matrix load: `.OP`, `.DC`, Newton iterations, and `.TRAN` candidate solves. | Real matrix/RHS stamps, local Jacobian terms, equivalent sources. |
| `IConvergenceBehavior.IsConvergent()` | After a Newton solve/update. | Device-specific checks that decide whether the latest guess is close enough. |
| `IBiasingUpdateBehavior.Update()` | After a biasing solve iteration, before the next convergence/load decision. | Internal state updates based on the latest real solution. |
| `IFrequencyBehavior.Load()` | At each AC/noise frequency point. | Complex admittance/current contributions using `s = jomega`. |
| `IFrequencyUpdateBehavior.Update()` | After a frequency-domain solve. | Frequency-domain state or export preparation. |
| `ITimeBehavior.InitializeStates()` | Before transient stepping starts. | Initial charge, flux, delay, or internal state setup from OP or `UIC`. |
| `ITruncatingBehavior.Prepare()` | Before probing the next transient point. | A maximum allowed next timestep, often to hit a scheduled event. |
| `ITruncatingBehavior.Evaluate()` | After a candidate transient point converges. | Whether the solved point allows the attempted timestep or requires a smaller retry. |
| `IAcceptBehavior.Accept()` | After a solve point is accepted. | Committing history, latch state, waveform state, or other trusted values. |
| `INoiseBehavior.Load()` | During noise analysis. | Noise source density contributions. |

Some names can be misleading until you see the lifecycle. `Load()` does not mean
"load from disk." It means "load this behavior's equations into the current
matrix and RHS." `Accept()` does not mean Newton merely converged; in transient
analysis it means the timestep was accepted and history can be trusted.

For `.TRAN`, the most common order is:

```text
initialize time states

for each candidate time:
  Prepare() truncation limits for the next step
  Load() during each Newton iteration
  IsConvergent() / Update() while Newton iterates
  Evaluate() truncation check after Newton converges
  Accept() only if the candidate timestep is accepted
```

That order explains why dynamic behavior code often has two kinds of state:

```text
candidate state:
  recomputed during Newton iterations

accepted state:
  committed only after Accept()
```

### Behavior Containers

During setup, SpiceSharp builds behavior containers for entities. A behavior container
is a named group of behaviors attached to one entity for one simulation context.

Conceptually:

```text
Circuit entity "R1"
  -> behavior container "R1"
       -> biasing behavior
       -> frequency behavior
       -> temperature behavior
```

The simulation asks for behaviors by interface. Operating point analysis cares about
`IBiasingBehavior`, `IConvergenceBehavior`, `IBiasingUpdateBehavior`, and
`ITemperatureBehavior`. AC analysis also asks for `IFrequencyBehavior`. Transient
analysis uses biasing plus time, accept, and truncation behavior.

This design is why SpiceSharp can add new analyses and custom devices without forcing
every component into one giant base class.

### One Component Full Lifecycle: Capacitor

Follow one capacitor through the system:

```spice
V1 in 0 PULSE(0 5 0 1n 1n 1m 2m)
R1 in out 1k
C1 out 0 1u IC=0
.TRAN 1u 5m
.SAVE V(out)
.END
```

The parser first turns the `C1` line into a SpiceSharp capacitor entity:

```text
name: C1
positive node: out
negative node: 0
capacitance: 1u
initial condition: 0 V
```

During simulation setup, SpiceSharp binds that entity to behaviors and runtime
state:

| Setup piece | What happens |
|-------------|--------------|
| Node variables | `out` is mapped to a solver variable; ground `0` is the reference. |
| Behavior container | `C1` receives capacitor behaviors for the active analyses. |
| Matrix locations | The behavior caches the matrix/RHS entries touched by the capacitor stamp. |
| Time state | The transient behavior creates an integration derivative state for charge. |
| Export | `.SAVE V(out)` creates a reader for the solved `out` node voltage. |

Before transient stepping starts, the capacitor history is initialized:

```text
without UIC:
  solve operating point
  use V(out) from OP to initialize q = C * V(out)

with UIC:
  use IC=0, .IC statements, or available initial values
  initialize q directly from that starting voltage
```

At each candidate timestep, `C1` participates in the Newton load just like every
other active behavior. For a linear capacitor, its time behavior conceptually
does this:

```text
read current voltage guess:
  v = V(out) - V(0)

compute present stored charge:
  q = C * v

ask the integration method:
  derive dq/dt from q, timestep, and accepted history

load companion model:
  matrix gets conductance-like term geq
  RHS gets history-current term ihistory
```

Accepted history means the last time-domain state that the simulator decided was
valid and committed. It is different from the candidate state currently being
tried:

```text
accepted history:
  last trusted charge/flux/delay/internal state

candidate state:
  values being tested at the current candidate time
```

For `C1`, accepted history includes the last trusted capacitor charge. If the
candidate timestep is rejected, the candidate charge is thrown away and the next
retry still starts from the previous accepted charge.

The loaded relation has the usual companion shape:

$$
i_n \approx g_{\text{eq}}v_n + i_{\text{history}}
$$

During Newton iterations, the candidate voltage `v_n` may change, so the
capacitor reloads its contribution using the current guess and the same accepted
history. If the candidate timestep is rejected, none of that candidate charge
history is kept.

When the timestep is accepted:

```text
Accept():
  current charge state becomes accepted history
  simulation time advances
  V(out) export reads the accepted solution
```

So one capacitor touches almost every major SpiceSharp idea:

```text
netlist line
  -> entity and parameters
  -> behavior container
  -> node variables and cached matrix locations
  -> integration state
  -> matrix/RHS companion stamp during Load()
  -> accepted charge history after Accept()
  -> V(out) export after the point is accepted
```

## Simulation Lifecycle

When user code calls:

```csharp
simulation.Execute(circuit);
```

SpiceSharp runs a setup and execution pipeline. The exact details differ by analysis,
but the flow is roughly:

```text
1. Create simulation states
2. Build behavior containers for circuit entities
3. Bind behaviors to variables, states, and solver entries
4. Apply temperature and parameter setup
5. Initialize analysis-specific state
6. Run the analysis loop
7. Fire export events
8. Accept or update final state
```

### State Creation

Simulation states are shared services for behaviors. They include things like:

- variable dictionaries,
- solver state,
- current solution vector,
- frequency point,
- time point,
- integration method,
- noise accumulation state.

`ISolverSimulationState<T>` is the state that exposes the solver and solution vector
used by MNA. Biasing analysis uses real numbers. Frequency analysis uses complex
numbers.

In application terms, state is the object graph that says "where are we right now in
this simulation?"

### Behavior Creation

The simulation inspects the circuit and asks each entity to create behaviors relevant
to the analysis. Components that do not participate in an analysis simply do not
provide that behavior type.

For example:

```text
OP:
  temperature behaviors
  biasing behaviors
  convergence/update behaviors

AC:
  operating-point/biasing behaviors
  frequency behaviors

TRAN:
  operating-point/biasing behaviors
  time behaviors
  accept/truncation behaviors
```

### Binding

Binding connects behaviors to the simulation. This is where behaviors find node
variables, create branch-current variables, get solver states, and reserve matrix
locations.

You can think of binding as dependency injection for simulation internals. The behavior
gets references to the services and variables it will need later.

After binding, the behavior should not need to search for `"out"` or `"V1"` by text in
the hot path. It already has references to the variables and matrix entries it will
touch.

### Execution

Execution is the repeated numerical part. The simulation clears the numeric values,
loads all relevant behaviors, solves, updates states, checks convergence, and exports
data. The exact loop depends on the analysis:

```text
OP:    one nonlinear operating-point solve
DC:    many operating-point solves over sweep values
AC:    one operating point, then one complex solve per frequency
TRAN:  many nonlinear solves over accepted/rejected timesteps
NOISE: operating point plus frequency-domain noise propagation
```

SpiceSharpParser's `*WithEvents` simulation wrappers and export objects hook into this
lifecycle so `.SAVE`, `.PRINT`, `.PLOT`, `.MEAS`, and `.FOUR` can collect data.

For a junior reader, the useful simplification is:

```text
setup builds the machine
execution runs the machine
exports read values from the machine
```

For `.TRAN`, the line "many nonlinear solves over accepted/rejected timesteps" means
there are two nested loops:

```text
outer loop:
  move forward in time

inner loop:
  solve the circuit at the current time guess
```

The outer loop chooses a candidate next time, for example from `1.0 us` to `1.1 us`.
The inner loop solves the circuit at `1.1 us`. If the solve converges and the timestep
looks accurate enough, that point is accepted. If not, SpiceSharp tries a smaller step,
for example from `1.0 us` to `1.05 us`.

So `.TRAN` is not simply:

```text
for t = 0 to stop:
  solve once
```

It is closer to:

```text
while time < stop:
  propose next time
  try to solve circuit there
  if the point is reliable:
    keep it
  else:
    retry with a smaller step
```

## Binding And Variable Allocation

Binding is the point where component pins become solver variables.

In a netlist, pins are strings:

```spice
R1 in out 1k
```

In the solver, strings are too slow and too vague. SpiceSharp turns them into indexed
variables:

```text
"in"  -> index 1
"out" -> index 2
```

SpiceSharp has to answer questions like:

```text
What is the solver index for node "out"?
Does this voltage source need a branch-current variable?
Does this model need an internal equation?
Which matrix entries should this behavior cache?
Which simulation states does this behavior need?
```

The `BindingContext` is the object that gives a behavior access to the simulation,
states, and variables during binding.

### Node Variables

A node pin such as `out` maps to a voltage variable:

```text
node "out" -> V(out) -> solver index k
```

All components connected to that same node share the same variable. This is how KCL
forms naturally: every component connected to `out` adds terms to row `out`.

This is similar to many objects holding a reference to the same shared object. The node
is shared; every connected component contributes to it.

### Branch Variables

Some devices need an extra current variable because their current cannot be expressed
directly from node voltages alone.

Common examples:

- independent voltage source,
- voltage-controlled voltage source,
- current-controlled voltage source,
- inductor,
- some dynamic or behavioral voltage-output devices.

The branch variable becomes both a column and a row:

```text
column: where the unknown branch current appears in KCL
row:    the voltage or device equation defining that branch
```

### Cached Matrix Elements

After a behavior knows its variable indices, it asks the solver for matrix and RHS
locations. Those locations are commonly stored in an `ElementSet<T>` or equivalent
references.

This is the performance trick to notice. The behavior does not say "find row p, column
n" every time. It caches the answer.

This is efficient because:

```text
setup cost happens once
load cost happens thousands or millions of times
```

During a transient simulation with many timesteps, a resistor does not rediscover
`Y[p,p]` on every timestep. It holds the element reference and only adds the current
conductance value.

### Local And Internal Variables

Some models introduce internal variables that are not directly visible as netlist nodes.
These may represent internal branches, transformed state equations, or model-specific
unknowns.

From the solver's perspective, they are ordinary variables:

```text
internal variable -> solver index -> row/column in MNA system
```

The difference is that user-facing exports usually target external node voltages,
source currents, or named properties rather than these internal implementation details.

## Convergence System

Linear circuits need one matrix solve. Nonlinear circuits need iteration.

If you have written an algorithm that keeps improving a guess until it is "close
enough", you already understand the basic shape of convergence.

SpiceSharp's nonlinear solve follows the Newton pattern:

```text
guess x0

repeat:
  linearize nonlinear devices around current guess
  load Jacobian matrix and RHS
  solve for a new x
  update device states
  test convergence
until converged or iteration limit is reached
```

The important point is that the matrix is not the original nonlinear equation. It is a
linear approximation that is valid near the current guess. Every nonlinear device must
cooperate by loading a good local Jacobian and by reporting whether it is converged.

Plain English:

```text
nonlinear device says:
  "near the current guess, behave like this simpler linear device"

solver says:
  "with those simpler devices, here is the next guess"
```

### Newton Iteration In Detail

Newton iteration is the bridge between nonlinear circuit equations and the linear
matrix solver. The sparse solver can solve this kind of problem:

$$
Yx = \text{rhs}
$$

But a nonlinear circuit is not naturally in that shape. A diode, BJT, MOSFET,
JFET, nonlinear behavioral source, or nonlinear passive may say:

$$
\text{current} = \text{nonlinear function of voltage/current}
$$

So the real problem is closer to:

$$
F(x) = 0
$$

where:

| Symbol | Meaning in a circuit |
|--------|----------------------|
| `x` | Unknown vector: node voltages plus branch currents. |
| `F(x)` | Equation error, also called residual. |
| `F(x) = 0` | KCL, voltage constraints, and model equations are all satisfied. |

The residual is "how wrong the circuit equations are at the current guess." If
KCL at a node says currents should sum to zero, but the current guess leaves
`1 mA` unbalanced, that `1 mA` is part of the residual.

Classic Newton writes one iteration like this:

$$
J(x_k)\Delta x = -F(x_k)
$$

$$
x_{k+1} = x_k + \Delta x
$$

where:

| Term | Meaning |
|------|---------|
| `x_k` | Current guess. |
| `F(x_k)` | Residual at the current guess. |
| `J(x_k)` | Jacobian matrix, the local derivative of `F` with respect to `x`. |
| `Delta x` | Correction that should improve the guess. |
| `x_{k+1}` | Next guess. |

Plain English:

```text
look at how wrong the current guess is
look at local slopes near that guess
solve for a correction
move the guess
repeat
```

SPICE-style MNA usually hides the explicit `Delta x` from you. Instead of
showing:

$$
J\Delta x = -\text{residual}
$$

devices stamp an equivalent linearized system:

$$
Yx_{\text{new}} = \text{rhs}
$$

Both views describe the same Newton idea. The delta view says "find the change."
The stamped MNA view says "find the next circuit state directly."

The device behavior is responsible for turning its nonlinear law into a local
linear stamp. At each Newton iteration, a nonlinear behavior typically does
this:

```text
read present guess from the simulation state
evaluate device current, charge, flux, or control expression
evaluate local derivatives
stamp derivatives into the matrix
stamp correction terms into the RHS
let the solver compute a new x
update cached device state
check whether the change is small enough
```

The most important derivative is "how much does this device current change if a
solver unknown changes?" For a one-port diode, that derivative is `dI/dV`. For a
MOSFET, there are many partial derivatives, such as drain current with respect
to gate voltage, drain voltage, source voltage, and bulk voltage.

#### A Tiny Newton Example

Before thinking about a whole circuit, solve one nonlinear equation:

$$
f(v) = v^2 - 2 = 0
$$

The true answer is:

$$
v = \sqrt{2} \approx 1.4142
$$

Newton uses the derivative:

$$
f'(v) = 2v
$$

Start with:

$$
v_0 = 1
$$

At the first guess:

$$
\begin{aligned}
f(v_0) &= 1^2 - 2 = -1 \\
f'(v_0) &= 2
\end{aligned}
$$

The correction is:

$$
\Delta v = -\frac{f(v_0)}{f'(v_0)}
          = -\frac{-1}{2}
          = 0.5
$$

So:

$$
v_1 = 1 + 0.5 = 1.5
$$

Next iteration:

$$
\begin{aligned}
f(1.5) &= 1.5^2 - 2 = 0.25 \\
f'(1.5) &= 3 \\
\Delta v &= -0.25 / 3 = -0.08333 \\
v_2 &= 1.41667
\end{aligned}
$$

Next iteration:

$$
\begin{aligned}
f(1.41667) &\approx 0.00694 \\
f'(1.41667) &\approx 2.83334 \\
\Delta v &\approx -0.00245 \\
v_3 &\approx 1.41422
\end{aligned}
$$

The pattern is:

```text
guess is rough
derivative gives a useful direction
correction gets smaller
guess settles near the true answer
```

Circuit Newton is the same idea, but `v` becomes a vector of many node voltages
and branch currents.

#### Newton In MNA Terms

For a circuit, the unknown vector might be:

$$
x =
\begin{bmatrix}
V(\text{in}) \\
V(\text{out}) \\
I(\text{V1})
\end{bmatrix}
$$

A nonlinear residual might include:

```text
KCL error at node in
KCL error at node out
voltage-source constraint error for V1
```

At Newton iteration `k`, SpiceSharp asks each behavior to load the local matrix
for the present guess `x_k`.

Linear devices load the same values every time:

| Device | Why the stamp is fixed during Newton |
|--------|--------------------------------------|
| Resistor | `i = g*v` already is linear. |
| Independent DC current source | RHS value is known. |
| Independent DC voltage source | Constraint value is known. |

Nonlinear devices load values that depend on the current guess:

| Device | What changes during Newton |
|--------|----------------------------|
| Diode | `gd`, equivalent current, junction terms. |
| BJT | Junction slopes, controlled-current slopes, RHS corrections. |
| MOSFET | `gm`, `gds`, body effect, junction slopes, charge terms. |
| Behavioral source | Expression value and derivatives. |
| Nonlinear `Q=` or `Flux=` device | Local derivative such as `dQ/dV` or `dPhi/dI`. |

This is why a Newton iteration reloads the matrix. The topology may be the same,
but the numeric coefficients are different because the nonlinear devices are
being re-approximated around a new guess.

#### Why The RHS Correction Exists

The RHS correction is the part that often feels mysterious. It exists because a
slope alone is not enough.

Suppose a nonlinear current is:

$$
i = f(v)
$$

At guess `v_k`, the tangent line is:

$$
i \approx f(v_k) + f'(v_k)(v - v_k)
$$

Rearrange:

$$
i \approx f'(v_k)v + \left(f(v_k) - f'(v_k)v_k\right)
$$

The first term is matrix-friendly:

```text
f'(v_k) * v
```

The second term is the correction:

```text
f(v_k) - f'(v_k) * v_k
```

Without that correction, the straight line would have the right slope but would
not pass through the actual nonlinear curve at the current guess. Newton needs
the tangent line, not just a line with the same slope.

For a diode:

$$
i_d \approx g_d v_d + i_{\text{eq}}
$$

where:

$$
i_{\text{eq}} = i_d(v_{d,k}) - g_d v_{d,k}
$$

Depending on the current direction convention, that equivalent current appears
with opposite signs in the two terminal RHS entries.

#### One Newton Iteration As A Conversation

The loop can be read as a conversation between device behaviors and the solver:

```text
simulation state:
  current guess is x_k

resistor:
  my slope is constant, I stamp the same conductance again

diode:
  at this voltage guess, my current is id and my slope is gd
  I stamp gd into Y and ieq into rhs

MOSFET:
  at this terminal-voltage guess, my gm/gds/body slopes are these values
  I stamp the local Jacobian and correction currents

solver:
  I solve the linear system and produce x_{k+1}

devices:
  I update my cached voltage/current values
  I compare old and new values against tolerances
```

If every nonlinear behavior says the change is small enough, Newton converged.
If any behavior says "not yet", the simulator reloads and solves again.

#### What Counts As Converged

Convergence is not the same as exact equality. Exact equality is impossible with
floating-point arithmetic and unnecessary for circuit simulation.

A practical convergence test asks questions like:

```text
did node voltages stop moving by a meaningful amount?
did branch currents stop moving by a meaningful amount?
are nonlinear device currents consistent with the latest voltages?
are residual errors small enough for the configured tolerances?
```

The important tolerance idea is:

```text
allowed error = absolute part + relative part
```

The absolute part protects values near zero. The relative part scales with large
signals. SpiceSharpParser maps common options such as `ABSTOL`, `RELTOL`, and
iteration limits into SpiceSharp simulation parameters.

#### Newton In `.OP`, `.DC`, And `.TRAN`

Newton appears in several analysis loops.

For `.OP`:

```text
choose initial guess
run Newton until the DC operating point converges
export final steady-state values
```

For `.DC`:

```text
for each sweep point:
  set source or parameter value
  often reuse previous sweep solution as the next initial guess
  run Newton
  export sweep value
```

For `.TRAN`:

```text
for each candidate timestep:
  build companion models from accepted history
  run Newton at this candidate time
  if Newton fails, reject or reduce the step
  if Newton converges, test timestep accuracy
  if timestep is accepted, commit history
```

This is why "Newton converged" is not the same thing as "the transient step was
accepted." Newton only proves that the algebraic equations were solved at that
candidate time. The timestep can still be rejected later if integration error is
too high.

#### Why Newton Can Fail

Newton works best when the local tangent points toward the real solution. It can
struggle when that local tangent is a poor guide.

Common causes:

| Cause | Why it hurts Newton |
|-------|---------------------|
| Floating node | Matrix has no solid equation for that voltage. |
| Ideal voltage-source loop | Conflicting constraints can make the matrix singular. |
| Ideal current-source cutset | KCL can become impossible or ill-conditioned. |
| Very steep diode or switch transition | Tiny voltage changes create huge slope changes. |
| Discontinuous behavioral expression | No useful derivative exists at the jump. |
| Bad initial guess | First tangent points far away from the real solution. |
| Extreme component ratios | Matrix becomes badly conditioned. |

The usual fixes match those causes:

```text
add a leakage path for floating nodes
avoid impossible ideal source networks
smooth behavioral expressions
use realistic Ron/Roff values
provide .NODESET or initial conditions
relax tolerances only when the requested accuracy allows it
increase iteration limits only after the model is physically reasonable
```

Iteration limits such as `ITL1`, `ITL2`, and `ITL4` prevent an endless loop. If
the limit is hit, the key question is not only "can I allow more iterations?"
It is also "why are the local linear approximations not settling?"

### What Linearization Means

Linearization means "replace a curve with a straight line near the current guess."

Imagine a nonlinear function:

$$
y = f(x)
$$

At the current guess `x0`, the device computes two things:

$$
\begin{aligned}
f(x_0) &= \text{value at the current guess} \\
\left.\frac{df}{dx}\right|_{x_0} &= \text{local slope at the current guess}
\end{aligned}
$$

Then it uses a straight-line approximation:

$$
f(x) \approx f(x_0) + \text{slope}\,(x - x_0)
$$

Rearranged:

$$
f(x) \approx \text{slope}\,x + \left(f(x_0) - \text{slope}\,x_0\right)
$$

That shape is perfect for MNA:

$$
\begin{aligned}
\text{slope} &\to \text{matrix} \\
f(x_0) - \text{slope}\,x_0 &\to \text{RHS correction}
\end{aligned}
$$

Plain English:

```text
matrix gets the local slope
RHS gets the correction that makes the straight line touch the curve
```

This is why you see names like `gd` and `ieq` for a diode:

$$
\begin{aligned}
g_d &= \text{local slope, resistor-like conductance} \\
i_{\text{eq}} &= \text{correction current source}
\end{aligned}
$$

### Example: Linearizing A Simple Function

Before looking at a diode, use a simpler fake nonlinear current:

$$
i = V(\text{out})^2
$$

Suppose the current guess is:

$$
V_{\text{guess}}(\text{out}) = 2
$$

The function value is:

$$
i(2) = 2^2 = 4
$$

The slope of `V^2` is `2*V`, so at `V = 2`:

$$
\text{slope} = 2 \cdot 2 = 4
$$

The straight-line approximation is:

$$
\begin{aligned}
i &\approx f(x_0) + \text{slope}\,(V - x_0) \\
i &\approx 4 + 4(V - 2) \\
i &\approx 4V - 4
\end{aligned}
$$

For this Newton iteration, the nonlinear device behaves like:

$$
\text{current} \approx 4V(\text{out}) - 4
$$

That means:

```text
matrix gets 4
RHS gets correction -4, with sign depending on source direction
```

After the solve, maybe the new value is:

$$
V_{\text{new}}(\text{out}) = 1.6
$$

The approximation was built around `2`, but the solver found `1.6`. The next iteration
linearizes around `1.6`:

$$
\begin{aligned}
i(1.6) &= 2.56 \\
\text{slope} &= 2 \cdot 1.6 = 3.2 \\
i &\approx 2.56 + 3.2(V - 1.6) \\
i &\approx 3.2V - 2.56
\end{aligned}
$$

So the device loads different matrix/RHS numbers on the next iteration.

### Example: Diode Linearization

A diode current is nonlinear. Conceptually:

$$
i_d = I_s\left(e^{v_d/(nV_t)} - 1\right)
$$

The exact formula is less important than the shape: current grows very quickly when the
diode voltage increases.

At the current Newton guess:

$$
v_{d,\text{guess}} = V(\text{anode}) - V(\text{cathode})
$$

the diode behavior computes:

$$
\begin{aligned}
i_d &= \text{diode current at } v_{d,\text{guess}} \\
g_d &= \text{diode current slope at } v_{d,\text{guess}}
\end{aligned}
$$

Then it computes the correction:

$$
i_{\text{eq}} = i_d - g_d v_{d,\text{guess}}
$$

For this iteration:

$$
\text{diode current} \approx g_dv_d + i_{\text{eq}}
$$

That looks like:

$$
\begin{aligned}
\text{resistor-like part} &: g_dv_d \\
\text{current-source part} &: i_{\text{eq}}
\end{aligned}
$$

So the diode can stamp a linear MNA system:

```text
Y[p,p] += gd
Y[p,n] -= gd
Y[n,p] -= gd
Y[n,n] += gd

rhs[p] -= ieq
rhs[n] += ieq
```

Then the solver finds a new diode voltage. The diode checks whether the new voltage and
current are close enough to the previous iteration. If not, it recomputes `gd` and
`ieq` around the new voltage and loads again.

Simplified iteration table:

| Iteration | Diode voltage guess | Loaded `gd` | Loaded `ieq` | Solver result | Stop? |
|-----------|---------------------|-------------|--------------|---------------|-------|
| 1 | far from final answer | rough slope | rough correction | big voltage change | no |
| 2 | closer | better slope | better correction | smaller change | no |
| 3 | close | accurate local slope | accurate correction | tiny change | yes |

The exact numbers depend on the diode model and circuit. The pattern is the important
part:

```text
guess -> local straight line -> solve -> better guess -> repeat
```

### Example: Behavioral Source Linearization

A behavioral current source can also be nonlinear:

```spice
B1 out 0 I={V(out)*V(out)}
```

That expression is the same fake example as above:

$$
i = V(\text{out})^2
$$

If the current guess is `V(out) = 2`, the behavior can linearize it as:

$$
i \approx 4V(\text{out}) - 4
$$

The `4*V(out)` part acts like a conductance in the matrix. The `-4` part acts like an
RHS correction. On the next Newton iteration, if the guess changes to `1.6`, the loaded
linear approximation changes too.

This is why nonlinear behavioral expressions can make convergence harder. If the
expression has sharp jumps, discontinuities, or very steep slopes, the "straight line
near the current guess" may be a poor guide for the next solve.

### What Convergence Means

Convergence usually means that relevant voltages and currents changed by less than the
configured absolute and relative tolerances. A device can compare:

- previous terminal voltage versus new terminal voltage,
- previous branch current versus new branch current,
- model-specific internal values,
- charge or dynamic state changes.

SpiceSharpParser maps `.OPTIONS abstol` and `.OPTIONS reltol` into the biasing
parameters used by SpiceSharp simulations. `abstol` protects small currents and values;
`reltol` scales tolerance with signal magnitude.

So convergence does not mean "exact". It means "close enough according to the requested
tolerances."

### Update Behaviors

After the solver finds a candidate solution, update behaviors copy the solution into
device state. This lets the next iteration use the latest voltage, current, charge, or
small-signal derivative values.

Conceptually:

```text
solve matrix
device.Update()
device.IsConvergent()
```

The update step and convergence test are separate because a model may need to cache the
new solution before deciding whether the change was acceptable.

### Gmin And Continuation

`gmin` is a small conductance used by biasing algorithms to avoid completely open
nonlinear junctions or singular intermediate systems. It can help Newton iteration find
a path toward the final solution.

Think of `gmin` as a tiny helper path that can make early guesses less fragile. It is
not a replacement for a valid circuit.

Continuation methods such as source stepping and gmin stepping are common SPICE
techniques:

```text
source stepping:
  solve an easier circuit with sources scaled down
  gradually scale sources up to final values

gmin stepping:
  solve with extra conductance support
  gradually reduce support toward final gmin
```

SpiceSharp has biasing support for these ideas, but this parser only maps a subset of
LTspice/PSpice solver options. Unsupported behavior-changing options are reported in
compatibility mode instead of silently pretending to match LTspice exactly.

### Iteration Limits

Iteration limits prevent infinite loops:

| Parser option | Meaning |
|---------------|---------|
| `itl1` | Operating-point maximum iterations. |
| `itl2` | DC sweep maximum iterations. |
| `itl4` | Transient Newton maximum iterations. |

When a limit is hit, the issue is usually one of:

- poor initial guess,
- discontinuous behavioral expression,
- floating node,
- unrealistic ideal source network,
- very stiff transient dynamics,
- model parameters outside a numerically reasonable range.

## Core Biasing Algorithm

Operating point, DC, transient, AC, and noise all depend on a biasing solution in some
way. A simplified Newton iteration is:

```text
create or reuse solver state
create device behaviors
apply temperature and parameter updates

for iteration = 1..maxIterations:
  clear Y and rhs numeric values

  for each biasing behavior:
    behavior.Load()

  factor Y
  solve Y * x = rhs

  for each update behavior:
    behavior.Update()

  if all convergence behaviors report converged:
    accept solution
    stop

report non-convergence
```

Linear circuits usually converge in one load/solve pass. Nonlinear circuits use Newton
linearization: each nonlinear device is replaced by a local linear approximation around
the current guess. The guess is solved, the device updates its local operating point,
and the loop repeats until changes are within tolerances.

If you only remember one thing from this section, remember this:

```text
biasing = find the steady-state starting point
```

Many other analyses need that starting point before they can do their own work.

## Analysis Algorithms

SpiceSharp supports several analysis types. Each one answers a different question:

| Analysis | Question it answers |
|----------|---------------------|
| `.OP` | What are the steady voltages and currents? |
| `.DC` | What happens when I sweep a source or parameter? |
| `.AC` | How does the circuit respond to tiny sine-wave signals at different frequencies? |
| `.TRAN` | What happens over time? |
| `.NOISE` | How much noise appears at the output? |

### Operating Point

`.OP` solves the DC steady state. Capacitors are open circuits and inductors are short
circuits in the DC limit, as implemented by their behaviors. Nonlinear devices iterate
until their voltages and currents converge.

```text
setup biasing state
run Newton loop
export final operating-point variables
```

### DC Sweep

`.DC` repeats the operating-point solve while changing one or more source or parameter
values.

```text
for each sweep point:
  set swept value
  use previous solution as initial guess when possible
  run Newton loop
  export sweep data
```

Using the previous point as a guess is important. Adjacent sweep points are usually
close, so convergence is easier than starting from zero each time.

Developer analogy: `.DC` is a loop around `.OP` with one input changed each iteration.

### AC Small-Signal

`.AC` first finds an operating point. Then nonlinear devices are linearized around that
point. The engine solves a complex-valued frequency-domain matrix at each frequency:

```text
solve operating point
linearize devices at that bias point

for each frequency:
  s = j * 2 * pi * frequency
  clear complex Y and rhs
  for each frequency behavior:
    behavior.Load()
  solve complex Y * x = rhs
  export complex voltages/currents
```

Independent AC source values populate the RHS. Capacitors stamp $sC$; inductors stamp
$sL$ in their branch equations or equivalent form.

AC analysis is not "simulate a waveform over time". It is more like asking:

```text
if I poke the circuit with a very small signal at this frequency,
how strongly does the output respond?
```

### Transient

`.TRAN` solves a sequence of time points. Dynamic elements are converted into companion
models by the active integration method. SpiceSharpParser can select trapezoidal, Gear,
or fixed Euler through `.OPTIONS method=...`.

The goal of `.TRAN` is to answer:

```text
what are the node voltages and branch currents as time moves forward?
```

A transient simulation is harder than `.OP` because some components remember the past.
A capacitor remembers previous voltage. An inductor remembers previous current. A
transmission line remembers delayed waves. A pulse source changes value at specific
times.

```text
optionally solve operating point
initialize integration history

while time < stopTime:
  choose timestep

  for Newton iteration at this timestep:
    clear Y and rhs
    load biasing and time behaviors
    factor and solve
    update nonlinear and integration states
    check convergence

  if timestep accepted:
    accept integration history
    export transient point
  else:
    reduce timestep and retry
```

Time-domain capacitors, inductors, transmission lines, Laplace sources, and waveform
sources all depend on current time and integration history.

Developer analogy: `.TRAN` is a game loop or animation loop, except each frame requires
solving a circuit.

More precise developer analogy:

```text
game loop:
  choose next frame time
  update world
  render frame

transient loop:
  choose next circuit time
  solve circuit
  accept/export point
```

The difference is that `.TRAN` may reject a frame. If the numerical result is not good
enough, SpiceSharp does not keep it. It shrinks the timestep and tries again.

### Noise

`.NOISE` uses the operating point and small-signal frequency-domain behavior. Noise
behaviors contribute source densities, and the engine propagates them through the
linearized circuit.

## Transient Integration Details

Transient analysis turns differential equations into algebraic MNA equations at each
time point. The integration method is the rule that performs that conversion.

For a more tutorial-style explanation with RC, RL, RLC, rectifier, `Q=`, and
`Flux=` examples, see [Transient Integration Methods And Engine Derivatives](transient-integration-methods.md).

If the word "differential" is uncomfortable, read it as "depends on how a value changes
over time." Capacitors and inductors remember history, so SpiceSharp needs a method for
turning that history into numbers for the current timestep.

Dynamic components do not just stamp fixed values. They depend on time history:

| Device | Dynamic quantity |
|--------|------------------|
| Capacitor | voltage history and charge/current relation. |
| Inductor | current history and flux/voltage relation. |
| Semiconductor junction | nonlinear charge and capacitance history. |
| Transmission line | delayed wave history. |
| Laplace source | transfer-function state and optional delay history. |

### Before The First Transient Step

The transient loop needs a starting point before it can move forward. There are
two related but different starting values:

| Starting value | What it seeds |
|----------------|---------------|
| Initial solution vector | The first guesses for node voltages and branch currents. |
| Initial dynamic history | Stored charge, flux, delay, or transfer-function state. |

Without `UIC`, the usual startup path is:

```text
solve DC operating point
use that solution as the time-zero circuit state
initialize capacitor, inductor, and internal dynamic histories from it
```

This gives `.TRAN` a physically steady starting point when the circuit has one.
For example, an ideal capacitor contributes no steady DC current during the
operating-point solve, but its terminal voltage from that solve still becomes the
initial charge history for transient analysis.

With `UIC`, the operating-point solve is skipped:

```text
read .IC statements and device IC= parameters
use those values as the starting state where applicable
initialize dynamic histories from those initial conditions
start the transient loop directly
```

That can intentionally start a circuit away from equilibrium, such as a charged
capacitor discharging through a resistor. It can also make the first few
timesteps harder, because the simulator did not first find a self-consistent DC
solution.

### What The .TRAN Parameters Control

The common syntax is:

```spice
.TRAN <tstep> <tstop> [<tstart> [<tmaxstep>]] [UIC]
```

These numbers do not all mean "take this exact solver step":

| Term | How it affects the loop |
|------|-------------------------|
| `tstep` | Output cadence and initial step hint. It is not a promise that every internal timestep equals this value. |
| `tstop` | Stop time for the transient analysis. |
| `tstart` | Time before which output is suppressed; the simulator still solves earlier points because they affect later history. |
| `tmaxstep` | Maximum internal timestep. This is the main user control for preventing the solver from jumping over fast behavior. |
| `UIC` | Skip the DC operating-point startup and seed transient state from initial conditions. |

The important distinction is output time versus internal time. A netlist can ask
for output every `1 us`, while the solver internally takes `200 ns`, `50 ns`, or
some other candidate step near a sharp event. Internal steps are about accuracy
and convergence. Output settings are about what results are reported.

### What Integration Methods Are For

Integration methods are the rules SpiceSharp uses to move a circuit forward in time.

In `.OP`, the simulator asks:

```text
what is the steady value?
```

In `.TRAN`, the simulator asks:

```text
what is the value at the next time point?
```

That is harder because some components depend on change over time.

Capacitor current depends on how voltage changes over time:

$$
i = C\frac{dv}{dt}
$$

Inductor voltage depends on how current changes over time:

$$
v = L\frac{di}{dt}
$$

The solver cannot directly put `dv/dt` or `di/dt` into the normal MNA matrix. It needs
ordinary algebra for the current timestep. An integration method converts "change over
time" into a temporary algebraic stamp.

Plain English:

```text
integration method =
  recipe for replacing time-derivative behavior
  with matrix/RHS numbers for this timestep
```

For a capacitor, the method creates:

```text
temporary conductance + history current
```

For an inductor, the method creates:

```text
temporary resistance-like branch term + history term
```

That is why integration methods are central to `.TRAN`: without them, SpiceSharp could
not turn capacitors, inductors, semiconductor charges, and other memory-based devices
into equations the solver can handle at each time point.

Simple analogy:

```text
You know where an object was before.
You know the timestep size.
An integration method estimates where it should be now.
```

Different methods make different tradeoffs:

| Method style | Basic tradeoff |
|--------------|----------------|
| Euler-like | Simple, but can need small steps. |
| Trapezoidal-like | More accurate for many smooth circuits. |
| Gear-like | More damping, often better for stiff circuits. |

### The Two Loops In .TRAN

`.TRAN` has an outer time loop and an inner solve loop.

Outer loop:

```text
pick the next candidate time
```

Inner loop:

```text
solve the circuit at that candidate time
```

Expanded:

```text
time = 0

while time < stopTime:
  candidateTime = chooseNextTime(time)

  prepare dynamic devices for candidateTime

  for iteration = 1..maxNewtonIterations:
    clear matrix and rhs
    load all component stamps for candidateTime
    solve Y * x = rhs
    update nonlinear devices

    if nonlinear devices converged:
      break

  if Newton did not converge:
    reject candidateTime
    reduce timestep
    retry

  if estimated time-integration error is too large:
    reject candidateTime
    reduce timestep
    retry

  accept candidateTime
  commit device history
  export values
  time = candidateTime
```

The inner Newton loop does not advance time. It keeps trying to solve the same
candidate time with better and better guesses:

```text
candidate time stays fixed
solution guess changes each Newton iteration
```

Read the inner loop like this:

| Step | Meaning |
|------|---------|
| `clear matrix and rhs` | Throw away the previous iteration's numeric matrix values. They were built around an older guess. |
| `load all component stamps for candidateTime` | Ask every component to contribute its current linearized equations for this time and this guess. |
| `solve Y * x = rhs` | Solve the temporary linear MNA system. The vector `x` contains node voltages and branch currents. |
| `update nonlinear devices` | Let nonlinear behaviors read the new solution and prepare their next local slopes, equivalent sources, and convergence checks. |
| `if nonlinear devices converged` | Stop iterating when the nonlinear device equations are no longer changing beyond tolerance. |

The repeated clearing and loading are necessary because nonlinear components do
not have one fixed stamp. A diode, for example, loads a conductance and
equivalent current source based on the present diode-voltage guess. After the
linear solve changes that voltage, the diode's local stamp may need to change
too.

For one candidate time, the loop is therefore:

```text
build a linear version of the nonlinear circuit
solve it
use that result as the next guess
repeat until the guess is self-consistent
```

### How Behaviors Read The New Solution

The pseudocode says `update nonlinear devices`, but most behaviors do not receive
the solved vector `x` as a method argument. Instead, during binding/setup they
cache variable objects that point into the solver state. After `solve Y * x =
rhs`, those variable objects expose the new solution values.

For a two-terminal device, reading the current voltage usually looks like:

```csharp
double voltage = Variables.Positive.Value - Variables.Negative.Value;
```

For a branch device, reading the current can look like:

```csharp
double current = Variables.Branch.Value;
```

Those `.Value` properties are the bridge from the solved MNA vector back into the
component behavior. The next time `Load()` runs, the behavior reads the updated
values and recomputes its local model.

A nonlinear diode-style behavior does this conceptually:

```csharp
double vd = Variables.PosPrime.Value - Variables.Negative.Value;

IdealDiodeEquation.Evaluate(
    Parameters,
    BiasingParameters,
    vd,
    out double cd,
    out double gd);
```

Here `cd` is the current at the present voltage guess and `gd` is the local
slope, `dI/dV`, at that same guess. The behavior then loads a linearized stamp
that represents:

```text
I(v) around this guess = conductance gd + equivalent current source
```

If the linear solve changes the diode voltage, the next Newton iteration reads
the new `Variables...Value`, recomputes `cd` and `gd`, clears the old stamp, and
loads the new local approximation.

A nonlinear dynamic device follows the same "read current guess, compute local
slope" pattern, but it also has stored history. The next section covers that
extra integration layer.

So the phrase "prepare the next local slopes" means:

```text
read the latest solved voltage/current values
evaluate the nonlinear equation at those values
compute the local derivative at those values
use that derivative when loading the next Newton matrix
```

### Nonlinear Devices With Integration History

Some devices are nonlinear but not dynamic. A diode's current depends on voltage,
so Newton needs a local slope `dI/dV`, but the diode does not by itself remember
time history.

Some devices are dynamic but linear. A normal capacitor has `q = C*v`, so it
needs integration history, but its local slope `dq/dv = C` is constant.

The interesting case is a device that is both nonlinear and dynamic. Examples
include semiconductor junction charges, a nonlinear `Q=` capacitor, or a
nonlinear `Flux=` inductor. These devices need two mechanisms at the same time:

| Mechanism | What it uses | What it answers |
|-----------|--------------|-----------------|
| Newton linearization | Current voltage/current guess and local derivatives. | How should this nonlinear equation be approximated for this iteration? |
| Integration method | Current candidate value, timestep, and accepted history. | How should this stored quantity change over time? |

For a nonlinear charge-defined capacitor:

$$
Q = Q(V)
$$

and:

$$
i = \frac{dQ}{dt}
$$

At each Newton iteration for a candidate timestep, the behavior reads the latest
voltage guess and computes:

```text
present stored quantity: Q(V_guess)
local Newton slope:      dQ/dV at V_guess
time derivative:         dQ/dt from integration history
```

In code, the custom nonlinear capacitor pattern is:

```csharp
double voltage = Voltage;
double charge = EvaluateCharge(voltage);
double capacitance = EvaluateChargeDerivative(voltage);

_chargeState.Value = charge;
_chargeState.Derive();
JacobianInfo info = _chargeState.GetContributions(capacitance, voltage);
```

Each line has a different job:

| Line | Meaning |
|------|---------|
| `EvaluateCharge(voltage)` | Evaluate the stored quantity at the current Newton guess. |
| `EvaluateChargeDerivative(voltage)` | Compute the local slope `dQ/dV` for the Newton Jacobian. |
| `_chargeState.Derive()` | Ask the active integration method to compute `dQ/dt` from candidate value plus history. |
| `GetContributions(capacitance, voltage)` | Combine the local slope and integration history into matrix/RHS contributions. |

The integration method owns the time-history part. The nonlinear behavior owns
the local derivative part. `GetContributions(...)` is where those two meet.

Conceptually, the integration method creates a shape like:

$$
\frac{dQ}{dt} \approx a_0 Q_n + \text{history}
$$

Newton then linearizes the nonlinear stored quantity around the current guess:

$$
Q(V) \approx Q(V_k) + \frac{dQ}{dV}\bigg|_k (V - V_k)
$$

So the matrix coefficient is based on:

```text
integration coefficient * local derivative
```

For a nonlinear capacitor, that means:

```text
Jacobian contribution ~ a0 * dQ/dV
RHS contribution      ~ accepted history plus linearization correction
```

For a nonlinear flux-defined inductor, the mirror image is:

```csharp
double current = Current;
double flux = EvaluateFlux(current);
double inductance = EvaluateFluxDerivative(current);

_fluxState.Value = flux;
_fluxState.Derive();
JacobianInfo info = _fluxState.GetContributions(inductance, current);
```

Here the stored quantity is flux:

$$
\Phi = \Phi(I)
$$

and the local slope is:

$$
\frac{d\Phi}{dI}
$$

The integration method turns `dPhi/dt` into an inductor voltage contribution, and
the local slope tells Newton how that voltage contribution changes when the
branch-current guess changes.

During a candidate timestep, accepted history is still the last trusted history.
Newton iterations may update the candidate charge or flux values many times, but
those values do not become history until the timestep is accepted:

```text
accepted history:
  fixed while solving candidate time

current Newton guess:
  changes each iteration

local derivative:
  recomputed from the current guess

history commit:
  happens only after the timestep is accepted
```

This is the main relationship:

```text
nonlinear device:
  supplies present stored value and local slope

integration method:
  supplies timestep coefficient and accepted-history terms

Newton matrix:
  receives their combined Jacobian and RHS contribution
```

This explains the phrase "many nonlinear solves over accepted/rejected timesteps":

```text
many timesteps
  each timestep may need many Newton iterations
  each timestep may be accepted or rejected
```

### What Gets Rebuilt At A Candidate Time

At a candidate time, some information is fixed for the whole solve attempt, while
other information can change on every Newton iteration.

Fixed for the candidate attempt:

| Value | Why it is fixed during the attempt |
|-------|------------------------------------|
| Candidate time `t_n` | The solver is trying to solve the circuit at that one time. |
| Timestep `h = t_n - t_{n-1}` | Integration coefficients are built for that attempted time jump. |
| Accepted history | Last trusted charge, flux, delay, or internal state from the previous accepted point. |
| Waveform time argument | Sources such as `PULSE`, `SIN`, and `PWL` are evaluated at the candidate time. |

Reloaded during Newton iterations:

| Value | Why it can change |
|-------|-------------------|
| Nonlinear currents and conductances | They depend on the current voltage/current guess. |
| Equivalent Newton sources | They are recomputed from the latest local linearization. |
| Nonlinear charge or flux slopes | `dQ/dV` or `dPhi/dI` can change with the current guess. |
| RHS residual corrections | The mismatch changes as Newton moves the solution. |

The matrix structure is usually stable: device pins, branch variables, and cached
sparse-matrix locations were allocated during setup. The numbers loaded into
those locations can change many times:

```text
same topology
same cached matrix locations
new source values, companion coefficients, nonlinear slopes, and RHS values
```

This is why transient analysis can reuse a lot of setup work while still
reloading the matrix at every Newton iteration.

### RC Charging Example

Consider a resistor charging a capacitor:

```spice
V1 in 0 PULSE(0 5 0 1n 1n 1m 2m)
R1 in out 1k
C1 out 0 1u
.TRAN 1u 5m
```

At each candidate time, the capacitor is replaced by a temporary companion model:

$$
i(t) \approx g_{\text{eq}}v(t) + i_{\text{history}}
$$

At time `0`, the capacitor may start near `0 V`. When the pulse source jumps toward
`5 V`, the capacitor voltage cannot jump instantly. SpiceSharp advances time in steps:

```text
t = 0 us     V(out) = 0.00 V
t = 1 us     V(out) = small increase
t = 2 us     V(out) = larger increase
...
```

The exact timestep is not always the print step from `.TRAN`. The print step says when
you want output. The internal solver may use smaller or different timesteps to keep the
solution accurate.

Near the sharp PULSE edge, SpiceSharp may need smaller steps:

```text
try step to 1.0 us:
  edge is sharp, estimated error too high
  reject

try step to 0.5 us:
  still too high
  reject

try step to 0.25 us:
  acceptable
  accept and commit capacitor history
```

Those numbers are illustrative. The key idea is that rejection is not a crash. It is
the simulator protecting accuracy and convergence.

### Integration Method State

SpiceSharp exposes integration methods through `IIntegrationMethod`. Common methods are:

| Method | Character |
|--------|-----------|
| `Trapezoidal` | Accurate for many circuits, but can show numerical ringing in stiff circuits. |
| `Gear` | More damping, often better for stiff circuits. |
| `FixedEuler` | Simple fixed-step method, useful for predictable stepping but less accurate. |
| `FixedTrapezoidal` | Fixed-step trapezoidal variant. |

SpiceSharpParser maps:

```text
.OPTIONS method=trap
.OPTIONS method=trapezoidal
.OPTIONS method=gear
.OPTIONS method=euler
```

to the corresponding SpiceSharp time-parameter factory.

### Companion Models

The integration method rewrites derivatives into algebraic companion models. For a
capacitor:

$$
i = C\frac{dv}{dt}
$$

becomes, for one timestep:

$$
i \approx g_{\text{eq}}v + i_{\text{history}}
$$

For an inductor:

$$
v = L\frac{di}{dt}
$$

becomes:

$$
v \approx r_{\text{eq}}i + v_{\text{history}}
$$

The exact coefficients depend on the method and timestep. The important concept is that
the dynamic device becomes a matrix stamp plus RHS history source for the current solve.

This is the main trick of transient simulation:

```text
hard time-dependent component
  -> temporary simpler component for this timestep
```

Companion models are broader than only `C` and `L`. Nonlinear devices also load
temporary Newton companions, such as a diode's local conductance plus equivalent
current source. Dynamic nonlinear devices can combine both ideas: local
linearization derivatives for the current Newton guess and integration-history
terms from previous accepted timesteps.

For the fuller companion-model catalog, see
[Transient Integration Methods](transient-integration-methods.md#companion-model-families).

### Accepted And Rejected Timesteps

Transient analysis distinguishes a candidate timestep from an accepted timestep.

This is like optimistic execution. SpiceSharp tries a step, checks whether it is good
enough, and either commits it or rolls it back and tries a smaller step.

```text
try timestep:
  predict/prepare history
  solve nonlinear MNA system
  estimate truncation error

if acceptable:
  accept solution
  commit integration history
  export point
else:
  reject solution
  reduce timestep
  retry
```

`IAcceptBehavior` lets devices commit state after a successful point. `ITruncatingBehavior`
lets devices participate in timestep control by reporting how aggressively the timestep
should be limited.

Truncation error is not the same thing as Newton non-convergence. They are two
different checks:

| Check | Meaning |
|-------|---------|
| Newton convergence | The loaded algebraic equations were solved at the candidate time. |
| Truncation-error control | The time jump from the previous accepted point is accurate enough. |

Newton works at one candidate time. It asks whether the current matrix, RHS,
companion models, and nonlinear linearizations produce a consistent set of node
voltages and branch currents.

Truncation-error control works across time. It asks whether the candidate step
was small enough for the integration method to follow capacitor charge, inductor
flux, and other dynamic histories accurately.

A candidate point can therefore solve successfully and still be rejected. That
means the equations at that candidate time were consistent, but the time jump
from the previous accepted point was too coarse to trust.

Accepted means:

```text
the solution converged
the estimated integration error is acceptable
device histories are committed
exports may be produced
the simulation time moves forward
```

Rejected means:

```text
do not commit device histories
do not trust this candidate point
make the timestep smaller
try again from the last accepted time
```

This distinction matters for capacitors and inductors. Their history must only be
updated when a timestep is accepted. If SpiceSharp committed history from a rejected
point, the next solve would be based on a state that the simulator already decided was
not reliable.

Small timeline example:

```text
accepted time: 10.0 us

try 12.0 us:
  Newton converges, but truncation error too high
  reject 12.0 us

try 11.0 us:
  Newton converges, error acceptable
  accept 11.0 us

next start point is now 11.0 us
```

There may also be Newton rejection:

```text
try 12.0 us:
  Newton iteration does not converge
  reject 12.0 us
  retry with smaller step
```

That is why transient simulation can slow down near switching events, sharp waveform
edges, or strongly nonlinear behavior. The simulator is doing extra inner solves to
find a trustworthy next point.

### What Truncation Means In .TRAN

In transient analysis, "truncation" usually means **timestep truncation**. The
simulator is not clipping a voltage or current value. It is limiting how far time
is allowed to jump.

The reason is local truncation error. An integration method represents the real
continuous-time curve using only a finite amount of history. For example, a
capacitor's real charge may bend between two time points, but the integration
method only sees accepted history values and derivatives. The part of the curve
that the method cannot represent is the truncation error.

Plain-language picture:

```text
real waveform:
  smooth curve through time

integration method:
  finite-step approximation from stored history

truncation check:
  is this time jump small enough for that approximation?
```

SpiceSharp uses truncation in two places around the transient loop.

Before trying the next point, truncating behaviors can limit the next proposed
timestep:

```text
for each truncating behavior:
  allowedStep = behavior.Prepare()
  integration method caps the next timestep to that value
```

This is useful for things like sampler points, waveform breakpoints, or device
state that knows the solver should not step past a particular time.

After Newton converges at a candidate point, the integration method and
truncating behaviors evaluate whether the solved point was accurate enough:

```text
Newton converged at candidate time
ask truncating behaviors for their allowed timestep
ask integration method whether the local error is acceptable

if acceptable:
  accept point and commit history
else:
  reduce timestep and retry from the last accepted time
```

The integration method can estimate local error from its tracked states. For a
capacitor, that state is charge; for an inductor, it is flux. The method compares
the solved value and history against the error tolerances and computes a maximum
timestep it would trust.

The result is an allowed timestep, not a corrected voltage:

```text
local error small enough:
  keep the point
  maybe allow the next timestep to grow

local error too large:
  reject the point
  shrink the timestep
  solve again
```

This is why a `.TRAN` run can show many rejected points near a sharp source edge
or switching event even when Newton itself converges. Newton answered "the
circuit equations balance at this candidate time." Truncation answered "the jump
from the previous accepted time to this one was too large to trust."

### Export Timing And Saved Values

Exports read the solved simulation state after a transient point is accepted.
Rejected candidate points are not exported because their histories are not
committed and the simulator has decided not to trust them.

`.SAVE`, `.PRINT`, `.PLOT`, and `.MEAS` choose which values are observed or
post-processed. They do not normally change the equations the solver must solve.
The solver still computes the full circuit state needed by the devices; the
export configuration only controls what is reported back to the caller or used by
measurement logic.

When `tstart` is present, early solved points still matter. A capacitor charged
before `tstart` still carries that charge into later output times. `tstart`
suppresses early reporting; it does not skip the physics before that time.

### Breakpoints And Discontinuities

Waveform sources and piecewise expressions can introduce discontinuities. The transient
solver should take points at important breakpoints so it does not step over sharp source
changes.

Examples:

- PULSE edge start/end times,
- PWL point times,
- delayed-source transition times,
- switching thresholds,
- transmission-line delayed events.

When a circuit has sharp discontinuities, the matrix can change abruptly. That often
causes smaller timesteps and more Newton iterations near the event.

Breakpoints do not make a discontinuity smooth. They only tell the timestepper
where important time points are. If a source has a very fast but finite edge, a
reasonable `tmaxstep` may still be needed so the accepted history has enough
points along that edge.

## AC Small-Signal Linearization

AC analysis is not a large-signal sinusoidal transient simulation. It is a small-signal
linear analysis around a DC operating point.

"Small-signal" means the circuit is assumed to move only a tiny amount around the DC
operating point. That lets nonlinear devices be replaced by linear approximations.

The flow is:

```text
1. Solve DC operating point.
2. Freeze nonlinear devices at that bias point.
3. Convert nonlinear devices to small-signal linear equivalents.
4. For each frequency, solve a complex MNA system.
```

A diode in AC is not loaded as an exponential current equation. It is loaded as its
small-signal conductance and capacitance at the operating point. A transistor becomes a
network of conductances, transconductances, capacitances, and controlled sources.

### Frequency-Domain Matrix

In AC, the matrix values are complex:

$$
s = j\omega = j\,2\pi f
$$

Typical dynamic stamps become:

| Device | Frequency-domain relation |
|--------|---------------------------|
| Capacitor | $Y = sC$ |
| Inductor | $Z = sL$, usually branch-equation form |
| Transmission line | frequency-dependent two-port relation |
| Laplace source | $H(s)$ evaluated at current $s$ |

The solve at each frequency is independent after the operating point is known:

```text
for each frequency:
  set complex frequency state
  load complex matrix
  solve
  export magnitude/phase/real/imaginary data
```

### AC Sources

Only AC source values drive the small-signal RHS. A source with DC value but no AC
value biases the circuit but does not inject a small-signal AC excitation.

This is why a source line often looks like:

```spice
V1 IN 0 DC 5 AC 1
```

The `DC 5` part affects the operating point. The `AC 1` part drives the frequency
solve.

## Model System

SPICE distinguishes component instances from models. In a netlist:

```spice
D1 OUT 0 DMOD
.MODEL DMOD D(IS=1e-14 N=1)
```

`D1` is the device instance. `DMOD` is the model. The instance says where the diode is
connected; the model says which parameter set and equations are used.

Software analogy:

```text
instance = object placed in the circuit
model    = shared configuration/type data
```

SpiceSharp follows the same idea:

```text
component instance
  -> points to model name
  -> owns instance parameters

model entity
  -> owns model parameters
  -> provides model behavior/parameter data
```

### Temperature-Dependent Parameters

Many models are temperature dependent. Before the main solve, temperature behavior can
derive effective parameters from nominal values, circuit temperature, and instance
temperature.

You do not need to know the physics to understand the timing: temperature-adjusted
values must be computed before the matrix is loaded, because the stamp uses those
adjusted values.

Examples:

- diode saturation current changes with temperature,
- resistor model temperature coefficients alter resistance,
- BJT and MOSFET model quantities are adjusted for temperature,
- capacitance and charge equations may depend on thermal voltage.

This is why `.TEMP`, `.OPTIONS temp=...`, and `.OPTIONS tnom=...` are handled before
the numerical solve. Matrix stamps should use the effective temperature-adjusted
parameters, not just the raw netlist values.

### Model-Dependent Stamps

A model is not usually a stamp by itself. It supplies equations and parameters used by
the instance behavior. The instance behavior then loads the matrix.

For simple models, this is almost direct:

```text
resistor model -> adjusted resistance -> conductance stamp
```

For semiconductor models, it is richer:

```text
model parameters + terminal voltages + temperature
  -> currents, charges, derivatives
  -> Jacobian matrix terms + RHS residual terms
```

That is why the component atlas describes semiconductor stamps conceptually. The exact
terms depend on model equations and operating region.

## Parameter System

SpiceSharp stores parameters in parameter sets. A parameter set is a structured object
with named properties such as resistance, capacitance, model coefficients, simulation
tolerances, and integration settings.

For a developer, this is the bridge between text and strongly typed objects:

```text
netlist text -> parsed value -> SpiceSharp parameter set
```

Important concepts:

| Concept | Role |
|---------|------|
| `ParameterSet` | Default reflection-backed parameter container. |
| `IParameterized<T>` | Object that exposes a strongly typed parameter set. |
| `IImportParameterSet<T>` | Supports setting parameters by name. |
| `IExportPropertySet<T>` | Supports reading/exporting properties by name. |
| `ParameterSetCollection` | Groups multiple parameter sets. |

SpiceSharpParser uses this system when it translates netlist values into SpiceSharp
objects. For example:

```text
R1 in out 1k
  -> Resistor entity
  -> resistance parameter set to 1000

.OPTIONS reltol=1e-4
  -> simulation biasing parameter set updated
```

### Parser Expressions Versus Engine Parameters

SpiceSharpParser must evaluate SPICE expressions before it can set many SpiceSharp
parameters. Some values are simple constants:

```spice
R1 in out 1k
```

Others depend on `.PARAM`, `.FUNC`, stochastic functions, `.STEP`, temperature, or
simulation context:

```spice
.PARAM rload=1k
R1 out 0 {rload}
```

The parser builds evaluation contexts and simulation preparation actions so parameters
that depend on sweeps or simulation-specific values can be updated at the correct time.

The important bit: not every parameter is known once at parse time. Some values must be
re-evaluated for each sweep, Monte Carlo run, temperature, or simulation.

### Import And Export Names

Parameter names are not always the same as SPICE syntax tokens. The parser maps SPICE
syntax onto SpiceSharp parameter names. For example, a resistor's netlist value becomes
the entity's resistance parameter. `.OPTIONS abstol` becomes a biasing tolerance.

Exports use the reverse idea: user-facing names such as `V(out)` or `I(V1)` are mapped
to simulation variables or entity properties that can be read during export events.

## Exports And Events

SpiceSharp simulations produce data while they run. SpiceSharpParser exposes that data
through exports and event handlers.

If the solver is the engine, exports are the dashboard. They read values from the
current simulation state.

Typical usage:

```csharp
var export = spiceModel.Exports.Find(e => e.Name == "V(OUT)");
sim.EventExportData += (sender, args) =>
{
    Console.WriteLine(export.Extract());
};
sim.Execute(spiceModel.Circuit);
```

The export is not just a stored number. It is an extractor that reads the current
simulation state when the simulation fires an export event.

That is why you attach to `EventExportData`: the value is meaningful at specific points
in the simulation loop.

### Why Events Matter

Different analyses export at different times:

| Analysis | Export timing |
|----------|---------------|
| `.OP` | Once after the operating point is solved. |
| `.DC` | Once per sweep point. |
| `.AC` | Once per frequency point. |
| `.TRAN` | Once per accepted output/time point. |
| `.NOISE` | Once per frequency/noise point. |

The event model lets user code collect data without forcing the simulator to allocate
one large result table for every possible use case.

### Export Kinds

SpiceSharpParser creates export objects for statements such as `.SAVE`, `.PRINT`,
`.PLOT`, `.MEAS`, and `.FOUR`.

Common export types:

- node voltage, such as `V(out)`,
- differential voltage, such as `V(out,in)`,
- source or branch current, such as `I(V1)`,
- real/imaginary/magnitude/phase AC values,
- device or model properties,
- noise quantities,
- expression-based exports.

Measurements and plots are layered on top of simulation events. They observe exported
values as the simulation runs and compute derived data after enough points are
available.

## Custom Component Extension Points

SpiceSharp is designed as a library, not only as a fixed executable simulator. Custom
components and models can participate in the same behavior and MNA system as built-in
devices.

This is useful even if you never write a custom component. It explains why the built-in
components are structured the way they are.

A custom component usually needs:

1. An entity class with name, pins, and parameter sets.
2. One or more behavior classes.
3. Binding code that maps pins to variables and allocates matrix locations.
4. Load code that stamps the matrix/RHS.
5. Optional update, convergence, temperature, time, frequency, noise, accept, or
   truncation behavior.

### Minimal Biasing Device Shape

Conceptually, a custom linear two-terminal conductance needs:

```text
entity:
  name
  positive node
  negative node
  conductance parameter

binding:
  get p and n voltage variables
  cache Y[p,p], Y[p,n], Y[n,p], Y[n,n]

load:
  add +g, -g, -g, +g
```

A nonlinear custom device adds:

```text
update:
  read solved voltages/currents

convergence:
  compare previous and current values

load:
  compute local Jacobian and RHS residual
```

A dynamic custom device adds:

```text
time behavior:
  register integration states
  compute companion-model coefficients

accept behavior:
  commit accepted history

truncating behavior:
  limit timestep when local error is too high
```

### What To Keep Stable

For custom devices, the most important performance rule is the same as for built-in
devices:

```text
allocate structure during setup
change numbers during load
```

Do not perform expensive node-name lookups, matrix location discovery, or expression
parsing inside every `Load()` call if the result can be cached during binding or setup.

## Parser-Configurable Solver Settings

SpiceSharpParser maps these `.OPTIONS` values into SpiceSharp simulation settings:

| Option | Effect |
|--------|--------|
| `abstol` | Absolute tolerance for biasing convergence. |
| `reltol` | Relative tolerance for biasing convergence and selected integration methods. |
| `gmin` | Minimum conductance used by biasing algorithms. |
| `itl1` | DC operating-point maximum iterations. |
| `itl2` | DC sweep maximum iterations. |
| `itl4` | Transient maximum iterations. |
| `method=trap` or `method=trapezoidal` | Use trapezoidal integration. |
| `method=gear` | Use Gear integration. |
| `method=euler` | Use fixed Euler integration. |

LTspice options such as `pivrel`, `pivtol`, `gminsteps`, and `srcsteps` are recognized
as behavior-changing solver options in compatibility mode, but this parser does not map
them yet.

## Component Stamp Atlas

This section shows the matrix contribution of each supported component category. For
linear ideal components, the stamp is exact. For model-dependent devices, the section
describes the local Jacobian and dynamic contribution rather than pretending that every
model variant has the same closed-form matrix.

Do not try to memorize this section. Use it like a dictionary:

1. Find the component type.
2. Read the plain meaning.
3. Look at which matrix/RHS entries it touches.
4. Notice whether the stamp is exact or model-dependent.

When a stamp says `Y[p,n] += -g`, read it as:

```text
in the equation for node p,
the voltage at node n contributes with coefficient -g
```

### R: Resistor

A resistor between `p` and `n` with resistance `R` has conductance:

$$
g = \frac{1}{R}
$$

Stamp:

| Location | Add |
|----------|-----|
| `Y[p,p]` | $+g$ |
| `Y[p,n]` | $-g$ |
| `Y[n,p]` | $-g$ |
| `Y[n,n]` | $+g$ |

This expresses:

$$
I(p \to n) = g\left(V(p) - V(n)\right)
$$

If either node is ground, its index is 0 and the corresponding ground entries are
ignored by the real equation system.

Beginner meaning: a resistor connects two nodes and lets current flow based on the
voltage difference. The four matrix entries are just the two-node KCL bookkeeping.

### I: Independent Current Source

A DC current source from `p` to `n` contributes only to the RHS:

| Location | Add |
|----------|-----|
| `rhs[p]` | $-I$ |
| `rhs[n]` | $+I$ |

In AC, the AC magnitude and phase are used for the complex RHS. In transient analysis,
waveforms such as `PULSE`, `SIN`, `PWL`, `SFFM`, and `AM` compute a time-dependent
current and stamp that current into the RHS at each time point.

Beginner meaning: a current source pushes a known current into the circuit. Because the
current is already known, it goes on the known side (`rhs`) rather than creating a new
unknown.

### V: Independent Voltage Source

An ideal voltage source cannot be represented as a simple conductance. MNA adds an
extra branch-current unknown `b = I(Vsrc)`.

Unknowns:

$$
V(p),\quad V(n),\quad I(\text{Vsrc})
$$

Stamp:

| Location | Add |
|----------|-----|
| `Y[p,b]` | $+1$ |
| `Y[n,b]` | $-1$ |
| `Y[b,p]` | $+1$ |
| `Y[b,n]` | $-1$ |
| `rhs[b]` | $+V_{\text{src}}$ |

The node rows inject the branch current into KCL. The branch row enforces:

$$
V(p) - V(n) = V_{\text{src}}
$$

In transient analysis, waveform voltage sources update `Vsrc` at each time point. In AC,
the source uses its complex AC value.

Beginner meaning: a voltage source forces a voltage difference. The simulator must add
one extra unknown because the source current is whatever it needs to be to enforce that
voltage.

### C: Capacitor

A capacitor has current:

$$
i = C\frac{d v(p,n)}{dt}
$$

Plain-language model:

```text
terminal voltage -> stored charge -> capacitor current
v                -> q = C*v       -> i = dq/dt
```

SpiceSharp cannot directly put `dq/dt` into the linear system. During
transient analysis it uses the active integration method to convert that
derivative into algebraic terms for the current candidate timestep. The result
looks like this to the solver:

$$
i_n \approx g_{\text{eq}}v_n + i_{\text{history}}
$$

So a capacitor in `.TRAN` is easiest to imagine as:

```text
temporary resistor-like stamp + remembered-history current source
```

The remembered quantity is charge. The voltage is the unknown solved by the
node equations, the charge is $q = Cv$, and the integration method computes
the current from the charge history.

The key rearrangement is easiest to see with backward Euler. Suppose the solver
is trying candidate timestep `n`, and the previous accepted timestep is `n-1`.
The previous capacitor voltage is already known:

```text
known history: v[n-1]
unknown now:   v[n]
```

Backward Euler says:

$$
i_n \approx \frac{q_n - q_{n-1}}{h}
$$

For a linear capacitor:

$$
q_n = C v_n
$$

So:

$$
i_n \approx \frac{C v_n - C v_{n-1}}{h}
$$

Split that into the part that contains the unknown and the part that is already
known:

$$
i_n \approx \frac{C}{h}v_n - \frac{C}{h}v_{n-1}
$$

That split is the matrix/RHS split:

| Term | Why it goes there |
|------|-------------------|
| $(C/h)v_n$ | Contains the unknown voltage, so it becomes a matrix coefficient. |
| $-(C/h)v_{n-1}$ | Uses accepted history, so it becomes a RHS/history source term. |

This is why the capacitor can be stamped like a conductance for the current
timestep even though it is physically storing charge.

In SpiceSharp 3.2.3 the built-in capacitor behavior stack is:

| Behavior | Interfaces | Main job |
|----------|------------|----------|
| `Capacitors.Temperature` | `ITemperatureBehavior` | Computes the effective capacitance from parameters, temperature, and multipliers. |
| `Capacitors.Frequency` | `IFrequencyBehavior` | Stamps the complex AC admittance $sC$. |
| `Capacitors.Time` | `ITimeBehavior`, `IBiasingBehavior` | Owns charge history and stamps the transient companion model. |

There is no separate public `Capacitors.Biasing` type in this SpiceSharp version.
The transient behavior, `Capacitors.Time`, also implements `IBiasingBehavior`.
That is why the same behavior can load the DC/open-circuit part of the capacitor
and, during transient analysis, add the integration companion terms.

The important runtime pieces are:

| Concept | Meaning |
|---------|---------|
| One-port variables | The positive and negative node voltages used to compute $v = V(p)-V(n)$. |
| `ElementSet` | Cached sparse-matrix/RHS locations for fast repeated loading. |
| `IBiasingSimulationState` | Real-valued solver state used by `.OP`, `.DC`, and `.TRAN` Newton solves. |
| `ITimeSimulationState` | Tells the behavior whether transient is using DC initialization or `UIC`. |
| `IDerivative _qcap` | Integration state that stores capacitor charge and computes $dq/dt$. |

Temperature/effective value:

```text
instance/model parameters
  -> temperature behavior
  -> effective capacitance C
```

For an ordinary linear capacitor, the stored quantity is:

$$
q = C v
$$

The time-domain current is:

$$
i = \frac{dq}{dt}
$$

Bias / operating point:

```text
ideal capacitor -> open circuit for DC current
```

In a DC operating point, nothing is changing with time, so an ideal capacitor
does not pass steady-state current. That is the meaning of "capacitor is open
in DC". The capacitor can still receive an initial voltage for later transient
history; it just does not add an ordinary DC conductance between its terminals.

In `.OP`, the ideal capacitor contributes no DC conductance between its terminals.
However, transient setup still needs a starting stored charge. Without `UIC`, that
starting charge comes from the operating-point voltage. With `UIC`, an `IC=`
parameter can seed the initial terminal voltage and therefore the initial charge.

AC:

$$
y = sC
$$

The AC stamp is resistor-like with `g` replaced by complex admittance `y`:

| Location | Add |
|----------|-----|
| `Y[p,p]` | $+sC$ |
| `Y[p,n]` | $-sC$ |
| `Y[n,p]` | $-sC$ |
| `Y[n,n]` | $+sC$ |

The frequency behavior also exposes complex voltage, current, and power exports.
Conceptually:

$$
I = s C V
$$

So `.AC` does not use timestep history. It uses the frequency point through
$s = j\omega$.

Transient:

The integration method turns the capacitor into a companion conductance plus a history
current source:

$$
i(t) \approx g_{\text{eq}}v(t) + i_{\text{history}}
$$

Stamp:

| Location | Add |
|----------|-----|
| `Y[p,p]` | $+g_{\text{eq}}$ |
| `Y[p,n]` | $-g_{\text{eq}}$ |
| `Y[n,p]` | $-g_{\text{eq}}$ |
| `Y[n,n]` | $+g_{\text{eq}}$ |
| `rhs[p]` | history-current contribution |
| `rhs[n]` | opposite history-current contribution |

The time behavior does this conceptually at each candidate timestep:

```text
read current terminal voltage v
compute charge q = C * v
store q in IDerivative
ask integration method to derive dq/dt
ask IDerivative for Jacobian/RHS contributions
stamp conductance-like matrix entries and history current
```

`geq` and `ihistory` depend on the integration method and previous accepted capacitor
voltage. For backward Euler, conceptually:

$$
\begin{aligned}
g_{\text{eq}} &= \frac{C}{\Delta t} \\
i_{\text{history}} &= -g_{\text{eq}}v_{\text{previous}}
\end{aligned}
$$

Trapezoidal and Gear use different coefficients and more history.

Beginner meaning: a capacitor remembers voltage history. In `.OP`, it mostly behaves
like an open circuit. In `.AC`, it becomes frequency-dependent. In `.TRAN`, the
integration method turns it into a temporary resistor-like stamp plus a history source.

### L: Inductor

An inductor has:

$$
v = L\frac{di}{dt}
$$

MNA usually gives the inductor a branch-current unknown `b = I(L)`.

Plain-language model:

```text
branch current -> stored flux -> inductor voltage
i              -> Phi = L*i   -> v = dPhi/dt
```

This is the dual of the capacitor. A capacitor naturally follows terminal
voltage. An inductor naturally follows branch current. That is why SpiceSharp
adds an extra current unknown for an inductor instead of stamping it as only a
node conductance.

During transient analysis, SpiceSharp cannot directly put `dPhi/dt` into the
matrix. The integration method converts it into an algebraic branch equation:

$$
v_n \approx r_{\text{eq}}i_n + v_{\text{history}}
$$

So an inductor in `.TRAN` is easiest to imagine as:

```text
branch-current unknown + branch equation + remembered-history voltage term
```

The remembered quantity is flux. The branch current is solved by MNA, the flux
is $\Phi = Li$, and the integration method computes voltage from the flux
history.

The same backward-Euler rearrangement explains the inductor stamp. Suppose the
solver is trying candidate timestep `n`, and the previous accepted branch
current is already known:

```text
known history: i[n-1]
unknown now:   i[n] = I(L)
```

Backward Euler says:

$$
v_n \approx \frac{\Phi_n - \Phi_{n-1}}{h}
$$

For a linear inductor:

$$
\Phi_n = L i_n
$$

So:

$$
v_n \approx \frac{L i_n - L i_{n-1}}{h}
$$

Split unknown current from known history:

$$
v_n \approx \frac{L}{h}i_n - \frac{L}{h}i_{n-1}
$$

The branch equation is therefore conceptually:

$$
V(p) - V(n) \approx \frac{L}{h}I(L) - \frac{L}{h}i_{n-1}
$$

Matrix/RHS meaning:

| Term | Why it goes there |
|------|-------------------|
| $V(p)-V(n)$ | Current node voltages, so they are matrix terms in the branch row. |
| $(L/h)I(L)$ | Current branch-current unknown, so it is a matrix coefficient. |
| $-(L/h)i_{n-1}$ | Accepted current history, so it becomes a RHS/history term. |

The exact sign in the RHS depends on the branch-current orientation and how the
equation is moved to the left-hand side, but the split is always the important
idea: current unknown in the matrix, old current history in the RHS.

In SpiceSharp 3.2.3 the built-in inductor behavior stack is:

| Behavior | Interfaces | Main job |
|----------|------------|----------|
| `Inductors.Temperature` | `ITemperatureBehavior` | Computes the effective inductance from parameters and multipliers. |
| `Inductors.Biasing` | `IBiasingBehavior`, `IBranchedBehavior<double>` | Creates and loads the real branch-current equation. |
| `Inductors.Frequency` | `IFrequencyBehavior`, `IBranchedBehavior<Complex>` | Loads the complex AC branch equation. |
| `Inductors.Time` | `ITimeBehavior`, `IBiasingBehavior` | Owns flux history and stamps the transient branch companion. |

The important runtime pieces are:

| Concept | Meaning |
|---------|---------|
| One-port variables | The positive and negative node voltages used to compute $v = V(p)-V(n)$. |
| Branch variable | Extra unknown `b = I(L)` for current through the inductor. |
| `ElementSet` | Cached matrix/RHS locations for node rows and the branch row. |
| `IBranchedBehavior<T>` | Exposes the branch variable to exports and coupled devices. |
| `IDerivative _flux` | Integration state that stores flux linkage and computes $d\Phi/dt$. |
| `UpdateFlux` | Hook used by mutual inductance to modify flux before time stamping. |

For an ordinary linear inductor, the stored quantity is:

$$
\Phi = L i
$$

The time-domain voltage is:

$$
v = \frac{d\Phi}{dt}
$$

Bias / operating point:

```text
ideal short in steady state, represented through branch equations
```

In a DC operating point, an ideal inductor has zero steady-state voltage. That
is the meaning of "inductor is short in DC". The current through that short is
not known in advance, so MNA creates the branch-current unknown `I(L)`.

The biasing behavior creates the branch-current unknown and stamps the ideal
voltage constraint:

$$
V(p) - V(n) = 0
$$

The node rows inject branch current into KCL:

| Location | Add |
|----------|-----|
| `Y[p,b]` | $+1$ |
| `Y[n,b]` | $-1$ |
| `Y[b,p]` | $+1$ |
| `Y[b,n]` | $-1$ |

That is why an inductor does not look like a plain resistor in MNA. Its natural
state variable is current, so the solver adds a current unknown and a branch
equation.

AC:

$$
V(p) - V(n) = sL I(L)
$$

Stamp:

| Location | Add |
|----------|-----|
| `Y[p,b]` | $+1$ |
| `Y[n,b]` | $-1$ |
| `Y[b,p]` | $+1$ |
| `Y[b,n]` | $-1$ |
| `Y[b,b]` | $-sL$ |

The branch row expresses:

$$
V(p) - V(n) - sLI(L) = 0
$$

The frequency behavior also exposes complex voltage, current, power, and branch
exports. Like the capacitor's AC behavior, it does not use timestep history; the
dynamic effect comes from $s = j\omega$.

Transient:

The integration method builds an inductor companion relation:

$$
v(t) \approx r_{\text{eq}}i(t) + v_{\text{history}}
$$

The branch equation stamps a branch-current column into node KCL, node-voltage terms
into the branch row, an equivalent resistance-like coefficient on `Y[b,b]`, and a
history term into `rhs[b]`. Exact coefficients depend on the integration method.

The time behavior does this conceptually at each candidate timestep:

```text
read current branch current i
compute flux Phi = L * i
raise UpdateFlux so coupling can adjust flux
store Phi in IDerivative
ask integration method to derive dPhi/dt
ask IDerivative for branch Jacobian/RHS contributions
stamp branch equation coefficient and history term
```

`UpdateFlux` is important for mutual inductance. A coupled inductor's flux is not
only its own $\Phi = Li$; it can also include coupling terms from other inductor branch
currents. The hook lets the mutual-inductance behavior modify the flux state
before the inductor commits the time-domain contribution.

Beginner meaning: an inductor remembers current history. Because its natural unknown is
current, MNA often gives it a branch-current variable.

Capacitor and inductor side by side:

| Topic | Capacitor | Inductor |
|-------|-----------|----------|
| Stored quantity | Charge `q` | Flux linkage `Phi` |
| Natural unknown | Terminal voltage `v` | Branch current `i` |
| Time law | $i = dq/dt$ | $v = d\Phi/dt$ |
| DC bias | Open circuit | Short branch constraint |
| AC form | $I = sCV$ | $V = sLI$ |
| Transient stamp | Node conductance plus RHS current | Branch coefficient plus RHS voltage/current term |
| Integration state | `IDerivative _qcap` | `IDerivative _flux` |
| History accepted after | Accepted timestep | Accepted timestep |

For a numeric walkthrough of the matrix/RHS split, see
[Transient Integration Methods](transient-integration-methods.md#tiny-numerical-examples).

### K: Mutual Inductance

Mutual inductance couples two inductor branch currents:

$$
M = k\sqrt{L_1L_2}
$$

For inductor branch currents `b1` and `b2`, the frequency-domain branch equations include
off-diagonal coupling:

$$
\begin{aligned}
V_1 &= sL_1I_1 + sMI_2 \\
V_2 &= sMI_1 + sL_2I_2
\end{aligned}
$$

Conceptual AC additions:

| Location | Add |
|----------|-----|
| `Y[b1,b2]` | $-sM$ |
| `Y[b2,b1]` | $-sM$ |

The self terms `Y[b1,b1]` and `Y[b2,b2]` come from the individual inductors. In
transient analysis, the same coupling is handled through integration-history terms and
equivalent coefficients.

Beginner meaning: mutual inductance says two inductors influence each other. That is why
the matrix gets off-diagonal entries connecting one inductor's branch current to the
other's equation.

### G: Voltage-Controlled Current Source

A VCCS outputs current from `p` to `n` controlled by `v(cp,cn)`:

$$
i = g_m\left(V(cp) - V(cn)\right)
$$

Stamp:

| Location | Add |
|----------|-----|
| `Y[p,cp]` | $+g_m$ |
| `Y[p,cn]` | $-g_m$ |
| `Y[n,cp]` | $-g_m$ |
| `Y[n,cn]` | $+g_m$ |

No extra branch unknown is required because the output is a current source.

Beginner meaning: a VCCS is a current source whose value is controlled by a voltage
somewhere else in the circuit.

### E: Voltage-Controlled Voltage Source

A VCVS enforces:

$$
V(p) - V(n) = \text{gain}\left(V(cp) - V(cn)\right)
$$

It requires a branch-current unknown `b`.

Stamp:

| Location | Add |
|----------|-----|
| `Y[p,b]` | $+1$ |
| `Y[n,b]` | $-1$ |
| `Y[b,p]` | $+1$ |
| `Y[b,n]` | $-1$ |
| `Y[b,cp]` | $-\text{gain}$ |
| `Y[b,cn]` | $+\text{gain}$ |

The branch row expresses:

$$
V(p) - V(n) - \text{gain}\left(V(cp) - V(cn)\right) = 0
$$

Beginner meaning: a VCVS is a voltage source whose voltage is controlled by another
voltage. Because its output is a voltage source, it needs a branch-current unknown.

### F: Current-Controlled Current Source

A CCCS outputs current from `p` to `n` controlled by another branch current `I(bc)`:

$$
i = \text{gain}\,I(bc)
$$

Stamp:

| Location | Add |
|----------|-----|
| `Y[p,bc]` | $+\text{gain}$ |
| `Y[n,bc]` | $-\text{gain}$ |

The controlling current is normally the current through a voltage source or another
MNA branch-current variable.

Beginner meaning: a CCCS is a current source controlled by a measured current somewhere
else.

### H: Current-Controlled Voltage Source

A CCVS enforces:

$$
V(p) - V(n) = r_{\text{trans}}I(bc)
$$

It requires its own branch-current unknown `b`.

Stamp:

| Location | Add |
|----------|-----|
| `Y[p,b]` | $+1$ |
| `Y[n,b]` | $-1$ |
| `Y[b,p]` | $+1$ |
| `Y[b,n]` | $-1$ |
| `Y[b,bc]` | $-r_{\text{trans}}$ |

The branch row expresses:

$$
V(p) - V(n) - r_{\text{trans}}I(bc) = 0
$$

Beginner meaning: a CCVS is a voltage source controlled by a current somewhere else.
Because its output is voltage-defined, it needs its own branch-current unknown.

### B: Behavioral Source

Behavioral sources can be voltage or current outputs with expressions. The expression
can depend on voltages, currents, parameters, time, frequency, and functions supported
by the parser and SpiceSharpBehavioral.

For a behavioral current source:

$$
i = f(x)
$$

At a Newton iteration, the engine can linearize the expression around the current
solution:

$$
i(x) \approx f(x_0) + J(x - x_0)
$$

where `J` contains partial derivatives with respect to referenced variables. The
Jacobian terms are stamped into `Y`; the residual/equivalent source term is stamped
into `rhs`.

For a behavioral voltage source, MNA also needs a branch-current unknown because the
output is voltage-defined. The branch row enforces the expression result as a voltage
constraint.

For expressions that are purely constant at a point, the stamp may reduce to an
ordinary independent source. For expressions with `TIME`, dynamic functions, table
lookups, or `LAPLACE`, the load can change at each timestep or frequency point.

Beginner meaning: behavioral sources are programmable sources. The expression is code
that computes a voltage or current from circuit variables.

### D: Diode

A diode is nonlinear:

$$
i = I_s\left(e^{v_d/(nV_t)} - 1\right)
$$

Newton iteration replaces it with a local conductance and equivalent current source:

$$
\begin{aligned}
g_d &= \left.\frac{di}{dv}\right|_{\text{current guess}} \\
i_{\text{eq}} &= i(v_{\text{guess}}) - g_dv_{\text{guess}} \\
i &\approx g_dv_d + i_{\text{eq}}
\end{aligned}
$$

Matrix stamp for `gd` is resistor-like:

| Location | Add |
|----------|-----|
| `Y[p,p]` | $+g_d$ |
| `Y[p,n]` | $-g_d$ |
| `Y[n,p]` | $-g_d$ |
| `Y[n,n]` | $+g_d$ |

RHS stamp for the equivalent diode current from `p` to `n`:

| Location | Add |
|----------|-----|
| `rhs[p]` | $-i_{\text{eq}}$ |
| `rhs[n]` | $+i_{\text{eq}}$ |

Junction capacitance and charge storage contribute additional dynamic stamps in AC and
transient analyses.

Beginner meaning: a diode is not linear. SpiceSharp repeatedly replaces it with a local
"resistor plus current source" approximation until the answer stops changing too much.
The local resistor value is `gd`; the local current-source correction is `ieq`. On each
Newton iteration, both are recomputed from the latest diode voltage guess.

### Q: Bipolar Junction Transistor

A BJT model is nonlinear and model-dependent. It is not one fixed stamp like a resistor.
At each operating point guess, the model computes local small-signal quantities such as:

- base-emitter conductance,
- base-collector conductance,
- controlled collector current terms,
- output conductance,
- junction capacitances,
- charge-storage terms,
- equivalent RHS residual currents.

Those quantities form a local Jacobian over the transistor pins:

```text
collector, base, emitter, optional substrate/internal nodes
```

The biasing stamp adds conductances and controlled-source terms to `Y`, plus equivalent
currents to `rhs`. AC uses the linearized small-signal model around the operating point.
Transient adds capacitance and charge companion terms through the integration method.

Beginner meaning: a BJT is a nonlinear multi-terminal device. Do not look for one simple
stamp; the model computes many local derivatives and equivalent sources.

### J: JFET

A JFET is also nonlinear and model-dependent. Its stamp is built from local derivatives
of drain current and gate junction currents. Conceptually, each Newton iteration loads:

- channel conductance terms between drain and source,
- transconductance controlled by gate-source voltage,
- gate junction conductance and equivalent current,
- capacitance terms in AC/transient,
- residual current terms on the RHS.

The exact coefficients depend on the selected JFET model and current operating region.

Beginner meaning: a JFET is also model-dependent. Its stamp changes with the operating
point.

### M: MOSFET

MOSFETs have the richest model-dependent stamps. Depending on the model and operating
region, the local Jacobian can include:

- drain-source output conductance,
- gate transconductance,
- body-effect transconductance,
- bulk diode conductances and equivalent currents,
- terminal capacitances,
- charge conservation terms,
- optional internal node terms.

The biasing matrix receives partial derivatives of terminal currents with respect to
terminal voltages. The RHS receives residual currents so the linearized system matches
the nonlinear equations at the current guess. AC uses the small-signal linearization.
Transient uses charge/capacitance companion models.

Beginner meaning: a MOSFET is the most complex common device here. The matrix entries
come from the model's derivatives, not from one universal four-entry stamp.

### S And W: Controlled Switches

Voltage-controlled switches (`S`) and current-controlled switches (`W`) behave like a
conductance that changes based on a control value.

Conceptually:

$$
\begin{aligned}
g &= \frac{1}{R_{\text{on}}} && \text{when on} \\
g &= \frac{1}{R_{\text{off}}} && \text{when off}
\end{aligned}
$$

Then the switch stamps like a resistor:

| Location | Add |
|----------|-----|
| `Y[p,p]` | $+g$ |
| `Y[p,n]` | $-g$ |
| `Y[n,p]` | $-g$ |
| `Y[n,n]` | $+g$ |

Real switch models usually smooth or limit transitions to help convergence. Abrupt
state changes can make Newton iteration harder because the matrix changes sharply as
the control crosses the threshold.

Beginner meaning: a switch is roughly a resistor that changes between `Ron` and `Roff`,
but the transition can make convergence harder.

### T: Lossless Transmission Line

A transmission line is not just a static resistor-like stamp. It relates present
terminal behavior to delayed wave history:

$$
\text{delay} = \frac{\text{length}}{\text{propagation velocity}}
$$

In transient analysis, the line uses history buffers and characteristic impedance to
inject delayed wave contributions. It behaves like a dynamic two-port whose RHS and
effective terminal relations depend on previous accepted time points.

In frequency-domain analysis, the line can be represented by frequency-dependent
two-port relationships. The exact stamp is behavior-specific and depends on line
parameters, delay, and characteristic impedance.

Beginner meaning: a transmission line has delay. It cannot be explained as one static
resistor/capacitor/inductor stamp.

### Laplace Sources

Laplace transfer sources implement:

$$
\text{output}(s) = H(s)\,\text{input}(s)
$$

where `H(s)` is a rational transfer function:

$$
H(s) = \frac{\text{numerator}(s)}{\text{denominator}(s)}
$$

In AC, this is naturally frequency-domain: evaluate $H(j\omega)$ and stamp the equivalent
controlled-source relationship.

In transient analysis, the transfer function needs state realization or equivalent
dynamic behavior. The source contributes extra internal state equations or companion
terms so the time-domain output follows the transfer function. Delay options add
history-buffer behavior.

The exact matrix shape depends on voltage-controlled versus current-controlled input
and voltage versus current output:

| Kind | MNA shape |
|------|-----------|
| Voltage output | Needs a branch-current unknown, like a voltage source. |
| Current output | Stamps current into output node KCL rows. |
| Voltage input | Reads `V(cp,cn)` as the control variable. |
| Current input | Reads another branch-current variable as the control variable. |

Beginner meaning: a Laplace source is a dynamic filter block inside the circuit. In AC,
it is evaluated as a frequency response. In transient analysis, it needs internal state.

### X: Subcircuit Instance

A subcircuit does not have its own physical matrix stamp. It is a hierarchy and naming
mechanism.

When subcircuits are expanded, internal devices receive generated names and internal
nodes are mapped into the parent circuit namespace. Then each internal resistor,
source, transistor, capacitor, and other component stamps the global matrix normally.

The important idea is:

```text
X instance stamp = sum of stamps from expanded internal components
```

Beginner meaning: a subcircuit is like a function call or component macro. It does not
solve separately; its inside parts are mapped into the parent circuit.

## Reading Solver Failures

| Symptom | Typical meaning |
|---------|-----------------|
| Singular matrix | The equations do not define a unique solution. Check floating nodes and ideal source loops. |
| Invalid or weak pivot | The matrix is nearly singular or badly scaled. Check extreme component values. |
| DC non-convergence | Newton iteration did not settle. Check nonlinear models, initial conditions, and discontinuous behavioral expressions. |
| Timestep too small | Transient solver repeatedly reduced the step and still could not converge or meet truncation error. |
| Huge or NaN result | Look for zero-valued elements, unsupported model parameters, or unstable behavioral expressions. |

Useful debugging steps:

1. Run `.OP` before `.TRAN`.
2. Add DC paths for floating nodes, often large resistors to ground.
3. Avoid ideal voltage-source loops and current-source cutsets.
4. Start with simpler models, then add nonlinear or dynamic devices.
5. Use `.NODESET`, `.IC`, `gmin`, and tolerances carefully.

## References

- [SpiceSharp project](https://github.com/SpiceSharp/SpiceSharp)
- [SpiceSharp documentation](https://spicesharp.github.io/SpiceSharp/)
- [Modified nodal analysis](https://spicesharp.github.io/SpiceSharp/articles/custom_components/modified_nodal_analysis.html)
- [Resistor MNA example](https://spicesharp.github.io/SpiceSharp/articles/custom_components/example_resistor.html)
- [Diode behavior example](https://spicesharp.github.io/SpiceSharp/articles/tutorials/writing_behaviors/example_diode.html)
- [Frequency-domain analysis](https://spicesharp.github.io/SpiceSharp/articles/structure/frequency.html)
