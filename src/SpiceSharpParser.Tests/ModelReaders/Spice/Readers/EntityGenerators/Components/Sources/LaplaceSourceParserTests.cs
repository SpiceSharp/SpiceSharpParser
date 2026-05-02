using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NSubstitute;
using SpiceSharp;
using SpiceSharp.Components;
using SpiceSharpBehavioral.Parsers.Nodes;
using SpiceSharpParser.Common;
using SpiceSharpParser.Common.Evaluation;
using SpiceSharpParser.Common.Mathematics.Probability;
using SpiceSharpParser.Common.Validation;
using SpiceSharpParser.Lexers.Expressions;
using SpiceSharpParser.ModelReaders.Netlist.Spice;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Context;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Context.Names;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Evaluation;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.EntityGenerators.Components.Sources;
using SpiceSharpParser.Models.Netlist.Spice.Objects;
using SpiceSharpParser.Models.Netlist.Spice.Objects.Parameters;
using Xunit;
using Parser = SpiceSharpParser.Parsers.Expression.Parser;

namespace SpiceSharpParser.Tests.ModelReaders.Spice.Readers.EntityGenerators.Components.Sources
{
    public class LaplaceSourceParserTests
    {
        public static IEnumerable<object[]> InvalidInputExpressions
        {
            get
            {
                yield return new object[] { "V(a)-V(b)" };
                yield return new object[] { "I(Vsense)" };
                yield return new object[] { "V(a,b,c)" };
                yield return new object[] { "V(a+1)" };
            }
        }

        public static IEnumerable<object[]> RejectedTransfers
        {
            get
            {
                yield return new object[] { "1/s" };
                yield return new object[] { "s" };
                yield return new object[] { "sin(s)" };
            }
        }

        public static IEnumerable<object[]> SupportedMultipliers
        {
            get
            {
                yield return new object[] { "2", 2.0 };
                yield return new object[] { "-2", -2.0 };
                yield return new object[] { "0", 0.0 };
            }
        }

        public static IEnumerable<object[]> SupportedDelays
        {
            get
            {
                yield return new object[] { "TD", "1n", 1e-9 };
                yield return new object[] { "DELAY", "1n", 1e-9 };
            }
        }

        public static IEnumerable<object[]> InvalidOptions
        {
            get
            {
                yield return new object[]
                {
                    new Parameter[] { Assignment("M", "2"), Assignment("M", "3") },
                    "only once",
                };
                yield return new object[]
                {
                    new Parameter[] { Assignment("TD", "1n"), Assignment("TD", "2n") },
                    "only once",
                };
                yield return new object[]
                {
                    new Parameter[] { Assignment("TD", "1n"), Assignment("DELAY", "2n") },
                    "only once",
                };
                yield return new object[]
                {
                    new Parameter[] { Assignment("TD", "-1n") },
                    "non-negative",
                };
                yield return new object[]
                {
                    new Parameter[] { new WordParameter("TD") },
                    "assignment syntax",
                };
            }
        }

        [Fact]
        public void When_SupportedSyntaxVariantsAreParsed_Expect_IdenticalDefinitions()
        {
            var canonical = ParseDefinition(CreateLaplaceParameters("V(inp,inn)", "10/(s+1000)"));
            var noEquals = ParseDefinition(CreateNoEqualsLaplaceParameters("V(inp,inn)", "10/(s+1000)"));
            var equalsAfterKeyword = ParseDefinition(CreateEqualsAfterKeywordLaplaceParameters("V(inp,inn)", "10/(s+1000)"));

            AssertEquivalentDefinition(canonical, noEquals);
            AssertEquivalentDefinition(canonical, equalsAfterKeyword);
        }

        [Fact]
        public void When_SingleEndedLaplaceSourceIsParsed_Expect_ControlNegativeGround()
        {
            var context = CreateReadingContext();
            context.EvaluationContext.SetParameter("tau", 1e-6);
            var parser = new LaplaceSourceParser();

            var definition = parser.ParseVoltageControlledSource(
                "E1",
                CreateLaplaceParameters("V(in)", "1/(1+s*tau)"),
                context);

            Assert.NotNull(definition);
            Assert.False(context.Result.ValidationResult.HasError);
            Assert.Equal("out", definition.OutputPositiveNode);
            Assert.Equal("0", definition.OutputNegativeNode);
            Assert.Equal("in", definition.Input.ControlPositiveNode);
            Assert.Equal("0", definition.Input.ControlNegativeNode);
            AssertCoefficients(new[] { 1.0 }, definition.TransferFunction.NumeratorCoefficients);
            AssertCoefficients(new[] { 1.0, 1e-6 }, definition.TransferFunction.DenominatorCoefficients);
            Assert.Equal(0.0, definition.Delay);
        }

