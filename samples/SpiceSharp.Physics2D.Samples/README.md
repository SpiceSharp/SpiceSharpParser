# SpiceSharp.Physics2D samples

The samples cover the implemented Phase 00–06 API. They use ordinary
SpiceSharp `Transient` simulations and do not depend on any later phase.

## Start here: numbered learning path

If you are learning the library, begin with the
[Learning](Learning/README.md) folder. It contains ten standalone projects,
ordered from a single vector through forces, springs, and joints. Every lesson
has one short `Program.cs` and deliberately repeats the normal SpiceSharp
setup so the execution model stays visible.

```powershell
dotnet run --project samples/SpiceSharp.Physics2D.Samples/Learning/01VectorBasics
dotnet run --project samples/SpiceSharp.Physics2D.Samples/Learning/04Gravity
dotnet run --project samples/SpiceSharp.Physics2D.Samples/Learning/08RevolutePendulum
```

## Feature gallery

The [FeatureGallery](FeatureGallery/README.md) project contains 14 short,
independently selectable demos:

| Demo | Feature |
| --- | --- |
| `api-oscillator` | Two-state transient extension API proof |
| `math-tour` | `Vector2D`, angle helpers, and smooth functions |
| `coordinate-coast` | Generalized mechanical coordinate |
| `rigid-body-coast` | Free rigid-body translation and rotation |
| `projectile` | Gravity and projectile motion |
| `time-force` | Deterministic time-dependent force |
| `drag-decay` | Linear and angular drag |
| `point-force` | Off-center force and generated torque |
| `two-body-spring` | Distance spring-damper between two bodies |
| `torsional-spring` | Smooth periodic rotational spring-damper |
| `revolute-pendulum` | Compliant revolute joint and diagnostics |
| `weld-fixture` | Compliant weld under force and torque |
| `prismatic-slider` | Free axial motion with normal/angle guidance |
| `rotating-guide` | Prismatic guide attached to a rotating body |

```powershell
dotnet run --project samples/SpiceSharp.Physics2D.Samples/FeatureGallery -- list
dotnet run --project samples/SpiceSharp.Physics2D.Samples/FeatureGallery -- point-force
dotnet run --project samples/SpiceSharp.Physics2D.Samples/FeatureGallery -- all
```

## Standalone samples

- [FreeFall](FreeFall/Program.cs) emits a full free-fall trajectory.
- [Pendulum](Pendulum/Program.cs) demonstrates a Phase 05 compliant spring
  pivot.
- [SliderCrank](SliderCrank/Program.cs) demonstrates a driven, loaded Phase 06
  mechanism.
- [CompliantFourBar](CompliantFourBar/Program.cs) demonstrates a closed-loop
  four-bar mechanism.

Every simulation sample emits invariant CSV so its output can be redirected
to a file and plotted with the tool of your choice.
