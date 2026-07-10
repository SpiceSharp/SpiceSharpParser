# Phase 02 verification

## Scope implemented

- Added the public `MechanicalCoordinate` entity and parameters.
- Added one generalized-position solver variable and one generalized-velocity
  solver variable during behavior construction.
- Added independent derivative histories for position and generalized momentum.
- Stamped the kinematic equation `qdot - u = 0` and inertial equation
  `M*udot = 0` into ordinary SpiceSharp `Transient`.
- Added deterministic operating-point holds for the requested initial position
  and velocity.
- Added `IMechanicalCoordinateBehavior` for setup-time binding by connected
  components without exposing mutable solver arrays.
- Added live exports for position, velocity, generalized mass, initial state,
  and kinetic energy.
- Added public-rule validation participation so coordinate-only circuits pass
  default SpiceSharp validation without unrelated electrical topology.
- Added internal test-only constant-force, linear-damping, and
  spring-to-reference entities. Each force behavior stamps its own dynamics
  contribution during its normal SpiceSharp load pass.

## Explicitly not implemented

- `GeneralizedForce` was not exposed. The current direct-stamping architecture
  has no reliable net-force accumulator, and a partial or stale value would be
  misleading. This follows the explicit Phase 2 allowance to omit it.
- No public force or spring entity; all three connected components are internal
  test infrastructure pending their assigned later phases.
- No rigid body, geometry, gravity, joint, contact, friction, cam, or
  electromechanical coupling.
- No parser change.
- No custom simulation, solver, integration method, world step, or force
  accumulator.
- No work from Phase 3 or any later phase.

## SpiceSharp API used

- `Entity<T>.CreateBehaviors(ISimulation)`.
- `BehaviorContainer`, `BindingContext`, and
  `ISimulation.EntityBehaviors`.
- `IBiasingBehavior` and `ITimeBehavior`.
- `IBiasingSimulationState.CreatePrivateVariable`, `Map`, and `Solver`.
- `IIntegrationMethod.CreateDerivative`.
- `IDerivative.Value`, `Derive`, and `GetContributions`.
- `ElementSet<double>` and `MatrixLocation`.
- `Reference.GetContainer` and `IBehaviorContainer.GetValue<T>()` for
  deterministic setup-time linking by the internal force entities.
- `GeneratedParametersAttribute`, `ParameterNameAttribute`, and
  `RealPropertyExport`.

The exact extension-point provenance remains documented in
[ADR-0001](../architecture/ADR-0001-spicesharp-extension-points.md).
The Phase 2 state, initialization, and direct-force assembly decisions are
documented in
[ADR-0003](../architecture/ADR-0003-mechanical-coordinate-and-direct-force-stamping.md).

## Files changed

- `src/SpiceSharp.Physics2D/Core/MechanicalInitialConditionMode.cs`
- `src/SpiceSharp.Physics2D/Core/MechanicalCoordinateParameters.cs`
- `src/SpiceSharp.Physics2D/Core/IMechanicalCoordinateBehavior.cs`
- `src/SpiceSharp.Physics2D/Core/MechanicalCoordinate.cs`
- `src/SpiceSharp.Physics2D/Core/MechanicalCoordinateBehavior.cs`
- `src/SpiceSharp.Physics2D.Tests/Coordinates/TestGeneralizedForces.cs`
- `src/SpiceSharp.Physics2D.Tests/Coordinates/MechanicalCoordinateTests.cs`
- `docs/architecture/ADR-0003-mechanical-coordinate-and-direct-force-stamping.md`
- `docs/verification/phase-02.md`

## Commands executed

Working-tree base: `248b4ddc8f040977cf6cd7c9524ba134b08d1ca5`.

- `dotnet restore SpiceSharp-Parser.sln`
  - Result: PASS; all projects were up-to-date.
- `dotnet format SpiceSharp-Parser.sln --verify-no-changes --no-restore
  --verbosity minimal`
  - Result: the unchanged repository-wide diagnostic retains the pre-existing
    whitespace violations documented by Phases 0 and 1 in untouched parser and
    integration-test files.
  - The repository has no checked-in formatter command or formatter gate, so
    unrelated parser files were preserved.
- `dotnet format SpiceSharp.Physics2D/SpiceSharp.Physics2D.csproj
  --verify-no-changes --no-restore --verbosity minimal`
  - Result: PASS.
- `dotnet format SpiceSharp.Physics2D.Tests/SpiceSharp.Physics2D.Tests.csproj
  --verify-no-changes --no-restore --verbosity minimal`
  - Result: PASS.