        [Theory]
        [MemberData(nameof(SupportedMultipliers))]
        public void When_MultiplierIsParsed_Expect_NumeratorScaled(string multiplierExpression, double expectedNumerator)
        {
            var context = CreateReadingContext();
            var parser = new LaplaceSourceParser();

            var definition = parser.ParseVoltageControlledSource(
                "E1",
                CreateLaplaceParameters("V(in)", "1/(1+s)", Assignment("M", multiplierExpression)),
                context);

            Assert.NotNull(definition);
            Assert.False(context.Result.ValidationResult.HasError);
            AssertCoefficients(new[] { expectedNumerator }, definition.TransferFunction.NumeratorCoefficients);
            AssertCoefficients(new[] { 1.0, 1.0 }, definition.TransferFunction.DenominatorCoefficients);
            Assert.Equal(0.0, definition.Delay);
        }

        [Theory]
        [MemberData(nameof(SupportedDelays))]
        public void When_DelayIsParsed_Expect_DefinitionDelay(string delayName, string delayExpression, double expectedDelay)
        {
            var context = CreateReadingContext();
            var parser = new LaplaceSourceParser();

            var definition = parser.ParseVoltageControlledSource(
                "E1",
                CreateLaplaceParameters("V(in)", "1/(1+s)", Assignment(delayName, delayExpression)),
                context);

            Assert.NotNull(definition);
            Assert.False(context.Result.ValidationResult.HasError);
            AssertCoefficients(new[] { 1.0 }, definition.TransferFunction.NumeratorCoefficients);
            Assert.Equal(expectedDelay, definition.Delay);
        }

        [Fact]
        public void When_DifferentialLaplaceSourceIsParsed_Expect_ControlNodeOrdering()
        {
            var context = CreateReadingContext();
            var parser = new LaplaceSourceParser();

            var definition = parser.ParseVoltageControlledSource(
                "E1",
                CreateLaplaceParameters("V(inp,inn)", "10/(1+s)"),
                context);

            Assert.NotNull(definition);
            Assert.False(context.Result.ValidationResult.HasError);
            Assert.Equal("inp", definition.Input.ControlPositiveNode);
            Assert.Equal("inn", definition.Input.ControlNegativeNode);
            AssertCoefficients(new[] { 10.0 }, definition.TransferFunction.NumeratorCoefficients);
            AssertCoefficients(new[] { 1.0, 1.0 }, definition.TransferFunction.DenominatorCoefficients);
        }

        [Fact]
        public void When_VoltageSourceGeneratorMapsLaplace_Expect_LaplaceEntityAndNodeOrder()
        {
            var context = CreateReadingContext();
            ParameterCollection createdNodes = null;
            context.When(x => x.CreateNodes(Arg.Any<IComponent>(), Arg.Any<ParameterCollection>()))
                .Do(call => createdNodes = call.ArgAt<ParameterCollection>(1));
            var generator = new VoltageSourceGenerator();

            var entity = generator.Generate(
                "E1",
                "E1",
                "e",
                CreateLaplaceParameters(
                    "V(inp,inn)",
                    "1000/(s+1000)",
                    Assignment("M", "2"),
                    Assignment("TD", "1n")),
                context);

            var laplace = Assert.IsType<LaplaceVoltageControlledVoltageSource>(entity);
            Assert.False(context.Result.ValidationResult.HasError);
            Assert.NotNull(createdNodes);
            Assert.Equal(new[] { "out", "0", "inp", "inn" }, createdNodes.Select(parameter => parameter.Value).ToArray());
            AssertCoefficients(new[] { 2000.0 }, laplace.Parameters.Numerator);
            AssertCoefficients(new[] { 1000.0, 1.0 }, laplace.Parameters.Denominator);
            Assert.Equal(1e-9, laplace.Parameters.Delay);
        }

        [Fact]
        public void When_VoltageSourceGeneratorMapsNoEqualsLaplace_Expect_LaplaceEntityAndNodeOrder()
        {
            var context = CreateReadingContext();
            ParameterCollection createdNodes = null;
            context.When(x => x.CreateNodes(Arg.Any<IComponent>(), Arg.Any<ParameterCollection>()))
                .Do(call => createdNodes = call.ArgAt<ParameterCollection>(1));
            var generator = new VoltageSourceGenerator();

            var entity = generator.Generate(
                "E1",
                "E1",
                "e",
                CreateNoEqualsLaplaceParameters("V(inp,inn)", "1000/(s+1000)"),
                context);

            var laplace = Assert.IsType<LaplaceVoltageControlledVoltageSource>(entity);
            Assert.False(context.Result.ValidationResult.HasError);
            Assert.NotNull(createdNodes);
            Assert.Equal(new[] { "out", "0", "inp", "inn" }, createdNodes.Select(parameter => parameter.Value).ToArray());
            AssertCoefficients(new[] { 1000.0 }, laplace.Parameters.Numerator);
            AssertCoefficients(new[] { 1000.0, 1.0 }, laplace.Parameters.Denominator);
            Assert.Equal(0.0, laplace.Parameters.Delay);
        }

