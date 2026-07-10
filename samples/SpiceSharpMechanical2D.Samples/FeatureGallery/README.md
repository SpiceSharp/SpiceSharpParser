# SpiceSharpMechanical2D feature gallery

This project contains small, independent demos for the features implemented
through Phase 06. It intentionally does not use or preview later-phase APIs.

List the demos:

```powershell
dotnet run --project samples/SpiceSharpMechanical2D.Samples/FeatureGallery -- list
```

Run one demo:

```powershell
dotnet run --project samples/SpiceSharpMechanical2D.Samples/FeatureGallery -- projectile
dotnet run --project samples/SpiceSharpMechanical2D.Samples/FeatureGallery -- two-body-spring
dotnet run --project samples/SpiceSharpMechanical2D.Samples/FeatureGallery -- prismatic-slider
```

Run every demo, or run them without CSV output as a quick smoke check:

```powershell
dotnet run --project samples/SpiceSharpMechanical2D.Samples/FeatureGallery -- all
dotnet run --project samples/SpiceSharpMechanical2D.Samples/FeatureGallery -- smoke
```

Each simulation demo emits a short, decimated CSV trace suitable for plotting.
The existing `FreeFall`, `Pendulum`, `SliderCrank`, and `CompliantFourBar`
projects remain standalone, longer-form examples.
