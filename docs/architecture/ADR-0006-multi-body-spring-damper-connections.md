# ADR-0006: Multi-body spring-damper connections and analytic coupled stamping

- Status: Accepted
- Date: 2026-07-10
- Phase: 5

## Context

Phase 3 established a planar rigid body with six private solver variables and
three dynamics rows. Phase 4 established direct external loads that bind to a
single body and own their additive force, torque, and Newton stamps. Phase 5
must extend that model to internal compliant connections that transfer force
or torque between two rigid bodies, while also allowing one endpoint to be
fixed in the world.

The connection work must preserve the boundaries established by
[ADR-0003](ADR-0003-mechanical-coordinate-and-direct-force-stamping.md),
[ADR-0004](ADR-0004-rigid-body-state-and-kinematics.md), and
[ADR-0005](ADR-0005-direct-rigid-body-loads-and-point-force-linearization.md):

- SpiceSharp owns time integration, Newton iteration, timestep acceptance,
  and sparse additive assembly;
- each rigid body owns its state and inertial equations;
- each connected component owns its load law, topology, residual
  contribution, and analytic derivatives;
- body state is accessed through `IRigidBody2DBehavior`, not electrical pins,
  reflection, or a mutable global mechanics registry;
- requested initial body state is held during the operating point, and
  mechanical loads begin during transient loading;
- no per-load entity lookup, matrix-location lookup, finite differencing, or
  allocation is allowed.

Unlike a one-body point force, a two-body distance connection is coupled to
as many as twelve state variables. A body-local anchor changes world position
with body translation and rotation, and its point velocity contains both
linear and angular motion. Consequently, the axial force, its direction, and
both center-of-mass torques depend on the states of both bodies. Omitting any
of those cross-body or angle derivatives produces an incomplete Newton
linearization.

Distance direction is undefined for exactly coincident anchors under the
ordinary Euclidean norm. Coincident or nearly coincident anchors are useful,
including the zero-rest-length compliant pivot in the Phase 5 sample. The
constitutive law therefore needs an explicit smooth regularization with
documented physical consequences rather than an arbitrary direction branch.

Relative rotation presents a separate periodicity problem. Body angles remain
unbounded solver states under ADR-0004, but a rotational spring should use the
shortest relative angular error. The chosen wrap has a branch seam at `+/-pi`;
the component must define where its analytic derivative is valid rather than
pretending the seam is globally differentiable.

Phase 5 implements compliant force transfer only. Exact distance constraints,
revolute joints, Lagrange multipliers, unilateral contact, friction, and
constraint stabilization belong to later phases.

## Decision

### Public connection entities

Provide two ordinary zero-pin entities in `SpiceSharp.Physics2D.Connections`:

- [`DistanceSpringDamper2D`](../../src/SpiceSharp.Physics2D/Connections/DistanceSpringDamper2D.cs);
- [`RotationalSpringDamper2D`](../../src/SpiceSharp.Physics2D/Connections/RotationalSpringDamper2D.cs).

Both entities have immutable endpoints and mutable, validated physical
parameters. They create `IBiasingBehavior` instances only for simulations
that request transient behaviors. At least one endpoint must reference a
rigid body; a world-to-world connection is rejected because it has no solver
row to affect.

The entities transfer loads directly between body dynamics rows. They do not
create a hidden body, electrical node, reaction-force variable, constraint
multiplier, or separate mechanics solve.

### Explicit endpoint value types and frames

[`MechanicalAnchor2D`](../../src/SpiceSharp.Physics2D/Connections/MechanicalAnchor2D.cs)
represents one distance-connection endpoint. It has two explicit construction
modes:

```text
MechanicalAnchor2D.Body(bodyName, localPoint)
MechanicalAnchor2D.World(worldPoint)
```

For a body anchor, `Point` is measured in that body's local frame from its
center of mass. For a world anchor, `Point` is an immutable point in world
coordinates. The frame is never inferred from a name, a zero vector, or the
other endpoint.

[`RotationalEndpoint2D`](../../src/SpiceSharp.Physics2D/Connections/RotationalEndpoint2D.cs)
similarly provides:

