# ADR-0003: Mechanical coordinate state and direct generalized-force stamping

- Status: Accepted
- Date: 2026-07-10
- Phase: 2

## Context

`SpiceSharp.Physics2D` needs a first mechanical state that can participate in
ordinary SpiceSharp transient analysis. One generalized coordinate must carry
position, velocity, inertia, and deterministic initial conditions while
remaining open to forces supplied by components introduced in later phases.

The design must preserve the boundary established by
[ADR-0001](ADR-0001-spicesharp-extension-points.md): SpiceSharp owns Newton
iteration, sparse equation assembly, integration history, timestep control,
and the transient lifecycle. Physics2D entities contribute equations through
public behavior and stamping APIs. They do not introduce a world-step loop,
custom solver, custom integration method, or separate force-accumulation pass.

For one generalized position `q`, generalized velocity `u`, constant positive
generalized mass `M`, and net applied generalized force `Q`, the governing
equations are:

```text
qdot - u       = 0
M*udot - Q     = 0
```

`Q` may depend on position, velocity, time, and later on other mechanical or
electrical state. The architecture therefore needs an explicit Newton sign
convention, a stable setup-time topology contract, and a public behavior
surface that lets connected components find the two solver variables without
exposing mutable solver storage.

## Decision

### State and equation ownership

[`MechanicalCoordinate`](../../src/SpiceSharp.Physics2D/Core/MechanicalCoordinate.cs)
is an ordinary zero-pin `Entity<MechanicalCoordinateParameters>`. Its
[`MechanicalCoordinateBehavior`](../../src/SpiceSharp.Physics2D/Core/MechanicalCoordinateBehavior.cs)
implements both `IBiasingBehavior` and `ITimeBehavior` and creates two private
real solver variables:

- `PositionVariable`, whose row owns the kinematic equation;
- `VelocityVariable`, whose row owns the generalized dynamics equation.

The solver unknown vector is `(q, u)`. Momentum is not a third unknown. During
transient load, the coordinate behavior contributes only:

```text
position row: qdot - u = 0
velocity row: M*udot   = 0
```

Connected force behaviors add their own `-Q` residual contribution directly
to the velocity row during their normal SpiceSharp load pass. Additive sparse
stamping makes the assembled row equivalent to `M*udot - sum(Qi) = 0` without
requiring an intermediate force accumulator.

### Position and momentum integration histories

The behavior owns two independent `IDerivative` histories:

```text
position history value = q
momentum history value = M*u
```

Before deriving transient companions, the behavior updates both values from
the current solver iterate. It asks the position history for contributions
with coefficient `1` and state `q`, and the momentum history for contributions
with coefficient `M` and state `u`. This produces the integration-method
companion terms for `qdot` and `M*udot` while retaining velocity as the public
solver unknown.

Storing generalized momentum in the second integration history makes the
inertial quantity being differentiated explicit. The resulting matrix
coefficient with respect to `u` includes `M`, and the history right-hand side
uses the same mass convention. SpiceSharp still owns all history advancement
and timestep decisions.

### Operating-point initial-condition policy

Phase 2 supports exactly one initialization mode:

```text
HoldSpecifiedStateDuringOperatingPoint
```

While `ITimeSimulationState.UseDc` is true, the coordinate stamps identity
holds:

```text
q = InitialPosition
u = InitialVelocity
```

Connected force behaviors do not stamp during this operating-point hold, so a
spring, damping term, or applied force cannot move the requested initial
state. `ITimeBehavior.InitializeStates()` then copies the solved `q` and
`M*u` into the two derivative histories before transient integration starts.

This policy is an initial-state hold, not a mechanical static-equilibrium
solve. A future equilibrium mode would be a separate policy with different
equation ownership and force participation; it must not silently change the
meaning of this mode.

### Direct generalized-force Newton stamp

For a connected component that evaluates `Q(q, u)`, define:

```text
Qq = dQ/dq
Qu = dQ/du
```

Its contribution to the dynamics residual is `Rf = -Q`. Linearizing that
residual at the current Newton iterate and expressing the system in
SpiceSharp's matrix/RHS form gives:

```text
matrix(q) = -Qq
matrix(u) = -Qu
rhs       = Q - Qq*q - Qu*u
```

Each force behavior owns and caches its own `ElementSet<double>` for those two
matrix locations and the velocity-row RHS location. The Phase 2 test-only
components exercise the convention:

| Force law | Matrix contribution | RHS contribution |
| --- | --- | --- |
| `Q = F` | none | `F` |
| `Q = -c*u` | `+c` in the velocity column | `0` |
| `Q = -k*(q-r)` | `+k` in the position column | `k*r` |

Later nonlinear force components must evaluate analytic `Qq` and `Qu` in
their production load path. Independent finite-difference checks belong in
the test project under the convention established by
[ADR-0002](ADR-0002-double-precision-mathematics.md).

### Setup-time topology and behavior contract

[`IMechanicalCoordinateBehavior`](../../src/SpiceSharp.Physics2D/Core/IMechanicalCoordinateBehavior.cs)
exposes the two `IVariable<double>` instances and read-only mechanical state:
position, velocity, generalized mass, requested initial state, and kinetic
energy. It does not expose solver indices, matrix locations, mutable arrays,
an `ElementSet`, or an imperative `AddForce` method.

A connected component resolves its coordinate behavior once during behavior
construction with `Reference.GetContainer(simulation)` and
`GetValue<IMechanicalCoordinateBehavior>()`. It maps the two variables through
the active simulation state and caches its own stamp locations. No entity-name
or behavior lookup occurs in `Load()`.

