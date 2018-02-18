using System.Collections.Generic;
using SpiceNetlist.SpiceSharpConnector.Processors.Controls;

namespace SpiceNetlist.SpiceSharpConnector
{
    public class ControlRegistry
    {
        private List<BaseControl> controls = new List<BaseControl>();
        private List<string> controlsType = new List<string>();

        private Dictionary<string, BaseControl> controlsByType = new Dictionary<string, BaseControl>();

        public ControlRegistry()
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

        public BaseControl GetControl(string type)
        {
            return controlsByType[type];
        }

        public int IndexOf(string type)
        {
            return controlsType.IndexOf(type);
        }
    }
}