```text
RotationalEndpoint2D.Body(bodyName)
RotationalEndpoint2D.World(fixedAngle)
```

A body endpoint follows that body's unbounded angle and angular velocity. A
world endpoint has a fixed world-frame angle and zero angular velocity.

Separate endpoint types are intentional. A translational anchor owns a point
and a rotational endpoint owns an angle; combining them into one union would
permit meaningless combinations and obscure the frame contract.

Endpoint coordinates, fixed angles, and names are validated when endpoint
values are created and again when the connection is constructed. The second
validation rejects invalid default struct values rather than allowing a
missing body name to fail later in a load iteration.

### Setup-time binding and fixed topology

Each non-world endpoint resolves its body once during `CreateBehaviors`:

```text
Reference(bodyName)
    -> GetContainer(simulation)
    -> GetValue<IRigidBody2DBehavior>()
```

The body entities must therefore precede their referencing connection in
behavior-construction order. A missing entity or missing
`IRigidBody2DBehavior` fails during setup.

During behavior construction, the connection maps all active body variables
and dynamics rows through `IBiasingSimulationState`. It then creates one
precomputed `ElementSet<double>` containing the dense coupling block required
by that connection. A world endpoint contributes constants to the equation
but creates no solver row or column.

The distance connection uses the full two-body state ordering:

```text
z = [xA, yA, thetaA, vxA, vyA, omegaA,
     xB, yB, thetaB, vxB, vyB, omegaB]
```

and generalized-load ordering:

```text
Q = [FAx, FAy, tauA, FBx, FBy, tauB]
```

The rotational connection uses:

```text
zTheta = [thetaA, omegaA, thetaB, omegaB]
QTheta = [tauA, tauB]
```

Rows and columns belonging to world endpoints are omitted from the sparse
stamp. The equation evaluators retain fixed orderings so one implementation
can serve body-to-body and body-to-world topology without runtime branching
over solver locations.

### Body-anchor kinematics

For a body with center position `x`, angle `theta`, center velocity `v`, and
angular velocity `omega`, and a local anchor `rLocal`, define:

```text
r = R(theta) * rLocal
p = x + r
vPoint = v + omega * perpendicular(r)
```

`R(theta)` and `perpendicular(r) = (-r.y, r.x)` use the counterclockwise,
right-handed conventions from ADR-0002 and ADR-0004. A world anchor instead
has:

```text
p = configured world point
vPoint = 0
r = 0
```

The zero radius for a world endpoint is an equation-evaluator convention. It
does not model a world center of mass and creates no world torque row.

For one scalar component of a body state, the endpoint differentials are:

| State | `dp` | `dvPoint` | `dr` |
| --- | --- | --- | --- |
| `x` | `(1, 0)` | `(0, 0)` | `(0, 0)` |
| `y` | `(0, 1)` | `(0, 0)` | `(0, 0)` |
| `theta` | `perpendicular(r)` | `-omega*r` | `perpendicular(r)` |
| `vx` | `(0, 0)` | `(1, 0)` | `(0, 0)` |
| `vy` | `(0, 0)` | `(0, 1)` | `(0, 0)` |
| `omega` | `(0, 0)` | `perpendicular(r)` | `(0, 0)` |

The angle derivative of point velocity includes `-omega*r` because
`d(perpendicular(r))/dtheta = -r`. This derivative is required even though
the spring direction law is expressed only with anchor positions and point
velocities.

### Distance spring-damper law

For endpoint positions `pA`, `pB`, point velocities `vA`, `vB`, configured
rest length `L0`, stiffness `k`, axial damping `c`, and positive length
regularization `epsilonL`, evaluate:

```text
d = pB - pA
L = sqrt(dot(d, d) + epsilonL^2)
n = d / L

vr = vB - vA
vn = dot(n, vr)

f = k*(L - L0) + c*vn
FA = f*n
FB = -FA
```

`FA` is the world force applied at anchor A. Positive extension therefore
pulls A toward B and applies the opposite force to B. Positive relative speed
along `n` increases the tensile force, so damping opposes separation on both
endpoints.

