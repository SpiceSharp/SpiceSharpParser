# SpiceSharpParser.CustomComponents: A Tutorial

This tutorial explains what the project does, why the circuit equations look
the way they do, and where to start reading the code. You need basic C# skills,
but you do not need prior simulator experience.

The shortest useful description is:

> `SpiceSharpParser.CustomComponents` is an optional translation and simulation
> layer for useful SPICE forms that the standard SpiceSharp components do not
> directly represent.

It does **not** replace the parser, the circuit, or the simulator.

## 1. The mental model

A simulation starts as text and ends as numbers:

```text
SPICE text
   |
   | parse words, numbers, expressions, and line positions
   v
parsed netlist model
   |
   | read each parsed statement and select a generator
   v
SpiceSharp Circuit + Simulations
   |
   | create analysis-specific behaviors
   v
solver equations
   |
   | solve DC, transient, or AC problem
   v
voltages, currents, measurements, and plots
```

This package participates in the middle two steps:

1. It installs custom-aware reader generators for `A`, `C`, `D`, and `L`
   netlist lines.
2. It either creates a custom SpiceSharp component or expands a portable
   subcircuit.

There are three implementation routes. Learning this table removes most of the
apparent complexity:

| Netlist form | What the package creates | Where its science lives |
| --- | --- | --- |
| `C ... Q=<expression>` or `L ... Flux=<expression>` | A native custom component | C# `Biasing`, `Time`, and `Frequency` behaviors |
| Ideal-diode `.MODEL D(...)` | A native custom diode and model | `IdealDiodeEquation` plus C# behaviors |
| LTspice A-device or public digital/analog helper | Ordinary entities from a `.lib` subcircuit | The embedded SPICE macromodel |

An ordinary line such as `C1 out 0 1u` still takes the standard SpiceSharp
route. Enabling custom components does not turn every capacitor, diode, or
inductor into a custom one.

## 2. A minimal program

The opt-in point is `UseCustomComponents()`:

```csharp
using System;
using SpiceSharpParser;
using SpiceSharpParser.CustomComponents;

string netlist = string.Join(
    Environment.NewLine,
    "Nonlinear capacitor tutorial",
    "V1 in 0 1",
    "R1 in out 1k",
    "C1 out 0 Q={1u*x + 100n*x*x*x}",
    ".tran 10u 5m",
    ".end");

var parser = new SpiceNetlistParser();
var parseResult = parser.ParseNetlist(netlist);

var reader = new SpiceSharpReader();
reader.Settings.UseCustomComponents();
var model = reader.Read(parseResult.FinalModel);

foreach (var simulation in model.Simulations)
{
    simulation.Execute(model.Circuit);
}
```

Keep the two jobs separate in your head:

- `SpiceNetlistParser` understands the text.
- `SpiceSharpReader` turns the parsed description into executable objects.

`UseCustomComponents()` changes reader mappings, so it must be called before
`Read`.

## 3. The small amount of circuit science you need

### 3.1 Voltage belongs between two nodes

A node is a named electrical connection. Voltage is always a difference:

```text
V(a, b) = voltage at a - voltage at b
```

Node `0` is the global reference, usually called ground. `V(out)` is shorthand
for `V(out, 0)`.

The order of a two-terminal component matters because it defines the positive
voltage and current directions. For:

```spice
C1 positive negative Q=1u*x
```

the package uses:

```text
V = V(positive) - V(negative)
```

### 3.2 The solver enforces Kirchhoff's current law

Kirchhoff's current law, or KCL, says that currents at a node balance. A
simulator builds one large system of equations from all component
contributions. Adding a contribution to that system is called **stamping**.

A resistor with conductance `G = 1/R` obeys:

```text
I = G * (Vpositive - Vnegative)
```

That relation contributes four signed values to the node matrix. The custom
components eventually do the same thing, even when their equations start as
charge, flux, or a smoothed diode curve.

### 3.3 Why derivatives appear

Linear equations have constant slopes. Nonlinear equations do not. The solver
therefore works around a current guess and replaces a nonlinear curve with its
local tangent:

```text
f(x) is approximated near x0 by:

f(x0) + f'(x0) * (x - x0)
```

This is the core idea behind Newton iteration. In the code, the derivative
usually becomes a matrix coefficient and the remaining correction becomes a
right-hand-side value.

The derivative is not an optimization detail. It is the local electrical value
seen by the solver.

### 3.4 The three important analyses

