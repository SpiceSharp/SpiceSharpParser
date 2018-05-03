using SpiceSharp.Simulations;
using SpiceSharpParser.Connector.Context;
using SpiceSharpParser.Connector.Processors.Controls.Exporters;
using SpiceSharpParser.Connector.Registries;
using SpiceSharpParser.Model.SpiceObjects;
using SpiceSharpParser.Model.SpiceObjects.Parameters;

namespace SpiceSharpParser.Connector.Processors.Controls
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
        protected Export GenerateExport(Parameter parameter, Simulation simulation, IProcessingContext context)
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
                    var componentName = rp.Image.Substring(1, rp.Image.IndexOf('[') - 1);
                    var propertyName = rp.Image.Substring(rp.Image.IndexOf('[') + 1);
                    propertyName = propertyName.Substring(0, propertyName.Length - 1);

                    var parameters = new ParameterCollection();
                    parameters.Add(new WordParameter(componentName));
                    parameters.Add(new WordParameter(propertyName));

                    return Registry.Get(type).CreateExport(type, parameters, simulation, context);
                }
            }

            throw new System.Exception("Unsuported export: " + parameter.Image);
        }
    }
}
