#region Using
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TShockAPI;
#endregion
namespace CommandManager
{
    #region Summary

    /// <summary> Adding this attrubute to method means
    /// that it is a <see cref="CommandManagerDelegate"/>.
    /// <para> Add <see cref="CommandNames"/>
    /// to define <see cref="Command.Names"/>. </para>
    /// Add <see cref="CommandPermissions"/>
    /// to define <see cref="Command.Permissions"/>.
    /// <para> Add <see cref="HelpAttribute"/> attribute
    /// to define <see cref="Command.HelpText"/>.
    /// Or <see cref="HelpDescAttribute"/> to define
    /// <see cref="Command.HelpDesc"/>. </para>
    /// Add <see cref="DisallowServerAttribute"/> attribute
    /// to set <see cref="Command.AllowServer"/> to false.
    /// <para> Add <see cref="DoNotLogAttribute"/> attribute
    /// to set <see cref="Command.DoLog"/> to false. </para> 
    /// Names array and each its element must
    /// be not null and not empty. </summary>
    /// <exception cref="ArgumentException"></exception>

    #endregion
    [AttributeUsage(AttributeTargets.Method)]
    public class CommandInfoAttribute : Attribute
    {
        /// <summary> <see cref="Command.Permissions"/>. </summary>
        public List<string> CommandPermissions { get; internal set; }
        /// <summary> <see cref="Command.Names"/>. </summary>
        public string[] CommandNames { get; }
        /// <summary> If method has <see cref="SubCommandAttribute"/>,
        /// defines parameter switch index. </summary>
        public int SubCommandParameterIndex { get; }
        #region AllowedPermissionTypes

        /// <summary> Allowed field types in PermissionContainer. </summary>
        public static readonly Type[] AllowedPermissionTypes =
            new Type[]
            {
                typeof(string),
                typeof(List<string>),
                typeof(string[])
            };

        #endregion

        #region Constructor

        #region string Permissions

        #region Summary

        /// <summary> Adding this attrubute to method means
        /// that it is a <see cref="CommandManagerDelegate"/>.
        /// <para> Add <see cref="CommandNames"/>
        /// to define <see cref="Command.Names"/>. </para>
        /// Add <see cref="CommandPermissions"/>
        /// to define <see cref="Command.Permissions"/>.
        /// <para> Add <see cref="HelpAttribute"/> attribute
        /// to define <see cref="Command.HelpText"/>.
        /// Or <see cref="HelpDescAttribute"/> to define
        /// <see cref="Command.HelpDesc"/>. </para>
        /// Add <see cref="DisallowServerAttribute"/> attribute
        /// to set <see cref="Command.AllowServer"/> to false.
        /// <para> Add <see cref="DoNotLogAttribute"/> attribute
        /// to set <see cref="Command.DoLog"/> to false. </para> 
        /// Names array and each its element must
        /// be not null and not empty. </summary>
        /// <exception cref="ArgumentException"></exception>

        #endregion
        public CommandInfoAttribute(string[] Permissions,
            int SubCommandParameterIndex, params string[] Names)
        {
            this.SubCommandParameterIndex =
                (SubCommandParameterIndex >= 0)
                    ? (SubCommandParameterIndex - 1)
                    : throw new ArgumentOutOfRangeException("SubCommandParameterIndex");
            string error = "Names array and each its " +
                "element must be not null and not empty.";
            this.CommandNames =
                Names?.Select(n => string.IsNullOrWhiteSpace(n)
                                    ? throw new ArgumentException(error)
                                    : n.ToLower())?
                                         .ToArray();
            if (Names?.Length == 0)
            { throw new ArgumentException(error); }
            else { Names = new List<string>(Names).ToArray(); }
            this.CommandPermissions = Permissions?.ToList() ?? new List<string>();
        }

        #endregion
        #region Type Permissions

        #region Summary

