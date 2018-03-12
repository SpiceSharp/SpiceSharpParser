using SpiceNetlist.SpiceObjects;
using System;
using System.Collections.Generic;
using System.Text;

namespace SpiceNetlist.SpiceSharpConnector.Processors
{
    public interface IStatementsProcessor
    {
        void Process(Statements statements, ProcessingContext context);
    }
}