        [Fact]
        public void When_CurrentSourceGeneratorMapsLaplace_Expect_LaplaceEntityAndNodeOrder()
        {
            var context = CreateReadingContext();
            ParameterCollection createdNodes = null;
            context.When(x => x.CreateNodes(Arg.Any<IComponent>(), Arg.Any<ParameterCollection>()))
                .Do(call => createdNodes = call.ArgAt<ParameterCollection>(1));
            var generator = new CurrentSourceGenerator();

            var entity = generator.Generate(
                "G1",
                "G1",
                "g",
                CreateLaplaceParameters(
                    "V(inp,inn)",
                    "1m*1000/(s+1000)",
                    Assignment("M", "2"),
                    Assignment("DELAY", "2n")),
                context);

            var laplace = Assert.IsType<LaplaceVoltageControlledCurrentSource>(entity);
            Assert.False(context.Result.ValidationResult.HasError);
            Assert.NotNull(createdNodes);
            Assert.Equal(new[] { "out", "0", "inp", "inn" }, createdNodes.Select(parameter => parameter.Value).ToArray());
            AssertCoefficients(new[] { 2.0 }, laplace.Parameters.Numerator);
            AssertCoefficients(new[] { 1000.0, 1.0 }, laplace.Parameters.Denominator);
            Assert.Equal(2e-9, laplace.Parameters.Delay);
        }

        [Fact]
        public void When_CurrentSourceGeneratorMapsEqualsAfterKeywordLaplace_Expect_LaplaceEntityAndNodeOrder()
        {
            var context = CreateReadingContext();
            ParameterCollection createdNodes = null;
            context.When(x => x.CreateNodes(Arg.Any<IComponent>(), Arg.Any<ParameterCollection>()))
                .Do(call => createdNodes = call.ArgAt<ParameterCollection>(1));
            var generator = new CurrentSourceGenerator();

            var entity = generator.Generate(
                "G1",
                "G1",
                "g",
                CreateEqualsAfterKeywordLaplaceParameters("V(inp,inn)", "1m*1000/(s+1000)"),
                context);

            var laplace = Assert.IsType<LaplaceVoltageControlledCurrentSource>(entity);
            Assert.False(context.Result.ValidationResult.HasError);
            Assert.NotNull(createdNodes);
            Assert.Equal(new[] { "out", "0", "inp", "inn" }, createdNodes.Select(parameter => parameter.Value).ToArray());
            AssertCoefficients(new[] { 1.0 }, laplace.Parameters.Numerator);
            AssertCoefficients(new[] { 1000.0, 1.0 }, laplace.Parameters.Denominator);
            Assert.Equal(0.0, laplace.Parameters.Delay);
        }

        [Theory]
        [MemberData(nameof(InvalidInputExpressions))]
        public void When_InputShapeIsUnsupported_Expect_ReaderValidationError(string inputExpression)
        {
            var context = CreateReadingContext();
            var parser = new LaplaceSourceParser();

            var definition = parser.ParseVoltageControlledSource(
                "E1",
                CreateLaplaceParameters(inputExpression, "1/(1+s)"),
                context);

            Assert.Null(definition);
            AssertSingleReaderError(context, "input expression");
        }

        [Theory]
        [MemberData(nameof(RejectedTransfers))]
        public void When_TransferIsUnsupported_Expect_ReaderValidationError(string transferExpression)
        {
            var context = CreateReadingContext();
            var parser = new LaplaceSourceParser();

            var definition = parser.ParseVoltageControlledSource(
                "E1",
                CreateLaplaceParameters("V(in)", transferExpression),
                context);

            Assert.Null(definition);
            AssertSingleReaderError(context, "laplace transfer");
        }

        [Theory]
        [MemberData(nameof(InvalidOptions))]
        public void When_InvalidOptionIsPresent_Expect_ReaderValidationError(Parameter[] options, string messageFragment)
        {
            var context = CreateReadingContext();
            var parser = new LaplaceSourceParser();

            var definition = parser.ParseVoltageControlledSource(
                "E1",
                CreateLaplaceParameters("V(in)", "1/(1+s)", options),
                context);

            Assert.Null(definition);
            AssertSingleReaderError(context, messageFragment);
        }

        [Fact]
        public void When_NonFiniteMultiplierIsPresent_Expect_ReaderValidationError()
        {
            var context = CreateReadingContext();
            context.EvaluationContext.SetParameter("inf", double.PositiveInfinity);
            var parser = new LaplaceSourceParser();

            var definition = parser.ParseVoltageControlledSource(
                "E1",
                CreateLaplaceParameters("V(in)", "1/(1+s)", Assignment("M", "inf")),
                context);

            Assert.Null(definition);
            AssertSingleReaderError(context, "finite");
        }

