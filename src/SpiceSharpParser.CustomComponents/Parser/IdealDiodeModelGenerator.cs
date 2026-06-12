using SpiceSharpParser.ModelReaders.Netlist.Spice.Context;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.EntityGenerators.Models;
using SpiceSharpParser.Models.Netlist.Spice.Objects;
using Model = SpiceSharpParser.ModelReaders.Netlist.Spice.Context.Models.Model;

namespace SpiceSharpParser.CustomComponents
{
    /// <summary>
    /// Creates a custom ideal diode model when LTspice ideal-diode parameters are present.
    /// </summary>
    public class IdealDiodeModelGenerator : DiodeModelGenerator
    {
        /// <inheritdoc />
        public override Model Generate(string id, string type, ParameterCollection parameters, IReadingContext context)
        {
            if (!IdealDiodeParserSupport.HasIdealParameter(parameters))
            {
                return base.Generate(id, type, parameters, context);
            }

            var model = new IdealDiodeModel(id);
            var contextModel = new Model(id, model, model.Parameters);
            IdealDiodeParserSupport.SetModelParameters(context, model, contextModel, parameters);

            return contextModel;
        }
    }
}
