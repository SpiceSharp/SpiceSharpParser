# <img src="https://spicesharp.github.io/SpiceSharp/api/images/logo_full.svg" width="45px" /> Spice#/SpiceSharpParser
SpiceSharpParser is a .NET library that allows to simulate circuits defined by Spice netlists.

## Installation

SpiceSharpParser is available as NuGet Package 
[<img src="https://img.shields.io/nuget/vpre/SpiceSharp-Parser.svg">]( https://www.nuget.org/packages/SpiceSharp-Parser)

## Quickstart

Parsing a netlist and executing a simulation is relatively straightforward. For example:

```csharp
var netlist = string.Join(Environment.NewLine,
                "Diode circuit",
                "D1 OUT 0 1N914",
                "V1 OUT 0 0",
                ".model 1N914 D(Is=2.52e-9 Rs=0.568 N=1.752 Cjo=4e-12 M=0.4 tt=20e-9)",
                ".DC V1 -1 1 10e-3",
                ".SAVE i(V1)",
                ".END");

var parser = new SpiceParser();
var parseResult = parser.ParseNetlist(netlist);
var spiceSharpModel = parseResult.SpiceSharpModel;
var simulation = spiceSharpModel.Simulations.Single();
simulation.Run(spiceSharpModel.Circuit);            
```
## Features
### Dot statements
|  Statement  |  Documentation | Status  |
|:------------|--------------- |--------:|
|.GLOBAL      |[Wiki](https://github.com/SpiceSharp/SpiceSharpParser/wiki/.GLOBAL)||           
|.LET         |[Wiki](https://github.com/SpiceSharp/SpiceSharpParser/wiki/.LET)||
|.NODESET     |[Wiki](https://github.com/SpiceSharp/SpiceSharpParser/wiki/.NODESET)||
|.PARAM       |[Wiki](https://github.com/SpiceSharp/SpiceSharpParser/wiki/.PARAM)||
|.OPTIONS     |[Wiki](https://github.com/SpiceSharp/SpiceSharpParser/wiki/.OPTIONS)||
|.SAVE        |[Wiki](https://github.com/SpiceSharp/SpiceSharpParser/wiki/.SAVE)||
|.PLOT        |[Wiki](https://github.com/SpiceSharp/SpiceSharpParser/wiki/.PLOT)||
|.IC          |[Wiki](https://github.com/SpiceSharp/SpiceSharpParser/wiki/.IC)||
|.TRAN        |[Wiki](https://github.com/SpiceSharp/SpiceSharpParser/wiki/.TRAN)||
|.AC          |[Wiki](https://github.com/SpiceSharp/SpiceSharpParser/wiki/.AC)||
|.OP          |[Wiki](https://github.com/SpiceSharp/SpiceSharpParser/wiki/.OP)||
|.NOISE       |[Wiki](https://github.com/SpiceSharp/SpiceSharpParser/wiki/.NOISE)||
|.DC          |[Wiki](https://github.com/SpiceSharp/SpiceSharpParser/wiki/.DC)||
|.SUBCKT      |[Wiki](https://github.com/SpiceSharp/SpiceSharpParser/wiki/.SUBCKT)||
|.INCLUDE     |[Wiki](https://github.com/SpiceSharp/SpiceSharpParser/wiki/.INCLUDE)||
|.APPENDMODEL |[Wiki](https://github.com/SpiceSharp/SpiceSharpParser/wiki/.APPENDMODEL)||
|.TEMP        |[Wiki](https://github.com/SpiceSharp/SpiceSharpParser/wiki/.TEMP)||
|.LIB         |[Wiki](https://github.com/SpiceSharp/SpiceSharpParser/wiki/.LIB)||
|.IF          |[Wiki](https://github.com/SpiceSharp/SpiceSharpParser/wiki/.IF)||
|.ELSE        |[Wiki](https://github.com/SpiceSharp/SpiceSharpParser/wiki/.ELSE)||
|.ENDIF       |[Wiki](https://github.com/SpiceSharp/SpiceSharpParser/wiki/.ENDIF)||
|.ST          |[Wiki](https://github.com/SpiceSharp/SpiceSharpParser/wiki/.ST)||
|.STEP        |[Wiki](https://github.com/SpiceSharp/SpiceSharpParser/wiki/.STEP)||
|.PRINT       |[Wiki](https://github.com/SpiceSharp/SpiceSharpParser/wiki/.PRINT)||
|.MC          |[Wiki](https://github.com/SpiceSharp/SpiceSharpParser/wiki/.MC)||

### Device statements
| Device Statement  |  Documentation | Status  |
|:------------|-------|----------------:|
|C (Capacitor)|[Wiki](https://github.com/SpiceSharp/SpiceSharpParser/wiki/C)||
|D (Diode)|[Wiki](https://github.com/SpiceSharp/SpiceSharpParser/wiki/D)||
|E (Voltage-Controlled Current Source)|[Wiki](https://github.com/SpiceSharp/SpiceSharpParser/wiki/E)||
|F (Voltage-Controlled Current Source)|[Wiki](https://github.com/SpiceSharp/SpiceSharpParser/wiki/F)||
|G (Voltage-Controlled Current Source)|[Wiki](https://github.com/SpiceSharp/SpiceSharpParser/wiki/G)||
|H (Current-Controlled Voltage Source)|[Wiki](https://github.com/SpiceSharp/SpiceSharpParser/wiki/H)||
|I (Independent Current Source)|[Wiki](https://github.com/SpiceSharp/SpiceSharpParser/wiki/I)||
|K (Bipolar Junction Transistor)|[Wiki](https://github.com/SpiceSharp/SpiceSharpParser/wiki/K)||
|L (Inductor)|[Wiki](https://github.com/SpiceSharp/SpiceSharpParser/wiki/L)||
|M (Mosfet)|[Wiki](https://github.com/SpiceSharp/SpiceSharpParser/wiki/M)||
|Q (Bipolar Junction Transistor)|[Wiki](https://github.com/SpiceSharp/SpiceSharpParser/wiki/Q)||
|R (Resistor)|[Wiki](https://github.com/SpiceSharp/SpiceSharpParser/wiki/R)||
|S (Independent Voltage Source)|[Wiki](https://github.com/SpiceSharp/SpiceSharpParser/wiki/S)||
|V (Independent Voltage Source)|[Wiki](https://github.com/SpiceSharp/SpiceSharpParser/wiki/V)||
|X (Subcircuit)|[Wiki](https://github.com/SpiceSharp/SpiceSharpParser/wiki/X)||

## Build status

|    | Status |
|:---|----------------:|
|**Windows**|[![Build status](https://ci.appveyor.com/api/projects/status/d8tpj2hm3hcullmw/branch/master?svg=true)](https://ci.appveyor.com/project/marcin-golebiowski/spicesharpparser/branch/master)|
|**Linux**|[![Build status](https://travis-ci.org/SpiceSharp/SpiceSharpParser.svg?branch=master)](https://travis-ci.org/SpiceSharp/SpiceSharpParser?branch=master)|

## Documentation
### API documentation is available at <https://spicesharp.github.io/SpiceSharpParser/api/index.html>.
### Wiki is available at <https://github.com/SpiceSharp/SpiceSharpParser/wiki>

## License
SpiceSharpParser is under MIT License
