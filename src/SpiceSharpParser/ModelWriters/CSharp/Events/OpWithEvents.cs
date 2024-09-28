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
            EventBeforeSetup?.Invoke(this, EventArgs.Empty);

            foreach (var code in codes)
            {
                switch (code)
                {
                    case OP.ExportOperatingPoint:

                        EventExportData?.Invoke(this, new ExportData() { });
                        break;
                }
                yield return code;
            }

            EventAfterExecute?.Invoke(this, EventArgs.Empty);
        }
    }
}
