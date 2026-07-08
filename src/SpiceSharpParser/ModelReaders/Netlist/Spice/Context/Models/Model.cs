using SpiceSharp.Entities;
using SpiceSharp.ParameterSets;
using SpiceSharpParser.Models.Netlist.Spice.Objects.Parameters;
using System.Collections.Generic;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Context.Models
{
    public class Model
    {
        private readonly Dictionary<string, double> _selectionParameters = new Dictionary<string, double>(System.StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, AssignmentParameter> _rawParameters = new Dictionary<string, AssignmentParameter>(System.StringComparer.OrdinalIgnoreCase);

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
        /// Gets all raw assignment parameters as a read-only dictionary.
        /// </summary>
        public IReadOnlyDictionary<string, AssignmentParameter> RawParameters => _rawParameters;

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
        /// Stores the raw assignment parameter for parser-level compatibility features that
        /// need the original expression rather than the eagerly evaluated numeric value.
        /// </summary>
        /// <param name="parameterName">The parameter name.</param>
        /// <param name="parameter">The raw assignment parameter.</param>
        public void SetRawParameter(string parameterName, AssignmentParameter parameter)
        {
            _rawParameters[parameterName] = parameter;
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

        /// <summary>
        /// Gets a raw assignment parameter.
        /// </summary>
        /// <param name="parameterName">The parameter name.</param>
        /// <param name="parameter">The raw assignment parameter.</param>
        /// <returns>True if the parameter exists.</returns>
        public bool TryGetRawParameter(string parameterName, out AssignmentParameter parameter)
        {
            return _rawParameters.TryGetValue(parameterName, out parameter);
        }
    }
}
