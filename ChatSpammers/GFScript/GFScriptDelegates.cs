using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GFScript
{
    /// <summary>
    /// </summary>
    /// <returns>Debug info.</returns>
    public delegate string GFScriptGlobalDelegate(object inputObject);

    /// <summary>
    /// </summary>
    /// <returns>Debug info.</returns>
    public delegate void GFScriptPartDelegate(object inputObject);

    /// <summary>
    /// </summary>
    /// <returns>Debug info.</returns>
    public delegate void GFScriptBindedMethodDelegate(object inputObject, List<object> parsedArgs);
}
