# ADR-0002: Double-precision mathematics and smooth-function conventions

- Status: Accepted
- Date: 2026-07-10
- Phase: 1

## Context

Future `SpiceSharp.Physics2D` components will evaluate geometry, generalized
forces, and analytic Jacobians inside SpiceSharp's transient Newton iterations.
Those calculations need one authoritative numerical convention before any
mechanical entity is introduced.

The main design risks are:

- silently losing precision through a float-backed vector type;
- allowing approximate equality to leak into structural equality or hashing;
- producing NaN or infinity from avoidable intermediate overflow;
- choosing inconsistent cross-product, perpendicular, rotation, or angle-seam
  conventions;
- duplicating slightly different contact-smoothing formulas;
- normalizing a degenerate vector to an arbitrary direction;
- using finite differences in production nonlinear load loops.

Phase 1 therefore establishes the mathematics contract used by later phases
and the independent numerical test support used to verify their Jacobians.

## Decision

### Authoritative scalar and storage precision

All authoritative mathematics state uses `double`. [`Vector2D`](../../src/SpiceSharp.Physics2D/Mathematics/Vector2D.cs)
and [`Matrix2x2D`](../../src/SpiceSharp.Physics2D/Mathematics/Matrix2x2D.cs)
are immutable `readonly struct` values containing only double-precision
coordinates or matrix elements.

`System.Numerics.Vector2` is not used because it stores `float` components.
Conversions to a float-backed presentation type, if ever needed by a client,
must remain outside the authoritative physics model.

### Vector and matrix orientation

The coordinate system is right-handed in the planar sense:

```text
dot(a, b)   = ax*bx + ay*by
cross(a, b) = ax*by - ay*bx
perp(a)     = (-ay, ax)
```

`perp(a)` and positive rotations are counterclockwise. Rotation matrices use
column vectors:

```text
[ cos(theta)  -sin(theta) ] [ x ]
[ sin(theta)   cos(theta) ] [ y ]
```

`Matrix2x2D * Vector2D` follows that convention. Matrix elements are exposed
as `M11`, `M12`, `M21`, and `M22`; multiplication uses ordinary row-by-column
composition.

### Exact and approximate equality

`Equals`, `GetHashCode`, `==`, and `!=` use exact component equality so they
remain suitable for structural use and deterministic collections.

Approximate comparison is explicit through `ApproximatelyEquals`. Each
component uses the scale-aware condition:

```text
abs(actual - expected)
    <= absoluteTolerance
     + relativeTolerance * max(abs(actual), abs(expected))
```

Callers must choose both tolerances for the numerical scale of their test or
diagnostic. Approximate equality is never used implicitly by operators or
hashing.

### Length and normalization

Euclidean length and regularized length use scaled norm evaluation rather than
forming an unscaled sum of squares. This prevents avoidable intermediate
overflow for large finite components such as `(3e200, 4e200)`.

Normalization always takes an explicit nonnegative epsilon. A vector is
normalizable only when its finite length is strictly greater than epsilon:

- `Normalized(epsilon)` throws for a degenerate or non-finite length;
- `TryNormalize(epsilon, out value)` returns `false` and zero only for a finite
  degenerate length;
- neither method invents an arbitrary direction.

### Angle conventions

Authoritative body angles in later phases remain unbounded real values.
[`AngleMath`](../../src/SpiceSharp.Physics2D/Mathematics/AngleMath.cs) is only
for display and relative-angle calculations.

- `WrapSigned` returns the half-open interval `[-pi, pi)`. Both seam
  representations therefore map to `-pi`.
- `WrapPositive` returns `[0, 2*pi)`.
- `ShortestDifference(from, to)` returns `WrapSigned(to - from)`.
- Non-finite input propagates as NaN rather than being replaced by a plausible
  angle.

### Shared smooth functions

All later contact and friction components must reuse
[`SmoothFunctions`](../../src/SpiceSharp.Physics2D/Mathematics/SmoothFunctions.cs)
instead of defining local variants. Smoothing parameters must be finite and
strictly positive.

