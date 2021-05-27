using SpiceSharpParser.Models.Netlist.Spice;
using System.Collections.Generic;

namespace SpiceSharpParser.ModelWriters.CSharp
{
    public interface IWriter<T>
        where T : SpiceObject
    {
        public List<CSharpStatement> Write(T @object, IWriterContext context);
    }
}