        [Fact]
        public void When_InputAssignmentIsMissing_Expect_ReaderValidationError()
        {
            var context = CreateReadingContext();
            var parser = new LaplaceSourceParser();
            var parameters = new ParameterCollection
            {
                new IdentifierParameter("out"),
                new IdentifierParameter("0"),
                new WordParameter("LAPLACE"),
                new ExpressionParameter("V(in)", null),
            };

            var definition = parser.ParseVoltageControlledSource("E1", parameters, context);

            Assert.Null(definition);
            AssertSingleReaderError(context, "transfer expression");
        }

        [Fact]
        public void When_InputExpressionIsMissing_Expect_ReaderValidationError()
        {
            var context = CreateReadingContext();
            var parser = new LaplaceSourceParser();
            var parameters = new ParameterCollection
            {
                new IdentifierParameter("out"),
                new IdentifierParameter("0"),
                new WordParameter("LAPLACE"),
            };

            var definition = parser.ParseVoltageControlledSource("E1", parameters, context);

            Assert.Null(definition);
            AssertSingleReaderError(context, "expects input expression");
        }

        [Fact]
        public void When_HSourceUsesLaplace_Expect_CurrentControlledLaplaceEntity()
        {
            var context = CreateReadingContext();
            ParameterCollection createdNodes = null;
            context.When(x => x.CreateNodes(Arg.Any<IComponent>(), Arg.Any<ParameterCollection>()))
                .Do(call => createdNodes = call.ArgAt<ParameterCollection>(1));
            var generator = new VoltageSourceGenerator();

            var entity = generator.Generate(
                "H1",
                "H1",
                "h",
                CreateLaplaceParameters(
                    "I(VSENSE)",
                    "1000/(s+1000)",
                    Assignment("M", "2"),
                    Assignment("TD", "1n")),
                context);

            var laplace = Assert.IsType<LaplaceCurrentControlledVoltageSource>(entity);
            Assert.False(context.Result.ValidationResult.HasError);
            Assert.NotNull(createdNodes);
            Assert.Equal(new[] { "out", "0" }, createdNodes.Select(parameter => parameter.Value).ToArray());
            Assert.Equal("VSENSE", laplace.ControllingSource);
            AssertCoefficients(new[] { 2000.0 }, laplace.Parameters.Numerator);
            AssertCoefficients(new[] { 1000.0, 1.0 }, laplace.Parameters.Denominator);
            Assert.Equal(1e-9, laplace.Parameters.Delay);
        }

        [Fact]
        public void When_HSourceUsesEqualsAfterKeywordLaplace_Expect_CurrentControlledLaplaceEntity()
        {
            var context = CreateReadingContext();
            var generator = new VoltageSourceGenerator();

            var entity = generator.Generate(
                "H1",
                "H1",
                "h",
                CreateEqualsAfterKeywordLaplaceParameters("I(VSENSE)", "1000/(s+1000)"),
                context);

            var laplace = Assert.IsType<LaplaceCurrentControlledVoltageSource>(entity);
            Assert.False(context.Result.ValidationResult.HasError);
            Assert.Equal("VSENSE", laplace.ControllingSource);
            AssertCoefficients(new[] { 1000.0 }, laplace.Parameters.Numerator);
            AssertCoefficients(new[] { 1000.0, 1.0 }, laplace.Parameters.Denominator);
        }

        [Fact]
        public void When_FSourceUsesLaplace_Expect_CurrentControlledLaplaceEntity()
        {
            var context = CreateReadingContext();
            ParameterCollection createdNodes = null;
            context.When(x => x.CreateNodes(Arg.Any<IComponent>(), Arg.Any<ParameterCollection>()))
                .Do(call => createdNodes = call.ArgAt<ParameterCollection>(1));
            var generator = new CurrentSourceGenerator();

            var entity = generator.Generate(
                "F1",
                "F1",
                "f",
                CreateLaplaceParameters(
                    "I(VSENSE)",
                    "1m*1000/(s+1000)",
                    Assignment("M", "2"),
                    Assignment("DELAY", "2n")),
                context);

            var laplace = Assert.IsType<LaplaceCurrentControlledCurrentSource>(entity);
            Assert.False(context.Result.ValidationResult.HasError);
            Assert.NotNull(createdNodes);
            Assert.Equal(new[] { "out", "0" }, createdNodes.Select(parameter => parameter.Value).ToArray());
            Assert.Equal("VSENSE", laplace.ControllingSource);
            AssertCoefficients(new[] { 2.0 }, laplace.Parameters.Numerator);
            AssertCoefficients(new[] { 1000.0, 1.0 }, laplace.Parameters.Denominator);
            Assert.Equal(2e-9, laplace.Parameters.Delay);
        }

