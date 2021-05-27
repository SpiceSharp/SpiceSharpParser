using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using SpiceSharpParser.Common.Validation;
using SpiceSharpParser.Models.Netlist.Spice.Objects;
using SpiceSharpParser.Models.Netlist.Spice.Objects.Parameters;

namespace SpiceSharpParser.Common.Processors
{
    public class AppendModelProcessor : IProcessor
    {
        public ValidationEntryCollection Validation { get; set; }

        /// <summary>
        /// Preprocess .APPENDMODEL statements.
        /// </summary>
        /// <param name="statements">Statements to process.</param>
        public Statements Process(Statements statements)
        {
            if (statements == null)
            {
                throw new ArgumentNullException(nameof(statements));
            }

            // 1. Iterate over all subcircuits
            var subCircuits = statements.Where(statement => statement is SubCircuit).Cast<SubCircuit>().ToList();
            if (subCircuits.Any())
            {
                foreach (SubCircuit subCircuit in subCircuits)
                {
                    Process(subCircuit.Statements);
                }
            }

            // 4. Find all APPENDMODELs from main circuit
            var appendModels = statements
                .Where(statement => statement is Control c && (c.Name.ToLower() == "appendmodel"))
                .Cast<Control>().ToList();

            if (appendModels.Any())
            {
                foreach (Control appendModel in appendModels)
                {
                    // 5. Read APPENDMODEL
                    ReadAppendModel(statements, appendModel);
                }
            }

            return statements;
        }

        /// <summary>
        /// Reads .APPENDMODEL statement.
        /// </summary>
        /// <param name="statements">Statements to process.</param>
        /// <param name="appendModel">Append model statement.</param>
        private void ReadAppendModel(Statements statements, Control appendModel)
        {
            if (appendModel.Parameters.Count != 4 && appendModel.Parameters.Count != 2)
            {
                throw new SpiceSharpParserException("Wrong parameter count for .APPENDMODEL");
            }

            if (appendModel.Parameters.Count == 4)
            {
                ReadAppendModelWithFourParameters(statements, appendModel);
            }
            else
            {
                ReadAppendModelWithTwoParameters(statements, appendModel);
            }
        }

        private void ReadAppendModelWithTwoParameters(Statements statements, Control appendModel)
        {
            string sourceModel = appendModel.Parameters.Get(0).Value;
            var sourceModelObj = (Model)statements.FirstOrDefault(s => s is Model m && m.Name == sourceModel);
            if (sourceModelObj == null)
            {
                throw new SpiceSharpParserException("Could not find source model for .APPENDMODEL");
            }

            string destinationModel = appendModel.Parameters.Get(1).Value;
            if (destinationModel == "*")
            {
                var destinationModelsObj = statements
                    .Where(s =>
                        s is Model m
                        && m.Name != sourceModel).Cast<Model>();

                AppendParametersToModel(destinationModelsObj, sourceModelObj.Parameters);
            }
            else
            {
                var destinationModelObj = (Model)statements
                    .FirstOrDefault(s => s is Model m && m.Name == destinationModel);

                if (destinationModelObj == null)
                {
                    throw new SpiceSharpParserException("Could not find destination model for .APPENDMODEL");
                }

                destinationModelObj.Parameters.Set(sourceModelObj.Parameters);
            }
        }

        private void ReadAppendModelWithFourParameters(Statements statements, Control appendModel)
        {
            string sourceModel = appendModel.Parameters.Get(0).Value;
            string destinationModel = appendModel.Parameters.Get(2).Value;
            string destinationModelType = appendModel.Parameters.Get(3).Value;

            var sourceModelObj = (Model)statements.FirstOrDefault(s => s is Model m && m.Name == sourceModel);

            if (sourceModelObj == null)
            {
                throw new SpiceSharpParserException("Could not find source model for .APPENDMODEL");
            }

            var parametersToSet = sourceModelObj.Parameters;
            if (parametersToSet[0] is SingleParameter)
            {
                parametersToSet = parametersToSet.Skip(1); // skip 1 parameter - type
            }

            if (destinationModel == "*")
            {
                IEnumerable<Model> destinationModelsObj = GetModelsOfType(statements, sourceModel, destinationModelType);
                AppendParametersToModel(destinationModelsObj, parametersToSet);
            }
            else if (destinationModel.Contains("*"))
            {
                string regularExpression = destinationModel.Replace("*", ".*");

                IEnumerable<Model> destinationModelsObj = GetModelsRegex(statements, sourceModel, destinationModelType, regularExpression);
                AppendParametersToModel(destinationModelsObj, parametersToSet);
            }
            else
            {
                var destinationModelObj = (Model)statements.FirstOrDefault(s => s is Model m && m.Name == destinationModel);

                destinationModelObj?.Parameters.Set(parametersToSet);
            }
        }

        /// <summary>
        /// Gets the models with name matching regex and with different name than source model.
        /// </summary>
        private IEnumerable<Model> GetModelsRegex(Statements statements, string sourceModelName, string destinationModelType, string regularExpression)
        {
            return statements
                .Where(s =>
                    s is Model m
                    && GetTypeOfModel(m).ToLower() == destinationModelType.ToLower()
                    && m.Name != sourceModelName
                    && Regex.Match(m.Name, regularExpression).Success).Cast<Model>();
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
        private IEnumerable<Model> GetModelsOfType(Statements statements, string sourceModelName, string modelType)
        {
            return statements
                .Where(s =>
                    s is Model m
                    && GetTypeOfModel(m).ToLower() == modelType.ToLower()
                    && m.Name != sourceModelName).Cast<Model>();
        }

        /// <summary>
        /// Appends parameters to models.
        /// </summary>
        /// <param name="models">A enumerable of models.</param>
        /// <param name="parametersToSet">Parameters to set.</param>
        private void AppendParametersToModel(IEnumerable<Model> models, ParameterCollection parametersToSet)
        {
            foreach (Model model in models)
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
        private string GetTypeOfModel(Model model)
        {
            if (model.Parameters[0] is BracketParameter b)
            {
                return b.Name;
            }

            if (model.Parameters[0] is StringParameter s)
            {
                return s.Value;
            }

            return null;
        }
    }
}