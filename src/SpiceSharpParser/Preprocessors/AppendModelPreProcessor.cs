using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
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
            // 1. Iterate over all subcircuits
            var subCircuits = netlistModel.Statements.Where(statement => statement is SubCircuit s);
            if (subCircuits.Any())
            {
                foreach (SubCircuit subCircuit in subCircuits)
                {
                    // 2. For each subcircuit find all APPENDMODELs
                    var subCircuitAppendModels = subCircuit.Statements.Where(statement => statement is Control c && (c.Name.ToLower() == "appendmodel"));

                    foreach (Control appendModel in subCircuitAppendModels)
                    {
                        // 3. Process APPENDMODEL
                        ProcessAppendModel(subCircuit.Statements, appendModel, subCircuitAppendModels);
                    }
                }
            }

            // 4. Find all APPENDMODELs from main circuit
            var appendModels = netlistModel.Statements.Where(statement => statement is Control c && (c.Name.ToLower() == "appendmodel"));

            if (appendModels.Any())
            {
                foreach (Control appendModel in appendModels)
                {
                    // 5. Process APPENDMODEL
                    ProcessAppendModel(netlistModel.Statements, appendModel, appendModels);
                }
            }
        }

        /// <summary>
        /// Processes APPENDMODEL statement
        /// </summary>
        /// <param name="statements">Statements to process</param>
        /// <param name="appendModel">Append model statement</param>
        /// <param name="appendModels">Append model statements</param>
        private void ProcessAppendModel(Statements statements, Control appendModel, IEnumerable<Statement> appendModels)
        {
            if (appendModel.Parameters.Count != 4)
            {
                throw new System.Exception("Wrong parameter count for .APPENDMODEL");
            }

            string sourceModel = appendModel.Parameters.GetString(0);
            string sourceModelType = appendModel.Parameters.GetString(1); // ignored (for now)
            string destinationModel = appendModel.Parameters.GetString(2);
            string destinationModelType = appendModel.Parameters.GetString(3);

            var sourceModelObj = (Model.SpiceObjects.Model)statements.FirstOrDefault(s => s is Model.SpiceObjects.Model m && m.Name == sourceModel);

            if (sourceModelObj == null)
            {
                throw new System.Exception("Could not find source model for .APPENDMODEL");
            }

            var parametersToSet = sourceModelObj.Parameters;
            if (parametersToSet[0] is SingleParameter)
            {
                parametersToSet = parametersToSet.Skip(1); // skip 1 parameter - type
            }

            if (destinationModel == "*")
            {
                var destinationModelsObj = statements
                    .Where(s =>
                    s is Model.SpiceObjects.Model m
                    && GetTypeOfModel(m).ToLower() == destinationModelType.ToLower()
                    && m.Name != sourceModel);

                foreach (Model.SpiceObjects.Model model in destinationModelsObj)
                {
                    model.Parameters.Set(parametersToSet);
                }
            }
            else if (destinationModel.Contains("*"))
            {
                string regularExpression = destinationModel.Replace("*", ".*");

                var destinationModelsObj = statements
                    .Where(s =>
                    s is Model.SpiceObjects.Model m
                    && GetTypeOfModel(m).ToLower() == destinationModelType.ToLower()
                    && m.Name != sourceModel
                    && Regex.Match(m.Name, regularExpression).Success);

                foreach (Model.SpiceObjects.Model model in destinationModelsObj)
                {
                    model.Parameters.Set(parametersToSet);
                }
            }
            else
            {
                var destinationModelObj = (Model.SpiceObjects.Model)statements.FirstOrDefault(s => s is Model.SpiceObjects.Model m && m.Name == destinationModel);

                if (destinationModelObj != null)
                {
                    destinationModelObj.Parameters.Set(parametersToSet);
                }
            }
        }

        /// <summary>
        /// Gets the type of the model
        /// </summary>
        /// <param name="model">The model</param>
        /// <returns>
        /// Type of model
        /// </returns>
        private string GetTypeOfModel(Model.SpiceObjects.Model model)
        {
            if (model.Parameters[0] is BracketParameter b)
            {
                return b.Name;
            }

            if (model.Parameters[0] is StringParameter s)
            {
                return s.Image;
            }

            return null;
        }
    }
}
