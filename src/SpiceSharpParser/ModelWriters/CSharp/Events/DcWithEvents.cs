using SpiceSharp;
using SpiceSharp.Simulations;
using System;
using System.Collections.Generic;

namespace SpiceSharpParser.Common
{
    public class DcWithEvents : DC, ISimulationWithEvents
    {
        public DcWithEvents(string name) : base(name)
        {
        }

        public DcWithEvents(string name, IEnumerable<ISweep> sweeps) : this(name)
        {
            sweeps.ThrowIfNull("sweeps");
            foreach (ISweep sweep in sweeps)
            {
                DCParameters.Sweeps.Add(sweep);
            }
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
            foreach (var code in codes)
            {
                switch (code)
                {
                    case DC.BeforeValidation:
                        EventBeforeValidation?.Invoke(this, EventArgs.Empty);
                        break;

                    case DC.AfterValidation:
                        EventAfterValidation?.Invoke(this, EventArgs.Empty);
                        break;

                    case 65536:
                        EventBeforeSetup?.Invoke(this, EventArgs.Empty);
                        break;
                    case DC.AfterSetup:
                        EventAfterSetup?.Invoke(this, EventArgs.Empty);
                        break;
                    case DC.BeforeUnsetup:
                        EventBeforeUnSetup?.Invoke(this, EventArgs.Empty);
                        break;

                    case DC.BeforeExecute:
                        EventBeforeExecute?.Invoke(this, EventArgs.Empty);

                        if (this is IBiasingSimulation)
                        {
                            EventBeforeTemperature?.Invoke(this, null);
                        }

                        break;

                    case DC.AfterExecute:
                        EventAfterExecute?.Invoke(this, EventArgs.Empty);
                        break;

                    case DC.ExportSweep:
                        EventExportData?.Invoke(this, new ExportData { });
                        break;
                }
                yield return code;
            }
        }
    }
}
