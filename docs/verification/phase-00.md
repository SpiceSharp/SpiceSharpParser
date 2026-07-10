# Phase 00 verification

## Scope implemented

- Added the `SpiceSharp.Physics2D` package project targeting `netstandard2.0`
  and `net8.0`.
- Added the non-mechanical `TransientApiProbe` entity.
- Allocated two private real solver variables and two derivative histories.
- Stamped the coupled harmonic proof equations through ordinary SpiceSharp
  biasing/time behaviors.
- Exposed both states through a behavior interface and `RealPropertyExport`.
- Resolved and cached an optional linked probe behavior during setup.
- Added eight automated Phase 0 proof tests.
- Documented the exact pinned SpiceSharp extension APIs in ADR-0001.

## Explicitly not implemented

- No mechanical coordinate, body, force, joint, contact, cam, coupling, export,
  or diagnostic type.
- No parser change.
- No custom simulation type.
- No custom timestepper, integrator, nonlinear solver, sparse solver, or global
  force accumulator.
- No work from Phase 1 or any later phase.

## SpiceSharp API used

- Package: `SpiceSharp` 3.2.3; assembly version `3.2.3.0`.
- Source identity:
  `34484fa6c66d636f6983626a0a770f75c0484553`.
- `Entity<T>.CreateBehaviors(ISimulation)`.
- `BehaviorContainer`, `BindingContext`, `ISimulation.UsesBehaviors<T>()`, and
  `ISimulation.EntityBehaviors`.
- `IBiasingBehavior` and `ITimeBehavior`.
- `IBiasingSimulationState.CreatePrivateVariable`, `Map`, and `Solver`.
- `IIntegrationMethod.CreateDerivative` and
  `IDerivative.GetContributions`.
- `ElementSet<double>` and `MatrixLocation`.
- `Reference.GetContainer` and `IBehaviorContainer.TryGetValue<T>()`.
- `GeneratedParametersAttribute`, `ParameterNameAttribute`, and
  `RealPropertyExport`.

See [ADR-0001](../architecture/ADR-0001-spicesharp-extension-points.md)
for source evidence and signature details.

## Files changed

- `src/SpiceSharp-Parser.sln`
- `src/SpiceSharp.Physics2D/SpiceSharp.Physics2D.csproj`
- `src/SpiceSharp.Physics2D/ApiProbe/TransientApiProbe.cs`
- `src/SpiceSharp.Physics2D/ApiProbe/TransientApiProbeParameters.cs`
- `src/SpiceSharp.Physics2D/ApiProbe/ITransientApiProbeBehavior.cs`
- `src/SpiceSharp.Physics2D/ApiProbe/TransientApiProbeBehavior.cs`
- `src/SpiceSharp.Physics2D.Tests/SpiceSharp.Physics2D.Tests.csproj`
- `src/SpiceSharp.Physics2D.Tests/ApiProof/TransientApiProbeTests.cs`
- `docs/architecture/ADR-0001-spicesharp-extension-points.md`
- `docs/verification/phase-00.md`

## Commands executed

Working-tree base: `ce88fd13a8346f47bb2dfbd5602c9203446ae2d7`.

- `dotnet restore SpiceSharp-Parser.sln`
  - Result: PASS; all solution projects restored.
- `dotnet format SpiceSharp-Parser.sln --verify-no-changes --no-restore
  --verbosity minimal`
  - Result: the repository-wide diagnostic reports pre-existing whitespace
    violations in untouched `SpiceSharpParser` and integration-test files.
    None of the reported paths is a Phase 0 file.
  - Repository convention: no formatter command exists in the three checked-in
    GitHub workflows, and there is no `.editorconfig`, formatter manifest, or
    formatting script. The plan's "repository does not use dotnet format"
    exception therefore applies; unrelated parser files were preserved.
- `dotnet format SpiceSharp.Physics2D/SpiceSharp.Physics2D.csproj
  --verify-no-changes --no-restore --verbosity minimal`
  - Result: PASS.
- `dotnet format SpiceSharp.Physics2D.Tests/SpiceSharp.Physics2D.Tests.csproj
  --verify-no-changes --no-restore --verbosity minimal`
  - Result: PASS.
- `dotnet build SpiceSharp-Parser.sln -c Release --no-restore --verbosity
  quiet`
  - Result: PASS; 0 warnings, 0 errors in the final build invocation.
- `dotnet test SpiceSharp-Parser.sln -c Release --no-build --logger
  "console;verbosity=minimal"`
  - Result: PASS; 2,311 passed, 11 skipped, 0 failed.
- `git -c core.whitespace=cr-at-eol diff --check`
  - Result: PASS. `cr-at-eol` matches the existing CRLF solution-file format.

