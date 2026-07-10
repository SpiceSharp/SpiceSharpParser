# Phase 03 verification

## Scope implemented

- Added the public `RigidBody2D` entity and parameter set as one zero-pin body
  entity rather than three user-managed coordinate entities.
- Added one public `IRigidBody2DBehavior` exposing six private solver
  variables: world x/y position, unbounded angle, world x/y velocity, and
  angular velocity.
- Added three position derivative histories and three generalized-momentum
  histories. Translational momentum uses `M*v`; angular momentum uses
  `I*omega`.
- Stamped three independent kinematic and inertial equation pairs into an
  ordinary SpiceSharp `Transient`:

  ```text
  xdot     - vx    = 0      M*vxdot     = 0
  ydot     - vy    = 0      M*vydot     = 0
  angledot - omega = 0      I*omegadot  = 0
  ```

- Added deterministic operating-point holds for all six requested initial
  state values.
- Added live scalar exports for position, angle, velocities, mass, inertia,
  linear kinetic energy, angular kinetic energy, and total kinetic energy.
- Added behavior-level geometry helpers for local/world points and vectors,
  point velocity, and torque from a world force applied at a local point.
- Added an internal test-only constant world-force/torque entity. It resolves
  the body behavior during setup and stamps directly into the three dynamics
  RHS locations during its normal SpiceSharp load pass.

## Explicitly not implemented

- No public force or torque entity. `Gravity2D`, `AppliedForce2D`,
  `PointForce2D`, `AppliedTorque2D`, `LinearDrag2D`, and `AngularDrag2D` remain
  assigned to Phase 4.
- No shape, collision geometry, center-of-mass offset, gravity, spring, joint,
  contact, friction, cam, or electromechanical coupling.
- No force or torque accumulator and no rigid-body world step.
- No parser change.
- No custom simulation, solver, or integration method.
- No work from Phase 4 or any later phase.

## SpiceSharp API used

- `Entity<T>.CreateBehaviors(ISimulation)`.
- `BehaviorContainer`, `BindingContext`, and
  `ISimulation.EntityBehaviors`.
- `IBiasingBehavior` and `ITimeBehavior`.
- `IBiasingSimulationState.CreatePrivateVariable`, `Map`, and `Solver`.
- `IIntegrationMethod.CreateDerivative`.
- `IDerivative.Value`, `Derive`, and `GetContributions`.
- `ElementSet<double>` with matrix/RHS locations for the body and an RHS-only
  element set for the test load.
- `Reference.GetContainer` and `IBehaviorContainer.GetValue<T>()` for
  deterministic setup-time body linking.
- `GeneratedParametersAttribute`, `ParameterNameAttribute`, and
  `RealPropertyExport`.

The Phase 3 body follows the coordinate state, initialization, and direct
assembly convention documented in
[ADR-0003](../architecture/ADR-0003-mechanical-coordinate-and-direct-force-stamping.md).
The body-level state ownership, frame conventions, kinematic helpers, and
public behavior boundary are documented in
[ADR-0004](../architecture/ADR-0004-rigid-body-state-and-kinematics.md).

## Files changed

- `src/SpiceSharp.Physics2D/Bodies/RigidBody2DParameters.cs`
- `src/SpiceSharp.Physics2D/Bodies/RigidBody2D.cs`
- `src/SpiceSharp.Physics2D/Bodies/IRigidBody2DBehavior.cs`
- `src/SpiceSharp.Physics2D/Bodies/RigidBody2DBehavior.cs`
- `src/SpiceSharp.Physics2D.Tests/Bodies/TestRigidBodyLoad2D.cs`
- `src/SpiceSharp.Physics2D.Tests/Bodies/RigidBody2DTests.cs`
- `docs/architecture/ADR-0004-rigid-body-state-and-kinematics.md`
- `docs/verification/phase-03.md`

## Commands executed

Working-tree base:
`b5a82a037f17e491bbd7892628dfe2fabab40960`.

Target frameworks:

- `SpiceSharp.Physics2D`: `netstandard2.0` and `net8.0`;
- `SpiceSharp.Physics2D.Tests`: `net8.0`.

Pinned package:

- `SpiceSharp` 3.2.3.

- `dotnet restore src/SpiceSharp-Parser.sln`
  - Result: PASS; all projects were up-to-date.
  - The first sandboxed attempt could not reach NuGet. The required rerun with
    network access succeeded without changing package versions.
- `dotnet format src/SpiceSharp-Parser.sln --verify-no-changes --no-restore
  --verbosity minimal`
  - Result: the unchanged repository-wide diagnostic retains the pre-existing
    parser and integration-test whitespace violations recorded by earlier
    phases.
  - No Phase 3 file appeared in the diagnostic.
