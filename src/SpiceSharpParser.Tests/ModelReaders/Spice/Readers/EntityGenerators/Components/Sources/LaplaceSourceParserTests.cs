using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NSubstitute;
using SpiceSharp;
using SpiceSharp.Components;
using SpiceSharpParser.Common;
using SpiceSharpParser.Common.Evaluation;
using SpiceSharpParser.Common.Mathematics.Probability;
using SpiceSharpParser.Common.Validation;
using SpiceSharpParser.ModelReaders.Netlist.Spice;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Context;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Context.Names;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Evaluation;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.EntityGenerators.Components.Sources;
using SpiceSharpParser.Models.Netlist.Spice.Objects;
using SpiceSharpParser.Models.Netlist.Spice.Objects.Parameters;
using Xunit;

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

        public static IEnumerable<object[]> UnsupportedOptions
        {
            get
            {
                yield return new object[] { Assignment("M", "2"), "multiplier" };
                yield return new object[] { Assignment("TD", "1n"), "delay" };
                yield return new object[] { Assignment("DELAY", "1n"), "delay" };
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
                CreateLaplaceParameters("V(inp,inn)", "1000/(s+1000)"),
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
                CreateLaplaceParameters("V(inp,inn)", "1m*1000/(s+1000)"),
                context);

            var laplace = Assert.IsType<LaplaceVoltageControlledCurrentSource>(entity);
            Assert.False(context.Result.ValidationResult.HasError);
            Assert.NotNull(createdNodes);
            Assert.Equal(new[] { "out", "0", "inp", "inn" }, createdNodes.Select(parameter => parameter.Value).ToArray());
            AssertCoefficients(new[] { 1.0 }, laplace.Parameters.Numerator);
            AssertCoefficients(new[] { 1000.0, 1.0 }, laplace.Parameters.Denominator);
            Assert.Equal(0.0, laplace.Parameters.Delay);
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
        [MemberData(nameof(UnsupportedOptions))]
        public void When_UnsupportedOptionIsPresent_Expect_ReaderValidationError(Parameter option, string messageFragment)
        {
            var context = CreateReadingContext();
            var parser = new LaplaceSourceParser();

            var definition = parser.ParseVoltageControlledSource(
                "E1",
                CreateLaplaceParameters("V(in)", "1/(1+s)", option),
                context);

            Assert.Null(definition);
            AssertSingleReaderError(context, messageFragment);
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
        public void When_HSourceUsesLaplace_Expect_UnsupportedReaderValidationError()
        {
            var context = CreateReadingContext();
            var generator = new VoltageSourceGenerator();

            var entity = generator.Generate(
                "H1",
                "H1",
                "h",
                CreateLaplaceParameters("V(in)", "1/(1+s)"),
                context);

            Assert.Null(entity);
            AssertSingleReaderError(context, "E and G");
        }

        [Fact]
        public void When_HSourceUsesEqualsAfterKeywordLaplace_Expect_UnsupportedReaderValidationError()
        {
            var context = CreateReadingContext();
            var generator = new VoltageSourceGenerator();

            var entity = generator.Generate(
                "H1",
                "H1",
                "h",
                CreateEqualsAfterKeywordLaplaceParameters("V(in)", "1/(1+s)"),
                context);

            Assert.Null(entity);
            AssertSingleReaderError(context, "E and G");
        }

        [Fact]
        public void When_FSourceUsesLaplace_Expect_UnsupportedReaderValidationError()
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
            AssertSingleReaderError(context, "E and G");
        }

        [Fact]
        public void When_FSourceUsesNoEqualsLaplace_Expect_UnsupportedReaderValidationError()
        {
            var context = CreateReadingContext();
            var generator = new CurrentSourceGenerator();

            var entity = generator.Generate(
                "F1",
                "F1",
                "f",
                CreateNoEqualsLaplaceParameters("V(in)", "1/(1+s)"),
                context);

            Assert.Null(entity);
            AssertSingleReaderError(context, "E and G");
        }

        [Fact]
        public void When_ValueLaplaceFunctionIsUsed_Expect_UnsupportedReaderValidationError()
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

            Assert.Null(entity);
            AssertSingleReaderError(context, "function syntax");
        }

        [Fact]
        public void When_ValueWordLaplaceFunctionIsUsed_Expect_UnsupportedReaderValidationError()
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

            Assert.Null(entity);
            AssertSingleReaderError(context, "function syntax");
        }

        [Fact]
        public void When_BSourceLaplaceFunctionIsUsed_Expect_UnsupportedReaderValidationError()
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

            Assert.Null(entity);
            AssertSingleReaderError(context, "function syntax");
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
            context.Result.Returns(new SpiceSharpModel(new Circuit(), "test"));
            context.ReaderSettings.Returns(
                new SpiceNetlistReaderSettings(
                    new SpiceNetlistCaseSensitivitySettings(),
                    () => string.Empty,
                    Encoding.Default));
            context.EvaluationContext.Returns(CreateEvaluationContext());
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
