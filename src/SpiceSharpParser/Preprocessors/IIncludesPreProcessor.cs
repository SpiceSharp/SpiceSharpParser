using SpiceSharpParser.Model;

namespace SpiceSharpParser.Preprocessors
{
    public interface IIncludesPreProcessor
    {
        /// <summary>
        /// Processes .include statements
        /// </summary>
        /// <param name="netlistModel">Netlist model to seach for .include statements</param>
        /// <param name="currentDirectoryPath">Current working directory path</param>
        void Process(Netlist netlistModel, string currentDirectoryPath = null);
    }
}