The smooth positive part and derivative are:

```text
positive(x, epsilon) = 0.5 * (x + sqrt(x^2 + epsilon^2))
dpositive/dx         = 0.5 * (1 + x/sqrt(x^2 + epsilon^2))
```

The implementation uses algebraically equivalent stable branches to avoid
cancellation for large negative x.

The smooth negative part is a signed approximation of `min(x, 0)`:

```text
negative(x, epsilon) = -positive(-x, epsilon)
dnegative/dx         = PositivePartDerivative(-x, epsilon)
```

The smooth absolute value, regularized vector length, and derivatives are:

```text
absolute(x, epsilon) = sqrt(x^2 + epsilon^2)
dabsolute/dx         = x / absolute(x, epsilon)

length(v, epsilon)   = sqrt(vx^2 + vy^2 + epsilon^2)
gradient(length)     = v / length(v, epsilon)
```

The regularized friction factor is dimensionless:

```text
friction(v, vs)      = tanh(v / vs)
dfriction/dv         = (1 - tanh(v / vs)^2) / vs
```

This factor is smooth and approaches the Coulomb bound asymptotically. It does
not represent exact static friction.

### Analytic derivatives and independent numerical checks

Production nonlinear components must use analytic derivatives. Central finite
differences are confined to the test project in
[`FiniteDifferenceJacobian`](../../src/SpiceSharp.Physics2D.Tests/Numerics/FiniteDifferenceJacobian.cs).

For each input column, the default test step is:

```text
max(1e-7, 1e-5 * max(1, abs(state)))
```

[`NumericAssert`](../../src/SpiceSharp.Physics2D.Tests/Numerics/NumericAssert.cs)
combines absolute and relative tolerances and reports maximum mismatches.
[`TimeSeriesComparison`](../../src/SpiceSharp.Physics2D.Tests/Numerics/TimeSeriesComparison.cs)
linearly interpolates an actual series at reference times and reports maximum
absolute and normalized RMS error. These helpers may allocate because they are
test-only; they must not be called from SpiceSharp load loops.

## Rejected alternatives

- `System.Numerics.Vector2`: rejected because its authoritative storage is
  single precision.
- Mutable vector or matrix classes: rejected because value semantics are
  clearer and avoid shared mutable geometry state.
- Approximate `==` or tolerance-based hashing: rejected because equality would
  cease to be transitive and collection behavior would become unreliable.
- Returning zero from every failed normalization: rejected because it hides a
  missing direction and can silently corrupt force or Jacobian signs.
- Naive `sqrt(x*x + y*y)`: rejected because finite large inputs can overflow
  before the square root.
- Wrapping authoritative angles after each step: rejected because it creates a
  discontinuity in solver state. Wrapping is limited to relative errors and
  presentation.
- Raw piecewise `Max`, `Min`, `Sign`, absolute value, or discontinuous friction
  as a physical law in later nonlinear equations: rejected because Newton
  iteration requires smooth residuals and analytic Jacobians.
- Finite differences in production load methods: rejected because they are
  slower, introduce step-size noise, and obscure residual signs.

## Consequences

- Later components share one precision, coordinate orientation, angle seam,
  smoothing definition, and derivative convention.
- Structural equality remains deterministic while numerical comparisons stay
  explicit and scale-aware.
- Degenerate geometry must be handled deliberately by each component rather
  than receiving an arbitrary normalized direction.
- Smooth contact has a documented nonzero smoothing tail, and regularized
  friction permits creep near zero slip.
- Every nonlinear component can be checked against a reusable, independent
  central-finite-difference implementation without shipping numerical
  differentiation in production code.
- The mathematics layer remains independent of SpiceSharp behaviors and
  entities, so it can be tested without a simulation.

## Verification

See [Phase 1 verification](../verification/phase-01.md). The measured maximum
scalar derivative mismatch is `2.330342585565859e-10`; the regularized-length
gradient mismatch is `1.0833534069831785e-10` absolute and
`8.280286425563326e-8` relative, all within the Phase 1 `1e-7` gate.
