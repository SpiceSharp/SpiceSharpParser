# Easy LTspice A-device Examples

SpiceSharpParser can translate supported native LTspice `A...` lines into the
portable models supplied by `DigitalSubcircuitLibrary` and
`AnalogSubcircuitLibrary`.

## Enable A-device Parsing

Install `SpiceSharpParser.CustomComponents`, select LTspice compatibility, and
enable the custom reader mappings:

```csharp
using SpiceSharpParser.CustomComponents;

var options = new SpiceCompileOptions
{
    Dialect = SpiceDialect.LTspice,
    ConfigureReader = settings => settings.UseCustomComponents(),
};
```

Every native A-device line has eight terminal positions followed by its model:

```text
A<name> n1 n2 n3 n4 n5 n6 n7 n8 <model> [parameter=value ...] [flag ...]
```

Unused terminals are written as `0`. Model and parameter names are
case-insensitive.

## Digital Examples

### SR flip-flop

`SET` drives `Q` high and `RESET` drives it low. The complementary output is
`QB`.

```spice
* Set Q, then reset it
VSET set 0 PULSE(0 5 10n 100p 100p 2n 100n)
VRESET reset 0 PULSE(0 5 30n 100p 100p 2n 100n)

ASR set reset 0 0 0 qb q 0 SRFLOP Vhigh=5 Vlow=0 Td=1n
RQ q 0 10k
RQB qb 0 10k

.tran 100p 45n 0 100p UIC
.meas tran q_after_set FIND V(q) AT=20n
.meas tran q_after_reset FIND V(q) AT=40n
.end
```

`Q_AFTER_SET` is approximately 5 V and `Q_AFTER_RESET` is approximately 0 V.

### D flip-flop

`Q` captures `DATA` on each rising edge of `CLOCK`. `PRESET` and `CLEAR` are
active high, so they are tied low here.

```spice
* First clock edge captures high; second captures low
VDATA data 0 PULSE(0 5 5n 100p 100p 17n 40n)
VCLOCK clock 0 PULSE(0 5 10n 100p 100p 5n 20n)
VPRESET preset 0 0
VCLEAR clear 0 0

ADFF data 0 clock preset clear qb q 0 DFLOP Vhigh=5 Vlow=0 Td=1n
RQ q 0 10k
RQB qb 0 10k

.tran 100p 40n 0 100p UIC
.meas tran q_after_first_edge FIND V(q) AT=15n
.meas tran q_after_second_edge FIND V(q) AT=35n
.end
```

The two measurements are approximately 5 V and 0 V.

### Counter

This counter repeats every four rising clock edges. `DUTY=0.5` keeps `Q` high
for half of each count cycle.

```spice
* Divide the input clock by four
VCLOCK clock 0 PULSE(0 5 5n 100p 100p 2n 10n)
VRESET reset 0 0

ACOUNT clock reset 0 0 0 qb q 0 COUNTER cycles=4 duty=0.5 Vhigh=5 Vlow=0
RQ q 0 10k
RQB qb 0 10k

.tran 100p 42n 0 100p UIC
.meas tran q_at_10n FIND V(q) AT=10n
.meas tran q_at_30n FIND V(q) AT=30n
.end
```

Plot `V(clock)`, `V(q)`, and `V(qb)` to see the divide-by-four sequence.

### Phase detector

The phase detector sources current when `A` leads `B` and sinks current when
`B` leads `A`. A resistor converts that current into an easy-to-view voltage.

```spice
* Convert phase error into an output voltage
VA a 0 PULSE(0 1 10n 100p 100p 2n 50n)
VB b 0 PULSE(0 1 20n 100p 100p 2n 30n)

APD a b 0 0 0 0 out 0 PHASEDET Iout=1m Vhigh=10 Vlow=-10
ROUT out 0 1k

.tran 100p 75n 0 100p UIC
.meas tran out_a_leads FIND V(out) AT=15n
.meas tran out_b_leads FIND V(out) AT=55n
.end
```

With `IOUT=1 mA` and `ROUT=1 kOhm`, the active source and sink levels are close
to +1 V and -1 V.

## Analog and Mixed-signal Examples

### Sample-and-hold

With terminal 4 tied low, the device captures its differential input when
`SAMPLE` rises. The input keeps ramping while `HELD` retains the sampled value.

```spice
* Capture a ramp at 10 us
VIN input 0 PWL(0 0 20u 2 40u 4)
VSAMPLE sample 0 PULSE(0 1 10u 10n 10n 1u 100u)

ASH input 0 sample 0 0 0 held 0 SAMPLEHOLD Rout=100
RLOAD held 0 100k

.tran 100n 30u 0 100n UIC
.meas tran input_later FIND V(input) AT=25u
.meas tran held_later FIND V(held) AT=25u
.end
```

