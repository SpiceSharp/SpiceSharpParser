# ADR-0004: Planar rigid-body state, equation ownership, and kinematics

- Status: Accepted
- Date: 2026-07-10
- Phase: 3

## Context

`SpiceSharp.Physics2D` needs a planar rigid body that later force, spring,
joint, contact, and cam components can reference as one mechanical object. A
body has three generalized coordinates at its center of mass:

```text
x translation
y translation
planar rotation
```

Each coordinate follows the two-state formulation accepted in
[ADR-0003](ADR-0003-mechanical-coordinate-and-direct-force-stamping.md), so
the body requires six simultaneous solver unknowns. The public model must not
force callers to create three coordinate entities, expose six electrical
nodes, or coordinate a separate mechanics lifecycle.

The design also needs one authoritative convention for body-local geometry.
Later components will transform local anchors, calculate point velocities,
turn applied forces into torque, and differentiate those expressions during
Newton iteration. Ambiguity in rotation direction, frame ownership, lever-arm
origin, or angle wrapping would spread sign errors through every later phase.

The existing architectural boundary remains unchanged: SpiceSharp owns
transient stepping, Newton iteration, integration history, sparse equation
assembly, and solver variables. Physics2D contributes body equations and
connected components contribute force and torque residuals. There is no body
world step or global load accumulator.

## Decision

### One entity and one behavior

[`RigidBody2D`](../../src/SpiceSharp.Physics2D/Bodies/RigidBody2D.cs) is one
ordinary zero-pin `Entity<RigidBody2DParameters>`. It creates one
[`RigidBody2DBehavior`](../../src/SpiceSharp.Physics2D/Bodies/RigidBody2DBehavior.cs)
for an ordinary SpiceSharp `Transient`.

The body is mathematically composed of three generalized coordinates but does
not instantiate three nested `MechanicalCoordinate` entities or behavior
containers. The one behavior creates and owns all six private variables:

```text
position variables:  x, y, theta
velocity variables:  vx, vy, omega
```

This gives users and linked components one stable entity name, one parameter
set, one behavior contract, and one setup-order dependency. Each body creates
its own six variable instances, so multiple bodies have independent solver
state.

### State and residual equations

The body state is:

```text
z = [x, y, theta, vx, vy, omega]
```

`x` and `y` are the world coordinates of the center of mass. `theta` is an
unbounded counterclockwise angle in radians. `vx` and `vy` are world-frame
center-of-mass velocities, and `omega` is counterclockwise angular velocity.

For constant positive mass `M`, positive center-of-mass moment of inertia `I`,
net world force `(Fx, Fy)`, and net torque `tau` about the center of mass, the
assembled residual is:

```text
Rx      = xdot     - vx
Ry      = ydot     - vy
Rtheta  = thetadot - omega

Rvx     = M*vxdot    - Fx
Rvy     = M*vydot    - Fy
Romega  = I*omegadot - tau
```

The body behavior owns only the kinematic and inertial terms. The rows of
`VelocityXVariable`, `VelocityYVariable`, and `AngularVelocityVariable` are
the x-force, y-force, and torque dynamics rows. Connected components add their
own `-Fx`, `-Fy`, and `-tau` residuals and analytic derivatives directly to
those rows during their normal SpiceSharp load pass.

This is the three-coordinate form of ADR-0003's direct generalized-force
stamp. A constant load contributes only:

```text
rhs(vx row)    += Fx
rhs(vy row)    += Fy
rhs(omega row) += tau
```

No force or torque is first stored on the body. Sparse additive stamping is
the authoritative load-composition mechanism.

### Integration histories

The behavior owns six independent `IDerivative` histories. For each
generalized coordinate, one history stores position and one stores generalized
momentum:

```text
position histories:  x, y, theta
momentum histories:  M*vx, M*vy, I*omega
```

Before deriving each transient companion, the behavior updates the history
from the current solver iterate. Position histories request contributions with
coefficient `1`; momentum histories request contributions with `M` or `I` and
the corresponding velocity unknown. Velocity and angular velocity remain
solver unknowns; linear and angular momentum are integration-history values,
not additional unknowns.

The three coordinate pairs are independent in the inertial body stamp. Any
coupling between translation and rotation comes from a connected component,
such as an off-center point force, spring, joint, or contact. This keeps the
body's constant center-of-mass mass matrix diagonal and makes interaction
ownership explicit.

### Operating-point initialization

`RigidBody2D` reuses the single supported policy:

```text
HoldSpecifiedStateDuringOperatingPoint
```

While `ITimeSimulationState.UseDc` is true, the body stamps six identity
holds:

```text
x     = InitialPosition.X
y     = InitialPosition.Y
theta = InitialAngle
vx    = InitialLinearVelocity.X
vy    = InitialLinearVelocity.Y
omega = InitialAngularVelocity
```

Connected mechanical loads do not participate in this hold. After the
operating point is solved, `ITimeBehavior.InitializeStates()` seeds the three
position and three momentum histories from the solved body state.

This is a deterministic requested-state initialization, not a static force or
torque equilibrium. An equilibrium policy would require separate semantics
for connected-component participation and remains future work.

### Coordinate frames and rotations

The world frame is Cartesian and right-handed in the planar sense established
by [ADR-0002](ADR-0002-double-precision-mathematics.md). Positive angle and
positive torque are counterclockwise. For a body-local vector `rLocal`, the
world vector is:

```text
         [ cos(theta)  -sin(theta) ]
rWorld = [ sin(theta)   cos(theta) ] rLocal
```

The authoritative angle is never wrapped during integration. Periodic
presentation or relative-angle operations may wrap a copy, but must not
introduce a seam into solver state.

Body-local points are expressed relative to the center of mass. The public
geometry helpers on
[`IRigidBody2DBehavior`](../../src/SpiceSharp.Physics2D/Bodies/IRigidBody2DBehavior.cs)
use the current solver iterate and implement:

```text
LocalVectorToWorld(v) = R(theta) * v
WorldVectorToLocal(v) = transpose(R(theta)) * v

LocalPointToWorld(r)  = center + R(theta) * r
WorldPointToLocal(p)  = transpose(R(theta)) * (p - center)
```

Points include translation; free vectors do not. The inverse transformations
use rotation by `-theta`, which is the transpose of the rotation matrix.

### Point velocity and torque

For a body-local point `rLocal`, define the current world lever arm:

```text
rWorld = R(theta) * rLocal
```

The point's world velocity is:

```text
vPoint = [vx, vy] + omega * perpendicular(rWorld)
perpendicular([rx, ry]) = [-ry, rx]
```

For a world-frame force `F` applied at that point, torque about the center of
mass is:

```text
tau = cross(rWorld, F)
    = rWorld.x * F.y - rWorld.y * F.x
```

A force upward at a point to the body's right therefore produces positive
counterclockwise torque. These helpers calculate kinematics only; Phase 3 does
not publish a point-force entity or stamp an angle-dependent force residual.
When later components do so, they must include all analytic derivatives with
respect to body angle, translational velocity, and angular velocity and verify
them independently with central finite differences.

### Behavior contract and setup-time binding

`IRigidBody2DBehavior` exposes:

- the six `IVariable<double>` instances;
- scalar and vector views of the current solver state;
- mass and inertia;
- linear, angular, and total kinetic energy;
- the six frame, point-velocity, and torque helpers.

It does not expose solver indices, matrix locations, `ElementSet` instances,
mutable stamp arrays, derivative histories, or an imperative `AddForce`
method.

A connected entity resolves the body once during behavior construction with
`Reference.GetContainer(simulation)` and
`GetValue<IRigidBody2DBehavior>()`. It maps the exposed variables through the
active simulation state and caches its own matrix and RHS locations. No body
name or behavior lookup occurs in `Load()`.

The referenced body must therefore precede a connected component in behavior
construction order. Missing or mistyped references fail during setup rather
than during a transient load.

### Parameters, validation, and scaling

The public constructor groups initial translation in `Vector2D` values while
the parameter set stores scalar x/y components. This preserves the natural C#
API and keeps scalar parameter and export names explicit.

Behavior construction rejects:

- mass that is non-finite or not strictly positive;
- inertia that is non-finite or not strictly positive;
- any non-finite initial position, angle, linear velocity, or angular
  velocity;
- any unsupported initial-condition enum value.

Values use direct SI conventions and are not silently rescaled. `M` is mass
for both translational coordinates; `I` is the scalar planar moment of inertia
about the modeled center of mass. Phase 3 does not infer either quantity from
geometry and does not support a center-of-mass offset or a full spatial inertia
tensor.

The pinned SpiceSharp public unit catalog has no mechanical generalized-state
units. All six private variables use `Units.Volt` only as required solver
bookkeeping. This metadata does not give the states electrical semantics.

### Exports and diagnostics

Live scalar exports are behavior-derived:

- position x (`positionx`, `x`) and y (`positiony`, `y`);
- angle (`angle`, `theta`);
- velocity x (`velocityx`, `vx`) and y (`velocityy`, `vy`);
- angular velocity (`angularvelocity`, `omega`);
- mass (`mass`) and inertia (`inertia`);
- linear kinetic energy (`linearkineticenergy`, `linearenergy`);
- angular kinetic energy (`angularkineticenergy`, `angularenergy`);
- total kinetic energy (`kineticenergy`, `ke`).

