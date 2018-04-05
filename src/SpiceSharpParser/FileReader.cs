using System.IO;

namespace SpiceSharpParser
{
    public class FileReader : IFileReader
    {
        /// <summary>
        /// Gets the content of the file located at the specified path
        /// </summary>
        /// <param name="path">Systme path to file (either absolute or relative)</param>
        /// <returns>
        /// The content of the file
        /// </returns>
        public string GetFileContent(string path)
        {
            return File.ReadAllText(path);
        }
    }
}
