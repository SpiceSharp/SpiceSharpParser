using System.Text;
using SpiceSharpParser.Common;
using SpiceSharpParser.ModelReaders.Netlist.Spice;
using Xunit;

namespace SpiceSharpParser.Tests.Common
{
    public class CompatibilityOptionsTests
    {
        [Fact]
        public void When_ParserSettingsAreCreated_Expect_NoCompatibilityByDefault()
        {
            var settings = new SpiceNetlistParserSettings();

            Assert.Same(CompatibilityOptions.None, settings.Compatibility);
            Assert.False(settings.Compatibility.IsLTspice);
            Assert.False(settings.Compatibility.IsPSpice);
        }

        [Fact]
        public void When_ReaderSettingsAreCreated_Expect_NoCompatibilityByDefault()
        {
            var settings = new SpiceNetlistReaderSettings();

            Assert.Same(CompatibilityOptions.None, settings.Compatibility);
            Assert.False(settings.Compatibility.IsLTspice);
            Assert.False(settings.Compatibility.IsPSpice);
        }

        [Fact]
        public void When_PSpiceCompatibilityIsAssigned_Expect_SettingsExposeIt()
        {
            var parserSettings = new SpiceNetlistParserSettings();
            var readerSettings = new SpiceNetlistReaderSettings();

            parserSettings.Compatibility = CompatibilityOptions.PSpice;
            readerSettings.Compatibility = CompatibilityOptions.PSpice;

            Assert.Same(CompatibilityOptions.PSpice, parserSettings.Compatibility);
            Assert.Same(CompatibilityOptions.PSpice, readerSettings.Compatibility);
            Assert.True(parserSettings.Compatibility.IsPSpice);
            Assert.True(readerSettings.Compatibility.IsPSpice);
            Assert.False(parserSettings.Compatibility.IsLTspice);
            Assert.False(readerSettings.Compatibility.IsLTspice);
        }

        [Fact]
        public void When_LTspiceCompatibilityIsAssigned_Expect_SettingsExposeIt()
        {
            var parserSettings = new SpiceNetlistParserSettings();
            var readerSettings = new SpiceNetlistReaderSettings();

            parserSettings.Compatibility = CompatibilityOptions.LTspice;
            readerSettings.Compatibility = CompatibilityOptions.LTspice;

            Assert.Same(CompatibilityOptions.LTspice, parserSettings.Compatibility);
            Assert.Same(CompatibilityOptions.LTspice, readerSettings.Compatibility);
            Assert.True(parserSettings.Compatibility.IsLTspice);
            Assert.True(readerSettings.Compatibility.IsLTspice);
            Assert.False(parserSettings.Compatibility.IsPSpice);
            Assert.False(readerSettings.Compatibility.IsPSpice);
        }

        [Fact]
        public void When_ReaderSettingsAreCloned_Expect_CompatibilityIsPreserved()
        {
            var settings = new SpiceNetlistReaderSettings(
                new SpiceNetlistCaseSensitivitySettings(),
                () => "work",
                Encoding.Default)
            {
                Compatibility = CompatibilityOptions.LTspice,
            };

            var clone = settings.Clone();

            Assert.Same(CompatibilityOptions.LTspice, clone.Compatibility);
        }
    }
}
