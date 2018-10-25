#region Using
using System;
using TShockAPI;
#endregion
namespace CommandManager
{
    #region Summary

    /// <summary> Will work only with
    /// <see cref="CommandInfoAttribute"/> attribute.
    /// <para> Adding this attribute to command delegate
    /// defines <see cref="Command.HelpText"/>. </para> </summary>

    #endregion
    [AttributeUsage(AttributeTargets.Method)]
    public class HelpAttribute : Attribute
    {
        /// <summary> <see cref="Command.HelpText"/>. </summary>
        public string CommandHelp { get; }
        #region Summary

        /// <summary> Will work only with
        /// <see cref="CommandInfoAttribute"/> attribute.
        /// <para> Adding this attribute to command delegate
        /// defines <see cref="Command.HelpText"/>. </para> </summary>
        /// <param name="CommandHelp"><see cref="Command.HelpText"/></param>

        #endregion
        public HelpAttribute(string CommandHelp) =>
            this.CommandHelp = (CommandHelp ?? "No help available.");
    }
}