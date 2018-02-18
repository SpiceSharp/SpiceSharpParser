using SpiceNetlist.SpiceSharpConnector.Processors.Controls;
using System.Collections.Generic;

namespace SpiceNetlist.SpiceSharpConnector.Processors
{
    public class ControlsRegistry
    {
        private List<BaseControl> controls = new List<BaseControl>();
        private List<string> controlsType = new List<string>();

        private Dictionary<string, BaseControl> controlsByType = new Dictionary<string, BaseControl>();

        public ControlsRegistry()
        {
        }

        public void Add(BaseControl control)
        {
            controls.Add(control);
            controlsByType[control.Type] = control;
        }

        public bool Supports(string type)
        {
            return controlsByType.ContainsKey(type);
        }

        internal BaseControl GetControl(string type)
        {
            return controlsByType[type];
        }

        internal int IndexOf(string type)
        {
            return controlsType.IndexOf(type);
        }
    }
}
