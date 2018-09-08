using SpiceSharp.Circuits;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Context
{
    public interface IModelsRegistry
    {
        /// <summary>
        /// Finds a model with given name.
        /// </summary>
        /// <param name="modelName">Name of model to get.</param>
        /// <returns>
        /// A reference to model.
        /// </returns>
        T FindBaseModel<T>(string modelName)
            where T : Entity;
    }
}
