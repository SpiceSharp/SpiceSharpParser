# SpiceSharpMechanical2D
## Phase-by-phase Codex implementation plan

**Purpose:** Implement a useful, verified 2D mechanical-physics library using the existing SpiceSharp `Transient` simulation and custom SpiceSharp entities/behaviors only.

**Target package name:** `SpiceSharpMechanical2D`

**Core restriction:** Do not implement a custom simulation type, custom time integrator, custom global solver, impulse solver, collision world, or game engine.

The library will represent mechanical state as SpiceSharp solver variables and will stamp smooth mechanical differential equations into the existing transient Newton/MNA system.

---

# 1. Product goal

Create a small but credible 2D mechanics library that supports:

- translational and rotational generalized coordinates;
- planar rigid bodies;
- gravity;
- applied force and torque;
- linear and angular damping;
- springs and dampers between body-local anchor points;
- compliant revolute, prismatic, and weld joints;
- explicitly declared smooth circle/plane and circle/circle contacts;
- regularized friction;
- custom cam lift laws;
- roller-follower contact against a smooth cam profile;
- mechanical exports and diagnostics;
- optional electrical-to-mechanical coupling in later phases.

The intended use cases are:

- mass–spring–damper systems;
- pendulums;
- double pendulums with compliant joints;
- slider-crank mechanisms;
- soft four-bar mechanisms;
- spring-loaded followers;
- custom cam profiles;
- explicit soft impacts;
- small electromechanical mechanisms.

The library is **not** intended to support:

- rigid impulse contact;
- exact complementarity constraints;
- automatic collision-pair discovery;
- broad-phase collision detection;
- persistent contact manifolds;
- exact Coulomb static friction;
- large stacks or piles of rigid objects;
- continuous collision detection;
- game-engine scenes;
- dynamically created solver variables during transient execution.

---

# 2. Non-negotiable architecture rules

Codex must follow all of these rules in every phase.

## 2.1 Use ordinary SpiceSharp `Transient`

All mechanics must execute through the existing SpiceSharp transient simulation.

Do not create:

```text
MechanicalTransient2D
PhysicsSimulation
CustomTransient
MechanicalSolver
```

Do not subclass or replace the built-in transient solver.

## 2.2 Use custom SpiceSharp entities and behaviors

Persistent mechanical model objects are SpiceSharp entities:

```text
MechanicalCoordinate
RigidBody2D
Gravity2D
PointForce2D
AppliedTorque2D
DistanceSpringDamper2D
RevoluteJoint2D
PrismaticJoint2D
WeldJoint2D
CircleGroundContact2D
CircleCircleContact2D
CamFollowerContact2D
```

Each entity creates the behavior interfaces required by the existing simulation, such as the exact supported forms of:

```text
IBiasingBehavior
ITimeBehavior
IConvergenceBehavior
```

Codex must verify the repository's actual public API before using any interface or helper.

## 2.3 Mechanics is stamped as equations

For one generalized coordinate, use two unknowns:

```text
q = generalized position
u = generalized velocity
```

The governing equations are:

\[
\dot q-u=0
\]

\[
M\dot u-Q(q,u,t)=0
\]

where:

- \(M\) is generalized mass or rotational inertia;
- \(Q\) is the total generalized force.

For a planar rigid body, create three coordinate pairs:

```text
x, vx
y, vy
angle, omega
```

The body stamps only inertia and kinematic relations. Force, joint, and contact components stamp their contributions directly into the corresponding body equations.

## 2.4 Do not use a global force accumulator

Do not build a separate world step that first gathers all forces.

Each behavior must stamp its own force and Jacobian contributions into the SpiceSharp matrix and RHS during the normal SpiceSharp load pass.

## 2.5 Dynamic topology is forbidden

All body-to-body interactions must be declared before simulation starts.

A contact is an explicit entity such as:

```csharp
new CircleGroundContact2D(...);
new CircleCircleContact2D(...);
```

Do not search for contacts automatically.

## 2.6 Smooth equations only

All nonlinear mechanics equations must be continuously differentiable enough for Newton iteration.

Avoid raw:

```text
Math.Max
Math.Min
Math.Sign
if penetration > 0
absolute value with a hard cusp
discontinuous friction
instantaneous collision impulses
```

Use documented smooth approximations with analytic derivatives.

## 2.7 Double precision throughout

Use a custom double-precision `Vector2D`.

Do not use `System.Numerics.Vector2` as the authoritative physics type because it stores `float`.

## 2.8 Analytic Jacobians are required

Every nonlinear component must provide its Jacobian.

Every nonlinear component must also have an automated central-finite-difference test comparing the analytic Jacobian against an independent numerical approximation.

Do not ship finite differences in the production load loop.

## 2.9 No parser work before the physics API is stable

The first release is programmatic C# only.

Do not modify SpiceSharpParser during these phases.

## 2.10 One Codex task equals one phase

Codex must:

1. implement only the requested phase;
2. add all required tests;
3. run the complete test suite;
4. write `docs/verification/phase-XX.md`;
5. summarize measured results;
6. stop.

Codex must not begin the next phase.

---

# 3. Proposed repository structure

Codex may adapt paths to the target repository, but should preserve this separation:

```text
src/
  SpiceSharpMechanical2D/
    Mathematics/
      Vector2D.cs
      Matrix2x2D.cs
      SmoothFunctions.cs
      AngleMath.cs

    Core/
      MechanicalCoordinate.cs
      MechanicalCoordinateParameters.cs
      IMechanicalCoordinateBehavior.cs
      MechanicalInitialConditionMode.cs

    Bodies/
      RigidBody2D.cs
      RigidBody2DParameters.cs
      IRigidBody2DBehavior.cs
      BodyLocalPoint2D.cs

    Forces/
      Gravity2D.cs
      AppliedForce2D.cs
      PointForce2D.cs
      AppliedTorque2D.cs
      LinearDrag2D.cs
      AngularDrag2D.cs

    Connections/
      DistanceSpringDamper2D.cs
      RotationalSpringDamper2D.cs

    Joints/
      RevoluteJoint2D.cs
      PrismaticJoint2D.cs
      WeldJoint2D.cs

    Contact/
      ContactMaterial2D.cs
      CircleGroundContact2D.cs
      CircleCircleContact2D.cs
      SmoothContactLaw.cs
      RegularizedFrictionLaw.cs

    Cams/
      ICamLiftLaw.cs
      PeriodicCubicSpline1D.cs
      CamLiftContact2D.cs
      IParametricCurve2D.cs
      PeriodicSplineCurve2D.cs
      RollerCamContact2D.cs

    Coupling/
      DcMotorCoupler2D.cs

    Exports/
      MechanicalCoordinateExports.cs
      RigidBody2DExports.cs
      ContactExports.cs

    Diagnostics/
      EnergyDiagnostics2D.cs
      ResidualDiagnostics2D.cs

    Internal/
      NonlinearStamp.cs
      BehaviorLookup.cs
      SolverVariablePair.cs

tests/
  SpiceSharpMechanical2D.Tests/
    ApiProof/
    Mathematics/
    Coordinates/
    Bodies/
    Forces/
    Connections/
    Joints/
    Contact/
    Cams/
    Coupling/
    Integration/
    Jacobians/
    Regression/

samples/
  SpiceSharpMechanical2D.Samples/
    FreeFall/
    Oscillator/
    Pendulum/
    SliderCrank/
    BouncingCircle/
    CamFollower/
    DcMotorMechanism/

docs/
  architecture/
  equations/
  verification/
```

---

# 4. General implementation conventions

## 4.1 Units

Use SI units:

```text
position        metre
velocity        metre/second
angle           radian
angular speed   radian/second
mass            kilogram
inertia         kilogram metre squared
force           newton
torque          newton metre
stiffness       newton/metre
damping         newton second/metre
time            second
```

Do not encode units in variable names such as `PositionMm`.

## 4.2 Angles

Store body angle as an unbounded real value in radians.

Do not wrap the authoritative angle state after every step.

Only wrap relative-angle errors where the physical component requires a shortest-angle difference.

## 4.3 Initial conditions

The first implementation must support:

```csharp
InitialPosition
InitialVelocity
```

For the transient operating-point stage, the default initialization policy must lock the generalized coordinate to its requested initial position and velocity.

Do not attempt a general mechanical static-equilibrium solver in the first release.

Potential API:

```csharp
public enum MechanicalInitialConditionMode
{
    HoldSpecifiedStateDuringOperatingPoint
}
```

Do not add another enum member until a tested behavior exists.

## 4.4 Scaling

The first implementation uses direct SI values.

Add diagnostics for badly scaled values, but do not silently rescale user values.

Examples of warnings:

```text
mass <= 0
inertia <= 0
contact stiffness produces an estimated natural period below 10 times the maximum timestep
length regularization is too large relative to rest length
friction smoothing speed is nonpositive
```

## 4.5 Runtime allocation

After behavior construction:

- no reflection in load loops;
- no dictionary lookup by entity name in load loops;
- no LINQ in load loops;
- no per-load arrays or lists;
- no per-load delegates;
- no hidden conversion to `float`.

Resolve linked body behaviors during behavior construction and cache all solver locations.

## 4.6 Failure behavior

Throw descriptive exceptions during setup for invalid topology or parameters.

During simulation:

- do not hide NaN;
- do not replace infinity with zero;
- do not catch convergence exceptions unless adding useful context and rethrowing;
- include entity names in diagnostic exceptions.

---

# 5. Numerical formulation

## 5.1 Generalized coordinate

Unknown vector:

\[
z=
\begin{bmatrix}
q\\
u
\end{bmatrix}
\]

Residual:

\[
R_q=\dot q-u
\]

\[
R_u=M\dot u-Q(q,u,t)
\]

The coordinate behavior owns and stamps:

- derivative state for \(q\);
- derivative state for \(u\), or momentum \(Mu\);
- the `-u` term in the kinematic equation;
- the inertial contribution \(M\dot u\);
- operating-point initial-condition equations.

Force components add \(-Q\) to the dynamics residual and the corresponding derivatives.

## 5.2 Newton stamping

For a nonlinear residual \(R(z)=0\), stamp:

\[
J(z_k)z_{k+1}=J(z_k)z_k-R(z_k)
\]

Codex must implement one tested internal helper or a documented explicit pattern for nonlinear mechanical stamping.

The helper must not conceal signs. Its tests must include a scalar nonlinear residual with a known Newton step.

## 5.3 Body-local anchor point

For local anchor \(r_l=(r_x,r_y)\) and body angle \(\theta\):

\[
r_w=
\begin{bmatrix}
\cos\theta&-\sin\theta\\
\sin\theta&\cos\theta
\end{bmatrix}r_l
\]

World anchor:

\[
p=p_c+r_w
\]

Point velocity:

\[
v_p=
\begin{bmatrix}v_x\\v_y\end{bmatrix}
+
\omega
\begin{bmatrix}-r_{w,y}\\r_{w,x}\end{bmatrix}
\]

Torque from a world force:

\[
\tau=r_{w,x}F_y-r_{w,y}F_x
\]

All derivatives with respect to angle, translational velocity, and angular velocity must be tested.

## 5.4 Smooth positive part

Provide a shared function:

\[
\operatorname{positive}_\epsilon(x)=
\frac12\left(x+\sqrt{x^2+\epsilon^2}\right)
\]

Also provide its first derivative.

Do not duplicate slightly different smoothing formulas across contact components.

## 5.5 Smooth friction

Initial regularized friction:

\[
F_t=-\mu F_n\tanh(v_t/v_s)
\]

where \(v_s>0\) is the smoothing speed.

This model is viscous-like near zero velocity and Coulomb-like away from zero. It is not exact static friction; document that clearly.

---

# 6. Global quality gates

Every phase must pass:

```bash
dotnet restore
dotnet format --verify-no-changes
dotnet build -c Release --no-restore
dotnet test -c Release --no-build
```

If the repository does not use `dotnet format`, Codex must document the existing formatting command rather than inventing one.

Every phase report must include:

```text
Commit or working-tree identifier
Target framework(s)
SpiceSharp package/project version
Build command and result
Test command and result
Number of tests added
Number of tests passed
Numerical tolerances
Measured maximum errors
Known limitations
Files added or modified
Confirmation that later phases were not implemented
```

Do not weaken a numerical tolerance merely to make a test pass.

A tolerance change requires:

1. measured results before and after;
2. an explanation of the numerical source;
3. a convergence study when time integration is involved;
4. an update to the verification report.

---

# 7. Phase 0 — Repository baseline and exact SpiceSharp API proof

## Goal

Prove the exact public SpiceSharp APIs required to implement a two-state transient custom component.

Do not implement physics.

## Required investigation

Codex must inspect the repository or pinned package source for current examples of:

- capacitor transient behavior;
- inductor transient behavior;
- a component that creates a private solver variable;
- a component that links to another entity's behavior;
- nonlinear Jacobian/RHS stamping;
- parameter/property exports;
- initial conditions in transient analysis;
- behavior registration attributes or builders.

Document exact class and method names in:

