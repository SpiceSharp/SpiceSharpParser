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
        public void When_SingleEndedLaplaceSourceIsParsed_Expect_ControlNegativeGround()
        {
            var context = CreateReadingContext();
            context.EvaluationContext.SetParameter("tau", 1e-6);
            var parser = new LaplaceSourceParser();

            var definition = parser.ParseVoltageControlledVoltageSource(
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

            var definition = parser.ParseVoltageControlledVoltageSource(
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

        [Theory]
        [MemberData(nameof(InvalidInputExpressions))]
        public void When_InputShapeIsUnsupported_Expect_ReaderValidationError(string inputExpression)
        {
            var context = CreateReadingContext();
            var parser = new LaplaceSourceParser();

            var definition = parser.ParseVoltageControlledVoltageSource(
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

            var definition = parser.ParseVoltageControlledVoltageSource(
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

            var definition = parser.ParseVoltageControlledVoltageSource(
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

            var definition = parser.ParseVoltageControlledVoltageSource("E1", parameters, context);

            Assert.Null(definition);
            AssertSingleReaderError(context, "separated by '='");
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

            var definition = parser.ParseVoltageControlledVoltageSource("E1", parameters, context);

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
            AssertSingleReaderError(context, "only for E");
        }

        [Fact]
        public void When_GSourceUsesLaplace_Expect_UnsupportedReaderValidationError()
        {
            var context = CreateReadingContext();
            var generator = new CurrentSourceGenerator();

            var entity = generator.Generate(
                "G1",
                "G1",
                "g",
                CreateLaplaceParameters("V(in)", "1/(1+s)"),
                context);

            Assert.Null(entity);
            AssertSingleReaderError(context, "G mapping remains unsupported");
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
