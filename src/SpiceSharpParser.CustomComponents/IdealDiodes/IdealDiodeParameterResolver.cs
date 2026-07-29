using SpiceSharp.Simulations;

namespace SpiceSharpParser.CustomComponents.IdealDiodes
{
    /// <summary>
    /// Combines model defaults, stepped model values, and explicit instance values.
    /// </summary>
    internal sealed class IdealDiodeParameterResolver
    {
        private readonly IdealDiode _diode;
        private readonly ISimulation _simulation;
        private readonly IdealDiodeParameters _instanceParameters;
        private readonly IdealDiodeParameters _fallbackModelParameters;

        public IdealDiodeParameterResolver(
            IdealDiode diode,
            ISimulation simulation,
            IdealDiodeParameters instanceParameters,
            IdealDiodeParameters fallbackModelParameters)
        {
            _diode = diode;
            _simulation = simulation;
            _instanceParameters = instanceParameters;
            _fallbackModelParameters = fallbackModelParameters;
        }

        public IdealDiodeParameters Resolve()
        {
            var effective = new IdealDiodeParameters();
            IdealDiodeParameters model =
                _diode?.GetModelParameters(_simulation) ?? _fallbackModelParameters;

            if (model == null)
            {
                _instanceParameters.CopyTo(effective);
                return effective;
            }

            model.CopyTo(effective);
            ApplySteppedModelParameters(effective);
            ApplyInstanceParameters(effective);
            return effective;
        }

        private void ApplySteppedModelParameters(IdealDiodeParameters effective)
        {
            if (_diode == null)
            {
                return;
            }

            foreach (var parameter in _diode.GetModelParameterOverrides(_simulation))
            {
                effective.SetParameter(parameter.Key, parameter.Value);
            }
        }

        private void ApplyInstanceParameters(IdealDiodeParameters effective)
        {
            // These are always instance parameters, even when their values were not
            // explicitly written in the netlist.
            effective.Area = _instanceParameters.Area;
            effective.Off = _instanceParameters.Off;
            effective.ParallelMultiplier = _instanceParameters.ParallelMultiplier;
            effective.SeriesMultiplier = _instanceParameters.SeriesMultiplier;

            CopyOverride("rs", () => effective.Resistance = _instanceParameters.Resistance);
            CopyOverride("ron", () => effective.OnResistance = _instanceParameters.OnResistance);
            CopyOverride("roff", () => effective.OffResistance = _instanceParameters.OffResistance);
            CopyOverride("vfwd", () => effective.ForwardVoltage = _instanceParameters.ForwardVoltage);
            CopyOverride("vrev", () => effective.ReverseVoltage = _instanceParameters.ReverseVoltage);
            CopyOverride("rrev", () => effective.ReverseResistance = _instanceParameters.ReverseResistance);
            CopyOverride("ilimit", () => effective.ForwardCurrentLimit = _instanceParameters.ForwardCurrentLimit);
            CopyOverride("revilimit", () => effective.ReverseCurrentLimit = _instanceParameters.ReverseCurrentLimit);
            CopyOverride("epsilon", () => effective.ForwardEpsilon = _instanceParameters.ForwardEpsilon);
            CopyOverride("revepsilon", () => effective.ReverseEpsilon = _instanceParameters.ReverseEpsilon);
        }

        private void CopyOverride(string parameterName, System.Action copy)
        {
            if (_instanceParameters.HasInstanceOverride(parameterName))
            {
                copy();
            }
        }
    }
}
