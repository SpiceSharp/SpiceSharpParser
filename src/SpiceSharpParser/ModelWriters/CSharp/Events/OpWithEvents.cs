using SpiceSharp.Simulations;
using System;
using System.Collections.Generic;

namespace SpiceSharpParser.Common
{
    public class OpWithEvents : OP, ISimulationWithEvents
    {
        public OpWithEvents(string name) : base(name)
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
            foreach (var code in codes)
            {
                switch (code)
                {
                    case OP.AfterExecute:
                        EventAfterExecute?.Invoke(this, EventArgs.Empty);
                        break;

                    case OP.BeforeExecute:
                        EventBeforeExecute?.Invoke(this, EventArgs.Empty);
                        break;

                    case OP.BeforeValidation:
                        EventBeforeValidation?.Invoke(this, EventArgs.Empty);
                        break;

                    case OP.AfterValidation:
                        EventAfterValidation?.Invoke(this, EventArgs.Empty);
                        break;
                    case OP.BeforeSetup:
                        EventBeforeSetup?.Invoke(this, EventArgs.Empty);
                        break;
                    case OP.AfterSetup:
                        EventAfterSetup?.Invoke(this, EventArgs.Empty);
                        break;
                    case OP.BeforeTemperature:
                        var state = this.GetState<ITemperatureSimulationState>();
                        EventBeforeTemperature?.Invoke(this, new TemperatureStateEventArgs(state));
                        break;
                    case OP.AfterTemperature:
                        EventAfterTemperature?.Invoke(this, EventArgs.Empty);
                        break;
                    case OP.BeforeUnsetup:
                        EventBeforeUnSetup?.Invoke(this, EventArgs.Empty);
                        break;
                    case OP.ExportOperatingPoint:
                        EventExportData?.Invoke(this, new ExportData() { });
                        break;
                }
                yield return code;
            }
        }
    }
}
