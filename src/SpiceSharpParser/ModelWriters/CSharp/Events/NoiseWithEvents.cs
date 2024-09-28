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

        public IEnumerable<int> RunWithEvents(IEnumerable<int> codes)
        {
            EventBeforeSetup.Invoke(this, EventArgs.Empty);
            foreach (var code in codes)
            {
                switch (code)
                {
                    case Simulation.BeforeValidation:
                        EventBeforeValidation.Invoke(this, EventArgs.Empty);
                        break;

                    case Simulation.AfterValidation:
                        EventAfterValidation.Invoke(this, EventArgs.Empty);
                        break;

                    case Simulation.BeforeSetup:
                        EventBeforeSetup.Invoke(this, EventArgs.Empty);
                        break;
                    case Simulation.AfterSetup:
                        EventAfterSetup.Invoke(this, EventArgs.Empty);
                        break;
                    case Simulation.BeforeUnsetup:
                        EventBeforeUnSetup.Invoke(this, EventArgs.Empty);
                        break;

                    case Simulation.BeforeExecute:
                        EventBeforeExecute.Invoke(this, EventArgs.Empty);

                        if (this is IBiasingSimulation)
                        {
                            EventBeforeTemperature?.Invoke(this, null);
                        }

                        break;

                    case Simulation.AfterExecute:
                        EventAfterExecute.Invoke(this, EventArgs.Empty);
                        break;


                    case Transient.Exports:

                        EventExportData.Invoke(this, new ExportData { }); //TODO });
                        break;
                }
                yield return code;
            }
        }
    }
}
