using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GFScript
{
    public class MethodNameAndArgsStrings : IFormattable
    {
        public MethodNameAndArgsStrings() { }
        public MethodNameAndArgsStrings(string methodName, List<string> arguments)
        {
            MethodName = methodName;
            Arguments = arguments;
        }
        public string MethodName;
        public List<string> Arguments;

        public string ToString(string format, IFormatProvider formatProvider)
        {
            string res = MethodName + " => { ";
            for (int i = 0; i < Arguments.Count; i++)
            {
                res += string.Format("*{0}*",Convert.ToString( Arguments[i]));
                if (i < Arguments.Count - 1)
                    res += ", ";
            }
            res += "}";
            return res;
        }
    }
}
