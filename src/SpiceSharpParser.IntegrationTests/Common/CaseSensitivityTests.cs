using SpiceSharp.Simulations;
using SpiceSharpParser.Common;
using SpiceSharpParser.ModelReaders.Netlist.Spice;
using System;
using System.IO;
using System.Linq;
using System.Text;
using Xunit;

namespace SpiceSharpParser.IntegrationTests.Common
{
    public class CaseSensitivityTests : BaseTests
    {
        [Fact]
        public void DotStatementsException()
        {
            var parser = new SpiceNetlistParser();

            parser.Settings.Lexing.HasTitle = true;
            parser.Settings.Lexing.IsDotStatementNameCaseSensitive = true;
            parser.Settings.Parsing.IsEndRequired = true;

            var text = string.Join(
                Environment.NewLine,
                "CaseSensitivity",
                ".End");
            var result = parser.ParseNetlist(text);

            Assert.NotNull(result);
            Assert.True(result.ValidationResult.HasError);
        }

        [Fact]
        public void FunctionNamePositive()
        {
            var parser = new SpiceNetlistParser();

            parser.Settings.Lexing.HasTitle = true;
            parser.Settings.Parsing.IsEndRequired = true;
            
            var text = string.Join(
                Environment.NewLine,
                "CaseSensitivity",
                "R1 0 1 {fUn(1)}",
                "V1 0 1 10",
                ".FUNC fun(x) = { x* x +1}",
                ".OP",
                ".End");

            var parseResult = parser.ParseNetlist(text);
            var reader = new SpiceSharpReader();
            reader.Settings.CaseSensitivity.IsFunctionNameCaseSensitive = false;

            var spiceModel = reader.Read(parseResult.FinalModel);

            Assert.NotNull(parseResult);
            Assert.False(parseResult.ValidationResult.HasError);
            Assert.False(parseResult.ValidationResult.HasWarning);

            spiceModel.Simulations[0].Run(spiceModel.Circuit);
        }

        [Fact]
        public void FunctionNamePositiveTest2()
        {
            var parser = new SpiceNetlistParser();

            parser.Settings.Lexing.HasTitle = true;
            parser.Settings.Parsing.IsEndRequired = true;

            var text = string.Join(
                Environment.NewLine,
                "CaseSensitivity",
                "R1 0 1 1",
                "V1 0 1 {PWR(V(2),2)}",
                "V2 0 2 10",
                ".OP",
                ".End");

            var parseResult = parser.ParseNetlist(text);
            var reader = new SpiceSharpReader();
            reader.Settings.CaseSensitivity.IsFunctionNameCaseSensitive = false;

            var spiceModel = reader.Read(parseResult.FinalModel);

            Assert.NotNull(parseResult);
            Assert.False(parseResult.ValidationResult.HasError);
            Assert.False(parseResult.ValidationResult.HasWarning);
            spiceModel.Simulations[0].Run(spiceModel.Circuit);
        }

        [Fact]
        public void When_DistributionNameNotSensitive_Expect_NoException()
        {
            var parser = new SpiceNetlistParser();
            var text = string.Join(Environment.NewLine,
               "Dev - Diode circuit",
                "D1 OUT 0 1N914",
                "V1 OUT 0 0",
                ".model 1N914 D(Is=2.52e-9 DEV/unifoRm 10% Rs=0.568 N=1.752 Cjo=4e-12 M=0.4 tt=20e-9)",
                ".DC V1 -1 1 0.1",
                ".SAVE i(V1)",
                ".DISTRIBUTION uniform (-1,1) (1, 1)",
                ".END");

            var parseResult = parser.ParseNetlist(text);
            var reader = new SpiceSharpReader();
            reader.Settings.CaseSensitivity.IsDistributionNameCaseSensitive = false;
            var spiceModel = reader.Read(parseResult.FinalModel);

            var exception = Record.Exception(() => spiceModel.Simulations[0].Run(spiceModel.Circuit));
            Assert.Null(exception);
        }

        [Fact]
        public void When_DistributionNameSensitive_Positive_Expect_NoException()
        {
            var parser = new SpiceNetlistParser();
            var text = string.Join(Environment.NewLine,
                "Dev - Diode circuit",
                "D1 OUT 0 1N914",
                "V1 OUT 0 0",
                ".model 1N914 D(Is=2.52e-9 DEV/unifoRm1 10% Rs=0.568 N=1.752 Cjo=4e-12 M=0.4 tt=20e-9)",
                ".DC V1 -1 1 0.1",
                ".SAVE i(V1)",
                ".DISTRIBUTION unifoRm1 (-1,1) (1, 1)",
                ".END");

            var parseResult = parser.ParseNetlist(text);
            var reader = new SpiceSharpReader();
            reader.Settings.CaseSensitivity.IsDistributionNameCaseSensitive = true;
            var spiceModel = reader.Read(parseResult.FinalModel);

            var codes = spiceModel.Simulations[0].Run(spiceModel.Circuit, -1);
            codes = spiceModel.Simulations[0].AttachEvents(codes);

            codes.ToArray();
        }

