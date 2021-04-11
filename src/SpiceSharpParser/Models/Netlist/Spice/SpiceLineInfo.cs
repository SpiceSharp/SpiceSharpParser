using SpiceSharpParser.Common;

namespace SpiceSharpParser.Models.Netlist.Spice
{
    public class SpiceLineInfo
    {
        public SpiceLineInfo()
        {
        }

        public SpiceLineInfo(ILocationProvider tokenLocationProvider)
        {
            LineNumber = tokenLocationProvider.LineNumber;
            StartColumnIndex = tokenLocationProvider.StartColumnIndex;
            EndColumnIndex = tokenLocationProvider.EndColumnIndex;
            FileName = tokenLocationProvider.FileName;
        }

        public int LineNumber { get; set; }

        public int StartColumnIndex { get; set; }

        public int EndColumnIndex { get; set; }

        public string FileName { get; set; }
    }
}
