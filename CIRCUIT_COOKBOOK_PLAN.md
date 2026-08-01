# Circuit Cookbook Plan

## Direction

SpiceSharpParser already has a broad feature surface. The next phase should
focus on proving what users can build with the existing parser and
`SpiceSharpParser.CustomComponents`, rather than adding more parser features or
component families.

The intended outcome is an executable analog-electronics textbook and
laboratory: a curated cookbook of useful circuits in which physical intuition,
mathematical prediction, simulation, measurement, and real-world limitations
reinforce one another. The repository should teach users how circuits behave,
not merely show that SpiceSharpParser can run them.

## Existing Foundation

The repository contains the infrastructure needed for this direction:


- `SpiceSharpParser.AIExamples` contains 948 unique measured netlists. These
  provide candidates for human and AI review and promotion into the cookbook.
- `circuits/a-devices` demonstrates a strong example format: runnable
  netlists, `.SAVE`, `.PLOT`, `.MEAS`, explanatory guides, and automated
  verification.
- `circuits/cookbook` contains twelve complete application-oriented recipes
  across the pure-SPICE and CustomComponents tracks.
- `tools/cookbook-schematic` renders deterministic, accessible SVG schematics
  from a SPICE netlist plus a compact `schematic.toml` layout. Its validator
  keeps displayed component and node names synchronized with the netlist.
- `tools/cookbook-report` provides a read-only cookbook quality gate. It checks
  required documentation, analysis and output directives, measurement-table
  coverage, layout topology, SVG accessibility, and generated schematic
  freshness. It can emit stable JSON for CI or another AI tool.
- Repository-local `cookbook-schematic` and `cookbook-report` skills document
  the corresponding AI workflows.
- The main documentation is comprehensive about syntax, analyses, and
  components, but it is not yet organized around practical things users can
  build.

The accepted AI examples should be treated as a quarry rather than published
as-is. Promoted circuits must receive an independent review of their topology,
equations, assumptions, measurements, and educational value.


## Learning Product Vision

The cookbook should become the learner-facing center of SpiceSharpParser. It
should bridge three gaps that conventional resources often leave separate:

1. **Physics to circuit model**: explain charge, fields, carrier motion, stored
   energy, dissipation, and feedback before reducing them to a lumped model.
2. **Circuit model to mathematics**: derive the equations, define signs and
   assumptions, estimate the result by hand, and check units and limiting
   cases.
3. **Mathematics to observable behavior**: run the exact circuit, inspect
   currents and voltages, compare the result with the hand model, and explain
   discrepancies caused by non-ideal models.

The goal is not to replace a university textbook, a component data sheet, or a
supervised hardware laboratory. The goal is to make those materials easier to
understand by providing an executable, inspectable companion in which every
important claim is connected to a netlist, a measurement, and an explanation.

A learner who completes the core path should be able to:

- explain current, voltage, power, stored energy, and signal flow in plain
  language;
- apply Ohm's law, KCL, KVL, nodal analysis, Thévenin/Norton equivalents,
  superposition, and small-signal linearization;
- solve first- and second-order transients and use phasors, impedance,
  transfer functions, Bode plots, and decibels;
- distinguish an ideal abstraction, a hand-analysis approximation, a SPICE
  compact model, and a physical component;
- analyze diode, BJT, MOSFET, and op-amp circuits in their important operating
  regions;
- predict bias, gain, impedance, bandwidth, noise, distortion, stability,
  efficiency, and tolerance sensitivity;
- use `.OP`, `.DC`, `.AC`, `.TRAN`, `.NOISE`, `.MEAS`, `.FOUR`,
  `.STEP`, `.TEMP`, and deterministic Monte Carlo studies appropriately;
- read a schematic and data sheet, choose realistic component values, and
  recognize voltage, current, power, temperature, and safe-operating limits;
- design and verify a modest analog subsystem rather than only reproduce a
  prepared example.

## Audience and Prerequisites

The primary path targets a learner who already knows the names of common
components and has seen Ohm's law, but wants a rigorous working understanding
of analog electronics. Every required mathematical idea must therefore be
either introduced in a short primer or linked to one.

The core path may assume:

- high-school algebra and scientific notation;
- comfort rearranging equations;
- a qualitative idea of current and voltage;
- willingness to learn complex numbers, logarithms, derivatives, integrals,
  and simple differential equations as they become useful.

The core path must not assume:

- prior semiconductor physics;
- memorized transistor small-signal models;
- prior SPICE experience;
- access to laboratory hardware;
- familiarity with a particular vendor's parts.

Advanced modules may require calculus, complex algebra, linearization, and
basic probability, but their prerequisite links must make that dependency
explicit.

## Authoritative Learning References

The curriculum and original explanations should be informed by authoritative
course material while remaining independently written:

- [MIT OpenCourseWare 6.002 Circuits and Electronics](https://ocw.mit.edu/courses/6-002-circuits-and-electronics-spring-2007/)
  supplies a proven first-course sequence from lumped models and resistive
  networks through energy storage, dynamics, amplifiers, and design.
- [Analog Devices Electronics I and II](https://wiki.analog.com/university/courses/electronics)
  provides an open sequence covering circuit theory, op amps, semiconductor
  devices, amplifier stages, current mirrors, differential amplifiers,
  references, power, and converters.
- [Analog Devices ADALM2000 laboratory activities](https://wiki.analog.com/university/courses/electronics/labs)
  provide useful patterns for prediction, simulation, measurement, and
  conclusion-based laboratory work.
- [UC Berkeley EE 105](https://www2.eecs.berkeley.edu/Courses/EE105/)
  anchors the device-to-circuit progression: semiconductor properties,
  p-n junctions, MOS devices, large- and small-signal models, amplifiers,
  frequency response, and differential stages.
- [TI Precision Labs for op amps](https://www.ti.com/video/series/precision-labs/ti-precision-labs-op-amps.html)
  supplies a practical progression from ideal behavior to offset, noise,
  bandwidth, slew rate, output swing, stability, and application errors.
- Manufacturer data sheets and application notes should be used for
  vendor-specific limits, model parameters, and real design practices.

External material is a source and cross-check, not text or artwork to copy.
Explanations, derivations, schematics, plots, and exercises in the repository
must be original and should cite the sources that materially informed them.
A reference must be labeled as general theory, manufacturer guidance, a data
sheet limit, or an independent simulator cross-check.

## Four Kinds of Learning Content

Do not force every educational item into the application-recipe contract.
Maintain four related content types.

| Content type | Purpose | Typical scope | Practical-use requirement |
| --- | --- | --- | --- |
| Concept lab | Isolate one law, physical effect, mathematical tool, or model | One to five elements and one main analysis | Not required; canonical clarity is the purpose |
| Classic circuit lesson | Teach a standard topology and its derivation | One functional stage with ideal and non-ideal variants | Explain where the topology appears |
| Application recipe | Solve a recognizable engineering problem | One or more stages with quantitative targets | Required |
| Capstone | Integrate several modules into a design workflow | Multi-stage system, corners, and trade-offs | Required |

Concept labs make room for indispensable examples such as a resistor I/V
curve, capacitor energy, diode temperature dependence, a BJT load line, or an
op-amp open-loop sweep. These are not lesser entries; they are the bridge
between physics and application recipes.

Difficulty and curriculum order are independent metadata. A lesson can be
mathematically foundational yet operationally Simple, or appear late in the
course because it combines prerequisites while still using few components.

## Core Pedagogical Loop

Every lesson should follow the same learning loop:

1. **Observe the problem** in a real circuit or physical situation.
2. **Tell the physical story** using charge, energy, fields, carrier motion,
   or feedback as appropriate.
3. **Choose a model** and state exactly what is ignored.
4. **Predict by hand** with equations, units, signs, and an order-of-magnitude
   check.
5. **Run the model** with a deliberate analysis and measured acceptance
   criteria.
6. **Compare prediction and simulation**, quantifying the difference.
7. **Explain the difference** by moving one level up the model ladder.
8. **Change one thing** through a parameter sweep or focused experiment.
9. **Transfer the idea** to a practical application or design challenge.

Simulation must never be presented as the source of physical truth. It is a
numerical experiment on a declared model.

## Model Ladder

Whenever a device or topology permits it, teach it through the same ladder.

| Level | Question answered | Example |
| --- | --- | --- |
| Physical picture | What charge, field, energy, or carrier process causes the behavior? | A depletion region widens under reverse bias |
| Ideal model | What is the simplest useful abstraction? | An ideal diode is open or short |
| Hand model | What approximation supports calculation? | Constant 0.7 V drop or small-signal resistance |
| SPICE model | Which nonlinear equations and parameters are actually simulated? | Exponential diode model with series resistance |
| Data-sheet component | Which limits, tolerances, temperature effects, and parasitics matter in hardware? | Forward-voltage spread and reverse-recovery time |
| System consequence | How does the model choice affect the surrounding circuit? | Rectifier loss, ripple, timing, or distortion |

A lesson should not jump from a symbol directly to a simulated waveform. It
must identify which rung of the ladder is used in each derivation and test.

## Mathematics and Physics Spine

Short primers and recurring sidebars should build the mathematics when it is
needed, not hide it.

| Topic | Physical idea | Mathematics | Required checks |
| --- | --- | --- | --- |
| Units and reference directions | Measurement needs a defined orientation | SI prefixes, dimensional analysis, signed quantities | Units cancel correctly; polarity is explicit |
| Charge, current, voltage | Charge motion and electric potential | `i=dq/dt`, work per charge | Conservation of charge and sign |
| Resistance and heat | Scattering converts electrical energy to heat | `v=iR`, `p=vi=i²R=v²/R` | Power balance and rating margin |
| Network laws | Lumped circuits conserve charge and energy | KCL, KVL, linear equations, nodal analysis | Residual current and loop-voltage sums |
| Capacitance | Electric-field energy and charge storage | `q=Cv`, `i=C dv/dt`, `E=CV²/2` | Voltage continuity and energy |
| Inductance | Magnetic-field energy and flux linkage | `v=L di/dt`, `E=LI²/2` | Current continuity and energy |
| First-order dynamics | Stored state relaxes exponentially | First-order ODEs, exponentials, time constant | Initial/final values and 63.2 percent point |
| Second-order dynamics | Energy exchanges between fields | Quadratic characteristic equation, damping, resonance | Natural frequency and damping regime |
| Sinusoidal steady state | Periodic energy exchange | Complex numbers, phasors, impedance | Magnitude, phase, and limiting frequency |
| Transfer functions | Input/output relation over frequency | Rational functions, poles, zeros, logarithms, dB | DC/high-frequency limits and slopes |
| Semiconductor junctions | Diffusion, drift, recombination, depletion | Exponential law, thermal voltage, logarithms | Temperature and current-density dependence |
| Transistor action | A controlled carrier flow transfers power | Bias equations, load lines, transconductance | Region, headroom, power, and gain limits |
| Small-signal models | Nonlinear curves look linear near a bias point | Derivative, tangent linearization, Jacobian intuition | Perturbation is small relative to bias |
| Feedback | Output information changes the input error | Loop gain, closed-loop algebra, stability margins | Sign, return ratio, bandwidth, saturation |
| Noise and tolerance | Microscopic randomness and manufacturing spread | RMS, spectral density, integration, probability | Bandwidth, seed, distribution, confidence |
| Thermal behavior | Dissipation raises temperature and changes parameters | Thermal resistance and feedback | Junction estimate and runaway risk |

Every derivation should define symbols on first use, retain units until the
last step, state the approximation, and check at least one limiting case.
Numerical results should use sensible significant figures rather than implying
unavailable precision.

## Curriculum Dependency Graph

Use this as the default prerequisite order:

`00 Measurement and models
  -> 01 DC networks
  -> 02 Energy storage and transients
  -> 03 Sinusoids, impedance, and passive filters
  -> 04 Diodes and junction circuits
  -> 05 BJT circuits
  -> 06 FET circuits
  -> 07 Ideal op amps and negative feedback
  -> 08 Real op amps and feedback stability
  -> 09 Noise, distortion, tolerance, and temperature
  -> 10 Power conversion, regulation, and protection
  -> 11 Oscillators, timing, and modulation
  -> 12 Sensors, interfaces, and data conversion
  -> 13 Signal integrity and distributed effects
  -> 14 Integrated capstones`

Modules 04 through 07 may be interleaved after Modules 00 through 03. Modules
09 through 13 draw on more than one device family and must declare their exact
prerequisites.

## Required Curriculum Examples

The following is a curriculum backlog, not a promise to implement every lesson
at once. “Complete” means a current recipe substantially covers the item.
“Partial” means useful material exists but needs a dedicated physics/math
lesson or an explicit prerequisite bridge. “Required” identifies a core gap.
“Extension” is valuable after the core sequence.

### Module 00: Measurement, Models, and SPICE as an Experiment

| Lesson or lab | Status | Main physics/math | Required observable |
| --- | --- | --- | --- |
| Voltage, current, reference direction, and ground | Required | Potential difference, charge flow, sign convention | Reversing a probe reverses the reported sign |
| Source-resistor-load first circuit | Required | Ohm's law, KCL, KVL, power balance | Node voltage, branch current, source/load power |
| What `.OP` solves | Required | Algebraic equilibrium and constitutive equations | Hand nodal solution equals operating point |
| DC sweep as an I/V curve tracer | Required | Function graphs and slope | Resistor and nonlinear-device I/V curves |
| Transient analysis and timestep | Required | Initial state and numerical sampling | Same physical result under timestep refinement |
| AC small-signal analysis | Required | Linearization, phasors, complex response | Magnitude and phase around a bias point |
| Model versus component | Required | Abstraction and parameterization | Ideal, simple, and fuller model comparison |
| Measurement uncertainty and significant figures | Required | Error, tolerance, resolution | Reported precision matches model confidence |

### Module 01: DC Resistive Networks

| Lesson or circuit | Status | Main physics/math | Required observable |
| --- | --- | --- | --- |
| Resistor I/V curve, dissipation, and power rating | Required | Conductivity abstraction, Ohm's law, Joule heating | I/V slope and power curve |
| Series and parallel networks | Required | KCL/KVL and equivalent resistance | Branch-current and voltage-divider checks |
| Nodal analysis with two unknowns | Required | Simultaneous linear equations | KCL residual at every essential node |
| Mesh/loop reasoning and source polarity | Required | KVL and signs | Sum of rises and drops equals zero |
| Loaded voltage divider and emitter follower | Complete | Thévenin resistance, loading, transistor buffer | 49.76-times droop improvement |
| Thévenin and Norton source equivalence | Required | Linear network equivalence | Identical load sweep for both models |
| Superposition with two independent sources | Required | Linearity | Sum of partial responses equals total |
| Wheatstone bridge and null measurement | Required | Ratios and differential voltage | Balance condition and sensitivity |
| Maximum-power transfer versus efficiency | Required | Quadratic power relation | Power peak at matched resistance |
| Dependent-source gain block | Required | Controlled sources and linear models | Gain, input resistance, output resistance |

### Module 02: Capacitors, Inductors, and Time-Domain Energy

| Lesson or circuit | Status | Main physics/math | Required observable |
| --- | --- | --- | --- |
| Capacitor charge, current, and stored energy | Required | `q=Cv`, `i=C dv/dt`, electric-field energy | Charge and energy during a ramp |
| RC charge and discharge | Required | First-order ODE and exponential | Initial/final values, tau, 10–90 percent rise |
| RC integrator and differentiator limits | Required | Time-scale separation and approximation | Pulse/triangle response versus frequency |
| Inductor voltage, current, and stored energy | Required | `v=L di/dt`, magnetic-field energy | Current continuity and energy |
| RL step and freewheel path | Required | First-order ODE | Current rise/decay and time constant |
| BJT relay driver with flyback diode | Complete | Inductor energy and switching | Clamp voltage and release decay |
| Source and load parasitics | Required | ESR, winding resistance, source impedance | Damping and loss compared with ideal model |
| Series RLC step response | Required | Second-order ODE | Under-, critical-, and over-damped cases |
| Series and parallel resonance | Required | Energy exchange and impedance | Resonant frequency, Q, bandwidth |
| Coupled inductors and transformer basics | Required | Mutual flux and turns ratio | Voltage/current ratio and reflected load |

### Module 03: Sinusoids, Phasors, Impedance, and Passive Filters

| Lesson or circuit | Status | Main physics/math | Required observable |
| --- | --- | --- | --- |
| Sine amplitude, RMS, frequency, and phase | Required | Trigonometry and energy equivalence | RMS and phase-delay measurement |
| Complex numbers and phasor addition | Required | Rectangular/polar forms | Hand vector sum equals AC result |
| Capacitive and inductive impedance | Required | Complex algebra | Magnitude/phase versus frequency |
| First-order RC low-pass and high-pass | Required | Transfer function and pole | Cutoff, -20 dB/decade slope, phase |
| Passive RC band-pass | Required | Cascaded high/low-pass behavior | Center region and loading error |
| RLC resonance and quality factor | Required | Second-order transfer function | `f0`, Q, bandwidth, phase crossing |
| RC sensor filter and input protector | Complete | Bandwidth, attenuation, clamp loading | AC cutoff and transient protection |
| Passive crossover network | Extension | Filter partition and load interaction | Driver-band overlap and phase |
| Twin-T or bridged-T notch filter | Extension | Poles, zeros, component matching | Notch depth and tolerance sensitivity |
| Transmission from time to frequency views | Required | Fourier intuition | Step response linked to bandwidth |

### Module 04: Diode Physics and Junction Circuits

| Lesson or circuit | Status | Main physics/math | Required observable |
| --- | --- | --- | --- |
| P-n junction, depletion region, and diode I/V | Required | Drift/diffusion picture and Shockley equation | Decades of current on semilog axes |
| Thermal voltage and temperature dependence | Required | Exponential law and temperature | Forward-voltage shift with `.TEMP` |
| Ideal, constant-drop, and exponential diode models | Required | Model ladder | Error of each model versus current |
| Half-wave and full-wave rectifiers | Partial | Conduction intervals and RMS/average values | DC output, ripple, diode current |
| Rectifier, reservoir, and Zener regulator | Complete | Charge pulses, stored energy, regulation | Ripple and load-step response |
| Diode clipper and limiter | Required | Piecewise-linear analysis | Transfer curve and clipping thresholds |
| DC restorer/clamper | Required | Charge conservation and time constants | Level shift and droop |
| Peak detector and AM envelope follower | Complete | Peak charging and RC release | Peak error and carrier ripple |
| Zener shunt regulator | Partial | Breakdown, dynamic resistance, load line | Line/load regulation and dissipation |
| LED resistor and load-line design | Required | Forward I/V, optical output proxy, power | Current across supply tolerance |
| Voltage doubler and charge pump | Required | Switched charge and ripple | No-load gain and loaded droop |
| Photodiode current source | Required | Photon-generated carriers and junction capacitance | Current-to-voltage response |

### Module 05: BJT Physics, Bias, and Amplifier Stages

| Lesson or circuit | Status | Main physics/math | Required observable |
| --- | --- | --- | --- |
| BJT carrier transport and operating regions | Required | Junction bias and transistor action | Cutoff, forward active, saturation |
| Output characteristics and DC load line | Required | Simultaneous device/network solution | Q-point intersection and power |
| BJT as a saturated switch | Partial | Forced beta, base charge, saturation | Drive margin and saturation voltage |
| BJT relay driver | Complete | Switch design and inductive load | Coil current and safe turn-off |
| Fixed bias versus emitter-degenerated bias | Required | Feedback and temperature stability | Q-point sensitivity to beta and temperature |
| Emitter follower | Complete | Buffering, base current, `VBE` | Gain, headroom, input/output resistance |
| Common-emitter voltage amplifier | Partial | Bias, transconductance, inversion | Midband gain and clipping |
| BJT audio preamplifier | Complete | Bias, coupling, bandwidth, distortion | Gain, cutoff frequencies, THD |
| Common-base stage | Required | Low input impedance and current transfer | Gain and impedance comparison |
| BJT current mirror | Required | Matched junctions and systematic error | Current ratio versus output voltage |
| Differential pair | Required | Current steering and transconductance | Differential/common-mode response |
| Cascode and Early-effect comparison | Extension | Output resistance and Miller effect | Gain/bandwidth improvement |
| Thermal runaway and emitter stabilization | Required | Electrothermal positive feedback | Temperature sweep and dissipation margin |

### Module 06: MOSFET and JFET Physics and Circuits

| Lesson or circuit | Status | Main physics/math | Required observable |
| --- | --- | --- | --- |
| MOS capacitor and field-effect picture | Required | Charge controlled by electric field | Qualitative channel formation |
| MOSFET transfer and output characteristics | Required | Threshold, square-law approximation, regions | `ID–VGS` and `ID–VDS` sweeps |
| MOSFET as a low-side switch | Required | On-resistance, gate drive, dissipation | Conduction loss and switching edge |
| Gate capacitance and drive resistance | Required | Charge, RC edge, switching energy | Rise/fall time and drive current |
| Source follower | Required | Local feedback and headroom | Gain and output resistance |
| Common-source amplifier | Required | Transconductance and inversion | Bias, gain, clipping, bandwidth |
| Source degeneration | Required | Feedback and linearity | Gain accuracy and THD improvement |
| MOS current source and mirror | Required | Saturation and channel-length modulation | Compliance and output resistance |
| JFET current source/buffer | Required | Depletion-mode field effect | Bias current and input impedance |
| CMOS inverter analog transfer curve | Required | Complementary conduction and gain | Threshold, gain, current overlap |
| MOS analog switch and transmission gate | Required | Bidirectional resistance and signal range | `RON` versus input/common mode |
| Body diode, body effect, and model boundary | Required | Junction/body coupling | Reverse path and threshold shift |

### Module 07: Ideal Op Amps and Negative Feedback

| Lesson or circuit | Status | Main physics/math | Required observable |
| --- | --- | --- | --- |
| Differential gain and the ideal op-amp rules | Required | High gain plus negative feedback | Input error approaches zero without assuming magic |
| Voltage follower | Required | Feedback and loading | Near-unity gain and impedance transformation |
| Inverting amplifier | Required | Virtual node and KCL | Gain set by resistor ratio |
| Non-inverting amplifier | Required | Divider feedback | Closed-loop gain and input impedance |
| Summing and averaging amplifier | Required | Superposition and KCL | Weighted sum |
| Difference amplifier | Required | Ratio matching and common-mode rejection | Differential gain and mismatch sensitivity |
| Integrator and practical integrator | Required | Differential equation and pole placement | Ramp response and DC stabilization |
| Differentiator and practical differentiator | Required | Derivative and noise gain | Edge response and bandwidth limiting |
| Active anti-alias filter and ADC buffer | Complete | Buffered poles and settling | Attenuation and ADC load response |
| Comparator versus op amp | Required | Open-loop saturation and recovery | Threshold behavior and model warning |
| Schmitt trigger | Required | Positive feedback and hysteresis | Rising/falling thresholds and noise rejection |
| Precision rectifier | Required | Feedback around diode nonlinearity | Low-level rectification error |

### Module 08: Real Op Amps, Feedback, and Stability

| Lesson or circuit | Status | Main physics/math | Required observable |
| --- | --- | --- | --- |
| Finite open-loop gain and gain error | Required | Return ratio and desensitivity | Error versus open-loop gain |
| Gain-bandwidth product | Required | Dominant pole and noise gain | Closed-loop bandwidth versus gain |
| Slew rate and full-power bandwidth | Required | Internal current limit charging capacitance | Large-signal distortion versus frequency |
| Input offset and bias current | Required | Error propagation | DC output error and compensation |
| Common-mode and output swing limits | Required | Internal headroom | Saturation despite valid ideal calculation |
| Output current limit and load drive | Required | Finite output stage | Gain collapse and dissipation |
| Capacitive-load stability | Required | Added pole and phase margin | Overshoot/ringing versus isolation resistor |
| Loop-gain and phase-margin laboratory | Required | Bode stability criteria | Crossover and margins |
| Transimpedance amplifier | Required | Current summing and noise gain | Gain, bandwidth, sensor capacitance |
| Instrumentation amplifier | Required | Differential gain and CMRR | Common-mode rejection and resistor error |
| Active current regulator | Required | Feedback and compliance | Line/load regulation |
| Linear regulator loop | Extension | Reference, error amplifier, pass device | Regulation, dropout, stability |

### Module 09: Noise, Distortion, Tolerance, and Temperature

| Lesson or circuit | Status | Main physics/math | Required observable |
| --- | --- | --- | --- |
| Johnson noise of a resistor | Required | Thermal agitation and spectral density | `.NOISE` result versus `sqrt(4kTRB)` |
| Shot and flicker-noise model boundary | Required | Discrete carriers and low-frequency processes | Device/source noise contribution |
| Noise bandwidth of RC filters | Required | Spectral integration | Integrated output noise |
| Signal-to-noise ratio and dynamic range | Required | RMS ratios and decibels | SNR before/after gain/filtering |
| Clipping and harmonic generation | Required | Nonlinearity and Fourier series | `.FOUR` harmonics and THD |
| Crossover distortion in push-pull stages | Required | Dead zone and bias | Waveform notch and THD |
| Component tolerance sweep | Required | Sensitivity and worst case | Gain/cutoff distribution |
| Temperature corners | Required | Parameter drift | Bias and regulation change |
| Seeded Monte Carlo laboratory | Required | Probability and reproducibility | Distribution with explicit seed |
| Sensitivity-ranked redesign | Required | Partial-derivative intuition | Dominant components and improvement |

### Module 10: Power Conversion, Regulation, and Protection

| Lesson or circuit | Status | Main physics/math | Required observable |
| --- | --- | --- | --- |
| Rectifier power supply | Complete | Pulsed charging, ripple, regulation | Startup, ripple, diode current |
| Zener and reference circuits | Partial | Breakdown and dynamic resistance | Line/load regulation |
| Ideal-diode redundant input | Complete | Power-path selection and reverse blocking | Failover and handback |
| Series-pass linear regulator | Required | Feedback, dropout, dissipation | Line/load response and thermal estimate |
| Current limiting and foldback | Required | Sensing and safe operating area | Limit curve and pass-device power |
| Constant-current LED driver | Planned | Feedback and compliance | Current regulation and dissipation |
| Reverse-polarity and surge protection | Required | Current paths and clamping energy | Fault current and clamp stress |
| Buck converter with saturating inductor | Planned | Switched energy and nonlinear magnetics | Duty, ripple, startup, saturation |
| Boost converter | Extension | Inductor energy transfer | Conversion ratio and switch stress |
| Charge-pump supply | Required | Switched-capacitor energy transfer | Output resistance and ripple |
| Efficiency and loss accounting | Required | Conservation of energy | Input/output/device power balance |
| Thermal model and derating | Required | Heat flow and feedback | Junction estimate and margin |

### Module 11: Oscillators, Timing, and Modulation

| Lesson or circuit | Status | Main physics/math | Required observable |
| --- | --- | --- | --- |
| Two-transistor astable LED flasher | Complete | Regenerative switching and RC timing | Frequency, phase, LED current |
| Schmitt-trigger relaxation oscillator | Required | Hysteresis and capacitor thresholds | Period derived from charge equations |
| 555 monostable and astable | Planned | Comparator/latch timing model | Pulse width, frequency, duty |
| Wien-bridge oscillator | Required | Positive feedback and amplitude control | Startup and steady amplitude |
| RC phase-shift oscillator | Required | Loop phase and gain condition | Oscillation frequency and startup |
| PWM generation and duty control | Required | Comparator modulation | Frequency, duty, spectrum |
| PWM digital-to-analog converter | Complete | Averaging and ripple filtering | Average, ripple, settling |
| AM generation and diode detection | Partial | Multiplication and envelope | Modulation depth and recovery |
| Simple phase-locked loop | Complete | Phase feedback and acquisition | Lock time, period error, control ripple |
| Voltage-controlled oscillator | Required | Control-to-frequency transfer | Gain and tuning linearity |
| Crystal oscillator | Extension | High-Q motional model | Model boundary and startup caution |

### Module 12: Sensors, Interfaces, and Data Conversion

| Lesson or circuit | Status | Main physics/math | Required observable |
| --- | --- | --- | --- |
| Wheatstone bridge sensor front end | Required | Differential ratios and sensitivity | Small resistance change to voltage |
| Thermistor divider and linearization | Required | Exponential resistance and approximation | Temperature error over range |
| Photodiode transimpedance front end | Required | Photocurrent and capacitance | Gain, bandwidth, noise |
| AC-coupled sensor input | Required | Bias restoration and high-pass response | Startup and low-frequency cutoff |
| RC sensor filter and protection | Complete | Noise bandwidth and fault paths | Filter response and clamp current |
| Active anti-alias filter | Complete | Sampling preparation and settling | Stopband and acquisition response |
| Sample-and-hold | Required | Charge acquisition, droop, switch error | Acquisition time and held-value droop |
| R-2R DAC | Required | Binary superposition and resistor matching | Code transfer and monotonicity |
| PWM DAC | Complete | Time averaging | DC error, ripple, settling |
| ADC input drive and kickback model | Required | Charge sharing and source impedance | Settling within acquisition window |
| 4–20 mA current-loop interface | Required | Current signaling and compliance | Fault/open-loop behavior |
| Comparator battery monitor | Planned | Thresholds, hysteresis, latching | Trip accuracy and chatter rejection |

### Module 13: Signal Integrity and Distributed Effects

| Lesson or circuit | Status | Main physics/math | Required observable |
| --- | --- | --- | --- |
| Interconnect as lumped R, L, and C | Required | Parasitic energy storage | Edge degradation versus length/load |
| Transmission-line propagation | Complete | Traveling waves and delay | One-way and round-trip timing |
| Source, load, and parallel termination | Complete | Reflection coefficient | Overshoot and settling comparison |
| Stub and capacitive-load reflection | Required | Impedance discontinuity | Reflection timing and amplitude |
| Differential signaling and termination | Required | Odd-mode current and common mode | Differential swing and balance |
| Ground return and shared impedance | Required | Current loops and common impedance | Crosstalk through return resistance |
| Supply decoupling network | Required | Source impedance versus frequency | Local droop and resonance |
| Probe/loading effects | Required | Measurement is part of the circuit | Waveform change with probe model |
| Scope of the transmission-line model | Required | Model limits | Explicit skin effect/EMI boundary |

### Module 14: Integrated Capstones

| Capstone | Status | Prerequisite synthesis | Required proof |
| --- | --- | --- | --- |
| Battery monitor and protected ADC input | Required | Dividers, reference, comparator, filtering, protection | Accuracy, thresholds, faults, tolerance |
| Low-noise sensor acquisition chain | Required | Sensor model, bias, op amp, filter, ADC drive | Noise, bandwidth, settling, error budget |
| Small audio preamplifier and output buffer | Partial | BJT/op amp gain, coupling, bias, distortion | Gain, bandwidth, output power, THD |
| Linear bench supply with current limit | Planned | Rectifier, reference, feedback, thermal behavior | Regulation, limit, foldback, dissipation |
| Efficient LED driver | Planned | Current feedback, switching, magnetics | Regulation, efficiency, stress |
| Function generator | Required | Oscillator, shaping, amplitude control | Frequency range, amplitude, distortion |
| Closed-loop DC motor controller | Planned | PWM, power switch, feedback, dynamic load | Startup, speed error, load rejection |
| Redundant low-voltage power front end | Partial | Ideal diode, surge protection, filtering | Failover, reverse current, surge stress |
| Complete AM signal chain | Required | Oscillator, modulation, filtering, detector | Modulation depth, recovery, distortion |
| Transmission-line pulse source and receiver | Partial | Driver impedance, line, termination, protection | Reflection, threshold, timing margin |

## Existing Recipe Placement

The current recipes remain useful; they become destinations in the course
rather than being rewritten as isolated beginner lessons.

| Existing recipe | Primary module | Prerequisites to add or link |
| --- | --- | --- |
| Loaded voltage divider and emitter follower | 01 and 05 | Thévenin model, BJT regions, `VBE` |
| RC sensor filter and input protector | 03 and 12 | RC pole, diode clamp model, ADC loading |
| BJT relay driver | 02 and 05 | Inductor energy, BJT switch, flyback path |
| PWM DAC | 03, 11, and 12 | Duty-cycle average, Fourier intuition, RC settling |
| Diode envelope detector | 04 and 11 | Diode I/V, peak detector, AM envelope |
| Astable LED flasher | 05 and 11 | BJT switch, capacitor thresholds, regeneration |
| Rectifier power supply | 04 and 10 | Full-wave conduction, reservoir charge, Zener load line |
| BJT audio preamplifier | 05 and 09 | Bias, small signal, coupling poles, clipping |
| Active anti-alias filter | 07 and 12 | Op-amp feedback, poles, sampling acquisition |
| Ideal-diode power OR | 10 | Real diode loss, power paths, switching thresholds |
| Simple PLL | 08 and 11 | feedback, phase detector, loop filter, VCO |
| Transmission-line termination | 13 | distributed model, impedance, reflection coefficient |

## Suggested Study Paths

### Core analog path

Study Modules 00 through 09 in order, then choose Modules 10, 11, or 12 by
interest. Complete at least one capstone. This is the recommended “book” path.

### Practical maker path

Study Modules 00, 01, 02, 04, and 05; then follow the power, sensor, audio, or
timing application links. Math primers remain available when a design exposes
a gap.

### Device-physics path

Study Modules 00 through 04, then Modules 05 and 06 in depth, including I/V
sweeps, temperature, load lines, small-signal derivatives, and model-card
parameters.

### Signal-chain path

Study Modules 00 through 04, then Modules 07, 08, 09, and 12. Finish with the
low-noise sensor chain or audio-chain capstone.

### Power path

Study Modules 00 through 06, the feedback core in Modules 07 and 08, then
Module 10. Finish with the bench supply, LED driver, or motor controller.

## Assessment and Exercise Strategy

Every core lesson should contain:

- one prediction question before the simulation;
- one hand calculation with a numeric answer;
- one waveform-reading question;
- one “what changes if” question tied to a sweep;
- one model-boundary question;
- one practical design or component-rating question;
- a short answer key or graduated hints kept separate from the main flow.

End each module with:

- a concept checklist;
- a cumulative circuit that uses at least two earlier ideas;
- a fault-finding exercise with one deliberate mistake;
- a design challenge with ranges rather than one exact component set;
- an explanation prompt that cannot be answered by quoting a simulated number.

Assessment should reward correct reasoning and honest model choice, not merely
matching a waveform.

## Learning Navigation and Metadata

A large cookbook must be navigable as a dependency graph, not only a flat
table. Add machine-readable metadata for:

- content type, module, sequence, and difficulty;
- prerequisites and “next lesson” links;
- learning objectives;
- physics and mathematics topics;
- device families and analyses;
- estimated study time;
- required dialect/custom components;
- hardware-lab availability and safety level;
- implementation status;
- measurements and acceptance ranges;
- source references and external cross-check status.

Generate these views from the metadata once the schema is stable:

- ordered textbook contents;
- browse by application;
- browse by device;
- browse by physics topic;
- browse by mathematical tool;
- browse by analysis type;
- browse by difficulty;
- prerequisite graph;
- implemented versus planned curriculum coverage.

## Current Implementation Status

The cookbook now has a broader simple-circuit foundation:

- Twelve recipes are present in the user-facing catalog: six Simple, four
  Medium, and two Difficult circuits across the Pure SPICE and
  CustomComponents tracks.
- Every current recipe has a runnable `.cir`, full guide, checked-in
  `schematic.toml`, generated `schematic.svg`, and accessible `response.svg`.
- All current schematic layouts display every parsed netlist component and
  render reproducibly from their checked-in sources.
- The repository-local cookbook report passes without errors or warnings.
- The data-driven `CircuitCookbookTests` suite compiles and simulates all
  current recipes and enforces their measurement ranges.

The next milestone should be the Learning Foundation release rather than
another difficult application. It should establish the course index, math and
modeling primers, lesson metadata, and the first missing concept sequence:
source-resistor-load, Thévenin/Norton equivalence, RC charge/discharge and
energy, diode I/V and temperature, BJT load line and bias, MOSFET curves, and
ideal op-amp feedback. The buck converter remains an important planned power
capstone after its inductor, switching, feedback, and loss-accounting
prerequisites exist.

## Cookbook Tracks

Maintain two visible tracks.

### Pure SPICE

These circuits should use the core parser and portable SPICE constructs where
practical. They demonstrate that useful work does not require the optional
custom-components package.

### CustomComponents

These lessons may use ideal diodes, nonlinear passives, LTspice A-devices, or
the packaged analog, digital, and 555 subcircuits when the learning objective
depends on them. Prefer Pure SPICE for foundational concept labs, and do not
use a custom component merely to hide physical behavior that the lesson should
explain.

## Candidate Circuit Roadmap

Maintain a balanced application cookbook with Simple, Medium, and Difficult
circuits. Difficulty describes design and explanation burden, not usefulness.
Application recipes and capstones must solve a recognizable practical problem;
concept labs and classic-circuit lessons may instead isolate a foundational
law or topology. Every published entry must expose objective measurements and
link to its prerequisites.

### Simple circuits

Use one main functional stage, a compact netlist, and a short measurement
story. These recipes should be approachable without prior simulator expertise.

| Circuit | Track | Status | Practical use | What it measures and teaches |
| --- | --- | --- | --- | --- |
| PWM-to-analog converter | Pure SPICE | Complete | Recover an analog control voltage from a microcontroller PWM output | Duty-cycle average, ripple, settling time, and cascaded filtering |
| RC sensor-noise filter and input protector | Pure SPICE | Complete | Condition a noisy low-voltage sensor before an ADC input | Cutoff frequency, step response, attenuation, and clamp current |
| BJT relay or solenoid driver with flyback diode | Pure SPICE | Complete | Drive an inductive load safely from a logic-level signal | Base drive, coil current, saturation voltage, turn-off transient, and flyback decay |
| RC switch debouncer with Schmitt-trigger buffer | CustomComponents | Planned | Convert a bouncing mechanical switch into a clean digital edge | Threshold hysteresis, rejected pulse width, propagation delay, and output edge count |
| Loaded voltage divider and emitter-follower buffer | Pure SPICE | Complete | Scale a battery or sensor voltage without heavily loading its source | Divider error, input impedance, output impedance, bias error, and headroom |
| Diode peak detector and audio envelope follower | Pure SPICE | Complete | Recover the peak or amplitude envelope of an AC signal | Diode-drop error, attack time, release time, ripple, and load sensitivity |
| Two-transistor astable LED beacon | Pure SPICE | Complete | Build a low-cost flasher without a timer IC | Startup, oscillation frequency, duty cycle, capacitor charging, and transistor switching |
| Varistor-protected DC input | CustomComponents | Planned | Clamp a supply surge before it reaches a sensitive load | Clamp voltage, surge current, absorbed energy, leakage, and source impedance |

### Medium circuits

Combine several functional stages or more than one analysis. These recipes
should introduce realistic design tradeoffs while remaining easy to modify.

| Circuit | Track | Status | Practical use | What it measures and teaches |
| --- | --- | --- | --- | --- |
| Rectifier, reservoir, and Zener regulator | Pure SPICE | Complete | Produce filtered low-voltage DC from an isolated AC secondary | Startup, diode conduction, reservoir ripple, load regulation, and load steps |
| Ideal-diode redundant power input | CustomComponents | Complete | OR two supplies with automatic failover and reverse-current blocking | Source priority, failover dip, handback, standby current, and reverse current |
| BJT audio preamplifier | Pure SPICE | Complete | Raise a small audio signal to a useful level for a following stage | Bias point, AC gain, bandwidth, clipping, loading, and Fourier distortion |
| 555 monostable pulse stretcher and PWM controller | Packaged subcircuit | Planned | Create a fixed-duration event pulse and an adjustable LED or fan drive | Pulse width, duty range, frequency, reset behavior, and load response |
| Active anti-alias filter and ADC buffer | Pure SPICE | Complete | Limit sensor bandwidth and drive a sampling input cleanly | Passband gain, cutoff, attenuation, phase shift, settling, and output loading |
| Class-AB headphone or line-output buffer | Pure SPICE | Planned | Drive a low-impedance audio load from a small-signal source | Quiescent current, crossover distortion, voltage swing, output power, and efficiency |
| Window-comparator battery monitor with latched alarm | CustomComponents | Planned | Detect under-voltage and over-voltage conditions and retain a fault indication | Threshold accuracy, hysteresis, alarm latency, reset behavior, and chatter rejection |
| Constant-current LED string driver | Pure SPICE | Planned | Hold LED current steady as supply and load voltage change | Current regulation, compliance voltage, transistor dissipation, line response, and load response |

### Difficult circuits

Use feedback, mixed-signal state, nonlinear energy storage, or distributed
effects. These recipes should remain practical, but may require longer
simulations and a deeper stability or timing explanation.

| Circuit | Track | Status | Practical use | What it measures and teaches |
| --- | --- | --- | --- | --- |
| Simple phase-locked loop | CustomComponents | Complete | Synchronize a controllable oscillator to a reference clock | Acquisition, phase/frequency correction, control ripple, period error, and lock behavior |
| Buck converter with saturating inductor | CustomComponents | Planned | Step a DC rail down efficiently while exposing magnetic limits | Current ripple, output ripple, duty ratio, startup, saturation onset, and load response |
| Transmission-line termination and cable driver | Pure SPICE | Complete | Preserve signal integrity over a delayed interconnect | Propagation delay, reflections, overshoot, settling, and source/load matching |
| Sampled sensor alarm and data recorder | CustomComponents | Planned | Sample an analog sensor, retain its value, and trigger digital decisions | Aperture timing, held-value droop, thresholds, alarm latency, and mixed-signal sequencing |
| Closed-loop PWM DC-motor speed controller | CustomComponents | Planned | Regulate motor speed through supply and mechanical load changes | Startup current, back EMF, speed error, loop response, duty limits, and load rejection |
| Class-D audio amplifier with LC output filter | CustomComponents | Planned | Drive a speaker efficiently from a PWM switching stage | Modulation, dead time, output ripple, harmonic distortion, filter response, and efficiency |
| Linear bench supply with current limiting and foldback | Pure SPICE | Planned | Provide a regulated DC output that survives overload and short-circuit conditions | Line regulation, load regulation, loop response, current limit, foldback, and device dissipation |
| Digital frequency counter and tachometer | CustomComponents | Planned | Count input events over a fixed gate interval and retain a readable result | Gate timing, count accuracy, overflow, latch timing, reset sequencing, and low-frequency error |

Keep the application backlog balanced, but prioritize prerequisite coverage
over equal category counts. The buck converter remains a high-value Difficult
recipe; it should follow the foundational inductor, switching, feedback, and
power-loss lessons. This roadmap is a candidate pool, not a commitment to
publish all 24 application circuits. Apply the selection criteria before
promotion and keep each published lesson maintainable.

## Completed Starting Sequence

The starting sequence established three complementary examples:

1. **Rectifier and filtered DC supply**
   Established the cookbook structure with an accessible, practical
   pure-SPICE circuit.

2. **PWM DAC**
   Added a compact transient example with clear measurements for output
   average, ripple, and settling time.

3. **Simple PLL**
   Demonstrated that CustomComponents can form a complete mixed-signal system,
   rather than only isolated device examples.

The ideal-diode redundant power input and BJT audio preamplifier were followed
by a four-recipe batch: the RC sensor filter, BJT relay driver, active
anti-alias filter, and transmission-line termination comparison.

## Repository Structure and Growth Path

Keep executable circuits beside the parser source and add a lightweight book
layer that links them into a course. Do not move existing recipe directories
merely to impose teaching order.

```text
circuits/
  README.md                         searchable application catalog
  cookbook/
    COURSE.md                       ordered curriculum and study paths
    GLOSSARY.md                     symbols, units, device and SPICE terms
    MATH_PRIMER.md                  algebra, complex numbers, dB, ODEs
    MODELING_PRIMER.md              abstraction ladder and SPICE limits
    SIMULATION_PRIMER.md            OP/DC/AC/TRAN/NOISE and measurement
    HARDWARE_SAFETY.md              low-voltage lab and measurement safety
    modules/
      00-measurement-and-models.md
      01-dc-networks.md
      ...
      14-capstones.md
    concept-labs/
      <lesson-slug>/
        README.md
        <lesson-slug>.cir
        lesson.toml
        schematic.toml
        schematic.svg
        response.svg
    pure-spice/
      <application-slug>/
        README.md
        <application-slug>.cir
        lesson.toml
        schematic.toml
        schematic.svg
        response.svg
    custom-components/
      <application-slug>/
        README.md
        <application-slug>.cir
        lesson.toml
        schematic.toml
        schematic.svg
        response.svg
tools/
  cookbook-schematic/
  cookbook-report/
  cookbook-lab/                     proposed learner-facing runner
.agents/skills/
  cookbook-schematic/
  cookbook-report/
```

The current `circuits/README.md` remains the application catalog.
`circuits/cookbook/COURSE.md` becomes the ordered learning path. Module pages
must link to existing recipes in place and may link to concept labs from more
than one module.

Implementation-oriented examples such as `digital-milestone-a` remain
regression and design-history material unless they are deliberately rewritten
as learner-facing lessons.

A generated documentation site may eventually publish the course, but source
netlists, manifests, guides, and compact SVG assets should remain in this
repository while they serve regression and compatibility review. Large raw
waveforms, copied textbook figures, and generated site output should not be
committed.

## Standard Learning-Content Documentation

Every learner-facing `README.md` should preserve the headings currently
checked by `cookbook-report` and progressively adopt the following order.
Concept labs may shorten the practical design sections, but they may not omit
the physical story, model choice, prediction, measured comparison, or model
boundary.

1. **Purpose and real-world connection**
   State the question answered and where the effect or topology appears.

2. **Prerequisites and learning objectives**
   Link prior lessons and use observable verbs: explain, calculate, predict,
   measure, compare, design, or diagnose.

3. **Design targets**
   State supply, signal, load, frequency, accuracy, power, and other
   quantitative constraints.

4. **Requirements and compatibility**
   Identify package, dialect, analyses, difficulty, content type, module, and
   external cross-check status.

5. **Schematic**
   Include an accessible SVG with names matching the netlist. Introduce
   reference directions for the important currents and voltages in prose or
   an annotation layer.

6. **Physical picture**
   Explain charge, energy, fields, carrier behavior, or feedback before using
   equations. State which aspects are microscopic and which are lumped-model
   abstractions.

7. **Model ladder**
   Compare the ideal, hand-analysis, SPICE, and hardware models used in the
   lesson. Name the model actually simulated.

8. **How it works**
   Explain states, conduction paths, energy flow, and signal flow. For
   switching circuits, walk through a complete cycle.

9. **Mathematical derivation**
   Define symbols, derive the governing equations without unexplained jumps,
   carry units, and state assumptions.

10. **Hand prediction**
    Calculate numeric bias points, time constants, gain, cutoff, stress, or
    other outcomes before showing simulated results. Include an
    order-of-magnitude and limiting-case check.

11. **Component selection**
    Explain calculated values, E-series rounding, ratings, tolerances, and why
    each component exists.

12. **Runnable netlist**
    Link the complete `.cir` and show only instructive excerpts. Explain
    model cards and non-obvious directives.

13. **Simulation setup**
    Explain why `.OP`, `.DC`, `.AC`, `.TRAN`, or `.NOISE` is the
    right numerical experiment. Justify sweep range, timestep, initial
    conditions, and saved signals.

14. **Verified response**
    Include accessible plots and a table mapping each `.MEAS` result to a
    prediction and acceptance range.

15. **Prediction versus simulation**
    Quantify the error between the hand model and SPICE model and explain its
    cause. Agreement must not be asserted only qualitatively.

16. **Power, energy, and stress**
    Account for source/load/device power where relevant and check voltage,
    current, energy, and thermal limits.

17. **Parameter exploration**
    Include at least one deterministic `.STEP`, `.TEMP`, or focused
    experiment with a prediction of direction before the result.

18. **Real-world applications**
    Give concrete uses and connect them to authoritative sources or data
    sheets.

19. **Common mistakes and debugging**
    Include polarity, reference, unit, bias-region, timestep, loading, and
    model-choice mistakes likely for this lesson.

20. **Experiments and design challenge**
    Provide safe modifications, a transfer problem, and at least one
    open-ended design task.

21. **Model boundary and hardware reality**
    State omitted tolerance, parasitic, noise, thermal, aging, layout,
    measurement, damage, and vendor-specific effects.

22. **Safety**
    State whether the circuit is simulation-only or suitable for a low-voltage
    bench. Never invite direct work on mains or hazardous stored energy.

23. **Summary and next lessons**
    Restate the durable ideas and link forward through prerequisites.

24. **Running and testing**
    Give exact commands for validation and simulation.

## Suggested Metadata Header

Each guide should begin with a compact human-readable table:

| Property | Value |
| --- | --- |
| Content type | Classic circuit lesson |
| Module | 05 — BJT physics, bias, and amplifiers |
| Prerequisites | 01 Thévenin; 04 diode junction |
| Learning objectives | Predict bias; derive gain; measure impedances |
| Physics | Carrier transport, charge storage |
| Mathematics | Load line, linearization, logarithms |
| Requires | SpiceSharpParser core |
| Dialect | Portable SPICE |
| Analyses | OP, DC, AC, TRAN |
| Difficulty | Medium |
| Study time | 60–90 minutes |
| Hardware | Optional low-voltage extension |
| Verified with | SpiceSharpParser 3.4.x |
| External cross-check | Not yet performed |

A versioned `lesson.toml` should carry the same machine-readable identity,
prerequisite, objective, analysis, measurement, plot, and safety fields. It is
the single manifest format for concept labs, classic lessons, application
recipes, and capstones.

For custom circuits, `Requires` must explicitly name
`SpiceSharpParser.CustomComponents` and the component families used.

## Verification Standard

A lesson is complete only when it passes four gates.

### Electrical and numerical gate

- It uses public, supported syntax and compiles without unexpected diagnostics.
- Every node has a deliberate DC path and every semiconductor has an explicit
  model.
- All analyses complete successfully with justified timestep/sweep settings.
- Important outcomes are named `.MEAS` values and automated tests assert
  meaningful tolerances.
- Sweeps, temperature, and Monte Carlo studies are deterministic where used.
- Portable-SPICE claims are independently cross-checked when practical.

### Physics and mathematics gate

- Every component has an explained physical and circuit role.
- The governing equations are derived with symbols, signs, units, and stated
  assumptions.
- A numeric hand prediction precedes the simulated result.
- Initial/final values, limiting cases, order of magnitude, and conservation
  of charge/energy/power are checked where relevant.
- The chosen device model and its omitted physics are explicit.
- Prediction/simulation disagreement is quantified and explained.

### Teaching gate

- Prerequisites and measurable learning objectives are present.
- Schematic, physical story, equations, netlist, waveform, and measurement
  table use the same node/component names and reference directions.
- The lesson includes a prediction prompt, parameter experiment, common
  mistake, model-boundary question, and transfer/design exercise.
- Real-world applications and source references are present.
- Another learner can complete the lesson without undocumented steps.
- Safety and hardware suitability are explicit.

### Artifact and maintenance gate

- `schematic.toml` passes topology and netlist-reference validation.
- `schematic.svg` and `response.svg` are fresh, deterministic, accessible
  outputs.
- Manifest measurements match netlist `.MEAS`, guide tables, and regression
  assertions.
- Links, prerequisite relationships, and catalog/module entries are valid.
- No large raw waveform or copied third-party artwork is committed.

Run the fast, read-only structural and generated-asset gate first:

```powershell
.\tools\cookbook-schematic\.venv\Scripts\cookbook-report
```

Use deterministic JSON when CI or an AI agent needs to consume the result:

```powershell
.\tools\cookbook-schematic\.venv\Scripts\cookbook-report `
  --format json --output artifacts\cookbook-report.json
```

Then run the compile, simulation, plot, and measurement regression suite:

```powershell
dotnet test src\SpiceSharpParser.Tests\SpiceSharpParser.Tests.csproj `
  --filter 'FullyQualifiedName~CircuitCookbookTests'
```

Portable-SPICE claims should be checked with another simulator when practical.
Native LTspice comparison can continue for LTspice A-device examples.

## Tooling Roadmap

Develop the cookbook tooling in small, independently useful layers.

### Complete: schematic source and rendering

`cookbook-schematic` supplies a versioned TOML layout, netlist-aware
validation, deterministic accessible SVG rendering, and a documented AI
workflow. Continue extending its symbol vocabulary only when a real cookbook
circuit needs a conventional symbol that cannot be expressed clearly today.

### Complete: read-only cookbook report

`cookbook-report` discovers recipes and produces deterministic text or JSON. It
checks the static cookbook contract and compares each checked-in schematic with
a fresh render without overwriting repository files.

### Next: lesson manifest and data-driven coverage

Add one small versioned `lesson.toml` per learner-facing item containing its
content type, module, prerequisites, objectives, netlist, required dialect or
custom mappings, analyses, measurement acceptance ranges, plot declarations,
safety level, and catalog metadata. Migrate `CircuitCookbookTests` from
hard-coded application cases to these manifests so adding a lesson does not
require editing a central C# list.

The report should compare manifest measurement names with `.MEAS` statements,
the guide's Verified Measurements table, prerequisite identifiers, and module
indexes.

### Next: learner-facing cookbook-lab runner

Add a small repository-local CLI that uses existing SpiceSharpParser APIs
rather than new parser features.

```text
cookbook-lab list [--module 05] [--difficulty Simple]
cookbook-lab run <lesson>
cookbook-lab measure <lesson>
cookbook-lab sweep <lesson> <parameter>
cookbook-lab compare <lesson> --variant ideal --variant real
cookbook-lab plot <lesson> --output artifacts/
```

The first version should list lessons, run their declared analyses, print
measurements with units and expected ranges, and return deterministic JSON.
Later versions may drive reviewed sweeps and model comparisons declared in the
manifest. The tool must expose the executed netlist and analysis settings; it
must not become an opaque “answer generator.”

### Later: structured simulation reports

Add a thin .NET runner that compiles each manifest with the correct dialect and
custom mappings, executes its analyses, and returns versioned JSON containing
diagnostics, plot names, measurement values, and convergence status. Integrate
that output into `cookbook-report` only after the JSON contract is stable.

### Later: deterministic response summaries and catalog generation

Generate `response.svg` from reviewed plot specifications plus structured
simulation results, while retaining visual review as a required step. Generate
the catalog and course tables from lesson manifests once the schema has proved
stable across several additional circuits.

## Implementation Milestones

### Learning Foundation release

1. Add `COURSE.md`, `GLOSSARY.md`, `MATH_PRIMER.md`,
   `MODELING_PRIMER.md`, `SIMULATION_PRIMER.md`, and
   `HARDWARE_SAFETY.md`.
2. Define `lesson.toml` by trialing it on the loaded divider, diode envelope
   detector, and one concept lab.
3. Extend `cookbook-report` with opt-in learning-contract checks before making
   new sections mandatory for all existing recipes.
4. Upgrade the loaded-divider guide as the first gold-standard lesson, adding
   prerequisites, model ladder, derivation, prediction error, power, common
   mistakes, and exercises.
5. Add the source-resistor-load, Thévenin/Norton, and RC charge/discharge
   concept labs.

### Device Foundations release

1. Add diode I/V and temperature, diode model comparison, and clipper/clamper
   lessons.
2. Add BJT output curves/load line, bias stability, and current-mirror lessons.
3. Add MOSFET transfer/output curves, switch, source follower, and
   common-source lessons.
4. Add explicit model-card explainers that link parameters to observed curves.
5. Use `.DC`, `.TEMP`, and small-signal comparisons to make model changes
   visible.

### Amplifiers and Feedback release

1. Add the ideal op-amp follower, inverting, non-inverting, summing,
   integrator, and Schmitt-trigger sequence.
2. Add finite gain, GBW, slew rate, offset, output swing, and capacitive-load
   stability lessons.
3. Add current mirror, differential pair, transimpedance, and instrumentation
   amplifier lessons.
4. Upgrade the BJT preamplifier and active anti-alias filter to the full
   learning contract.

### Signal Quality and Systems release

1. Add noise, Fourier distortion, tolerance, temperature, and sensitivity
   laboratories.
2. Add sensor, ADC-drive, sample/hold, R-2R DAC, and current-loop interfaces.
3. Add power regulation/protection and oscillator/modulation sequences.
4. Implement deterministic structured simulation output and generated response
   plots.
5. Publish at least one capstone per major study path.

### Advanced Applications release

1. Implement the buck converter only after the prerequisite magnetics,
   switching, loss, feedback, and thermal lessons exist.
2. Add transmission-line extensions, motor control, class-D audio, and other
   advanced capstones selectively.
3. Cross-check portable lessons with an independent simulator and selected
   hardware measurements where practical and safe.

## Immediate Next Steps

1. Treat the Learning Foundation release as the active roadmap priority.
2. Define the minimal `lesson.toml` schema and prerequisite identifiers.
3. Create the ordered course shell and the math/model/simulation primers.
4. Upgrade loaded voltage divider into the reference lesson.
5. Implement source-resistor-load and Thévenin/Norton concept labs.
6. Implement RC charge/discharge with energy and timestep convergence.
7. Extend `cookbook-report` to report curriculum coverage without failing
   legacy recipes during migration.
8. Migrate cookbook tests from the central hard-coded list to manifests.
9. Implement the first read-only `cookbook-lab list/run/measure` workflow.
10. Keep the buck converter planned until the prerequisite chain is present.

## Keeping the Project Small

Use the following constraints to prevent the cookbook effort from becoming
feature growth:

- A proposed circuit must fit the existing public feature surface.
- Unsupported syntax is a documented limitation, not an automatic parser
  feature request.
- Fix defects in already-supported behavior when a circuit exposes them, but
  do not expand syntax or add component families merely to complete an
  example.
- Keep cookbook examples and visual assets out of NuGet packages unless they
  are required at runtime.
- Keep the curriculum broad by linking shared primers rather than repeating
  the same derivation, safety warning, or simulator tutorial in every lesson.
- Prefer small concept labs and composable classic lessons over duplicating a
  complete application for every minor variation.
- Store compact SVG schematics and plots; do not commit large raw waveform
  datasets.
- Prefer one data-driven test harness for cookbook circuits over substantial
  new test code for every example.
- Use `.MEAS` as the netlist-level source of truth for important outcomes.
- Avoid embedding dated whole-suite pass counts in circuit documents.
- Document expected values and tolerances; let CI report current test counts.
- Prefer one authoritative circuit guide instead of separate requirements,
  documentation, and results files unless the design is genuinely complex.
- Keep generated files reproducible from compact, reviewable source files; do
  not introduce a new generator without a corresponding freshness check.
- Keep report and validation commands read-only unless their command name and
  documentation explicitly promise generation or repair.

## Selection Criteria

When choosing a candidate from the AI example suite or designing a new one,
score it against these questions:

1. Which curriculum gap, prerequisite edge, or practical design need does it
   fill?
2. Is it a concept lab, classic lesson, application recipe, or capstone?
3. Can the physical mechanism be explained honestly at the intended level?
4. Can the governing mathematics be derived without hidden steps?
5. Can a learner make a useful hand prediction before simulation?
6. Does it add a capability or perspective not already taught better by
   another lesson?
7. Can important behavior be measured objectively with supported analyses?
8. Can ideal and non-ideal variants reveal why the approximation works or
   fails?
9. Does it expose voltage, current, power, energy, noise, tolerance, or
   stability—not merely an attractive waveform?
10. Is the result deterministic and fast enough for regression testing?
11. Can it be modified through a small, meaningful parameter set?
12. Are hardware limitations and safety boundaries clear?
13. Does it link backward to prerequisites and forward to an application?
14. Can authoritative sources or data sheets support its real-world claims?

Prefer the smallest circuit that makes the intended idea undeniable. Avoid
minor variations unless the comparison itself teaches a new model, failure
mode, or design trade-off.

## Guiding Rule

Every new item must **explain the physics, derive the mathematics, predict the
behavior, run the model, and reconcile the result**. Application recipes and
capstones must also solve a recognizable problem. None of this requires a new
parser capability unless an independently justified parser requirement exists.
