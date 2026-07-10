# Phase 06 verification

## Scope implemented

- Added compliant `RevoluteJoint2D`, `WeldJoint2D`, and
  `PrismaticJoint2D` entities.
- Added body/body and body/world topology using explicit
  `MechanicalAnchor2D` endpoint frames.
- Added explicit weld and prismatic reference angles; setup never captures a
  reference silently from initial body state.
- Added isotropic revolute anchor stiffness and damping with off-center force
  and torque transfer.
- Added weld position and smooth periodic angular stiffness/damping.
- Added a prismatic guide that is world-fixed when endpoint A is world and
  rotates analytically with body A otherwise.
- Added normal prismatic compliance, relative-angle compliance, free axial
  travel by default, and optional axial spring/damping.
- Added complete coupled `6 x 12` Newton Jacobians through an allocation-free,
  fixed-width first-order analytic derivative value type.
- Added typed accepted-timepoint diagnostics for errors, reactions, stored
  elastic energy, and dissipated power.
- Added setup rejection for invalid endpoints, world/world and same-body
  topology, missing bodies, nonfinite references, invalid guide axes, and
  negative/nonfinite passive parameters.
- Added trace warnings for large initial preload without modifying the
  requested initial state.
- Added executable `SliderCrank` and `CompliantFourBar` samples to the
  solution. Both emit invariant CSV through more than one mechanism cycle.
- Recorded the formulation and lifecycle in
  [ADR-0007](../architecture/ADR-0007-compliant-planar-joints.md).

## Explicitly not implemented

- No exact algebraic constraints, Lagrange multipliers, projection,
  stabilization, or separate mechanics solve.
- No joint limits, stops, backlash, friction, contact, collision detection,
  or unilateral behavior.
- No parser syntax or netlist mapping for joint entities.
- No automatic stiffness selection, timestep selection, reference-angle
  capture, or preload correction.
- No work from Phase 07 or later phases.

## SpiceSharp API used

- `Entity<T>.CreateBehaviors(ISimulation)`.
- `BehaviorContainer`, `BindingContext`, and
  `ISimulation.EntityBehaviors`.
- `IBiasingBehavior`, `ITimeBehavior`, and `IAcceptBehavior`.
- `Reference.GetContainer` and `IBehaviorContainer.GetValue<T>()` for
  deterministic setup-time body binding.
- `IBiasingSimulationState.Map` and `ElementSet<double>` for precomputed
  coupled matrix/RHS locations.
- `ITimeSimulationState.UseDc` to preserve requested initial body state during
  the operating point.
- generated real-property exports backed by accepted diagnostic snapshots.

## Files added or changed

- `src/SpiceSharpMechanical2D/Joints/Dual12.cs`
- `src/SpiceSharpMechanical2D/Joints/JointEquationSupport.cs`
- `src/SpiceSharpMechanical2D/Joints/JointBehaviorBase.cs`
- `src/SpiceSharpMechanical2D/Joints/JointValidation.cs`
- `src/SpiceSharpMechanical2D/Joints/RevoluteJoint2D*.cs`
- `src/SpiceSharpMechanical2D/Joints/WeldJoint2D*.cs`
- `src/SpiceSharpMechanical2D/Joints/PrismaticJoint2D*.cs`
- `src/SpiceSharpMechanical2D/Joints/IRevoluteJoint2DBehavior.cs`
- `src/SpiceSharpMechanical2D/Joints/IWeldJoint2DBehavior.cs`
- `src/SpiceSharpMechanical2D/Joints/IPrismaticJoint2DBehavior.cs`
- `src/SpiceSharpMechanical2D.Tests/Joints/JointEquationTests.cs`
- `src/SpiceSharpMechanical2D.Tests/Joints/JointTransientTests.cs`
- `src/SpiceSharpMechanical2D.Tests/Joints/MechanismVerificationTests.cs`
- `samples/SpiceSharpMechanical2D.Samples/SliderCrank/*`
- `samples/SpiceSharpMechanical2D.Samples/CompliantFourBar/*`
- `src/SpiceSharp-Parser.sln`
- `docs/architecture/ADR-0007-compliant-planar-joints.md`
- `docs/verification/phase-06.md`

