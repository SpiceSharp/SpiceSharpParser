# Learn SpiceSharpMechanical2D step by step

These lessons are intentionally repetitive. Each folder is a complete,
standalone console project with one short `Program.cs`. Nothing is hidden
behind a sample framework.

Read and run them in order:

| Lesson | Main idea |
| --- | --- |
| [01 Vector basics](01VectorBasics/Program.cs) | Vectors, rotation, dot product, and cross product |
| [02 Coordinate coasting](02CoordinateCoasting/Program.cs) | One generalized position and velocity |
| [03 Rigid body coasting](03RigidBodyCoasting/Program.cs) | Position, angle, linear velocity, and angular velocity |
| [04 Gravity](04Gravity/Program.cs) | A force entity changes body motion |
| [05 Push and spin](05PushAndSpin/Program.cs) | Independent force and torque |
| [06 Drag](06Drag/Program.cs) | Linear and angular damping |
| [07 Two-body spring](07TwoBodySpring/Program.cs) | Equal-and-opposite internal force |
| [08 Revolute pendulum](08RevolutePendulum/Program.cs) | A compliant pivot joint |
| [09 Welded body](09WeldedBody/Program.cs) | Position and angle held compliantly |
| [10 Prismatic slider](10PrismaticSlider/Program.cs) | One free direction and two guided directions |

Run a lesson from the repository root:

```powershell
dotnet run --project samples/SpiceSharpMechanical2D.Samples/Learning/01VectorBasics
dotnet run --project samples/SpiceSharpMechanical2D.Samples/Learning/04Gravity
dotnet run --project samples/SpiceSharpMechanical2D.Samples/Learning/10PrismaticSlider
```

## The recurring simulation pattern

Nearly every lesson after the vector introduction follows the same five
steps:

1. Create one or more mechanical entities, such as a body.
2. Create force, connection, or joint entities that refer to those bodies by
   name.
3. Create an ordinary SpiceSharp `Transient` simulation.
4. Create `RealPropertyExport` objects for the values you want to observe.
5. Run a `Circuit` containing the entities and read the final exports.

Property exports are sampled while `simulation.Run(...)` is being enumerated.
The lessons copy the values whenever the returned code is
`Transient.ExportTransient`, then print the last copied values after the run.
This is why the small `foreach` block is present even when a lesson only
prints one final result.

The body owns state and inertia. A force, connection, or joint adds its own
equations to the same SpiceSharp solve. There is no separate physics loop.

`Directory.Build.props` only supplies the common target framework and project
references. Every lesson's actual model and simulation are visible in its
single `Program.cs`.

## Mental model

- `MechanicalCoordinate` is one position/velocity pair. `RigidBody2D` is
  three such motions: world X, world Y, and angle.
- A body supplies inertia but does not contain a force list. `Gravity2D`,
  `AppliedForce2D`, drag, springs, and joints each add their own equations to
  the same solve.
- Connections and joints refer to bodies by entity name. Put body entities in
  the `Circuit` before the entities that reference them.
- Body-local anchor points rotate with the body. World anchors do not move.
- Phase 06 joints are compliant. A stiff revolute joint has a small anchor
  error; it is not an exact geometric constraint.
- Stiffer systems generally need smaller timesteps. The joint lessons use a
  smaller `MaxStep` than the free-motion lessons for that reason.
