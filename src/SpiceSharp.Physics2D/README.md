# SpiceSharp.Physics2D

`SpiceSharp.Physics2D` adds smooth, two-dimensional mechanical components to
ordinary [SpiceSharp](https://github.com/SpiceSharp/SpiceSharp) transient
simulations.

It is useful when a mechanical model should use the same simulation lifecycle,
integration method, nonlinear iterations, and exports as a SpiceSharp circuit.
There is no separate physics update loop.

The current implementation covers the verified Phase 00–06 API:

- double-precision vectors, matrices, angles, and smooth helper functions;
- one-dimensional generalized mechanical coordinates;
- planar rigid bodies with translation and rotation;
- gravity, applied force, point force, torque, and drag;
- distance and rotational spring-damper connections;
- compliant revolute, weld, and prismatic joints;
- transient property exports and joint diagnostics.

Contact, collision detection, friction, cams, and electrical-mechanical
couplers are not implemented yet. This is a smooth, circuit-oriented mechanics
extension, not a real-time game physics engine.

## Quick start

From this repository, run the smallest motion example:

```powershell
dotnet run --project samples/SpiceSharp.Physics2D.Samples/Learning/02CoordinateCoasting
```

Then try gravity and a compliant pendulum:

```powershell
dotnet run --project samples/SpiceSharp.Physics2D.Samples/Learning/04Gravity
dotnet run --project samples/SpiceSharp.Physics2D.Samples/Learning/08RevolutePendulum
```

The complete learning path is described in the
[samples README](../../samples/SpiceSharp.Physics2D.Samples/Learning/README.md).

## A complete first simulation

This body starts 10 m above the origin and falls under gravity for one second:

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
        Console.WriteLine($"{simulation.Time:F2}, {y.Value:F4}, {velocityY.Value:F4}");
}
```

The important shape is always the same:

1. Create state-owning bodies or coordinates.
2. Create loads, connections, and joints that refer to them by entity name.
3. Create a standard SpiceSharp `Transient` simulation.
4. Define `RealPropertyExport` values before the run.
5. Run one `Circuit` containing every entity.

## Mental model

### State owners

`MechanicalCoordinate` owns one position `q`, one velocity `u`, and one
generalized mass. `RigidBody2D` owns six state values:

```text
position:          x, y, angle
velocity:          vx, vy, angular velocity
inertia parameters: mass, rotational inertia
```

Angles are unbounded counterclockwise radians. Positions and linear velocities
are expressed in the world frame.

### Loads

Loads are separate entities. For example, a `RigidBody2D` does not contain a
gravity property or force collection. `Gravity2D`, `AppliedForce2D`,
`PointForce2D`, `AppliedTorque2D`, `LinearDrag2D`, and `AngularDrag2D` each add
their contribution to the same SpiceSharp system.

### Anchors and connections

`MechanicalAnchor2D.World(point)` is fixed in world space.
`MechanicalAnchor2D.Body(bodyName, localPoint)` moves and rotates with a body.
Distance springs and joints connect two such anchors.

Connections apply equal-and-opposite internal loads. Joints are compliant:
they resist unwanted motion with stiffness and damping rather than imposing an
exact algebraic constraint.

## Implemented components

| Area | Types |
| --- | --- |
| Mathematics | `Vector2D`, `Matrix2x2D`, `AngleMath`, `SmoothFunctions` |
| State | `MechanicalCoordinate`, `RigidBody2D` |
| Loads | `Gravity2D`, `AppliedForce2D`, `PointForce2D`, `AppliedTorque2D`, `LinearDrag2D`, `AngularDrag2D` |
| Connections | `DistanceSpringDamper2D`, `RotationalSpringDamper2D` |
| Joints | `RevoluteJoint2D`, `WeldJoint2D`, `PrismaticJoint2D` |

## Common exports

Create exports before enumerating `simulation.Run(...)`:

```csharp
var x = new RealPropertyExport(simulation, body.Name, "x");
var y = new RealPropertyExport(simulation, body.Name, "y");
var angle = new RealPropertyExport(simulation, body.Name, "angle");
var vx = new RealPropertyExport(simulation, body.Name, "vx");
var vy = new RealPropertyExport(simulation, body.Name, "vy");
var omega = new RealPropertyExport(simulation, body.Name, "omega");
var energy = new RealPropertyExport(simulation, body.Name, "kineticenergy");
```

Useful joint diagnostics include:

```text
revolute: anchorerrorx, anchorerrory, storedelasticenergy, dissipatedpower
weld:     anchorerrorx, anchorerrory, relativeangleerror
prismatic: normalerror, axialtravel, axialvelocity, relativeangleerror
```

Read export values only at a matching export code, normally
`Transient.ExportTransient`.

## Modeling rules that matter

- Use consistent SI units: meters, seconds, kilograms, newtons, radians, and
  newton-meters.
- Give every entity a unique name.
- Put bodies in the `Circuit` before loads, connections, or joints that refer
  to them.
- Mass and rotational inertia must be finite and greater than zero.
- Stiffness and damping must be finite and nonnegative.
- A body-local force application point rotates with its body.
- `AppliedForce2D` acts through the center of mass; use `PointForce2D` when the
  force should also generate torque.
- Smaller timesteps are usually required as stiffness increases.
- Inspect joint errors. A compliant joint should have a small, converged error,
  not exactly zero error.

## Installation while developing this repository

Samples target .NET 8 and reference the project directly. In another project
inside the same checkout, use:

```xml
<ItemGroup>
  <ProjectReference Include="path/to/SpiceSharp.Physics2D.csproj" />
</ItemGroup>
```

The library itself targets `netstandard2.0` and `net8.0` and currently uses
SpiceSharp 3.2.3.

## Learn more

- [Getting-started tutorial](../../docs/tutorials/spicesharp-physics2d-getting-started.md)
- [Numbered learning samples](../../samples/SpiceSharp.Physics2D.Samples/Learning/README.md)
- [Feature gallery](../../samples/SpiceSharp.Physics2D.Samples/FeatureGallery/README.md)
- [Standalone samples](../../samples/SpiceSharp.Physics2D.Samples/README.md)
- [Architecture decisions](../../docs/architecture)
- [Implementation and verification plan](../../docs/implementation-plan.md)

## Status

This project is experimental. The implemented features are backed by analytic,
Jacobian, transient, mechanism, and energy-oriented tests, but the API may
still change before a stable release. See the Phase 00–06 reports in
[`docs/verification`](../../docs/verification) for measured evidence and known
limits.