        /// <summary> Adding this attrubute to method means
        /// that it is a <see cref="CommandManagerDelegate"/>.
        /// <para> Add <see cref="CommandNames"/>
        /// to define <see cref="Command.Names"/>. </para>
        /// Add <see cref="CommandPermissions"/>
        /// to define <see cref="Command.Permissions"/>.
        /// <para> Add <see cref="HelpAttribute"/> attribute
        /// to define <see cref="Command.HelpText"/>.
        /// Or <see cref="HelpDescAttribute"/> to define
        /// <see cref="Command.HelpDesc"/>. </para>
        /// Add <see cref="DisallowServerAttribute"/> attribute
        /// to set <see cref="Command.AllowServer"/> to false.
        /// <para> Add <see cref="DoNotLogAttribute"/> attribute
        /// to set <see cref="Command.DoLog"/> to false. </para> 
        /// Names array and each its element must
        /// be not null and not empty. </summary>
        /// <exception cref="ArgumentException"></exception>

        #endregion
        public CommandInfoAttribute(Type PermissionContainer,
            int SubCommandParameterIndex, params string[] Names)
        {
            this.SubCommandParameterIndex =
                (SubCommandParameterIndex >= 0)
                    ? (SubCommandParameterIndex - 1)
                    : throw new ArgumentOutOfRangeException("SubCommandParameterIndex");
            string error = "Names array and each its " +
                "element must be not null and not empty.";
            this.CommandNames =
                Names?.Select(n => string.IsNullOrWhiteSpace(n)
                                    ? throw new ArgumentException(error)
                                    : n.ToLower())?
                                         .ToArray();
            if (Names?.Length == 0)
            { throw new ArgumentException(error); }
            else { Names = new List<string>(Names).ToArray(); }
            if (PermissionContainer != null)
            {
                string name = Names[0];
                FieldInfo fperm =
                    PermissionContainer
                        .GetFields()
                        .Where(f => ((f.Name.ToLower() == name)
                                   && AllowedPermissionTypes.Contains(f.FieldType)))
                        .FirstOrDefault();
                if (fperm != null)
                {
                    object p = fperm.GetValue(null);
                    if (p is string s)
                    { this.CommandPermissions = new List<string>() { s }; }
                    else if (p is List<string> l)
                    { this.CommandPermissions = new List<string>(l); }
                    else if (p is string[] a)
                    { this.CommandPermissions = new List<string>(a); }
                }
                else
                {
                    PropertyInfo pperm =
                        PermissionContainer
                            .GetProperties()
                            .Where(p => ((p.Name.ToLower() == name)
                                       && AllowedPermissionTypes.Contains(p.PropertyType)))
                            .FirstOrDefault();
                    if (pperm != null)
                    {
                        object p = pperm.GetValue(null);
                        if (p is string s)
                        { this.CommandPermissions = new List<string>() { s }; }
                        else if (p is List<string> l)
                        { this.CommandPermissions = new List<string>(l); }
                        else if (p is string[] a)
                        { this.CommandPermissions = new List<string>(a); }
                    }
                }
            }
            if (this.CommandPermissions == null)
            { this.CommandPermissions = new List<string>(); }
        }

        #endregion

        #region Summary

        /// <summary> Adding this attrubute to method means
        /// that it is a <see cref="CommandManagerDelegate"/>.
        /// <para> Add <see cref="CommandNames"/>
        /// to define <see cref="Command.Names"/>. </para>
        /// Add <see cref="CommandPermissions"/>
        /// to define <see cref="Command.Permissions"/>.
        /// <para> Add <see cref="HelpAttribute"/> attribute
        /// to define <see cref="Command.HelpText"/>.
        /// Or <see cref="HelpDescAttribute"/> to define
        /// <see cref="Command.HelpDesc"/>. </para>
        /// Add <see cref="DisallowServerAttribute"/> attribute
        /// to set <see cref="Command.AllowServer"/> to false.
        /// <para> Add <see cref="DoNotLogAttribute"/> attribute
        /// to set <see cref="Command.DoLog"/> to false. </para> 
        /// Names array and each its element must
        /// be not null and not empty. </summary>
        /// <exception cref="ArgumentException"></exception>