```text
docs/architecture/ADR-0001-spicesharp-extension-points.md
```

## Implementation

Create a temporary but production-quality `TransientApiProbe` component that:

- has no mechanical terminology;
- allocates two private real solver variables;
- creates and initializes two integration derivative states;
- stamps two coupled linear differential equations;
- exposes both state values as properties;
- can be linked to a second probe behavior by entity name;
- runs under the existing `Transient` class;
- uses only public or intentionally supported APIs.

Suggested proof equations:

\[
\dot a=b
\]

\[
\dot b=-a
\]

with \(a(0)=1\), \(b(0)=0\).

This is a harmonic oscillator used only to verify infrastructure.

## Required tests

1. Behavior is created for `Transient`.
2. Both solver variables exist and are independently queryable.
3. Initial conditions are applied.
4. At \(t=0\), exported values match the requested state.
5. Oscillator remains finite for at least ten periods.
6. Reducing maximum timestep improves trajectory error.
7. A linked probe resolves another behavior during setup.
8. No private reflection into SpiceSharp internals is used.
9. Existing SpiceSharp tests remain unchanged and passing.

## Acceptance gate

- the proof runs with the repository's actual SpiceSharp version;
- all APIs are named and cited in the ADR;
- no mechanics classes exist;
- no custom simulation type exists;
- no custom solver or integrator exists.

## Verification report

```text
docs/verification/phase-00.md
```

Stop after Phase 0.

---

# 8. Phase 1 — Double-precision mathematics and numerical test support

## Goal

Create the small mathematics layer needed by later mechanics components.

Do not add SpiceSharp entities in this phase.

## Implement

```text
Vector2D
Matrix2x2D
AngleMath
SmoothFunctions
FiniteDifferenceJacobian
NumericAssert
TimeSeriesComparison
```

`Vector2D` must support:

- addition and subtraction;
- scalar multiplication and division;
- dot product;
- 2D scalar cross product;
- perpendicular vector;
- squared length and length;
- normalized vector with explicit epsilon handling;
- rotation by angle;
- equality only for exact structural use;
- separate approximate-comparison helpers.

`SmoothFunctions` must support:

- smooth positive part and derivative;
- smooth negative part and derivative;
- smooth absolute value and derivative;
- `tanh` friction function and derivative;
- safe regularized vector length.

## Required tests

- algebra identities;
- rotation invariance of length;
- cross-product signs;
- derivative checks for every smooth function;
- behavior near zero;
- behavior for magnitudes many times larger than epsilon;
- no `float` fields;
- no dependency on `System.Numerics.Vector2`.

## Acceptance gate

For central finite differences with appropriate step size:

```text
maximum derivative mismatch <= 1e-7
```

Use scale-aware assertions.

## Verification report

```text
docs/verification/phase-01.md
```

Stop after Phase 1.

---

# 9. Phase 2 — `MechanicalCoordinate`

## Goal

Implement one generalized mechanical coordinate entirely inside ordinary SpiceSharp `Transient`.

## Public API

A possible API is:

```csharp
var coordinate = new MechanicalCoordinate(
    "slider",
    generalizedMass: 2.0,
    initialPosition: 0.25,
    initialVelocity: -0.1);
```

Required public properties:

```text
Position
Velocity
GeneralizedMass
InitialPosition
InitialVelocity
GeneralizedForce
KineticEnergy
```

`GeneralizedForce` is the current net force represented by stamped connected components if a reliable diagnostic can be calculated. If not, omit it rather than reporting a misleading number.

## Internal behavior contract

Expose a behavior interface that later force entities can bind to:

```csharp
public interface IMechanicalCoordinateBehavior : IBehavior
{
    IVariable<double> PositionVariable { get; }
    IVariable<double> VelocityVariable { get; }

    // Exact row/location abstractions must follow the proven API.
}
```

Do not expose mutable raw solver arrays publicly.

## Required capabilities

- initial position and velocity;
- generalized mass greater than zero;
- transient integration;
- operating-point initial-state hold;
- parameter/property export;
- deterministic behavior lookup;
- default SpiceSharp validation for a purely mechanical circuit without an
  unrelated electrical component;
- support for external components adding force terms.

## Add test-only force components

Create internal test entities:

```text
ConstantGeneralizedForce
LinearGeneralizedDamping
LinearGeneralizedSpringToReference
```

Do not make them public yet unless their API is already final.

## Required tests

1. Zero force, zero initial velocity: position remains constant.
2. Zero force, nonzero velocity: constant-velocity motion.
3. Constant force: compare to analytic acceleration.
4. Linear damping: compare velocity to exponential decay.
5. Linear spring: compare small oscillator period.
6. Damped oscillator: compare decay envelope and damped frequency.
7. Negative or zero mass is rejected during setup.
8. Initial-condition mode is deterministic.
9. Results converge under maximum-timestep refinement.
10. Energy is constant for the undamped oscillator within integration tolerance.
11. Exported position and velocity refer to behavior state, not stale entity parameters.
12. A coordinate-only circuit passes default `Transient` validation without a
    dummy resistor or disabled validation.

## Acceptance thresholds

For smooth analytic cases using a sufficiently small maximum timestep:

```text
constant-force position relative error <= 1e-4
constant-force velocity relative error <= 1e-5
damped-decay relative error <= 2e-4
oscillator period relative error <= 2e-3
```

Also show results for:

```text
h
h/2
h/4
```

The error should generally decrease. If the selected SpiceSharp integration method has a known order or damping behavior, document it rather than asserting an unsupported convergence order.

## Verification report

```text
docs/verification/phase-02.md
```

Stop after Phase 2.

---

# 10. Phase 3 — `RigidBody2D`

## Goal

Build a planar rigid body from three verified generalized coordinates:

```text
x translation
y translation
rotation
```

## Public API

```csharp
var body = new RigidBody2D(
    "body",
    mass: 1.5,
    inertia: 0.08,
    initialPosition: new Vector2D(0.0, 1.0),
    initialAngle: 0.2,
    initialLinearVelocity: new Vector2D(0.5, 0.0),
    initialAngularVelocity: 2.0);
```

Required properties:

```text
PositionX
PositionY
Angle
VelocityX
VelocityY
AngularVelocity
Mass
Inertia
LinearKineticEnergy
AngularKineticEnergy
KineticEnergy
```

## Design rule

`RigidBody2D` may internally share coordinate implementation code, but should present one body entity and one body behavior to linked components.

Do not require users to manually create six nodes or three coordinate entities.

## Add geometry helpers

Implement and test:

```text
LocalPointToWorld
LocalVectorToWorld
WorldPointToLocal
WorldVectorToLocal
GetPointVelocity
ComputeTorque
```

## Required tests

1. Constant world force through center of mass.
2. Constant torque.
3. Combined force and torque.
4. No-force inertial motion.
5. Local/world transform round trip.
6. Point velocity with pure angular motion.
7. Torque sign for known force/lever-arm cases.
8. Translational and rotational kinetic energy.
9. Invalid mass or inertia rejected.
10. Angle is not forcibly wrapped.
11. Two bodies have independent solver variables.
12. Repeated runs are deterministic.
13. A body-only circuit passes default `Transient` validation without a dummy
    resistor or disabled validation.

## Acceptance thresholds

Use analytic constant-force and constant-torque solutions:

```text
linear velocity relative error <= 1e-5
angular velocity relative error <= 1e-5
position/angle relative or scale-aware absolute error <= 1e-4
transform round-trip absolute error <= 1e-12
```

## Verification report

```text
docs/verification/phase-03.md
```

Stop after Phase 3.

---

# 11. Phase 4 — Basic force and torque components

## Goal

Implement practical force components that stamp into `RigidBody2D`.

## Implement

```text
Gravity2D
AppliedForce2D
PointForce2D
AppliedTorque2D
LinearDrag2D
AngularDrag2D
```

## Required semantics

### `Gravity2D`

Apply:

\[
F=m g
\]

Allow a world acceleration vector.

### `AppliedForce2D`

Apply a force through the center of mass.

Support initially:

- constant world force;
- a time function using the exact SpiceSharp-supported waveform/function mechanism, or a package-owned delegate if lifecycle and determinism are clear.

Do not add expression parsing.

### `PointForce2D`

Apply a force at a body-local point.

Support force coordinates explicitly:

```text
World
BodyLocal
```

If the force vector is body-local, rotate it into the world frame and include all angle derivatives.

### `AppliedTorque2D`

Apply scalar world torque.

### `LinearDrag2D`

\[
F=-c(v-v_{medium})
\]

### `AngularDrag2D`

\[
\tau=-c_\omega(\omega-\omega_{medium})
\]

## Required tests

- free fall;
- projectile motion without drag;
- exponential velocity decay with linear drag;
- angular-speed decay;
- off-center force produces expected torque;
- equal center force produces no torque;
- local force rotates with body;
- analytic Jacobian versus finite difference for `PointForce2D`;
- an off-center world-force transient against an independent nonlinear
  trajectory reference, exercising the production Newton stamp;
- force superposition;
- component order does not change results beyond roundoff.

## Acceptance thresholds

```text
free-fall velocity relative error <= 1e-5
free-fall position relative error <= 1e-4
drag decay relative error <= 2e-4
point-force torque absolute error <= 1e-11 for direct evaluation
Jacobian relative mismatch <= 2e-6
```

## Sample

Add:

```text
samples/SpiceSharpMechanical2D.Samples/FreeFall
```

It must output CSV with:

```text
time,x,y,vx,vy,angle,omega
```

## Verification report

```text
docs/verification/phase-04.md
```

Stop after Phase 4.

---

# 12. Phase 5 — Distance and rotational spring-dampers

## Goal

Implement nonlinear force transfer between two rigid bodies through local anchor points.

## Implement

```text
DistanceSpringDamper2D
RotationalSpringDamper2D
```

Allow either body endpoint to refer to a fixed world anchor where appropriate.

## Distance spring-damper

For anchor points \(p_A,p_B\):

\[
d=p_B-p_A
\]

Use regularized length:

\[
L_\epsilon=\sqrt{d\cdot d+\epsilon_L^2}
\]

\[
n=d/L_\epsilon
\]

Relative anchor velocity:

\[
v_r=v_B^{point}-v_A^{point}
\]

Normal speed:

\[
v_n=n\cdot v_r
\]

Force magnitude:

\[
f=k(L-L_0)+cv_n
\]

Apply equal-and-opposite forces and corresponding torques.

Document that regularization modifies behavior when anchor separation is comparable to \(\epsilon_L\).

## Rotational spring-damper

Let the unbounded relative error be:

\[
e_\theta=\theta_B-\theta_A-\theta_0
\]

\[
e_\omega=\omega_B-\omega_A
\]

\[
\tau=k_\theta\sin(e_\theta)+c_\theta e_\omega
\]

Here `k_theta` is the tangent stiffness at the reference angle. This periodic
law is smooth across every `+/-pi` representation seam; a separately reported
shortest-angle diagnostic may use `wrapToPi`, but the solver residual and
Jacobian must not use a discontinuous wrapped-linear law. Ensure torque signs
oppose the relative error for `abs(wrapToPi(e_theta)) < pi` and stamp the exact
angle derivative `k_theta*cos(e_theta)`.

## Required tests

1. Two-body spring force is equal and opposite.
2. Net internal linear force is zero.
3. Net internal torque about the world origin is zero within tolerance.
4. One-body spring to world.
5. Oscillation frequency for a simple reduced-mass case.
6. Damped oscillation.
7. Off-center anchors generate torque.
8. Pure rotational spring.
9. Torque and analytic Jacobian remain smooth through relative error
   \(-\pi/\pi\), while the diagnostic shortest-angle representation wraps.
10. Full analytic Jacobian versus finite difference.
11. No NaN at nearly coincident anchors.
12. Timestep-refinement study.
13. An off-center production transient matches an independent nonlinear
    rigid-body trajectory reference.

## Acceptance thresholds

```text
force-action/reaction residual <= 1e-11 N
world torque residual <= 1e-10 N m
Jacobian relative mismatch <= 5e-6
oscillator frequency relative error <= 3e-3
```

## Sample

Add:

```text
samples/SpiceSharpMechanical2D.Samples/Pendulum
```

Use a compliant connection, not an exact rigid constraint.

## Verification report

```text
docs/verification/phase-05.md
```

Stop after Phase 5.

---

# 13. Phase 6 — Compliant joints

## Goal

Provide mechanism-building components that remain smooth and compatible with ordinary SpiceSharp Newton solving.

## Implement

```text
RevoluteJoint2D
PrismaticJoint2D
WeldJoint2D
```

These are compliant joints, not exact Lagrange-multiplier constraints.

## Revolute joint

For anchor error:

\[
e_p=p_B-p_A
\]

and relative point velocity:

\[
e_v=v_B^{point}-v_A^{point}
\]

Apply:

\[
F=K_p e_p+C_p e_v
\]

with signs chosen to reduce the error.

Allow isotropic stiffness/damping first. A 2x2 stiffness matrix is optional only after scalar behavior is verified.

## Weld joint

Combine:

- compliant revolute-anchor constraint;
- rotational spring-damper.

## Prismatic joint

Define a guide axis attached either to world or body A.

Constrain:

- anchor displacement normal to the axis;
- relative normal velocity;
- relative angle.

Leave translation along the axis free unless optional axial spring/damping is enabled.

The axis orientation may depend on body angle, requiring analytic derivatives.

## Required tests

### Revolute

- single pendulum period;
- bounded anchor separation;
- force action/reaction;
- off-center torque balance.

### Weld

- two bodies move approximately as one body under slow loading;
- relative angle remains bounded;
- relative anchor error remains bounded.

### Prismatic

- free motion along axis;
- suppressed motion normal to axis;
- rotating guide-axis Jacobian;
- slider under axial force.

### Mechanisms

- compliant slider-crank;
- compliant four-bar with low-speed drive.

## Important diagnostics

Each joint must expose:

```text
PositionError
VelocityError
ReactionForce
ReactionTorque where applicable
StoredElasticEnergy
DissipatedPower
```

Label reaction values as compliant-element forces, not exact rigid-constraint multipliers.

## Acceptance thresholds

Thresholds depend on user stiffness and timestep. Use normalized criteria in tests:

```text
maximum anchor error <= 1e-3 times characteristic link length
maximum prismatic normal error <= 1e-3 times stroke
Jacobian relative mismatch <= 1e-5
```

Tests must demonstrate that reducing timestep and/or increasing stiffness reduces geometric error until numerical conditioning becomes limiting.

## Samples

Add:

```text
SliderCrank
CompliantFourBar
```

## Verification report

```text
docs/verification/phase-06.md
```

Stop after Phase 6.

## Optional coupling-first roadmap fork

Electrical-mechanical coupling has no technical dependency on the contact or
cam phases. If the product direction stops after Phase 6, the detailed
coupling milestone in Phase 12 may be separately authorized as the next and
only feature milestone while Phases 7-11 remain unimplemented. In that case:

- do not claim that the skipped phases are complete;
- use `docs/verification/coupling-01.md` rather than creating empty Phase
  7-11 reports;
- implement only the motor-driven rotor and existing-mechanism samples;
- keep the motor-driven cam case conditional on the cam phases ever being
  implemented;
- stop after the coupling milestone and return to documentation, samples, and
  productization rather than contact physics.

This fork is the shortest route to demonstrating why the mechanical equations
live inside SpiceSharp. It does not authorize implementation by itself.

---

# 14. Phase 7 — Geometry primitives for explicit contact

## Goal

Implement deterministic, independently testable geometry calculations before adding contact forces.

Do not add dynamic contact behavior in this phase.

## Implement

```text
CircleShape2D
PlaneShape2D
SegmentShape2D
ClosestPointOnSegment
CirclePlaneGeometry
CircleCircleGeometry
CircleSegmentGeometry
```

Geometry result:

```csharp
public readonly record struct ContactGeometry2D(
    double Gap,
    Vector2D Normal,
    Vector2D PointOnA,
    Vector2D PointOnB);
```

Convention:

```text
normal points from shape A toward shape B
gap > 0 means separated
gap = 0 means touching
gap < 0 means geometric overlap
```

Keep the convention identical everywhere.

## Required tests

- translated and rotated invariance;
- symmetry under swapping shapes with adjusted normal;
- circle-plane analytic cases;
- circle-circle analytic cases;
- circle-segment endpoint and interior cases;
- degenerate segment rejected;
- normal is unit length within tolerance;
- contact points reproduce the gap;
- finite-difference geometry derivatives for smooth, non-feature-switching cases.

## Acceptance thresholds

```text
analytic gap absolute error <= 1e-12
normal length error <= 1e-12
contact-point consistency <= 1e-11
```

Feature transitions at segment endpoints may be nondifferentiable. Document them and do not use such transitions as Newton test points.

## Verification report

```text
docs/verification/phase-07.md
```

Stop after Phase 7.

---

# 15. Phase 8 — Smooth normal contact

## Goal

Add explicitly declared, smooth, compliant contact.

## Implement

```text
SmoothContactLaw
CircleGroundContact2D
CircleCircleContact2D
CircleSegmentContact2D
```

Do not add friction yet.

## Normal law

For gap \(g\), geometric penetration is:

\[
\delta=-g
\]

Smoothed penetration:

\[
\delta_+=\operatorname{positive}_\epsilon(\delta)
\]

Relative normal speed:

\[
v_n=n\cdot(v_B^{point}-v_A^{point})
\]

Use a non-attractive law. A suitable first formulation is:

\[
F_{elastic}=k\delta_+^p
\]

\[
F_{damping}=c\delta_+^r
\operatorname{positive}_{\epsilon_v}(-v_n)
\]

\[
F_n=\operatorname{positive}_{\epsilon_F}
(F_{elastic}+F_{damping})
\]

Default exponent:

```text
p = 1
```

Optional Hertz-like exponent:

```text
p = 1.5
```

All exponents and zero behavior must be well defined.

## Time-step guidance

Expose a diagnostic estimate:

\[
\omega_n\approx\sqrt{k/m_{effective}}
\]

and warn when the configured maximum timestep is too large relative to the contact period.

Do not attempt to change the SpiceSharp timestep from inside the component in this phase.

## Required tests

1. Static circle on ground: penetration approaches approximately \(mg/k\) for linear contact.
2. No attractive force when separated beyond smoothing region.
3. Smooth force around contact onset.
4. Damped impact does not add energy.
5. Circle-circle equal-and-opposite force and torque.
6. Circle-segment interior contact.
7. Analytic Jacobian versus finite difference.
8. Force increases monotonically with penetration for supported parameters.
9. No NaN at zero relative speed.
10. Timestep and stiffness convergence study.

## Acceptance thresholds

```text
static penetration relative error <= 2% after transient settling
separated force <= documented smoothing-force bound
action/reaction force residual <= 1e-10 N
Jacobian relative mismatch <= 2e-5
```

The 2% static threshold may be tightened after measured behavior is known. Do not loosen it without evidence.

## Sample

Add:

```text
BouncingCircle
```

Export:

```text
time,y,vy,gap,penetration,normalForce,totalEnergy
```

## Verification report

```text
docs/verification/phase-08.md
```

Stop after Phase 8.

---

# 16. Phase 9 — Regularized friction and contact material

## Goal

Add smooth tangential contact behavior while preserving Newton compatibility.

## Implement

```text
ContactMaterial2D
RegularizedFrictionLaw
friction support in explicit contact entities
```

Parameters:

```text
NormalStiffness
NormalDamping
NormalExponent
FrictionCoefficient
FrictionSmoothingSpeed
PenetrationSmoothing
VelocitySmoothing
```

Do not call the friction coefficient “static friction” or “dynamic friction” in this model. It is one regularized Coulomb coefficient.

## Friction law

\[
F_t=-\mu F_n\tanh(v_t/v_s)
\]

where tangent direction is consistently derived from the normal.

Apply friction forces at the contact point, including torque.

## Required tests

1. Friction is zero when normal force is zero.
2. Friction opposes slip.
3. \(|F_t|<\mu F_n\) for finite slip and approaches the limit asymptotically.
4. Near-zero slip is smooth.
5. Sliding block decelerates without numerical chatter.
6. Inclined plane settles to a small creep speed rather than falsely claiming exact sticking.
7. Angular response from off-center friction.
8. Analytic Jacobian versus finite difference.
9. Mechanical power from friction is nonpositive within tolerance.
10. Repeated simulation is deterministic.

## Acceptance thresholds

```text
friction bound violation <= 1e-12 N in direct law tests
positive friction power <= 1e-10 W plus scale-aware tolerance
Jacobian relative mismatch <= 3e-5
```

Document creep near zero velocity as an expected property of regularization.

## Verification report

```text
docs/verification/phase-09.md
```

Stop after Phase 9.

---

# 17. Phase 10 — Cam lift laws and translating follower

## Goal

Implement the first practical cam/follower component using an exact lift law rather than general collision geometry.

## Implement

```text
ICamLiftLaw
HarmonicCamLiftLaw
PolynomialCamLiftLaw
PeriodicCubicSpline1D
CamLiftContact2D
```

## Lift-law interface

```csharp
public interface ICamLiftLaw
{
    double Period { get; }
    double EvaluatePosition(double angle);
    double EvaluateFirstDerivative(double angle);
    double EvaluateSecondDerivative(double angle);
}
```

Derivatives are with respect to cam angle, not time.

## Component model

Inputs:

```text
cam body angle and angular velocity
follower body position and velocity
world follower axis
clearance
contact stiffness and damping
optional follower offset
```

Gap:

\[
g=y_f-s(\theta_c)-clearance
\]

using the correct sign convention for the configured follower axis.

Gap speed:

\[
\dot g=v_f-s'(\theta_c)\omega_c
\]

Use the verified smooth contact law.

The cam reaction torque must follow virtual work:

\[
\tau_c=-F_n s'(\theta_c)
\]

with signs validated by power balance.

## Required tests

1. Constant-radius/no-lift law.
2. Sinusoidal lift law.
3. Polynomial rise/dwell/return law.
4. Periodic spline continuity at the seam.
5. Spline value and derivative against independent finite differences.
6. Follower remains in contact under sufficient preload.
7. Lift-off produces near-zero contact force.
8. Recontact remains finite and converges with timestep.
9. Cam torque and follower force satisfy power balance:
   \[
   \tau\omega+Fv\approx0
   \]
   excluding damping and stored contact energy.
10. Full component Jacobian versus finite difference.

## Acceptance thresholds

```text
spline seam position mismatch <= 1e-12
spline seam first-derivative mismatch <= 1e-10
spline derivative finite-difference mismatch <= 1e-7 scale-aware
contact power-balance residual <= 1e-5 of characteristic power
Jacobian relative mismatch <= 5e-5
```

## Sample

Add:

```text
CamFollower
```

Export:

```text
time,camAngle,camOmega,followerPosition,followerVelocity,
lift,gap,normalForce,camReactionTorque
```

## Verification report

```text
docs/verification/phase-10.md
```

Stop after Phase 10.

---

# 18. Phase 11 — Geometric roller cam profile

## Goal

Support a smooth custom 2D cam curve and circular roller follower.

This phase is more difficult than the lift-law component and must not begin until Phase 10 is stable.

## Implement

```text
IParametricCurve2D
CurveSample2D
ClosestPointResult2D
PeriodicSplineCurve2D
RollerCamContact2D
```

Interface:

```csharp
public interface IParametricCurve2D
{
    double Period { get; }
    CurveSample2D Evaluate(double parameter);
    ClosestPointResult2D FindClosestPoint(
        Vector2D localPoint,
        double initialParameter);
}
```

`CurveSample2D` contains:

```text
Position
FirstDerivative
SecondDerivative
UnitTangent
UnitNormal
Curvature
```

## Closest-point algorithm

Use:

- previous accepted/contact parameter as initial seed;
- periodic parameter wrapping;
- safeguarded Newton iteration;
- bounded step size;
- fallback coarse sampling followed by local refinement;
- deterministic tie-breaking;
- explicit failure diagnostics.

Do not search an unbounded number of samples during every Newton load.

Cache the closest parameter in the behavior and update it carefully. Do not commit iteration history as accepted transient history until SpiceSharp accepts the timestep if the API exposes the distinction. If that distinction cannot be safely observed, document the limitation and use a deterministic recomputation strategy.

## Roller contact

Transform roller center into cam-local coordinates.

Find closest cam point and normal.

Gap:

\[
g=distance(center,curve)-rollerRadius-clearance
\]

Use the verified smooth normal/friction laws.

Apply:

- force and torque to follower;
- equal-and-opposite force and torque to cam;
- complete derivatives, including closest-point dependence where practical.

If exact implicit differentiation of the closest-point parameter is too large for this phase, Codex must first implement and validate the envelope-theorem form for regular closest points, document its assumptions, and reject singular curve cases.

## Required tests

1. Circle cam against analytic result.
2. Eccentric circle.
3. Ellipse.
4. Periodic spline seam.
5. Closest-point invariance under rigid transform.
6. Closest-point orthogonality condition.
7. Roller gap against analytic circle geometry.
8. Force/torque action-reaction.
9. Power balance.
10. Profile parameter continuity over multiple revolutions.
11. Fallback seed path.
12. Jacobian test away from curvature singularities and branch changes.
13. Timestep/profile-resolution convergence.

## Acceptance thresholds

```text
analytic circle gap error <= 1e-10 m
closest-point orthogonality residual <= 1e-10 scale-aware
action/reaction force residual <= 1e-9 N
action/reaction world torque residual <= 1e-8 N m
Jacobian relative mismatch <= 2e-4 in regular cases
```

The looser Jacobian threshold reflects closest-point conditioning; report actual values.

## Verification report

```text
docs/verification/phase-11.md
```

Stop after Phase 11.

---

# 19. Phase 12 — Reciprocal electrical–mechanical coupling

## Goal

