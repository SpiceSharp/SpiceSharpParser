# ADR-0005: Direct rigid-body loads and analytic point-force linearization

- Status: Accepted
- Date: 2026-07-10
- Phase: 4

## Context

Phase 3 established a planar rigid body whose velocity-variable rows own the
world x-force, world y-force, and center-of-mass torque equations. Phase 4
needs practical components that contribute gravity, applied force, point
force, applied torque, and linear or angular drag to those rows.

The components must preserve the boundary established by
[ADR-0003](ADR-0003-mechanical-coordinate-and-direct-force-stamping.md) and
[ADR-0004](ADR-0004-rigid-body-state-and-kinematics.md): SpiceSharp owns the
transient lifecycle, Newton iteration, integration histories, and additive
sparse assembly. A body owns only its kinematic and inertial equations. Every
connected load owns and stamps its own residual contribution; there is no
force-gathering pass or mutable body-level accumulator.

Most Phase 4 loads are linear or independent of solver state. `PointForce2D`
is different. Its application point is fixed in the body frame, and its force
may also be fixed in that frame. The world lever arm, world force, and torque
then depend on the body's unbounded angle. That state dependence must be
linearized analytically with signs consistent across all three body dynamics
rows.

Phase 4 also needs a limited time-dependent applied-force API. It must use the
current timepoint probed by SpiceSharp, remain deterministic when Newton
re-evaluates the same point, avoid expression-parser work, and allocate no
objects in the load loop.

## Decision

### Public load entities

Provide six ordinary zero-pin entities in `SpiceSharp.Physics2D.Forces`:

- [`Gravity2D`](../../src/SpiceSharp.Physics2D/Forces/Gravity2D.cs);
- [`AppliedForce2D`](../../src/SpiceSharp.Physics2D/Forces/AppliedForce2D.cs);
- [`PointForce2D`](../../src/SpiceSharp.Physics2D/Forces/PointForce2D.cs);
- [`AppliedTorque2D`](../../src/SpiceSharp.Physics2D/Forces/AppliedTorque2D.cs);
- [`LinearDrag2D`](../../src/SpiceSharp.Physics2D/Forces/LinearDrag2D.cs);
- [`AngularDrag2D`](../../src/SpiceSharp.Physics2D/Forces/AngularDrag2D.cs).

Each entity has an immutable referenced body name and mutable parameters that
callers configure before behavior construction. Each creates an
`IBiasingBehavior` only for simulations that request transient behaviors.

The shared internal `RigidBodyLoadBehavior` implements the common lifecycle.
It caches `ITimeSimulationState` during construction and calls the derived
load method only when `UseDc` is false. Consequently, loads do not alter the
six requested-state identity equations during the transient operating-point
stage.

### Setup-time body binding

Every load resolves its body once during `CreateBehaviors`:

```text
Reference(bodyName)
    -> GetContainer(simulation)
    -> GetValue<IRigidBody2DBehavior>()
```

The behavior maps the body variables through the active
`IBiasingSimulationState` and creates its own `ElementSet<double>`. No entity
name, container, behavior, or solver-location lookup occurs in `Load()`.

The body must precede referencing load entities in behavior-construction
order. A missing body or missing `IRigidBody2DBehavior` fails during setup,
before transient timepoints are evaluated.

### Residual and Newton stamp convention

For body state:

```text
z = [x, y, theta, vx, vy, omega]
```

and a component generalized-load vector:

```text
Q(z, t) = [Fx, Fy, tau]
```

the component contributes to the three dynamics residuals:

```text
Rload = -Q
```

At the current Newton iterate `zk`, the general direct-stamp convention is:

```text
matrix += -dQ/dz
rhs    += Q(zk, t) - (dQ/dz)*zk
```

Only columns on which a load actually depends are present in that component's
`ElementSet`. Constant loads therefore add RHS values only. Drag adds diagonal
velocity derivatives. Point force adds angle-column derivatives to the x,
y, and angular dynamics rows.