| Analysis | Question | Custom behavior |
| --- | --- | --- |
| DC operating point | Where does the circuit settle with no time change? | `Biasing` |
| Transient | How does the circuit change over time? | `Time` plus `Biasing` |
| AC small signal | How does a tiny sine-wave disturbance behave around the DC point? | `Frequency` plus `Biasing` |

The same entity can therefore create different behavior objects for different
analyses.

## 4. Nonlinear capacitors: charge first, capacitance second

A normal capacitor is often introduced as:

```text
Q = C * V
```

The physical law used in time is:

```text
I = dQ/dt
```

For a constant capacitor, differentiating `Q = C*V` gives the familiar
`I = C*dV/dt`. A nonlinear capacitor generalizes this by allowing any supported
charge curve `Q(V)`.

Consider:

```spice
C1 out 0 Q={1u*x + 100n*x*x*x}
```

For a capacitor, `x` means its terminal voltage. The expression is:

```text
Q(V) = 1e-6*V + 100e-9*V^3 coulombs
```

The local or incremental capacitance is:

```text
Cinc(V) = dQ/dV
        = 1e-6 + 300e-9*V^2 farads
```

At `V = 0`, the local capacitance is `1 uF`. At `V = 2 V`, it is
`2.2 uF`. The component gets "larger" as the magnitude of voltage rises.

The code follows the same steps:

1. `NonlinearPassiveGenerator` notices `Q=`.
2. It creates `NonlinearCapacitor` instead of the standard capacitor.
3. `SingleVariableExpression` parses both `Q(x)` and its symbolic derivative.
4. `NonlinearCapacitors.Biasing` evaluates charge and `dQ/dV`.
5. `NonlinearCapacitors.Time` asks the integration method to turn `dQ/dt`
   into a matrix companion model.
6. `NonlinearCapacitors.Frequency` uses `s*Cinc` at the operating point.

At DC, an ideal capacitor is an open circuit. Its biasing behavior still
evaluates the derivative so exports and later AC setup know the operating-point
capacitance.

## 5. Nonlinear inductors: the dual idea

For an inductor, the stored quantity is flux linkage:

```text
Phi = L * I
V = dPhi/dt
```

A nonlinear inductor accepts `Flux=<expression>`, and `x` now means branch
current:

```spice
L1 in out Flux={2m*tanh(x)}
```

Its incremental inductance is:

```text
Linc(I) = dPhi/dI
```

The code mirrors the capacitor implementation, with one important topology
difference: an inductor needs a **branch-current variable**. Node voltages alone
cannot directly express the voltage-source-like branch equation
`V = dPhi/dt`.

Read these files side by side:

- `NonlinearCapacitors/Biasing.cs`
- `NonlinearInductors/Biasing.cs`
- `NonlinearCapacitors/Time.cs`
- `NonlinearInductors/Time.cs`

The symmetry is intentional. It exposes what is common and highlights why the
inductor has an extra solver unknown.

## 6. Ideal diodes: one numerical law, several solver adapters

An ideal verbal diode is simple:

- forward-biased: conduct strongly;
- reverse-biased: block current;
- optionally clamp reverse voltage or current.

A perfectly abrupt switch is difficult for Newton iteration because its slope
changes discontinuously. The custom diode supports resistances, thresholds,
smoothing, reverse behavior, and current limits that form a numerically usable
piecewise curve.

The ideal-diode area is deliberately layered:

```text
netlist parameters
      |
      v
IdealDiodeParserSupport
      |
      v
IdealDiodeParameterResolver
      |
      v
effective IdealDiodeParameters
      |
      v
IdealDiodeEquation.Evaluate(voltage)
      |
      +--> current
      `--> conductance = dI/dV
                |
                v
          solver behaviors
```

This separation is valuable:

- the equation can be understood without parser types;
- the parser can handle model selection without matrix knowledge;
- `Biasing` and `Frequency` adapt the same effective equation to their
  analyses.

When debugging a numerical diode result, first inspect
`IdealDiodeEquation`. When debugging why a `.MODEL` value was not selected,
start in `IdealDiodeGenerator` and `IdealDiodeParameterResolver`.

## 7. Subcircuit libraries: reusable circuits, not new solver physics

The digital and analog helpers use embedded `.lib` files. A subcircuit is a
named circuit template with ordered external nodes and optional parameters.

For example:

```csharp
var digital = DigitalSubcircuitLibrary.LoadBuiltIn();
digital.AddBinaryGate(
    circuit,
    DigitalGateKind.Nand2,
    instanceName: "XU1",
    firstInputNode: "a",
    secondInputNode: "b",
    outputNode: "y",
    positiveSupplyNode: "vdd",
    negativeSupplyNode: "0");