## Commands executed

Target frameworks:

- `SpiceSharpMechanical2D`: `netstandard2.0` and `net8.0`;
- tests and samples: `net8.0`.

Pinned package: `SpiceSharp` 3.2.3.

- `dotnet restore` for each new sample project
  - Result: PASS after the required network-enabled rerun.
  - The first sandboxed attempt could not reach NuGet repository-signature
    metadata. No dependency version changed.
- `dotnet format` on the Physics2D project, Physics2D test project, and both
  new sample projects with `--no-restore`
  - Result: PASS.
- `dotnet build src/SpiceSharpMechanical2D/SpiceSharpMechanical2D.csproj
  -c Release --no-restore --verbosity minimal`
  - Result: PASS for both target frameworks; 0 warnings, 0 errors.
- `dotnet test
  src/SpiceSharpMechanical2D.Tests/SpiceSharpMechanical2D.Tests.csproj
  -c Release --no-build --no-restore --filter
  FullyQualifiedName~SpiceSharpMechanical2D.Tests.Joints`
  - Result: PASS; 17 passed, 0 skipped, 0 failed.
- `dotnet test
  src/SpiceSharpMechanical2D.Tests/SpiceSharpMechanical2D.Tests.csproj
  -c Release --no-restore`
  - Result: PASS; 142 passed, 0 skipped, 0 failed.
- Release builds of both new sample projects with `--no-restore`
  - Result: PASS; 0 warnings, 0 errors.
- Release runs of both samples with `--no-build --no-restore`
  - Result: PASS; both emitted their declared CSV headers and finite samples
    through 3.5 seconds.
- `dotnet build src/SpiceSharp-Parser.sln -c Release --no-restore
  --verbosity quiet`
  - Result: PASS; 0 errors. The 259 warnings are the unchanged parser,
    code-analysis, and parser-test baseline. Focused Phase 06 builds have zero
    warnings.
- `dotnet test src/SpiceSharp-Parser.sln -c Release --no-build --no-restore`
  - First run: one unrelated parser integration operating-point case failed.
    It passed immediately in isolation.
  - Final complete rerun: PASS; 2,445 passed, 11 skipped, 0 failed.

## Test summary

- Phase 06 tests added: 17.
- Physics2D project: 142 passed, 0 skipped, 0 failed.
- Complete solution: 2,445 passed, 11 skipped, 0 failed.
- The performance-test assembly continues to contain no discoverable tests.

## Acceptance measurements

| Gate | Acceptance | Measured | Result |
| --- | --- | --- | --- |
| Revolute full Jacobian | scale-aware relative `<= 1e-5` | maximum absolute mismatch `2.7888171771905945e-9` | Pass |
| Weld full Jacobian | scale-aware relative `<= 1e-5` | maximum absolute mismatch `2.7888171771905945e-9` | Pass |
| Rotating prismatic full Jacobian | scale-aware relative `<= 1e-5` | maximum absolute mismatch `3.7002081398895825e-9` | Pass |
| Prismatic energy gradient | absolute/relative `<= 2e-7` | maximum absolute mismatch `3.081346733324608e-9` | Pass |
| Static action/reaction | roundoff | revolute `0`, weld `0`, prismatic `1.7763568394002505e-15` | Pass |
| Pendulum period | relative `<= 1.5e-2` | `4.3074492619435347e-4` | Pass |
| Pendulum anchor error | characteristic relative `<= 1e-3` | `1.6426576127620106e-4` | Pass |
| Slider-crank joint error | characteristic relative `<= 1e-3` | `9.302430282677036e-4` | Pass |
| Four-bar closure error | characteristic relative `<= 1e-3` | `4.526919423578098e-4` | Pass |
| Slider-crank work-energy balance | relative `<= 3e-2` | `1.1646423497003638e-4` | Pass |

## Jacobian verification

Each equation test perturbs all twelve entries in:

```text
[xA, yA, thetaA, vxA, vyA, omegaA,
 xB, yB, thetaB, vxB, vyB, omegaB]
```