The energy definitions are:

```text
linear kinetic energy  = 0.5 * M * (vx^2 + vy^2)
angular kinetic energy = 0.5 * I * omega^2
total kinetic energy   = linear + angular
```

Current state comes from solver variables. Entity initial values remain model
configuration and are not overwritten after a run.

Net force, net torque, and acceleration are intentionally not exported. With
direct independently owned stamps, the body has no authoritative load
accumulator, and a partial or stale diagnostic would be misleading.

### Load-path allocation policy

All six private variables, twelve matrix locations, six RHS locations, six
integration histories, one `ElementSet<double>`, and its reusable value array
are created during behavior construction. The load loop updates existing
arrays and derivative objects. It performs no reflection, LINQ, entity-name
lookup, dictionary construction, per-load delegate creation, or per-load
array/list allocation.

The body does not select an integration method or timestep. Tests use
SpiceSharp's variable-step trapezoidal method with explicit maximum timesteps
only to make numerical acceptance measurements reproducible.

## Rejected alternatives

- Requiring users to create three `MechanicalCoordinate` entities: rejected
  because a rigid body should have one identity, one parameter set, and one
  behavior contract for linked components.
- Representing the six states as shared circuit nodes: rejected because body
  state is private mechanical solver state, not electrical topology.
- Creating a body-specific simulation, solver, integrator, registry, or world
  step: rejected because ordinary SpiceSharp transient behaviors already own
  the required lifecycle.
- A body-level force and torque accumulator or `AddForce` callback: rejected
  because it would introduce mutable shared load state and a second assembly
  phase parallel to SpiceSharp's additive stamping.
- Using momentum rather than velocity as the public solver unknown: rejected
  because point kinematics, drag, joints, and contact naturally consume
  velocity; momentum remains internal integration history.
- Wrapping `theta` into a principal interval after each step: rejected because
  the seam would make authoritative solver state discontinuous and lose full
  revolution count.
- Storing authoritative linear velocity in body-local coordinates: rejected
  because world x/y inertia and most external loads are naturally expressed in
  the world frame; local values are derived by frame transforms.
- Adding shape, radius, mass-distribution inference, or center-of-mass offset
  to the base body: rejected because Phase 3 defines state and kinematics only,
  and later explicit interaction entities own geometry.
- Publishing constant force or torque entities in Phase 3: rejected because
  their public semantics belong to Phase 4; Phase 3 uses internal verification
  fixtures only.
- Exposing raw row indices, mutable solver storage, or derivative histories:
  rejected because those belong to a particular simulation and behavior.
- Reporting partially observed net force, net torque, or acceleration:
  rejected because direct stamps provide no authoritative aggregation point.
- Production finite-difference derivatives for later angle-dependent loads:
  rejected because nonlinear production components require analytic
  Jacobians; finite differences remain independent test support.

## Consequences

- Users create one body entity while SpiceSharp solves six private unknowns
  and advances six integration histories.
- Translation and rotation are independent in the base inertial stamp. Later
  connected components introduce physical coupling by stamping force, torque,
  and their analytic derivatives.
- All later body interactions share one current-iterate frame convention,
  point-velocity formula, torque sign, and unbounded angle state.
- Components can bind to one behavior and map only the variables their
  equations require, without receiving mutable body-owned stamp storage.
- Behavior-construction order is explicit: a body must precede entities that
  reference it.
- Mass and inertia are constant scalar parameters in Phase 3; variable mass,
  spatial inertia tensors, and inferred mass properties are unsupported.
- The body does not provide a net-load or acceleration diagnostic until a
  reliable ownership model exists.
- Mechanical states carry placeholder SpiceSharp unit metadata, and extreme
  or mixed-domain scales may require a future explicit conditioning policy.
- A circuit containing only zero-pin bodies still needs ordinary SpiceSharp
  electrical validation topology; tests use the isolated grounded resistor
  established by ADR-0001.

## Verification

See [Phase 3 verification](../verification/phase-03.md). Nineteen Phase 3 test
cases cover constant force, constant torque, combined loading, inertial motion,
frame round trips, point velocity, torque signs, kinetic energy, invalid
parameters, unbounded angle, independent body state, and deterministic repeat
runs. Fine-step relative errors are `9.69697179119645e-9` for position,
`3.638321237325814e-15` for linear velocity, `2.272756528430942e-9` for angle,
and `3.338892948131952e-14` for angular velocity. The maximum transform
round-trip error is `2.220446049250313e-16`. The complete repository suite
passes with 2,384 tests passed, 11 skipped, and no failures.
