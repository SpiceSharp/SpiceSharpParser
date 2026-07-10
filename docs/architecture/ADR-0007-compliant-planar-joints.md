# ADR-0007: Compliant planar joints, rotating guides, and accepted diagnostics

- Status: Accepted
- Date: 2026-07-10
- Phase: 6

## Context

Phase 5 established direct, fully coupled spring-damper stamps between rigid
bodies. Phase 6 needs the familiar revolute, weld, and prismatic abstractions
needed to assemble mechanisms. These joints must remain ordinary SpiceSharp
entities and participate in its Newton iteration and transient integration.

This phase deliberately does not introduce exact algebraic constraints,
Lagrange multipliers, projection, Baumgarte stabilization, or a second
mechanics solver. Joint errors are physical compliance deflections. Their
magnitude is controlled by configured stiffness, damping, loads, inertia, and
timestep rather than being forced to machine zero.

The prismatic joint is the difficult case. A guide attached to endpoint A
rotates with body A. Its normal error, velocity error, generalized reactions,
and Jacobian therefore depend on both translations, both rotations, both
linear velocities, and both angular velocities. Treating the guide as fixed
during differentiation produces incorrect body-A torque and incomplete Newton
coupling.

Diagnostics introduce a second lifecycle concern. Values observed during a
load call may belong to an intermediate or rejected Newton iterate. Public
errors, reactions, energy, and power must describe the last accepted
timepoint.

## Decision

### Public joint entities

Add three zero-pin entities in `SpiceSharpMechanical2D.Joints`:

- `RevoluteJoint2D`;
- `WeldJoint2D`;
- `PrismaticJoint2D`.

Each endpoint is an existing `MechanicalAnchor2D`, so its frame is explicit:
a body endpoint contains a body name and body-local point, while a world
endpoint contains a fixed world point. At least one endpoint must be a body.
A joint may not connect a body to itself.

All reference quantities are constructor inputs. In particular, weld and
prismatic reference angles are never captured silently from initial body
state. The prismatic guide axis is normalized robustly at construction and is
interpreted in endpoint A's frame: world coordinates when A is world, and
body-A local coordinates otherwise.

### Common state, load, and stamp contract

The equation state and generalized-load order is:

```text
z = [xA, yA, thetaA, vxA, vyA, omegaA,
     xB, yB, thetaB, vxB, vyB, omegaB]

Q = [FAx, FAy, tauA, FBx, FBy, tauB]
```

World endpoint rows and columns are omitted from the sparse stamp. Body
locations are resolved and mapped once during behavior construction. At a
Newton iterate `zk`, the component contributes:

```text
matrix += -dQ/dz
rhs    += Q(zk) - (dQ/dz)*zk
```

Loading is skipped during the requested-state operating point, consistently
with the body, force, and Phase 5 connection lifecycle.

### Revolute joint

For anchor positions and point velocities:

```text
e  = pB - pA
ed = vB - vA
FA = kp*e + cp*ed
FB = -FA

tauA = cross(rA, FA)
tauB = cross(rB, FB)
```

The stored energy and nonnegative dissipated power are:

```text
U = 0.5*kp*dot(e, e)
Pd = cp*dot(ed, ed)
```

This is an isotropic compliant pivot. Rotation remains free; the anchor
coincidence error is not an exact constraint.

### Weld joint

The weld combines the revolute law with a smooth periodic angular law:

```text
eThetaRaw = thetaB - thetaA - thetaReference
eOmega = omegaB - omegaA
tauAngular = kTheta*sin(eThetaRaw) + cTheta*eOmega
```

`tauAngular` is added to A and subtracted from B. The diagnostic angle is
wrapped only for presentation. The solver uses the unbounded angle states and
the smooth sine/cosine law established by ADR-0006. Its angular potential is
`kTheta*(1-cos(eThetaRaw))`.

### Prismatic joint

Let `aLocal` be the normalized configured guide. For a world endpoint A,
`a = aLocal`. For a body endpoint A:

```text
a = R(thetaA)*aLocal
n = perpendicular(a)
d = pB - pA

en = dot(n, d)
s  = dot(a, d)
```

The exact guide-relative velocity measures are:

```text
enDot = dot(n, vBPoint-vAPoint) - omegaA*dot(a, d)
sDot  = dot(a, vBPoint-vAPoint) + omegaA*dot(n, d)
```

