# Phase 01 verification

## Scope implemented

- Added the public double-precision `Vector2D`, `Matrix2x2D`, `AngleMath`, and
  `SmoothFunctions` mathematics layer.
- Added internal reusable test support for central finite-difference
  Jacobians, scale-aware numeric assertions, and interpolated time-series
  comparisons.
- Added algebra, rotation, normalization, angle-wrapping, smoothing,
  derivative, overflow, type-safety, and numerical-support tests.
- Retained exact structural equality separately from approximate comparisons.
- Used stable scaled-length formulas to avoid intermediate overflow for large
  finite vector components.
- Approximate vector/matrix comparisons reject every non-finite component,
  including matching NaN and infinity values.
- The smooth positive/negative-part stable forms remain finite through
  magnitude `1e308`, including a comparably scaled smoothing parameter.

## Explicitly not implemented

- No new SpiceSharp entity or behavior.
- No mechanical coordinate, body, force, joint, contact, cam, or coupling.
- No parser change.
- No custom simulation, solver, or integration method.
- No work from Phase 2 or any later phase.

## SpiceSharp API used

No SpiceSharp API was introduced or consumed by the Phase 1 mathematics
implementation. The package project retains its Phase 0 SpiceSharp reference,
but every new file under `Mathematics` depends only on the .NET base class
library.

The numerical conventions are recorded in
[ADR-0002](../architecture/ADR-0002-double-precision-mathematics.md).

## Files changed

- `src/SpiceSharp.Physics2D/Mathematics/Vector2D.cs`
- `src/SpiceSharp.Physics2D/Mathematics/Matrix2x2D.cs`
- `src/SpiceSharp.Physics2D/Mathematics/AngleMath.cs`
- `src/SpiceSharp.Physics2D/Mathematics/SmoothFunctions.cs`
- `src/SpiceSharp.Physics2D.Tests/Mathematics/Vector2DTests.cs`
- `src/SpiceSharp.Physics2D.Tests/Mathematics/Matrix2x2DTests.cs`
- `src/SpiceSharp.Physics2D.Tests/Mathematics/AngleMathTests.cs`
- `src/SpiceSharp.Physics2D.Tests/Mathematics/SmoothFunctionsTests.cs`
- `src/SpiceSharp.Physics2D.Tests/Numerics/FiniteDifferenceJacobian.cs`
- `src/SpiceSharp.Physics2D.Tests/Numerics/NumericAssert.cs`
- `src/SpiceSharp.Physics2D.Tests/Numerics/TimeSeriesComparison.cs`
- `src/SpiceSharp.Physics2D.Tests/Numerics/NumericalTestSupportTests.cs`
- `docs/architecture/ADR-0002-double-precision-mathematics.md`
- `docs/verification/phase-01.md`

## Commands executed

Working-tree base: `d828c4f8693744bdc8c8e72417c4f2c086251f9b`.

- `dotnet restore SpiceSharp-Parser.sln`
  - Result: PASS; all projects were up-to-date.
