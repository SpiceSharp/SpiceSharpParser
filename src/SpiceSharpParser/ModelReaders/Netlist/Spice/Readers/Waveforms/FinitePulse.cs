using System;
using SpiceSharp.Components;
using SpiceSharp.Entities;
using SpiceSharp.ParameterSets;
using SpiceSharp.Simulations;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.Waveforms
{
    internal sealed class FinitePulse : ParameterSet<IWaveformDescription>, IWaveformDescription
    {
        public double InitialValue { get; set; }

        public double PulsedValue { get; set; }

        public double Delay { get; set; }

        public double RiseTime { get; set; }

        public double FallTime { get; set; }

        public double PulseWidth { get; set; }

        public double Period { get; set; }

        public double CycleCount { get; set; }

        public IWaveform Create(IBindingContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            var pulse = new Pulse
            {
                InitialValue = InitialValue,
                PulsedValue = PulsedValue,
                Delay = Delay,
                RiseTime = RiseTime,
                FallTime = FallTime,
                PulseWidth = PulseWidth,
                Period = Period,
            };

            return new Instance(
                context.GetState<IIntegrationMethod>(),
                pulse.Create(context),
                InitialValue,
                Delay,
                Period,
                CycleCount);
        }

        private sealed class Instance : IWaveform
        {
            private readonly IIntegrationMethod _method;
            private readonly IWaveform _inner;
            private readonly double _initialValue;
            private readonly double _stopTime;

            public Instance(
                IIntegrationMethod method,
                IWaveform inner,
                double initialValue,
                double delay,
                double period,
                double cycleCount)
            {
                _method = method ?? throw new ArgumentNullException(nameof(method));
                _inner = inner ?? throw new ArgumentNullException(nameof(inner));
                _initialValue = initialValue;
                _stopTime = delay + (period * cycleCount);
                Value = initialValue;
            }

            public double Value { get; private set; }

            public void Probe()
            {
                if (_method.Time >= _stopTime)
                {
                    Value = _initialValue;
                    return;
                }

                _inner.Probe();
                Value = _inner.Value;
            }

            public void Accept()
            {
                _inner.Accept();
            }
        }
    }
}