Normal and optional axial efforts are:

```text
lambdaN = kn*en + cn*enDot
lambdaS = ks*(s-sReference) + cs*sDot
```

The default axial stiffness and damping are zero, leaving translation along
the guide free. A nonzero axial law turns that degree of freedom into a
configured spring-damper.

Generalized loads are computed as negative transposed constraint gradients,
not merely as a force pair with moment arms:

```text
Qguide = -Jen^T*lambdaN - Js^T*lambdaS
```

For the angular columns:

```text
d(en)/d(thetaA) = -dot(a,d) - dot(n,perpendicular(rA))
d(en)/d(thetaB) =  dot(n,perpendicular(rB))

d(s)/d(thetaA)  =  dot(n,d) - dot(a,perpendicular(rA))
d(s)/d(thetaB)  =  dot(a,perpendicular(rB))
```

These body-A terms include guide rotation. The same smooth angular law used
by the weld fixes relative orientation. Stored energy and dissipated power
are the sums of normal, axial, and angular contributions.

### Complete analytic derivatives

The production equations evaluate values and their exact first derivatives
with an internal fixed-width `Dual12` value type. It contains twelve scalar
derivative fields; it uses no derivative arrays, reflection, finite
differences, or heap allocation in the load path. Ordinary product and chain
rules through rotation, point velocity, efforts, and generalized reactions
produce the complete `6 x 12` Jacobian from the same expressions as the load.

This is forward analytic differentiation, not numerical differentiation.
Independent central finite differences remain test-only and verify every
state column, including the rotating prismatic guide.

### Accepted-timepoint diagnostics

Each joint exposes a typed behavior interface:

- `IRevoluteJoint2DBehavior`;
- `IWeldJoint2DBehavior`;
- `IPrismaticJoint2DBehavior`.

Diagnostics include applicable position and velocity errors, forces on both
endpoints, torques on both endpoints, stored elastic energy, and nonnegative
dissipated power. Equation evaluation writes trial diagnostics. An
`IAcceptBehavior` copies them to the public snapshot only when SpiceSharp
accepts the timepoint. Generated real-property exports read that accepted
snapshot.

World reactions remain visible in the typed diagnostics even though the
world endpoint has no solver row. Forces on A and B are exact opposites;
torques are the generalized endpoint reactions including moment-arm, guide,
and angular-law contributions.

### Validation and preload warnings

Construction rejects invalid/default endpoints, world-to-world and same-body
topology, a nonfinite or zero guide axis, nonfinite references, and nonfinite
or negative stiffness/damping. Missing bodies fail during setup with both the
joint and body name in the exception.

Large initial position/angle errors or estimated reactions emit a trace
warning. They are not silently corrected because an intentional preload is a
valid compliant model.

## Consequences

- Closed-loop mechanisms can be assembled entirely from ordinary entities
  and SpiceSharp sparse stamps.
- Joint error is finite and load-dependent. Increasing stiffness improves
  geometric fidelity but also increases system stiffness and may require a
  smaller timestep.
- A rotating prismatic guide has full body-A translation/rotation/velocity
  coupling and preserves the intended moving frame.
- Public diagnostics are stable accepted-timepoint observations rather than
  Newton-iteration probes.
- The fixed-width derivative representation is more verbose than hand-coded
  partial derivatives but keeps load and Jacobian expressions coherent and
  allocation-free.
- Exact constraint drift elimination, singular-configuration rank handling,
  hard stops, friction, contact, and joint limits remain outside Phase 6.

## Rejected alternatives

- Exact constraint multipliers: rejected for this phase because they require
  new algebraic unknowns, rank policy, stabilization, and verification.
- Projection after each step: rejected because it bypasses SpiceSharp's
  residual and acceptance lifecycle and obscures energy transfer.
- Capturing reference angles from the initial state: rejected because entity
  behavior would then depend silently on setup state and entity ordering.
- A world-fixed prismatic axis for every topology: rejected because it cannot
  represent a guide attached to a moving body.
- Moment-arm torque alone for a rotating guide: rejected because it omits the
  guide-orientation component of `-J^T*lambda`.
- Finite differences in production: rejected because they add evaluations,
  step-size policy, noise, and allocation pressure to every Newton load.
- Publishing trial diagnostics directly from `Load()`: rejected because
  rejected iterates are not physical accepted timepoints.