Independent central differences use relative step `1e-6` and minimum step
`1e-7`. Comparison uses absolute tolerance `2e-7` and scale-aware relative
tolerance `1e-5`. The rotating prismatic case uses nonzero local anchors,
nonzero body angles, nonzero linear and angular velocities, normal
stiffness/damping, angular stiffness/damping, and optional axial
stiffness/damping. It therefore exercises the body-A guide rotation terms and
all cross-body columns.

## Transient verification

### Revolute pendulum

- Length `0.6 m`, mass `1.2 kg`, center inertia `0.12 kg*m^2`.
- Initial angle `0.08 rad`, gravity `9.81 m/s^2`.
- Pivot stiffness `2e5 N/m`, damping `120 N*s/m`.
- Expected small-angle period: `1.7565020270556762 s`.
- Measured period: `1.7572586313916805 s`.
- Maximum anchor error: `9.855945676572064e-5 m`.

### Prismatic and weld cases

- With the axial law disabled, the slider retained its configured `1.25 m/s`
  axial velocity and matched inertial travel within `2e-10` while normal and
  angle state remained at roundoff.
- A slider initialized with `0.04 m` normal error and `0.03 rad` angle error
  settled below `2e-5` in both coordinates while preserving axial motion.
- A world weld under constant force `(2,-1) N` and torque `0.3 N*m` converged
  to the expected compliant static deflections within the configured
  analytic comparison tolerances.
- Revolute oscillator final-position errors for timesteps `0.02`, `0.01`, and
  `0.005 s` were `4.9722594911205675e-5`,
  `1.2575951775284366e-5`, and `3.163552069973541e-6 m` respectively.

## Mechanism verification

### Slider-crank

- Crank length `0.2 m`, rod length `0.5 m`, payload force `-0.015 N`, drive
  torque `0.035 N*m`.
- Crank travel: `15.824380328713481 rad`, more than two revolutions.
- Slider velocity reversals: 5.
- Maximum joint error: `4.651215141338518e-4 m`.
- Relative work-energy-dissipation residual: `1.1646423497003638e-4`.
- Sample final row:
  `3.5,16.27438032871348,0.31918326865109,0.6834472435083273,5.033046347893506e-6,0.00014425651256481445`.

### Compliant four-bar

- Ground/crank/coupler/rocker lengths: `0.6/0.2/0.5/0.4 m`.
- Crank travel: `16.957811863412932 rad`, more than two revolutions.
- Maximum closure error: `2.263459711789049e-4 m`.
- Sample final row:
  `3.5,17.557811863412933,1.0865610079684702,2.4659739429527705,7.0253619751773945e-6`.

## Energy and diagnostic checks

- The prismatic elastic generalized loads match the negative numerical
  gradient of reported stored energy.
- Every joint reports nonnegative damping power for the exercised state.
- Mechanism energy and dissipation exports are read through generated
  real-property exports, thereby exercising accepted-timepoint diagnostic
  publication rather than internal equation results.
- The slider-crank work balance integrates drive power, payload power, and
  reported joint dissipation and compares them with the change in body kinetic
  plus joint elastic energy.

## Determinism and performance observations

All new tests and samples use fixed input data and no random source. The
17-case focused joint run completed in approximately 1.9 seconds on the
verification host. Both closed-loop mechanism tests completed in under one
second in the focused run. Formal performance baselines remain assigned to a
later phase.

## Known limitations

- These joints are compliant penalties. Error decreases with stiffness but is
  not projected to zero.
- Large stiffness-to-mass ratios can make the transient system stiff and may
  require a smaller maximum timestep.
- The smooth periodic angular potential has an unstable equilibrium at an
  antipodal orientation, as documented for Phase 5.
- Prismatic guide direction is fixed in endpoint A's selected frame; there is
  no independently animated guide frame.
- Axial limits, stops, friction, backlash, and unilateral response are absent.
- Bodies must precede referencing joints during behavior construction.
- The first repository-wide verification run encountered one unrelated
  parser integration operating-point failure. The same case passed in
  isolation and the complete rerun passed; no Physics2D test failed.

## Decision

PASS.

All Phase 06 equation, transient, refinement, mechanism, diagnostics, energy,
sample, focused-build, and repository-regression gates pass.

## Confirmation

No work from Phase 07 or a later phase was implemented.
