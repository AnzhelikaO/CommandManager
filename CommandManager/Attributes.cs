#region Using
using System;
using System.Collections.Generic;
using System.Linq;
using TShockAPI;
#pragma warning disable 1591
#endregion
namespace CommandManager
{
    #region CommandInfo

    #region Summary

    /// <summary> Adding this attrubute to method means
    /// that it is a <see cref="Command.CommandDelegate"/>.
    /// <para> Add <see cref="CommandNames"/>
    /// to define <see cref="Command.Names"/>. </para>
    /// Add <see cref="CommandPermissions"/>
    /// to define <see cref="Command.Permissions"/>.
    /// <para> Add <see cref="Help"/> attribute
    /// to define <see cref="Command.HelpText"/>.
    /// Or <see cref="HelpDesc"/> to define
    /// <see cref="Command.HelpDesc"/>. </para>
    /// Add <see cref="DisallowServer"/> attribute
    /// to set <see cref="Command.AllowServer"/> to false.
    /// <para> Add <see cref="DoNotLog"/> attribute
    /// to set <see cref="Command.DoLog"/>
    /// to false. </para> 
    /// Names array and each its element must
    /// be not null and not empty. </summary>
    /// <exception cref="InvalidOperationException"></exception>

    #endregion
    public class CommandInfo : Attribute
    {
        /// <summary> <see cref="Command.Permissions"/>. </summary>
        public List<string> CommandPermissions { get; internal set; }
        /// <summary> <see cref="Command.Names"/>. </summary>
        public string[] CommandNames { get; }
        #region Summary

        /// <summary> Adding this attrubute to method means
        /// that it is a <see cref="Command.CommandDelegate"/>.
        /// <para> Add <paramref name="Names"/>
        /// to define <see cref="Command.Names"/>. </para>
        /// <para> Add <see cref="Help"/> attribute
        /// to define <see cref="Command.HelpText"/>.
        /// Or <see cref="HelpDesc"/> to define
        /// <see cref="Command.HelpDesc"/>. </para>
        /// Add <see cref="DisallowServer"/> attribute
        /// to set <see cref="Command.AllowServer"/> to false.
        /// <para> Add <see cref="DoNotLog"/> attribute
        /// to set <see cref="Command.DoLog"/>
        /// to false. </para> 
        /// Names array and each its element must
        /// be not null and not empty. </summary>
        /// <param name="Names"><see cref="Command.Names"/></param>
        /// <exception cref="InvalidOperationException"></exception>

        #endregion
        public CommandInfo(params string[] Names)
            : this(new List<string>(), Names) { }
        #region Summary

        /// <summary> Adding this attrubute to method means
        /// that it is a <see cref="Command.CommandDelegate"/>.
        /// <para> Add <paramref name="Names"/>
        /// to define <see cref="Command.Names"/>. </para>
        /// Add <paramref name="Permission"/>
        /// to define <see cref="Command.Permissions"/>.
        /// <para> Add <see cref="Help"/> attribute
        /// to define <see cref="Command.HelpText"/>.
        /// Or <see cref="HelpDesc"/> to define
        /// <see cref="Command.HelpDesc"/>. </para>
        /// Add <see cref="DisallowServer"/> attribute
        /// to set <see cref="Command.AllowServer"/> to false.
        /// <para> Add <see cref="DoNotLog"/> attribute
        /// to set <see cref="Command.DoLog"/>
        /// to false. </para> 
        /// Names array and each its element must
        /// be not null and not empty. </summary>
        /// <param name="Permission"><see cref="Command.Permissions"/></param>
        /// <param name="Names"><see cref="Command.Names"/></param>
        /// <exception cref="InvalidOperationException"></exception>

        #endregion
        public CommandInfo(string Permission, params string[] Names)
            : this((string.IsNullOrWhiteSpace(Permission)
                        ? null
                        : new List<string>() { Permission }),
                   Names) { }
        #region Summary

