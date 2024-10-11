using SpiceSharp.Simulations;
using System;
using System.Collections.Generic;

namespace SpiceSharpParser.Common
{
    public class ACWithEvents : AC, ISimulationWithEvents
    {
        protected ACWithEvents(string name) : base(name)
        {
        }

        public ACWithEvents(string name, IEnumerable<double> frequencySweep): base(name, frequencySweep)
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

        public IEnumerable<int> AttachEvents(IEnumerable<int> codes)
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


                    case AC.Exports:

                        double frequency = base.Frequency;
                        EventExportData.Invoke(this, new ExportData { Frequency = frequency });
                        break;
                }
                yield return code;
            }
        }
    }
}