- `dotnet format SpiceSharp-Parser.sln --verify-no-changes --no-restore
  --verbosity minimal`
  - Result: the unchanged repository-wide diagnostic still reports the
    pre-existing whitespace violations documented by Phase 0 in untouched
    parser and integration-test files.
  - The repository has no checked-in formatter command or formatter gate, so
    unrelated parser files were preserved as required.
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
  -c Release --no-restore --logger "console;verbosity=detailed"`
  - Result: PASS; 48 passed, 0 skipped, 0 failed.
- `dotnet build SpiceSharp-Parser.sln -c Release --no-restore --verbosity
  quiet`
  - Result: PASS; final invocation reported 0 warnings and 0 errors.
  - An earlier full recompilation reported the solution's 259 existing
    warnings, all in pre-existing parser, code-analysis, or parser-test files;
    the Phase 1 focused builds reported 0 warnings.
- `dotnet test SpiceSharp-Parser.sln -c Release --no-build --logger
  "console;verbosity=minimal"`
  - Result: PASS; 2,351 passed, 11 skipped, 0 failed.
- Isolated baseline retry:
  `dotnet test SpiceSharpParser.IntegrationTests/SpiceSharpParser.IntegrationTests.csproj
  -c Release --no-build --no-restore --filter
  "FullyQualifiedName=SpiceSharpParser.IntegrationTests.Examples.Example01Tests.When_Simulated_2_Expect_NoExceptions"`
  - Result: PASS; 1 passed. This was run because one complete parallel suite
    attempt transiently failed that unchanged operating-point test. The
    immediately preceding complete run and the subsequent complete retry both
    passed all 2,351 tests.

## Test summary

- Phase 1 test cases added: 40.
- Physics2D test project: 48 passed, including the 8 retained Phase 0 cases.
- Complete solution: 2,351 passed, 11 skipped, 0 failed.
- Test-project breakdown:
  - `SpiceSharp.Physics2D.Tests`: 48 passed.
  - `SpiceSharpParser.AIExamples`: 948 passed.
  - `SpiceSharpParser.Tests`: 408 passed, 9 skipped.
  - `SpiceSharpParser.IntegrationTests`: 947 passed, 2 skipped.
  - `SpiceSharpParser.PerformanceTests`: no discoverable tests, unchanged.

## Numerical cases

### Vector and matrix algebra

- Parameters: representative positive, negative, zero, and large-magnitude
  coordinates; rotation angles 0, 0.25, -2.75, and 31.125 radians.
- Expected result: vector identities, dot/perpendicular orthogonality,
  right-handed cross-product signs, orthogonal rotation matrices, and unit
  rotation determinant.
- Measured result: all exact algebraic identities passed; rotation length and
  orthogonality checks passed with absolute and relative tolerances `1e-14`.
- Large-magnitude case: length of `(3e200, 4e200)` measured as `5e200` within
  relative tolerance `1e-15`, without infinity or NaN.
- h / h/2 / h/4 results: not applicable; no time integration was introduced.

### Normalization and equality semantics

- Parameters: `(3, 4)`, zero, non-finite input, and epsilon `1e-12`.
- Expected result: unit vector `(0.6, 0.8)` for the regular case; explicit
  failure for degenerate or non-finite length.
- Measured result: unit length within `1e-15`; zero returns `false` from
  `TryNormalize` and throws from `Normalized`; non-finite length throws.
- Exact equality distinguishes a `1e-12` coordinate change while the explicit
  approximate helper accepts it at `1e-11` and rejects it at `1e-14`.

### Smooth functions near zero and away from smoothing

- Parameters: epsilon/smoothing speed `0.2`; scalar states from -2 through 2,
  including -1e-6, 0, and 1e-6; far-field magnitude `1000 * epsilon`.
- Expected at zero:
  - positive part `epsilon/2`, derivative 0.5;
  - negative part `-epsilon/2`, derivative 0.5;
  - smooth absolute value `epsilon`, derivative 0;
  - tanh friction factor 0, derivative `1/epsilon`;
  - regularized vector length `epsilon`, gradient zero.
- Measured result: all zero-state values matched exactly.
- Far-field result: positive/negative/absolute functions approached their hard
  limits within the analytic smoothing-tail bound; tanh approached +/-1.
- Stability result: inputs through `1e200` and vector `(3e200, 4e200)` remained
  finite.

### Numerical test support

- Finite-difference polynomial state: `(0.75, -1.25)`.
- Step policy: `max(1e-7, 1e-5 * max(1, |state|))` per column.
- Maximum absolute Jacobian mismatch:
  `1.237787650154587e-11`.
- Maximum relative Jacobian mismatch:
  `8.251917667697247e-12`.
- Time-series interpolation: a linear two-channel reference sampled at
  0, 0.5, and 1 was compared with endpoints only; maximum absolute and
  normalized RMS errors were exactly 0.
- Known time-series offset: expected and measured normalized RMS error 0.25.

## Jacobian verification

### Scalar smooth functions

- Components: positive part, negative part, smooth absolute value, and tanh
  friction factor.
- States: -2, -0.2, -1e-6, 0, 1e-6, 0.2, and 2.
- Central-difference step: `1e-6`.
- Maximum absolute derivative mismatch:
  `2.330342585565859e-10`.
- Scale-aware assertion: absolute tolerance `1e-9`, relative tolerance `1e-7`.

### Regularized vector length

- States: `(0, 0)`, `(1e-6, -2e-6)`, `(0.2, -0.3)`, and `(3, 4)`.
- Epsilon: `0.2`.
- Analytic gradient: `vector / sqrt(x^2 + y^2 + epsilon^2)`.
- Central finite difference: independent two-column Jacobian using the test
  support step policy.
- Maximum absolute mismatch: `1.0833534069831785e-10`.
- Maximum relative mismatch: `8.280286425563326e-8`.

All absolute and relative derivative mismatches are below the Phase 1
acceptance ceiling of `1e-7`. The relative maximum occurs at a near-zero
gradient component; its absolute mismatch remains approximately `1e-10`.

## Energy or power checks

Not applicable. Phase 1 adds mathematics and test support only, with no
physical state or time evolution.

## Determinism check

Repeated focused runs produced the same reported derivative mismatch values
and all 48 tests passed. All test inputs are fixed and contain no random state.

## Performance observations

The final detailed focused run completed 48 tests in 0.8988 seconds according
to VSTest. This is not the formal performance baseline assigned to Phase 14.

## Known limitations

- Approximate vector and matrix comparison is coordinate-wise and requires the
  caller to choose explicit absolute and relative tolerances.
- Normalization deliberately rejects lengths at or below epsilon rather than
  silently selecting a direction.
- `WrapSigned` uses the half-open interval `[-pi, pi)`, so both seam
  representations map to `-pi`.
- Smooth positive/negative parts have a nonzero smoothing tail by design.
- Tanh friction is a dimensionless regularization factor, not a static-friction
  model.
- Finite-difference and time-series helpers are test-project internals and are
  not used in production load loops.
- One unchanged parser integration test showed a single operating-point
  convergence flake during a parallel solution run. It passed in isolation and
  in the complete retry; no Phase 1 code participates in that parser circuit.

## Decision

### Post-review corrective verification

Four boundary cases were added after the Phase 00-05 implementation review:
three non-finite vector comparison cases and one matrix comparison case. The
large-magnitude smoothing case was strengthened from `1e200` to `1e308` and
now also covers `x` and `epsilon` at the same scale. All cases pass in Release;
the complete Physics2D project now contains 125 passing tests.

PASS.

The production mathematics and test-support layers meet the Phase 1 scope,
both target frameworks build without warnings, and measured derivative
mismatches satisfy the `1e-7` acceptance gate.

## Confirmation

No work from a later phase was implemented.