- `dotnet build SpiceSharp.Physics2D/SpiceSharp.Physics2D.csproj -c Release
  --no-restore --no-incremental`
  - Result: PASS for `netstandard2.0` and `net8.0`; 0 warnings, 0 errors.
- `dotnet test SpiceSharp.Physics2D.Tests/SpiceSharp.Physics2D.Tests.csproj
  -c Release --no-restore --filter
  "FullyQualifiedName~SpiceSharp.Physics2D.Tests.Coordinates" --logger
  "console;verbosity=detailed"`
  - Result: PASS; 14 passed, 0 skipped, 0 failed.
- `dotnet test SpiceSharp.Physics2D.Tests/SpiceSharp.Physics2D.Tests.csproj
  -c Release --no-build --no-restore`
  - Result: PASS; 62 passed, 0 skipped, 0 failed.
- `dotnet build SpiceSharp-Parser.sln -c Release --no-restore --verbosity
  quiet`
  - Result: PASS; 0 errors. The 259 reported warnings are the existing parser,
    code-analysis, and parser-test baseline; focused Phase 2 builds have
    0 warnings.
- `dotnet test SpiceSharp-Parser.sln -c Release --no-build --logger
  "console;verbosity=minimal"`
  - Result: PASS; 2,365 passed, 11 skipped, 0 failed.

## Test summary

- Phase 2 test cases added: 14.
- Physics2D test project: 62 passed, including all retained Phase 0 and Phase 1
  cases.
- Complete solution: 2,365 passed, 11 skipped, 0 failed.
- Test-project breakdown:
  - `SpiceSharp.Physics2D.Tests`: 62 passed.
  - `SpiceSharpParser.AIExamples`: 948 passed.
  - `SpiceSharpParser.Tests`: 408 passed, 9 skipped.
  - `SpiceSharpParser.IntegrationTests`: 947 passed, 2 skipped.
  - `SpiceSharpParser.PerformanceTests`: no discoverable tests, unchanged.

## Numerical cases

All transient cases use SpiceSharp's variable-step `Trapezoidal` integration
method with both `InitialStep` and `MaxStep` set explicitly. No integration
behavior was added or replaced by Physics2D.

### Free coordinate

- Parameters: mass 2; initial position 0.75; initial velocity 0; stop time 2;
  maximum timestep 0.05.
- Expected result: position 0.75 and velocity 0.
- Measured result: both matched with absolute and relative tolerances `1e-12`.

### Constant velocity

- Parameters: mass 2; initial position 0.25; initial velocity -0.3; stop time 2;
  maximum timestep 0.05.
- Expected final state: position -0.35, velocity -0.3.
- Measured result: position matched within `1e-11`; velocity within `1e-12`.

### Constant generalized force

- Parameters: mass 2, force 3, initial position 0.25, initial velocity -0.1,
  stop time 2.
- Expected final state: position 3.05, velocity 2.9.
- Relative errors, h / h/2 / h/4:
  - position: `7.86885239663337e-8` / `1.9672131173587198e-8` /
    `4.918037561895692e-9`;
  - velocity: `9.188052617587503e-16` / `4.594026308793752e-16` /
    `7.962978935242503e-15`;
  - maximum timesteps: 0.04 / 0.02 / 0.01.
- Acceptance: position `4.91804e-9 <= 1e-4`; velocity
  `7.96298e-15 <= 1e-5`.
- The position error decreases by approximately four per halving. Velocity is
  integrated essentially exactly for constant acceleration, so roundoff
  dominates and monotonic refinement is not expected at this scale.

### Linear damping

- Parameters: mass 2, damping 0.8, initial position 0.4, initial velocity 3,
  stop time 2, maximum timestep 0.002.
- Analytic solution:
  `u(t) = u0*exp(-(c/M)t)` and
  `q(t) = q0 + (M*u0/c)*(1-exp(-(c/M)t))`.
- Measured relative error:
  - position: `3.165073251398092e-8`;
  - velocity/decay: `4.2546042284541637e-8`.
- Acceptance: decay `4.25460e-8 <= 2e-4`.

### Undamped spring oscillator

- Parameters: mass 2, stiffness 8, initial position 1, initial velocity 0.
- Expected period: `3.141592653589793`.
- Maximum timestep: expected period / 500.
- Measured period from two same-direction interpolated zero crossings:
  `3.1416339946431058`.
- Relative period error: `1.315926597467087e-5`.
- Acceptance: `1.31593e-5 <= 2e-3`.