        [Fact]
        public void When_FSourceUsesNoEqualsLaplace_Expect_CurrentControlledLaplaceEntity()
        {
            var context = CreateReadingContext();
            var generator = new CurrentSourceGenerator();

            var entity = generator.Generate(
                "F1",
                "F1",
                "f",
                CreateNoEqualsLaplaceParameters("I(VSENSE)", "1000/(s+1000)"),
                context);

            var laplace = Assert.IsType<LaplaceCurrentControlledCurrentSource>(entity);
            Assert.False(context.Result.ValidationResult.HasError);
            Assert.Equal("VSENSE", laplace.ControllingSource);
            AssertCoefficients(new[] { 1000.0 }, laplace.Parameters.Numerator);
            AssertCoefficients(new[] { 1000.0, 1.0 }, laplace.Parameters.Denominator);
        }

        [Fact]
        public void When_FSourceUsesVoltageLaplaceInput_Expect_CurrentInputValidationError()
        {
            var context = CreateReadingContext();
            var generator = new CurrentSourceGenerator();

            var entity = generator.Generate(
                "F1",
                "F1",
                "f",
                CreateLaplaceParameters("V(in)", "1/(1+s)"),
                context);

            Assert.Null(entity);
            AssertSingleReaderError(context, "I(source)");
        }

        [Fact]
        public void When_ValueLaplaceFunctionIsUsed_Expect_LaplaceEntity()
        {
            var context = CreateReadingContext();
            var generator = new VoltageSourceGenerator();

            var entity = generator.Generate(
                "E1",
                "E1",
                "e",
                new ParameterCollection
                {
                    new IdentifierParameter("out"),
                    new IdentifierParameter("0"),
                    Assignment("VALUE", "LAPLACE(V(in), 1/(1+s))"),
                },
                context);

            var laplace = Assert.IsType<LaplaceVoltageControlledVoltageSource>(entity);
            Assert.False(context.Result.ValidationResult.HasError);
            AssertCoefficients(new[] { 1.0 }, laplace.Parameters.Numerator);
            AssertCoefficients(new[] { 1.0, 1.0 }, laplace.Parameters.Denominator);
        }

        [Fact]
        public void When_ValueWordLaplaceFunctionIsUsed_Expect_CurrentLaplaceEntity()
        {
            var context = CreateReadingContext();
            var generator = new CurrentSourceGenerator();

            var entity = generator.Generate(
                "G1",
                "G1",
                "g",
                new ParameterCollection
                {
                    new IdentifierParameter("out"),
                    new IdentifierParameter("0"),
                    new WordParameter("VALUE"),
                    new ExpressionParameter("LAPLACE(V(in), 1/(1+s))", null),
                },
                context);

            var laplace = Assert.IsType<LaplaceVoltageControlledCurrentSource>(entity);
            Assert.False(context.Result.ValidationResult.HasError);
            AssertCoefficients(new[] { 1.0 }, laplace.Parameters.Numerator);
            AssertCoefficients(new[] { 1.0, 1.0 }, laplace.Parameters.Denominator);
        }

        [Fact]
        public void When_BSourceLaplaceFunctionIsUsed_Expect_LaplaceEntity()
        {
            var context = CreateReadingContext();
            var generator = new ArbitraryBehavioralGenerator();

            var entity = generator.Generate(
                "B1",
                "B1",
                "b",
                new ParameterCollection
                {
                    new IdentifierParameter("out"),
                    new IdentifierParameter("0"),
                    Assignment("V", "LAPLACE(V(in), 1/(1+s))"),
                },
                context);

            var laplace = Assert.IsType<LaplaceVoltageControlledVoltageSource>(entity);
            Assert.False(context.Result.ValidationResult.HasError);
            AssertCoefficients(new[] { 1.0 }, laplace.Parameters.Numerator);
            AssertCoefficients(new[] { 1.0, 1.0 }, laplace.Parameters.Denominator);
        }

        [Fact]
        public void When_MixedBSourceLaplaceFunctionIsUsed_Expect_HelperAndBehavioralEntity()
        {
            var context = CreateReadingContext();
            var generator = new ArbitraryBehavioralGenerator();

            var entity = generator.Generate(
                "B1",
                "B1",
                "b",
                new ParameterCollection
                {
                    new IdentifierParameter("out"),
                    new IdentifierParameter("0"),
                    Assignment("V", "1 + 2*LAPLACE(V(in), 1/(1+s))"),
                },
                context);

            var behavioral = Assert.IsType<BehavioralVoltageSource>(entity);
            Assert.False(context.Result.ValidationResult.HasError);
            Assert.Contains("__ssp_laplace_B1_0", behavioral.Parameters.Expression);
            Assert.Contains(context.ContextEntities, item => item is LaplaceVoltageControlledVoltageSource && item.Name == "__ssp_laplace_B1_0_src");
        }