At 25 us, `INPUT` is about 2.5 V while `HELD` remains near 1 V.

### Operational transconductance amplifier

In `Linear` mode, the OTA multiplies two differential input voltages and
converts the result into output current. The load resistor converts that
current into voltage.

```spice
* 0.1 V differential input multiplied by 1 V
VIN1N in1n 0 0
VIN1P in1p 0 0.1
VIN2P in2p 0 1
VIN2N in2n 0 0

AOTA in1n in1p in2p in2n 0 rail out 0 OTA G=1m Linear Vhigh=5 Vlow=-5
RLOAD out 0 10k

.tran 10n 1u UIC
.meas tran output FIND V(out) AT=500n
.end
```

This configuration produces approximately 1 V at `OUT`.

### Voltage-controlled varistor

The control voltage sets the clamp magnitude. Here a 2 V control limits an
output driven from a 10 V supply.

```spice
* Clamp OUT near the 2 V control level
VCONTROL control 0 2
VSUPPLY supply 0 10
RDRIVE supply out 1k

AVAR control 0 0 0 0 0 out 0 VARISTOR Rclamp=10

.tran 10n 1u UIC
.meas tran clamped_output FIND V(out) AT=500n
.end
```

`CLAMPED_OUTPUT` is slightly above 2 V because `RCLAMP` is finite.

### Frequency and amplitude modulator

`FM=0.5 V` selects the midpoint between `SPACE=1 kHz` and `MARK=2 kHz`, giving
a 1.5 kHz output. `AM=2 V` sets the sine-wave amplitude.

```spice
* 1.5 kHz sine wave with 2 V amplitude
VFM fm 0 0.5
VAM am 0 2

AMOD fm am 0 0 0 0 out 0 MODULATOR mark=2k space=1k Rout=1
RLOAD out 0 100k

.tran 1u 1m 0 1u UIC
.meas tran positive_peak FIND V(out) AT=166.6666667u
.end
```

`POSITIVE_PEAK` is approximately 2 V. `MODULATE` is accepted as an alias for
`MODULATOR`.

## Math and Physical Intuition

The A-device implementations are functional macromodels. They preserve the
useful logic, timing, current, clamping, and loading behavior without modeling
every transistor or semiconductor region inside a physical device.

### Digital voltage levels and loading

Let `q` be a Boolean state: 0 for low and 1 for high. Its ideal output voltage
is

$$
V_Q=V_{LOW}+q(V_{HIGH}-V_{LOW}).
$$

Without an explicit `Ref`, an input changes logic state at the midpoint:

$$
V_{REF}=V_{LOW}+0.5(V_{HIGH}-V_{LOW}).
$$

The finite output resistance and capacitance turn an ideal logic step into a
first-order electrical transition. With an external capacitive load,

$$
\tau\approx R_{OUT}(C_{OUT}+C_{LOAD}),
\qquad t_{10\%-90\%}\approx2.2\tau.
$$

This is the physical bridge between Boolean logic and a voltage that can drive
the rest of the circuit.

### SR flip-flop state

For reset-dominant active-high inputs,

$$
q_{n+1}=
\begin{cases}
0, & R=1,\\
1, & R=0\text{ and }S=1,\\
q_n, & R=0\text{ and }S=0,
\end{cases}
\qquad \overline q_{n+1}=1-q_{n+1}.
$$

A physical latch uses regenerative positive feedback to preserve state. The
macromodel stores that state directly; it does not model transistor-level
metastability.

### D flip-flop state

The state immediately after an event is

$$
q^+=
\begin{cases}
0, & CLEAR=1,\\
1, & CLEAR=0\text{ and }PRESET=1,\\
d, & \text{on a rising clock edge},\\
q, & \text{otherwise}.
\end{cases}
$$

A physical edge-triggered flip-flop is built from latches or a pulse-triggered
storage path. This model captures the event and applies `Td`, but it does not
calculate setup time, hold time, or metastability probability.

### Counter

Let `k` be the number of rising edges since reset, `N=CYCLES`, and

$$
H=\operatorname{clip}(\operatorname{round}(N\cdot DUTY),1,N-1).
$$

Then

$$
q(k)=
\begin{cases}
1, & k\bmod N < H,\\
0, & k\bmod N \ge H.
\end{cases}
$$

The output frequency and realized duty ratio are

$$
f_Q=\frac{f_{CLOCK}}{N},
\qquad D_Q=\frac{H}{N}.
$$