### Damped spring oscillator

- Parameters: mass 1, stiffness 9, damping 0.6, initial position 1, initial
  velocity 0.
- Maximum timestep: damped period / 1000; duration 2.5 periods.
- Comparison: exact underdamped position/velocity trajectory, including the
  exponential envelope and damped frequency.
- Normalized trajectory RMS error: `7.893909251131847e-5`.
- Damped-period relative error: `3.189497424431112e-6`.

### Maximum-timestep refinement

- Parameters: mass 1, stiffness 4, damping 0.4, initial position 1, initial
  velocity 0.25, stop time 2.
- Endpoint vector errors for h / h/2 / h/4:
  `0.04407780562348547` / `0.011541346941898654` /
  `0.0029517798735579736` at maximum timesteps 0.2 / 0.1 / 0.05.
- Error decreased at each refinement. The ratios are approximately 3.82 and
  3.91, consistent with the selected trapezoidal method's second-order
  behavior for this smooth linear system. The test asserts monotonic decrease,
  not an exact convergence order.

### Initial-condition policy and exports

- `MechanicalInitialConditionMode` contains exactly one tested member:
  `HoldSpecifiedStateDuringOperatingPoint`.
- Two repeated simulations produced exactly equal export counts, times,
  positions, and velocities.
- Operating-point exports exactly matched requested position 0.75 and velocity
  -0.25.
- Live transient exports for position, velocity, generalized mass, initial
  state, and kinetic energy resolved from behavior state. Entity initial
  parameters remained unchanged after the transient.

## Jacobian verification

Phase 2 introduces no nonlinear equation. The coordinate equations and all
three test-only force laws are linear, with constant analytic derivatives:

```text
Qconstant = F                 dQ/dq = 0    dQ/du = 0
Qdamping  = -c*u              dQ/dq = 0    dQ/du = -c
Qspring   = -k*(q-reference)  dQ/dq = -k   dQ/du = 0
```

For a connected force `Q(q,u)`, the test behavior contributes directly to the
coordinate dynamics row:

```text
matrix(q) = -dQ/dq
matrix(u) = -dQ/du
rhs       = Q - dQ/dq*q - dQ/du*u
```

The coordinate contributes only the two derivative companions. Analytic
trajectory, action-sign, energy, and refinement tests validate the combined
linear stamps. Independent finite-difference Jacobian testing is not required
until a nonlinear component is introduced.

## Energy or power checks

- Undamped oscillator parameters: mass 1.5, stiffness 6, initial position 1,
  initial velocity 0.2, duration 10 periods.
- Maximum timestep: period / 200.
- Energy checked:
  `0.5*M*u^2 + 0.5*k*q^2`.
- Maximum relative energy drift: `1.9739206074589836e-7`.
- Acceptance tolerance used: `1e-6`.

## Determinism check

Two complete coordinate simulations with identical configuration produced
bitwise-equal exported times, positions, and velocities. All tests use fixed
inputs and contain no random state.

## Performance observations

The focused 14-case Phase 2 run completed in 0.8110 seconds according to
VSTest. The complete 62-case Physics2D project completed in 0.168 seconds in
the minimal-output run. Formal performance baselines remain assigned to
Phase 14.

## Known limitations

- `GeneralizedForce` is intentionally omitted until a reliable direct-stamp
  diagnostic exists.
- Only `HoldSpecifiedStateDuringOperatingPoint` is supported. There is no
  general mechanical static-equilibrium initialization.
- The three force entities are internal test infrastructure, not public API.
- Linked force entities resolve their coordinate during setup and must follow
  it in circuit behavior-construction order.
- Position and velocity variables use SpiceSharp's `Units.Volt` only as solver
  bookkeeping because the pinned public unit catalog has no mechanical or
  dimensionless generalized-coordinate units.
- Pure coordinate-only circuits register the existing ground reference through
  SpiceSharp's public validation rules; no electrical pin or solver stamp is
  added and validation remains enabled.
- Large masses, stiffnesses, or disparate scales are not silently rescaled.

## Decision

### Post-review corrective verification

`CoordinateOnlyCircuitPassesDefaultValidation` runs an ordinary `Transient`
over a circuit containing only one `MechanicalCoordinate`. Validation remains
enabled, no dummy resistor is present, and the case passes in Release.

PASS.

All Phase 2 analytic thresholds pass, timestep refinement decreases error,
energy drift remains within integration tolerance, and the complete repository
suite passes without a regression.

## Confirmation

No work from a later phase was implemented.