        #endregion
        public CommandInfoAttribute(string[] Permissions, params string[] Names)
            : this(Permissions, 0, Names) { }

        #region Summary

        /// <summary> Adding this attrubute to method means
        /// that it is a <see cref="CommandManagerDelegate"/>.
        /// <para> Add <see cref="CommandNames"/>
        /// to define <see cref="Command.Names"/>. </para>
        /// Add <see cref="CommandPermissions"/>
        /// to define <see cref="Command.Permissions"/>.
        /// <para> Add <see cref="HelpAttribute"/> attribute
        /// to define <see cref="Command.HelpText"/>.
        /// Or <see cref="HelpDescAttribute"/> to define
        /// <see cref="Command.HelpDesc"/>. </para>
        /// Add <see cref="DisallowServerAttribute"/> attribute
        /// to set <see cref="Command.AllowServer"/> to false.
        /// <para> Add <see cref="DoNotLogAttribute"/> attribute
        /// to set <see cref="Command.DoLog"/> to false. </para> 
        /// Names array and each its element must
        /// be not null and not empty. </summary>
        /// <exception cref="ArgumentException"></exception>

        #endregion
        public CommandInfoAttribute(string Permission,
            int SubCommandParameterIndex, params string[] Names)
            : this((string.IsNullOrWhiteSpace(Permission)
                        ? new string[0]
                        : new string[] { Permission }),
                          SubCommandParameterIndex, Names) { }
        #region Summary

        /// <summary> Adding this attrubute to method means
        /// that it is a <see cref="CommandManagerDelegate"/>.
        /// <para> Add <see cref="CommandNames"/>
        /// to define <see cref="Command.Names"/>. </para>
        /// Add <see cref="CommandPermissions"/>
        /// to define <see cref="Command.Permissions"/>.
        /// <para> Add <see cref="HelpAttribute"/> attribute
        /// to define <see cref="Command.HelpText"/>.
        /// Or <see cref="HelpDescAttribute"/> to define
        /// <see cref="Command.HelpDesc"/>. </para>
        /// Add <see cref="DisallowServerAttribute"/> attribute
        /// to set <see cref="Command.AllowServer"/> to false.
        /// <para> Add <see cref="DoNotLogAttribute"/> attribute
        /// to set <see cref="Command.DoLog"/> to false. </para> 
        /// Names array and each its element must
        /// be not null and not empty. </summary>
        /// <exception cref="ArgumentException"></exception>

        #endregion
        public CommandInfoAttribute(string Permission, params string[] Names)
            : this((string.IsNullOrWhiteSpace(Permission)
                        ? new string[0]
                        : new string[] { Permission }), 0, Names) { }

        #region Summary

        /// <summary> Adding this attrubute to method means
        /// that it is a <see cref="CommandManagerDelegate"/>.
        /// <para> Add <see cref="CommandNames"/>
        /// to define <see cref="Command.Names"/>. </para>
        /// Add <see cref="CommandPermissions"/>
        /// to define <see cref="Command.Permissions"/>.
        /// <para> Add <see cref="HelpAttribute"/> attribute
        /// to define <see cref="Command.HelpText"/>.
        /// Or <see cref="HelpDescAttribute"/> to define
        /// <see cref="Command.HelpDesc"/>. </para>
        /// Add <see cref="DisallowServerAttribute"/> attribute
        /// to set <see cref="Command.AllowServer"/> to false.
        /// <para> Add <see cref="DoNotLogAttribute"/> attribute
        /// to set <see cref="Command.DoLog"/> to false. </para> 
        /// Names array and each its element must
        /// be not null and not empty. </summary>
        /// <exception cref="ArgumentException"></exception>

        #endregion
        public CommandInfoAttribute(Type PermissionContainer, params string[] Names)
            : this(PermissionContainer, 0, Names) { }

        #endregion
    }
}