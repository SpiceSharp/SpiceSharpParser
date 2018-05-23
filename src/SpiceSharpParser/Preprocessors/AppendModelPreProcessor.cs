using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using SpiceSharpParser.Model.Spice;
using SpiceSharpParser.Model.Spice.Objects;
using SpiceSharpParser.Model.Spice.Objects.Parameters;

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
            if (appendModel.Parameters.Count != 4 && appendModel.Parameters.Count != 2)
            {
                throw new System.Exception("Wrong parameter count for .APPENDMODEL");
            }

            if (appendModel.Parameters.Count == 4)
            {
                ProcessAppendModelWithFourParameters(statements, appendModel);
            }
            else
            {
                ProcessAppendModelWithTwoParameters(statements, appendModel);
            }
        }

        private void ProcessAppendModelWithTwoParameters(Statements statements, Control appendModel)
        {
            string sourceModel = appendModel.Parameters.GetString(0);
            var sourceModelObj = (Model.Spice.Objects.Model)statements.FirstOrDefault(s => s is Model.Spice.Objects.Model m && m.Name == sourceModel);
            if (sourceModelObj == null)
            {
                throw new System.Exception("Could not find source model for .APPENDMODEL");
            }

            string destinationModel = appendModel.Parameters.GetString(1);
            if (destinationModel == "*")
            {
                var destinationModelsObj = statements
                   .Where(s =>
                   s is Model.Spice.Objects.Model m
                   && m.Name != sourceModel);

                AppendParametersToModel(destinationModelsObj, sourceModelObj.Parameters);
            }
            else
            {
                var destinationModelObj = (Model.Spice.Objects.Model)statements
                    .FirstOrDefault(s => s is Model.Spice.Objects.Model m && m.Name == destinationModel);

                if (destinationModelObj == null)
                {
                    throw new System.Exception("Could not find destination model for .APPENDMODEL");
                }

                destinationModelObj.Parameters.Set(sourceModelObj.Parameters);
            }
        }

        private void ProcessAppendModelWithFourParameters(Statements statements, Control appendModel)
        {
            string sourceModel = appendModel.Parameters.GetString(0);
            string sourceModelType = appendModel.Parameters.GetString(1); // ignored (for now)
            string destinationModel = appendModel.Parameters.GetString(2);
            string destinationModelType = appendModel.Parameters.GetString(3);

            var sourceModelObj = (Model.Spice.Objects.Model)statements.FirstOrDefault(s => s is Model.Spice.Objects.Model m && m.Name == sourceModel);

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
                IEnumerable<Statement> destinationModelsObj = GetModelsOfType(statements, sourceModel, destinationModelType);
                AppendParametersToModel(destinationModelsObj, parametersToSet);
            }
            else if (destinationModel.Contains("*"))
            {
                string regularExpression = destinationModel.Replace("*", ".*");

                IEnumerable<Statement> destinationModelsObj = GetModelsRegex(statements, sourceModel, destinationModelType, regularExpression);
                AppendParametersToModel(destinationModelsObj, parametersToSet);
            }
            else
            {
                var destinationModelObj = (Model.Spice.Objects.Model)statements.FirstOrDefault(s => s is Model.Spice.Objects.Model m && m.Name == destinationModel);

                if (destinationModelObj != null)
                {
                    destinationModelObj.Parameters.Set(parametersToSet);
                }
            }
        }

        /// <summary>
        /// Gets the models with name matching regex and with different name than source model.
        /// </summary>
        private IEnumerable<Statement> GetModelsRegex(Statements statements, string sourceModelName, string destinationModelType, string regularExpression)
        {
            return statements
                .Where(s =>
                s is Model.Spice.Objects.Model m
                && GetTypeOfModel(m).ToLower() == destinationModelType.ToLower()
                && m.Name != sourceModelName
                && Regex.Match(m.Name, regularExpression).Success);
        }

        /// <summary>
        /// Gets the models with given type and with different name than source model.
        /// </summary>
        /// <param name="statements">The statements.</param>
        /// <param name="sourceModelName">The name of source model.</param>
        /// <param name="modelType">The type of model to search.</param>
        /// <returns>
        /// Enumerable of models.
        /// </returns>
        private IEnumerable<Statement> GetModelsOfType(Statements statements, string sourceModelName, string modelType)
        {
            return statements
            .Where(s =>
                s is Model.Spice.Objects.Model m
                && GetTypeOfModel(m).ToLower() == modelType.ToLower()
                && m.Name != sourceModelName);
        }

        /// <summary>
        /// Appends paramters to models.
        /// </summary>
        /// <param name="models">A enumerable of models.</param>
        /// <param name="parametersToSet">Parameters to set.</param>
        private void AppendParametersToModel(IEnumerable<Statement> models, ParameterCollection parametersToSet)
        {
            foreach (Model.Spice.Objects.Model model in models)
            {
                model.Parameters.Set(parametersToSet);
            }
        }

        /// <summary>
        /// Gets the type of the model.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <returns>
        /// Type of model.
        /// </returns>
        private string GetTypeOfModel(Model.Spice.Objects.Model model)
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
