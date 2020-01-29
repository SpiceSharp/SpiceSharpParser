using System.Collections.Generic;
using System.Linq;

namespace SpiceSharpParser.Common.Validation
{
    public class ValidationEntryCollection : List<ValidationEntry>
    {
        public bool HasError => this.Any(c => c.Level == ValidationEntryLevel.Error);

        public bool IsValid => !HasError;

        public bool HasWarning => this.Any(c => c.Level == ValidationEntryLevel.Warning);

        public IEnumerable<ValidationEntry> Errors
        {
            get
            {
                return this.Where(c => c.Level == ValidationEntryLevel.Error);
            }
        }

        public IEnumerable<ValidationEntry> Warnings
        {
            get
            {
                return this.Where(c => c.Level == ValidationEntryLevel.Warning);
            }
        }
    }
}