        /// <summary> Adding this attrubute to method means
        /// that it is a <see cref="Command.CommandDelegate"/>.
        /// <para> Add <paramref name="Names"/>
        /// to define <see cref="Command.Names"/>. </para>
        /// Add <paramref name="Permissions"/>
        /// to define <see cref="Command.Permissions"/>.
        /// <para> Add <see cref="Help"/> attribute
        /// to define <see cref="Command.HelpText"/>.
        /// Or <see cref="HelpDesc"/> to define
        /// <see cref="Command.HelpDesc"/>. </para>
        /// Add <see cref="DisallowServer"/> attribute
        /// to set <see cref="Command.AllowServer"/> to false.
        /// <para> Add <see cref="DoNotLog"/> attribute
        /// to set <see cref="Command.DoLog"/>
        /// to false. </para> 
        /// Names array and each its element must
        /// be not null and not empty. </summary>
        /// <param name="Permissions"><see cref="Command.Permissions"/></param>
        /// <param name="Names"><see cref="Command.Names"/></param>
        /// <exception cref="InvalidOperationException"></exception>

        #endregion
        public CommandInfo(List<string> Permissions, params string[] Names)
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
            else { Names = new List<string>(Names).ToArray(); }
            this.CommandPermissions =
                new List<string>(Permissions ?? new List<string>());
        }
    }

    #endregion
    #region Summary

    /// <summary> Will work only with
    /// <see cref="CommandInfo"/> attribute.
    /// <para> Adding this attribute to command delegate
    /// sets <see cref="Command.AllowServer"/>
    /// to false. </para> </summary>

    #endregion
    public class DisallowServer : Attribute { }
    #region Summary

    /// <summary> Will work only with
    /// <see cref="CommandInfo"/> attribute.
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
    /// <see cref="CommandInfo"/> attribute
    /// has been properly added. </summary>

    #endregion
    public class DoNotRegister : Attribute { }
    #region Help

    #region Summary

    /// <summary> Will work only with
    /// <see cref="CommandInfo"/> attribute.
    /// <para> Adding this attribute to command delegate
    /// defines <see cref="Command.HelpText"/>. </para> </summary>

    #endregion
    public class Help : Attribute
    {
        /// <summary> <see cref="Command.HelpText"/>. </summary>
        public string CommandHelp { get; }
        #region Summary

        /// <summary> Will work only with
        /// <see cref="CommandInfo"/> attribute.
        /// <para> Adding this attribute to command delegate
        /// defines <see cref="Command.HelpText"/>. </para> </summary>
        /// <param name="CommandHelp"><see cref="Command.HelpText"/></param>

        #endregion
        public Help(string CommandHelp) =>
            this.CommandHelp = (CommandHelp ?? "No help available.");
    }

    #region Summary

    /// <summary> Will work only with
    /// <see cref="CommandInfo"/> attribute.
    /// <para> Adding this attribute to command delegate
    /// defines <see cref="Command.HelpDesc"/>. </para> </summary>

    #endregion
    public class HelpDesc : Attribute
    {
        /// <summary> <see cref="Command.HelpDesc"/>. </summary>
        public string[] CommandHelpDesc { get; }
        #region Summary

        /// <summary> Will work only with
        /// <see cref="CommandInfo"/> attribute.
        /// <para> Adding this attribute to command delegate
        /// defines <see cref="Command.HelpDesc"/>. </para> </summary>
        /// <param name="CommandHelpDesc"><see cref="Command.HelpDesc"/></param>

        #endregion
        public HelpDesc(params string[] CommandHelpDesc) =>
            this.CommandHelpDesc =
                ((CommandHelpDesc == null)
                    ? null
                    : new List<string>(CommandHelpDesc).ToArray());
    }

    #endregion
    #region Token
    
    public class Token : Attribute
    {
        public string CommandToken { get; }
        public Token(string CommandToken) =>
            this.CommandToken = ((string.IsNullOrWhiteSpace(CommandToken)
                                    ? throw new InvalidOperationException()
                                    : CommandToken));
    }

    #endregion
}