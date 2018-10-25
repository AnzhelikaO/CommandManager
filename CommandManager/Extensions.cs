#region Using
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TShockAPI;
#endregion
namespace CommandManager
{
    /// <summary> <see cref="Clone(Command)"/>,
    /// <see cref="IsCMDelegate(MethodInfo)"/>. </summary>
    public static class Extensions
    {
        #region Clone

        /// <summary> Returns deep copy of Command. </summary>
        public static Command Clone(this Command Command)
        {
            if (Command == null) { return null; }
            if (Command is CommandByManager cmd)
            {
                return new CommandByManager
                (
                    cmd.MethodInfo,
                    new List<Command>(cmd.Saved.Select(c => c.Clone())),
                    ((Command.Permissions == null)
                        ? new List<string>()
                        : new List<string>(Command.Permissions)),
                    Command?.CommandDelegate,
                    new List<string>(Command.Names).ToArray()
                )
                {
                    AllowServer = Command.AllowServer,
                    DoLog = Command.DoLog,
                    HelpDesc = ((Command.HelpDesc == null)
                                    ? null
                                    : new List<string>(Command.HelpDesc).ToArray()),
                    HelpText = Command.HelpText
                };
            }
            return new Command
            (
                ((Command.Permissions == null)
                    ? new List<string>()
                    : new List<string>(Command.Permissions)),
                Command?.CommandDelegate,
                new List<string>(Command.Names).ToArray()
            )
            {
                AllowServer = Command.AllowServer,
                DoLog = Command.DoLog,
                HelpDesc = ((Command.HelpDesc == null)
                                ? null
                                : new List<string>(Command.HelpDesc).ToArray()),
                HelpText = Command.HelpText
            };
        }

        #endregion
        #region IsCommandManagerDelegate

        #region Summary

        /// <summary> Method is not null.
        /// <para> Return type is void. </para>
        /// Method is static.
        /// <para> Method is public. </para>
        /// Parameter count is 1.
        /// <para> Parameter is <see cref="CommandManagerArgs"/>. </para> </summary>

        #endregion
        public static bool IsCMDelegate(this MethodInfo MethodInfo) =>
           MethodInfo.IsCMDelegate(MethodInfo.Name, null, out string e);

        #region Summary

        /// <summary> Method is not null.
        /// <para> Return type is void. </para>
        /// Method is static.
        /// <para> Method is public. </para>
        /// Parameter count is 1.
        /// <para> Parameter is <see cref="CommandManagerArgs"/>. </para>
        /// If <paramref name="HasCommandInfoAttribute"/> is null,
        /// availability of <see cref="CommandInfoAttribute"/> doesn't matter.
        /// <para> Else, method must (if true) or must not
        /// (if false) have this attribute. </para> </summary>

        #endregion
        public static bool IsCMDelegate(this MethodInfo MethodInfo,
            string ExceptionName, bool? HasCommandInfoAttribute, out string Exception)
        {
            if ((MethodInfo == null)
                || (MethodInfo.ReturnType != typeof(void))
                || (MethodInfo.GetParameters().Length != 1)
                || (MethodInfo.GetParameters()[0].ParameterType != typeof(CommandManagerArgs)))
            {
                Exception = ExceptionName + " must be method of CommandManagerDelegate.";
                return false;
            }
            if (!MethodInfo.IsStatic)
            {
                Exception = ExceptionName + " must be static.";
                return false;
            }
            if (!MethodInfo.IsPublic)
            {
                Exception = ExceptionName + " must be public.";
                return false;
            }
            if (HasCommandInfoAttribute != null)
            {
                bool cInfo = MethodInfo.GetCustomAttributes
                    (typeof(CommandInfoAttribute)).Any();
                if (cInfo != HasCommandInfoAttribute)
                {
                    Exception = $"{ExceptionName} must {(cInfo ? "not " : "")}" +
                        "have CommandInfoAttribute.";
                    return false;
                }
            }
            Exception = null;
            return true;
        }

        #endregion
    }
}