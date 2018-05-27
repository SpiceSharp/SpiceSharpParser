using SpiceSharpParser.ModelReader.Netlist.Spice.Processors.Controls;

namespace SpiceSharpParser.ModelReader.Netlist.Spice.Registries
{
    /// <summary>
    /// Interface for all control registries
    /// </summary>
    public interface IControlRegistry : IRegistry
    {
        /// <summary>
        /// Gets a value indicating whether a specified control is in registry
        /// </summary>
        /// <param name="type">Type of control</param>
        /// <returns>
        /// A value indicating whether a specified control is in registry
        /// </returns>
        bool Supports(string type);

        /// <summary>
        /// Gets order of control in the registry
        /// </summary>
        /// <param name="type">Type of control</param>
        /// <returns>
        /// The order
        /// </returns>
        int IndexOf(string type);

        /// <summary>
        /// Gets a control by type
        /// </summary>
        /// <param name="type">A type of control</param>
        /// <returns>
        /// The control
        /// </returns>
        BaseControl Get(string type);

        /// <summary>
        /// Adds a control to registy
        /// </summary>
        /// <param name="control">A control to add</param>
        void Add(BaseControl control);
    }
}