Each component owns its matrix locations, RHS locations, reusable value
array, validation, and analytic derivatives. Sparse addition is the only
load-superposition operation. Component ordering may change floating-point
addition at roundoff scale but cannot change the governing equations.

### Gravity

`Gravity2D` accepts a finite world acceleration vector `g` and contributes:

```text
F = M*g
tau = 0
```

where `M` is read from the referenced body behavior. The mass multiplication
belongs to gravity so bodies with different masses receive the same
acceleration while their force stamps differ physically. Gravity has no state
derivative in Phase 4 and stamps only the x/y dynamics RHS.

Gravity acts through the modeled center of mass. A gravity field with a
spatial gradient, buoyancy center, or distributed torque is outside this
component's semantics.

### Applied center force and transient function

`AppliedForce2D` acts through the center of mass and therefore contributes no
torque. It supports two construction modes:

```text
constant world force
ForceFunction2D(time) -> world force
```

[`ForceFunction2D`](../../src/SpiceSharp.Physics2D/Forces/ForceFunction2D.cs)
is a package-owned vector delegate. Its input is
`IIntegrationMethod.Time`, the timepoint currently being probed by
SpiceSharp. A non-null function takes precedence over the constant-force
parameter.

SpiceSharp may call component loads multiple times at the same timepoint while
Newton iterates, retry a timepoint, or reject a proposed step. A force function
must therefore:

- be deterministic for a given input time;
- be side-effect-free;
- derive no accepted-history state from call count;
- return finite double-precision components.

The behavior validates every returned vector and throws a named
`SpiceSharpException` containing the probed time if it is non-finite. It does
not catch exceptions thrown by user code or replace invalid values.

The delegate is deliberately time-only. It does not receive mutable solver
objects, a body behavior, or a simulation instance. State-dependent forces
belong in explicit behaviors with analytic Jacobians rather than opaque
callbacks.

### Point-force frames

`PointForce2D.LocalPoint` is always a body-local point measured from the center
of mass. [`ForceCoordinateSystem2D`](../../src/SpiceSharp.Physics2D/Forces/ForceCoordinateSystem2D.cs)
makes the force-vector frame explicit:

```text
World      force vector is fixed in the world frame
BodyLocal  force vector is fixed in the body frame and rotates with the body
```

No frame is inferred from vector values, names, or application point.

For angle `theta`, local application point `r`, configured force `f`, and the
counterclockwise rotation matrix `R(theta)`:

```text
rWorld = R(theta)*r
drWorld/dtheta = perpendicular(rWorld)
```

In world-force mode:

```text
FWorld = f
dFWorld/dtheta = 0
```

In body-local-force mode:

```text
FWorld = R(theta)*f
dFWorld/dtheta = perpendicular(FWorld)
```

The world torque about the center of mass and its derivative are:

```text
tau = cross(rWorld, FWorld)

dtau/dtheta = cross(drWorld/dtheta, FWorld)
             + cross(rWorld, dFWorld/dtheta)
```

For a body-local point and body-local force, both vectors rotate together, so
their cross product and torque are rotation-invariant. The two analytic
derivative terms cancel. For a world-fixed force, only the lever arm rotates,
so torque generally depends on angle.

### Point-force Newton linearization

The internal `PointForce2DEquation` evaluates one coherent contribution:

```text
FWorld
tau
dFWorld/dtheta
dtau/dtheta
```

The production behavior uses that same result for all three body rows. With
`Q` representing `Fx`, `Fy`, or `tau`, it stamps:

```text
matrix(row, angle) = -dQ/dtheta
rhs(row)           = Q - dQ/dtheta*theta
```

This is the ADR-0003 Newton convention applied to the body's angle column.
There is no dependence on body position, translational velocity, or angular
velocity for a constant configured Phase 4 point force, so no other columns
are stamped.

The evaluator is internal rather than public physics API. The test assembly
receives friend access solely to compare its analytic derivatives with the
independent Phase 1 central-finite-difference helper. Finite differences do
not execute in production.

