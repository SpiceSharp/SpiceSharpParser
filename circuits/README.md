# SpiceSharpParser Circuit Cookbook

This cookbook contains complete circuits that solve recognizable design
problems using the existing SpiceSharpParser feature set. Every cookbook entry
includes a runnable netlist, named measurements, plot requests, a schematic,
documented design calculations, and automated regression coverage.

## Cookbook Circuits

| Circuit | Track | Analyses | Difficulty | Main lesson |
| --- | --- | --- | --- | --- |
| [PWM digital-to-analog converter](cookbook/pure-spice/pwm-dac/README.md) | Pure SPICE | TRAN | Simple | Duty-cycle averaging, ripple attenuation, and settling |
| [RC sensor filter and input protector](cookbook/pure-spice/rc-sensor-input-filter/README.md) | Pure SPICE | AC, TRAN | Simple | Noise filtering, bandwidth, rail clamps, and fault current |
| [BJT relay driver with flyback diode](cookbook/pure-spice/bjt-relay-driver/README.md) | Pure SPICE | TRAN | Simple | Inductive-load switching, saturation, flyback clamping, and release decay |
| [Rectifier power supply](cookbook/pure-spice/rectifier-power-supply/README.md) | Pure SPICE | TRAN | Medium | Rectification, reservoir ripple, Zener regulation, and a load step |
| [BJT audio preamplifier](cookbook/pure-spice/bjt-audio-preamplifier/README.md) | Pure SPICE | OP, AC, TRAN, FOUR | Medium | Biasing, gain, bandwidth, coupling, loading, and harmonic distortion |
| [Active anti-alias filter and ADC buffer](cookbook/pure-spice/active-anti-alias-filter/README.md) | Pure SPICE | AC, TRAN | Medium | Buffered filter poles, stopband attenuation, settling, and ADC loading |
| [Ideal-diode redundant power input](cookbook/custom-components/ideal-diode-power-or/README.md) | CustomComponents | TRAN | Medium | Supply OR-ing, automatic failover, handback, and reverse-current blocking |
| [Simple phase-locked loop](cookbook/custom-components/simple-pll/README.md) | CustomComponents | TRAN | Difficult | Phase detection, loop filtering, and voltage-controlled oscillation |
| [Transmission-line termination](cookbook/pure-spice/transmission-line-termination/README.md) | Pure SPICE | TRAN | Difficult | Propagation delay, reflections, ringing, and impedance matching |

## Other Runnable Examples

The following directories remain valuable device demonstrations and
implementation-validation fixtures:

- [LTspice A-device examples](a-devices/README.md)
- [Digital routing milestone](digital-milestone-a/documentation.md)
- [Digital clocked-state milestone](digital-milestone-b/documentation.md)

Cookbook circuits are organized by application. Milestone and A-device
examples are organized by the library feature they validate.

## Common Contract

A cookbook circuit must:

- use existing public APIs and supported syntax;
- compile without errors and complete every simulation;
- expose important behavior through `.MEAS` statements;
- request the waveforms discussed in its guide with `.SAVE` and `.PLOT`;
- document its calculations, assumptions, experiments, and model boundary;
- pass the data-driven `CircuitCookbookTests` regression suite.

Run the fast documentation, layout, and generated-asset audit from the
repository root:

```powershell
.\tools\cookbook-schematic\.venv\Scripts\cookbook-report
```

Then run the simulation regression suite:

```powershell
dotnet test src/SpiceSharpParser.Tests/SpiceSharpParser.Tests.csproj `
  --filter 'FullyQualifiedName~CircuitCookbookTests'
```

The pure-SPICE track needs only `SpiceSharp-Parser`. The custom-components
track also requires `SpiceSharpParser.CustomComponents` and explicitly enables
its reader mappings.
