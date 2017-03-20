using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GFScript
{
    public class GFScriptParseException : Exception
    {
        public GFScriptParseException() : base() { }
        public GFScriptParseException(string msg) : base(msg) { }
    }
}