### Applied torque

`AppliedTorque2D` accepts a finite scalar world torque. Positive torque is
counterclockwise under ADR-0004. It contributes only:

```text
rhs(omega row) += tau
```

It does not imply an application point, force couple geometry, or reaction
body. Those must be modeled explicitly by a later connection when needed.

### Linear drag

`LinearDrag2D` implements isotropic world-frame drag relative to a configured
finite medium velocity:

```text
F = -c*(v - vMedium)
```

with finite `c >= 0`. For each translational component:

```text
dF/dv = -c
matrix(v row, v column) = c
rhs(v row)              = c*vMedium
```

The current velocity cancels from the linearized RHS exactly. Damping zero is
valid and produces a no-op stamp. Negative damping is rejected rather than
being silently treated as an active actuator.

The model is linear and isotropic. It does not represent quadratic
aerodynamic drag, a body-local drag tensor, area, fluid density, or lift.

### Angular drag

`AngularDrag2D` implements:

```text
tau = -cOmega*(omega - omegaMedium)
```

with finite `cOmega >= 0` and finite medium angular velocity. Its exact linear
stamp is:

```text
matrix(omega row, omega column) = cOmega
rhs(omega row)                  = cOmega*omegaMedium
```

Like linear drag, zero damping is valid and negative damping is rejected.

### Validation and failure behavior

Behavior construction validates topology-independent parameters before
transient loading:

- constant acceleration, force, point, and torque values must be finite;
- force-coordinate enum values must be supported;
- damping must be finite and nonnegative;
- medium linear and angular velocities must be finite.

Body references resolve during setup. Time-function output is the one value
that cannot be validated completely at setup, so it is checked on every
evaluation. Exceptions include the load entity name and, for time functions,
the current timepoint.

The components do not catch SpiceSharp convergence exceptions, suppress NaN,
replace infinity, clamp parameters, or silently rescale SI values.

### Operating-point and transient lifecycle

All six components skip loading while `ITimeSimulationState.UseDc` is true.
This preserves `RigidBody2D`'s documented
`HoldSpecifiedStateDuringOperatingPoint` policy. Forces and torques begin
contributing only after the requested state has seeded SpiceSharp's transient
integration histories.

Phase 4 therefore does not solve static mechanical equilibrium. Introducing
an equilibrium initialization mode would require an explicit policy defining
which loads participate and how underconstrained mechanical systems are
handled.

### Runtime allocation and caching

During behavior construction, each component resolves its body, maps the
required variables, creates its `ElementSet<double>`, and allocates a reusable
value array. The transient load path performs arithmetic, reads solver state,
optionally invokes the already-stored force delegate, and mutates cached
values.

The load path performs no reflection, LINQ, dictionary or entity-name lookup,
per-load array/list allocation, per-load delegate creation, or conversion to
single precision. `Vector2D` values are double-precision value types.

### Diagnostics and sample boundary

Phase 4 does not expose current per-component force, torque, power, body net
force, body net torque, or acceleration exports. Direct independent stamps do
not provide an authoritative aggregation point, and load-loop values are not
guaranteed to represent an accepted timepoint. Stable diagnostics remain
assigned to the later export/diagnostic phase.

The FreeFall sample demonstrates only programmatic construction, ordinary
SpiceSharp `Transient`, `Gravity2D`, and body property exports. It emits
invariant CSV:

```text
time,x,y,vx,vy,angle,omega
```

The sample does not introduce a wrapper simulation, parser syntax, hidden
world object, or alternative stepping API.

## Rejected alternatives

- A global force/torque accumulator or body `AddForce` method: rejected
  because it would add shared mutable state, ordering rules, and a second load
  phase parallel to SpiceSharp's sparse additive assembly.
- Applying loads during the operating-point hold: rejected because it would
  silently turn exact requested-state initialization into a potentially
  singular static-equilibrium solve.