        [Fact]
        public void When_DistributionNameSensitive_Negative_Expect_Exception()
        {
            var parser = new SpiceNetlistParser();

            var text = string.Join(Environment.NewLine,
                "Dev - Diode circuit",
                "D1 OUT 0 1N914",
                "V1 OUT 0 0",
                ".model 1N914 D(Is=2.52e-9 DEV/unifoRm1 10% Rs=0.568 N=1.752 Cjo=4e-12 M=0.4 tt=20e-9)",
                ".DC V1 -1 1 0.1",
                ".SAVE i(V1)",
                ".DISTRIBUTION uniform1 (-1,1) (1, 1)",
                ".END");

            var parseResult = parser.ParseNetlist(text);

            var reader = new SpiceSharpReader();
            reader.Settings.CaseSensitivity.IsDistributionNameCaseSensitive = true;
            var spiceModel = reader.Read(parseResult.FinalModel);

            var codes = spiceModel.Simulations[0].Run(spiceModel.Circuit, -1);
            codes = spiceModel.Simulations[0].AttachEvents(codes);


            Assert.Throws<ArgumentException>(() => codes.ToArray());
        }

        [Fact]
        public void BuiltInFunctionNamePositive()
        {
            var parser = new SpiceNetlistParser();

            parser.Settings.Lexing.HasTitle = true;
            parser.Settings.Parsing.IsEndRequired = true;
            
            var text = string.Join(
                Environment.NewLine,
                "CaseSensitivity",
                "R1 0 1 {Cos(1)}",
                "V1 0 1 10",
                ".OP",
                ".End");

            var parseResult = parser.ParseNetlist(text);

            var reader = new SpiceSharpReader();
            reader.Settings.CaseSensitivity.IsDistributionNameCaseSensitive = true;
            var spiceModel = reader.Read(parseResult.FinalModel);

            var exception = Record.Exception(() => spiceModel.Simulations[0].Run(spiceModel.Circuit).ToArray());
            Assert.Null(exception);

        }

        [Fact]
        public void DotStatementsNoException2()
        {
            var parser = new SpiceNetlistParser();

            parser.Settings.Lexing.HasTitle = true;
            parser.Settings.Lexing.IsDotStatementNameCaseSensitive = false;
            parser.Settings.Parsing.IsEndRequired = false;
            parser.Settings.Parsing.IsNewlineRequired = false;

            var text = string.Join(
                Environment.NewLine,
                "CaseSensitivity",
                ".End");

            var exception = Record.Exception(() => parser.ParseNetlist(text));
            Assert.Null(exception);
        }

        [Fact]
        public void DotStatementsNoException3()
        {
            var parser = new SpiceNetlistParser();

            parser.Settings.Lexing.HasTitle = true;
            parser.Settings.Lexing.IsDotStatementNameCaseSensitive = false;
            parser.Settings.Parsing.IsEndRequired = false;
            parser.Settings.Parsing.IsNewlineRequired = false;

            var text = string.Join(
                Environment.NewLine,
                "CaseSensitivity",
                ".END");

            var exception = Record.Exception(() => parser.ParseNetlist(text));
            Assert.Null(exception);
        }

        [Fact]
        public void DotStatementsNoException()
        {
            var parser = new SpiceNetlistParser();

            parser.Settings.Lexing.HasTitle = true;
            parser.Settings.Lexing.IsDotStatementNameCaseSensitive = false;
            parser.Settings.Parsing.IsEndRequired = true;

            var text = string.Join(
                Environment.NewLine,
                "CaseSensitivity",
                ".End");

            var exception = Record.Exception(() => parser.ParseNetlist(text));
            Assert.Null(exception);
        }

        [Fact]
        public void ComponentNamesPositive()
        {
            var parser = new SpiceNetlistParser();

            parser.Settings.Lexing.HasTitle = true;
            parser.Settings.Parsing.IsEndRequired = true;

            var text = string.Join(
                Environment.NewLine,
                "CaseSensitivity",
                "R1 0 OUT 1",
                "V1 0 OUT 10",
                ".SAVE I(r1)",
                ".OP",
                ".END");

            var parseResult = parser.ParseNetlist(text);

            var reader = new SpiceSharpReader();
            reader.Settings.CaseSensitivity.IsEntityNamesCaseSensitive = false;
            var spiceModel = reader.Read(parseResult.FinalModel);

            var export = RunOpSimulation(spiceModel, "I(r1)");

            Assert.Equal(10, export);
        }

