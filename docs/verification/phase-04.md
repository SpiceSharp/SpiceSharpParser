# Phase 04 verification

## Scope implemented

- Added six public rigid-body load entities:
  - `Gravity2D`;
  - `AppliedForce2D`;
  - `PointForce2D`;
  - `AppliedTorque2D`;
  - `LinearDrag2D`;
  - `AngularDrag2D`.
- Added a shared internal transient load behavior that resolves one
  `IRigidBody2DBehavior` during setup, skips the requested-state operating
  point, and stamps during the ordinary SpiceSharp transient load pass.
- Added constant world acceleration with `F = M*g`.
- Added constant and time-dependent center-of-mass world force. The time
  overload uses the package-owned `ForceFunction2D(double time)` delegate and
  SpiceSharp's current `IIntegrationMethod.Time`.
- Added point force at a body-local application point with explicit `World`
  and `BodyLocal` force-coordinate modes.
- Added analytic angle derivatives for rotated point offsets, rotated local
  force, and resulting torque, and stamped the complete Newton matrix/RHS
  contribution into all three body dynamics rows.
- Added constant world torque, isotropic linear drag relative to a world
  medium velocity, and angular drag relative to a medium angular velocity.
- Added setup-time validation for finite vectors/torques, supported force
  coordinates, and finite nonnegative damping.
- Added a FreeFall executable sample to the solution. It emits invariant CSV
  columns `time,x,y,vx,vy,angle,omega`.

## Explicitly not implemented

- No spring, rotational spring, damper connection, joint, contact, friction,
  cam, or electromechanical coupling.
- No time-dependent point force or time-dependent applied torque; Phase 4
  requires time dependence only for `AppliedForce2D`.
- No expression parsing or parser changes.
- No force/torque accumulator, interaction registry, collision search, or
  world step.
- No custom simulation, solver, or integration method.
- No work from Phase 5 or any later phase.

## SpiceSharp API used

- `Entity<T>.CreateBehaviors(ISimulation)`.
- `BehaviorContainer`, `BindingContext`, and
  `ISimulation.EntityBehaviors`.
- `IBiasingBehavior` and `ITimeBehavior`.
- `Reference.GetContainer` and `IBehaviorContainer.GetValue<T>()` for
  deterministic setup-time rigid-body resolution.
- `IBiasingSimulationState.Map` and `Solver`.
- `ElementSet<double>` with RHS-only locations for constant loads and matrix
  plus RHS locations for drag and point-force derivatives.
- `ITimeSimulationState.UseDc` to exclude loads from the requested-state
  operating point.
- `IIntegrationMethod.Time` for deterministic time-function evaluation at the
  currently probed transient point.

The body state, row ownership, frame, point-velocity, and torque conventions
remain those documented in
[ADR-0004](../architecture/ADR-0004-rigid-body-state-and-kinematics.md).
The direct generalized-load Newton sign convention remains that of
[ADR-0003](../architecture/ADR-0003-mechanical-coordinate-and-direct-force-stamping.md).
The Phase 4 load lifecycle, time-function contract, frame semantics, drag
stamps, and point-force linearization are documented in
[ADR-0005](../architecture/ADR-0005-direct-rigid-body-loads-and-point-force-linearization.md).

## Files changed

- `src/SpiceSharp.Physics2D/Forces/ForceFunction2D.cs`
- `src/SpiceSharp.Physics2D/Forces/ForceCoordinateSystem2D.cs`
- `src/SpiceSharp.Physics2D/Forces/RigidBodyLoadBehavior.cs`
- `src/SpiceSharp.Physics2D/Forces/PointForce2DEquation.cs`
- `src/SpiceSharp.Physics2D/Forces/Gravity2DParameters.cs`
- `src/SpiceSharp.Physics2D/Forces/Gravity2D.cs`
- `src/SpiceSharp.Physics2D/Forces/AppliedForce2DParameters.cs`
- `src/SpiceSharp.Physics2D/Forces/AppliedForce2D.cs`
- `src/SpiceSharp.Physics2D/Forces/PointForce2DParameters.cs`
- `src/SpiceSharp.Physics2D/Forces/PointForce2D.cs`
- `src/SpiceSharp.Physics2D/Forces/AppliedTorque2DParameters.cs`
- `src/SpiceSharp.Physics2D/Forces/AppliedTorque2D.cs`
- `src/SpiceSharp.Physics2D/Forces/LinearDrag2DParameters.cs`
- `src/SpiceSharp.Physics2D/Forces/LinearDrag2D.cs`
- `src/SpiceSharp.Physics2D/Forces/AngularDrag2DParameters.cs`
- `src/SpiceSharp.Physics2D/Forces/AngularDrag2D.cs`
- `src/SpiceSharp.Physics2D/Properties/AssemblyInfo.cs`
- `src/SpiceSharp.Physics2D.Tests/Forces/ForceTestSimulation.cs`
- `src/SpiceSharp.Physics2D.Tests/Forces/BasicForceTests.cs`
- `src/SpiceSharp.Physics2D.Tests/Forces/PointForce2DJacobianTests.cs`
- `docs/architecture/ADR-0005-direct-rigid-body-loads-and-point-force-linearization.md`
- `samples/SpiceSharp.Physics2D.Samples/FreeFall/SpiceSharp.Physics2D.Samples.FreeFall.csproj`
- `samples/SpiceSharp.Physics2D.Samples/FreeFall/Program.cs`
- `src/SpiceSharp-Parser.sln`
- `docs/verification/phase-04.md`

