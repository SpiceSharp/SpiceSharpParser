using System.Linq;
using SpiceSharpParser.Connector.Context;
using SpiceSharpParser.Connector.Processors.Controls.Exporters;
using SpiceSharpParser.Connector.Registries;
using SpiceSharpParser.Model.SpiceObjects;
using SpiceSharpParser.Model.SpiceObjects.Parameters;

namespace SpiceSharpParser.Connector.Processors.Controls
{
    /// <summary>
    /// Processes .SAVE <see cref="Control"/> from spice netlist object model.
    /// </summary>
    public class SaveControl : ExportControl
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SaveControl"/> class.
        /// </summary>
        /// <param name="registry">The exporter registry</param>
        public SaveControl(IExporterRegistry registry) : base(registry)
        {
        }

        /// <summary>
        /// Gets the type of genereator
        /// </summary>
        public override string TypeName => "save";

        /// <summary>
        /// Processes <see cref="Control"/> statement and modifies the context
        /// </summary>
        /// <param name="statement">A statement to process</param>
        /// <param name="context">A context to modify</param>
        public override void Process(Control statement, IProcessingContext context)
        {
            foreach (var parameter in statement.Parameters)
            {
                if (parameter is BracketParameter bracketParameter)
                {
                    context.Result.AddExport(GenerateExport(bracketParameter, context.Result.Simulations.First(), context));
                }
                else if (parameter is ReferenceParameter referenceParameter)
                {
                    context.Result.AddExport(GenerateExport(referenceParameter, context.Result.Simulations.First(), context));
                }
                else if (parameter is SingleParameter s)
                {
                    string expressionName = s.Image;
                    var expressionNames = context.Evaluator.GetExpressionNames();

                    if (expressionNames.Contains(expressionName))
                    {
                        context.Result.AddExport(new ExpressionExport(expressionName, context.Evaluator.GetExpression(expressionName), context.Evaluator));
                    }
                }
            }
        }
    }
}