- Representing load connections with electrical nodes: rejected because body
  state is exposed through a typed behavior contract and private solver
  variables.
- Resolving body names or matrix locations during every load: rejected because
  topology is static and failures should occur during setup.
- A string expression for time-dependent force: rejected because Phase 4 does
  not modify or depend on SpiceSharpParser, and an expression language would
  broaden lifecycle and validation concerns.
- Separate scalar callbacks for x and y force: rejected in favor of one
  double-precision vector delegate evaluated once per load.
- Passing the simulation or mutable body behavior into a force callback:
  rejected because it would permit opaque state-dependent loads without an
  enforceable analytic Jacobian contract.
- Inferring whether a point force is world-fixed or body-local: rejected
  because the two laws have different values and angle derivatives; the frame
  must be explicit.
- Treating the application point as a fixed world point: rejected because the
  Phase 4 component specifically models force applied at a body-local point.
- Omitting the angle derivative for a world-fixed point force: rejected
  because the rotating lever arm makes torque angle-dependent.
- Rotating a body-local force without differentiating it: rejected because
  the translational force rows also depend on angle.
- Using finite differences inside `PointForce2D.Load()`: rejected because
  analytic derivatives are faster, deterministic, and free of step-size noise.
- Wrapping body angle before evaluating a point force: rejected because the
  authoritative angle is unbounded and sine/cosine already provide periodic
  geometry without a solver-state seam.
- Allowing negative drag coefficients: rejected because these components are
  damping models, not active propulsion or control elements.
- Quadratic drag, lift, distributed gravity, or state-dependent arbitrary
  callbacks: rejected as different public laws outside Phase 4.
- Publishing partial force or power diagnostics from the last Newton load:
  rejected because such values may be stale, rejected, or incomplete.
- Implementing Phase 5 spring-dampers while load infrastructure was open:
  rejected because one task implements one phase and two-body connections
  require a separate nonlinear contract and verification set.

## Consequences

- Practical external loads participate directly in an ordinary SpiceSharp
  transient without a custom mechanics stepping layer.
- Multiple components superimpose by sparse addition and are order-independent
  up to floating-point roundoff.
- Center forces and gravity affect translation only; applied torque affects
  rotation only; point force is the explicit translation/rotation coupling.
- Body-local force and application geometry remain attached to the body while
  world force remains fixed in the world frame.
- `PointForce2D` introduces the first public nonlinear mechanical component
  and establishes the analytic angle-column stamp used by later components.
- Time-dependent center force is convenient but intentionally constrained to
  a deterministic time-only delegate. Purity is documented and cannot be
  mechanically proven for arbitrary user code.
- Linear and angular drag converge through exact constant Jacobian terms and
  support moving media without a separate relative-state object.
- Loads must follow their referenced body in behavior-construction order.
- Requested initial states remain exact holds rather than static equilibria.
- Direct stamping continues to preclude trustworthy net-load diagnostics
  until a later diagnostic ownership model is designed.
- Extreme force, damping, mass, inertia, or mixed-domain scales remain visible
  to callers and may require smaller timesteps; the library does not silently
  rescale them.

## Verification

See [Phase 4 verification](../verification/phase-04.md). Twenty-one Phase 4
cases cover free fall, projectile motion, linear and angular drag, constant
and time-dependent force, applied torque, center and off-center point force,
world/local force frames, analytic point-force Jacobians, superposition,
component order, and invalid damping.

Fine-step relative errors are `6.93898214062451e-8` for free-fall position,
`3.450432986694902e-15` for free-fall velocity, and
`2.0136206006404327e-7` for linear-drag velocity. The worst point-force
scale-aware Jacobian mismatch is `1.9956858388070486e-10`, direct torque error
is zero, and reversing component order changes exported state by at most
`5.551115123125783e-17`. The FreeFall sample emits the required CSV, and the
complete repository suite passes with 2,405 tests passed, 11 skipped, and no
failures.
