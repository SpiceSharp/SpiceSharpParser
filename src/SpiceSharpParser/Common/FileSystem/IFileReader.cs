namespace SpiceSharpParser.Common.FileSystem
{
    public interface IFileReader
    {
        /// <summary>
        /// Gets the content of the file located at the specified path.
        /// </summary>
        /// <param name="path">System path to file (either absolute or relative).</param>
        /// <returns>
        /// The content of the file.
        /// </returns>
        string ReadAll(string path);
    }
}