        [Fact]
        public void When_SingleEqualsAppearsInsideFunctionArgument_Expect_EqualityNode()
        {
            var root = Parser.Parse(Lexer.FromString("LAPLACE(V(in), 1/(1+s), TD=1n)"), true);
            var call = Assert.IsType<FunctionNode>(root);
            var option = Assert.IsType<BinaryOperatorNode>(call.Arguments[2]);

            Assert.Equal(NodeTypes.Equals, option.NodeType);
            var left = Assert.IsType<VariableNode>(option.Left);
            Assert.Equal("TD", left.Name);
        }

        [Fact]
        public void When_DirectLaplaceFunctionUsesInlineOptions_Expect_NumeratorScaledAndDelaySet()
        {
            var context = CreateReadingContext();
            var generator = new ArbitraryBehavioralGenerator();

            var entity = generator.Generate(
                "B1",
                "B1",
                "b",
                new ParameterCollection
                {
                    new IdentifierParameter("out"),
                    new IdentifierParameter("0"),
                    Assignment("V", "LAPLACE(V(in), 1/(1+s), M=2, TD=1n)"),
                },
                context);

            var laplace = Assert.IsType<LaplaceVoltageControlledVoltageSource>(entity);
            Assert.False(context.Result.ValidationResult.HasError);
            AssertCoefficients(new[] { 2.0 }, laplace.Parameters.Numerator);
            Assert.Equal(1e-9, laplace.Parameters.Delay);
        }

        [Fact]
        public void When_MixedLaplaceFunctionUsesPerCallInlineOptions_Expect_EachHelperOwnsOptions()
        {
            var context = CreateReadingContext();
            var generator = new ArbitraryBehavioralGenerator();

            var entity = generator.Generate(
                "B1",
                "B1",
                "b",
                new ParameterCollection
                {
                    new IdentifierParameter("out"),
                    new IdentifierParameter("0"),
                    Assignment("V", "LAPLACE(V(a), 1/(1+s), M=2, TD=1n) + LAPLACE(V(b), 1/(1+s), M=3, DELAY=2n)"),
                },
                context);

            Assert.IsType<BehavioralVoltageSource>(entity);
            Assert.False(context.Result.ValidationResult.HasError);
            var firstHelper = Assert.IsType<LaplaceVoltageControlledVoltageSource>(context.ContextEntities["__ssp_laplace_B1_0_src"]);
            var secondHelper = Assert.IsType<LaplaceVoltageControlledVoltageSource>(context.ContextEntities["__ssp_laplace_B1_1_src"]);
            AssertCoefficients(new[] { 2.0 }, firstHelper.Parameters.Numerator);
            AssertCoefficients(new[] { 3.0 }, secondHelper.Parameters.Numerator);
            Assert.Equal(1e-9, firstHelper.Parameters.Delay);
            Assert.Equal(2e-9, secondHelper.Parameters.Delay);
        }

        [Fact]
        public void When_LaplaceFunctionUsesArbitraryInput_Expect_InputHelperAndVoltageControlledLaplace()
        {
            var context = CreateReadingContext();
            var generator = new ArbitraryBehavioralGenerator();

            var entity = generator.Generate(
                "B1",
                "B1",
                "b",
                new ParameterCollection
                {
                    new IdentifierParameter("out"),
                    new IdentifierParameter("0"),
                    Assignment("V", "LAPLACE(2*V(in), 1/(1+s))"),
                },
                context);

            Assert.IsType<LaplaceVoltageControlledVoltageSource>(entity);
            Assert.False(context.Result.ValidationResult.HasError);
            var inputHelper = Assert.IsType<BehavioralVoltageSource>(context.ContextEntities["__ssp_laplace_input_B1_0_src"]);
            Assert.Contains("V(in)", inputHelper.Parameters.Expression);
        }

        [Fact]
        public void When_LaplaceFunctionUsesDifferentialExpression_Expect_NoInputHelper()
        {
            var context = CreateReadingContext();
            var generator = new ArbitraryBehavioralGenerator();

            var entity = generator.Generate(
                "B1",
                "B1",
                "b",
                new ParameterCollection
                {
                    new IdentifierParameter("out"),
                    new IdentifierParameter("0"),
                    Assignment("V", "LAPLACE(V(a)-V(b), 1/(1+s))"),
                },
                context);

            Assert.IsType<LaplaceVoltageControlledVoltageSource>(entity);
            Assert.False(context.Result.ValidationResult.HasError);
            Assert.DoesNotContain(context.ContextEntities, item => item.Name.StartsWith("__ssp_laplace_input_", StringComparison.Ordinal));
        }

