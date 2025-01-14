﻿using SpiceSharp.Simulations;
using System.Collections.Generic;

namespace SpiceSharpParser.Common
{
    public delegate void OnBeforeSetup(object sender, object argument);

    public delegate void OnBeforeUnSetup(object sender, object argument);

    public delegate void OnAfterSetup(object sender, object argument);

    public delegate void OnBeforeValidation(object sender, object argument);

    public delegate void OnBeforeTemperature(object sender, TemperatureStateEventArgs argument);

    public delegate void OnAfterTemperature(object sender, object argument);

    public delegate void OnBeforeExecute(object sender, object argument);

    public delegate void OnAfterExecute(object sender, object argument);

    public delegate void OnExportData(object sender, object argument);

    public interface ISimulationWithEvents : ISimulation
    {
        event OnAfterSetup EventAfterSetup;
        
        event OnAfterTemperature EventAfterTemperature;
        
        event OnBeforeValidation EventAfterValidation;
        
        event OnBeforeExecute EventBeforeExecute;
        
        event OnAfterExecute EventAfterExecute;
       
        event OnBeforeSetup EventBeforeSetup;
        
        event OnBeforeTemperature EventBeforeTemperature;
        
        event OnBeforeSetup EventBeforeUnSetup;
        
        event OnBeforeValidation EventBeforeValidation;
        
        event OnExportData EventExportData;

        IEnumerable<int> InvokeEvents(IEnumerable<int> codes);
    }
}