Demonstrate the main architectural reason for placing mechanics inside
SpiceSharp: electrical node voltages, electrical branch current, and rigid-body
angular velocity participate in one ordinary SpiceSharp transient solve.

The first component is a passive, ideal DC motor/generator transducer. It is
not a voltage-controlled `AppliedTorque2D`, a callback that samples circuit
current after a step, or a pair of separately advanced simulations. Torque and
back-EMF are reciprocal off-diagonal terms in the same matrix.

This phase is transient-only. Do not add AC, noise, temperature, magnetic
saturation, commutation ripple, thermal winding behavior, or parser syntax
until the transient power-conjugate component is complete and verified.

## Applications and product boundary

This milestone supports circuit-centric mechatronic studies where electrical
loading and mechanism motion must affect one another during the same timestep:

- startup current, acceleration, and speed droop of a motor-driven mechanism;
- stall current and torque sizing against an explicit supply and winding;
- back-EMF, regenerative operation, and dynamic braking into a resistor;
- voltage reversal, load steps, and supply-current ripple caused by a
  slider-crank or other already implemented mechanism;
- effects of source resistance, current limiting implemented in the circuit,
  winding inductance, flyback paths, and switching circuits on shaft motion;
- effects of inertia, drag, payload, joint stiffness, and damping on circuit
  current and electrical energy consumption;
- compact actuator/sensor demonstrations for controls, power electronics,
  education, and reduced-order digital prototypes.

The differentiator is not broader rigid-body simulation. It is that the
electrical network and mechanical coordinates share one Newton system, so
back-EMF and torque are simultaneous reciprocal terms rather than delayed data
exchanged between two engines.

This does not make the project a replacement for a general real-time 2D/3D
collision engine. Do not claim support for large contact stacks, game-style
collision detection, articulated robotics at interactive rates, finite-element
electromagnetics, magnetic field geometry, or detailed motor commutation. For
those uses, a specialized physics or electromagnetic engine may be the primary
model and coupling would require a separately designed co-simulation boundary.

## Locally verified SpiceSharp API fit

The pinned SpiceSharp 3.2.3 package and existing repository components provide
the required supported APIs:

- derive the two-terminal entity from `Component<TParameters>`;
- declare `[Pin(0, "+")]` and `[Pin(1, "-")]`, construct with pin count 2,
  and call `Connect(positive, negative)`;
- create a `ComponentBindingContext` so the behavior receives
  `IComponentBindingContext.Nodes`;
- call `context.Nodes.CheckNodes(2)`;
- obtain terminal variables through
  `IBiasingSimulationState.GetSharedVariable(context.Nodes[index])`;
- create a private current unknown with
  `CreatePrivateVariable(Name.Combine("branch"), Units.Ampere)`;
- expose that unknown through `IBranchedBehavior<double>.Branch`;
- resolve `IRigidBody2DBehavior` once during behavior construction using the
  same `Reference(...).GetContainer(...).GetValue<T>()` pattern as existing
  mechanical loads;
- map terminal, branch, and body angular-velocity variables through
  `IBiasingSimulationState.Map`;
- precompute the coupled sparse locations in one `ElementSet<double>`;
- use `ITimeSimulationState.UseDc` to distinguish requested-state operating
  point behavior from transient torque loading;
- use `IAcceptBehavior` for stable public power and conversion diagnostics.

The repository's `NonlinearInductor` is concrete evidence for the two-pin
component, shared terminal variables, private ampere-valued branch,
`IBranchedBehavior<double>`, transient derivative state, and `ElementSet`
patterns. The existing rigid-body load and joint behaviors prove mapping and
stamping into `IRigidBody2DBehavior.AngularVelocityVariable`.

No private SpiceSharp reflection or parser dependency is required.

## Scope decision: ideal transducer, external winding

Implement one ideal transducer and keep winding resistance and inductance as
ordinary SpiceSharp components:

```text
VoltageSource -> Resistor -> Inductor -> DcMotorCoupler2D -> ground
```

Reasons:

- `Resistor` already owns copper loss `R*i^2`;
- `Inductor` already owns flux history and magnetic energy `0.5*L*i^2`;
- their DC, transient, initial-condition, and export semantics are already
  tested by SpiceSharp;
- the coupler can focus on energy conversion rather than duplicating standard
  electrical devices;
- locked-rotor current, electrical time constant, and winding initial current
  remain visible in an ordinary equivalent circuit;
- users may replace the simple winding with a richer external circuit without
  changing the mechanical component.

Do not add optional resistance or inductance properties to the first coupler.
An integrated winding may be considered only after the external-component
version passes all energy and backdrive tests.

## Public entity contract

```csharp
[Pin(0, "+"), Pin(1, "-")]
public sealed class DcMotorCoupler2D
    : Component<DcMotorCoupler2DParameters>
{
    public DcMotorCoupler2D(
        string name,
        string positiveNode,
        string negativeNode,
        string bodyName,
        double motorConstant,
        double shaftSign = 1.0);
}
```

Required public properties:

```text
BodyName       immutable referenced rigid-body entity name
MotorConstant positive finite ideal SI motor constant
ShaftSign      exactly +1 or -1
```

Use one `MotorConstant`, not independently configurable `Kt` and `Ke`, for
the ideal component. In coherent SI units:

```text
K = Kt = Ke
Kt units: N*m/A
Ke units: V/(rad/s)
```

Radians are dimensionless in the power identity. Separate unequal constants
would imply an unreported efficiency, loss, or gain and are therefore not part
of an ideal passive transducer.

The coupled body supplies rotor inertia. The coupler must not create a hidden
mechanical body, hidden inertia, electrical ground, winding resistor,
inductor, drag, gear ratio, or load torque.

## Direction and sign convention

Define:

```text
v = V(positiveNode) - V(negativeNode)
i > 0 enters the positive electrical terminal
omega > 0 is the body's counterclockwise angular velocity
s = ShaftSign, exactly +1 or -1
```

The ideal transducer law is:

\[
e=sK\omega
\]

\[
\tau_{body}=sKi
\]

and the electrical branch equation is:

\[
v-e=0.
\]

Positive `v*i` is electrical power absorbed by the transducer. Positive
`tauBody*omega` is mechanical power delivered to the body. Therefore:

\[
p_{electrical}-p_{shaft}
=vi-\tau_{body}\omega
=0.
\]

This convention produces motoring when electrical power enters and generator
operation when mechanical power enters. Reversing either terminal polarity or
`ShaftSign` must produce the documented mapped result without changing the
underlying power identity.

## Unknowns and local equations

The behavior uses the shared positive and negative node voltages, one private
branch-current unknown, and the existing body angular velocity:

```text
local columns = [vp, vn, i, omega]
local rows    = [KCL positive, KCL negative, motor branch, body angular dynamics]
```

Transient residual contributions are:

```text
Rpositive +=  i
Rnegative += -i
Rbranch   +=  vp - vn - s*K*omega
Rbody     += -s*K*i
```

`Rbody` follows the existing mechanical convention `R = inertia - Q`, hence
the negative motor-torque term.

The complete constant matrix contribution is:

| Row | Column | Value | Meaning |
| --- | --- | ---: | --- |
| positive-node KCL | branch current | `+1` | current entering positive terminal |
| negative-node KCL | branch current | `-1` | current leaving negative terminal |
| branch equation | positive voltage | `+1` | terminal voltage |
| branch equation | negative voltage | `-1` | terminal voltage |
| branch equation | body omega | `-s*K` | back-EMF coupling |
| body angular dynamics | branch current | `-s*K` | torque coupling in residual form |

The two off-diagonal conversion terms are equal under the chosen power
variables. This reciprocity is an explicit test target. The component has no
nonzero RHS for constant `K` and `s`.

Do not finite-difference this linear production law. Direct matrix-value tests
must still verify every location and sign; end-to-end tests verify the mapped
solver rows and columns.

## Operating-point and transient lifecycle

The electrical branch must exist during the DC operating point so the circuit
is not topologically different between DC and transient:

```text
DC:
    stamp positive/negative KCL
    stamp vp - vn - s*K*omega = 0
    do not stamp motor torque into the body row

Transient:
    stamp the same electrical equations
    additionally stamp -s*K*i into body angular dynamics
```

The body behavior holds its requested initial angle and angular velocity
during DC. The branch equation therefore sees the requested initial omega and
the external winding circuit determines a consistent initial current. Motor
torque begins with the other mechanical loads during transient loading.

This intentional initialization asymmetry preserves the Phase 3/4 requested-
state contract. It must be documented and tested; it is not an equilibrium
solve for the complete motor-mechanism system.

If later requirements demand electromechanical static equilibrium, define a
new explicit initialization policy rather than silently changing this phase.

## Behavior construction outline

Construction may allocate and cache:

1. the two shared terminal variables;
2. the private ampere-valued branch variable;
3. the resolved `IRigidBody2DBehavior` reference;
4. mapped positive, negative, branch, and angular-dynamics indices;
5. one `ElementSet<double>` for all six transient matrix entries;
6. cached trial and accepted diagnostic scalars;
7. `ITimeSimulationState`.

The load path must perform no reflection, name lookup, behavior lookup,
matrix-location lookup, LINQ, list construction, or allocation. Because the
law is linear, it fills constant cached values and adds the precomputed element
set.

The body entity must precede the coupler in behavior-construction order under
the current deterministic reference contract. Ordinary electrical component
ordering must not affect results beyond floating-point assembly order.

## Typed accepted diagnostics

Expose a type-specific behavior interface, for example:

```csharp
public interface IDcMotorCoupler2DBehavior : IBehavior
{
    IVariable<double> Branch { get; }
    double Voltage { get; }
    double Current { get; }
    double BackEmf { get; }
    double TorqueOnBody { get; }
    double ElectricalPowerIn { get; }
    double ShaftPowerOut { get; }
    double ConversionPowerResidual { get; }
}
```

Definitions:

```text
Voltage                 vp - vn
Current                 branch current, positive into + terminal
BackEmf                 s*K*omega
TorqueOnBody            s*K*i
ElectricalPowerIn       Voltage*Current
ShaftPowerOut           TorqueOnBody*omega
ConversionPowerResidual ElectricalPowerIn-ShaftPowerOut
```

Publish values from the last accepted transient timepoint using
`IAcceptBehavior`. Never expose the last Newton trial as an accepted power
measurement. Generated real-property names should include at least:

```text
v, i, backemf, torque, electricalpower, shaftpower, conversionresidual
```

Copper loss and magnetic energy do not belong to this behavior when the
winding uses external `Resistor` and `Inductor` entities. Samples and energy
tests calculate them from the explicit circuit parameters and accepted
current.

## Validation and setup diagnostics

Reject before transient loading:

- a null, empty, or unresolved body name;
- a referenced entity without `IRigidBody2DBehavior`;
- nonfinite or nonpositive `MotorConstant`;
- a `ShaftSign` other than exactly `+1` or `-1`;
- an invalid pin count or missing terminal name;
- nonfinite requested body initial angular velocity;
- unsupported simulation modes when no appropriate behavior exists.

The ordinary SpiceSharp validation rules remain responsible for floating
electrical nodes and voltage-defined loops. Add contextual motor diagnostics
where useful, but do not disable or bypass those rules.

Warn locally, without mutating the model, about:

- identical positive and negative terminal names;
- estimated stall current or torque far outside configured diagnostic limits;
- electrical time constant `L/R`, mechanical time constant `J/b`, or coupled
  natural modes poorly resolved by maximum timestep;
- a zero-load, zero-drag model that has no finite settling time.

The coupler cannot infer an arbitrary external winding or complete circuit
topology from its own binding context. Detection of an ideal voltage-source
loop, nonpositive external winding resistance, floating networks, and
whole-system timestep risks belongs to ordinary SpiceSharp validation or the
optional read-only Phase 13 setup validator. Phase 12 tests should still
construct those bad setups and record which supported validation layer reports
them; do not add a hidden topology walker to the component.

Do not clamp current, torque, omega, voltage, back-EMF, or power. Nonfinite
solver values must remain visible as failures.

## Required equation and topology tests

1. Two electrical terminals and one private branch variable are created with
   volt and ampere units respectively.
2. `IBranchedBehavior<double>.Branch` exposes the same current unknown used by
   the matrix stamp.
3. Positive and negative KCL entries are exact opposites.
4. The branch row stamps `vp-vn-s*K*omega=0` with the exact mapped omega
   column.
5. The body dynamics row stamps `-s*K*i` with the exact branch column.
6. The reciprocal off-diagonal coefficients are equal for an ideal SI motor.
7. Direct values satisfy `backEmf=s*K*omega` and `torque=s*K*i`.
8. `electricalPowerIn-shaftPowerOut` is zero within roundoff at arbitrary
   finite current and omega.
9. `ShaftSign=-1`, reversed terminals, reversed voltage, and reversed shaft
   velocity each follow a named sign table.
10. DC includes the electrical branch but excludes transient mechanical
    torque, preserving the requested body state.
11. Missing body, wrong entity type, invalid constant, invalid sign, and bad
    terminal topology fail with the motor name in the diagnostic.
