# Analog Special-Function Subcircuit Library

`SpiceSharpParser.CustomComponents.Analog` provides portable functional models
for the analog LTspice A-device families. Load them independently from the
digital library:

```csharp
using SpiceSharpParser.CustomComponents.Analog;

AnalogSubcircuitLibrary analog = AnalogSubcircuitLibrary.LoadBuiltIn();
```

The assembly embeds `standard-analog.lib`, and the NuGet package copies the
same file to
`contentFiles/any/any/SpiceSharpParser.CustomComponents/Analog`.

## Included Components

| API | Subcircuit | Ordered pins | Function |
| --- | --- | --- | --- |
| `AddSampleHold` | `ANALOG_SAMPLE_HOLD` | INP, INN, CLK, SH, OUT, COM | Differential track or rising-edge sample |
| `AddOperationalTransconductanceAmplifier` | `ANALOG_OTA` | IN1N, IN1P, IN2P, IN2N, RAIL, OUT, COM | Four-quadrant transconductance stage |
| `AddVoltageControlledVaristor` | `ANALOG_VARISTOR` | CONTROL_P, CONTROL_N, OUT, COM | Bidirectional controlled clamp |
| `AddModulator` | `ANALOG_MODULATOR` | FM, AM, OUT, COM | MARK/SPACE oscillator with amplitude input |

These are analog or mixed-signal subcircuits built from ordinary SpiceSharp
entities. They do not parse native LTspice `A...` instance lines and do not
promise solver-identical waveforms.

## Behavior

### Sample/hold

When SH exceeds `REF`, the output tracks `V(INP,INN)`. With SH low, a rising
CLK crossing samples the differential input and holds it. `VHIGH` and `VLOW`
limit the stored result.

### Operational transconductance amplifier

The OTA combines the two differential pairs:

```text
RAW = (REF - V(IN1N,IN1P)) * V(IN2P,IN2N)
```

`LINEAR=1` selects direct `G*RAW + IOFFSET` operation. The default limited mode
uses `ISRC`, `ISINK`, and `ASYM` to shape the output current. The RAIL pin
exposes `VLOW`.

### Voltage-controlled varistor

The magnitude of `V(CONTROL_P,CONTROL_N)` sets the positive and negative clamp
level. Beyond either level, `RCLAMP` determines the incremental resistance;
inside the window, `ROFF` supplies leakage.

### Modulator

FM interpolates linearly from `SPACE` at 0 V to `MARK` at 1 V. AM sets the
sine-wave amplitude. Values outside the 0-to-1 V FM range extrapolate the
frequency rather than clamp it.

## Parameters and Defaults

| Subcircuit | Parameters and defaults |
| --- | --- |
| `ANALOG_SAMPLE_HOLD` | `REF=0.5 VHIGH=10 VLOW=-10 TPD=0 RIN=1G ROUT=1k COUT=500f RTRACK=1 RHOLD=1T CMEM=10p` |
| `ANALOG_OTA` | `G=1 REF=0 IOUT=10u ISRC=10u ISINK=-10u IOFFSET=0 POWERUP=1 ASYM=0 LINEAR=0 ROUT=1T COUT=1p VHIGH=2 VLOW=0 RCLAMP=1` |
| `ANALOG_VARISTOR` | `RCLAMP=1 ROFF=1T COUT=1p` |
| `ANALOG_MODULATOR` | `MARK=1k SPACE=1k ROUT=1 COUT=1p CPHASE=1p` |

Parameter overrides use the same dictionary convention as other subcircuit
facades:

```csharp
analog.AddModulator(
    circuit,
    "XVCO",
    "fm",
    "am",
    "out",
    "0",
    new Dictionary<string, string>
    {
        ["SPACE"] = "1k",
        ["MARK"] = "2k",
    });
```

## LTspice A-device Compatibility

The optional `LTspiceADeviceCompatibilityGoldenTests` suite runs native
LTspice A-devices and these portable subcircuits with the same stimuli. Its
analog cases cover `SAMPLEHOLD`, `OTA`, `VARISTOR`, and `MODULATOR`. Set
`LTSPICE_EXE` to the LTspice executable path to enable the tests; they are
skipped when it is unset.
