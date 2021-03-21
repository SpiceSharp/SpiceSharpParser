# <img src="https://spicesharp.github.io/SpiceSharp/api/images/logo_full.svg" width="45px" /> Spice#/SpiceSharpParser
 [<img src="https://img.shields.io/nuget/vpre/SpiceSharp-Parser.svg">]( https://www.nuget.org/packages/SpiceSharp-Parser)
[![Windows](https://ci.appveyor.com/api/projects/status/d8tpj2hm3hcullmw/branch/master?svg=true)](https://ci.appveyor.com/project/marcin-golebiowski/spicesharpparser/branch/master)
[![Linux](https://travis-ci.org/SpiceSharp/SpiceSharpParser.svg?branch=master)](https://travis-ci.org/SpiceSharp/SpiceSharpParser?branch=master)
[![codecov](https://codecov.io/gh/SpiceSharp/SpiceSharpParser/branch/master/graph/badge.svg)](https://codecov.io/gh/SpiceSharp/SpiceSharpParser)
[![CodeFactor](https://www.codefactor.io/repository/github/spicesharp/spicesharpparser/badge)](https://www.codefactor.io/repository/github/spicesharp/spicesharpparser)
[![Sonarcloud Status](https://sonarcloud.io/api/project_badges/measure?project=SpiceSharpParser&metric=alert_status)](https://sonarcloud.io/dashboard?id=SpiceSharpParser)
[![Sonarcloud Status](https://sonarcloud.io/api/project_badges/measure?project=SpiceSharpParser&metric=bugs)](https://sonarcloud.io/dashboard?id=SpiceSharpParser)
[![Sonarcloud Status](https://sonarcloud.io/api/project_badges/measure?project=SpiceSharpParser&metric=code_smells)](https://sonarcloud.io/dashboard?id=SpiceSharpParser)
[![Lines of Code](https://sonarcloud.io/api/project_badges/measure?project=SpiceSharpParser&metric=ncloc)](https://sonarcloud.io/dashboard?id=SpiceSharpParser)
[![Duplicated Lines (%)](https://sonarcloud.io/api/project_badges/measure?project=SpiceSharpParser&metric=duplicated_lines_density)](https://sonarcloud.io/dashboard?id=SpiceSharpParser)
[![FOSSA Status](https://app.fossa.io/api/projects/git%2Bgithub.com%2Fmarcin-golebiowski%2FSpiceSharpParser.svg?type=shield)](https://app.fossa.io/projects/git%2Bgithub.com%2Fmarcin-golebiowski%2FSpiceSharpParser?ref=badge_shield)


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
            var netlist = string.Join(Environment.NewLine,
                "Diode circuit",
                "D1 OUT 0 1N914",
                "V1 OUT 0 0",
                ".model 1N914 D(Is=2.52e-9 Rs=0.568 N=1.752 Cjo=4e-12 M=0.4 tt=20e-9)",
                ".DC V1 -1 1 10e-3",
                ".SAVE i(V1)",
                ".END");

            // Parsing part - SpiceSharpParser
            var parser = new SpiceParser();
            var parseResult = parser.ParseNetlist(netlist);
            var spiceModel = parseResult.SpiceModel;

            // Simulation part - SpiceSharp
            var simulation = spiceModel.Simulations.Single();
            var export = spiceModel.Exports.Find(e => e.Name == "i(V1)");
            simulation.ExportSimulationData += (sender, args) => Console.WriteLine(export.Extract());
            simulation.Run(spiceModel.Circuit);
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

Note: Only voltage and current expressions are supported in TABLE, VALUE

### Dot statements supported:
|  Statement  |  Documentation   |
|:------------|-----------------------:|
|.AC          |[Wiki](https://github.com/SpiceSharp/SpiceSharpParser/wiki/.AC)|
|.APPENDMODEL |[Wiki](https://github.com/SpiceSharp/SpiceSharpParser/wiki/.APPENDMODEL)|
|.DC          |[Wiki](https://github.com/SpiceSharp/SpiceSharpParser/wiki/.DC)|
|.DISTRIBUTION|[Wiki](https://github.com/SpiceSharp/SpiceSharpParser/wiki/.DISTRIBUTION)|
|.ELSE        |[Wiki](https://github.com/SpiceSharp/SpiceSharpParser/wiki/.ELSE)|
|.ENDIF       |[Wiki](https://github.com/SpiceSharp/SpiceSharpParser/wiki/.ENDIF)|
|.FUNC        |[Wiki](https://github.com/SpiceSharp/SpiceSharpParser/wiki/.FUNC)|
|.GLOBAL      |[Wiki](https://github.com/SpiceSharp/SpiceSharpParser/wiki/.GLOBAL)|         
|.IC          |[Wiki](https://github.com/SpiceSharp/SpiceSharpParser/wiki/.IC)|
|.IF          |[Wiki](https://github.com/SpiceSharp/SpiceSharpParser/wiki/.IF)|
|.INCLUDE     |[Wiki](https://github.com/SpiceSharp/SpiceSharpParser/wiki/.INCLUDE)|
|.LET         |[Wiki](https://github.com/SpiceSharp/SpiceSharpParser/wiki/.LET)|
|.LIB         |[Wiki](https://github.com/SpiceSharp/SpiceSharpParser/wiki/.LIB)|
|.MC          |[Wiki](https://github.com/SpiceSharp/SpiceSharpParser/wiki/.MC)|
|.NODESET     |[Wiki](https://github.com/SpiceSharp/SpiceSharpParser/wiki/.NODESET)|
|.NOISE       |[Wiki](https://github.com/SpiceSharp/SpiceSharpParser/wiki/.NOISE)|
|.OP          |[Wiki](https://github.com/SpiceSharp/SpiceSharpParser/wiki/.OP)|
|.OPTIONS     |[Wiki](https://github.com/SpiceSharp/SpiceSharpParser/wiki/.OPTIONS)|
|.PARAM       |[Wiki](https://github.com/SpiceSharp/SpiceSharpParser/wiki/.PARAM)|
|.PLOT        |[Wiki](https://github.com/SpiceSharp/SpiceSharpParser/wiki/.PLOT)|
|.PRINT       |[Wiki](https://github.com/SpiceSharp/SpiceSharpParser/wiki/.PRINT)|
|.TRAN        |[Wiki](https://github.com/SpiceSharp/SpiceSharpParser/wiki/.TRAN)|
|.SAVE        |[Wiki](https://github.com/SpiceSharp/SpiceSharpParser/wiki/.SAVE)|
|.SPARAM       |[Wiki](https://github.com/SpiceSharp/SpiceSharpParser/wiki/.SPARAM)|
|.ST          |[Wiki](https://github.com/SpiceSharp/SpiceSharpParser/wiki/.ST)||
|.STEP        |[Wiki](https://github.com/SpiceSharp/SpiceSharpParser/wiki/.STEP)|
|.SUBCKT      |[Wiki](https://github.com/SpiceSharp/SpiceSharpParser/wiki/.SUBCKT)|
|.TEMP        |[Wiki](https://github.com/SpiceSharp/SpiceSharpParser/wiki/.TEMP)|

### Device statements supported:
| Device Statement  |  Documentation   |
|:------------|-----------------------:|
|B (Arbitrary Behavioral Voltage or Current Source)|[Wiki](https://github.com/SpiceSharp/SpiceSharpParser/wiki/B)|
|C (Capacitor)|[Wiki](https://github.com/SpiceSharp/SpiceSharpParser/wiki/C)|
|D (Diode)|[Wiki](https://github.com/SpiceSharp/SpiceSharpParser/wiki/D)|
|E (Voltage-Controlled Voltage Source)|[Wiki](https://github.com/SpiceSharp/SpiceSharpParser/wiki/E)|
|F (Current-Controlled Current Source)|[Wiki](https://github.com/SpiceSharp/SpiceSharpParser/wiki/F)|
|G (Voltage-Controlled Current Source)|[Wiki](https://github.com/SpiceSharp/SpiceSharpParser/wiki/G)|
|H (Current-Controlled Voltage Source)|[Wiki](https://github.com/SpiceSharp/SpiceSharpParser/wiki/H)|
|I (Independent Current Source)|[Wiki](https://github.com/SpiceSharp/SpiceSharpParser/wiki/I)|
|J (JFET)|[Wiki](https://github.com/SpiceSharp/SpiceSharpParser/wiki/J)|
|K (Mutual Inductance)|[Wiki](https://github.com/SpiceSharp/SpiceSharpParser/wiki/K)|
|L (Inductor)|[Wiki](https://github.com/SpiceSharp/SpiceSharpParser/wiki/L)|
|M (Mosfet)|[Wiki](https://github.com/SpiceSharp/SpiceSharpParser/wiki/M)|
|Q (Bipolar Junction Transistor)|[Wiki](https://github.com/SpiceSharp/SpiceSharpParser/wiki/Q)|
|R (Resistor)|[Wiki](https://github.com/SpiceSharp/SpiceSharpParser/wiki/R)|
|S (Voltage Switch)|[Wiki](https://github.com/SpiceSharp/SpiceSharpParser/wiki/S)|
|T (Lossless Transmission Line)|[Wiki](https://github.com/SpiceSharp/SpiceSharpParser/wiki/T)|
|V (Independent Voltage Source)|[Wiki](https://github.com/SpiceSharp/SpiceSharpParser/wiki/V)|
|W (Current Switch)|[Wiki](https://github.com/SpiceSharp/SpiceSharpParser/wiki/W)|
|X (Subcircuit)|[Wiki](https://github.com/SpiceSharp/SpiceSharpParser/wiki/X)|

## Documentation
* API documentation is available at <https://spicesharp.github.io/SpiceSharpParser/api/index.html>.
* Wiki is available at <https://github.com/SpiceSharp/SpiceSharpParser/wiki>

## License
SpiceSharpParser is under MIT License


[![FOSSA Status](https://app.fossa.io/api/projects/git%2Bgithub.com%2Fmarcin-golebiowski%2FSpiceSharpParser.svg?type=large)](https://app.fossa.io/projects/git%2Bgithub.com%2Fmarcin-golebiowski%2FSpiceSharpParser?ref=badge_large)
