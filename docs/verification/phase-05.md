# Phase 05 verification: multi-body spring-damper connections

Date: 2026-07-10

Working-tree base: `a6735a14861ccc60bf594476bc24287e3f64a380`.
The verified corrective tree is intentionally uncommitted.

Target frameworks:

- `SpiceSharpMechanical2D`: `netstandard2.0` and `net8.0`;
- tests and samples: `net8.0`.

Pinned package: `SpiceSharp` 3.2.3.

## Scope implemented

Phase 05 provides nonlinear force transfer between rigid bodies, or between a
rigid body and the fixed world, through:

- `DistanceSpringDamper2D`, using body-local or fixed world anchors;
- `RotationalSpringDamper2D`, using body or fixed world rotations;
- a compliant `Pendulum` sample built from a distance connection and gravity.

The Phase 00-05 corrective review also:

- changed the torsional spring to a smooth periodic sine law;
- made coordinate-only and body-only circuits pass default SpiceSharp
  validation without dummy electrical components;
- made approximate vector/matrix comparison reject non-finite values;
- removed avoidable overflow from the smooth positive/negative part at
  magnitude `1e308`;
- added end-to-end nonlinear production-stamp references for `PointForce2D`
  and `DistanceSpringDamper2D`.

## Explicitly not implemented

- No exact constraint, joint, contact, friction, cam, or Phase 06 feature.
- No parser change.
- No custom simulation, integrator, solver, or force accumulator.
- No electrical pin or electrical solver equation on a mechanical entity.

## SpiceSharp API used

The implementation retains the public API documented by ADR-0001 through
ADR-0006. `MechanicalCoordinate` and `RigidBody2D` additionally implement the
public `IRuleSubject` hook and use `ComponentRuleParameters.Factory` plus
`IConductiveRule.AddPath` to register the existing ground reference during
validation. This is validation metadata only; mechanical state remains in
private real solver variables.

## Equation and sign conventions

The distance connection evaluates:

```text
d     = pB - pA
L_eps = sqrt(dot(d, d) + epsilon_L^2)
n     = d / L_eps
vr    = vBpoint - vApoint
vn    = dot(n, vr)
f     = k * (L_eps - L0) + c * vn
FA    = f * n
FB    = -FA
```

Each body receives `cross(r, F)`. The production behavior stamps derivatives
with respect to both bodies' positions, angles, linear velocities, and angular
velocities using the residual convention `-d(load)/d(state)`.

The rotational connection evaluates:

```text
e_raw        = thetaB - thetaA - theta0
e_diagnostic = wrapToPi(e_raw)
e_omega      = omegaB - omegaA
tauA         = k_theta * sin(e_raw) + c_theta * e_omega
tauB         = -tauA
d(tauA)/d(e_raw) = k_theta * cos(e_raw)
```

`k_theta` is the tangent stiffness at the reference angle. The diagnostic
shortest angle wraps, but neither the solver torque nor its Jacobian has a
branch discontinuity at `+/-pi`.

## Files added or modified

Production:

- `src/SpiceSharpMechanical2D/Core/MechanicalValidation.cs`;
- `src/SpiceSharpMechanical2D/Core/MechanicalCoordinate.cs`;
- `src/SpiceSharpMechanical2D/Bodies/RigidBody2D.cs`;
- `src/SpiceSharpMechanical2D/Mathematics/Vector2D.cs`;
- `src/SpiceSharpMechanical2D/Mathematics/SmoothFunctions.cs`;
- `src/SpiceSharpMechanical2D/Connections/RotationalSpringDamper2D.cs`;
- `src/SpiceSharpMechanical2D/Connections/RotationalSpringDamper2DEquation.cs`;
- `src/SpiceSharpMechanical2D/Connections/RotationalSpringDamper2DParameters.cs`.

Tests and samples:

- `src/SpiceSharpMechanical2D.Tests/Mathematics/Vector2DTests.cs`;
- `src/SpiceSharpMechanical2D.Tests/Mathematics/Matrix2x2DTests.cs`;
- `src/SpiceSharpMechanical2D.Tests/Mathematics/SmoothFunctionsTests.cs`;
- `src/SpiceSharpMechanical2D.Tests/Coordinates/MechanicalCoordinateTests.cs`;
- `src/SpiceSharpMechanical2D.Tests/Bodies/RigidBody2DTests.cs`;
- `src/SpiceSharpMechanical2D.Tests/Forces/BasicForceTests.cs`;
- `src/SpiceSharpMechanical2D.Tests/Forces/ForceTestSimulation.cs`;
- `src/SpiceSharpMechanical2D.Tests/Connections/ConnectionEquationTests.cs`;
- `src/SpiceSharpMechanical2D.Tests/Connections/ConnectionTestSimulation.cs`;
- `src/SpiceSharpMechanical2D.Tests/Connections/ConnectionTransientTests.cs`;
- `samples/SpiceSharpMechanical2D.Samples/FreeFall/Program.cs`;
- `samples/SpiceSharpMechanical2D.Samples/Pendulum/Program.cs`.

Documentation:

- `docs/implementation-plan.md`;
- `docs/architecture/ADR-0003-mechanical-coordinate-and-direct-force-stamping.md`;
- `docs/architecture/ADR-0004-rigid-body-state-and-kinematics.md`;
- `docs/architecture/ADR-0006-multi-body-spring-damper-connections.md`;
- `docs/verification/phase-01.md` through `phase-05.md` where affected.

## Commands executed

- `dotnet restore src/SpiceSharp-Parser.sln`
  - Initial sandboxed result: network-denied `NU1301`.
  - Required network-enabled rerun: PASS; all projects restored.
- Focused `dotnet format ... --verify-no-changes --no-restore` for the
  Physics2D library, test project, FreeFall sample, and Pendulum sample.
  - Result: PASS for all four projects.
- `dotnet build src/SpiceSharp-Parser.sln -c Release --no-restore --verbosity minimal`
  - Result: PASS; 0 errors.
  - Physics2D library/tests/samples: 0 warnings.
  - Complete solution: 259 pre-existing parser/code-analysis warnings.
- `dotnet test src/SpiceSharp-Parser.sln -c Release --no-build --no-restore --logger "console;verbosity=minimal"`
  - Result: PASS; 2,428 passed, 11 skipped, 0 failed.
- Release runs of both samples with `--no-build --no-restore`.
  - Result: PASS with required headers and finite final rows.

The repository does not define a solution-wide formatter gate and the legacy
parser tree is not `dotnet format` baseline-clean. Every changed project passes
formatter verification; unrelated parser formatting remains untouched.

## Test summary

- Corrective test cases added: 9.
- Phase 5 connection tests: 16 passed.
- Combined Phase 4-5 force/connection tests: 38 passed.
- Complete Physics2D project: 125 passed, 0 skipped, 0 failed.
- Complete solution: 2,428 passed, 11 skipped, 0 failed.

## Numerical cases

| Check | Required | Measured | Result |
| --- | ---: | ---: | --- |
| Distance force action/reaction | `<= 1e-11 N` | `0 N` | Pass |
| Net internal world-origin torque | `<= 1e-10 N*m` | `0 N*m` | Pass |
| Distance full-state analytic Jacobian | relative `<= 5e-6` | `7.503406784792332e-10` | Pass |
| Rotational full-state analytic Jacobian | relative `<= 5e-6` | `2.105423613230073e-10` | Pass |
| Rotational seam Jacobian | relative `<= 5e-6` | `9.77279590586022e-10` absolute | Pass |
| Off-center distance production trajectory | scale-aware `<= 2e-6` | `4.6107812567974804e-8` maximum absolute | Pass |
| Off-center world-point-force trajectory | scale-aware `<= 2e-7` | `9.331235506504498e-9` maximum angular-state absolute | Pass |
| Reduced-mass oscillator frequency | relative `<= 3e-3` | `2.2222331169160068e-6` | Pass |
| Periodic torsional oscillator frequency | relative `<= 3e-3` | `1.5757813460237635e-4` | Pass |
| Damped world spring displacement | analytic comparison | `2.6532872210993652e-6 m` absolute | Pass |
| Timestep refinement | decreasing | `4.9722594911205675e-5`, `1.2575951775284366e-5`, `3.163552069973541e-6 m` | Pass |
| Smooth positive part at finite `1e308` | finite | finite, relative error `<= 1e-15` | Pass |

The torsional frequency case uses initial amplitude `0.05 rad`; the remaining
small frequency shift is the expected finite-amplitude behavior of the sine
law. Both independent production-stamp references use fourth-order Runge-Kutta
with a reference step at least 50 times smaller than the SpiceSharp maximum
timestep.

## Jacobian verification

The direct equation tests finite-difference every distance state and every
rotational state. A separate finite-difference stencil is centered exactly at
relative error `pi`, proving that the diagnostic wrap seam does not appear in
the torque or tangent stiffness. The two independent transient references
exercise the production row/column mapping, sign convention, and Newton RHS
linearization rather than calling the production equation helper.

## Energy and power checks

The periodic torsional elastic potential is
`k_theta * (1 - cos(e_raw))`; its derivative is the stamped sine torque.
Distance and rotational damping retain nonpositive internal damping power.
No energy claim is made for the externally driven point-force reference.

## Determinism check

All new cases use fixed parameters and no random input. Existing repeated-run
and entity-order tests continue to pass bitwise or within their documented
roundoff tolerance.

## Performance observations

The complete Physics2D Release suite runs 125 tests in under one second on the
verification host. The independent high-resolution reference integrators are
test-only and are not present in production load loops.

## Known limitations

- The sine torsional law has an unstable zero-torque equilibrium at antipodal
  relative orientation; this is explicit periodic-potential behavior.
- A wrapped shortest-angle diagnostic is discontinuous as a displayed value at
  `+/-pi`, but it is not used in the solver residual or Jacobian.
- Mechanical validation registration supplies no electrical equation and does
  not disable or bypass checks for actual floating electrical topology.
- Connection entities still resolve referenced bodies during behavior
  construction, so bodies must precede their connections in entity order.
- Scaling diagnostics remain scheduled for the diagnostics phase.

## Decision

PASS.

All five Phase 00-05 review findings requested for correction are resolved and
covered by Release tests.

## Confirmation

No Phase 06 or later implementation was added.