## Commands executed

Working-tree base:
`8e0d40f4b4a940fe0861ab5f1632d890f0165566`.

Target frameworks:

- `SpiceSharp.Physics2D`: `netstandard2.0` and `net8.0`;
- `SpiceSharp.Physics2D.Tests`: `net8.0`;
- `SpiceSharp.Physics2D.Samples.FreeFall`: `net8.0`.

Pinned package:

- `SpiceSharp` 3.2.3.

- `dotnet restore src/SpiceSharp-Parser.sln`
  - Result: PASS after the required network-enabled rerun; all solution and
    referenced projects restored.
  - The sandboxed attempt could not retrieve NuGet repository-signature
    metadata. No dependency version changed.
- `dotnet format src/SpiceSharp-Parser.sln --verify-no-changes --no-restore
  --verbosity minimal`
  - Result: the unchanged repository-wide diagnostic retains the pre-existing
    parser and integration-test whitespace violations recorded by earlier
    phases.
  - No Physics2D, Phase 4 test, or FreeFall sample file appeared in the
    diagnostic.
- `dotnet format src/SpiceSharp.Physics2D/SpiceSharp.Physics2D.csproj
  --verify-no-changes --no-restore --verbosity minimal`
  - Result: PASS.
- `dotnet format
  src/SpiceSharp.Physics2D.Tests/SpiceSharp.Physics2D.Tests.csproj
  --verify-no-changes --no-restore --verbosity minimal`
  - Result: PASS.
- `dotnet format
  samples/SpiceSharp.Physics2D.Samples/FreeFall/SpiceSharp.Physics2D.Samples.FreeFall.csproj
  --verify-no-changes --no-restore --verbosity minimal`
  - Result: PASS.
- `dotnet build src/SpiceSharp.Physics2D/SpiceSharp.Physics2D.csproj
  -c Release --no-restore --no-incremental`
  - Result: PASS for `netstandard2.0` and `net8.0`; 0 warnings, 0 errors.
- `dotnet test
  src/SpiceSharp.Physics2D.Tests/SpiceSharp.Physics2D.Tests.csproj
  -c Release --no-restore --filter
  "FullyQualifiedName~SpiceSharp.Physics2D.Tests.Forces" --logger
  "console;verbosity=detailed"`
  - Result: PASS; 21 passed, 0 skipped, 0 failed.
- `dotnet test
  src/SpiceSharp.Physics2D.Tests/SpiceSharp.Physics2D.Tests.csproj
  -c Release --no-build --no-restore --logger "console;verbosity=minimal"`
  - Result: PASS; 102 passed, 0 skipped, 0 failed.
- `dotnet build
  samples/SpiceSharp.Physics2D.Samples/FreeFall/SpiceSharp.Physics2D.Samples.FreeFall.csproj
  -c Release --no-restore --no-incremental`
  - Result: PASS; 0 warnings, 0 errors.
- `dotnet run --project
  samples/SpiceSharp.Physics2D.Samples/FreeFall/SpiceSharp.Physics2D.Samples.FreeFall.csproj
  -c Release --no-build --no-restore`
  - Result: PASS; emitted the required CSV header and finite samples from time
    0 through 2 seconds.
  - Final row:
    `2,4.000000000000002,-9.613300098066492,2,-19.613300000000084,0,0`.
- `dotnet build src/SpiceSharp-Parser.sln -c Release --no-restore
  --verbosity quiet`
  - Result: PASS; 0 errors. The 259 warnings are the unchanged parser,
    code-analysis, and parser-test baseline. Focused Physics2D and sample
    builds have 0 warnings.
