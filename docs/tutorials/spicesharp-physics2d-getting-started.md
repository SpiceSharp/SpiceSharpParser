# Getting started with SpiceSharp.Physics2D

This tutorial builds a small mechanical simulation one idea at a time. It
assumes basic C# knowledge but does not assume experience with a physics
engine.

By the end, you will understand how to create bodies, apply forces, connect
anchors, add a compliant joint, run a SpiceSharp transient analysis, and read
the result.

## 1. What is being simulated?

`SpiceSharp.Physics2D` expresses mechanical differential equations as
SpiceSharp behaviors. During a transient simulation, SpiceSharp solves the
mechanical positions and velocities along with every other unknown in its
system.

This means there are not two nested loops such as:

```text
run circuit step
copy values
run physics step
copy values back
```

Instead, bodies, forces, springs, and joints are entities in one `Circuit` and
participate in one transient solve.

The implemented mechanics are smooth and compliant. They are suitable for
small mechanisms, oscillators, pendulums, actuator prototypes, and educational
models. They are not currently a collision or contact engine.

## 2. Run an existing lesson

From the repository root:

```powershell
dotnet run --project samples/SpiceSharp.Physics2D.Samples/Learning/04Gravity
```

Expected output is approximately:

```text
final y  = 5.095
final vy = -9.81
```

The starting height is 10 m. After one second under `-9.81 m/s^2`, elementary
kinematics gives:

```text
y  = 10 + 0*t + 0.5*(-9.81)*t^2 = 5.095 m
vy = 0 + (-9.81)*t               = -9.81 m/s
```

That analytic result gives us a useful first check.

## 3. Create a rigid body

A `RigidBody2D` owns planar position and velocity state:

```csharp
using SpiceSharp.Physics2D.Bodies;
using SpiceSharp.Physics2D.Mathematics;

var body = new RigidBody2D(
    "falling-body",
    mass: 2.0,
    inertia: 0.2,
    initialPosition: new Vector2D(0.0, 10.0));
```

The constructor arguments mean:

| Argument | Meaning |
| --- | --- |
| `"falling-body"` | Unique SpiceSharp entity name |
| `mass: 2.0` | Translational mass in kilograms |
| `inertia: 0.2` | Rotational inertia in kg m² |
| `initialPosition` | Center-of-mass world position in meters |

Optional constructor arguments set the initial angle, linear velocity, and
angular velocity. Positive angle and angular velocity are counterclockwise.

Even if a particular example does not rotate, inertia must be finite and
greater than zero because every rigid body owns rotational state.

## 4. Add gravity

The body contains inertia, not a list of forces. Gravity is another entity:

```csharp
using SpiceSharp.Physics2D.Forces;

var gravity = new Gravity2D(
    "gravity",
    body.Name,
    new Vector2D(0.0, -9.81));
```

The gravity entity finds `falling-body` by name and applies `mass *
acceleration` to it. This separation is intentional: multiple load entities
can independently contribute to the same body equations.

Other available loads are:

- `AppliedForce2D` for a center-of-mass world force;
- `PointForce2D` for a force at a body-local point;
- `AppliedTorque2D` for a counterclockwise torque;
- `LinearDrag2D` for velocity-proportional force;
- `AngularDrag2D` for angular-velocity-proportional torque.

Use `PointForce2D` when the application point matters. A force through the
center of mass changes translation but creates no torque; an off-center force
usually changes both translation and rotation.

## 5. Configure the transient simulation

Mechanical components currently participate in transient analysis:

```csharp
using SpiceSharp.Simulations;
using SpiceSharp.Simulations.IntegrationMethods;

var simulation = new Transient("tran", new Trapezoidal
{
    InitialStep = 0.01,
    MaxStep = 0.01,
    StopTime = 1.0,
});
```

`StopTime` is the duration. `MaxStep` limits how far the integrator advances at
once. A small step resolves fast motion and stiff springs more accurately but
requires more work.

For a first model, choose a step that gives many points across the shortest
motion timescale. Then repeat with half the step. If the result changes
materially, refine again.

## 6. Export state

Create exports before running the simulation:

```csharp
using SpiceSharp;

var y = new RealPropertyExport(simulation, body.Name, "y");
var velocityY = new RealPropertyExport(simulation, body.Name, "vy");
```

Common body property names are:

