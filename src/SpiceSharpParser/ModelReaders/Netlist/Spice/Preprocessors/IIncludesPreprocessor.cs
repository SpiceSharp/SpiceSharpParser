using SpiceSharpParser.Models.Netlist.Spice;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Preprocessors
{
    public interface IIncludesPreprocessor
    {
        /// <summary>
        /// Preprocess .include statements.
        /// </summary>
        /// <param name="netlistModel">Netlist model to seach for .include statements</param>
        /// <param name="currentDirectoryPath">Current working directory path</param>
        void Preprocess(SpiceNetlist netlistModel, string currentDirectoryPath = null);
    }
}
