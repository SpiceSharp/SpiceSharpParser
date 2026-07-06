using System;
using SpiceSharp.Components;
using SpiceSharp.Entities;
using SpiceSharp.ParameterSets;
using SpiceSharp.Simulations;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.Waveforms
{
    internal sealed class FiniteSine : ParameterSet<IWaveformDescription>, IWaveformDescription
    {
        public double Offset { get; set; }

        public double Amplitude { get; set; }

        public double Frequency { get; set; }

        public double Delay { get; set; }

        public double Theta { get; set; }

        public double Phase { get; set; }

        public double CycleCount { get; set; }

        public IWaveform Create(IBindingContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            var sine = new Sine
            {
                Offset = Offset,
                Amplitude = Amplitude,
                Frequency = Frequency,
                Delay = Delay,
                Theta = Theta,
                Phase = Phase,
            };

            return new Instance(
                context.GetState<IIntegrationMethod>(),
                sine.Create(context),
                Offset,
                Delay,
                Frequency,
                CycleCount);
        }

        private sealed class Instance : IWaveform
        {
            private readonly IIntegrationMethod _method;
            private readonly IWaveform _inner;
            private readonly double _offset;
            private readonly double _stopTime;

            public Instance(
                IIntegrationMethod method,
                IWaveform inner,
                double offset,
                double delay,
                double frequency,
                double cycleCount)
            {
                _method = method ?? throw new ArgumentNullException(nameof(method));
                _inner = inner ?? throw new ArgumentNullException(nameof(inner));
                _offset = offset;
                _stopTime = delay + (cycleCount / frequency);
                Value = offset;
            }

            public double Value { get; private set; }

            public void Probe()
            {
                if (_method.Time >= _stopTime)
                {
                    Value = _offset;
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
