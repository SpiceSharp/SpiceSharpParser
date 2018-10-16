# <img src="https://spicesharp.github.io/SpiceSharp/api/images/logo_full.svg" width="45px" /> Spice#/SpiceSharpParser
 [<img src="https://img.shields.io/nuget/vpre/SpiceSharp-Parser.svg">]( https://www.nuget.org/packages/SpiceSharp-Parser)
[![Windows](https://ci.appveyor.com/api/projects/status/d8tpj2hm3hcullmw/branch/master?svg=true)](https://ci.appveyor.com/project/marcin-golebiowski/spicesharpparser/branch/master)
[![Linux](https://travis-ci.org/SpiceSharp/SpiceSharpParser.svg?branch=master)](https://travis-ci.org/SpiceSharp/SpiceSharpParser?branch=master)
[![codecov](https://codecov.io/gh/SpiceSharp/SpiceSharpParser/branch/master/graph/badge.svg)](https://codecov.io/gh/SpiceSharp/SpiceSharpParser)

SpiceSharpParser is a .NET library that allows to simulate circuits defined by Spice netlists.

## Installation

SpiceSharpParser is available as [NuGet Package](https://www.nuget.org/packages/SpiceSharp-Parser) and can be installed:

```
Install-Package SpiceSharp-Parser
```
or 

```
dotnet add package SpiceSharp-Parser
```

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
|.AC          |[Wiki](https://github.com/SpiceSharp/SpiceSharpParser/wiki/.AC)||
|.APPENDMODEL |[Wiki](https://github.com/SpiceSharp/SpiceSharpParser/wiki/.APPENDMODEL)||
|.DC          |[Wiki](https://github.com/SpiceSharp/SpiceSharpParser/wiki/.DC)||
|.ELSE        |[Wiki](https://github.com/SpiceSharp/SpiceSharpParser/wiki/.ELSE)||
|.ENDIF       |[Wiki](https://github.com/SpiceSharp/SpiceSharpParser/wiki/.ENDIF)||
|.GLOBAL      |[Wiki](https://github.com/SpiceSharp/SpiceSharpParser/wiki/.GLOBAL)||           
|.IC          |[Wiki](https://github.com/SpiceSharp/SpiceSharpParser/wiki/.IC)||
|.IF          |[Wiki](https://github.com/SpiceSharp/SpiceSharpParser/wiki/.IF)||
|.INCLUDE     |[Wiki](https://github.com/SpiceSharp/SpiceSharpParser/wiki/.INCLUDE)||
|.LET         |[Wiki](https://github.com/SpiceSharp/SpiceSharpParser/wiki/.LET)||
|.LIB         |[Wiki](https://github.com/SpiceSharp/SpiceSharpParser/wiki/.LIB)||
|.MC          |[Wiki](https://github.com/SpiceSharp/SpiceSharpParser/wiki/.MC)||
|.NODESET     |[Wiki](https://github.com/SpiceSharp/SpiceSharpParser/wiki/.NODESET)||
|.NOISE       |[Wiki](https://github.com/SpiceSharp/SpiceSharpParser/wiki/.NOISE)||
|.OP          |[Wiki](https://github.com/SpiceSharp/SpiceSharpParser/wiki/.OP)||
|.OPTIONS     |[Wiki](https://github.com/SpiceSharp/SpiceSharpParser/wiki/.OPTIONS)||
|.PARAM       |[Wiki](https://github.com/SpiceSharp/SpiceSharpParser/wiki/.PARAM)||
|.PLOT        |[Wiki](https://github.com/SpiceSharp/SpiceSharpParser/wiki/.PLOT)||
|.PRINT       |[Wiki](https://github.com/SpiceSharp/SpiceSharpParser/wiki/.PRINT)||
|.TRAN        |[Wiki](https://github.com/SpiceSharp/SpiceSharpParser/wiki/.TRAN)||
|.SAVE        |[Wiki](https://github.com/SpiceSharp/SpiceSharpParser/wiki/.SAVE)||
|.ST          |[Wiki](https://github.com/SpiceSharp/SpiceSharpParser/wiki/.ST)||
|.STEP        |[Wiki](https://github.com/SpiceSharp/SpiceSharpParser/wiki/.STEP)||
|.SUBCKT      |[Wiki](https://github.com/SpiceSharp/SpiceSharpParser/wiki/.SUBCKT)||
|.TEMP        |[Wiki](https://github.com/SpiceSharp/SpiceSharpParser/wiki/.TEMP)||

### Device statements
| Device Statement  |  Documentation | Status  |
|:------------|-------|----------------:|
|C (Capacitor)|[Wiki](https://github.com/SpiceSharp/SpiceSharpParser/wiki/C)||
|D (Diode)|[Wiki](https://github.com/SpiceSharp/SpiceSharpParser/wiki/D)||
|E (Voltage-Controlled Voltage Source)|[Wiki](https://github.com/SpiceSharp/SpiceSharpParser/wiki/E)||
|F (Current-Controlled Current Source)|[Wiki](https://github.com/SpiceSharp/SpiceSharpParser/wiki/F)||
|G (Voltage-Controlled Current Source)|[Wiki](https://github.com/SpiceSharp/SpiceSharpParser/wiki/G)||
|H (Current-Controlled Voltage Source)|[Wiki](https://github.com/SpiceSharp/SpiceSharpParser/wiki/H)||
|I (Independent Current Source)|[Wiki](https://github.com/SpiceSharp/SpiceSharpParser/wiki/I)||
|K (Bipolar Junction Transistor)|[Wiki](https://github.com/SpiceSharp/SpiceSharpParser/wiki/K)||
|L (Inductor)|[Wiki](https://github.com/SpiceSharp/SpiceSharpParser/wiki/L)||
|M (Mosfet)|[Wiki](https://github.com/SpiceSharp/SpiceSharpParser/wiki/M)||
|Q (Bipolar Junction Transistor)|[Wiki](https://github.com/SpiceSharp/SpiceSharpParser/wiki/Q)||
|R (Resistor)|[Wiki](https://github.com/SpiceSharp/SpiceSharpParser/wiki/R)||
|S (Voltage Switch)|[Wiki](https://github.com/SpiceSharp/SpiceSharpParser/wiki/S)||
|V (Independent Voltage Source)|[Wiki](https://github.com/SpiceSharp/SpiceSharpParser/wiki/V)||
|W (Current Switch)|[Wiki](https://github.com/SpiceSharp/SpiceSharpParser/wiki/W)||
|X (Subcircuit)|[Wiki](https://github.com/SpiceSharp/SpiceSharpParser/wiki/X)||

### Expression function
|  Function name  |  Documentation | Status  |
|:------------|--------------- |--------:|
|@      |[Wiki](https://github.com/SpiceSharp/SpiceSharpParser/wiki/@)||
|cos      |[Wiki](https://github.com/SpiceSharp/SpiceSharpParser/wiki/cos)||           
|sin         |[Wiki](https://github.com/SpiceSharp/SpiceSharpParser/wiki/sin)||
|tan     |[Wiki](https://github.com/SpiceSharp/SpiceSharpParser/wiki/tan)||
|cosh       |[Wiki](https://github.com/SpiceSharp/SpiceSharpParser/wiki/cosh)||
|sinh     |[Wiki](https://github.com/SpiceSharp/SpiceSharpParser/wiki/sinh)||
|tanh        |[Wiki](https://github.com/SpiceSharp/SpiceSharpParser/wiki/tanh)||
|acos        |[Wiki](https://github.com/SpiceSharp/SpiceSharpParser/wiki/acos)||
|asin         |[Wiki](https://github.com/SpiceSharp/SpiceSharpParser/wiki/asin)||
|atan        |[Wiki](https://github.com/SpiceSharp/SpiceSharpParser/wiki/atan)||
|atan2          |[Wiki](https://github.com/SpiceSharp/SpiceSharpParser/wiki/atan2)||
|def          |[Wiki](https://github.com/SpiceSharp/SpiceSharpParser/wiki/def)||
|lazy       |[Wiki](https://github.com/SpiceSharp/SpiceSharpParser/wiki/lazy)||
|if          |[Wiki](https://github.com/SpiceSharp/SpiceSharpParser/wiki/if)||
|gauss     |[Wiki](https://github.com/SpiceSharp/SpiceSharpParser/wiki/gauss)||
|random |[Wiki](https://github.com/SpiceSharp/SpiceSharpParser/wiki/random)||
|flat        |[Wiki](https://github.com/SpiceSharp/SpiceSharpParser/wiki/flat)||
|table         |[Wiki](https://github.com/SpiceSharp/SpiceSharpParser/wiki/table)||
|pow          |[Wiki](https://github.com/SpiceSharp/SpiceSharpParser/wiki/pow)||
|pwr        |[Wiki](https://github.com/SpiceSharp/SpiceSharpParser/wiki/pwr)||
|pwrs       |[Wiki](https://github.com/SpiceSharp/SpiceSharpParser/wiki/pwrs)||
|sqrt          |[Wiki](https://github.com/SpiceSharp/SpiceSharpParser/wiki/sqrt)||
|**        |[Wiki](https://github.com/SpiceSharp/SpiceSharpParser/wiki/**)||
|min       |[Wiki](https://github.com/SpiceSharp/SpiceSharpParser/wiki/min)||
|max          |[Wiki](https://github.com/SpiceSharp/SpiceSharpParser/wiki/max)||
|limit          |[Wiki](https://github.com/SpiceSharp/SpiceSharpParser/wiki/limit)||
|ln          |[Wiki](https://github.com/SpiceSharp/SpiceSharpParser/wiki/ln)||
|log          |[Wiki](https://github.com/SpiceSharp/SpiceSharpParser/wiki/log)||
|log10          |[Wiki](https://github.com/SpiceSharp/SpiceSharpParser/wiki/log10)||
|cbrt          |[Wiki](https://github.com/SpiceSharp/SpiceSharpParser/wiki/cbrt)||
|buf          |[Wiki](https://github.com/SpiceSharp/SpiceSharpParser/wiki/buf)||
|ceil          |[Wiki](https://github.com/SpiceSharp/SpiceSharpParser/wiki/ceil)||
|abs          |[Wiki](https://github.com/SpiceSharp/SpiceSharpParser/wiki/abs)||
|floor          |[Wiki](https://github.com/SpiceSharp/SpiceSharpParser/wiki/floor)||
|hypot          |[Wiki](https://github.com/SpiceSharp/SpiceSharpParser/wiki/hypot)||
|int          |[Wiki](https://github.com/SpiceSharp/SpiceSharpParser/wiki/int)||
|inv          |[Wiki](https://github.com/SpiceSharp/SpiceSharpParser/wiki/inv)||
|exp          |[Wiki](https://github.com/SpiceSharp/SpiceSharpParser/wiki/exp)||
|db          |[Wiki](https://github.com/SpiceSharp/SpiceSharpParser/wiki/db)||
|round          |[Wiki](https://github.com/SpiceSharp/SpiceSharpParser/wiki/round)||
|u          |[Wiki](https://github.com/SpiceSharp/SpiceSharpParser/wiki/u)||
|sgn          |[Wiki](https://github.com/SpiceSharp/SpiceSharpParser/wiki/sgn)||
|uramp          |[Wiki](https://github.com/SpiceSharp/SpiceSharpParser/wiki/uramp)||


## Documentation
* API documentation is available at <https://spicesharp.github.io/SpiceSharpParser/api/index.html>.
* Wiki is available at <https://github.com/SpiceSharp/SpiceSharpParser/wiki>

## License
SpiceSharpParser is under MIT License