## Test summary

- Tests added: 8.
- Direct Phase 0 test result: 8 passed, 0 failed, 0 skipped.
- Direct Phase 0 test wall time reported by VSTest: 0.8943 seconds.
- Complete solution result: 2,311 passed, 11 skipped, 0 failed.
- Existing test-project breakdown:
  - `SpiceSharpParser.AIExamples`: 948 passed.
  - `SpiceSharpParser.Tests`: 408 passed, 9 skipped.
  - `SpiceSharpParser.IntegrationTests`: 947 passed, 2 skipped.
  - `SpiceSharpParser.PerformanceTests`: no discoverable tests, unchanged.
- Phase 0 test-project breakdown: 8 passed.

## Numerical cases

### Requested operating-point state

- Parameters: `InitialA = -0.125`, `InitialB = 0.625`.
- Maximum timestep: 0.05 seconds.
- Integration method: SpiceSharp `Trapezoidal`.
- Expected result at operating point/time zero: `A = -0.125`, `B = 0.625`.
- Measured result: `A = -0.125`, `B = 0.625`.
- Absolute error: 0 for both states.
- Relative error: 0 for both states.
- h / h/2 / h/4 results: not applicable to the algebraic operating-point hold.

### Ten-period oscillator

- Parameters: `A(0) = 1`, `B(0) = 0`, stop time `20*pi` seconds.
- Maximum timestep: 0.05 seconds, set explicitly through
  `Trapezoidal.MaxStep`.
- Integration method: SpiceSharp variable-step `Trapezoidal` with
  `InitialStep = MaxStep = 0.05` seconds.
- Expected result: `A = cos(t)`, `B = -sin(t)`; all values finite through ten
  periods.
- Measured transient samples: 1265.
- Measured maximum radius error `|sqrt(A^2+B^2)-1|`:
  `2.4999993863961123e-7`.
- Measured ten-period endpoint vector error: `0.013071516388294678`.
- h / h/2 / h/4 endpoint vector errors:
  `0.20703923023238047` / `0.05219077097848883` /
  `0.013071516388294678` for h = 0.2 / 0.1 / 0.05 seconds.
- Observed error ratios: 3.967 and 3.993, consistent with second-order
  trajectory convergence.

The common `Transient(name, step, stop)` constructor was not used for this
study because inspection showed its `step` argument initializes
`Trapezoidal.InitialStep`, while `MaxStep` defaults to `stop/50`. The test sets
both values explicitly so the independent variable is genuinely the maximum
timestep.

## Jacobian verification

- Component: `TransientApiProbeBehavior`.
- State: arbitrary A and B because the equations are linear.
- Analytic matrix:
  `[Ja, -1; 1, Jb]`, where Ja and Jb are supplied by the two independent
  SpiceSharp derivative histories.
- Finite difference: not applicable; Phase 0 introduces no nonlinear equation.
- Maximum absolute mismatch: not applicable.
- Maximum relative mismatch: not applicable.

The analytic sign pattern is trajectory-tested against `cos(t)` and
`-sin(t)`. Independent finite-difference Jacobian infrastructure belongs to
Phase 1 and was not implemented early.

## Energy or power checks

For this normalized oscillator, `A^2 + B^2` is the conserved quadratic
invariant. Over ten periods at maximum timestep 0.05 seconds, the maximum
radius error was `2.4999993863961123e-7`. No physical energy or power quantity
is claimed for the API probe.

## Determinism check

Repeated direct Phase 0 runs completed with the same passing assertions. The
probe has no random input, load-time lookup, or mutable global state.

## Performance observations

The direct eight-test Phase 0 assembly completed in 0.8943 seconds according
to VSTest. This is a correctness probe, not a performance baseline; the formal
performance baseline belongs to Phase 14.

## Known limitations

- A pure zero-pin entity circuit does not satisfy SpiceSharp's electrical
  variable-presence validation. Tests keep validation enabled and add one
  isolated grounded resistor that is not coupled to the probe states.
- Linked probes are resolved during behavior construction, so the referenced
  entity must be earlier in construction order.
- `Units.Volt` is used only as solver bookkeeping for normalized proof states
  because the pinned public unit catalog has no dimensionless state unit.
- The endpoint phase error is intentionally visible at coarse timesteps; the
  refinement study demonstrates convergence rather than imposing an arbitrary
  trajectory tolerance on this infrastructure proof.

## Decision

PASS.

The repository does not define or use a formatter gate, and a solution-wide
`dotnet format` diagnostic is not baseline-clean. Both new projects pass
formatter verification, the complete Release build passes, and every
discoverable non-skipped test passes. No unrelated parser formatting was
changed.

## Confirmation

No work from a later phase was implemented.
