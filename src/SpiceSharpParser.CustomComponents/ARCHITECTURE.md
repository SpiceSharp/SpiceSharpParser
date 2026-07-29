# Custom Components Architecture

This project is an optional compatibility layer between parsed SPICE netlists
and SpiceSharp. It recognizes syntax that the standard components do not
implement and translates it into custom components or portable subcircuits.

It does not own the general SPICE parser or the simulation loop.

## End-to-end flow

```text
SPICE text
    |
    v
SpiceNetlistParser                 Parses text into the netlist model
    |
    v
SpiceSharpReader + mappings       Selects a component generator
    |
    +--> C/L generator            Creates a nonlinear passive when Q/Flux is present
    |
    +--> D generator              Creates an ideal diode for an ideal-diode model
    |
    `--> A-device generator       Expands an LTspice A-device into subcircuit entities
             |
             v
SpiceSharp Circuit                Contains ordinary and custom entities
    |
    v
Component behaviors              Stamp equations into the SpiceSharp solver
```

`UseCustomComponents()` is the composition root. It replaces the normal
reader mappings for `A`, `C`, `D`, and `L`. Each custom generator must explicitly
fall back to the standard generator when the syntax does not describe a custom
component.

## Project areas

### Parser

The classes in `Parser` translate the parser's netlist model into SpiceSharp
entities. They deal with syntax, diagnostics, model selection, and
simulation-dependent expression resolution. They do not contain device
equations.

- `NonlinearPassiveGenerator` recognizes `Q=<expression>` and
  `Flux=<expression>`.
- `IdealDiodeGenerator` selects ideal-diode models and combines model and
  instance parameters.
- `LTspiceADeviceGenerator` coordinates parsing and expansion of native
  LTspice A-device syntax.

### Nonlinear capacitors and inductors

A nonlinear passive is described by a stored-energy expression:

- A capacitor supplies charge as `Q(v)`. Its incremental capacitance is
  `dQ/dv`.
- An inductor supplies flux as `Flux(i)`. Its incremental inductance is
  `dFlux/di`.

The biasing behavior evaluates the expression and its symbolic derivative. The
time behavior asks SpiceSharp's integration method to convert the stored-energy
derivative into a companion model. The frequency behavior uses the derivative
at the operating point as the small-signal capacitance or inductance.

The `Variables` types only describe solver variables and matrix locations. They
do not implement the device equation.

### Ideal diode

`IdealDiodeEquation` is the numerical core. It is independent of the parser
and solver topology: given effective parameters and a voltage, it returns
current and conductance.

The surrounding classes have separate responsibilities:

- `IdealDiodeParserSupport` recognizes supported netlist parameters.
- `IdealDiodeGenerator` selects a model for the current simulation.
- `IdealDiodeParameterResolver` combines model values, instance overrides, and
  stepped model values.
- `Biasing` and `Frequency` stamp the resolved equation into the solver.

### Subcircuit libraries

The `.lib` files are the actual functional macromodels. The C# library classes
are typed facades that validate arguments and instantiate those subcircuits:

- `DigitalSubcircuitLibrary` exposes the digital models.
- `AnalogSubcircuitLibrary` exposes the analog and mixed-signal models.

The facades are split by model family for navigation, but remain one public
type so existing callers keep the same API.

### LTspice A-devices

An A-device is translated in three stages:

1. `LTspiceADeviceInstanceReader` reads the eight terminals, model name, flags,
   and raw parameter expressions.
2. `LTspiceADeviceValidator` checks model-specific requirements and parameter
   ranges.
3. `LTspiceADeviceGenerator` selects the appropriate digital or analog
   subcircuit expansion.

Expansion may add several entities. If any part fails, the generator restores
the reading context to its pre-expansion state and reports one reader
diagnostic.

## SpiceSharp terminology

- **Entity**: Something placed in a circuit, such as a component or model.
- **Behavior**: The analysis-specific implementation of an entity.
- **Biasing behavior**: The DC/Newton-iteration implementation.
- **Time behavior**: The transient-analysis implementation.
- **Frequency behavior**: The AC small-signal implementation.
- **Stamping**: Adding a component's equation to the solver matrix and
  right-hand-side vector.
- **Branch variable**: An extra solver unknown used when a component current
  cannot be represented using node voltages alone.
- **Incremental value**: The local derivative used by Newton iteration or AC
  analysis, such as `dQ/dv`.

## Refactoring rules

- Keep the public component and facade APIs compatible.
- Keep numerical equations out of parser generators.
- Prefer focused unit tests for validation and equations, backed by the
  existing end-to-end and LTspice golden tests.
- Preserve the standard-generator fallback for ordinary `C`, `D`, and `L`
  syntax.
- Treat matrix element order as part of the implementation contract; describe
  that order next to each stamp.