- `dotnet test src/SpiceSharp-Parser.sln -c Release --no-build --no-restore
  --logger "console;verbosity=minimal"`
  - Result: PASS; 2,405 passed, 11 skipped, 0 failed.

## Test summary

- Phase 4 test cases added: 21.
- Physics2D test project: 102 passed, including all retained Phase 0 through
  Phase 3 cases.
- Complete solution: 2,405 passed, 11 skipped, 0 failed.
- Test-project breakdown:
  - `SpiceSharp.Physics2D.Tests`: 102 passed;
  - `SpiceSharpParser.AIExamples`: 948 passed;
  - `SpiceSharpParser.Tests`: 408 passed, 9 skipped;
  - `SpiceSharpParser.IntegrationTests`: 947 passed, 2 skipped;
  - `SpiceSharpParser.PerformanceTests`: no discoverable tests, unchanged.

## Numerical cases

All transient acceptance cases use SpiceSharp's variable-step `Trapezoidal`
integration method with `InitialStep` and `MaxStep` set explicitly.

### Free fall

- Parameters: mass 2, acceleration `(0, -9.81)`, initial position `(0.5, 12)`,
  initial velocity `(0.2, 0.3)`, stop time 1.5.
- Expected result:
  `p(t) = p0 + v0*t + 0.5*g*t^2`, `v(t) = v0 + g*t`.
- Maximum component relative errors for h / h/2 / h/4:
  - position: `6.245092574325771e-7` / `2.775596193790285e-7` /
    `6.93898214062451e-8`;
  - velocity: `1.971675992397087e-15` / `4.066581734318992e-15` /
    `3.450432986694902e-15`;
  - maximum timesteps: 0.04 / 0.02 / 0.01.
- Acceptance: fine position `6.93898e-8 <= 1e-4`; fine velocity
  `3.45043e-15 <= 1e-5`.
- Position error decreased under refinement. Constant acceleration is
  integrated essentially exactly in velocity, so roundoff dominates there.

### Projectile without drag

- Parameters: mass 1.7, acceleration `(0, -9.81)`, initial position `(-1, 2)`,
  initial velocity `(3.5, 6)`, stop time 1.25, maximum timestep 0.01.
- Maximum position relative error: `5.3433160474014584e-8`.
- Maximum velocity relative error: `6.240295483721438e-15`.
- Horizontal speed remained constant while vertical motion matched the
  analytic gravitational trajectory.

### Linear drag

- Parameters: mass 2, damping 0.8, medium velocity `(0.5, -0.25)`, initial
  position `(-0.4, 0.8)`, initial velocity `(3, -2)`, stop time 2.
- Expected result:

  ```text
  v(t) = vMedium + (v0-vMedium)*exp(-(c/M)t)
  p(t) = p0 + vMedium*t + (M/c)*(v0-vMedium)*(1-exp(-(c/M)t))
  ```

- Maximum component relative errors for h / h/2 / h/4:
  - position: `3.917928948142365e-6` / `9.86073783314349e-7` /
    `2.4734275566136134e-7`;
  - velocity: `3.189591910182845e-6` / `8.027641326984261e-7` /
    `2.0136206006404327e-7`;
  - maximum timesteps: 0.02 / 0.01 / 0.005.
- Acceptance: fine decay error `2.01362e-7 <= 2e-4`.
- Both errors decreased by approximately four per halving, consistent with the
  selected trapezoidal method for this smooth linear system.

### Angular drag

- Parameters: inertia 0.6, damping 0.3, medium angular velocity 0.2, initial
  angle -0.4, initial angular velocity 2.5, stop time 2, maximum timestep
  0.002.
- Angle relative error: `4.837560722768628e-8`.
- Angular-velocity decay relative error: `6.723100751616383e-8`.
- Acceptance: `6.72310e-8 <= 2e-4`.
- Final kinetic energy was lower than the initial 1.875 joules.

### Time-dependent applied force

- Parameters: mass 2, `F(t) = (2t, -t)`, zero initial state, stop time 1,
  maximum timestep 0.002.
- Expected final velocity `(0.5, -0.25)` and position
  `(1/6, -1/12)`.
- Maximum absolute position error: `3.3254314088515535e-7`.
- Maximum absolute velocity error: `4.000038078544321e-10`.
- The delegate was evaluated from the current SpiceSharp integration time; no
  expression parser or alternate time lifecycle was introduced.

### Torque, point force, superposition, and order

