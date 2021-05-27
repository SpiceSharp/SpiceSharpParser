using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace SpiceSharpParser.Common.FileSystem
{
    public static class CsvFileReader
    {
        public static IEnumerable<double[]> Read(string path, bool hasHeader, Encoding encoding)
        {
            if (path == null)
            {
                throw new ArgumentNullException(nameof(path));
            }

            FileReader fileReader = new FileReader(() => encoding);
            var lines = fileReader.ReadAllLines(path);

            if (lines.Length == 0)
            {
                throw new SpiceSharpParserException("Empty CSV file");
            }

            bool hasCommaSeparator = lines[0].Contains(",");
            bool hasSemicolonSeparator = lines[0].Contains(";");
            bool hasTab = lines[0].Contains('\t');

            char separator = hasSemicolonSeparator ? ';' : (hasCommaSeparator ? ',' : (hasTab ? '\t' : ' '));

            foreach (var line in lines.Skip(hasHeader ? 1 : 0))
            {
                var parts = line.Split(separator);

                if (double.TryParse(parts[0].Trim(), NumberStyles.Any, CultureInfo.InvariantCulture, out var time)
                    && double.TryParse(parts[1].Trim(), NumberStyles.Any, CultureInfo.InvariantCulture, out var value))
                {
                    yield return new[] { time, value };
                }
            }
        }

        internal static object Read(string fullFilePath, bool v, object externalFilesEncoding)
        {
            throw new NotImplementedException();
        }
    }
}