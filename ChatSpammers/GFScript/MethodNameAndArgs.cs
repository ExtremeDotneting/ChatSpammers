using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GFScript
{
    public class MethodNameAndArgs : IFormattable
    {
        public MethodNameAndArgs() { }
        public MethodNameAndArgs(string methodName, List<object> arguments)
        {
            MethodName = methodName;
            Arguments = arguments;
        }
        public string MethodName;
        public List<object> Arguments;

        public string ToString(string format, IFormatProvider formatProvider)
        {
            string res = MethodName + " => { \n----";
            for (int i = 0; i < Arguments.Count; i++)
            {
                res += string.Format("*{0}*", Convert.ToString(Arguments[i]).Replace("\n","\n----"));
                if (i < Arguments.Count - 1)
                    res += ",\n----";
            }
            res += "}";
            return res;
        }
    }
}
