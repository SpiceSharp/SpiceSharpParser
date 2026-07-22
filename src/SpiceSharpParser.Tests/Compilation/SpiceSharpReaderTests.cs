using System;
using System.Collections.Generic;
using SpiceSharp.Entities;
using SpiceSharpParser.Common;
using SpiceSharpParser.Diagnostics;
using SpiceSharpParser.ModelReaders.Netlist.Spice;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Context;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.EntityGenerators;
using SpiceSharpParser.Models.Netlist.Spice;
using SpiceSharpParser.Models.Netlist.Spice.Objects;
using Xunit;

namespace SpiceSharpParser.Tests.Compilation
{
    public class SpiceSharpReaderTests
    {
        [Fact]
        public void ReadResult_WhenReaderReportsInputError_ReturnsDiagnosticAndPartialModel()
        {
            SpiceNetlist netlist = Parse(
                Lines(
                    "reader structured result",
                    "V1 out 0 1",
                    ".backanno",
                    ".end"),
                "vendor.net");

            SpiceNetlistReadResult result = new SpiceSharpReader().ReadResult(netlist);

            Assert.False(result.Success);
            Assert.Null(result.Model);
            Assert.NotNull(result.PartialModel);
            SpiceDiagnostic diagnostic = Assert.Single(result.Diagnostics);
            Assert.Equal(SpiceDiagnosticCodes.UnsupportedControl, diagnostic.Code);
            Assert.Equal(DiagnosticStage.Reader, diagnostic.Stage);
            Assert.Equal("vendor.net", diagnostic.Span.FilePath);
            Assert.Equal(3, diagnostic.Span.Start.Line);
        }

        [Fact]
        public void Read_WhenReaderReportsInputError_ReturnsLegacyPartialModel()
        {
            SpiceNetlist netlist = Parse(
                Lines(
                    "reader legacy result",
                    "V1 out 0 1",
                    ".backanno",
                    ".end"),
                "legacy.net");

            SpiceSharpModel model = new SpiceSharpReader().Read(netlist);

            Assert.NotNull(model);
            Assert.True(model.ValidationResult.HasError);
        }

        [Fact]
        public void ReadResult_WhenSimulationFactoryRejectsInput_ReturnsDiagnosticInsteadOfNullReference()
        {
            SpiceNetlist netlist = Parse(
                Lines(
                    "reader invalid transient",
                    "V1 out 0 1",
                    ".tran 1m",
                    ".end"),
                "transient.net");

            SpiceNetlistReadResult result = new SpiceSharpReader().ReadResult(netlist);

            Assert.False(result.Success);
            SpiceDiagnostic diagnostic = Assert.Single(result.Diagnostics);
            Assert.Equal(DiagnosticStage.Reader, diagnostic.Stage);
            Assert.Equal("transient.net", diagnostic.Span.FilePath);
            Assert.Equal(3, diagnostic.Span.Start.Line);
            Assert.Contains("Maximum time expected", diagnostic.Message);
        }

        [Fact]
        public void ReadResult_WhenRecoverableFailureEscapesStatementReader_ReturnsLocatedDiagnostic()
        {
            var failure = new SpiceSharpParserException(
                "Invalid reader input",
                new SpiceLineInfo
                {
                    FileName = "input.lib",
                    LineNumber = 7,
                    StartColumnIndex = 4,
                    EndColumnIndex = 12,
                });
            var reader = new SpiceSharpReader();
            reader.Settings.Orderer = new ThrowingOrderer(failure);

            SpiceNetlistReadResult result = reader.ReadResult(EmptyNetlist());

            Assert.False(result.Success);
            Assert.NotNull(result.PartialModel);
            SpiceDiagnostic diagnostic = Assert.Single(result.Diagnostics);
            Assert.Equal(DiagnosticStage.Reader, diagnostic.Stage);
            Assert.Equal("input.lib", diagnostic.Span.FilePath);
            Assert.Equal(7, diagnostic.Span.Start.Line);
            Assert.Contains("Invalid reader input", diagnostic.Message);
        }

        [Fact]
        public void ReadMethods_WhenUnexpectedReaderFailureOccurs_PropagateOriginalException()
        {
            var failure = new UnexpectedReaderException();
            var reader = new SpiceSharpReader();
            reader.Settings.Mappings.Components.Map("R", new ThrowingComponentGenerator(failure));
            SpiceNetlist netlist = Parse(
                Lines(
                    "reader internal failure",
                    "R1 out 0 1k",
                    ".end"),
                "internal.net");

            Assert.Same(failure, Assert.Throws<UnexpectedReaderException>(() => reader.ReadResult(netlist)));
            Assert.Same(failure, Assert.Throws<UnexpectedReaderException>(() => reader.Read(netlist)));
        }

        [Fact]
        public void ReadMethods_WhenModelIsNull_ThrowArgumentNullException()
        {
            var reader = new SpiceSharpReader();

            Assert.Throws<ArgumentNullException>(() => reader.ReadResult(null));
            Assert.Throws<ArgumentNullException>(() => reader.Read(null));
        }

        private static SpiceNetlist Parse(string source, string sourceName)
        {
            SpiceNetlistParseResult result = new SpiceNetlistParser().ParseNetlist(source, sourceName);
            Assert.NotNull(result.FinalModel);
            Assert.False(result.ValidationResult.HasError);
            return result.FinalModel;
        }

        private static SpiceNetlist EmptyNetlist()
        {
            return new SpiceNetlist("reader boundary", new Statements());
        }

        private static string Lines(params string[] lines)
        {
            return string.Join(Environment.NewLine, lines);
        }

        private sealed class ThrowingOrderer : ISpiceStatementsOrderer
        {
            private readonly Exception exception;

            public ThrowingOrderer(Exception exception)
            {
                this.exception = exception;
            }

            public IEnumerable<Statement> Order(Statements statements)
            {
                throw this.exception;
            }
        }

        private sealed class UnexpectedReaderException : Exception
        {
        }

        private sealed class ThrowingComponentGenerator : IComponentGenerator
        {
            private readonly Exception exception;

            public ThrowingComponentGenerator(Exception exception)
            {
                this.exception = exception;
            }

            public IEntity Generate(
                string componentIdentifier,
                string originalName,
                string type,
                ParameterCollection parameters,
                IReadingContext context)
            {
                throw this.exception;
            }
        }
    }
}
