using SpiceSharp.Simulations;
using System;
using System.Collections.Generic;

namespace SpiceSharpParser.Common
{
    public class TransientWithEvents : Transient, ISimulationWithEvents
    {
        public TransientWithEvents(string name) : base(name)
        {
        }

        public TransientWithEvents(string name, TimeParameters parameters) : base(name, parameters)
        {

        }
        public TransientWithEvents(string name, double step, double final)
            : base(name, step, final)
        {
        }

        //
        // Summary:
        //     Initializes a new instance of the SpiceSharp.Simulations.Transient class.
        //
        // Parameters:
        //   name:
        //     The name of the simulation.
        //
        //   step:
        //     The step size.
        //
        //   final:
        //     The final time.
        //
        //   maxStep:
        //     The maximum step.
        public TransientWithEvents(string name, double step, double final, double maxStep)
            : base(name, step, final, maxStep)
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
            EventBeforeTemperature?.Invoke(this, null);

            foreach (var code in codes)
            {
                switch (code)
                {
                    case Transient.BeforeValidation:
                        EventBeforeValidation?.Invoke(this, EventArgs.Empty);
                        break;

                    case Transient.AfterValidation:
                        EventAfterValidation?.Invoke(this, EventArgs.Empty);
                        break;

                    case Transient.BeforeSetup:
                        EventBeforeSetup?.Invoke(this, EventArgs.Empty);
                        break;
                    case Transient.AfterSetup:
                        EventAfterSetup?.Invoke(this, EventArgs.Empty);
                        break;
                    case Transient.BeforeUnsetup:
                        EventBeforeUnSetup?.Invoke(this, EventArgs.Empty);
                        break;

                    case Simulation.BeforeExecute:
                        EventBeforeExecute?.Invoke(this, EventArgs.Empty);
                        break;

                    case Simulation.AfterExecute:
                        EventAfterExecute?.Invoke(this, EventArgs.Empty);
                        break;


                    case Transient.ExportTransient:

                        EventExportData?.Invoke(this, new ExportData { Time = this.Time }); //TODO });
                        break;
                }
                yield return code;
            }

            EventAfterExecute?.Invoke(this, EventArgs.Empty);
        }
    }
}
