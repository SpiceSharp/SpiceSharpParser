using SpiceSharp.Simulations;
using SpiceSharpParser.ModelReader.Netlist.Spice.Context;
using SpiceSharpParser.ModelReader.Netlist.Spice.Readers.Controls.Exporters;
using SpiceSharpParser.ModelReader.Netlist.Spice.Registries;
using SpiceSharpParser.Model.Netlist.Spice.Objects;
using SpiceSharpParser.Model.Netlist.Spice.Objects.Parameters;

namespace SpiceSharpParser.ModelReader.Netlist.Spice.Readers.Controls
{
    public abstract class ExportControl : BaseControl
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ExportControl"/> class.
        /// </summary>
        /// <param name="registry">The exporter registry</param>
        public ExportControl(IExporterRegistry registry)
        {
            Registry = registry;
        }

        /// <summary>
        /// Gets the exporter registry
        /// </summary>
        protected IExporterRegistry Registry { get; }

        /// <summary>
        /// Generates a new export
        /// </summary>
        protected Export GenerateExport(Parameter parameter, Simulation simulation, IReadingContext context)
        {
            if (parameter is BracketParameter bp)
            {
                string type = bp.Name.ToLower();

                if (Registry.Supports(type))
                {
                    return Registry.Get(type).CreateExport(type, bp.Parameters, simulation, context);
                }
            }

            if (parameter is ReferenceParameter rp)
            {
                string type = "@";

                if (Registry.Supports(type))
                {
                    var parameters = new ParameterCollection();
                    parameters.Add(new WordParameter(rp.Name));
                    parameters.Add(new WordParameter(rp.Argument));

                    return Registry.Get(type).CreateExport(type, parameters, simulation, context);
                }
            }

            throw new System.Exception("Unsuported export: " + parameter.Image);
        }
    }
}