        [Fact]
        public void ComponentNamesException()
        {
            var parser = new SpiceNetlistParser();

            parser.Settings.Lexing.HasTitle = true;
            parser.Settings.Parsing.IsEndRequired = true;

            var text = string.Join(
                Environment.NewLine,
                "CaseSensitivity",
                "R1 0 OUT 1",
                "V1 0 OUT 10",
                ".SAVE I(r1)",
                ".OP",
                ".END");

            var parseResult = parser.ParseNetlist(text);

            var reader = new SpiceSharpReader();
            reader.Settings.CaseSensitivity.IsEntityNamesCaseSensitive = true;
            var spiceModel = reader.Read(parseResult.FinalModel);

            Assert.Throws<SpiceSharpParserException>(() => RunOpSimulation(spiceModel, "I(r1)"));
        }

        [Fact]
        public void ModelNamesPositive()
        {
            var parser = new SpiceNetlistParser();

            parser.Settings.Lexing.HasTitle = true;
            parser.Settings.Parsing.IsEndRequired = true;

            var text = string.Join(
                Environment.NewLine,
                "CaseSensitivity - Diode circuit",
                "D1 OUT 0 default",
                "V1 OUT 0 1",
                ".model DEFAULT D",
                ".OP",
                ".SAVE i(V1)",
                ".END");

            var parseResult = parser.ParseNetlist(text);

            var reader = new SpiceSharpReader();
            reader.Settings.CaseSensitivity.IsEntityNamesCaseSensitive = false;
            var spiceModel = reader.Read(parseResult.FinalModel);

            var export = RunOpSimulation(spiceModel, "i(V1)");

            Assert.True(EqualsWithTol(-618.507827392572, export));
        }

        [Fact]
        public void ParamNamesPositive()
        {
            var parser = new SpiceNetlistParser();

            parser.Settings.Lexing.HasTitle = true;
            parser.Settings.Parsing.IsEndRequired = true;

            var text = string.Join(
                Environment.NewLine,
                "CaseSensitivity - Diode circuit",
                "D1 OUT 0 default",
                "V1 OUT 0 {parameter}",
                ".model default D",
                ".OP",
                ".PARAM parameter = 1",
                ".SAVE i(V1)",
                ".END");

            var parseResult = parser.ParseNetlist(text);

            var reader = new SpiceSharpReader();
            reader.Settings.CaseSensitivity.IsParameterNameCaseSensitive = false;
            var spiceModel = reader.Read(parseResult.FinalModel);

            var export = RunOpSimulation(spiceModel, "i(V1)");

            Assert.True(EqualsWithTol(-618.507827392572, export));
        }

        [Fact]
        public void ParamNamesFunctionParamPositive()
        {
            var parser = new SpiceNetlistParser();

            parser.Settings.Lexing.HasTitle = true;
            parser.Settings.Parsing.IsEndRequired = true;

            var text = string.Join(
                Environment.NewLine,
                "CaseSensitivity - Diode circuit",
                "D1 OUT 0 default",
                "V1 OUT 0 {fun(1)}",
                ".model default D",
                ".OP",
                ".PARAM fun(X) = {x}",
                ".SAVE i(V1)",
                ".END");

            var parseResult = parser.ParseNetlist(text);

            var reader = new SpiceSharpReader();
            reader.Settings.CaseSensitivity.IsParameterNameCaseSensitive = false;
            var spiceModel = reader.Read(parseResult.FinalModel);

            var export = RunOpSimulation(spiceModel, "i(V1)");

            Assert.True(EqualsWithTol(-618.507827392572, export));
        }

        [Fact]
        public void ParamNameError()
        {
            var parser = new SpiceNetlistParser();

            parser.Settings.Lexing.HasTitle = true;
            parser.Settings.Parsing.IsEndRequired = true;

            var text = string.Join(
                Environment.NewLine,
                "CaseSensitivity - Diode circuit",
                "D1 OUT 0 default",
                "V1 OUT 0 {PARAMETER}",
                ".model default D",
                ".OP",
                ".PARAM parameter = 1",
                ".SAVE i(V1)",
                ".END");

            var parseResult = parser.ParseNetlist(text);

            var reader = new SpiceSharpReader();
            reader.Settings.CaseSensitivity.IsParameterNameCaseSensitive = true;
            var spiceModel = reader.Read(parseResult.FinalModel);

            Assert.True(spiceModel.ValidationResult.HasError);
        }

        [Fact]
        public void LetNamePositive()
        {
            var parser = new SpiceNetlistParser();

            parser.Settings.Lexing.HasTitle = true;
            parser.Settings.Parsing.IsEndRequired = true;

            var text = string.Join(
                Environment.NewLine,
                "CaseSensitivity - Diode circuit",
                "D1 OUT 0 default",
                "V1 OUT 0 {parameter}",
                ".model default D",
                ".OP",
                ".PARAM parameter = 1",
                ".LET A {i(V1)}",
                ".SAVE A",
                ".END");

            var parseResult = parser.ParseNetlist(text);

            var reader = new SpiceSharpReader();
            reader.Settings.CaseSensitivity.IsExpressionNameCaseSensitive = false;
            var spiceModel = reader.Read(parseResult.FinalModel);

            var export = RunOpSimulation(spiceModel, "A");

            Assert.True(EqualsWithTol(-618.507827392572, export));
        }