- `dotnet format src/SpiceSharp.Physics2D/SpiceSharp.Physics2D.csproj
  --verify-no-changes --no-restore --verbosity minimal`
  - Result: PASS.
- `dotnet format
  src/SpiceSharp.Physics2D.Tests/SpiceSharp.Physics2D.Tests.csproj
  --verify-no-changes --no-restore --verbosity minimal`
  - Result: PASS.
- `dotnet build src/SpiceSharp.Physics2D/SpiceSharp.Physics2D.csproj
  -c Release --no-restore --no-incremental`
  - Result: PASS for `netstandard2.0` and `net8.0`; 0 warnings, 0 errors.
- `dotnet test
  src/SpiceSharp.Physics2D.Tests/SpiceSharp.Physics2D.Tests.csproj
  -c Release --no-restore --filter
  "FullyQualifiedName~SpiceSharp.Physics2D.Tests.Bodies" --logger
  "console;verbosity=detailed"`
  - Result: PASS; 19 passed, 0 skipped, 0 failed.
- `dotnet test
  src/SpiceSharp.Physics2D.Tests/SpiceSharp.Physics2D.Tests.csproj
  -c Release --no-build --no-restore --logger "console;verbosity=minimal"`
  - Result: PASS; 81 passed, 0 skipped, 0 failed.
- `dotnet build src/SpiceSharp-Parser.sln -c Release --no-restore
  --verbosity quiet`
  - Result: PASS; 0 errors. The 259 reported warnings are the unchanged parser,
    code-analysis, and parser-test baseline; the focused Physics2D build has
    0 warnings.
- `dotnet test src/SpiceSharp-Parser.sln -c Release --no-build --no-restore
  --logger "console;verbosity=minimal"`
  - Result: PASS; 2,384 passed, 11 skipped, 0 failed.

## Test summary

- Phase 3 test cases added: 19 across 12 required behaviors, including eight
  invalid mass/inertia data cases.
- Physics2D test project: 81 passed, including all retained Phase 0 through
  Phase 2 cases.
- Complete solution: 2,384 passed, 11 skipped, 0 failed.
- Test-project breakdown:
  - `SpiceSharp.Physics2D.Tests`: 81 passed;
  - `SpiceSharpParser.AIExamples`: 948 passed;
  - `SpiceSharpParser.Tests`: 408 passed, 9 skipped;
  - `SpiceSharpParser.IntegrationTests`: 947 passed, 2 skipped;
  - `SpiceSharpParser.PerformanceTests`: no discoverable tests, unchanged.

## Numerical cases

All transient cases use SpiceSharp's variable-step `Trapezoidal` integration
method with `InitialStep` and `MaxStep` set explicitly. Physics2D does not add
or replace an integration method.

### Constant world force through the center of mass

- Parameters: mass 2, inertia 0.7, force `(3, -2)`, initial position
  `(0.5, -0.75)`, initial velocity `(0.2, 0.4)`, initial angle 0.3, initial
  angular velocity -0.2, stop time 1.25.
- Expected result: independent constant-acceleration motion in x and y; no
  angular acceleration.
- Maximum component relative errors for h / h/2 / h/4:
  - position: `6.0606060668361e-8` / `3.8787879628726485e-8` /
    `9.69697179119645e-9`;
  - linear velocity: `4.2803779262656633e-16` /
    `8.560755852531327e-16` / `3.638321237325814e-15`;
  - maximum timesteps: 0.04 / 0.02 / 0.01.
- Acceptance: fine position error `9.69697e-9 <= 1e-4`; fine linear-velocity
  error `3.63832e-15 <= 1e-5`.
- Position error decreased under timestep refinement. Constant acceleration is
  integrated essentially exactly in velocity, so roundoff dominates there.

### Constant torque

- Parameters: mass 1, inertia 0.8, torque 1.6, initial angle 0.4, initial
  angular velocity -0.3, stop time 1.5.
- Expected angular acceleration: 2 radians per second squared.
- Relative errors for h / h/2 / h/4:
  - angle: `3.636363836308346e-8` / `9.090912467257795e-9` /
    `2.272756528430942e-9`;
  - angular velocity: `2.1382073066854867e-15` /
    `1.0691036533427433e-14` / `3.338892948131952e-14`;
  - maximum timesteps: 0.02 / 0.01 / 0.005.
- Acceptance: fine angle error `2.27276e-9 <= 1e-4`; fine angular-velocity
  error `3.33889e-14 <= 1e-5`.
- Angle error decreased by approximately four per halving, consistent with the
  selected trapezoidal method for this smooth case. Angular-velocity error is
  at roundoff scale.

