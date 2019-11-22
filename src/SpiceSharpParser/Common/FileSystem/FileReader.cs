using System.IO;

namespace SpiceSharpParser.Common.FileSystem
{
    public class FileReader : IFileReader
    {
        /// <summary>
        /// Gets the content of the file located at the specified path.
        /// </summary>
        /// <param name="path">System path to file (either absolute or relative).</param>
        /// <returns>
        /// The content of the file.
        /// </returns>
        public string ReadAll(string path)
        {
            if (path == null)
            {
                throw new System.ArgumentNullException(nameof(path));
            }

            if (string.IsNullOrEmpty(path))
            {
                throw new System.ArgumentException(nameof(path));
            }

            return File.ReadAllText(path);
        }

        /// <summary>
        /// Gets the content of the file located at the specified path.
        /// </summary>
        /// <param name="path">System path to file (either absolute or relative).</param>
        /// <returns>
        /// The content of the file in lines.
        /// </returns>
        public string[] ReadAllLines(string path)
        {
            if (path == null)
            {
                throw new System.ArgumentNullException(nameof(path));
            }

            if (string.IsNullOrEmpty(path))
            {
                throw new System.ArgumentException(nameof(path));
            }

            return File.ReadAllLines(path);
        }
    }
}