        [Fact]
        public void LetNameError()
        {
            var parser = new SpiceNetlistParser();

            parser.Settings.Lexing.HasTitle = true;
            parser.Settings.Parsing.IsEndRequired = true;

            var text = string.Join(
                Environment.NewLine,
                "CaseSensitivity - Diode circuit",
                "D1 OUT 0 default",
                "V1 OUT 0 {PARAMETER}",
                ".model default D",
                ".OP",
                ".PARAM parameter = 1",
                ".LET a {i(V1)}",
                ".SAVE A",
                ".END");

            var parseResult = parser.ParseNetlist(text);

            var reader = new SpiceSharpReader();
            reader.Settings.CaseSensitivity.IsExpressionNameCaseSensitive = true;
            var spiceModel = reader.Read(parseResult.FinalModel);

            Assert.True(spiceModel.ValidationResult.HasError);
        }

        [Fact]
        public void EntityParameterPositive()
        {
            var parser = new SpiceNetlistParser();

            parser.Settings.Lexing.HasTitle = true;
            parser.Settings.Parsing.IsEndRequired = true;

            var text = string.Join(
                Environment.NewLine,
                "CaseSensitivity - Diode circuit",
                "D1 OUT 0 1N914",
                "V1 OUT 0 0",
                ".model 1N914 D(Is=2.52e-9 Rs=0.568 N=1.752 Cjo=4e-12 M=0.4 tt=20e-9)",
                ".DC V1 -1 1 10e-3",
                ".SAVE i(V1)",
                ".END");

            var parseResult = parser.ParseNetlist(text);

            var reader = new SpiceSharpReader();
            reader.Settings.CaseSensitivity.IsExpressionNameCaseSensitive = true;
            var spiceModel = reader.Read(parseResult.FinalModel);

            var export = RunDCSimulation(spiceModel, "i(V1)");

            // Get reference
            double[] references =
            {
               2.520684772022719e-09, 2.520665232097485e-09, 2.520645248083042e-09, 2.520624819979389e-09, 2.520603725741921e-09, 2.520582409459848e-09, 2.520560649088566e-09, 2.520538000538863e-09, 2.520515129944556e-09, 2.520491593216434e-09, 2.520467612399102e-09, 2.520442965447955e-09, 2.520417652362994e-09, 2.520391229055008e-09, 2.520364583702417e-09, 2.520336828126801e-09, 2.520307962328161e-09, 2.520278874484916e-09, 2.520248454374041e-09, 2.520216701995537e-09, 2.520184505527823e-09, 2.520150754747874e-09, 2.520115449655691e-09, 2.520079700474298e-09, 2.520041952891461e-09, 2.520002650996389e-09, 2.519962460922898e-09, 2.519919828358752e-09, 2.519875419437767e-09, 2.519829456204548e-09, 2.519781050480674e-09, 2.519730646355356e-09, 2.519677799739384e-09, 2.519621844498943e-09, 2.519563668812452e-09, 2.519502162456888e-09, 2.519437547476855e-09, 2.519369379783143e-09, 2.519297437331147e-09, 2.519221276031658e-09, 2.519140673840070e-09, 2.519055408711779e-09, 2.518964592468365e-09, 2.518868003065222e-09, 2.518765085390839e-09, 2.518655506378309e-09, 2.518538155804606e-09, 2.518412922647428e-09, 2.518278474639146e-09, 2.518133923601340e-09, 2.517978381355590e-09, 2.517810848701174e-09, 2.517629882348160e-09, 2.517434039006616e-09, 2.517221764364308e-09, 2.516991060019791e-09, 2.516739705527016e-09, 2.516465702484538e-09, 2.516165609200982e-09, 2.515836872163391e-09, 2.515475161501968e-09, 2.515076480413825e-09, 2.514635832895351e-09, 2.514147445786818e-09, 2.513604324683172e-09, 2.512998475978634e-09, 2.512320573799798e-09, 2.511559182849510e-09, 2.510700980451475e-09, 2.509729757349533e-09, 2.508625973618450e-09, 2.507366425597013e-09, 2.505921525841615e-09, 2.504256357838130e-09, 2.502326790221332e-09, 2.500077533884593e-09, 2.497439421933478e-09, 2.494324247148683e-09, 2.490618655759391e-09, 2.486175321170236e-09, 2.480800564974572e-09, 2.474236482363779e-09, 2.466134130241215e-09, 2.456014613905211e-09, 2.443208080293857e-09, 2.426758793916406e-09, 2.405272869765440e-09, 2.377086694149710e-09, 2.341755483969976e-09, 2.297702500486665e-09, 2.242774105321033e-09, 2.174284835509965e-09, 2.088886258411193e-09, 1.982402894618041e-09, 1.849628367134315e-09, 1.684070757845824e-09, 1.477634958835239e-09, 1.220227058285062e-09, 8.992606936875092e-10, 4.990415580774510e-10, -4.208324063460023e-23, -6.222658915921997e-10, -1.398183520351370e-09, -2.365693620165477e-09, -3.572105541915782e-09, -5.076410555804323e-09, -6.952166481388744e-09, -9.291094477115180e-09, -1.220756418174318e-08, -1.584418615752092e-08, -2.037878504834723e-08, -2.603309548487864e-08, -3.308360396747645e-08, -4.187506874586688e-08, -5.283737797290300e-08, -6.650657008444583e-08, -8.355104497148602e-08, -1.048042475026989e-07, -1.313054202034536e-07, -1.643504193848955e-07, -2.055550786805860e-07, -2.569342167357824e-07, -3.210001533471285e-07, -4.008855497006358e-07, -5.004965768495850e-07, -6.247039000539800e-07, -7.795808144028804e-07, -9.727001679671332e-07, -1.213504582209257e-06, -1.513768057126441e-06, -1.888171507258285e-06, -2.355020333966173e-06, -2.937139061076621e-06, -3.662986687191783e-06, -4.568047149322574e-06, -5.696562662471649e-06, -7.103694343424394e-06, -8.858215224893939e-06, -1.104586649613992e-05, -1.377353976839135e-05, -1.717448782878606e-05, -2.141481549700064e-05, -2.670156304629412e-05, -3.329276978536466e-05, -4.150999799246158e-05, -5.175391113643180e-05, -6.452363948283857e-05, -8.044083562419591e-05, -1.002795274224200e-04, -1.250031216609715e-04, -1.558102032684916e-04, -1.941911156088105e-04, -2.419976971548277e-04, -3.015289829039203e-04, -3.756361388815854e-04, -4.678503519079946e-04, -5.825377853404534e-04, -7.250859365310891e-04, -9.021256392514054e-04, -1.121792314356274e-03, -1.394028558000970e-03, -1.730927332259435e-03, -2.147110349238757e-03, -2.660129130047428e-03, -3.290866177790397e-03, -4.063900589218683e-03, -5.007786885759424e-03, -6.155179858715831e-03, -7.542725667060601e-03, -9.210636215301937e-03, -1.120187715360643e-02, -1.356093587232876e-02, -1.633219609264214e-02, -1.955802291413677e-02, -2.327673904312388e-02, -2.752072521737903e-02, -3.231488373640667e-02, -3.767565498752745e-02, -4.361068391378264e-02, -5.011912351695047e-02, -5.719246701131020e-02, -6.481574162201031e-02, -7.296888192813844e-02, -8.162812138207687e-02, -9.076728201979800e-02, -1.003588891563969e-01, -1.103750792457605e-01, -1.207882998568939e-01, -1.315718201008491e-01, -1.427000796200868e-01, -1.541489071231517e-01, -1.658956380366401e-01, -1.779191572005734e-01, -1.901998880621483e-01, -2.027197453741645e-01, -2.154620644191900e-01, -2.284115164341036e-01, -2.415540172223232e-01, -2.548766338536659e-01, -2.683674927728656e-01, -2.820156914701786e-01
            };

            Assert.True(EqualsWithTol(export, references));
        }

