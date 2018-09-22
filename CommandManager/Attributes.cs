#region Using
using System;
using System.Collections.Generic;
using System.Linq;
using TShockAPI;
#endregion
namespace CommandManager
{
    #region Summary

    /// <summary> Adding this attrubute to method means
    /// that it is a CommandDelegate of a command.
    /// <para> Add <see cref="Names"/> attribute
    /// to define <see cref="Command.Names"/>. </para>
    /// Add <see cref="Permissions"/> attribute
    /// to define <see cref="Command.Permissions"/>.
    /// <para> Add <see cref="Help"/> attribute
    /// to define <see cref="Command.HelpText"/>.
    /// Or <see cref="HelpDesc"/> to define
    /// <see cref="Command.HelpDesc"/>. </para>
    /// Add <see cref="DisallowServer"/> attribute
    /// to set <see cref="Command.AllowServer"/> to false.
    /// <para> Add <see cref="DoNotLog"/> attribute
    /// to set <see cref="Command.DoLog"/>
    /// to false. </para> </summary>

    #endregion
    public class IsCommand : Attribute { }
    #region Summary

    /// <summary> Will work only with <see cref="IsCommand"/>
    /// and <see cref="Names"/> attributes.
    /// <para> Adding this attribute to command delegate
    /// sets <see cref="Command.AllowServer"/>
    /// to false. </para> </summary>

    #endregion
    public class DisallowServer : Attribute { }
    #region Summary

    /// <summary> Will work only with <see cref="IsCommand"/>
    /// and <see cref="Names"/> attributes.
    /// <para> Adding this attribute to command delegate
    /// sets <see cref="Command.DoLog"/>
    /// to false. </para> </summary>

    #endregion
    public class DoNotLog : Attribute { }
    #region Summary

    /// <summary> Adding this attribute to command
    /// delegate means that it will not replace existing
    /// command(s) if one of the <see cref="Command.Names"/>
    /// already taken. </summary>

    #endregion
    public class DoNotReplaceIfNameExists : Attribute { }
    #region Summary

    /// <summary> Adding this attribute to command
    /// delegate means that it will not be added to
    /// <see cref="Commands.ChatCommands"/> even if 
    /// <see cref="IsCommand"/> and <see cref="Names"/>
    /// attributes has been properly added. </summary>

    #endregion
    public class DoNotRegister : Attribute { }
    #region Names

    #region Summary

    /// <summary> Will work only with
    /// <see cref="IsCommand"/> attribute.
    /// <para> Adding this attribute to command
    /// delegate defines <see cref="Command.Names"/>. </para>
    /// Names array and each its element must
    /// be not null and not empty. </summary>
    /// <exception cref="InvalidOperationException"></exception>

    #endregion
    public class Names : Attribute
    {
        /// <summary> <see cref="Command.Names"/>. </summary>
        public string[] CommandNames { get; }
        #region Summary

        /// <summary> Will work only with
        /// <see cref="IsCommand"/> attribute.
        /// <para> Adding this attribute to command
        /// delegate defines <see cref="Command.Names"/>. </para>
        /// Names array and each its element must
        /// be not null and not empty. </summary>
        /// <exception cref="InvalidOperationException"></exception>

        #endregion
        public Names(params string[] Names)
        {
            string error = "Names array and each its " +
                "element must be not null and not empty.";
            this.CommandNames =
                Names?.Select(n => string.IsNullOrWhiteSpace(n)
                                    ? throw new InvalidOperationException(error)
                                    : n.ToLower())?
                                         .ToArray();
            if (Names?.Length == 0)
            { throw new InvalidOperationException(error); }
        }
    }

    #endregion
    #region Permissions

    #region Summary

    /// <summary> Will work only with <see cref="IsCommand"/>
    /// and <see cref="Names"/> attributes.
    /// <para> Adding this attribute to command delegate
    /// defines <see cref="Command.Permissions"/>. </para> </summary>

    #endregion
    public class Permissions : Attribute
    {
        /// <summary> <see cref="Command.Permissions"/>. </summary>
        public List<string> CommandPermissions { get; }
        #region Summary

        /// <summary> Will work only with <see cref="IsCommand"/>
        /// and <see cref="Names"/> attributes.
        /// <para> Adding this attribute to command delegate
        /// defines <see cref="Command.Permissions"/>. </para> </summary>

        #endregion
        public Permissions(params string[] CommandPermissions) =>
            this.CommandPermissions = (CommandPermissions ?? new string[0]).ToList();
    }

    #endregion
    #region Help

    #region Summary

    /// <summary> Will work only with <see cref="IsCommand"/>
    /// and <see cref="Names"/> attributes.
    /// <para> Adding this attribute to command delegate
    /// defines <see cref="Command.HelpText"/>. </para> </summary>

    #endregion
    public class Help : Attribute
    {
        /// <summary> <see cref="Command.HelpText"/>. </summary>
        public string CommandHelp { get; }
        #region Summary

        /// <summary> Will work only with <see cref="IsCommand"/>
        /// and <see cref="Names"/> attributes.
        /// <para> Adding this attribute to command delegate
        /// defines <see cref="Command.HelpText"/>. </para> </summary>

        #endregion
        public Help(string CommandHelp) =>
            this.CommandHelp = (CommandHelp ?? "No help available.");
    }

    #region Summary

    /// <summary> Will work only with <see cref="IsCommand"/>
    /// and <see cref="Names"/> attributes.
    /// <para> Adding this attribute to command delegate
    /// defines <see cref="Command.HelpDesc"/>. </para> </summary>

    #endregion
    public class HelpDesc : Attribute
    {
        /// <summary> <see cref="Command.HelpDesc"/>. </summary>
        public string[] CommandHelpDesc { get; }
        #region Summary

        /// <summary> Will work only with <see cref="IsCommand"/>
        /// and <see cref="Names"/> attributes.
        /// <para> Adding this attribute to command delegate
        /// defines <see cref="Command.HelpDesc"/>. </para> </summary>

        #endregion
        public HelpDesc(params string[] CommandHelpDesc) =>
            this.CommandHelpDesc = CommandHelpDesc;
    }

    #endregion
}