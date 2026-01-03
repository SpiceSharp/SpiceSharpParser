using SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.EntityGenerators.Components.Semiconductors;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.EntityGenerators.Models;
using System.IO;
using Xunit;

namespace SpiceSharpParser.IntegrationTests.Examples.Extensions
{
    public class CustomMosfetModelTest : BaseTests
    {
        [Fact]
        public void When_CustomMosfetModel_Used_NoExceptions()
        {
            // Create a model from text file
            string path = Path.Combine(Directory.GetCurrentDirectory(), "Examples/Circuits/MosfetExample.cir");
            var netlistContent = File.ReadAllText(path);
            var parser = new SpiceNetlistParser();
            parser.Settings.Lexing.HasTitle = true;
            var parseResult = parser.ParseNetlist(netlistContent);

            // Convert to Spice#
            var spiceSharpReader = new SpiceSharpReader();
            spiceSharpReader.Settings.CaseSensitivity.IsModelTypeCaseSensitive = false;
            spiceSharpReader.Settings.Mappings.Models.Map(new[] { "PMOS", "NMOS" }, new CustomMosfetModelGenerator());
            var spiceSharpModel = spiceSharpReader.Read(parseResult.FinalModel);
            
            Assert.False(spiceSharpModel.ValidationResult.HasError);
            Assert.False(spiceSharpModel.ValidationResult.HasWarning);
        }
        [Fact]
        public void When_CustomMosfetModel2_Used_NoExceptions()
        {
            // Create a model from text file
            string path = Path.Combine(Directory.GetCurrentDirectory(), "Examples/Circuits/MosfetExample2.cir");
            var netlistContent = File.ReadAllText(path);
            var parser = new SpiceNetlistParser();
            parser.Settings.Lexing.HasTitle = true;
            var parseResult = parser.ParseNetlist(netlistContent);

            // Convert to Spice#
            var spiceSharpReader = new SpiceSharpReader();
            spiceSharpReader.Settings.CaseSensitivity.IsModelTypeCaseSensitive = false;

            // custom mosfet models
            var modelGenerator = new MosfetModelGenerator();
            modelGenerator.AddLevel<SpiceSharp.Components.Mosfet3Model, SpiceSharp.Components.Mosfets.Level3.ModelParameters>(4);
            spiceSharpReader.Settings.Mappings.Models.Map(new[] { "PMOS", "NMOS" }, modelGenerator);
            var mosfetGenerator = new MosfetGenerator();
            mosfetGenerator.AddMosfet<SpiceSharp.Components.Mosfet3Model, SpiceSharp.Components.Mosfet3>();
            spiceSharpReader.Settings.Mappings.Components.Map("M", mosfetGenerator);


            var spiceSharpModel = spiceSharpReader.Read(parseResult.FinalModel);

            Assert.False(spiceSharpModel.ValidationResult.HasError);
            Assert.False(spiceSharpModel.ValidationResult.HasWarning);
        }
    }
}