For body endpoints, the center-of-mass torques are:

```text
tauA = cross(rA, FA)
tauB = cross(rB, FB)
```

World endpoint loads remain present in the internal six-load vector for a
uniform action/reaction equation, but their rows are not stamped.

`RestLength`, `Stiffness`, and `Damping` must be finite and nonnegative.
`LengthRegularization` must be finite and strictly positive. Zero stiffness
or damping is valid and disables that part of the constitutive law. Negative
values are rejected because these entities model passive springs and dampers,
not active actuators.

### Distance regularization semantics

The regularized length is part of the public physical model:

```text
L = sqrt(dot(d, d) + epsilonL^2)
```

It is not merely a comparison tolerance. It ensures:

- `L` is strictly positive;
- `n = d/L` is smooth and finite at coincidence;
- no arbitrary direction must be chosen when `d = 0`;
- all implemented force and Jacobian terms remain finite for finite inputs.

At exact coincidence:

```text
L = epsilonL
n = 0
FA = FB = 0
```

The scalar `f` may be nonzero there, but the vector force is zero because the
regularized direction is zero. The local force derivative remains finite and
defines how the connection begins responding as the anchors separate.

Regularization intentionally changes behavior when physical separation is
comparable to `epsilonL`. For a nonzero rest length, the zero-static-force
separation away from coincidence satisfies approximately:

```text
physicalLength = sqrt(L0^2 - epsilonL^2)
```

when `L0 >= epsilonL`. For `L0 = 0`, the elastic force simplifies to `k*d`,
but axial damping still uses the regularized direction. Callers must choose
`epsilonL` small relative to the physical length scale while keeping the
resulting near-coincident Jacobian appropriate for the problem.

The library does not silently derive `epsilonL` from body dimensions,
stiffness, timestep, or machine epsilon, and does not switch to an
unregularized branch away from coincidence.

### Analytic distance Jacobian

For any scalar state variable `q`, let endpoint kinematic differentials be
`dpA`, `dvA`, `drA`, `dpB`, `dvB`, and `drB`. The analytic chain is:

```text
dd = dpB - dpA
dL = dot(n, dd)
dn = (dd - n*dL) / L

dvr = dvB - dvA
dvn = dot(dn, vr) + dot(n, dvr)

df = k*dL + c*dvn
dFA = n*df + f*dn
dFB = -dFA
```

The torque derivatives include both moment-arm and force changes:

```text
dtauA = cross(drA, FA) + cross(rA, dFA)
dtauB = cross(drB, FB) + cross(rB, dFB)
```

The internal `DistanceSpringDamper2DEquation` evaluates all six loads and the
complete `6 x 12` Jacobian from one coherent set of kinematics. This prevents
the translation rows, torque rows, and cross-body columns from drifting into
different approximations.

Finite differences are used only by the friend test assembly to verify the
analytic implementation. Production loading does not select a perturbation
size or execute the equation more than once per load call.

### Internal force and torque invariants

The connection constructs `FB` by exact negation of `FA`, so its net internal
linear force is zero up to floating-point representation:

```text
FA + FB = 0
```

For two body endpoints, the total torque about the world origin is:

```text
cross(xA, FA) + tauA + cross(xB, FB) + tauB
  = cross(pA, FA) + cross(pB, FB)
  = cross(pA - pB, FA)
  = cross(-d, f*d/L)
  = 0
```

Thus the axial internal connection introduces no net world-origin torque.
This proof depends on applying equal/opposite collinear forces at the actual
anchor points and on including each body's moment-arm torque. A world endpoint
represents an external support; its reaction is computed by the equation but
is not a dynamic world-body row.

### Rotational spring-damper law

For endpoint angles `thetaA`, `thetaB`, angular velocities `omegaA`, `omegaB`,
reference relative angle `theta0`, rotational stiffness `kTheta`, and damping
`cTheta`, evaluate:

```text
eTheta = AngleMath.WrapSigned(thetaB - thetaA - theta0)
eOmega = omegaB - omegaA

tauA = kTheta*eTheta + cTheta*eOmega
tauB = -tauA
```

