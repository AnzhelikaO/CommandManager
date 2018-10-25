#region Using
using System;
using System.Collections.Generic;
using System.Reflection;
using TShockAPI;
#endregion
namespace CommandManager
{
    #region Summary

    /// <summary> Defines subcommands at
    /// <see cref="CommandInfoAttribute.SubCommandParameterIndex"/>.
    /// <para> If <see cref="Names"/> is null, method will be executed with
    /// any value of parameter, if value doesn't match one of another
    /// subcommand names. </para> </summary>

    #endregion
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class SubCommandAttribute : Attribute
    {
        internal static Dictionary<MethodInfo, CommandDelegate> SubCommands =
            new Dictionary<MethodInfo, CommandDelegate>();

        /// <summary> SubCommand's <see cref="CommandManagerDelegate"/>. </summary>
        public MethodInfo Method { get; }
        /// <summary> Namespace.Type.Method.Name </summary>
        public string MethodName { get; internal set; }
        #region Summary

        /// <summary> f null, method will be executed with
        /// any value of parameter, if value doesn't match
        /// one of another subcommand names. </summary>

        #endregion
        public string[] Names { get; }
        #region Summary

        /// <summary> Defines subcommands at
        /// <see cref="CommandInfoAttribute.SubCommandParameterIndex"/>.
        /// <para> If <see cref="Names"/> is null, method will be executed with
        /// any value of parameter, if value doesn't match one of another
        /// subcommand names. </para> </summary>

        #endregion
        public SubCommandAttribute(Type MethodClass,
            string MethodName, params string[] SubCommandNames)
        {
            if (MethodClass == null)
            { throw new ArgumentNullException("MethodClass"); }
            if (string.IsNullOrWhiteSpace(MethodName))
            { throw new ArgumentNullException("MethodName"); }
            this.MethodName =
                $"{MethodClass.Namespace}.{MethodClass.Name}.{MethodName}";
            this.Method = MethodClass.GetMethod(MethodName);
            if (Method == null)
            { throw new ArgumentException($"Method '{this.MethodName}' was not found."); }
            if (!Method.IsCMDelegate($"'{this.MethodName}'", false, out string exception))
            { throw new ArgumentException(exception); }
            
            if (SubCommandNames?.Length == 0) { return; }
            List<string> cmds = new List<string>();
            for (int i = 0; i < SubCommandNames.Length; i++)
            {
                if (string.IsNullOrWhiteSpace(SubCommandNames[i]))
                { throw new ArgumentNullException($"SubCommandNames[{i}]"); }
                string cmd = SubCommandNames[i].ToLower();
                if (cmds.Contains(cmd))
                {
                    //check in all subcommands for this method
                    throw new ArgumentException("SubCommand names " +
                        "must be unique.", $"SubCommandNames[{i}]");
                }
                cmds.Add(cmd);
            }
            this.Names = cmds.ToArray();
        }
    }
}