| State | Export name |
| --- | --- |
| world position | `x`, `y` |
| unbounded angle | `angle` or `theta` |
| world velocity | `vx`, `vy` |
| angular velocity | `omega` |
| total kinetic energy | `kineticenergy` or `ke` |

The export object is a view of the current accepted simulation result. Read it
while enumerating `simulation.Run(...)`:

```csharp
foreach (int exportType in simulation.Run(new Circuit(body, gravity)))
{
    if (exportType != Transient.ExportTransient)
        continue;

    Console.WriteLine(
        $"t={simulation.Time:F2}, y={y.Value:F4}, vy={velocityY.Value:F4}");
}
```

The `Circuit` order matters for references: place state-owning bodies before
loads, connections, and joints that refer to them.

## 7. The complete program

```csharp
using SpiceSharp;
using SpiceSharp.Physics2D.Bodies;
using SpiceSharp.Physics2D.Forces;
using SpiceSharp.Physics2D.Mathematics;
using SpiceSharp.Simulations;
using SpiceSharp.Simulations.IntegrationMethods;

var body = new RigidBody2D(
    "falling-body",
    mass: 2.0,
    inertia: 0.2,
    initialPosition: new Vector2D(0.0, 10.0));

var gravity = new Gravity2D(
    "gravity",
    body.Name,
    new Vector2D(0.0, -9.81));

var simulation = new Transient("tran", new Trapezoidal
{
    InitialStep = 0.01,
    MaxStep = 0.01,
    StopTime = 1.0,
});

var y = new RealPropertyExport(simulation, body.Name, "y");
var velocityY = new RealPropertyExport(simulation, body.Name, "vy");

foreach (int exportType in simulation.Run(new Circuit(body, gravity)))
{
    if (exportType == Transient.ExportTransient)
    {
        Console.WriteLine(
            $"t={simulation.Time:F2}, y={y.Value:F4}, vy={velocityY.Value:F4}");
    }
}
```

## 8. Connect a body to the world

Connections use anchors. An anchor is either fixed in the world or attached to
a point expressed in a body's local coordinates:

```csharp
using SpiceSharp.Physics2D.Connections;

var worldPoint = MechanicalAnchor2D.World(new Vector2D(0.0, 0.0));
var bodyPoint = MechanicalAnchor2D.Body(body.Name, new Vector2D(0.0, 0.6));
```

If the body translates or rotates, `bodyPoint` follows it. `worldPoint` never
moves.

A distance spring can join two bodies or one body and the world:

```csharp
var spring = new DistanceSpringDamper2D(
    "spring",
    worldPoint,
    bodyPoint,
    restLength: 0.6,
    stiffness: 100.0,
    damping: 2.0,
    lengthRegularization: 1e-9);
```

The spring force acts along the line between its anchors. Damping acts on
relative speed along that line. The small positive length regularization keeps
the law finite if the anchors become coincident.

For a body-to-body spring, both ends are body anchors:

```csharp
var spring = new DistanceSpringDamper2D(
    "spring",
    MechanicalAnchor2D.Body(bodyA.Name, Vector2D.Zero),
    MechanicalAnchor2D.Body(bodyB.Name, Vector2D.Zero),
    restLength: 1.0,
    stiffness: 20.0,
    damping: 0.5);
```

The connection stamps equal-and-opposite loads. You should not create a second
spring in the reverse direction.

## 9. Build a pendulum with a revolute joint

A revolute joint keeps two anchor points near one another while allowing their
relative rotation to remain free:

```csharp
using SpiceSharp.Physics2D.Joints;

const double length = 0.6;
const double initialAngle = 0.25;
var localPivot = new Vector2D(0.0, length);

var pendulum = new RigidBody2D(
    "pendulum",
    mass: 1.0,
    inertia: 0.12,
    initialPosition: -localPivot.Rotate(initialAngle),
    initialAngle: initialAngle);

var pivot = new RevoluteJoint2D(
    "pivot",
    MechanicalAnchor2D.World(Vector2D.Zero),
    MechanicalAnchor2D.Body(pendulum.Name, localPivot),
    stiffness: 100_000.0,
    damping: 90.0);

var gravity = new Gravity2D(
    "gravity",
    pendulum.Name,
    new Vector2D(0.0, -9.81));
```

The expression `-localPivot.Rotate(initialAngle)` positions the center of mass
so the body-local pivot starts exactly at the world origin. Starting from
consistent geometry avoids an artificial initial spring impulse.