`WrapSigned` returns the half-open interval `[-pi, pi)`. If B is positively
ahead of A, A receives positive torque and B receives negative torque, so the
pair opposes the relative error. If A is world and B is a body, only `tauB` is
stamped and is restoring. If B is world and A is a body, only `tauA` is
stamped and is likewise restoring.

`ReferenceAngle` must be finite. Rotational stiffness and damping must be
finite and nonnegative. A world endpoint's fixed angle must be finite and its
angular velocity is exactly zero.

Away from the wrap seam, the complete analytic Jacobian is constant:

```text
             thetaA  omegaA  thetaB  omegaB
d(tauA)/dz = [ -k,     -c,      k,      c ]
d(tauB)/dz = [  k,      c,     -k,     -c ]
```

### Wrapped-angle seam

The body angles themselves remain unbounded and are never rewritten or
wrapped in solver storage. Only relative error is wrapped inside the
rotational constitutive law.

The selected shortest-error function is continuous across equivalent angle
representations such as one endpoint just below `+pi` and the other just above
`-pi`. It nevertheless has an unavoidable branch discontinuity at the exact
relative error represented by `+/-pi`. At that point the restoring direction
is ambiguous and a global derivative does not exist.

The component uses derivative one for the wrapped error within each branch.
Therefore:

- Newton iterates must not rely on a derivative exactly at the branch seam;
- a timestep that crosses the seam may require ordinary SpiceSharp timestep
  reduction or additional Newton work;
- tests exercise equivalent representations near the seam without claiming
  differentiability at the exact boundary;
- Phase 5 does not smooth the seam, add hysteresis, or preserve a separate
  winding-number state.

### Residual and Newton stamp

The body dynamics residual convention remains:

```text
Rconnection = -Q(z)
```

At current Newton state `zk`, for the active connection rows and columns:

```text
J = dQ/dz

matrix += -J
rhs    += Q(zk) - J*zk
```

For a body-to-body distance connection, this is a `6 x 12` dense local block.
For a body-to-world distance connection, it is a `3 x 6` block. A rotational
connection uses `2 x 4` or `1 x 2`, respectively. Dense here describes the
component-local coupling; the values are inserted into SpiceSharp's global
sparse solver through the precomputed `ElementSet<double>`.

Only active body state is included in `J*zk`. Fixed world positions and angles
are equation constants, not solver variables, so they affect `Q(zk)` but do
not create columns or appear in the linearized-state product.

Every body dynamics row receives the negative load derivative and the
corresponding linearized RHS. The connection never modifies body kinematic
rows or integration histories.

### Operating-point and transient lifecycle

Both connection behaviors cache `ITimeSimulationState` during construction
and skip loading while `UseDc` is true. This preserves the exact requested
body states during the transient operating point, consistent with ADR-0004
and ADR-0005. Spring and damper loads begin after the initial state has seeded
SpiceSharp's integration histories.

Phase 5 therefore does not find a static connected-system equilibrium before
transient integration. An initially extended spring may have nonzero force at
the first transient timepoint. Callers that require equilibrium must provide
consistent initial states or wait for a later explicit equilibrium policy.

### Runtime allocation and evaluator boundary

Behavior construction allocates and caches:

- resolved body behavior references;
- active load and state index arrays;
- mapped matrix and RHS locations;
- one `ElementSet<double>`;
- reusable state, load, Jacobian, and element-value storage;
- the transient simulation-state reference.

The transient load path captures current body scalar values, evaluates the
analytic equation, fills cached matrix/RHS values, and adds the element set.
It performs no reflection, LINQ, entity-name lookup, behavior lookup,
matrix-location lookup, list or array allocation, finite differencing, or
single-precision conversion.

The equation evaluators and their result structs are internal. Their purpose
is to keep one authoritative value-and-derivative implementation and to allow
the friend test assembly to perform independent finite-difference checks.
They are not a second public simulation API and do not expose mutable solver
storage.

### Diagnostics and sample boundary

