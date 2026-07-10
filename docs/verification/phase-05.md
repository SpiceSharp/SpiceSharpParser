# Phase 05 Verification: Multi-body spring-damper connections

Date: 2026-07-10

## Scope

Phase 05 adds nonlinear force transfer between rigid bodies, or between one
rigid body and the fixed world, through:

- `DistanceSpringDamper2D`, using body-local or fixed world anchors;
- `RotationalSpringDamper2D`, using body or fixed world rotations;
- a compliant `Pendulum` sample built from a distance spring-damper and gravity.

No exact constraints, contacts, joints, or Phase 06 functionality are included.

## Equation and sign conventions

For endpoint positions `pA` and `pB`, the distance connection evaluates

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

Each body receives the corresponding moment-arm torque `cross(r, F)`. The
solver stamp contains the analytic derivatives with respect to both bodies'
positions, angles, linear velocities, and angular velocities. Matrix terms use
the existing rigid-body residual convention `-d(load)/d(state)`.

`epsilon_L` keeps the force and Jacobian finite at coincident anchors. It also
intentionally changes the constitutive response when physical separation is
comparable to `epsilon_L`; it is a model parameter, not only a numerical
tolerance.

The rotational connection evaluates

```text
e_theta = wrapToPi(thetaB - thetaA - theta0)
e_omega = omegaB - omegaA
tauA    = k_theta * e_theta + c_theta * e_omega
tauB    = -tauA
```

These signs oppose the relative error on each dynamic body. The analytic angle
derivative is valid within each wrapped branch. The exact half-open `+/-pi`
branch seam is discontinuous and is not a valid Newton linearization point;
the seam test crosses the periodic representation without evaluating exactly
on that branch boundary.

## Acceptance evidence

| Check | Required | Measured | Result |
| --- | ---: | ---: | --- |
| Distance force action/reaction | `<= 1e-11 N` | `0 N` | Pass |
| Net internal linear force | zero within tolerance | `0 N` | Pass |
| Net internal world-origin torque | `<= 1e-10 N*m` | `0 N*m` | Pass |
| Distance full-state analytic Jacobian | relative `<= 5e-6` | `7.503406784792332e-10` | Pass |
| Rotational full-state analytic Jacobian | relative `<= 5e-6` | `2.105423613230073e-10` | Pass |
| Reduced-mass oscillator frequency | relative `<= 3e-3` | `2.2222331169160068e-6` | Pass |
| Pure rotational oscillator frequency | relative `<= 3e-3` | `1.3334268135212213e-6` | Pass |
| Damped world spring analytic displacement | analytic comparison | `2.6532872210993652e-6 m` absolute error | Pass |
| Coincident anchors | all finite | no NaN or infinity in 6 loads or 72 derivatives | Pass |
| Timestep refinement | convergent | `4.9722594911205675e-5`, `1.2575951775284366e-5`, `3.163552069973541e-6 m` | Pass |

The invariant and equation tests also cover one-body-to-world attachment,
off-center torques on both bodies, a pure rotational spring, damping, and the
wrapped-angle seam.

## Sample verification

Command:

```text
dotnet run --project samples/SpiceSharp.Physics2D.Samples/Pendulum/SpiceSharp.Physics2D.Samples.Pendulum.csproj --no-restore
```

Result: completed through `t = 3 s` and emitted finite CSV state and
`pivot_error` values. The final compliant anchor error was
`0.004321287349603146 m`. This is deliberately a compliant connection rather
than an exact revolute constraint.

## Build, format, and test commands

Focused formatting checks:

```text
dotnet format src/SpiceSharp.Physics2D/SpiceSharp.Physics2D.csproj --no-restore --verify-no-changes --include src/SpiceSharp.Physics2D/Connections
dotnet format src/SpiceSharp.Physics2D.Tests/SpiceSharp.Physics2D.Tests.csproj --no-restore --verify-no-changes --include src/SpiceSharp.Physics2D.Tests/Connections
dotnet format samples/SpiceSharp.Physics2D.Samples/Pendulum/SpiceSharp.Physics2D.Samples.Pendulum.csproj --no-restore --verify-no-changes
```

Result: all passed with no formatting changes required.

Focused build and tests:

```text
dotnet build src/SpiceSharp.Physics2D/SpiceSharp.Physics2D.csproj --no-restore
dotnet test src/SpiceSharp.Physics2D.Tests/SpiceSharp.Physics2D.Tests.csproj --no-restore --verbosity normal
```

Result: both `netstandard2.0` and `net8.0` Physics2D targets built with zero
warnings and zero errors; 116 tests passed, zero failed, zero skipped.

Complete repository suite:

```text
dotnet test src/SpiceSharp-Parser.sln --no-restore --verbosity minimal
```

Result: 2,419 tests passed, zero failed, and 11 pre-existing tests were skipped.
The command continued to emit the repository's pre-existing parser and test
warning baseline; Phase 05's focused projects emitted no warnings.