12. Body-before-coupler ordering succeeds; a coupler placed before its body
    fails during setup with a descriptive reference error.

## Required isolated operating cases

### Locked rotor / stall

Use a sufficiently stiff, separately verified weld or angular fixture and an
explicit winding resistance. For supply `V` and resistance `R` after the
electrical transient settles:

\[
i_{stall}=\frac{V}{R}
\]

\[
\tau_{stall}=K\frac{V}{R}.
\]

Report the residual fixture motion and demonstrate convergence as fixture
stiffness increases and timestep decreases. Do not call a compliant fixture
an exact lock.

### No-load speed with viscous drag

For resistance `R`, motor constant `K`, body angular drag `b`, supply `V`, and
negligible load torque, the steady state is:

\[
\omega_{ss}=\frac{VK}{K^2+Rb}
\]

\[
i_{ss}=\frac{Vb}{K^2+Rb}.
\]

Test several `R`, `b`, and polarity values. As `b` approaches zero, verify
`omega` approaches `V/K` and current approaches zero without asserting a
finite settling time at exactly zero drag.

### Loaded steady state

With constant opposing load torque `tauLoad` and drag `b`:

\[
i=\frac{b\omega+\tau_{load}}{K}
\]

\[
V=Ri+K\omega.
\]

Compare current, speed droop, motor torque, and load power with the analytic
solution for positive and negative load torque.

### Backdriven generator

- Open-circuit approximation: connect a documented high resistance and drive
  the shaft mechanically; terminal voltage approaches `s*K*omega` and current
  approaches zero.
- Resistive load: connect a finite load resistance; induced current creates a
  motor torque opposing the applied mechanical drive.
- Near short circuit: use a small positive resistance, not a hidden current or
  torque clamp; verify braking torque and copper loss while monitoring
  conditioning.
- Reverse shaft direction and confirm voltage, current, and torque polarity.

## Independent coupled transient oracle

With explicit winding `R`, `L`, rotor inertia `J`, angular drag `b`, supply
`V(t)`, and opposing load torque `tauLoad(t)`, compare production results with
an independent high-resolution state-space integration:

\[
L\dot i=V(t)-Ri-sK\omega
\]

\[
J\dot\omega=sKi-b\omega-\tau_{load}(t)
\]

or:

\[
\frac{d}{dt}
\begin{bmatrix}i\\\omega\end{bmatrix}
=
\begin{bmatrix}
-R/L & -sK/L\\
sK/J & -b/J
\end{bmatrix}
\begin{bmatrix}i\\\omega\end{bmatrix}
+
\begin{bmatrix}V/L\\-\tau_{load}/J\end{bmatrix}.
\]

The reference must not call the production coupler equation. Use either an
analytic matrix-exponential solution for constant inputs or a fourth-order
reference integrator with a step at least 50 times smaller than the smallest
production maximum timestep.

Exercise:

- voltage step from rest;
- nonzero initial current and angular velocity;
- voltage reversal;
- load-torque step;
- motoring-to-generating transition;
- underdamped and overdamped parameter sets where applicable;
- timestep refinement at `h`, `h/2`, and `h/4`.

## Energy and power verification

For an external ideal supply, winding resistor and inductor, rotor inertia,
angular drag, and external mechanical load, integrate:

\[
E_{supply}=\int V_{supply}i\,dt
\]

\[
E_{copper}=\int Ri^2\,dt
\]

\[
E_{drag}=\int b\omega^2\,dt
\]

\[
E_{load}=\int \tau_{load}\omega\,dt
\]

\[
E_{magnetic}=\frac12Li^2,
\qquad
E_{kinetic}=\frac12J\omega^2.
\]

The driven-system budget is:

\[
E_{supply}
-E_{copper}
-E_{drag}
-E_{load}
-\Delta E_{magnetic}
-\Delta E_{kinetic}
\approx0.
\]

For a backdriven generator, include mechanical input work with the opposite
boundary direction and verify electrical energy delivered to the load. Always
state the sign convention used by the numerical integral.

Also integrate the coupler-local conversion residual:

\[
\int(vi-\tau\omega)dt\approx0.
\]

Do not combine supplied energy, loss, and stored energy into an unsigned total
that can hide sign mistakes.

## Coupled mechanism verification

The mandatory mechanism case uses already implemented Phase 6 components:

```text
Voltage source
    -> winding resistor and inductor
    -> DcMotorCoupler2D
    -> revolute-supported crank
    -> compliant slider-crank
    -> payload force
```

Test:

- startup from rest;
- at least one full crank revolution after startup;
- slider reversal at both dead centers;
- increased payload produces increased mean/RMS current and speed droop;
- reversing supply polarity reverses the mechanism after the transient;
- backdriving the slider returns electrical energy to a resistive load;
- loop-closure error remains within the Phase 6 normalized limit;
- supply energy closes against copper loss, body kinetic energy, joint elastic
  storage, joint damping, payload work, and the coupler residual.

A motor-driven cam follower is conditional: add it only if Phases 10-11 have
actually been implemented. Skipping contact and cam phases must not block the
motor-driven rotor or slider-crank acceptance cases.

## Determinism and metamorphic checks

- Repeat identical runs and compare accepted traces within roundoff.
- Reverse terminal polarity together with shaft sign and verify the mapped
  trajectory.
- Mirror the planar mechanism and map torque/angle signs.
- Reorder independent electrical components and mechanical loads; results may
  differ only by documented floating-point assembly effects.
- Scale `K`, `R`, `L`, `J`, and `b` over a documented practical range and
  report electrical and mechanical time constants.
- Verify no per-load allocation after behavior construction.

## Acceptance thresholds

```text
direct stamp coefficient error                  exactly zero
ideal instantaneous conversion-power residual <= 1e-12 scale-aware
steady-state current and speed relative error  <= 2e-4
state-space trajectory normalized RMS error    <= 1e-3
integrated system energy-budget residual       <= 1e-3 of supplied/input energy
integrated coupler conversion residual         <= 1e-10 of transferred energy
Phase 6 mechanism closure-error limits         unchanged
```

If small transferred energy makes a relative power tolerance ill-conditioned,
use and report a physically scaled absolute floor rather than dividing by a
near-zero number. Tolerances may not be weakened without the usual measured
refinement evidence.

## Samples

Add small samples in increasing order:

```text
Learning/11MotorConstant
Learning/12MotorWithWinding
DcMotorRotor
DcMotorSliderCrank
DcMotorCamFollower      conditional on implemented cam phases
```

`Learning/11MotorConstant` should directly show `backEmf=K*omega` and
`torque=K*current` with the sign convention. `Learning/12MotorWithWinding`
should show the ordinary source/resistor/inductor/coupler circuit and print
current, speed, back-EMF, torque, electrical power, and shaft power.

## Deferred coupling variants

Do not add these to the first coupling milestone, but preserve compatible
energy/sign conventions:

- translational voice-coil coupler with `F=K*i` and
  `e=K*dot(axis, pointVelocity)`, including the body-local point torque;
- ideal rotational or translational sensors with an explicitly documented
  nonreciprocal, infinite-input-impedance measurement contract;
- solenoid/variable-reluctance actuator derived from a flux-linkage or
  co-energy model `lambda(i,x)`, where voltage and force come from the same
  differentiable energy function;
- nonlinear saturation and hysteresis models;
- thermal winding resistance and temperature coupling;
- multi-phase, commutated, brushless, or stepper motors;
- gearing inside the transducer.

For any future nonlinear magnetic actuator, require one authoritative smooth
energy/co-energy function, analytic mixed derivatives, Maxwell reciprocity,
and a closed electrical-mechanical energy budget. Independent force and
back-EMF curves without an integrability check are not acceptable.

## Required ADR content

Create `ADR-0008-reciprocal-electromechanical-coupling.md` before production
code. It must record at least:

1. **Context:** why circuit-mechanism feedback cannot be represented by a
   one-way applied torque or by reading an export after each accepted point.
2. **Decision:** use one SpiceSharp solve with an electrical branch current and
   the existing body angular-velocity unknown in the same sparse stamp.
3. **Passivity decision:** use one ideal SI constant `K=Kt=Ke` and the exact
   electrical/shaft power identity.
4. **Topology decision:** keep winding resistance and inductance as external
   standard SpiceSharp components.
5. **Initialization decision:** retain the electrical branch in DC but suppress
   motor torque during DC to preserve requested mechanical initial state.
6. **Sign decision:** define terminal current, voltage, positive body rotation,
   `ShaftSign`, motor torque, and generator operation with worked numeric
   examples for both signs.
7. **Lifecycle decision:** resolve body references and sparse locations once;
   publish only accepted diagnostic values.
8. **Scope decision:** transient only; parser syntax, AC linearization,
   efficiency maps, saturation, thermal behavior, commutation, gearing, and
   generalized co-simulation are deferred.
9. **Consequences:** one additional branch unknown per coupler, six transient
   matrix entries, body-before-coupler construction ordering, exact reciprocal
   energy conversion, and potential conditioning problems in ideal zero-
   impedance loops.
10. **Alternatives rejected for this milestone:** post-step callback coupling,
    two independently stepped solvers, voltage-controlled torque without
    back-EMF, arbitrary unequal `Kt`/`Ke`, a hidden rotor body, and a monolithic
    motor that duplicates resistor/inductor behavior.
11. **Revisit triggers:** a demonstrated need for AC analysis, static
    electromechanical equilibrium, variable/nonlinear magnetic energy,
    externally clocked co-simulation, or construction-order-independent
    references.

The ADR equations, matrix signs, and DC policy must be copied into focused
tests. If implementation evidence forces any of these decisions to change,
update and review the ADR before changing the production stamp.

## Planned files

```text
src/SpiceSharpMechanical2D/Coupling/DcMotorCoupler2D.cs
src/SpiceSharpMechanical2D/Coupling/DcMotorCoupler2DParameters.cs
src/SpiceSharpMechanical2D/Coupling/DcMotorCoupler2DBehavior.cs
src/SpiceSharpMechanical2D/Coupling/IDcMotorCoupler2DBehavior.cs
src/SpiceSharpMechanical2D.Tests/Coupling/DcMotorStampTests.cs
src/SpiceSharpMechanical2D.Tests/Coupling/DcMotorTransientTests.cs
src/SpiceSharpMechanical2D.Tests/Coupling/DcMotorEnergyTests.cs
src/SpiceSharpMechanical2D.Tests/Coupling/DcMotorMechanismTests.cs
docs/architecture/ADR-0008-reciprocal-electromechanical-coupling.md
```

## Verification report

Use:

```text
docs/verification/phase-12.md
```

or, when using the coupling-first roadmap fork after Phase 6:

```text
docs/verification/coupling-01.md
```

The report must include exact SpiceSharp API evidence, the final sparse stamp,
all sign conventions, operating-point behavior, state-space parameters,
timestep refinement, instantaneous and integrated power residuals, sample
outputs, allocations, and confirmation that no skipped phase was implemented.

## Planning evidence inspected

The detailed plan above is grounded in:

```text
pinned SpiceSharp 3.2.3 XML documentation:
    Component<T>
    ComponentBindingContext / IComponentBindingContext.Nodes
    IBranchedBehavior<double>.Branch
    IVariableFactory.GetSharedVariable
    IVariableFactory.CreatePrivateVariable
    IBiasingSimulationState.Map
    ElementSet<double>
    ITimeSimulationState.UseDc
    IAcceptBehavior

repository examples:
    src/SpiceSharpParser.CustomComponents/NonlinearInductor.cs
    src/SpiceSharpParser.CustomComponents/NonlinearInductors/
        NonlinearInductorVariables.cs
        Biasing.cs
        Time.cs
    src/SpiceSharpMechanical2D/Bodies/IRigidBody2DBehavior.cs
    src/SpiceSharpMechanical2D/Forces/RigidBodyLoadBehavior.cs
    src/SpiceSharpMechanical2D/Joints/JointBehaviorBase.cs
```

Stop after Phase 12, or after the separately authorized coupling-first
milestone.

---

# 20. Phase 13 — Exports, diagnostics, documentation, and packaging

## Goal

Make the package usable without changing the numerical model.

## Implement exports

Provide stable export/query helpers for:

### Coordinate

```text
Position
Velocity
Acceleration if reliably computed
KineticEnergy
```

### Body

```text
PositionX
PositionY
Angle
VelocityX
VelocityY
AngularVelocity
KineticEnergy
```

### Spring/joint

```text
Length
Extension
Force
Torque
StoredEnergy
DissipatedPower
PositionError
VelocityError
```

### Contact

```text
Gap
SmoothedPenetration
NormalForce
TangentialForce
SlipVelocity
NormalPower
FrictionPower
```

### Cam

```text
ProfileParameter
Lift
Gap
ContactForce
CamReactionTorque
```

## Documentation

Create:

```text
README.md
docs/architecture/overview.md
docs/equations/generalized-coordinate.md
docs/equations/rigid-body.md
docs/equations/springs-and-joints.md
docs/equations/smooth-contact.md
docs/equations/cam-follower.md
docs/limitations.md
docs/numerical-guidance.md
```