A physical counter stores state in interconnected flip-flops. The macromodel
stores a normalized count and regenerates `Q` and `QB`.

### Phase detector

Let `s=+1` while `A` leads, `s=-1` while `B` leads, and `s=0` after the
matching edge. The output current and resistively converted voltage are

$$
I_{OUT}=sI_{SET},
\qquad V_{OUT}\approx I_{OUT}R_{LOAD}.
$$

Here $I_{SET}$ is the magnitude configured by the device's `IOUT` parameter.
For a phase-error pulse of width $\Delta t$ in a repeating period `T`,

$$
\overline I_{OUT}\approx I_{SET}\frac{\Delta t}{T}.
$$

This is the charge-pump principle used in phase-locked loops: edge timing
error becomes charge delivered to a loop filter.

### Sample-and-hold

The sampled signal is the differential voltage

$$
v_{IN}=V(INP)-V(INN).
$$

While tracking, the memory capacitor approximately follows

$$
C_{MEM}\frac{dv_{MEM}}{dt}=
\frac{v_{IN}-v_{MEM}}{R_{TRACK}},
\qquad \tau_{TRACK}=R_{TRACK}C_{MEM}.
$$

During hold, leakage produces droop:

$$
\frac{dv_{MEM}}{dt}\approx
-\frac{v_{MEM}}{R_{HOLD}C_{MEM}},
\qquad \tau_{HOLD}=R_{HOLD}C_{MEM}.
$$

Physically, this is an idealized switch charging a hold capacitor. With the
default `RHOLD=1 TOhm` and `CMEM=10 pF`, the hold time constant is about 10 s,
so microsecond-scale droop is negligible.

### Operational transconductance amplifier

Define the four-quadrant input product

$$
u=(REF-V(IN1N,IN1P))V(IN2P,IN2N).
$$

In `Linear` mode,

$$
I_{OUT}=Gu+I_{OFFSET},
\qquad V_{OUT}\approx I_{OUT}R_{LOAD},
$$

until the output reaches `VLOW` or `VHIGH`. In the example, $u=0.1\,V^2$,
`G=1 mA/V²`, and `RLOAD=10 kOhm`; therefore $I_{OUT}=100\,\mu A$ and
$V_{OUT}\approx1\,V$.

Physically, an OTA converts differential voltage into current. This
four-quadrant form is also an analog multiplier: changing the sign of either
differential pair reverses the output current.

### Voltage-controlled varistor

Let

$$
V_C=|V(CONTROL_P,CONTROL_N)|,
\qquad v=V(OUT,COM).
$$

The current law is

$$
i(v)=
\begin{cases}
\dfrac{v-V_C}{R_{CLAMP}}, & v>V_C,\\[6pt]
\dfrac{v}{R_{OFF}}, & |v|\le V_C,\\[6pt]
\dfrac{v+V_C}{R_{CLAMP}}, & v<-V_C.
\end{cases}
$$

For the example above, Kirchhoff's current law gives

$$
\frac{10-v}{1000}=\frac{v-2}{10},
$$

so $v\approx2.079\,V$. A physical varistor becomes strongly conductive above
its voltage threshold. This model captures the bidirectional clamp slope, but
not heating, aging, or destructive surge energy.

### Frequency and amplitude modulator

Normalize the frequency-control voltage as

$$
m(t)=\frac{V(FM,COM)}{1\,V}.
$$

The instantaneous frequency, phase in cycles, and output voltage are

$$
f(t)=SPACE+(MARK-SPACE)m(t),
$$

$$
\phi(t)=\phi_0+\int_0^t f(\lambda)\,d\lambda,
\qquad
v_{OUT}(t)=V(AM,COM)\sin(2\pi\phi(t)).
$$

For `FM=0.5 V`, `SPACE=1 kHz`, and `MARK=2 kHz`, the frequency is
1.5 kHz. Its period is about $666.7\,\mu s$, so the positive peak occurs after
one quarter-period, about $166.7\,\mu s$. Physically, this is a
voltage-controlled oscillator followed by an amplitude multiplier.

## Compatibility Tests

`LTspiceADeviceCompatibilityGoldenTests` executes the same native netlist in
LTspice and SpiceSharpParser and compares the resulting `.MEAS` values:

```powershell
$env:LTSPICE_EXE = 'C:\Program Files\ADI\LTspice\LTspice.exe'
dotnet test src/SpiceSharpParser.Tests/SpiceSharpParser.Tests.csproj `
  --filter 'FullyQualifiedName~LTspiceADeviceCompatibilityGoldenTests'
```