        [Fact]
        public void When_InlineAndSourceLevelDelayAreBothPresent_Expect_ReaderValidationError()
        {
            var context = CreateReadingContext();
            var generator = new ArbitraryBehavioralGenerator();

            var entity = generator.Generate(
                "B1",
                "B1",
                "b",
                new ParameterCollection
                {
                    new IdentifierParameter("out"),
                    new IdentifierParameter("0"),
                    Assignment("V", "LAPLACE(V(in), 1/(1+s), TD=1n)"),
                    Assignment("TD", "2n"),
                },
                context);

            Assert.Null(entity);
            AssertSingleReaderError(context, "only once");
        }

        [Fact]
        public void When_InlineAndSourceLevelMultiplierAreBothDirect_Expect_ReaderValidationError()
        {
            var context = CreateReadingContext();
            var generator = new ArbitraryBehavioralGenerator();

            var entity = generator.Generate(
                "B1",
                "B1",
                "b",
                new ParameterCollection
                {
                    new IdentifierParameter("out"),
                    new IdentifierParameter("0"),
                    Assignment("I", "LAPLACE(V(in), 1/(1+s), M=2)"),
                    Assignment("M", "3"),
                },
                context);

            Assert.Null(entity);
            AssertSingleReaderError(context, "only once");
        }

        [Fact]
        public void When_MixedCurrentOutputUsesInlineAndSourceLevelMultiplier_Expect_BothScalesPreserved()
        {
            var context = CreateReadingContext();
            var generator = new ArbitraryBehavioralGenerator();

            var entity = generator.Generate(
                "B1",
                "B1",
                "b",
                new ParameterCollection
                {
                    new IdentifierParameter("out"),
                    new IdentifierParameter("0"),
                    Assignment("I", "1 + LAPLACE(V(in), 1/(1+s), M=2)"),
                    Assignment("M", "3"),
                },
                context);

            var behavioral = Assert.IsType<BehavioralCurrentSource>(entity);
            Assert.False(context.Result.ValidationResult.HasError);
            Assert.Contains("* (3)", behavioral.Parameters.Expression);
            var helper = Assert.IsType<LaplaceVoltageControlledVoltageSource>(context.ContextEntities["__ssp_laplace_B1_0_src"]);
            AssertCoefficients(new[] { 2.0 }, helper.Parameters.Numerator);
        }

        [Fact]
        public void When_InlineOptionIsUnknown_Expect_ReaderValidationError()
        {
            var context = CreateReadingContext();
            var generator = new ArbitraryBehavioralGenerator();

            var entity = generator.Generate(
                "B1",
                "B1",
                "b",
                new ParameterCollection
                {
                    new IdentifierParameter("out"),
                    new IdentifierParameter("0"),
                    Assignment("V", "LAPLACE(V(in), 1/(1+s), FOO=1)"),
                },
                context);

            Assert.Null(entity);
            AssertSingleReaderError(context, "unknown");
        }

        [Fact]
        public void When_InlineOptionHasNoAssignment_Expect_ReaderValidationError()
        {
            var context = CreateReadingContext();
            var generator = new ArbitraryBehavioralGenerator();

            var entity = generator.Generate(
                "B1",
                "B1",
                "b",
                new ParameterCollection
                {
                    new IdentifierParameter("out"),
                    new IdentifierParameter("0"),
                    Assignment("V", "LAPLACE(V(in), 1/(1+s), TD)"),
                },
                context);

            Assert.Null(entity);
            AssertSingleReaderError(context, "assignment syntax");
        }

        private static LaplaceSourceDefinition ParseDefinition(ParameterCollection parameters)
        {
            var context = CreateReadingContext();
            var parser = new LaplaceSourceParser();

            var definition = parser.ParseVoltageControlledSource("E1", parameters, context);

            Assert.NotNull(definition);
            Assert.False(context.Result.ValidationResult.HasError);
            return definition;
        }

        private static ParameterCollection CreateLaplaceParameters(
            string inputExpression,
            string transferExpression,
            params Parameter[] extraParameters)
        {
            var parameters = new ParameterCollection
            {
                new IdentifierParameter("out"),
                new IdentifierParameter("0"),
                new WordParameter("LAPLACE"),
                new ExpressionAssignmentParameter(inputExpression, transferExpression, null),
            };

            foreach (var parameter in extraParameters)
            {
                parameters.Add(parameter);
            }

            return parameters;
        }

        private static ParameterCollection CreateNoEqualsLaplaceParameters(
            string inputExpression,
            string transferExpression,
            params Parameter[] extraParameters)
        {
            var parameters = new ParameterCollection
            {
                new IdentifierParameter("out"),
                new IdentifierParameter("0"),
                new WordParameter("LAPLACE"),
                new ExpressionParameter(inputExpression, null),
                new ExpressionParameter(transferExpression, null),
            };

            foreach (var parameter in extraParameters)
            {
                parameters.Add(parameter);
            }

            return parameters;
        }