`docs/limitations.md` must prominently state:

- contacts are explicit;
- contacts are compliant;
- no exact rigid-body impulses;
- no exact static friction;
- joint constraints are compliant;
- large stiffness requires small timesteps;
- output contact force depends on the compliant model and numerical resolution;
- this is not a game engine.

## Packaging

- package metadata;
- XML documentation;
- nullable reference types;
- deterministic build if repository supports it;
- symbol package if repository conventions use one;
- license compliance;
- no parser dependency;
- no dependency on a game or geometry engine.

## Acceptance gate

All samples run.

Public APIs have XML documentation.

No analyzer warnings introduced unless documented and justified.

## Verification report

```text
docs/verification/phase-13.md
```

Stop after Phase 13.

---

# 21. Phase 14 — Robustness, regression corpus, and performance baseline

## Goal

Establish confidence before declaring version 1.0.

Do not add new physics features.

## Regression scenes

Include at least:

```text
1D oscillator
free fall
projectile
damped pendulum
double pendulum
slider-crank
four-bar
circle-ground settling
circle-ground impact
circle-circle impact
sliding contact
lift-law cam follower
geometric roller cam follower
DC-motor-driven rotor
DC-motor-driven slider-crank
DC-motor-driven cam follower (only when Phases 10-11 are implemented)
```

## Randomized tests

Use deterministic seeds.

Generate valid parameter combinations within documented ranges for:

- body mass and inertia;
- spring stiffness and damping;
- anchor locations;
- contact stiffness and damping;
- friction coefficient;
- cam speed and follower preload.

Reject invalid generated scenes rather than silently clipping parameters.

Assertions:

- no NaN or infinity;
- simulation either completes or fails with a categorized convergence exception;
- internal force pairs balance;
- dissipative elements do not generate net positive power beyond tolerance;
- deterministic repeated run;
- exported values remain finite.

## Performance baseline

Measure, but do not optimize prematurely:

```text
1 body
10 bodies with explicit springs
25 bodies with explicit springs
10 explicit contacts
cam follower for 10,000 accepted points
```

Report:

- wall time;
- allocated bytes if available;
- accepted time points;
- rejected points if available;
- Newton iteration count if available;
- peak memory if available.

Do not promise real-time performance.

## Release gate

Version 1.0 may be proposed only when:

- all earlier phase reports exist;
- all regression scenes pass;
- all nonlinear components have Jacobian tests;
- limitations are documented;
- no tolerance was weakened without recorded evidence;
- no hidden custom simulation or solver was added;
- a clean clone can restore, build, test, and run samples.

## Verification report

```text
docs/verification/phase-14.md
```

Stop after Phase 14.

---

# 22. Release checkpoints

```text
0.1-coordinate
    Phase 2 complete

0.2-rigid-body
    Phase 4 complete

0.3-mechanisms
    Phase 6 complete

0.4-smooth-contact
    Phase 9 complete

0.5-cam
    Phase 11 complete

0.6-electromechanical
    Phase 12 complete

1.0
    Phase 14 complete
```

Do not publish a checkpoint if its phase report is incomplete.

For the coupling-first fork after Phase 6, do not reserve or publish empty
contact/cam checkpoints. Publish an explicitly named
`0.4-electromechanical` checkpoint only after `coupling-01.md` is complete;
the sequential roadmap keeps the `0.6-electromechanical` name above. This is a
release-label fork, not a claim that Phases 7-11 were completed.

---

# 23. Verification report template

Each `docs/verification/phase-XX.md` should follow:

```markdown
# Phase XX verification

## Scope implemented

## Explicitly not implemented

## SpiceSharp API used

## Files changed

## Commands executed

## Test summary

## Numerical cases

### Case name
- Parameters:
- Maximum timestep:
- Integration method:
- Expected result:
- Measured result:
- Absolute error:
- Relative error:
- h / h/2 / h/4 results:

## Jacobian verification
- Component:
- State:
- Analytic:
- Finite difference:
- Maximum absolute mismatch:
- Maximum relative mismatch:

## Energy or power checks

## Determinism check

## Performance observations

## Known limitations

## Decision
PASS / FAIL

## Confirmation
No work from a later phase was implemented.
```

---

# 24. Reusable Codex prompt for every phase

Use this prompt and replace the phase number and name:

```text
Implement only Phase <N> — <PHASE NAME> from the attached
SpiceSharpMechanical2D implementation plan.

Hard constraints:
- Use the existing SpiceSharp Transient simulation.
- Do not create a custom simulation type.
- Do not create a custom global solver or time integrator.
- Do not implement work assigned to later phases.
- Use only the exact SpiceSharp APIs verified for this repository.
- Preserve all existing behavior and tests.
- Use double precision throughout.
- Add analytic Jacobians and finite-difference tests for every nonlinear
  equation introduced in this phase.
- Do not weaken an existing numerical tolerance without measured evidence
  and a written rationale.

Before coding:
1. Inspect the current repository state and previous phase verification report.
2. Confirm that all prerequisite phases are complete.
3. State the files and APIs you expect to modify.

Then:
1. Implement the phase.
2. Add all required automated tests.
3. Run restore, formatting verification, Release build, and the complete tests.
4. Run every numerical acceptance case specified by the phase.
5. Write docs/verification/phase-<NN>.md using the supplied template.
6. Report exact measured errors and commands.
7. Confirm that no later-phase work was implemented.
8. Stop.
```

---

# 25. First prompt to give Codex

```text
Implement Phase 0 — Repository baseline and exact SpiceSharp API proof from
the attached SpiceSharpMechanical2D implementation plan.

Do not implement mechanics.

Prove, using the repository's actual pinned SpiceSharp version, that an
ordinary custom component running under the existing Transient simulation can:

- allocate two private real solver variables;
- create and initialize transient derivative states;
- stamp two coupled differential equations;
- expose both states as queryable properties;
- resolve a linked entity behavior during behavior construction;
- run without reflection into private SpiceSharp internals.

Use the harmonic API-proof equations specified by Phase 0. Add the required
ADR, tests, and docs/verification/phase-00.md. Run the complete repository
build and tests, report exact measured results, confirm that no mechanics was
implemented, and stop.
```

---

# 26. Human review checklist before authorizing the next phase

Check all answers are **yes**:

```text
[ ] Did Codex implement only the requested phase?
[ ] Is ordinary SpiceSharp Transient still the only simulation?
[ ] Were no custom solver or integration method added?
[ ] Are all solver variables created during setup?
[ ] Are linked behaviors resolved before load loops?
[ ] Are nonlinear residual signs documented?
[ ] Are analytic Jacobians independently tested?
[ ] Are time-step refinement results reported?
[ ] Are energy, power, or action-reaction checks present where applicable?
[ ] Are tests deterministic?
[ ] Did the complete pre-existing test suite pass?
[ ] Does the phase verification report contain measured values?
[ ] Were no tolerances weakened without evidence?
[ ] Are limitations stated honestly?
[ ] Did Codex stop?
```

Authorize the next phase only if every applicable item passes.

---

# 27. Final technical boundary

This project is successful when it provides a verified library for **small, smooth, explicitly connected 2D mechanical systems inside SpiceSharp**.

The project must not gradually turn into a hidden rigid-body engine.

The correct boundary is:

```text
SpiceSharp owns
    transient stepping
    Newton iteration
    integration history
    sparse equation solving
    behavior lifecycle
    solver variables
    exports and properties

SpiceSharpMechanical2D owns
    mechanical equations
    rigid-body coordinate mapping
    force and torque stamps
    smooth springs and joints
    explicit compliant contacts
    regularized friction
    cam profile evaluation
    electromechanical coupling
    physics-specific tests and diagnostics
```

When a requested feature requires:

```text
dynamic contact topology
impact impulses
complementarity
persistent manifolds
broad phase
CCD
exact static friction
```

it belongs in a future custom simulation type or a dedicated physics engine, not in this package.

---

# 28. Review addendum — transferable lessons from `physics_engine`

**Review date:** 2026-07-10

The sibling `physics_engine` workspace project was reviewed for ideas that can
strengthen this plan. It is a real-time, three-dimensional, sequential-impulse
engine, so its solver implementation is not a source architecture for
`SpiceSharpMechanical2D`. The useful material is primarily its validation
strategy, mechanism oracles, invariance tests, energy accounting, and explicit
treatment of geometric degeneracies.

This addendum supplements Sections 1–27. It applies prospectively to Phases
6–14 and to Phase 14 regression coverage for completed components. It does not
reopen Phases 0–5, add a dependency on `physics_engine`, or authorize work from
a later phase during an earlier task.

## 28.1 Architectural filter

Transfer these ideas:

- structured setup issues with severity, category, entity name, and stable
  diagnostic code;
- cheap local validation followed by optional topology- and profile-level
  validation;
- analytic physical oracles rather than tests that only assert finite output;
- isolated component tests with unrelated forces and contacts disabled;
- sustained mechanism tests under load, reversal, and backdrive;
- conservative, dissipative, and externally driven energy-budget categories;
- deterministic repeated-run, symmetry, coordinate-frame, scale, and entity
  insertion-order checks;
- explicit reporting of characteristic frequency, damping ratio, geometric
  error, and timestep resolution;
- deterministic sampling of user-supplied laws over their declared domain;
- test speed categories and machine-readable test results for long regression
  runs.

Do not transfer:

- a custom world step or fixed-step loop;
- sequential impulses, PGS/TGS, warm starting, Baumgarte bias, NGS, shock
  propagation, position projection, or accumulated-impulse state;
- force, torque, impulse, velocity, or derivative clamps that silently change
  the requested equation;
- fallback normals or axes such as a fixed unit vector when geometry is
  degenerate;
- silent replacement of NaN or infinity with zero;
- sleep/wake systems, island solvers, dynamic contact topology, broad phase,
  manifolds, CCD, collision callbacks, or runtime constraint creation;
- rendering, GPU, scene-threading, or snapshot infrastructure;
- `float`-based OpenTK mathematics;
- hard unilateral branches, breakable wrappers, latches, or trigger state
  machines without a separately approved smooth formulation and phase.

SpiceSharp remains the only owner of transient stepping, Newton iteration,
integration history, sparse solving, and accepted/rejected timepoints.

## 28.2 Cross-phase verification layers

Every new physical component from Phase 6 onward should be verified at the
following layers where applicable:

1. **Equation law:** direct values, signs, units, limiting cases, passivity,
   and analytic Jacobian against an independent finite difference.
2. **Isolated transient:** one component in zero gravity or otherwise isolated
   from unrelated contacts and loads, compared with an analytic solution.
3. **Coupled mechanism:** the component in a small mechanism under a known
   static or dynamic load, including reverse drive when the law is reciprocal.
4. **Metamorphic invariance:** equivalent results under rigid translation,
   rigid rotation, mirror reflection, endpoint swap with mapped signs, and
   entity insertion-order changes.
5. **Resolution study:** at least `h`, `h/2`, and `h/4` for time-dependent
   acceptance cases and a parameter sweep for regularization or stiffness.
6. **Setup diagnostics:** invalid references and non-finite or degenerate
   parameters fail before transient loading; suspicious but valid scaling is
   reported without mutating the model.
7. **Sustained regression:** run for a physical duration stated in periods,
   revolutions, or settling times and compare against an oracle, not merely
   `IsFinite`.

Classify every integration scenario before asserting energy behavior:

```text
Conservative
    no damping, friction, contact damping, source, or prescribed motion
    assert bounded total-energy drift with timestep refinement

Dissipative
    passive damping or friction and no source
    assert nonpositive component power and a closing energy-loss budget

Driven
    applied force, torque, voltage source, prescribed motion, or preload work
    assert input work/power closes against stored-energy change and losses
```

The general integrated budget is:

\[
E(t)-E(0)-W_{external}(0,t)+E_{dissipated}(0,t)\approx0
\]

Use scale-aware absolute and relative tolerances and report the actual residual.
Do not assume that total mechanical energy must decrease in a driven case.

## 28.3 Phase 6 additions — compliant joints

### Reference-state semantics

Define deterministic reference state for every joint:

- `RevoluteJoint2D` has explicit local anchors. Initial anchor mismatch is an
  intentional preload and is never silently erased.
- `WeldJoint2D` and `PrismaticJoint2D` expose an explicit relative reference
  angle, or a clearly named construction mode that captures it from the entity
  initial states during behavior binding, before the operating point.
- An omitted reference value must not be inferred from a later Newton iterate.
- Tests cover zero preload, explicit preload, world anchoring, two-body
  anchoring, and reversed endpoint order.

Large initial anchor or angle preload is valid when finite, but should produce
a setup warning containing the joint name and estimated initial force/torque.

### Full rotating-guide formulation

Replace the abbreviated prismatic description in Phase 6 with the following
minimum formulation. For a guide axis attached to body A:

\[
a=R(\theta_A)a_l
\]

\[
n=\operatorname{perpendicular}(a)
\]

\[
d=p_B-p_A
\]

Normal error and free axial travel are:

\[
e_n=n\cdot d
\]