Phase 5 does not add public exports for current connection force, extension,
damping power, stored energy, or world reaction. As with direct loads in
ADR-0005, values observed during `Load()` can belong to an intermediate or
rejected Newton iteration and are not automatically accepted-timepoint
diagnostics.

The [`Pendulum`](../../samples/SpiceSharp.Physics2D.Samples/Pendulum/Program.cs)
sample demonstrates a compliant pivot by connecting a body-local point to a
fixed world point with a stiff, damped, zero-rest-length
`DistanceSpringDamper2D`. Gravity is an ordinary Phase 4 load. The sample uses
ordinary SpiceSharp `Transient` and body property exports and emits invariant
CSV:

```text
time,x,y,angle,vx,vy,omega,pivot_error
```

`pivot_error` is expected to be nonzero because the sample is compliant. The
sample is not evidence of an exact revolute joint and does not introduce a
custom integrator or constraint projection.

## Rejected alternatives

- An exact distance constraint or revolute joint in Phase 5: rejected because
  it requires algebraic constraint variables, rank handling, stabilization,
  and a separate verification contract assigned to later phases.
- A hidden dynamic or infinite-mass world body: rejected because fixed world
  endpoint constants need no state, inertial equation, or solver row.
- Electrical pins for body endpoints: rejected because connection topology is
  typed through body behavior references and must not expose mechanical state
  as electrical node voltage.
- A body-level force accumulator or `AddForce` callback: rejected because
  sparse additive assembly already provides superposition without shared
  mutable ordering state.
- Separate body-to-body and body-to-world entity classes: rejected because
  explicit endpoint value types express both topologies with one equation and
  one sign convention.
- One generic endpoint union for position and rotation: rejected because it
  would permit meaningless point/angle combinations and obscure frame units.
- Treating every configured anchor point as world-fixed: rejected because a
  body connection must rotate its local point and include angular point
  velocity.
- Inferring local versus world frame from endpoint order or vector value:
  rejected because frames affect both values and derivatives and must be
  explicit in the public API.
- Using the ordinary norm `sqrt(dot(d, d))`: rejected because its direction
  and derivatives are undefined at coincident anchors.
- Choosing a fixed fallback direction when anchors coincide: rejected because
  it breaks rotational symmetry and introduces a discontinuous branch.
- Switching between regularized and unregularized formulas at a threshold:
  rejected because the switch would add another derivative seam and make the
  constitutive law harder to reproduce.
- Treating `epsilonL` as an invisible solver tolerance: rejected because it
  changes force and equilibrium behavior near its scale and must remain a
  visible, validated model parameter.
- Applying component-wise x/y damping: rejected because a distance damper is
  axial and uses only relative point velocity projected onto the connection
  direction.
- Omitting angular velocity from anchor-point velocity: rejected because an
  off-center rotating anchor has real translational velocity and damping force.
- Omitting the angle derivative `-omega*r` from point velocity: rejected
  because it leaves the damping Jacobian incomplete for rotating anchors.
- Stamping only each body's self-derivatives: rejected because the internal
  force depends on relative two-body state and requires cross-body columns for
  a complete Newton step.
- Computing torques from center separation rather than anchor moment arms:
  rejected because it would violate the actual point of force application and
  the zero net world-origin torque invariant.
- Calculating A and B forces independently: rejected because exact negation
  provides the clearest action/reaction invariant and avoids roundoff drift
  between two evaluations.
- Finite differences in production: rejected because analytic derivatives are
  deterministic, allocation-free, and avoid perturbation noise in a strongly
  coupled nonlinear stamp.
- Leaving rotational error unwrapped: rejected because equivalent physical
  orientations separated by `2*pi` would produce different torque and the
  spring could restore along the long path.
- Wrapping the body solver angles themselves: rejected because ADR-0004 makes
  angle unbounded and other components depend on continuous solver history.
- Replacing wrapped linear angular error with `sin(eTheta)`: rejected because
  it changes the intended linear torsional stiffness law and has zero slope at
  the antipodal orientation rather than making the ambiguity explicit.