        private static ParameterCollection CreateEqualsAfterKeywordLaplaceParameters(
            string inputExpression,
            string transferExpression,
            params Parameter[] extraParameters)
        {
            var parameters = new ParameterCollection
            {
                new IdentifierParameter("out"),
                new IdentifierParameter("0"),
                Assignment("LAPLACE", inputExpression),
                new ExpressionParameter(transferExpression, null),
            };

            foreach (var parameter in extraParameters)
            {
                parameters.Add(parameter);
            }

            return parameters;
        }

        private static AssignmentParameter Assignment(string name, string value)
        {
            return new AssignmentParameter
            {
                Name = name,
                Value = value,
            };
        }

        private static IReadingContext CreateReadingContext()
        {
            var context = Substitute.For<IReadingContext>();
            var evaluationContext = CreateEvaluationContext();
            var circuit = new Circuit();
            context.Result.Returns(new SpiceSharpModel(circuit, "test"));
            context.ContextEntities.Returns(circuit);
            context.ReaderSettings.Returns(
                new SpiceNetlistReaderSettings(
                    new SpiceNetlistCaseSensitivitySettings(),
                    () => string.Empty,
                    Encoding.Default));
            context.EvaluationContext.Returns(evaluationContext);
            context.Evaluator.Returns(evaluationContext.Evaluator);
            context.NameGenerator.Returns(evaluationContext.NameGenerator);
            context.SimulationPreparations.Returns(Substitute.For<ISimulationPreparations>());
            return context;
        }

        private static EvaluationContext CreateEvaluationContext()
        {
            var caseSettings = new SpiceNetlistCaseSensitivitySettings();
            var nodeNameGenerator = new MainCircuitNodeNameGenerator(
                new[] { "0" },
                caseSettings.IsEntityNamesCaseSensitive,
                ".");

            var objectNameGenerator = new ObjectNameGenerator(string.Empty, ".");
            INameGenerator nameGenerator = new NameGenerator(nodeNameGenerator, objectNameGenerator);
            var expressionParserFactory = new ExpressionParserFactory(caseSettings);
            var expressionResolverFactory = new ExpressionResolverFactory(caseSettings);

            var context = new SpiceEvaluationContext(
                string.Empty,
                caseSettings,
                new Randomizer(caseSettings.IsDistributionNameCaseSensitive, seed: 0),
                expressionParserFactory,
                new ExpressionFeaturesReader(expressionParserFactory, expressionResolverFactory),
                nameGenerator);

            context.Evaluator = new Evaluator(context, new ExpressionValueProvider(expressionParserFactory));
            return context;
        }

        private static void AssertSingleReaderError(IReadingContext context, string messageFragment)
        {
            var error = Assert.Single(context.Result.ValidationResult.Errors);
            Assert.Equal(ValidationEntrySource.Reader, error.Source);
            Assert.Contains(messageFragment, error.Message, StringComparison.OrdinalIgnoreCase);
        }

        private static void AssertEquivalentDefinition(
            LaplaceSourceDefinition expected,
            LaplaceSourceDefinition actual)
        {
            Assert.Equal(expected.OutputPositiveNode, actual.OutputPositiveNode);
            Assert.Equal(expected.OutputNegativeNode, actual.OutputNegativeNode);
            Assert.Equal(expected.Input.ControlPositiveNode, actual.Input.ControlPositiveNode);
            Assert.Equal(expected.Input.ControlNegativeNode, actual.Input.ControlNegativeNode);
            Assert.Equal(expected.InputExpression, actual.InputExpression);
            Assert.Equal(expected.TransferExpression, actual.TransferExpression);
            Assert.Equal(expected.Delay, actual.Delay);
            AssertCoefficients(
                expected.TransferFunction.NumeratorCoefficients.ToArray(),
                actual.TransferFunction.NumeratorCoefficients);
            AssertCoefficients(
                expected.TransferFunction.DenominatorCoefficients.ToArray(),
                actual.TransferFunction.DenominatorCoefficients);
        }

        private static void AssertCoefficients(double[] expected, IReadOnlyList<double> actual)
        {
            Assert.Equal(expected.Length, actual.Count);
            for (var i = 0; i < expected.Length; i++)
            {
                var tolerance = Math.Max(1e-10, Math.Abs(expected[i]) * 1e-12);
                Assert.True(
                    Math.Abs(expected[i] - actual[i]) <= tolerance,
                    $"Expected coefficient {i} to be {expected[i]}, but found {actual[i]}.");
            }
        }
    }
}
