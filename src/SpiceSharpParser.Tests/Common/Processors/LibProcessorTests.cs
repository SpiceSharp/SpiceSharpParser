using NSubstitute;
using SpiceSharpParser.Common;
using SpiceSharpParser.Common.FileSystem;
using SpiceSharpParser.Common.Processors;
using SpiceSharpParser.Common.Validation;
using SpiceSharpParser.Lexers.Netlist.Spice;
using SpiceSharpParser.Models.Netlist.Spice;
using SpiceSharpParser.Models.Netlist.Spice.Objects;
using SpiceSharpParser.Models.Netlist.Spice.Objects.Parameters;
using SpiceSharpParser.Parsers.Netlist.Spice;
using System.Collections.Generic;
using System.IO;
using Xunit;

namespace SpiceSharpParser.Tests.Common.Processors
{
    public class LibProcessorTests
    {
        [Fact]
        public void When_SelectedLibHasNoEndl_Expect_ParserException()
        {
            using var fixture = CreateFixture(CreateLibStatements(includeEndl: false));

            Assert.Throws<SpiceSharpParserException>(() => fixture.Processor.Process(CreateRequestStatements(fixture.LibPath)));
        }

        [Fact]
        public void When_SelectedLibHasEndl_Expect_EndlIsNotIncludedInReplacement()
        {
            var includedComponent = CreateComponent("R1");
            using var fixture = CreateFixture(CreateLibStatements(includeEndl: true, includedComponent));
            var requestStatements = CreateRequestStatements(fixture.LibPath);

            fixture.Processor.Process(requestStatements);

            var statement = Assert.Single(requestStatements);
            Assert.Same(includedComponent, statement);
        }

        private static ProcessorFixture CreateFixture(Statements includeStatements)
        {
            var libPath = Path.GetTempFileName();
            File.WriteAllText(libPath, "unused");

            var fileReader = Substitute.For<IFileReader>();
            fileReader.ReadAll(Arg.Any<string>()).Returns("unused");

            var tokens = new[] { new SpiceToken(SpiceTokenType.EOF, string.Empty) };
            var tokenProvider = Substitute.For<ISpiceTokenProvider>();
            tokenProvider.GetTokens("unused").Returns(tokens);

            var tokenProviderPool = Substitute.For<ISpiceTokenProviderPool>();
            tokenProviderPool.GetSpiceTokenProvider(Arg.Any<SpiceLexerSettings>()).Returns(tokenProvider);

            var parser = Substitute.For<ISingleSpiceNetlistParser>();
            parser.Parse(tokens).Returns(new SpiceNetlist(string.Empty, includeStatements));

            var includesProcessor = Substitute.For<IProcessor>();
            var processor = new LibProcessor(
                fileReader,
                tokenProviderPool,
                parser,
                includesProcessor,
                () => Path.GetDirectoryName(libPath),
                new SpiceLexerSettings(false))
            {
                Validation = new ValidationEntryCollection(),
            };

            return new ProcessorFixture(libPath, processor);
        }

        private static Statements CreateRequestStatements(string libPath)
        {
            var statements = new Statements();
            statements.Add(new Control(
                "lib",
                new ParameterCollection(
                    new List<Parameter>()
                    {
                        new WordParameter(libPath),
                        new WordParameter("entry"),
                    }),
                null));

            return statements;
        }

        private static Statements CreateLibStatements(bool includeEndl, Component includedComponent = null)
        {
            var statements = new Statements();
            statements.Add(new Control(
                "lib",
                new ParameterCollection(new List<Parameter>() { new WordParameter("entry") }),
                null));
            statements.Add(includedComponent ?? CreateComponent("R1"));

            if (includeEndl)
            {
                statements.Add(new Control("endl", new ParameterCollection(), null));
            }

            return statements;
        }

        private static Component CreateComponent(string name)
        {
            return new Component(
                name,
                new ParameterCollection(
                    new List<Parameter>()
                    {
                        new WordParameter("in"),
                        new WordParameter("gnd"),
                        new ValueParameter("1k"),
                    }),
                null);
        }

        private sealed class ProcessorFixture : System.IDisposable
        {
            public ProcessorFixture(string libPath, LibProcessor processor)
            {
                this.LibPath = libPath;
                this.Processor = processor;
            }

            public string LibPath { get; }

            public LibProcessor Processor { get; }

            public void Dispose()
            {
                if (File.Exists(this.LibPath))
                {
                    File.Delete(this.LibPath);
                }
            }
        }
    }
}
