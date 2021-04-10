using SpiceSharp.Entities;
using SpiceSharp.ParameterSets;
using System;
using System.Collections.Generic;
using System.Text;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Custom
{
    public class ISwitchModel : Entity<BindingContext>, IParameterized<ISwitchModelBaseParameters>
    {
        public ISwitchModel(string name)
            : base(name)
        {
        }

        public ISwitchModelBaseParameters Parameters { get; } = new ISwitchModelBaseParameters();
    }
}