The joint is compliant, not mathematically exact. Export its anchor error:

```csharp
var errorX = new RealPropertyExport(simulation, pivot.Name, "anchorerrorx");
var errorY = new RealPropertyExport(simulation, pivot.Name, "anchorerrory");

double error = Math.Sqrt(
    (errorX.Value * errorX.Value) +
    (errorY.Value * errorY.Value));
```

A useful model has a small error that decreases predictably as stiffness and
timestep are refined. Expecting exact zero hides the actual approximation.

## 10. Choose a joint

| Joint | Motion resisted | Motion left free |
| --- | --- | --- |
| `RevoluteJoint2D` | Relative anchor translation | Relative rotation |
| `WeldJoint2D` | Relative anchor translation and angle | None, except compliance |
| `PrismaticJoint2D` | Guide-normal translation and relative angle | Guide-axis translation |

All three are penalty-style compliant joints. Their stiffness and damping are
physical model parameters, not merely solver settings.

The prismatic joint can optionally add axial stiffness and damping. Leaving
those values at zero makes axial travel free.

## 11. Stiffness, damping, and timestep

Suppose a body of effective mass `m` is held by stiffness `k`. Its approximate
undamped natural angular frequency is:

```text
omega_n = sqrt(k / m)
period  = 2*pi / omega_n
```

A stiffer joint has a shorter period and needs a smaller `MaxStep`. Increasing
stiffness without refining time can produce inaccurate motion or convergence
failure.

Damping reduces oscillation but does not remove the need to resolve the
system's fastest dynamics. A practical workflow is:

1. Start with moderate stiffness.
2. Choose damping appropriate for the desired physical behavior.
3. Run with timestep `h`.
4. Run again with `h/2`.
5. Compare positions, velocities, energies, and joint errors.
6. Increase stiffness only if the remaining compliance is unacceptable.

Do not fix a difficult model by blindly adding extreme stiffness or damping.

## 12. Initial conditions

Bodies and coordinates hold their requested initial position and velocity
during SpiceSharp's operating-point stage. Mechanical loads begin acting in
the transient stage.

For connected mechanisms, initialize bodies so their anchors and relative
angles already satisfy the intended geometry as closely as possible. This
reduces startup transients and makes failures easier to interpret.

The standalone slider-crank sample demonstrates the extra work needed to make
both initial positions and velocities kinematically consistent.

## 13. Common mistakes

### A referenced body cannot be found

Check spelling and entity order:

```csharp
// Correct: body before its load.
new Circuit(body, gravity);
```

Every entity name must be unique.

### A force does not rotate with the body

`AppliedForce2D` is a world-frame center-of-mass force. For a force at a local
point, use `PointForce2D`. Its force vector may be interpreted in world or
body-local coordinates using `ForceCoordinateSystem2D`.

### The joint moves slightly

That is expected. Joints are compliant. Inspect their error exports, refine
the timestep, and select stiffness based on the acceptable deflection.

### The solution becomes difficult after increasing stiffness

Reduce `MaxStep`, check initial geometry, and increase stiffness gradually.
Also verify that mass, inertia, damping, and geometry use a consistent scale.

### Export values look stale

Read them only while enumerating `simulation.Run(...)` and after checking the
returned export code.

### The model falls through an imagined floor

Contact and collision are not implemented. A world anchor or joint can attach
a body to the world, but there is no implicit ground plane.

## 14. Where to go next

Follow the numbered lessons in order:

1. `01VectorBasics`
2. `02CoordinateCoasting`
3. `03RigidBodyCoasting`
4. `04Gravity`
5. `05PushAndSpin`
6. `06Drag`
7. `07TwoBodySpring`
8. `08RevolutePendulum`
9. `09WeldedBody`
10. `10PrismaticSlider`

Then run the larger mechanism samples:

```powershell
dotnet run --project samples/SpiceSharp.Physics2D.Samples/SliderCrank
dotnet run --project samples/SpiceSharp.Physics2D.Samples/CompliantFourBar
```

The [Feature Gallery](../../samples/SpiceSharp.Physics2D.Samples/FeatureGallery/README.md)
also lets you select one focused feature by name.

For implementation evidence and numerical limitations, read the reports in
[`docs/verification`](../verification). Planned later features are described
in the [implementation plan](../implementation-plan.md), but planned APIs
should not be used as if they already exist.
