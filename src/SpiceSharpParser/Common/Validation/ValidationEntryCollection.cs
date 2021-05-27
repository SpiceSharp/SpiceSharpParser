using System;
using System.Collections.Generic;
using System.Linq;
using SpiceSharpParser.Models.Netlist.Spice;

namespace SpiceSharpParser.Common.Validation
{
    public class ValidationEntryCollection : List<ValidationEntry>
    {
        public bool HasError => this.Any(c => c.Level == ValidationEntryLevel.Error);

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
                return this.Where(c => c.Level == ValidationEntryLevel.Error);
            }
        }

        public void AddError(ValidationEntrySource source, string message, SpiceLineInfo lineInfo = null, Exception exception = null)
        {
            Add(new ValidationEntry(source, ValidationEntryLevel.Error, message, lineInfo, exception));
        }

        public void AddWarning(ValidationEntrySource source, string message, SpiceLineInfo lineInfo = null)
        {
            Add(new ValidationEntry(source, ValidationEntryLevel.Warning, message, lineInfo));
        }
    }
}