### Combined force and torque

- Parameters: mass 1.5, inertia 0.6, force `(-2, 4)`, torque -0.9, initial
  position `(-0.4, 0.8)`, initial velocity `(0.5, -0.2)`, initial angle -0.7,
  initial angular velocity 0.3, stop time 1.2, maximum timestep 0.01.
- Maximum relative errors for position / velocity / angle / angular velocity:
  `1.754386124679024e-8` / `7.549516567451066e-15` /
  `1.0563383813988627e-8` / `4.440892098500627e-16`.
- Translation and rotation matched their independent analytic solutions with
  no cross-coupling.

### No-force inertial motion

- Parameters: mass 2, inertia 0.7, initial position `(0.25, -0.5)`, initial
  velocity `(-0.4, 0.75)`, initial angle 0.6, initial angular velocity -1.25,
  stop time 2, maximum timestep 0.04.
- Expected result: constant linear and angular velocities with affine position
  and angle.
- Maximum absolute error across all six states:
  `2.886579864025407e-15`.

### Geometry helpers

- Transform state: world position `(2, -1)`, angle 0.73 radians.
- Local and world point/vector transforms were tested in both directions.
- Maximum round-trip absolute component error:
  `2.220446049250313e-16`.
- Acceptance: `2.22045e-16 <= 1e-12`.
- Pure angular point-velocity case: angle `pi/2`, angular velocity 3,
  local point `(2, 0)`; expected and measured velocity `(-6, 0)` within
  `1e-12` scale-aware tolerance.
- Torque signs: `(2,0) x (0,3) = +6`, `(2,0) x (0,-3) = -6`, and
  `(0,2) x (3,0) = -6`; all matched exactly.

### Unbounded angle and independent bodies

- Unbounded-angle case: initial angle `5*pi`, angular velocity 4, stop time 3.
- Expected angle: `27.707963267948966`; measured:
  `27.707963267949218`.
- The state remained greater than `2*pi` and was not wrapped.
- Two bodies in one simulation exposed six distinct solver-variable instances
  each and retained distinct positions, velocities, and angular velocities.

## Jacobian verification

Phase 3 introduces no nonlinear residual. Each body coordinate contributes the
same linear kinematic and inertial companions verified in Phase 2, and the
test-only constant body load has zero state derivatives and stamps only:

```text
rhs(vx row)    = Fx
rhs(vy row)    = Fy
rhs(omega row) = torque
```

The local/world geometry helpers evaluate kinematics but do not stamp a
residual in this phase. Independent finite-difference Jacobian testing becomes
required when Phase 4 introduces angle-dependent point-force equations.

## Energy or power checks

- Parameters: mass 2, inertia 0.5, linear velocity `(3, 4)`, angular velocity
  2.
- Expected linear / angular / total kinetic energy: 25 / 1 / 26.
- Measured: `24.999999999999865` / `0.9999999999999918` /
  `25.999999999999858`.
- Each value passed a `1e-12` absolute and relative scale-aware tolerance.
- Phase 3 introduces no dissipative or energy-storing interaction component,
  so no separate power-balance law applies.

## Determinism check

Two complete combined-load simulations with identical configuration produced
59 samples each. Export type, time, x/y position, angle, x/y velocity, and
angular velocity were bitwise equal at every sample.

## Performance observations

The focused 19-case Phase 3 run completed in 0.8150 seconds according to
VSTest. The complete 81-case Physics2D project completed in 0.279 seconds in
the final solution run. Formal performance baselines remain assigned to
Phase 14.

## Known limitations

- The only force/torque entity is internal test infrastructure. Public force,
  torque, gravity, and drag components remain Phase 4 work.
- The body represents center-of-mass translation and planar rotation only; it
  has no shape, collision geometry, or center-of-mass offset.
- The behavior does not expose an authoritative net force, net torque, or
  acceleration diagnostic because connected components stamp directly.
- Linked test loads resolve their body during setup and must follow it in
  behavior-construction order.
- All six private solver variables use SpiceSharp's `Units.Volt` only as
  solver bookkeeping because the pinned public unit catalog has no mechanical
  generalized-coordinate units.
- A pure zero-pin circuit does not satisfy SpiceSharp's electrical validation;
  tests retain the isolated grounded validation resistor documented by Phase
  0.
- Mass, inertia, and initial values are validated, but large or disparate SI
  scales are not silently rescaled.

## Decision

PASS.

All Phase 3 analytic and geometry thresholds pass, timestep refinement reduces
the position and angle errors, two bodies retain independent state, repeated
runs are deterministic, and the complete repository suite passes without a
regression.

## Confirmation

No work from a later phase was implemented.
