# .FOUR Statement

> ## Status: `.FOUR` is implemented
>
> Writing `.FOUR` in a netlist attaches transient post-processing collectors to
> existing `.TRAN` simulations. After each transient completes, results are
> stored in `model.FourierAnalyses`.
>
> If you want FFT-style Fourier / THD numbers outside netlist control flow, use
> the [`WaveformAnalyzer` C# helper](#what-you-can-do-today-waveformanalyzer-c).
> It remains a plain C# utility and uses a different FFT-based method.
>
> This article is split into three layers: **the concept** (universal),
> **the C# helper** (`WaveformAnalyzer`), and **the `.FOUR` netlist control**.

---

## The Idea, In Plain Language

Read this first. No math.

Any signal that *repeats* can be rebuilt by adding together a handful of plain
sine waves:

- One **fundamental** tone at the repeat rate (for a 1 kHz buzzer, that is 1 kHz).
- Optional **harmonics**: extra tones at exactly 2×, 3×, 4× … the fundamental.
- A possible **DC** part: a constant offset that does not wiggle at all.

Think of a musical chord. A pure flute note is almost a single tone. The same
note on a distorted electric guitar is the *same* fundamental pitch plus a stack
of harmonics — that extra stack is what makes it sound "dirty."

Fourier analysis is just **measuring how loud each of those tones is** in a
captured waveform:

- A clean sine → almost everything is in the fundamental, harmonics ≈ 0.
- A clipped, buzzy, or switching waveform → big harmonics on top of the
  fundamental.

**THD** (Total Harmonic Distortion) rolls that up into one number: *"what
fraction of the signal is harmonics instead of the clean fundamental?"* 0% means
a perfect sine; 10% means the harmonic junk is about a tenth of the fundamental.

That is the whole idea. Everything below is either (a) the math behind it,
(b) the C# helper path, or (c) the `.FOUR` netlist command.

---

## Two Fourier Paths — Read This Before The Examples

The confusing part of this topic is that two different things share the name
"Fourier." This table is the key:

| Aspect | Available today — `WaveformAnalyzer` (C#) | Available today — `.FOUR` (netlist) |
|--------|---------------------------------------------|--------------------------------|
| How you invoke it | Call `WaveformAnalyzer.THD(...)` / `FFT(...)` from C# on samples you collected | Write `.FOUR 1k V(OUT)` in the netlist |
| Parser support | Not a control — it is a plain C# utility | Implemented as a netlist control |
| Transform used | Zero-pad to a power of two, then Cooley–Tukey radix-2 FFT over the **whole** captured waveform | Resample the **last complete period**, correlate at exact harmonics |
| Sample spacing | Assumes you supply (roughly) uniform samples; **no window** applied | Automatic resampling of the final period |
| Phase | **Not computed** — magnitude only | Cosine-referenced phase in degrees |
| THD harmonics | Harmonics `2 … numHarmonics+1` (default **2–11**), nearest FFT bin | Harmonics 2–9 at exact multiples of $f_0$ |
| Normalized dB | Not provided | Provided per harmonic |
| Result you get | A `double` THD %, or a raw `List<(double Frequency, double Magnitude)>` | Structured `model.FourierAnalyses` |

If you only remember one thing: `WaveformAnalyzer` is a direct C# helper, while
`.FOUR` is the netlist-driven transient post-processor.

---

## How Fourier Analysis Works (The Concept)

This section is universal — it is true for any SPICE-like Fourier analysis and
for the `WaveformAnalyzer` helper. It does not describe a SpiceSharpParser
feature by itself.

### Fundamental Frequency

The *fundamental frequency* $f_0$ is the base repeat rate of the waveform. If the
waveform repeats every period $T$:

$$
f_0 = \frac{1}{T}
$$

$$
T = \frac{1}{f_0}
$$

Harmonic frequencies are integer multiples of $f_0$:

$$
\begin{aligned}
\text{harmonic 1} &= 1 \cdot f_0 \quad \text{(the fundamental itself)} \\
\text{harmonic 2} &= 2 \cdot f_0 \\
\text{harmonic 3} &= 3 \cdot f_0 \\
&\ldots
\end{aligned}
$$

For a 1 kHz analysis:

| Harmonic | Frequency |
|----------|-----------|
| 1 | `1 kHz` |
| 2 | `2 kHz` |
| 3 | `3 kHz` |
| 9 | `9 kHz` |

Common choices:

| Waveform | Period | Correct $f_0$ | Notes |
|----------|--------|--------------|-------|
| 1 kHz sine wave | `1 ms` | `1k` | Harmonic 1 is the sine itself. |
| 10 kHz square wave | `100 us` | `10k` | Odd harmonics at `30k`, `50k`, … |
| 20 kHz pulse train | `50 us` | `20k` | Use the repetition rate, not the edge rate. |
| 60 Hz mains | `16.6667 ms` | `60` | Harmonics at `120 Hz`, `180 Hz`, … |

Choosing the wrong $f_0$ is the easiest way to get misleading results. If the
real signal is 1 kHz but you analyze at 900 Hz, the analyzer probes 900 Hz,
1.8 kHz, 2.7 kHz … the energy no longer lands cleanly on harmonic 1 and spreads
across bins. This is **spectral leakage**.

### Harmonics And THD

| Term | Meaning |
|------|---------|
| DC | Average value of the waveform over the analyzed window. |
| fundamental | Harmonic 1, at $f_0$. |
| second harmonic | Harmonic 2, at $2 \cdot f_0$. |
| THD | RMS sum of the distortion harmonics divided by harmonic 1. |

An ideal sine has only harmonic 1; a distorted sine such as
$\sin(2\pi f_0 t) + 0.1 \cdot \sin(2\pi \cdot 2 f_0 \cdot t)$ has a
fundamental of `1` and a second harmonic of `0.1`, giving a THD of about `10%`.
THD ignores DC and ignores harmonic 1 (the reference).

### Math Background

A repeating waveform is a DC value plus sine and cosine waves at integer
multiples of $f_0$. Over one period $T = 1 / f_0$:

$$
x(t) = \text{dc} + \sum_{k=1}^{H}
\left[a_k \cos(2\pi k f_0 t) + b_k \sin(2\pi k f_0 t)\right]
$$

| Symbol | Meaning |
|--------|---------|
| $x(t)$ | Signal being analyzed, e.g. `V(OUT)`. |
| $\text{dc}$ | Average value of $x(t)$ over one period. |
| $k$ | Harmonic number. |
| $H$ | Highest harmonic considered. |
| $a_k$, $b_k$ | Cosine / sine coefficients for harmonic $k$. |
| $f_0$ | Fundamental frequency. |

#### Correlation In Plain Language

Sine/cosine correlation asks, *"how much does this waveform look like a sine or
cosine at this exact frequency?"* For each harmonic `k` the waveform is compared
to $\cos(2\pi k f_0 t)$ and $\sin(2\pi k f_0 t)$. If the waveform rises and
falls in the same pattern as the reference, the sum is large; if it has unrelated
shape, the positive and negative parts cancel and the sum is small. That is why
a pure 1 kHz sine correlates strongly at 1 kHz and weakly at 2 kHz.

#### Discrete Formulas

For `M` uniformly spaced samples `x[0]…x[M-1]` covering the analyzed window:

$$
\text{dc} = \frac{1}{M} \sum_{n=0}^{M-1} x[n]
$$

$$
a_k = \frac{2}{M} \sum_{n=0}^{M-1} x[n]\cos\left(\frac{2\pi k n}{M}\right)
$$

$$
b_k = \frac{2}{M} \sum_{n=0}^{M-1} x[n]\sin\left(\frac{2\pi k n}{M}\right)
$$

#### Magnitude And Phase

$$
\text{magnitude}_k = \sqrt{a_k^2 + b_k^2}
$$

$$
\text{phase}_k = \operatorname{atan2}(-b_k, a_k) \cdot \frac{180}{\pi}
$$

Phase is **conceptual here** — note that the `WaveformAnalyzer` helper does not
compute phase (magnitude only). With a cosine reference:

| Waveform | Phase |
|----------|-------|
| $\cos(2\pi f_0 t)$ | about `0 deg` |
| $\sin(2\pi f_0 t)$ | about `-90 deg` |
| $-\cos(2\pi f_0 t)$ | about `±180 deg` |
| $-\sin(2\pi f_0 t)$ | about `90 deg` |

#### Normalized Magnitude And dB

$$
\text{normalized}_k = \frac{\text{magnitude}_k}{\text{magnitude}_1}
$$

$$
\text{normalized\_db}_k = 20 \cdot \log_{10}(\text{normalized}_k)
$$

| $\text{normalized}_k$ | $\text{normalized\_db}_k$ | Meaning |
|----------------|-------------------|---------|
| `1` | `0 dB` | Equal to the fundamental. |
| `0.5` | about `-6.02 dB` | Half the fundamental. |
| `0.1` | `-20 dB` | One tenth of the fundamental. |
| `0.01` | `-40 dB` | One hundredth of the fundamental. |

#### THD

$$
\text{THD percent} =
100 \cdot
\frac{\sqrt{\text{magnitude}_2^2 + \text{magnitude}_3^2 + \ldots}}
     {\text{magnitude}_1}
$$

- DC does not count toward THD.
- Harmonic 1 does not count (it is the reference).
- If $\text{magnitude}_1 \approx 0$, THD is undefined (`NaN`) — no useful
  denominator.

#### Worked Math Examples

Pure cosine $x(t) = \cos(2\pi f_0 t)$:

| Quantity | Value |
|----------|-------|
| $\text{dc}$ | `0` |
| $\text{magnitude}_1$ | `1` |
| $\text{phase}_1$ (concept) | `0 deg` |
| `THD` | `0%` |

Pure sine $x(t) = \sin(2\pi f_0 t)$:

| Quantity | Value |
|----------|-------|
| $\text{dc}$ | `0` |
| $\text{magnitude}_1$ | `1` |
| $\text{phase}_1$ (concept) | `-90 deg` |
| `THD` | `0%` |

DC offset plus sine $x(t) = 2 + \sin(2\pi f_0 t)$:

| Quantity | Value |
|----------|-------|
| $\text{dc}$ | `2` |
| $\text{magnitude}_1$ | `1` |
| `THD` | `0%` (offset is not distortion) |

Fundamental plus second harmonic
$x(t) = \sin(2\pi f_0 t) + 0.1 \cdot \sin(2\pi \cdot 2 f_0 \cdot t)$:

| Quantity | Value |
|----------|-------|
| $\text{magnitude}_1$ | `1` |
| $\text{magnitude}_2$ | `0.1` |
| $\text{normalized}_2$ | `0.1` (`-20 dB`) |
| `THD` | `10%` |

Ideal 50% square wave — odd harmonics only, relative to the fundamental:

| Harmonic | Normalized magnitude | Normalized dB |
|----------|----------------------|---------------|
| 1 | `1` | `0 dB` |
| 3 | $1/3 \approx 0.333$ | about `-9.54 dB` |
| 5 | $1/5 = 0.2$ | about `-13.98 dB` |
| 7 | $1/7 \approx 0.143$ | about `-16.90 dB` |
| 9 | $1/9 \approx 0.111$ | about `-19.08 dB` |

Real simulated square waves have finite edges, so high harmonics are smaller
than these ideal ratios.

---

## What You Can Do Today: `WaveformAnalyzer` (C#)

This is the **only Fourier path that actually runs today.** It is a plain static
helper, not a netlist control. Source:
[WaveformAnalyzer.cs](../../SpiceSharpParser/Analysis/WaveformAnalyzer.cs)
(namespace `SpiceSharpParser.Analysis`).

You run a normal `.TRAN` simulation, collect `(time, value)` samples yourself,
then call the helper.

### The real API

```csharp
// Magnitude spectrum, bins 0 .. Nyquist
List<(double Frequency, double Magnitude)> FFT(
    List<(double Time, double Value)> data);

// Total Harmonic Distortion, as a percentage
double THD(
    List<(double Time, double Value)> data,
    double fundamentalFreq,
    int numHarmonics = 10);

// Useful siblings
double DCOffset(List<(double Time, double Value)> data);
double RMS(List<(double Time, double Value)> data,
           double fromTime = double.MinValue, double toTime = double.MaxValue);
double Average(List<(double Time, double Value)> data,
               double fromTime = double.MinValue, double toTime = double.MaxValue);
double SNR(List<(double Time, double Value)> data, double signalFreq); // dB
```

### How it actually computes (be aware of these)

`FFT`
([WaveformAnalyzer.cs:232](../../SpiceSharpParser/Analysis/WaveformAnalyzer.cs#L232)):

- Needs **≥ 4** samples, otherwise it returns an empty list.
- **Zero-pads** the values up to the next power of two, then runs an in-place
  Cooley–Tukey radix-2 FFT over the whole captured waveform.
- Derives the sample rate as `(count − 1) / (lastTime − firstTime)`, i.e. it
  **assumes roughly uniform sample spacing**. SpiceSharp's transient steps are
  adaptive (non-uniform), so the frequency axis is approximate unless you keep
  the step small and even.
- Returns `(frequency, magnitude)` for bins `0 … n/2`, where
  $\text{magnitude} = \sqrt{re^2 + im^2} \cdot 2 / \text{count}$ and the DC bin
  is halved.
- Applies **no window** (rectangular). Capturing a non-integer number of periods
  leaks energy between bins.
- **No phase** is computed or returned — magnitude only.

`THD`
([WaveformAnalyzer.cs:287](../../SpiceSharpParser/Analysis/WaveformAnalyzer.cs#L287)):

- Takes the FFT, then reads the **nearest FFT bin** to `fundamentalFreq` (no
  interpolation between bins).
- Sums squared nearest-bin magnitudes for harmonics `h = 2 … numHarmonics + 1`.
  So the default `numHarmonics = 10` covers harmonics **2 through 11** — *not*
  2–9. Pass `numHarmonics: 8` to get the classic 2–9 range.
- Returns $100 \cdot \sqrt{\sum \text{harmonic}^2} / \text{fundamentalMag}$.
- Returns `double.NaN` if the spectrum is empty (too few samples) or the
  nearest-bin fundamental magnitude is `≤ 0`.

> **Practical tip.** Because the FFT assumes uniform spacing and nearest-bin
> lookup, the cleanest results come from: a small, even `.TRAN` max step; a
> capture that is an integer number of fundamental periods; and an $f_0$ that
> lands close to an FFT bin. Treat magnitudes as good estimates, not exact
> reference values.

### Runnable end-to-end example

Netlist — a 1 kHz fundamental plus a 2 kHz second harmonic ($\approx 10\%$ THD):

```spice
Second harmonic distortion
V1 A 0 SIN(0 1 1k)
V2 A OUT SIN(0 0.1 2k)
R1 OUT 0 1k
.TRAN 2u 10m 0 2u
.SAVE V(OUT)
.END
```

C# — run the transient, collect samples, compute THD/FFT (same run/extract
idiom as the [.TRAN article](tran.md)):

```csharp
using SpiceSharp.Simulations;        // Transient
using SpiceSharpParser.Analysis;     // WaveformAnalyzer

// model = the parsed netlist above (it contains a .TRAN analysis)
var sim  = model.Simulations.First(s => s is Transient);
var vout = model.Exports.Find(e => e.Name == "V(OUT)");

var samples = new List<(double Time, double Value)>();
sim.EventExportData += (sender, args) =>
{
    double time = ((Transient)sim).Time;
    samples.Add((time, vout.Extract()));
};

foreach (var _ in sim.InvokeEvents(sim.Run(model.Circuit))) { /* drive the run */ }

double thdPercent = WaveformAnalyzer.THD(samples, 1000.0);   // f0 = 1 kHz
double dc         = WaveformAnalyzer.DCOffset(samples);
var spectrum      = WaveformAnalyzer.FFT(samples);           // (freq, magnitude)

Console.WriteLine($"THD ≈ {thdPercent:F1} %, DC ≈ {dc:F4}");
foreach (var (freq, mag) in spectrum.Where(s => s.Magnitude > 0.01))
    Console.WriteLine($"  {freq,8:F0} Hz : {mag:F4}");
```

Expected: THD on the order of `~10%`, a strong bin near `1 kHz`, and a smaller
bin near `2 kHz`. Exact figures depend on step size and how many whole periods
were captured (see the practical tip above).

---

## `.FOUR` Netlist Support

### How `.FOUR` Works

To get THD from a netlist, add **one line to the netlist** and read the
structured results after the transient run:

```spice
.TRAN 1u 10m 0 2u
.FOUR 1k V(OUT)
```

Here is the story end to end:

1. **You declare intent in the netlist.** `.FOUR 1k V(OUT)` means *"after the
   transient, decompose `V(OUT)` assuming it repeats at 1 kHz."* No C# required.
2. **The `.TRAN` runs as normal.** `.FOUR` adds nothing to the simulation — it
   is pure post-processing, like `.MEAS`.
3. **The `.FOUR` reader picks the right slice of data for you.** It computes the
   period $T = 1 / f_0$, then takes the **last complete period** before the end of
   the run — automatically skipping startup transients you would otherwise have
   to trim by hand.
4. **It measures exactly at your harmonic frequencies.** Instead of an FFT whose
   bins may not line up with $f_0$, it correlates the waveform against
   $\cos$/$\sin$ at exactly $f_0, 2 \cdot f_0, 3 \cdot f_0, \ldots$ — so
   harmonic 3 is *exactly* 3 kHz, not "the nearest bin."
5. **It hands back a finished result object** with magnitude, phase, normalized
   dB, and THD already computed, one per analyzed signal.

So the value of `.FOUR` over the C# helper is: it chooses the settled period for
you, it has no bin-rounding error, it gives phase and normalized dB, and the
result is structured per signal — none of which the current `WaveformAnalyzer`
does.

### Syntax

```spice
.FOUR <fundamental_frequency> <expr1> [<expr2> ...]
```

| Parameter | Description |
|-----------|-------------|
| `fundamental_frequency` | Base repeating frequency in hertz; positive and finite. |
| `expr` | Signal expression, e.g. `V(OUT)`, `I(V1)`. |

Examples: `.FOUR 1k V(OUT)`, `.FOUR 60 V(LINE) I(VLOAD)`,
`.FOUR {freq} V(IN) V(OUT)`. `.FOUR` requires a `.TRAN` analysis and
post-processes its samples.

### How The Analysis Runs, Step By Step

Each step below says *what* happens and *why* — this is the internal flow the
reader follows:

| Step | What it does | Why |
|------|--------------|-----|
| 1 | Run the `.TRAN` simulation. | `.FOUR` needs a transient waveform to analyze. |
| 2 | Collect the requested signal samples during the run (via the same export event used by `.MEAS`). | Captures `V(OUT)` over time. |
| 3 | Compute one period $T = 1 / f_0$. | $f_0$ from your `.FOUR` line defines what "one cycle" means. |
| 4 | Select the **last complete period** before the final time. | The first cycles often contain startup transients; the last settled cycle is the steady-state answer. |
| 5 | Resample that period onto evenly spaced points. | Transient steps are non-uniform; the correlation math needs even spacing. |
| 6 | Correlate against $\cos$/$\sin$ at exactly $f_0, 2 \cdot f_0, \ldots$. | Measures each harmonic at its *true* frequency — no FFT bin rounding. |
| 7 | Convert to magnitude, phase, normalized magnitude, and dB. | The human-readable per-harmonic numbers. |
| 8 | Compute THD from harmonics 2–9 vs. harmonic 1. | One distortion figure of merit. |
| 9 | Store a structured result per signal. | Read it later from `model.FourierAnalyses`. |

#### Worked trace: what `.FOUR 1k V(OUT)` produces

Take the *sine plus second harmonic* netlist below, with `.TRAN 1u 10m 0 2u`.
Follow the steps:

$$
V(OUT) \approx
\sin(2\pi \cdot 1\,\text{kHz} \cdot t) +
0.1 \cdot \sin(2\pi \cdot 2\,\text{kHz} \cdot t)
$$

- **Step 3** — $f_0 = 1 \text{ kHz}$, so $T = 1 \text{ ms}$.
- **Step 4** — the run is `10 ms` = 10 periods. The reader keeps the window
  roughly `9 ms … 10 ms` (the last whole period). Startup is irrelevant here, but
  this is what protects you in circuits that need to settle.
- **Step 5** — that 1 ms slice is resampled to evenly spaced points.
- **Step 6** — correlating at `1 kHz` finds a strong match (the `sin` term);
  at `2 kHz` a weaker match (the `0.1` term); at `3–9 kHz` ≈ 0.
- **Step 7–8** — the result becomes:

| Harmonic | Frequency | Magnitude | Normalized | Normalized dB |
|----------|-----------|-----------|------------|---------------|
| 0 (DC) | `0` | ≈ `0` | — | — |
| 1 | `1 kHz` | ≈ `1.0` | `1.0` | `0 dB` |
| 2 | `2 kHz` | ≈ `0.1` | `0.1` | `-20 dB` |
| 3–9 | `3–9 kHz` | ≈ `0` | ≈ `0` | very negative |

  $\text{THD} = 100 \cdot \sqrt{0.1^2 + 0^2 + \ldots} / 1.0 \approx 10\%$,
  and $\text{phase}_1 \approx -90^\circ$
  (a pure `sin` against a cosine reference).

That whole table is what you read back from one `FourierAnalysisResult` —
no FFT code, no manual period trimming.

> Contrast with `WaveformAnalyzer`: it zero-pads and FFTs the entire capture,
> looks up the *nearest bin* to 1 kHz and 2 kHz, gives you no phase, and (with
> the default `numHarmonics`) rolls harmonics 2–11 into THD.

### C# API

You do not write any FFT or sampling code — you just run the simulations and
read `model.FourierAnalyses`. One entry is stored per `.FOUR` signal and per
transient simulation:

```csharp
RunSimulations(model);

foreach (var result in model.FourierAnalyses)
{
    // e.g. "V(OUT)  f0 = 1000 Hz  THD = 10.0 %"
    Console.WriteLine($"{result.SignalName}  f0 = {result.FundamentalFrequency} Hz" +
                      $"  THD = {result.TotalHarmonicDistortionPercent:F1} %");

    foreach (var h in result.Harmonics)
        Console.WriteLine($"  h{h.HarmonicNumber} @ {h.Frequency} Hz : " +
                          $"mag={h.Magnitude:F4}  {h.NormalizedMagnitudeDecibels:F1} dB" +
                          $"  phase={h.PhaseDegrees:F0} deg");
}
```

For the worked trace above, that loop prints harmonic 1 at ≈ `1.0`
(`0 dB`), harmonic 2 at ≈ `0.1` (`-20 dB`), the rest near zero, and a top-line
THD of ≈ `10 %` — i.e. you read the result table directly instead of computing
it. Contrast this with the [direct C# helper path](#runnable-end-to-end-example),
which returns only a bare `double` and a `(freq, magnitude)` list.

`FourierAnalysisResult`:

| Field | Meaning |
|-------|---------|
| `SignalName` | Analyzed expression, e.g. `V(OUT)`. |
| `SimulationName` | Transient simulation that produced this result. |
| `FundamentalFrequency` | Frequency passed to `.FOUR`. |
| `TotalHarmonicDistortionPercent` | THD from harmonics 2–9. |
| `Harmonics` | Rows for DC and harmonics 1–9. |
| `Success` | `true` when a complete final-period result was computed. |
| `ErrorMessage` | Reason the result failed, when `Success` is `false`. |

Harmonic row: `HarmonicNumber`, `Frequency`, `Magnitude`,
`PhaseDegrees` (cosine reference), `NormalizedMagnitude`,
`NormalizedMagnitudeDecibels`.

### Circuit examples

Each "expected" table below is what *theory* predicts and what `.FOUR` reports
for sufficiently settled simulations with small enough time steps.

**Pure sine**

```spice
Pure sine Fourier analysis
V1 IN 0 SIN(0 1 1k)
R1 IN 0 1k
.TRAN 1u 10m 0 2u
.FOUR 1k V(IN)
.END
```

| Result | Predicted |
|--------|-----------|
| harmonic 1 magnitude | `1` |
| harmonics 2–9 | near `0` |
| THD | near `0%` |

**Sine plus second harmonic**

```spice
Second harmonic distortion
V1 A 0 SIN(0 1 1k)
V2 A OUT SIN(0 0.1 2k)
R1 OUT 0 1k
.TRAN 1u 10m 0 2u
.FOUR 1k V(OUT)
.END
```

| Result | Predicted |
|--------|-----------|
| harmonic 1 magnitude | `1` |
| harmonic 2 magnitude | `0.1` (`-20 dB` normalized) |
| THD | `10%` |

**Square wave odd harmonics**

```spice
Square wave Fourier analysis
V1 OUT 0 PULSE(-1 1 0 100n 100n 500u 1m)
R1 OUT 0 1k
.TRAN 1u 10m 0 2u
.FOUR 1k V(OUT)
.END
```

| Result | Predicted |
|--------|-----------|
| harmonic 2 | near `0` |
| harmonic 3 normalized | about `0.333` |
| harmonic 5 normalized | about `0.2` |
| harmonic 7 normalized | about `0.143` |

**RC low-pass at cutoff** (`R = 1k`, `C = 159.154943n` → `fc ≈ 1 kHz`)

```spice
RC low-pass Fourier analysis
V1 IN 0 SIN(0 1 1k)
R1 IN OUT 1k
C1 OUT 0 159.154943n
.TRAN 1u 10m 0 2u
.FOUR 1k V(IN) V(OUT)
.END
```

| Result | Predicted |
|--------|-----------|
| `V(IN)` harmonic 1 | `1` |
| `V(OUT)` harmonic 1 | `≈ 0.707` |
| `V(OUT)` phase shift | about `-45 deg` |

**Low-pass harmonic cleanup**

```spice
Low-pass harmonic cleanup
V1 IN 0 SIN(0 1 1k)
V3 IN MID SIN(0 0.2 3k)
R1 MID OUT 1k
C1 OUT 0 159.154943n
.TRAN 1u 10m 0 2u
.FOUR 1k V(MID) V(OUT)
.END
```

| Result | Predicted |
|--------|-----------|
| `V(MID)` THD | about `20%` |
| `V(OUT)` THD | about `9%` (filter attenuates the 3 kHz harmonic) |

**Wrong fundamental frequency** — source is 1 kHz but `.FOUR` asks for 900 Hz:

```spice
Wrong fundamental frequency example
V1 IN 0 SIN(0 1 1k)
R1 IN 0 1k
.TRAN 1u 10m 0 2u
.FOUR 900 V(IN)
.END
```

Harmonic 1 would *not* be near `1`, higher harmonics would not be near zero, and
THD would look nonzero — all because of spectral leakage from the mismatched
$f_0$. Fix: `.FOUR 1k V(IN)`.

### `.TRAN` guidance

These rules matter for `.FOUR` and also matter when using `WaveformAnalyzer`
directly:

- **Simulate enough periods** so startup behavior settles before the final time
  (e.g. `.TRAN 1u 10m 0 2u` = ten periods of a 1 kHz wave).
- **Use a small, even max step.** To resolve up to the 9th harmonic of a 1 kHz
  analysis (9 kHz, ~111 µs period), a 2 µs step gives ~55 samples per
  9th-harmonic period. Aim for ≥ 20 samples/period for rough THD, 50–100 for
  good magnitudes.
- **Watch for unsettled output**: if fundamental magnitude or THD changes when
  you increase `tstop`, the final cycle is not steady state yet.
- **For filters**, analyze input and output together to compare attenuation and
  THD before/after.

### Troubleshooting

| Symptom | Likely cause | Check |
|---------|--------------|-------|
| `.FOUR` produces nothing | No `.TRAN`, invalid signal, or not enough final-period data | Ensure a `.TRAN` exists, the signal is valid, and the run contains one complete final period. |
| Harmonics appear in a pure sine | Wrong $f_0$, coarse step, or unsettled waveform | Match $f_0$ to the source; reduce max step. |
| THD changes when `tstop` changes | Final waveform not settled | Simulate more periods. |
| THD is `NaN` | Fundamental magnitude ≈ 0 | Confirm harmonic 1 is the intended reference frequency. |
| Phase missing | `WaveformAnalyzer` computes no phase | Use `.FOUR` when you need phase. |

### Limitations

- Requires a `.TRAN` analysis and at least one complete settled period.
- Support targets harmonics 0–9 (0 = DC); THD from harmonics 2–9.
  (Contrast with today's `WaveformAnalyzer.THD`, default harmonics **2–11**.)
- The API exposes structured results, not LTspice's exact textual report.
- Phase uses a cosine reference and may not match other simulators.
- Accuracy always depends on step size, settling, and capture length.

---

## See Also

- [.TRAN](tran.md) — produces the transient samples Fourier analysis consumes.
- [.MEAS](meas.md) — other transient post-processing measurements.
- [`WaveformAnalyzer`](../../SpiceSharpParser/Analysis/WaveformAnalyzer.cs) — the
  working FFT/THD/RMS source.
