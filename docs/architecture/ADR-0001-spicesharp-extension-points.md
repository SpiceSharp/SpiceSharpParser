# ADR-0001: SpiceSharp extension points for two-state transient entities

- Status: Accepted
- Date: 2026-07-10
- Phase: 0

## Context

`SpiceSharp.Physics2D` must put future equations into the existing SpiceSharp
transient Newton/MNA system. Before introducing any mechanical terminology or
model, Phase 0 needs to prove that the pinned dependency supports two private
real unknowns, two integration-history states, coupled equation stamps,
property exports, initial-state handling, and setup-time behavior linking.

The repository pins `SpiceSharp` 3.2.3. The installed NuGet package reports:

- assembly version `3.2.3.0`;
- informational version
  `3.2.3+HEAD.34484fa.34484fa6c66d636f6983626a0a770f75c0484553`;
- repository commit
  [`34484fa6c66d636f6983626a0a770f75c0484553`](https://github.com/SpiceSharp/SpiceSharp/commit/34484fa6c66d636f6983626a0a770f75c0484553);
- package SHA-256
  `51098D1893641B3256E55FB08B1721C53FE4A896EBFA9DC291A882A3A2AEF572`.

The local package XML documentation and assembly metadata were treated as the
authority for public signatures. The repository's existing custom components
were treated as compile-tested usage examples against that same package.

## Investigated extension points

| Need | Exact public API | Evidence | Phase 0 use |
| --- | --- | --- | --- |
| Entity behavior creation | `Entity<T>.CreateBehaviors(ISimulation)` | Existing [`NonlinearCapacitor.CreateBehaviors`](../../src/SpiceSharpParser.CustomComponents/NonlinearCapacitor.cs) and [`NonlinearInductor.CreateBehaviors`](../../src/SpiceSharpParser.CustomComponents/NonlinearInductor.cs); package XML member `SpiceSharp.Entities.Entity.CreateBehaviors` | `TransientApiProbe` overrides the method. |
| Behavior registration | `BehaviorContainer`, `BindingContext`, `ISimulation.UsesBehaviors<T>()`, and `ISimulation.EntityBehaviors.Add(...)` | Same repository entities; package XML types `SpiceSharp.Behaviors.BehaviorContainer` and `SpiceSharp.Entities.BindingContext` | The probe registers one behavior only when `ITimeBehavior` is requested. |
| Behavior requirements/builders | `BehaviorContainer.Build<TContext>()` with `IBehaviorContainerBuilder<TContext>.AddIfNo<T>()`; declarative `BehaviorRequiresAttribute` also exists | Package XML types `SpiceSharp.Behaviors.BehaviorContainer.BehaviorContainerBuilder<TContext>` and `SpiceSharp.Attributes.BehaviorRequiresAttribute` | The repository's explicit registration pattern was selected because the probe has one behavior and no dependency graph to build. |
| Biasing load | `IBiasingBehavior.Load()` | Existing nonlinear capacitor, nonlinear inductor, and [`IdealDiodes.Biasing`](../../src/SpiceSharpParser.CustomComponents/IdealDiodes/Biasing.cs) | Stamps the operating-point initial-state hold and transient companion equations. |
| Transient initialization | `ITimeBehavior.InitializeStates()` and `ITimeSimulationState.UseDc`/`UseIc` | Existing [`NonlinearCapacitors.Time`](../../src/SpiceSharpParser.CustomComponents/NonlinearCapacitors/Time.cs) and [`NonlinearInductors.Time`](../../src/SpiceSharpParser.CustomComponents/NonlinearInductors/Time.cs); pinned package types [`Capacitors.Time`](https://github.com/SpiceSharp/SpiceSharp/blob/34484fa6c66d636f6983626a0a770f75c0484553/SpiceSharp/Components/Capacitors/Time.cs) and [`Inductors.Time`](https://github.com/SpiceSharp/SpiceSharp/blob/34484fa6c66d636f6983626a0a770f75c0484553/SpiceSharp/Components/Inductors/Time.cs) | Copies the solved operating-point values into both derivative histories. |
| Private real variables | `IBiasingSimulationState.CreatePrivateVariable(string, IUnit)` inherited from `IVariableFactory<IVariable<double>>` | Existing [`NonlinearInductorVariables<T>`](../../src/SpiceSharpParser.CustomComponents/NonlinearInductors/NonlinearInductorVariables.cs); package XML member `SpiceSharp.Simulations.IVariableFactory<V>.CreatePrivateVariable` | Allocates `probe#a` and `probe#b` during behavior construction. |
| Variable-to-solver mapping | `IBiasingSimulationState.Map`, `IBiasingSimulationState.Solver`, `IVariableMap` indexer, `MatrixLocation` | Existing nonlinear capacitor and inductor variable/stamp helpers | Caches the four matrix locations and two RHS locations during construction. |
| Integration history | `IIntegrationMethod.CreateDerivative(bool)`, `IDerivative.Value`, `Derive()`, and `GetContributions(double, double)` | Both repository nonlinear transient behaviors; package XML types `IIntegrationMethod`, `IDerivative`, and `JacobianInfo` | Creates two independent derivative histories and obtains each companion Jacobian/history RHS. |
| Matrix/RHS load | `ElementSet<double>` constructors and `Add(double[])` | Existing nonlinear transient behaviors and ideal-diode biasing behavior | Adds four coefficients and two RHS values without per-load collection construction. |
| Linked behavior resolution | `Reference.GetContainer(ISimulation)` followed by `IBehaviorContainer.TryGetValue<T>()`/`GetValue<T>()` | Package XML types `SpiceSharp.Simulations.Base.Reference` and `SpiceSharp.General.ITypeSet<T>`; built-in `MutualInductances.BindingContext` exposes resolved primary and secondary inductor behavior containers | An optional linked probe is resolved once in `CreateBehaviors` and cached by the behavior. |
| Parameter/property export | `GeneratedParametersAttribute`, `ParameterNameAttribute`, and `RealPropertyExport` | Existing generated behavior properties and [`NonlinearPassiveTests`](../../src/SpiceSharpParser.Tests/CustomComponents/NonlinearPassiveTests.cs); package XML type `SpiceSharp.Simulations.RealPropertyExport` | State properties are exported as `a` and `b`. |
| Nonlinear Newton stamping | Analytic tangent plus companion RHS through `ElementSet<double>`; derivative terms use `IDerivative.GetContributions` | [`NonlinearCapacitors.Time`](../../src/SpiceSharpParser.CustomComponents/NonlinearCapacitors/Time.cs) and [`IdealDiodes.Biasing`](../../src/SpiceSharpParser.CustomComponents/IdealDiodes/Biasing.cs) | Phase 0 has only linear equations; later nonlinear entities must follow this analytic-tangent pattern and test their Jacobians independently. |

## Decision

Use an ordinary zero-pin `Entity<TransientApiProbeParameters>` and one behavior
implementing both `IBiasingBehavior` and `ITimeBehavior`. The behavior owns two
private `IVariable<double>` instances and two `IDerivative` histories.

During the operating-point load, it stamps:

```text
A = InitialA
B = InitialB
```

`ITimeBehavior.InitializeStates()` initializes each integration history from
that solved state. During transient load, it stamps:

```text
dA/dt - B = 0
dB/dt + A = 0
```

For `IDerivative.GetContributions(1.0, x)`, SpiceSharp represents the candidate
derivative as:

```text
derivative = Jacobian * x + Rhs
```

Therefore the cached stamp is:

```text
[ Ja  -1 ] [ A ] = [ -RhsA ]
[  1  Jb ] [ B ]   [ -RhsB ]
```

This sign convention is explicit in the implementation and is verified by the
analytic oscillator trajectory.

An optional link uses `Reference.GetContainer(simulation)` during
`CreateBehaviors`, then resolves `ITransientApiProbeBehavior` from that
container. The linked behavior is cached; no entity-name lookup occurs in the
load loop. The referenced entity must appear earlier in behavior-construction
order, matching SpiceSharp's built-in setup-time linking model.

The probe uses `Units.Volt` only as solver-variable bookkeeping because the
pinned public `Units` catalog has no dimensionless state unit. A and B are
normalized API-proof states and are not electrical or mechanical quantities.

## Rejected alternatives

- A custom simulation, solver, or integration method: prohibited and
  unnecessary; all required extension points are public on ordinary
  `Transient`.
- Reflection into solver or integration internals: unnecessary; public state,
  variable factory, map, derivative, and element APIs are sufficient.
- Shared circuit nodes for A and B: rejected because the proof specifically
  requires private solver variables.
- Dynamic or load-time linked-behavior lookup: rejected because topology is
  known during setup and should be cached.
- A finite-difference load loop: unnecessary for these linear equations and
  prohibited for future nonlinear production loads.

## Consequences

- Phase 0 proves the infrastructure needed for a later two-state generalized
  coordinate without introducing any mechanical API.
- The probe runs entirely under `SpiceSharp.Simulations.Transient` and uses its
  existing Newton solve, integration histories, timestep control, and sparse
  solver.
- Future phases can expose solver variables through behavior contracts while
  keeping matrix locations and mutable stamp storage internal.
- A circuit containing only zero-pin entities does not satisfy SpiceSharp's
  ordinary electrical variable-presence validation. Phase 0 tests keep
  validation enabled and include one isolated grounded resistor as validation
  topology; the resistor is not coupled to either probe state.

## Verification

See [Phase 0 verification](../verification/phase-00.md).