- Constant applied torque matched the analytic constant-angular-acceleration
  solution within `1e-5` angular-velocity and `1e-4` angle tolerances.
- A center point force left angular velocity constant within `1e-12`.
- A body-local center force at initial angle `pi/2` produced world y-velocity
  1 and zero angular velocity.
- An off-center body-local force with local lever arm `(1,0)` and local force
  `(0,4)` produced constant torque 4 and matched analytic angle/angular
  velocity within the Phase 4 motion tolerances.
- Two separate center-force stamps produced the same state as their summed
  force within `1e-12`.
- Reversing three component entities changed any exported state by at most
  `5.551115123125783e-17`.

## Jacobian verification

`PointForce2D` is the only nonlinear Phase 4 component. For body angle
`theta`, local point `r`, and configured force `F`:

```text
rWorld = R(theta)*r
drWorld/dtheta = perpendicular(rWorld)

world-force mode:  FWorld = F
                   dFWorld/dtheta = 0

body-local mode:   FWorld = R(theta)*F
                   dFWorld/dtheta = perpendicular(FWorld)

tau = cross(rWorld, FWorld)
dtau/dtheta = cross(drWorld/dtheta, FWorld)
             + cross(rWorld, dFWorld/dtheta)
```

For each generalized load `Q` in `(Fx, Fy, tau)`, the dynamics residual
contains `-Q`, so the component stamps:

```text
matrix(angle) = -dQ/dtheta
rhs           = Q - dQ/dtheta*theta
```

Independent central finite differences used angle 0.63, local point
`(0.7, -1.1)`, force `(2.3, -0.8)`, relative step `1e-6`, and minimum step
`1e-7`.

- World force:
  - maximum absolute mismatch: `1.5587842128184093e-10`;
  - maximum scale-aware relative mismatch: `4.91326486207471e-11`.
- Body-local force:
  - maximum absolute mismatch: `1.9956858388070486e-10`;
  - maximum scale-aware relative mismatch: `1.9956858388070486e-10`.
- Acceptance: worst relative mismatch `1.99569e-10 <= 2e-6`.
- Direct off-center torque expected 6 and measured 6; absolute error 0,
  satisfying `0 <= 1e-11`.

Gravity, constant force, constant torque, linear drag, and angular drag are
linear in Phase 4. Their constant analytic derivatives are exercised by the
trajectory, superposition, and order tests.

## Energy or power checks

- Linear and angular drag stamps have positive damping coefficients in the
  residual matrix and oppose velocity relative to their configured medium.
- The angular-drag trajectory began with kinetic energy 1.875 joules and
  ended lower while matching the analytic exponential decay.
- No conservative interaction or stored potential-energy component is added
  in Phase 4.

## Determinism check

- Force functions receive only the currently probed time and are documented
  to be deterministic and side-effect-free because SpiceSharp may evaluate a
  timepoint more than once during Newton iteration.
- Reordering additive constant components changed the full exported time
  series by at most `5.551115123125783e-17`, within roundoff.
- All tests use fixed inputs and contain no random state.

## Performance observations

The focused 21-case Phase 4 run completed in 0.9280 seconds according to
VSTest. The complete 102-case Physics2D project completed in 0.274 seconds in
the final solution run. The FreeFall sample completed in 3.3 seconds including
process startup and CSV output. Formal performance baselines remain assigned
to Phase 14.

## Known limitations

- `ForceFunction2D` is arbitrary user code. It must be deterministic,
  side-effect-free, and finite; the behavior rejects a non-finite returned
  force but cannot prove purity.
- `PointForce2D` supports constant configured force vectors only. Time-varying
  point force is not part of Phase 4.
- Drag is linear and isotropic. It is not quadratic aerodynamic drag and does
  not infer area or fluid density.
- Loads do not participate in the specified-state operating point. There is
  no mechanical static-equilibrium initialization.
- Load entities resolve their body during setup and must follow it in
  behavior-construction order.
- No authoritative net-force, net-torque, acceleration, or per-component
  diagnostic export is provided because components stamp directly.
- A pure zero-pin circuit does not satisfy SpiceSharp electrical validation;
  tests and the sample retain the isolated grounded validation resistor
  documented by Phase 0.
- Large loads, damping coefficients, or disparate scales are not silently
  rescaled.

## Decision

PASS.

All Phase 4 analytic, refinement, torque, Jacobian, superposition, order, and
sample gates pass. The complete repository suite passes without a regression.

## Confirmation

No work from a later phase was implemented.
