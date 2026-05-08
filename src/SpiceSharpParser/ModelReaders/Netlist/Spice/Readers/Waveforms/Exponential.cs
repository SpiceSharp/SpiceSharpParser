using System;
using SpiceSharp.Components;
using SpiceSharp.Entities;
using SpiceSharp.ParameterSets;
using SpiceSharp.Simulations;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.Waveforms
{
    internal sealed class Exponential : ParameterSet<IWaveformDescription>, IWaveformDescription
    {
        public double InitialValue { get; set; }

        public double PulsedValue { get; set; }

        public double RiseDelay { get; set; }

        public double RiseTimeConstant { get; set; }

        public double FallDelay { get; set; }

        public double FallTimeConstant { get; set; }

        public IWaveform Create(IBindingContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            return new Instance(
                context.GetState<IIntegrationMethod>(),
                InitialValue,
                PulsedValue,
                RiseDelay,
                RiseTimeConstant,
                FallDelay,
                FallTimeConstant);
        }

        private sealed class Instance : IWaveform
        {
            private readonly IIntegrationMethod _method;
            private readonly double _initialValue;
            private readonly double _pulsedValue;
            private readonly double _riseDelay;
            private readonly double _riseTimeConstant;
            private readonly double _fallDelay;
            private readonly double _fallTimeConstant;

            public Instance(
                IIntegrationMethod method,
                double initialValue,
                double pulsedValue,
                double riseDelay,
                double riseTimeConstant,
                double fallDelay,
                double fallTimeConstant)
            {
                _method = method ?? throw new ArgumentNullException(nameof(method));
                _initialValue = initialValue;
                _pulsedValue = pulsedValue;
                _riseDelay = riseDelay;
                _riseTimeConstant = riseTimeConstant;
                _fallDelay = fallDelay;
                _fallTimeConstant = fallTimeConstant;
                Value = initialValue;
            }

            public double Value { get; private set; }

            public void Probe()
            {
                Value = At(_method.Time);
            }

            public void Accept()
            {
            }

            private double At(double time)
            {
                if (time <= _riseDelay)
                {
                    return _initialValue;
                }

                var value = _initialValue
                    + (_pulsedValue - _initialValue)
                    * (1.0 - Math.Exp(-(time - _riseDelay) / _riseTimeConstant));

                if (time > _fallDelay)
                {
                    value += (_initialValue - _pulsedValue)
                        * (1.0 - Math.Exp(-(time - _fallDelay) / _fallTimeConstant));
                }

                return value;
            }
        }
    }
}
