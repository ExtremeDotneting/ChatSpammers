using Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GFScript
{
    public struct GFScriptPartDelegateAndAnother
    {
        string MethodName;
        GFScriptPartDelegate Method;
    }
    public class GFScriptInterpreter
    {
        class GFScriptPartDelegateAndAnother
        {
            public GFScriptPartDelegateAndAnother(string methodName,GFScriptPartDelegate method)
            {
                MethodName = methodName;
                Method = method;
            }
            public string MethodName;
            public GFScriptPartDelegate Method;
        }

        const string MethodNameSymbolsStart = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890";
        const string MethodNameSymbolsAnother= "1234567890";
        public Dictionary<string, GFScriptBindedMethodDelegate> BindedMethods { get; private set; } = new Dictionary<string, GFScriptBindedMethodDelegate>();
        string strToEscapeDoubleQuotes = "--#string#--";

        public void BindMethod(string methodName, GFScriptBindedMethodDelegate method)
        {
            if (BindedMethods.ContainsKey(methodName))
            {
                BindedMethods[methodName] = method;
            }
            else
            {
                BindedMethods.Add(methodName, method);
            }
        }
        public GFScriptGlobalDelegate ParseScript(string script, bool showParseResult)
        {
            GFScriptGlobalDelegate res = null;
            string parseResult=null;
            try
            {
                res = ParseScript(script, out parseResult, showParseResult);
            }
            finally
            {
                if ( showParseResult)
                    HelpFuncs.ShowInNotebook(parseResult ?? "null");
            }
            return res;
        }
        GFScriptGlobalDelegate ParseScript(string script, out string parseResult, bool showParseResult)
        {
            //Parsing to text
            List<string>expressions= SplitScriptText(script);
            var methodNameAndArgsStringsList = new List<MethodNameAndArgsStrings>();
            for (int i =0;i< expressions.Count; i++)
            {
                var tuple=SplitScriptExpression(expressions[i]);
                methodNameAndArgsStringsList.Add(
                    new MethodNameAndArgsStrings(
                        tuple.Item1,
                        SplitScriptExpressionArgs(tuple.Item2)
                        )
                    );
            }

            //Convert arguments to normal values
            var methodNameAndArgsList = new List<MethodNameAndArgs>();
            for (int i = 0; i < methodNameAndArgsStringsList.Count; i++)
            {
                methodNameAndArgsList.Add(
                    new MethodNameAndArgs(
                        methodNameAndArgsStringsList[i].MethodName,
                        PrepareScriptExpressionArgs(methodNameAndArgsStringsList[i].Arguments)
                        )
                    );
            }

            //Parse result
            if (showParseResult)
            {
                StringBuilder parseResultSB = new StringBuilder();
                for (int i = 0; i < methodNameAndArgsStringsList.Count; i++)
                {
                    parseResultSB.AppendLine(string.Format("<<<{0}>>>\n<<<{1}>>>", methodNameAndArgsStringsList[i], methodNameAndArgsList[i]));
                    parseResultSB.AppendLine("-----------------------------------------");
                    parseResultSB.AppendLine();
                    parseResultSB.AppendLine();
                }
                parseResult = parseResultSB.ToString();
            }
            else
                parseResult = null;

            //Create parts of method
            List<GFScriptPartDelegateAndAnother> partsOfParsedAction = new List<GFScriptPartDelegateAndAnother>();
            for (int i = 0; i < methodNameAndArgsList.Count; i++)
            {
                var methodNameAndArgs = methodNameAndArgsList[i];
                if (BindedMethods.ContainsKey(methodNameAndArgs.MethodName))
                {
                    var bindedAct = BindedMethods[methodNameAndArgs.MethodName];
                    GFScriptPartDelegate newAct = (object inputObject) =>
                    {
                        bindedAct(inputObject, methodNameAndArgs.Arguments);
                    };

                    partsOfParsedAction.Add(new GFScriptPartDelegateAndAnother(methodNameAndArgs.MethodName, newAct));
                }
                else
                {
                    throw new GFScriptParseException(string.Format("Haven`t method binded to method name \"{0}\".", methodNameAndArgsList[i].MethodName));
                }
            }

            //Finally create parsed method.
            GFScriptGlobalDelegate parsedAction = (inputObject) =>
            {
                StringBuilder debugInfo = new StringBuilder();
                foreach(var item in partsOfParsedAction)
                {

                    string partOfDebugInfo = string.Format("Run ---> {0};\n", item?.MethodName ?? "null");
                    try
                    {
                        item.Method?.Invoke(inputObject);
                    }
                    catch (Exception scriptRuntimeEx)
                    {
                        partOfDebugInfo += string.Format("Exception ---> {0};", scriptRuntimeEx.Message.Replace("\n", "\n\t"));
                    }
                    debugInfo.AppendLine(partOfDebugInfo);
                }
                return debugInfo.ToString();
            };
            return parsedAction;
        }
        List<string> SplitScriptText(string script)
        {
            script = script.Replace("\n", "").Replace("\t", "").Trim();
            List<string> res = new List<string>();
            int i=0;
            bool openedDoubleQuotes = false;
            int openedArcs = 0;
            int lastCopyIndex = 0;
            while (++i < script.Length)
            {

                if (script[i] == '"')
                {
                    bool isSlashEscaped = false;
                    if (script[i - 1] == '\\')
                    {
                        if (!openedDoubleQuotes)
                            throw new GFScriptParseException("Try to escape double quotes, that open string.");
                        int j = 0;
                        while (script[i - (++j)] == '\\')
                        {
                            isSlashEscaped = !isSlashEscaped;
                        }
                    }
                    if (!isSlashEscaped)
                    {
                        
                        openedDoubleQuotes = !openedDoubleQuotes;
                        script = script.Remove(i, 1);
                        script=script.Insert(i, strToEscapeDoubleQuotes);
                    }

                    continue;
                    }
                if (script[i] == ';' && !openedDoubleQuotes)
                {
                    if (openedArcs != 0)
                        throw new GFScriptParseException("Every opened '(' must by closed by ')'.");
                    string expr = script.Substring(lastCopyIndex, i - lastCopyIndex);
                    if (!string.IsNullOrWhiteSpace(expr))
                        res.Add(expr);
                    lastCopyIndex = i + 1;
                    continue;
                }

                if (script[i] == '(' && !openedDoubleQuotes)
                {
                    openedArcs++;
                    continue;
                }
                if (script[i] == ')' && !openedDoubleQuotes)
                {
                    openedArcs--;
                    continue;
                }

                if (script[i] == ' ' && !openedDoubleQuotes && openedArcs!=0)
                {
                    script = script.Remove(i, 1);
                    i--;
                }

            }
            if (openedDoubleQuotes)
                throw new GFScriptParseException("Double quotes wasn`t closed.");
            if (openedArcs > 0)
                throw new GFScriptParseException("Arcs wasn`t closed.");
            if (openedArcs < 0)
                throw new GFScriptParseException("Arcs was closed more times than was opened.");
            
            return res;
        }
        Tuple<string, string> SplitScriptExpression(string expression)
        {
            int splitIndex = -1;
            while (expression[++splitIndex] != '(' && splitIndex < expression.Length) { }
            int secondAcr = -1;
            while (expression[++secondAcr] != ')' && splitIndex < expression.Length) { }
            if (splitIndex >= secondAcr)
                throw new GFScriptParseException(string.Format("Wrong args in \"{0}\"", expression));
            string args = expression.Substring(splitIndex + 1, secondAcr - splitIndex);
            string methodName = expression.Substring(0, splitIndex).Trim();
            if (ValidateMethodName(methodName))
                throw new GFScriptParseException(string.Format("Method name can`t have space symbol. Current method name \"{0}\"", methodName));
            return new Tuple<string, string>(methodName, args);
        }
        List<string> SplitScriptExpressionArgs(string args)
        {
            var res = new List<string>();
            bool openedDoubleQuotes = false;
            int i = -1;
            int lastSplit = 0;
            while (++i < args.Length)
            {
                if ((args[i] == ',' || args[i] == ')') && !openedDoubleQuotes)
                {
                    int splitIndex = i;
                    string arg = args.Substring(lastSplit, splitIndex - lastSplit);
                    if(!string.IsNullOrWhiteSpace(arg))
                        res.Add(arg);
                    lastSplit = splitIndex + 1;
                    continue;
                }
                if (args[i] == '-' && args[i + 1] == '-' && (i + strToEscapeDoubleQuotes.Length < args.Length))
                {
                    if (args.Substring(i, strToEscapeDoubleQuotes.Length) == strToEscapeDoubleQuotes)
                    {
                        openedDoubleQuotes = !openedDoubleQuotes;
                    }
                }

            }
            return res;
        }
        List<object> PrepareScriptExpressionArgs(List<string> splitedArgs)
        {
            var res = new List<object>();
            for(int i = 0; i < splitedArgs.Count; i++)
            {
                res.Add(ConvertParsedStringValueToNormalType(splitedArgs[i]));
            }
            return res;
        }
        object ConvertParsedStringValueToNormalType(string argument)
        {
            object res=null;

            bool isNumber = true;
            string numbersString = "1234567890";
            bool numberWithDot = false;
            for (int j = 0; j < argument.Length; j++)
            {
                char ch = argument[j];
                isNumber = numbersString.IndexOf(ch)>=0 || ch=='.';
                if (ch == '.')
                {
                    if (numberWithDot)
                        throw new GFScriptParseException("Two or more dots in number.");
                    isNumber = numberWithDot = true;
                }
                if (!isNumber)
                    break;
            }
            if (isNumber)
            {
                try
                {
                    if (numberWithDot)
                    {
                        argument = argument.Replace(".", ",");
                        res = Convert.ToDouble(argument);
                    }
                    else
                    {
                        argument = argument.Replace(".", ",");
                        try
                        {
                            res = Convert.ToInt32(argument);
                        }
                        catch
                        {
                            res = Convert.ToInt64(argument);
                        }
                    }
                }
                catch
                {
                    isNumber = false;
                }
            }
            if (!isNumber || res == null)
            {
                res = argument.Replace(strToEscapeDoubleQuotes, "").Replace("\\\"", "\"").Replace("\\t", "\t").Replace("\\n", "\n");
            }
            return res;
        }
        bool ValidateMethodName(string methodName)
        {
            if (MethodNameSymbolsStart.IndexOf(methodName[0]) < 0)
                return false;
            for (int i = 1; i < methodName.Length; i++)
            {
                if (MethodNameSymbolsAnother.IndexOf(methodName[i]) < 0)
                    return false;
            }
            return true;
        }

    }
}