```

The C# facade contributes:

- discoverable method and parameter names;
- argument validation;
- a stable API;
- the ordered node list expected by the subcircuit.

The `.lib` file contributes the functional electrical model. After expansion,
SpiceSharp sees ordinary resistors, capacitors, behavioral sources, and other
normal entities. There is no special "NAND solver".

These are functional macromodels. They aim to reproduce useful input/output
behavior, not every transistor inside a physical chip.

## 8. LTspice A-devices: a translation pipeline

Every supported native A-device has eight terminal positions, a model name, and
parameters:

```spice
A<name> T1 T2 T3 T4 T5 T6 T7 T8 <model> [name=value ...]
```

The refactored code makes the translation stages explicit.

### Stage 1: read syntax

`LTspiceADeviceInstanceReader` verifies the eight terminals, model, flags, and
parameter assignments. Its output, `LTspiceADeviceInstance`, contains raw
parameter expressions and source locations for good diagnostics.

No circuit entities are added at this stage.

### Stage 2: look up the translation

`LTspiceADeviceCatalog` is the central translation table. One definition keeps
together:

- portable subcircuit name;
- native-terminal-to-subcircuit-node order;
- output terminals;
- native-to-portable parameter names;
- whether digital power rails must be generated;
- zero-delay and compatibility policies;
- small model-specific parameter adjustments.

For a counter, the important part reads conceptually as:

```text
native model:       COUNTER
portable model:     DIG_COUNTER
portable nodes:     T1, T2, T7, T6, generated-high, generated-low
native parameters:  cycles -> CYCLES
                    duty   -> DUTY
                    rout   -> ROUT
