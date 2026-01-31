using SpiceSharp.Entities;
using SpiceSharp.ParameterSets;
using System.Collections.Generic;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Context.Models
{
    public class Model
    {
        private readonly Dictionary<string, double> _dimensionParameters = new Dictionary<string, double>(System.StringComparer.OrdinalIgnoreCase);

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
        /// Sets a dimension parameter (lmin, lmax, wmin, wmax) for model selection.
        /// </summary>
        /// <param name="parameterName">The parameter name.</param>
        /// <param name="value">The parameter value.</param>
        public void SetDimensionParameter(string parameterName, double value)
        {
            _dimensionParameters[parameterName] = value;
        }

        /// <summary>
        /// Gets a dimension parameter (lmin, lmax, wmin, wmax) for model selection.
        /// </summary>
        /// <param name="parameterName">The parameter name.</param>
        /// <param name="value">The parameter value.</param>
        /// <returns>True if the parameter exists.</returns>
        public bool TryGetDimensionParameter(string parameterName, out double value)
        {
            return _dimensionParameters.TryGetValue(parameterName, out value);
        }
    }
}
