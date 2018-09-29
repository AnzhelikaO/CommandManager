#region Using
using System;
using System.Collections.Generic;
using TShockAPI;
#endregion
namespace CommandManager
{
    #region Summary

    /// <summary> Will work only with
    /// <see cref="CommandInfoAttribute"/> attribute.
    /// <para> Adding this attribute to command delegate
    /// defines <see cref="Command.HelpDesc"/>. </para> </summary>

    #endregion
    public class HelpDescAttribute : Attribute
    {
        /// <summary> <see cref="Command.HelpDesc"/>. </summary>
        public string[] CommandHelpDesc { get; }
        #region Summary

        /// <summary> Will work only with
        /// <see cref="CommandInfoAttribute"/> attribute.
        /// <para> Adding this attribute to command delegate
        /// defines <see cref="Command.HelpDesc"/>. </para> </summary>
        /// <param name="CommandHelpDesc"><see cref="Command.HelpDesc"/></param>

        #endregion
        public HelpDescAttribute(params string[] CommandHelpDesc) =>
            this.CommandHelpDesc =
                ((CommandHelpDesc == null)
                    ? null
                    : new List<string>(CommandHelpDesc).ToArray());
    }
}