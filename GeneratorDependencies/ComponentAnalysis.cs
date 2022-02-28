using System;
using System.Collections.Generic;
using System.Text;

namespace GeneratorDependencies
{
    [System.AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public sealed class ComponentAnalysisAttribute : Attribute
    {
        readonly string positionalString;

        // This is a positional argument
        public ComponentAnalysisAttribute(string positionalString)
        {
            this.positionalString = positionalString;

            // TODO: Implement code here

            //throw new NotImplementedException();
        }

        public ComponentAnalysisAttribute()
        {
            this.positionalString = string.Empty;

            // TODO: Implement code here

            //throw new NotImplementedException();
        }

        public string PositionalString
        {
            get { return positionalString; }
        }

        // This is a named argument
        public int NamedInt { get; set; }
    }
}
