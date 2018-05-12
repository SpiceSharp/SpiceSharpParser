using System.Linq;
using SpiceSharpParser.Model;
using SpiceSharpParser.Model.SpiceObjects;
using SpiceSharpParser.Model.SpiceObjects.Parameters;

namespace SpiceSharpParser.Preprocessors
{
    public class AppendModelPreProcessor : IAppendModelPreProcessor
    {
        /// <summary>
        /// Processes .appendmodel statements
        /// </summary>
        /// <param name="netlistModel">Netlist model to seach for .appendmodel statements</param>
        public void Process(Netlist netlistModel)
        {
            var subCircuits = netlistModel.Statements.Where(statement => statement is SubCircuit s);
            if (subCircuits.Any())
            {
                foreach (SubCircuit subCircuit in subCircuits.ToArray())
                {
                    var subCircuitAppendModels = subCircuit.Statements.Where(statement => statement is Control c && (c.Name.ToLower() == "appendmodel"));

                    foreach (Control appendModel in subCircuitAppendModels.ToArray())
                    {
                        ProcessAppendModel(subCircuit.Statements, appendModel);
                    }
                }
            }

            var appendModels = netlistModel.Statements.Where(statement => statement is Control c && (c.Name.ToLower() == "appendmodel"));

            if (appendModels.Any())
            {
                foreach (Control appendModel in appendModels.ToArray())
                {
                    ProcessAppendModel(netlistModel.Statements, appendModel);
                }
            }
        }

        private void ProcessAppendModel(Statements statements, Control appendModel)
        {
            string sourceModel = appendModel.Parameters.GetString(0);
            string sourceModelType = appendModel.Parameters.GetString(1); // ignored (for now)
            string destinationModel = appendModel.Parameters.GetString(2);
            string destinationModelType = appendModel.Parameters.GetString(3); // ignored (for now)

            var sourceModelObj = (Model.SpiceObjects.Model)statements.FirstOrDefault(s => s is Model.SpiceObjects.Model m && m.Name == sourceModel);
            var destinationModelObj = (Model.SpiceObjects.Model)statements.FirstOrDefault(s => s is Model.SpiceObjects.Model m && m.Name == destinationModel);

            if (sourceModelObj != null && destinationModelObj != null)
            {
                var parametersToSet = sourceModelObj.Parameters;
                if (parametersToSet[0] is SingleParameter)
                {
                    parametersToSet = parametersToSet.Skip(1); // skip type (1 parameter)
                }

                destinationModelObj.Parameters.Set(parametersToSet);
            }
        }
    }
}