\[
s=a\cdot d
\]

The normal error rate is:

\[
\dot e_n=n\cdot(v_B^{point}-v_A^{point})
-\omega_A a\cdot d
\]

The second term is required because the guide normal rotates with body A. For
a world-fixed guide it is zero.

With `perpendicular(x,y)=(-y,x)`, the position derivatives include:

\[
\frac{\partial e_n}{\partial x_A}=-n,
\qquad
\frac{\partial e_n}{\partial x_B}=n
\]

\[
\frac{\partial e_n}{\partial\theta_A}
=-a\cdot d-n\cdot\operatorname{perpendicular}(r_A)
\]

\[
\frac{\partial e_n}{\partial\theta_B}
=n\cdot\operatorname{perpendicular}(r_B)
\]

For scalar compliant effort

\[
\lambda=k_ne_n+c_n\dot e_n
\]

stamp generalized load from the error Jacobian:

\[
Q=-J^T\lambda
\]

This includes the guide-orientation torque. Applying only a normal force at the
anchor is incomplete when the guide itself rotates. The production behavior
must include the full derivative of `Q`, including the state dependence of
`J`, `e_n`, and `dot e_n`.

### Energy, power, and reaction semantics

For the isotropic revolute compliance:

\[
U_{revolute}=\frac12 k_p(e_p\cdot e_p)
\]

\[
P_{dissipated}=c_p(e_v\cdot e_v)\ge0
\]

Weld energy adds:

\[
U_{angle}=\frac12k_\theta e_\theta^2
\]

Prismatic energy adds the constrained normal and relative-angle terms:

\[
U_{prismatic}=\frac12k_ne_n^2+\frac12k_\theta e_\theta^2
\]

`DissipatedPower` means positive energy loss rate. Mechanical power contributed
by the damper is its negative. Tests must cover:

- zero error and zero relative speed;
- force/torque as the negative gradient of stored energy;
- nonnegative dissipated power;
- zero net internal linear force;
- zero net internal world-origin moment;
- static support reaction approaching a known applied weight or torque;
- underdamped, near-critical, and overdamped isolated cases using effective
  mass or inertia;
- disturbance recovery and slow load reversal.

Diagnostics must be type-specific and define direction:

```text
Revolute
    AnchorError
    AnchorVelocityError

Prismatic
    NormalError
    NormalVelocityError
    AxialTravel
    RelativeAngleError

Weld
    AnchorError
    AnchorVelocityError
    RelativeAngleError

All joints
    ForceOnA / ForceOnB
    TorqueOnA / TorqueOnB
    StoredElasticEnergy
    DissipatedPower
```

Do not expose an unsigned or ambiguously named `ReactionForce`. Values must
represent a documented accepted transient timepoint, not the last Newton load.

### Joint setup validation

Reject during setup:

- missing or unresolved bodies;
- the same body used as both dynamic endpoints unless a specific joint law
  documents meaningful self-connection behavior;
- non-finite anchors, axes, coefficients, and reference values;
- a zero-length guide axis;
- negative stiffness or damping;
- world-to-world joints and other topologies with no dynamic row;
- a reference mode whose initial state cannot be resolved deterministically.

Warn, without modifying the model, about:

- large initial preload;
- a local anchor several declared characteristic body lengths from its center;
- duplicate parallel compliant joints;
- inconsistent initial geometry in a closed loop;
- a local natural period that is poorly resolved by maximum timestep.

Because the joints are compliant, duplicate or nominally over-constrained
topology is generally a conditioning warning, not automatically a hard error.

Mechanism tests must isolate joint behavior first, then test the slider-crank
and four-bar under payload, drive reversal, and at least one complete cycle
after initial transients. Report loop-closure error and power balance over the
cycle.

## 28.4 Phase 7 additions — geometry contract and degeneracy gate

### Shape data and orientation

Define local-space shape contracts before implementation:

```text
CircleShape2D(LocalCenter, Radius)
PlaneShape2D(LocalUnitNormal, LocalOffset)
SegmentShape2D(LocalStart, LocalEnd)
```

`PlaneShape2D` is an oriented, one-sided half-space boundary. State explicitly
whether `SegmentShape2D` is two-sided or has a configured front normal; do not
infer side from current body position. Geometry queries receive explicit body
poses and return world-space results.

Reject non-finite values, `Radius <= 0`, a zero plane normal, and a segment
below a documented scale-aware minimum length. Normalize valid configured axes
once during setup. Never substitute an arbitrary axis for invalid input.

### Result invariants and swap operation

Require:

\[
PointOnB-PointOnA=Gap\,Normal
\]

Define one authoritative swap operation:

```text
Swap(g) = (
    Gap: g.Gap,
    Normal: -g.Normal,
    PointOnA: g.PointOnB,
    PointOnB: g.PointOnA,
    mapped feature/status)
```

Define canonical ordering for each query. Reversed queries must call the swap
operation rather than independently recomputing signs.

Add deterministic property tests, with fixed seeds, for:

- rigid translation and rotation covariance;
- uniform geometric scaling;
- A/B swap identity;
- point-gap reconstruction;
- repeated-query bitwise determinism where the runtime permits it;
- analytic derivatives against finite differences while the same geometric
  feature remains active.

### Analytic derivative result

Phase 7 currently asks for finite-difference geometry derivatives but does not
define an analytic derivative API. Add either:

```text
ContactGeometryDerivatives2D
```

or equivalent internal derivative storage consumed directly by Phase 8. It
must cover the gap, normal, and contact-point derivatives with respect to all
participating body pose variables. Do not reconstruct these derivatives by
finite difference inside contact loading.

Return feature and differentiability status, for example:

```text
CirclePlane
CircleCircle
SegmentInterior
SegmentStart
SegmentEnd
FeatureBoundary
NormalUndefined
```

### Mandatory degeneracy decision before Phase 8

Coincident circle centers do not have a unique unit normal. A circle center
exactly on its closest segment point has the same problem. A closest point on a
hard finite segment also changes derivative branch at each endpoint.

Before Phase 8 begins, its ADR must choose and test one of these strategies for
each dynamic contact type:

1. a rotationally covariant smooth regularization with complete analytic
   derivatives and documented non-unit behavior near its scale;
2. a mathematically justified stateful normal based only on accepted history,
   with deterministic retry/rollback semantics proven against SpiceSharp; or
3. an explicit domain restriction that rejects or defers the degenerate or
   feature-switching case.

A fixed fallback direction may be returned only as diagnostic geometry marked
`IsDifferentiable = false`; it must not enter a production Newton force stamp.
Do not claim a unit-normal tolerance inside a regularized neighborhood where
the selected formulation is intentionally non-unit.

For circle-segment contact, either provide a smooth closest-feature blend or
limit the first dynamic component to a fixed smooth feature region. The hard
`clamp` used by ordinary closest-point-on-segment geometry is not by itself a
smooth Phase 8 force law.

Phase 8 is blocked until this degeneracy/feature-transition decision is
documented and its derivative contract is verified.

## 28.5 Phases 8–9 additions — contact scaling and passivity

### Exact-zero smooth contact activation

The normal-law proposal in Phase 8 must be revised before implementation. The
square-root positive part

\[
\frac12(x+\sqrt{x^2+\epsilon^2})
\]

is useful for regularization, but it is strictly positive for every finite
negative input. Applying another positive-part regularization to the final
normal force also produces a nonzero force when its input is zero. Used as
written in Phase 8, the law therefore has an infinite-range repulsive tail and
cannot be exactly inactive while separated.

For the first contact model, use a compact one-sided `C2` ramp such as:

\[
P_\epsilon(x)=
\begin{cases}
0,&x\le0\\
\epsilon(6t^3-8t^4+3t^5),\quad t=x/\epsilon,&0<x<\epsilon\\
x,&x\ge\epsilon
\end{cases}
\]

Its value, first derivative, and second derivative match at both boundaries.
The implementation may use branches because the resulting mathematical law is
`C2`; it must provide the exact analytic derivatives for each region.

This ramp changes compliance inside its activation interval. The static
`mg/k` oracle is valid only when the predicted equilibrium penetration lies in
the linear region, or else the test must compare against the exact inverse of
the configured ramp law. Report the ratio of equilibrium penetration to
`epsilon_delta`.

Use:

\[
\delta_+=P_{\epsilon_\delta}(-g)
\]

and a dimensionless `C2` activation:

\[
S_\epsilon(x)=
\begin{cases}
0,&x\le0\\
10t^3-15t^4+6t^5,\quad t=x/\epsilon,&0<x<\epsilon\\
1,&x\ge\epsilon
\end{cases}
\]

For the v1 linear contact law:

\[
F_{elastic}=k\delta_+
\]

\[
F_{damping}=cS_{\epsilon_\delta}(-g)
P_{\epsilon_v}(-v_n)
\]

\[
F_n=F_{elastic}+F_{damping}
\]

No outer positive-part or force clamp is needed: both terms are nonnegative,
and both are exactly zero for `g >= 0`. Damping is active only for closing
motion and penetration activation, so its two-body mechanical power satisfies:

\[
P_{damping}=F_{damping}v_n\le0
\]

Fix the v1 exponents to linear elastic response and ordinary viscous units.
Do not expose the original damping exponent `r`. A later Hertz-like law may be
added only with an explicitly named coefficient whose units are
`N/m^p`, a defined tangent stiffness, and separate tests. `NormalStiffness` in
`N/m` is dimensionally correct only for `p = 1`.

Add direct tests for exact zero force at every positive gap, `C2` boundary
matching, monotonic elastic force, nonpositive damping power, zero damping on
separation, and the full analytic Jacobian on each smooth region. Test close to
both ramp boundaries without centering a finite-difference stencil across a
piecewise boundary.

### Directional effective mass

Make the Phase 8 timestep diagnostic explicit. For a contact row along normal
`n` with center-to-contact lever arms `rA` and `rB`, the two-dimensional inverse
effective mass is:

\[
m_{eff}^{-1}=
m_A^{-1}+m_B^{-1}
+\frac{(r_A\times n)^2}{I_A}
+\frac{(r_B\times n)^2}{I_B}
\]

Omit world/static terms. Use the local tangent stiffness of the nonlinear
normal law at the current or estimated operating penetration:

\[
k_t=\frac{\partial F_{elastic}}{\partial\delta}
\]

Then report:

\[
\omega_n\approx\sqrt{k_t/m_{eff}}
\]

and, where a linearized damping coefficient is meaningful:

\[
\zeta\approx\frac{c_t}{2\sqrt{k_t m_{eff}}}
\]

The setup diagnostic should report estimated points per period at the
configured maximum timestep. It must never change the timestep, stiffness, or
damping automatically.

### Contact and friction verification

Add these tests to Phases 8 and 9:

- swap the two dynamic shapes and verify mapped force, torque, gap, and power;
- rigidly rotate the complete scene, including gravity and fixed geometry, and
  verify mapped trajectories;
- verify off-center effective mass against an independently derived impulse or
  acceleration response without implementing an impulse solver;
- verify elastic contact storage and damping loss in an isolated normal
  oscillator;
- sweep smoothing length, smoothing speed, stiffness, and timestep rather than
  validating only one tuned case;
- verify that increasing damping does not increase rebound energy;
- verify long sliding runs at low, nominal, and high slip speed;
- verify deterministic repeated trajectories on both sides of zero slip;
- verify that contact and friction never create net internal force or
  world-origin moment for a two-body pair.

Tangential slip velocity must use full surface-point velocity for both bodies:

\[
v_t=t\cdot
\left(v_B+\omega_B\operatorname{perpendicular}(r_B)
-v_A-\omega_A\operatorname{perpendicular}(r_A)\right)
\]

The tangent convention must be derived once from the oriented normal and map
consistently under A/B swap. Friction passivity is checked using the complete
point-force power, not center velocity alone.

For `RegularizedFrictionLaw`, also test:

- odd symmetry: `Ft(-vt) = -Ft(vt)`;
- origin slope: `dFt/dvt = -mu*Fn/vs` at zero slip;
- monotonic force magnitude as `abs(vt)` increases;
- `mu = 0` reproduces the Phase 8 normal-only trajectory;
- exact zero friction whenever normal contact is inactive;
- high-slip incline acceleration approaches
  `g*(sin(angle) - mu*cos(angle))` under the documented sign convention.

In v1, `ContactMaterial2D` is the already-combined material owned by one
explicit contact entity. Do not add implicit per-body material combination.
Validate every material value as finite, with positive normal stiffness,
nonnegative damping and friction coefficient, and strictly positive smoothing
scales. Exceptions include the contact entity name.

Do not import restitution impulses, static/dynamic friction pairs, manifold
warm starts, penetration slop, hard stick/slip branches, or force caps. Contact
instability must remain visible through convergence failure, resolution
studies, and setup guidance.

## 28.6 Phase 10 additions — lift-law validation and preload oracle

### Lift-law validation contract

Require `ICamLiftLaw.Period` to be finite and strictly positive. During setup,
sample the declared operating domain deterministically, including both sides
of the periodic seam, and validate:

- `s(theta)`, `s'(theta)`, and `s''(theta)` are finite and do not throw;
- position, first derivative, and second derivative are periodic;
- configured lift fits the follower's declared travel range;
- derivative and acceleration scales are reported for timestep guidance.

Retain runtime fail-fast checks because a user implementation can still be
stateful or invalid. Never replace invalid law output with zero, clamp a
derivative, or finite-difference a derivative in production.

Add tests for invalid/nonpositive period, non-finite values from each method,
exceptions, second-derivative seam mismatch, and validation that does not
mutate the law.

### Define the polynomial law

Define `PolynomialCamLiftLaw` as a `C2` rise–dwell–return law using normalized
3-4-5 motion for rise and return:

\[
q(u)=10u^3-15u^4+6u^5,
\qquad 0\le u\le1
\]

Segment spans must be finite, positive where motion occurs, nonoverlapping,
and sum to exactly one period under a deterministic boundary-ownership rule.
At every rise/dwell/return join, position is continuous and both velocity and
acceleration approach zero. Test `s`, `s'`, and `s''` at both sides of every
boundary and verify near-zero follower velocity during dwell.

### Quantitative preload/lift-off oracle

For prescribed cam angular speed and acceleration, the follower acceleration
implied by the lift law is:

\[
a_f=s''(\theta)\omega^2+s'(\theta)\dot\omega
\]

Use this to estimate the maximum separating inertial plus external load. Test
preload just below and just above the predicted contact-maintenance threshold,
with timestep refinement for the compliant transition. The sample must include
an explicit return spring and report contact-loss count as well as gap.

Exercise arbitrary periods and multi-lobe laws. A law with period `2*pi/n`
must produce exactly `n` lift cycles per shaft revolution. Run at least ten
revolutions without seam force/torque spikes and compare cycle-aligned output
after initial transients.

Add reciprocal-load tests: follower preload must increase cam reaction torque,
and a follower-side disturbance must transmit mechanical power back to the cam
with the virtual-work sign convention.

## 28.7 Phase 11 additions — profile admissibility and pressure angle

Before constructing geometric roller contact, validate:

- periodic closure of position, first derivative, and second derivative;
- finite samples and nonzero tangent magnitude;
- one documented winding and outward-normal convention;
- finite curvature;
- no ambiguous equal-distance branch in the declared operating range, or an
  explicit deterministic branch policy;
- regular roller-offset geometry, with `1 +/- rollerRadius*curvature` bounded
  away from zero according to the chosen normal convention;
- no cusp, undercut, or self-intersection in the contact offset used by the
  tested operating range.

Reverse parameterization must not change physical gap, outward normal, or
contact force. Uniform scale and rigid-transform tests must map closest point,
normal, curvature, and gap correctly.

Add geometric pressure-angle diagnostics:

\[
\alpha=\cos^{-1}(|n\cdot a_f|)
\]

where `a_f` is the follower-axis unit vector. Export instantaneous and maximum
pressure angle and test analytic circle/ellipse cases. A configurable design
warning may be emitted above a documented threshold, but it must never modify
the force law. Pressure angle belongs to geometric Phase 11; a lift law alone
does not contain enough geometry.

## 28.8 Phase 12 additions — reciprocal motor coupling

Use the Phase 12 decision: one ideal SI motor constant with numerically equal
torque and back-EMF constants. A future component may use separate `Kt` and
`Ke` only with an explicit conversion/efficiency model whose loss or gain
appears in the energy budget. Do not expose arbitrary unequal constants while
also claiming an ideal closing power balance.

Directly verify the off-diagonal coupled stamp. With the configured shaft sign,
the electrical back-EMF derivative and mechanical torque derivative must be
power-conjugate.

Add backdrive tests:

- open circuit: terminal voltage approaches `Ke*omega` with the documented
  polarity;
- resistive or shorted load: induced current creates torque opposing rotation;
- mechanical energy lost during backdrive closes against copper loss and any
  stored magnetic-energy change;
- reversing electrical polarity and reversing shaft direction map consistently.

Always extend the motor-driven Phase 6 slider-crank verification with unloaded
and loaded cases:

- payload force or inertia increases armature current;
- finite supply voltage produces loaded speed droop;
- current and torque ripple are phase-correlated with crank angle and slider
  force;
- integrated supply energy closes against copper loss, magnetic and kinetic
  energy changes, elastic storage, damping, and payload work.

Only when Phases 10-11 exist, also extend the motor-driven cam verification
with unloaded and preloaded cases:

- follower preload or mass increases armature current;
- finite supply voltage produces loaded speed droop;
- current and torque ripple are phase-correlated with cam slope and follower
  force;
- integrated supply energy closes against copper loss, magnetic and kinetic
  energy changes, elastic storage, damping, and contact dissipation.

Export at least:

```text
voltage,current,backEmf,electricalPower,shaftPower,copperLoss,
bodyAngle,bodyTorque,mechanismPosition,mechanismForce
```

The conditional cam sample additionally exports
`camAngle,camTorque,followerForce,gap`.

## 28.9 Phase 13 additions — setup validator and accepted diagnostics

Add an optional read-only setup validator over the explicit SpiceSharp entity
collection. It may accept a read-only validation context containing such
values as the configured maximum timestep, but it is not a world, solver,
force accumulator, or alternate setup lifecycle. It never changes parameters
or topology.

Suggested levels:

```text
Basic
    unresolved references
    NaN / infinity
    invalid signs and ranges
    degenerate axes, shapes, and world-to-world no-ops

Standard
    characteristic-frequency and timestep-resolution warnings
    suspicious regularization-to-length ratios
    large anchor lever arms when a characteristic length is declared
    duplicate components and initial contact overlap

Full
    compliant-joint topology and closed-loop consistency warnings
    cam-law domain and seam sampling
    geometric-cam regularity, curvature, and pressure-angle checks
    electromechanical passivity configuration
```

Each issue contains:

```text
StableCode
Severity: Error / Warning / Info
Category
EntityName or involved entity names
Message with values and units
Optional suggested corrective action
```

Errors represent configurations that cannot produce the documented equation.
Warnings represent conditioning, resolution, or unusual-but-valid modeling
choices. Tests must assert stable codes and structured fields rather than full
English message text.

Add characteristic diagnostics where mathematically defined:

- directional effective mass or inertia;
- local tangent stiffness;
- natural frequency and period;
- damping ratio;
- maximum timestep divided by local period;
- regularization divided by characteristic geometric scale;
- initial preload force/torque;
- cam maximum slope, acceleration, curvature, and pressure angle.

Diagnostics and exports must identify accepted-timepoint semantics. Do not
publish mutable last-Newton values as if they were accepted history. If the
SpiceSharp API cannot expose an accepted value reliably, omit that diagnostic
and record the limitation.

Define `EnergyDiagnostics2D` and `ResidualDiagnostics2D` rather than leaving
them as directory placeholders. They are opt-in and should remain
allocation-free in the load path. Where values are reliable, include:

```text
kinetic and stored elastic energy
elastic and damping portions of normal force
reversible elastic power
nonnegative damping and friction loss rates
action/reaction force residual
world-origin torque residual
first non-finite time, entity, and channel
normalized accepted-timepoint energy-balance residual
```

Do not label an internal SpiceSharp Newton residual as a physics residual unless
the supported public API exposes it with unambiguous iteration/timepoint
semantics.

## 28.10 Phase 14 additions — oracle-based regression matrix

For every regression scene, record:

```text
physical duration in periods, revolutions, or settling times
initial phase and initial preload
maximum timestep and at least one refined timestep
expected direction and analytic amplitude/frequency/ratio where available
maximum geometric error
peak force, torque, power, and current where applicable
contact-loss or lift-off count
cycle-to-cycle residual after settling
energy or power-budget residual
deterministic repeated-run comparison
```

Add a metamorphic matrix across representative components:

- rigid translation of the complete model;
- rigid rotation of model, gravity, planes, guide axes, and outputs;
- mirror reflection with mapped torque signs;
- A/B endpoint swap with mapped reactions;
- entity insertion-order permutation;
- uniform length/mass/stiffness scaling with analytically scaled expected
  frequency or trajectory;
- initial angles on both sides of every periodic seam;
- low, nominal, and high operating speed;
- low, nominal, and high mass ratio and lever arm;
- regularization and timestep sweeps around documented guidance boundaries.

Use accepted export trajectories to create a deterministic fingerprint for
test comparison if useful. Build the fingerprint from stable entity-name order
and explicitly quantized double values at fixed query times. On mismatch,
report the first differing entity, channel, time, and values. Promise only
same-runtime, same-configuration determinism unless cross-runtime evidence is
measured. Do not use reflection to snapshot or restore SpiceSharp private
integration or Newton state, and do not promise rollback.

For cams, include preload just below/above lift-off, multiple revolutions, and
two phase-offset followers on one shaft. The latter verifies that independent
component stamps add rather than overwrite one another and that shaft power
matches the sum of both branches.

For the motor, include stall, no-load, backdriven, and loaded speed-current
points plus the motor-driven rotor and slider-crank. Include the motor-driven
cam only when its prerequisite phases exist. For completed springs and joints,
include under-, near-critical-, and over-damped cases and slow-load/reversal
cases.

Classify tests by expected runtime, for example:

```text
Fast
    direct laws, Jacobians, validation, and short analytic transients

Integration
    mechanisms, multi-period convergence, and energy budgets

LongRunning
    full regression corpus, parameter sweeps, and performance baselines
```

Emit machine-readable test results for the long suite and use a documented
hang timeout. Keep failures reproducible by recording deterministic seed,
parameters, timestep, integration method, target framework, and commit.

Settling checks must use a tail window rather than one favorable final sample.
Report mean error or penetration, RMS velocity, peak-to-peak chatter, force
balance, and the window duration. Sweep both mass ratio and the dimensionless
resolution ratio

\[
\chi=h\sqrt{k_t/m_{eff}}
\]

through the documented operating range. When a deterministic randomized case
fails, print the full generated parameters and promote the minimized case to a
named fixed regression before release.

Performance reports must include both total allocation and steady-state
allocation after behavior construction. Report cost per accepted point in
addition to wall time, because rejected Newton/timepoint work may otherwise be
hidden.

## 28.11 Deferred candidates discovered by the review

The following concepts are potentially useful but are not added to the current
phase scope:

```text
GearCoupling2D
RackAndPinionCoupling2D
PulleyCoupling2D
smooth tension-only spring
progressive spring
bistable spring
joint limits, motors, latches, detents, and breakable elements
```

A compliant gear relation is a plausible future component:

\[
C=R\theta_A+\theta_B-C_0
\]

\[
\dot C=R\omega_A+\omega_B
\]

but it must receive a separate approved phase or plan revision with analytic
Jacobians, loop-consistency validation, power tests, and unbounded-angle
semantics. None of these candidates may be pulled into Phase 6 or Phase 14
merely because the reference project contains an impulse-based version.

## 28.12 Non-normative review evidence

The following sibling-project artifacts motivated this addendum. They are
review provenance only and are not package inputs or dependencies:

```text
physics_engine/src/PhysicsEngine/Diagnostics/SetupValidator.cs
physics_engine/src/PhysicsEngine/Diagnostics/ValidationRules.cs
physics_engine/src/PhysicsEngine/Dynamics/PrismaticConstraint.cs
physics_engine/src/PhysicsEngine/Dynamics/CamFollowerConstraint.cs
physics_engine/src/PhysicsEngine/Dynamics/ContactSolver.cs
physics_engine/docs/engine/reference/Effective-Mass-Computation.md
physics_engine/docs/engine/reference/Coupling-Constraints-Math.md
physics_engine/docs/general/mechanisms/motion-conversion/cams/Cam-Follower-Physics.md
physics_engine/tests/PhysicsEngine.Tests/Constraints/SpringDamperVerificationTests.cs
physics_engine/tests/PhysicsEngine.Tests/Collision/CollisionResponseTests.cs
physics_engine/tests/PhysicsEngine.Tests/Stability/AdvancedDiagnosticTests.cs
physics_engine/tests/PhysicsEngine.Tests/Stability/ContactTuningTests.cs
physics_engine/tests/PhysicsEngine.Tests/Stability/RobustnessInvarianceTests.cs
physics_engine/tests/PhysicsEngine.Tests/Stability/DeterministicReplayTests.cs
physics_engine/tests/PhysicsEngine.Tests/Validation/CamProfileValidationTests.cs
physics_engine/tests/PhysicsEngine.Tests/Validation/AnchorLeverArmTests.cs
physics_engine/tests/PhysicsEngine.Tests/Constraints/ConstraintMechanismVerificationTests.cs
physics_engine/tests/PhysicsEngine.Tests/Constraints/MechanismUnderLoadTests.cs
physics_engine/tests/PhysicsEngine.Tests/Mechanisms/RackAndPinionTests.cs
```

Where the reference project silently clamps, substitutes values, or uses an
iterative-impulse workaround, this plan deliberately keeps the equation visible
and requires either setup rejection, a smooth analytic regularization, or a
documented convergence failure.