        [Fact]
        public void EntityParameterPositive2()
        {
            var parser = new SpiceNetlistParser();

            parser.Settings.Lexing.HasTitle = true;
            parser.Settings.Parsing.IsEndRequired = true;

            var text = string.Join(
                Environment.NewLine,
                "CaseSensitivity - Diode circuit",
                "D1 OUT 0 1N914",
                "V1 OUT 0 0",
                ".model 1N914 D(iS=2.52e-9 RS=0.568 N=1.752 cJO=4e-12 m=0.4 TT=20e-9)",
                ".DC V1 -1 1 10e-3",
                ".SAVE i(V1)",
                ".END");

            var parseResult = parser.ParseNetlist(text);

            var reader = new SpiceSharpReader();
            reader.Settings.CaseSensitivity.IsExpressionNameCaseSensitive = true;
            var spiceModel = reader.Read(parseResult.FinalModel);

            var export = RunDCSimulation(spiceModel, "i(V1)");

            // Get reference
            double[] references =
            {
               2.520684772022719e-09, 2.520665232097485e-09, 2.520645248083042e-09, 2.520624819979389e-09, 2.520603725741921e-09, 2.520582409459848e-09, 2.520560649088566e-09, 2.520538000538863e-09, 2.520515129944556e-09, 2.520491593216434e-09, 2.520467612399102e-09, 2.520442965447955e-09, 2.520417652362994e-09, 2.520391229055008e-09, 2.520364583702417e-09, 2.520336828126801e-09, 2.520307962328161e-09, 2.520278874484916e-09, 2.520248454374041e-09, 2.520216701995537e-09, 2.520184505527823e-09, 2.520150754747874e-09, 2.520115449655691e-09, 2.520079700474298e-09, 2.520041952891461e-09, 2.520002650996389e-09, 2.519962460922898e-09, 2.519919828358752e-09, 2.519875419437767e-09, 2.519829456204548e-09, 2.519781050480674e-09, 2.519730646355356e-09, 2.519677799739384e-09, 2.519621844498943e-09, 2.519563668812452e-09, 2.519502162456888e-09, 2.519437547476855e-09, 2.519369379783143e-09, 2.519297437331147e-09, 2.519221276031658e-09, 2.519140673840070e-09, 2.519055408711779e-09, 2.518964592468365e-09, 2.518868003065222e-09, 2.518765085390839e-09, 2.518655506378309e-09, 2.518538155804606e-09, 2.518412922647428e-09, 2.518278474639146e-09, 2.518133923601340e-09, 2.517978381355590e-09, 2.517810848701174e-09, 2.517629882348160e-09, 2.517434039006616e-09, 2.517221764364308e-09, 2.516991060019791e-09, 2.516739705527016e-09, 2.516465702484538e-09, 2.516165609200982e-09, 2.515836872163391e-09, 2.515475161501968e-09, 2.515076480413825e-09, 2.514635832895351e-09, 2.514147445786818e-09, 2.513604324683172e-09, 2.512998475978634e-09, 2.512320573799798e-09, 2.511559182849510e-09, 2.510700980451475e-09, 2.509729757349533e-09, 2.508625973618450e-09, 2.507366425597013e-09, 2.505921525841615e-09, 2.504256357838130e-09, 2.502326790221332e-09, 2.500077533884593e-09, 2.497439421933478e-09, 2.494324247148683e-09, 2.490618655759391e-09, 2.486175321170236e-09, 2.480800564974572e-09, 2.474236482363779e-09, 2.466134130241215e-09, 2.456014613905211e-09, 2.443208080293857e-09, 2.426758793916406e-09, 2.405272869765440e-09, 2.377086694149710e-09, 2.341755483969976e-09, 2.297702500486665e-09, 2.242774105321033e-09, 2.174284835509965e-09, 2.088886258411193e-09, 1.982402894618041e-09, 1.849628367134315e-09, 1.684070757845824e-09, 1.477634958835239e-09, 1.220227058285062e-09, 8.992606936875092e-10, 4.990415580774510e-10, -4.208324063460023e-23, -6.222658915921997e-10, -1.398183520351370e-09, -2.365693620165477e-09, -3.572105541915782e-09, -5.076410555804323e-09, -6.952166481388744e-09, -9.291094477115180e-09, -1.220756418174318e-08, -1.584418615752092e-08, -2.037878504834723e-08, -2.603309548487864e-08, -3.308360396747645e-08, -4.187506874586688e-08, -5.283737797290300e-08, -6.650657008444583e-08, -8.355104497148602e-08, -1.048042475026989e-07, -1.313054202034536e-07, -1.643504193848955e-07, -2.055550786805860e-07, -2.569342167357824e-07, -3.210001533471285e-07, -4.008855497006358e-07, -5.004965768495850e-07, -6.247039000539800e-07, -7.795808144028804e-07, -9.727001679671332e-07, -1.213504582209257e-06, -1.513768057126441e-06, -1.888171507258285e-06, -2.355020333966173e-06, -2.937139061076621e-06, -3.662986687191783e-06, -4.568047149322574e-06, -5.696562662471649e-06, -7.103694343424394e-06, -8.858215224893939e-06, -1.104586649613992e-05, -1.377353976839135e-05, -1.717448782878606e-05, -2.141481549700064e-05, -2.670156304629412e-05, -3.329276978536466e-05, -4.150999799246158e-05, -5.175391113643180e-05, -6.452363948283857e-05, -8.044083562419591e-05, -1.002795274224200e-04, -1.250031216609715e-04, -1.558102032684916e-04, -1.941911156088105e-04, -2.419976971548277e-04, -3.015289829039203e-04, -3.756361388815854e-04, -4.678503519079946e-04, -5.825377853404534e-04, -7.250859365310891e-04, -9.021256392514054e-04, -1.121792314356274e-03, -1.394028558000970e-03, -1.730927332259435e-03, -2.147110349238757e-03, -2.660129130047428e-03, -3.290866177790397e-03, -4.063900589218683e-03, -5.007786885759424e-03, -6.155179858715831e-03, -7.542725667060601e-03, -9.210636215301937e-03, -1.120187715360643e-02, -1.356093587232876e-02, -1.633219609264214e-02, -1.955802291413677e-02, -2.327673904312388e-02, -2.752072521737903e-02, -3.231488373640667e-02, -3.767565498752745e-02, -4.361068391378264e-02, -5.011912351695047e-02, -5.719246701131020e-02, -6.481574162201031e-02, -7.296888192813844e-02, -8.162812138207687e-02, -9.076728201979800e-02, -1.003588891563969e-01, -1.103750792457605e-01, -1.207882998568939e-01, -1.315718201008491e-01, -1.427000796200868e-01, -1.541489071231517e-01, -1.658956380366401e-01, -1.779191572005734e-01, -1.901998880621483e-01, -2.027197453741645e-01, -2.154620644191900e-01, -2.284115164341036e-01, -2.415540172223232e-01, -2.548766338536659e-01, -2.683674927728656e-01, -2.820156914701786e-01
            };

            Assert.True(EqualsWithTol(export, references));
        }

