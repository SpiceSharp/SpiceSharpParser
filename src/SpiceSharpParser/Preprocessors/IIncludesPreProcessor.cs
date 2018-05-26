using SpiceSharpParser.Model.Netlist.Spice;

namespace SpiceSharpParser.Preprocessors
{
    public interface IIncludesPreProcessor
    {
        /// <summary>
        /// Processes .include statements
        /// </summary>
        /// <param name="netlistModel">Netlist model to seach for .include statements</param>
        /// <param name="currentDirectoryPath">Current working directory path</param>
        void Process(SpiceNetlist netlistModel, string currentDirectoryPath = null);
    }
}