```

The definition is the first file to edit or review when terminal wiring looks
wrong.

### Stage 3: validate values

`LTspiceADeviceValidator` checks facts such as:

- `COUNTER Cycles` is an integer of at least two;
- `Duty` is between zero and one;
- resistances are positive;
- OTA source and sink current limits have the correct signs;
- required modulator frequencies exist.

Validation happens before circuit expansion so bad input does not leave a
half-created macromodel.

### Stage 4: resolve terminal semantics

LTspice uses terminal 8 as common. Repeating that same node in another terminal
position means "unused".

There is an important trap: if terminal 8 is not global ground, literal node
`0` is a real connection to global ground. It is not unused.

`LTspiceADeviceConnections` records which terminals were unused. Unused inputs
may remain at common. Unused outputs are moved to private nodes so a portable
output stage does not accidentally drive common.

### Stage 5: expand

`LTspiceADeviceExpander`:

1. evaluates and maps parameters;
2. creates private digital high/low rails when required;
3. applies small compatibility defaults;
4. chooses a regular or zero-delay subcircuit;
5. builds the ordered node list from the definition;
6. adds the subcircuit's entities.

`LTspiceADeviceGenerator` is now only the coordinator for these stages. It also
takes a circuit snapshot and removes newly added entities if expansion throws.
To the reader, a multi-entity expansion is therefore one operation.

## 9. Suggested source-reading order

Do not start by reading every file alphabetically. Use this path:

1. `Parser/CustomComponentReaderExtensions.cs`
   See exactly which four netlist letters are intercepted.

2. `Parser/NonlinearPassiveGenerator.cs`
   See a small custom-or-fallback decision.

3. `NonlinearCapacitor.cs` and `NonlinearCapacitors/Biasing.cs`
   Learn entity versus behavior.

4. `NonlinearCapacitors/Time.cs`
   Connect `Q(V)` to `I=dQ/dt` and the matrix stamp.

5. `IdealDiodes/IdealDiodeEquation.cs`
   See a numerical law isolated from parser and topology concerns.

6. `Parser/LTspiceADeviceGenerator.cs`
   Read the five-stage coordinator.

7. `Parser/LTspiceADeviceDefinition.cs`
   Inspect all A-device wiring and parameter maps in one place.

8. `Parser/LTspiceADeviceExpander.cs`
   See how a definition becomes circuit entities.

9. `Digital/Netlists/standard-digital.lib` or
   `Analog/Netlists/standard-analog.lib`
   Read one public facade method and then find its matching `.SUBCKT`.

## 10. A practical debugging method

When a custom netlist does not behave as expected, ask these questions in
order.

### Did the custom path activate?

- Was `UseCustomComponents()` called before `Read`?
- Is the form actually custom (`Q=`, `Flux=`, ideal-diode parameters, or `A`)?
- Did an ordinary component correctly take the standard fallback?

### Did reading succeed?

Inspect `model.ValidationResult`. A parser/reader error is usually more useful
than a later simulation exception because it retains source line information.

### Was the right entity or subcircuit created?

Inspect `model.Circuit`:

- nonlinear passive: expect `NonlinearCapacitor` or `NonlinearInductor`;
- ideal diode: expect `IdealDiode`;
- A-device: expect several entities with a generated `X...` prefix, not one
  A-device entity.

### Is the equation wrong or only the selected value?

- Wrong charge/flux curve: inspect the expression and symbolic derivative.
- Wrong diode curve: inspect `IdealDiodeEquation`.
- Wrong diode parameter precedence: inspect `IdealDiodeParameterResolver`.
- Wrong A-device wiring: inspect `LTspiceADeviceCatalog`.
- Wrong A-device electrical behavior: inspect the matching `.SUBCKT`.

### Is it an analysis issue?

A component may be correct in DC and wrong in transient or AC. Go directly to
the behavior for that analysis. Check units and signs before changing solver
tolerances.

## 11. Safe extension recipes

### Add another A-device backed by a subcircuit

1. Add or identify the portable `.SUBCKT`.
2. Add its definition to `LTspiceADeviceCatalog`.
3. Add model-specific ranges to `LTspiceADeviceValidator`.
4. Add a parameter adjustment only if a declarative map is insufficient.
5. Add reader tests for valid syntax, invalid parameters, terminal order, and
   unused outputs.
6. Add transient or AC behavior tests for the macromodel.

Avoid putting another large model switch back into the generator. Wiring data
belongs in the catalog; generic mechanics belong in the expander.

### Add a new native solver component

Use the nonlinear passives as the template:

1. Entity class and parameter set.
2. Solver-variable layout.
3. Biasing behavior.
4. Time and/or frequency behavior.
5. Reader generator with an explicit standard fallback where applicable.
6. Equation-focused tests plus an end-to-end netlist test.

Keep numerical equations out of the parser generator. A generator decides
*what to create*; a behavior decides *what equation to stamp*.

## 12. Common misconceptions

**"CustomComponents is a second parser."**
No. It plugs additional generators into the existing reader after parsing.

**"`Q=` is a voltage-dependent capacitance expression."**
No. It is total charge. Capacitance is its derivative `dQ/dV`.

**"`Flux=` is an inductance expression."**
No. It is total flux linkage. Inductance is its derivative `dPhi/dI`.

**"An A-device becomes one custom SpiceSharp entity."**
No. It expands into multiple ordinary entities from a portable subcircuit.

**"Terminal 8 is always ground."**
No. It is the A-device's common node and may be any node.

**"An unused output can stay connected to common."**
No. The portable output stage must be detached onto a private node.

**"AC analysis follows the whole nonlinear curve."**
No. Small-signal AC uses the local derivative at the DC operating point.

## 13. Glossary

- **Entity**: an object placed in a `Circuit`.
- **Parameters**: configuration shared by an entity's behaviors.
- **Behavior**: the analysis-specific implementation of an entity.
- **Biasing**: DC and Newton-iteration behavior.
- **Stamp**: matrix and right-hand-side contributions from a component.
- **Jacobian**: matrix of local derivatives used by Newton iteration.
- **Incremental value**: a local slope, such as `dQ/dV`.
- **Branch variable**: an extra solver unknown, commonly a component current.
- **Companion model**: an equivalent algebraic model for a dynamic component at
  one timestep.
- **Subcircuit**: a reusable parameterized circuit template.
- **Macromodel**: a functional circuit approximation of a more complicated
  device.
- **Generator**: reader code that turns parsed netlist data into entities.
- **Fallback**: delegation to the standard generator when syntax is ordinary.

## 14. What to remember

If you remember only five ideas, remember these:

1. The package is an opt-in reader and simulation extension.
2. Native custom behaviors and `.lib` macromodels are different mechanisms.
3. Nonlinear solvers need local derivatives.
4. Generators choose objects; behaviors own equations.
5. A-device wiring belongs in the catalog, while expansion mechanics belong in
   the expander.

With that map, every folder has a specific job and the project becomes much
smaller than it first appears.