        [Fact]
        public void EntityParameterPositive3()
        {
            var parser = new SpiceNetlistParser();

            parser.Settings.Lexing.HasTitle = true;
            parser.Settings.Parsing.IsEndRequired = true;

            var text = string.Join(
                Environment.NewLine,
                "CaseSensitivity - Diode circuit",
                "D1 OUT 0 1N914",
                "V1 OUT 0 0",
                ".model 1N914 D(iS=2.52e-9 RS=0.568 N=1.752 cJO=4e-12 m=0.4 TT=20e-9)",
                ".DC V1 -1 1 10e-3",
                ".SAVE @V1[I]",
                ".END");

            var parseResult = parser.ParseNetlist(text);

            var reader = new SpiceSharpReader();
            reader.Settings.CaseSensitivity.IsExpressionNameCaseSensitive = true;
            var spiceModel = reader.Read(parseResult.FinalModel);

            var export = RunDCSimulation(spiceModel, "@V1[I]");

            // Get reference
            double[] references =
            {
               2.520684772022719e-09, 2.520665232097485e-09, 2.520645248083042e-09, 2.520624819979389e-09, 2.520603725741921e-09, 2.520582409459848e-09, 2.520560649088566e-09, 2.520538000538863e-09, 2.520515129944556e-09, 2.520491593216434e-09, 2.520467612399102e-09, 2.520442965447955e-09, 2.520417652362994e-09, 2.520391229055008e-09, 2.520364583702417e-09, 2.520336828126801e-09, 2.520307962328161e-09, 2.520278874484916e-09, 2.520248454374041e-09, 2.520216701995537e-09, 2.520184505527823e-09, 2.520150754747874e-09, 2.520115449655691e-09, 2.520079700474298e-09, 2.520041952891461e-09, 2.520002650996389e-09, 2.519962460922898e-09, 2.519919828358752e-09, 2.519875419437767e-09, 2.519829456204548e-09, 2.519781050480674e-09, 2.519730646355356e-09, 2.519677799739384e-09, 2.519621844498943e-09, 2.519563668812452e-09, 2.519502162456888e-09, 2.519437547476855e-09, 2.519369379783143e-09, 2.519297437331147e-09, 2.519221276031658e-09, 2.519140673840070e-09, 2.519055408711779e-09, 2.518964592468365e-09, 2.518868003065222e-09, 2.518765085390839e-09, 2.518655506378309e-09, 2.518538155804606e-09, 2.518412922647428e-09, 2.518278474639146e-09, 2.518133923601340e-09, 2.517978381355590e-09, 2.517810848701174e-09, 2.517629882348160e-09, 2.517434039006616e-09, 2.517221764364308e-09, 2.516991060019791e-09, 2.516739705527016e-09, 2.516465702484538e-09, 2.516165609200982e-09, 2.515836872163391e-09, 2.515475161501968e-09, 2.515076480413825e-09, 2.514635832895351e-09, 2.514147445786818e-09, 2.513604324683172e-09, 2.512998475978634e-09, 2.512320573799798e-09, 2.511559182849510e-09, 2.510700980451475e-09, 2.509729757349533e-09, 2.508625973618450e-09, 2.507366425597013e-09, 2.505921525841615e-09, 2.504256357838130e-09, 2.502326790221332e-09, 2.500077533884593e-09, 2.497439421933478e-09, 2.494324247148683e-09, 2.490618655759391e-09, 2.486175321170236e-09, 2.480800564974572e-09, 2.474236482363779e-09, 2.466134130241215e-09, 2.456014613905211e-09, 2.443208080293857e-09, 2.426758793916406e-09, 2.405272869765440e-09, 2.377086694149710e-09, 2.341755483969976e-09, 2.297702500486665e-09, 2.242774105321033e-09, 2.174284835509965e-09, 2.088886258411193e-09, 1.982402894618041e-09, 1.849628367134315e-09, 1.684070757845824e-09, 1.477634958835239e-09, 1.220227058285062e-09, 8.992606936875092e-10, 4.990415580774510e-10, -4.208324063460023e-23, -6.222658915921997e-10, -1.398183520351370e-09, -2.365693620165477e-09, -3.572105541915782e-09, -5.076410555804323e-09, -6.952166481388744e-09, -9.291094477115180e-09, -1.220756418174318e-08, -1.584418615752092e-08, -2.037878504834723e-08, -2.603309548487864e-08, -3.308360396747645e-08, -4.187506874586688e-08, -5.283737797290300e-08, -6.650657008444583e-08, -8.355104497148602e-08, -1.048042475026989e-07, -1.313054202034536e-07, -1.643504193848955e-07, -2.055550786805860e-07, -2.569342167357824e-07, -3.210001533471285e-07, -4.008855497006358e-07, -5.004965768495850e-07, -6.247039000539800e-07, -7.795808144028804e-07, -9.727001679671332e-07, -1.213504582209257e-06, -1.513768057126441e-06, -1.888171507258285e-06, -2.355020333966173e-06, -2.937139061076621e-06, -3.662986687191783e-06, -4.568047149322574e-06, -5.696562662471649e-06, -7.103694343424394e-06, -8.858215224893939e-06, -1.104586649613992e-05, -1.377353976839135e-05, -1.717448782878606e-05, -2.141481549700064e-05, -2.670156304629412e-05, -3.329276978536466e-05, -4.150999799246158e-05, -5.175391113643180e-05, -6.452363948283857e-05, -8.044083562419591e-05, -1.002795274224200e-04, -1.250031216609715e-04, -1.558102032684916e-04, -1.941911156088105e-04, -2.419976971548277e-04, -3.015289829039203e-04, -3.756361388815854e-04, -4.678503519079946e-04, -5.825377853404534e-04, -7.250859365310891e-04, -9.021256392514054e-04, -1.121792314356274e-03, -1.394028558000970e-03, -1.730927332259435e-03, -2.147110349238757e-03, -2.660129130047428e-03, -3.290866177790397e-03, -4.063900589218683e-03, -5.007786885759424e-03, -6.155179858715831e-03, -7.542725667060601e-03, -9.210636215301937e-03, -1.120187715360643e-02, -1.356093587232876e-02, -1.633219609264214e-02, -1.955802291413677e-02, -2.327673904312388e-02, -2.752072521737903e-02, -3.231488373640667e-02, -3.767565498752745e-02, -4.361068391378264e-02, -5.011912351695047e-02, -5.719246701131020e-02, -6.481574162201031e-02, -7.296888192813844e-02, -8.162812138207687e-02, -9.076728201979800e-02, -1.003588891563969e-01, -1.103750792457605e-01, -1.207882998568939e-01, -1.315718201008491e-01, -1.427000796200868e-01, -1.541489071231517e-01, -1.658956380366401e-01, -1.779191572005734e-01, -1.901998880621483e-01, -2.027197453741645e-01, -2.154620644191900e-01, -2.284115164341036e-01, -2.415540172223232e-01, -2.548766338536659e-01, -2.683674927728656e-01, -2.820156914701786e-01
            };

            Assert.True(EqualsWithTol(export, references));
        }

