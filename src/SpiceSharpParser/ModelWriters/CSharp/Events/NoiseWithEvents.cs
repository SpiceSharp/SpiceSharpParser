using SpiceSharp.Simulations;
using System;
using System.Collections.Generic;

namespace SpiceSharpParser.Common
{
    public class NoiseWithEvents : Noise, ISimulationWithEvents
    {
        public NoiseWithEvents(string name) : base(name)
        {
        }

        public NoiseWithEvents(string name, string input, string output, IEnumerable<double> frequencySweep) : base(name, input, output, frequencySweep)
        {
        }

        public NoiseWithEvents(string name, string input, string output, string reference, IEnumerable<double> frequencySweep) : base(name, input, output, reference, frequencySweep)
        {
        }

        public event OnBeforeSetup EventBeforeSetup;

        public event OnBeforeSetup EventBeforeUnSetup;

        public event OnAfterSetup EventAfterSetup;

        public event OnBeforeValidation EventBeforeValidation;

        public event OnBeforeValidation EventAfterValidation;

        public event OnBeforeTemperature EventBeforeTemperature;

        public event OnAfterTemperature EventAfterTemperature;

        public event OnBeforeExecute EventBeforeExecute;

        public event OnAfterExecute EventAfterExecute;

        public event OnExportData EventExportData;

        public IEnumerable<int> InvokeEvents(IEnumerable<int> codes)
        {
            foreach (var code in codes)
            {
                switch (code)
                {
                    case Noise.BeforeValidation:
                        EventBeforeValidation?.Invoke(this, EventArgs.Empty);
                        break;

                    case Noise.AfterValidation:
                        EventAfterValidation?.Invoke(this, EventArgs.Empty);
                        break;

                    case Noise.BeforeSetup:
                        EventBeforeSetup?.Invoke(this, EventArgs.Empty);
                        break;
                    case Noise.AfterSetup:
                        EventAfterSetup?.Invoke(this, EventArgs.Empty);
                        break;
                    case Noise.BeforeUnsetup:
                        EventBeforeUnSetup?.Invoke(this, EventArgs.Empty);
                        break;

                    case Noise.BeforeExecute:
                        EventBeforeExecute?.Invoke(this, EventArgs.Empty);

                        if (this is IBiasingSimulation)
                        {
                            EventBeforeTemperature?.Invoke(this, null);
                        }

                        break;

                    case Noise.AfterExecute:
                        EventAfterExecute?.Invoke(this, EventArgs.Empty);
                        break;


                    case Noise.ExportNoise:

                        EventExportData?.Invoke(this, EventArgs.Empty);
                        break;
                }
                yield return code;
            }
        }
    }
}
