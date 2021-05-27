using System;
using System.Collections.Generic;
using System.Text;

namespace SpiceSharpParser.Common.FileSystem
{
    public class FileReader : IFileReader
    {
        public FileReader(Func<Encoding> encoding)
        {
            Encoding = encoding;
        }

        public Func<Encoding> Encoding { get; }

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
                throw new ArgumentNullException(nameof(path));
            }

            if (string.IsNullOrEmpty(path))
            {
                throw new ArgumentException(nameof(path));
            }

            using var reader = new System.IO.StreamReader(path, Encoding(), true);
            return reader.ReadToEnd();
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
                throw new ArgumentNullException(nameof(path));
            }

            if (string.IsNullOrEmpty(path))
            {
                throw new ArgumentException(nameof(path));
            }

            using var reader = new System.IO.StreamReader(path, Encoding(), true);
            var lines = new List<string>();
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                lines.Add(line);
            }

            return lines.ToArray();
        }
    }
}