        [Fact]
        public void SubcircuitPositive()
        {
            var text = string.Join(Environment.NewLine, "Subcircuit - Case",
                "V1 IN 0 4.0",
                "X1 IN OUT resistor",
                "RX OUT 0 1",
                ".SUBCKT RESISTOR input output params: R=1",
                "R1 input output {R}",
                ".ENDS RESISTOR",
                ".OP",
                ".SAVE V(OUT)",
                ".END");
            var parser = new SpiceNetlistParser();
            var netlist = parser.ParseNetlist(text);

            var spiceSharpSettings = new SpiceNetlistReaderSettings(new SpiceNetlistCaseSensitivitySettings() { IsSubcircuitNameCaseSensitive = false }, () => parser.Settings.WorkingDirectory, Encoding.Default);
            var spiceSharpReader = new SpiceNetlistReader(spiceSharpSettings);

            var model = spiceSharpReader.Read(netlist.FinalModel);
            Assert.False(model.ValidationResult.HasError);
        }

        [Fact]
        public void SubcircuitPositive2()
        {
            var text = string.Join(Environment.NewLine, "Subcircuit - Case",
                "V1 IN 0 4.0",
                "X1 IN OUT resistor",
                "RX OUT 0 1",
                ".SUBCKT RESISTOR input output params: R=1",
                "R1 input output {R}",
                ".ENDS RESISTOR",
                ".OP",
                ".SAVE V(OUT)",
                ".END");

            var parser = new SpiceNetlistParser();
            var netlist = parser.ParseNetlist(text);

            var spiceSharpSettings = new SpiceNetlistReaderSettings(new SpiceNetlistCaseSensitivitySettings() { IsSubcircuitNameCaseSensitive = true }, () => parser.Settings.WorkingDirectory, Encoding.Default);
            var spiceSharpReader = new SpiceNetlistReader(spiceSharpSettings);

            var model = spiceSharpReader.Read(netlist.FinalModel);
            Assert.True(model.ValidationResult.HasError);
        }
    }
}