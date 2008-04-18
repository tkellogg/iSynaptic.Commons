using System;
using System.Collections.Generic;
using System.Text;

namespace iSynaptic.Commons.Text.Parsing
{
    public class Token<T>
    {
        public Token()
        {
        }

        public T Kind { get; set; }
        public int Position { get; set; }
        public int Column { get; set; }
        public int Line { get; set; }
        public int Length { get; set; }
        public string Value { get; set; }
    }
}
