#region Using
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
#endregion
namespace CommandManager
{
    internal class ParameterInfo
    {
        public string Name { get; }
        public Type Type { get; }
        public bool ShowType { get; }
        public bool? AllowMergeInErrorMessage { get; internal set; }
        #region Constructor

        public ParameterInfo(string Name, Type Type, bool ShowType)
        {
            this.Name = Name;
            this.Type = Type;
            this.ShowType = ShowType;
            this.AllowMergeInErrorMessage = null;
        }

        #endregion
        #region ToString

        private static string ToString(string Name,
            Type Type, bool ShowType, bool Required)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(Required ? "<" : "[");
            sb.Append(Name);
            if (ShowType && (Type != null))
            {
                sb.Append(" (");
                sb.Append(Type.Name.Replace("TSPlayer", "Player"));
                sb.Append(")");
            }
            sb.Append(Required ? ">" : "]");
            return sb.ToString();
        }
        
        public string ToString(bool Required = true)
        {
            string param = ToString(Name, Type, ShowType, Required);
            if (AllowMergeInErrorMessage.HasValue
                && !AllowMergeInErrorMessage.Value)
            { param += $" {param} ..."; }
            return param;
        }

        #endregion
        #region ParseOne
        
        public bool Parse(string Parameter, out ParameterParseResult Result)
        {
            Result = null;
            if ((Type == null)
                || !CommandManager.Parsers.TryGetValue(Type,
                    out Func<string, ParameterParseResult> func))
            { return true; }
            Result = func(Parameter);
            if (Result.Error != null)
            {
                Result.Error = "Invalid parameter " +
                    $"'{Name}': {Result.Error}";
            }
            return Result.Success;
        }

        #endregion
        #region ParseMany

        public bool Parse(IEnumerable<string> Parameters,
            out ParameterParseResult[] Result)
        {
            Result = Parameters.Select(p =>
                new ParameterParseResult(p, p, true, null)).ToArray();
            if ((Type == null)
                || !CommandManager.Parsers.TryGetValue(Type,
                    out Func<string, ParameterParseResult> func))
            { return true; }
            for (int i = 0; i < Result.Length; i++)
            {
                Result[i] = func(Parameters.ElementAt(i));
                if (Result[i].Error != null)
                {
                    Result[i].Error = "Invalid parameter " +
                        $"'{Name}': {Result[i].Error}";
                }
            }
            return Result.All(r => r.Success);
        }

        #endregion
    }
}