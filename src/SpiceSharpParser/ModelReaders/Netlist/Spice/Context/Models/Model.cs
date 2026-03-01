using SpiceSharp.Entities;
using SpiceSharp.ParameterSets;
using System.Collections.Generic;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Context.Models
{
    public class Model
    {
        private readonly Dictionary<string, double> _selectionParameters = new Dictionary<string, double>(System.StringComparer.OrdinalIgnoreCase);

        public Model(string name, IEntity entity, IParameterSet parameters)
        {
            Name = name;
            Entity = entity;
            Parameters = parameters;
        }

        public string Name { get; }

        public IEntity Entity { get; }

        public IParameterSet Parameters { get; }

        /// <summary>
        /// Gets all selection parameters as a read-only dictionary.
        /// </summary>
        public IReadOnlyDictionary<string, double> SelectionParameters => _selectionParameters;

        /// <summary>
        /// Sets a selection parameter for model matching.
        /// </summary>
        /// <param name="parameterName">The parameter name.</param>
        /// <param name="value">The parameter value.</param>
        public void SetSelectionParameter(string parameterName, double value)
        {
            _selectionParameters[parameterName] = value;
        }

        /// <summary>
        /// Gets a selection parameter for model matching.
        /// </summary>
        /// <param name="parameterName">The parameter name.</param>
        /// <param name="value">The parameter value.</param>
        /// <returns>True if the parameter exists.</returns>
        public bool TryGetSelectionParameter(string parameterName, out double value)
        {
            return _selectionParameters.TryGetValue(parameterName, out value);
        }
    }
}
