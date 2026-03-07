# <img src="https://spicesharp.github.io/SpiceSharp/api/images/logo_full.svg" width="45px" /> Spice#/SpiceSharpParser
 [<img src="https://img.shields.io/nuget/vpre/SpiceSharp-Parser.svg">]( https://www.nuget.org/packages/SpiceSharp-Parser)

SpiceSharpParser is a .NET library that allows to parse SPICE netlists and to simulate them using SpiceSharp.

## Installation

SpiceSharpParser is available as [NuGet Package](https://www.nuget.org/packages/SpiceSharp-Parser).

## Quickstart

Parsing a netlist and executing a simulation is relatively straightforward. For example:

```csharp
using System;
using System.Linq;
using SpiceSharpParser;

namespace SpiceSharpParserExample
{
    class Program
    {
        static void Main(string[] programArgs)
        {
            var netlistText = string.Join(Environment.NewLine,
                "Diode circuit",
                "D1 OUT 0 1N914",
                "V1 OUT 0 0",
                ".model 1N914 D(Is=2.52e-9 Rs=0.568 N=1.752 Cjo=4e-12 M=0.4 tt=20e-9)",
                ".DC V1 -1 1 10e-3",
                ".SAVE i(V1)",
                ".END");

            // Parsing part
            var parser = new SpiceNetlistParser();
            var parseResult = parser.ParseNetlist(netlistText);
            var netlist = parseResult.FinalModel;

            // Translating netlist model to SpiceSharp
            var reader = new SpiceSharpReader();
            var spiceSharpModel = reader.Read(netlist);

            // Simulation using SpiceSharp
            var simulation = spiceSharpModel.Simulations.Single();
            var export = spiceSharpModel.Exports.Find(e => e.Name == "i(V1)");
            simulation.EventExportData += (sender, args) => Console.WriteLine(export.Extract());
            var codes = simulation.Run(spiceSharpModel.Circuit, -1);
            codes = simulation.InvokeEvents(codes);
            codes.ToArray(); //eval
        }
    }
}    

```
## Compatibility
### PSpice
SpiceSharpParser is able to parse some of PSpice netlists. 
At the moment due to lack of implementation of LAPLACE and FREQ (part of analog behavioral modeling) and other features parsing or simulation can fail.


## Capabilities
### Analog Behavioral Modeling supported:
* POLY(n)
* TABLE 
* VALUE

### Dot statements supported:
|  Statement  |  Documentation   |
|:------------|-----------------------:|
|.AC          |[Docs](src/docs/articles/ac.md)|
|.APPENDMODEL |[Docs](src/docs/articles/appendmodel.md)|
|.DC          |[Docs](src/docs/articles/dc.md)|
|.DISTRIBUTION|[Docs](src/docs/articles/distribution.md)|
|.ELSE        |[Docs](src/docs/articles/if.md)|
|.ENDIF       |[Docs](src/docs/articles/if.md)|
|.FUNC        |[Docs](src/docs/articles/func.md)|
|.GLOBAL      |[Docs](src/docs/articles/global.md)|
|.IC          |[Docs](src/docs/articles/ic.md)|
|.IF          |[Docs](src/docs/articles/if.md)|
|.INCLUDE     |[Docs](src/docs/articles/include.md)|
|.LET         |[Docs](src/docs/articles/let.md)|
|.LIB         |[Docs](src/docs/articles/lib.md)|
|.MC          |[Docs](src/docs/articles/mc.md)|
|.MEAS        |[Docs](src/docs/articles/meas.md)|
|.MEASURE     |[Docs](src/docs/articles/meas.md)|
|.NODESET     |[Docs](src/docs/articles/nodeset.md)|
|.NOISE       |[Docs](src/docs/articles/noise.md)|
|.OP          |[Docs](src/docs/articles/op.md)|
|.OPTIONS     |[Docs](src/docs/articles/options.md)|
|.PARAM       |[Docs](src/docs/articles/param.md)|
|.PLOT        |[Docs](src/docs/articles/plot.md)|
|.PRINT       |[Docs](src/docs/articles/print.md)|
|.TRAN        |[Docs](src/docs/articles/tran.md)|
|.SAVE        |[Docs](src/docs/articles/save.md)|
|.SPARAM      |[Docs](src/docs/articles/sparam.md)|
|.ST          |[Docs](src/docs/articles/st.md)|
|.STEP        |[Docs](src/docs/articles/step.md)|
|.SUBCKT      |[Docs](src/docs/articles/subckt.md)|
|.TEMP        |[Docs](src/docs/articles/temp.md)|

### Device statements supported:
| Device Statement  |  Documentation   |
|:------------|-----------------------:|
|B (Arbitrary Behavioral Voltage or Current Source)|[Docs](src/docs/articles/behavioral-source.md)|
|C (Capacitor)|[Docs](src/docs/articles/capacitor.md)|
|D (Diode)|[Docs](src/docs/articles/diode.md)|
|E (Voltage-Controlled Voltage Source)|[Docs](src/docs/articles/vcvs.md)|
|F (Current-Controlled Current Source)|[Docs](src/docs/articles/cccs.md)|
|G (Voltage-Controlled Current Source)|[Docs](src/docs/articles/vccs.md)|
|H (Current-Controlled Voltage Source)|[Docs](src/docs/articles/ccvs.md)|
|I (Independent Current Source)|[Docs](src/docs/articles/current-source.md)|
|J (JFET)|[Docs](src/docs/articles/jfet.md)|
|K (Mutual Inductance)|[Docs](src/docs/articles/mutual-inductance.md)|
|L (Inductor)|[Docs](src/docs/articles/inductor.md)|
|M (Mosfet)|[Docs](src/docs/articles/mosfet.md)|
|Q (Bipolar Junction Transistor)|[Docs](src/docs/articles/bjt.md)|
|R (Resistor)|[Docs](src/docs/articles/resistor.md)|
|S (Voltage Switch)|[Docs](src/docs/articles/voltage-switch.md)|
|T (Lossless Transmission Line)|[Docs](src/docs/articles/transmission-line.md)|
|V (Independent Voltage Source)|[Docs](src/docs/articles/voltage-source.md)|
|W (Current Switch)|[Docs](src/docs/articles/current-switch.md)|
|X (Subcircuit)|[Docs](src/docs/articles/subcircuit-instance.md)|

## Documentation
* Documentation articles are available in [src/docs/articles](src/docs/articles).
* API documentation is available at <https://spicesharp.github.io/SpiceSharpParser/api/index.html>.

## License
SpiceSharpParser is under MIT License

[![FOSSA Status](https://app.fossa.com/api/projects/git%2Bgithub.com%2FSpiceSharp%2FSpiceSharpParser.svg?type=large)](https://app.fossa.com/projects/git%2Bgithub.com%2FSpiceSharp%2FSpiceSharpParser?ref=badge_large)