- Claiming the wrap seam is differentiable: rejected because shortest signed
  angle necessarily changes branch at `+/-pi` without additional state.
- Adding seam hysteresis or a winding-number history in Phase 5: rejected
  because it introduces path-dependent state and lifecycle decisions beyond a
  memoryless spring-damper.
- Applying connections during the operating-point hold: rejected because it
  would replace exact requested-state initialization with a potentially
  singular static-equilibrium solve.
- Allowing negative spring or damping coefficients: rejected because these
  public entities model passive compliance, not active control or propulsion.
- Publishing last-Newton connection diagnostics: rejected because those
  values may be stale, rejected, or not associated with an accepted timepoint.
- Implementing Phase 6 exact constraints while the connection stamp was open:
  rejected because each task implements one phase and Phase 6 has different
  algebraic topology and acceptance tests.

## Consequences

- Two rigid bodies can exchange nonlinear internal force and torque directly
  inside an ordinary SpiceSharp transient.
- Either endpoint can instead be fixed in the world without creating a hidden
  body or changing the constitutive equation.
- Endpoint factories make body-local versus world-frame interpretation
  explicit at call sites.
- Equal/opposite axial forces preserve net internal linear force, and correct
  moment-arm torques preserve net world-origin torque for body-to-body
  connections.
- Off-center distance connections couple translation and rotation through both
  force values and a complete analytic `6 x 12` Jacobian.
- Axial damping uses full point velocity, so body rotation can create damping
  force and torque even when centers are instantaneously stationary.
- Regularized length makes coincident anchors finite and deterministic, but
  deliberately alters the law near `epsilonL`. Poor scale choices can create
  materially different compliance or a difficult near-coincident Jacobian.
- Rotational springs use shortest signed relative error while body state stays
  unbounded. The exact antipodal seam remains a documented nonsmooth point.
- Large stiffness or damping relative to mass, inertia, and timestep can make
  a transient numerically stiff. The library exposes those scales and relies
  on ordinary SpiceSharp timestep control rather than silently weakening the
  connection.
- World endpoint reactions are implicit in the equal/opposite equation but are
  not yet public accepted-timepoint diagnostics.
- Connections, like Phase 4 loads, must follow referenced bodies in behavior
  construction order and do not participate in the requested-state operating
  point.
- The internal evaluator boundary gives tests direct value/Jacobian access
  without expanding public solver-state API.
- The compliant Pendulum sample demonstrates composition of body, connection,
  and gravity, but does not guarantee exact anchor coincidence.
- Exact constraints, contacts, and constraint-force diagnostics remain open
  architectural work for later phases.

## Verification

See [Phase 5 verification](../verification/phase-05.md). Fourteen Phase 5
tests cover action/reaction, zero net internal linear force, zero net
world-origin torque, body-to-world attachment, off-center torque, a pure
rotational spring, wrapped-angle representation near the seam, full analytic
distance and rotational Jacobians, coincident anchors, reduced-mass frequency,
damped analytic motion, pure rotational frequency, and timestep refinement.

The measured force action/reaction error and world-origin torque residual are
both zero. The maximum scale-aware Jacobian mismatches are
`7.503406784792332e-10` for the distance connection and
`2.105423613230073e-10` for the rotational connection, against a `5e-6`
limit. The reduced-mass oscillator frequency relative error is
`2.2222331169160068e-6`, against a `3e-3` limit. The pure rotational
oscillator frequency relative error is `1.3334268135212213e-6`.

The damped world spring final-displacement absolute error is
`2.6532872210993652e-6 m`. Timestep-refinement endpoint errors decrease from
`4.9722594911205675e-5 m` to `1.2575951775284366e-5 m` and then
`3.163552069973541e-6 m`. Coincident-anchor load and Jacobian values are all
finite.

The Pendulum sample completes through `3 s` with finite CSV output and a final
compliant anchor error of `0.004321287349603146 m`. The focused Physics2D suite
passes 116 tests with zero warnings. The complete repository suite passes with
2,419 tests passed, 11 skipped, and no failures; its remaining parser and test
warnings are the pre-existing repository baseline.
