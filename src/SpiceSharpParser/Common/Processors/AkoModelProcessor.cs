using System;
using System.Linq;
using SpiceSharpParser.Common.Validation;
using SpiceSharpParser.Models.Netlist.Spice.Objects;
using SpiceSharpParser.Models.Netlist.Spice.Objects.Parameters;

namespace SpiceSharpParser.Common.Processors
{
    public class AkoModelProcessor : IProcessor
    {
        public ValidationEntryCollection Validation { get; set; }

        public Statements Process(Statements statements)
        {
            if (statements == null)
            {
                throw new ArgumentNullException(nameof(statements));
            }

            var subCircuits = statements.Where(statement => statement is SubCircuit).Cast<SubCircuit>().ToList();
            if (subCircuits.Any())
            {
                foreach (SubCircuit subCircuit in subCircuits)
                {
                    Process(subCircuit.Statements);
                }
            }

            var akoModels = statements
                .Where(statement => statement is Model m && (m.Parameters[0] is WordParameter wp && wp.Value.ToLower().StartsWith("ako")))
                .Cast<Model>().ToList();

            if (akoModels.Any())
            {
                foreach (Model appendModel in akoModels)
                {
                    ReplaceAko(statements, appendModel);
                }
            }

            return statements;
        }

        private void ReplaceAko(Statements statements, Model akoModel)
        {
            var sourceModelName = akoModel.Parameters[0].Value.Substring(4);
            var sourceModel = (Model)statements.FirstOrDefault(s => s is Model m && m.Name == sourceModelName);

            if (sourceModel != null)
            {
                AppendParametersToModel(akoModel, sourceModel.Parameters);
            }

            akoModel.Parameters.RemoveAt(0);
        }

        /// <summary>
        /// Appends parameters to models.
        /// </summary>
        /// <param name="model">A model.</param>
        /// <param name="parametersToSet">Parameters to set.</param>
        private void AppendParametersToModel(Model model, ParameterCollection parametersToSet)
        {
            model.Parameters.Set(parametersToSet);
        }
    }
}