This makes behavior-construction order part of the topology contract: a
referenced coordinate must appear before the component that binds to it. A
missing coordinate or missing coordinate behavior fails during setup rather
than producing a late or partial transient failure.

### Parameters, scaling, and solver units

Behavior construction rejects a generalized mass that is non-finite or not
strictly positive, non-finite initial position or velocity, and unsupported
initial-condition enum values. Phase 2 uses direct SI values and performs no
silent rescaling; conditioning remains visible to the model author.

The pinned SpiceSharp public unit catalog has no mechanical or dimensionless
generalized-coordinate units. Both private variables therefore use
`Units.Volt` solely as required solver-variable bookkeeping. This does not
assign electrical voltage semantics to position or velocity. Mechanical
meaning is defined by the Physics2D entity, behavior contract, equations, and
exports.

### Exports and diagnostics

The behavior is the source of live exports:

- position (`position`, `q`);
- velocity (`velocity`, `u`);
- generalized mass (`mass`, `generalizedmass`);
- requested initial position (`initialposition`, `q0`);
- requested initial velocity (`initialvelocity`, `u0`);
- kinetic energy (`kineticenergy`, `ke`), calculated as `0.5*M*u^2`.

Current position and velocity are read from solver variables, not copied back
into entity configuration. Initial parameters therefore remain configuration
values after a transient run.

Phase 2 deliberately omits a `GeneralizedForce` export. With independent
direct stamps there is no authoritative net-force accumulator, and exposing
only a subset, a last-loaded value, or a reconstructed stale value would be
misleading. Such a diagnostic may be added only when its ownership and
lifecycle are defined without changing the equation assembly model.

### Load-path allocation policy

Matrix locations, RHS locations, `ElementSet<double>` instances, and reusable
value arrays are created during behavior construction. Transient load updates
the cached arrays and adds their values. It performs no reflection, LINQ,
dictionary construction, name lookup, or per-load collection allocation.

The integration method used by a simulation remains a caller choice. Phase 2
tests select SpiceSharp's variable-step trapezoidal method and explicitly set
initial and maximum timesteps for reproducible numerical gates; the coordinate
does not hard-code either the method or a timestep.

## Rejected alternatives

- A custom mechanical simulation, integrator, solver, or world-step loop:
  rejected because ordinary SpiceSharp transient behaviors provide the
  required coupled Newton and integration lifecycle.
- A single solver unknown `q` with velocity inferred only from its derivative:
  rejected because velocity-dependent forces need `u` in the simultaneous
  Newton system and velocity is a first-class state and export.
- Generalized momentum as the public second solver unknown: rejected because
  later kinematic and force laws naturally consume velocity; using `p` would
  spread `u = p/M` conversions and derivative scaling across components.
- A velocity-valued second history followed by ad hoc external mass scaling:
  rejected in favor of differentiating the physical inertial state `M*u` and
  obtaining its companion terms through one consistent history contract.
- A global force accumulator or a coordinate `AddForce` callback: rejected
  because it would create a second load phase, ordering concerns, mutable
  shared state, and a lifecycle parallel to SpiceSharp's additive assembly.
- Exposing solver row numbers, raw matrix storage, or reusable stamp arrays:
  rejected because those values belong to the active simulation and would let
  connected components mutate coordinate-owned state.
- Looking up the coordinate by name during every load: rejected because the
  topology is static and lookup failures should occur during setup.
- Letting forces participate in the specified-state operating point: rejected
  because it would turn the documented hold into an implicit and potentially
  singular static-equilibrium solve.
- Publishing the constant-force, damping, and spring entities in Phase 2:
  rejected because they are verification fixtures; public components belong
  to their assigned later phases.
- A partial `GeneralizedForce` diagnostic: rejected because a plausible but
  incomplete force value is worse than an explicitly unavailable diagnostic.
- Production finite-difference Jacobians: rejected because analytic stamps are
  deterministic, cheaper, and avoid step-size noise in Newton iteration.

## Consequences

- A generalized coordinate participates in an ordinary SpiceSharp transient
  as two coupled private unknowns with two integration histories.
- Every connected force component owns its residual and analytic Jacobian
  contribution and can be added independently through sparse additive stamps.
- Reference ordering is explicit: coordinates must be constructed before
  components that bind to them.
- Exact requested position and velocity seed the transient, but Phase 2 does
  not provide general static mechanical equilibrium.
- Live position, velocity, and kinetic-energy exports remain behavior-derived;
  entity parameters remain configuration.
- There is intentionally no authoritative net generalized-force diagnostic in
  this phase.
- Mechanical variables carry placeholder SpiceSharp unit metadata until a
  supported mechanical-unit mechanism exists.
- Extreme mass, stiffness, damping, or mixed-domain scales may require future
  explicit scaling policy; this phase does not conceal conditioning problems.
- A circuit containing only zero-pin mechanical entities still needs ordinary
  SpiceSharp electrical validation topology. Tests retain the isolated
  grounded resistor documented by ADR-0001.

## Verification

See [Phase 2 verification](../verification/phase-02.md). Fourteen coordinate
tests cover free motion, constant velocity, constant force, damping, spring
period, damped response, timestep refinement, exact initial-state holds, live
exports, deterministic repeated runs, and energy conservation. The complete
repository suite passes with 2,365 tests passed, 11 skipped, and no failures.
