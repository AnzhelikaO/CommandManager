#region Using
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TerrariaApi.Server;
using TShockAPI;
#endregion
namespace CommandManager
{
    /// <summary> Load/unload commands. </summary>
    public class CommandManager
    {
        #region GetAllCommandDelegates

        #region Summary

        /// <summary> Gets all methods of Plugin's assembly
        /// with <see cref="IsCommand"/> and <see cref="Names"/>
        /// attributes and without <see cref="DoNotRegister"/>
        /// attribute that matches Flags and are type of
        /// <see cref="CommandDelegate"/>. </summary>
        
        #endregion
        public static List<MethodInfo> GetAllCommandDelegates(TerrariaPlugin Plugin)
        {
            List<MethodInfo> cmds = new List<MethodInfo>();
            foreach (Type type in Assembly.GetAssembly(Plugin.GetType()).GetTypes())
            {
                foreach (MethodInfo method in type.GetMethods
                    (BindingFlags.Public | BindingFlags.Static))
                {
                    ParameterInfo[] @params = method?.GetParameters();
                    if ((@params?.Length != 1)
                        || (@params[0].ParameterType != typeof(CommandArgs))
                        || (method.ReturnType != typeof(void)))
                    { continue; }
                    IEnumerable<Attribute> attributes = method.GetCustomAttributes();
                    if (attributes.Any(a => (a.GetType() == typeof(DoNotRegister)))
                        || !attributes.Any(a => a.GetType() == typeof(IsCommand))
                        || !attributes.Any(a => a.GetType() == typeof(Names)))
                    { continue; }
                    cmds.Add(method);
                }
            }
            return cmds;
        }

        #endregion
        #region LoadAll

        /// <summary> Registers all methods found in
        /// <see cref="GetAllCommandDelegates(TerrariaPlugin)"/>.
        /// </summary>
        public static void LoadAll(TerrariaPlugin Plugin)
        {
            foreach (MethodInfo method in GetAllCommandDelegates(Plugin))
            {
                List<Attribute> attributes = method.GetCustomAttributes().ToList();
                Permissions permissions = (Permissions)attributes.FirstOrDefault
                    (a => (a is Permissions)) ?? new Permissions();
                CommandByManager cmd = new CommandByManager
                    (Plugin.GetType(), permissions.CommandPermissions,
                    (CommandDelegate)Delegate.CreateDelegate
                    (typeof(CommandDelegate), method),
                    method.GetCustomAttribute<Names>().CommandNames);
                foreach (Attribute a in attributes)
                {
                    if (a is Help h)
                    { cmd.HelpText = h.CommandHelp; }
                    else if (a is HelpDesc d)
                    { cmd.HelpDesc = d.CommandHelpDesc; }
                    else if (a is DisallowServer)
                    { cmd.AllowServer = false; }
                    else if (a is DoNotLog)
                    { cmd.DoLog = false; }
                }
                Console.WriteLine(cmd.CommandDelegate?.Method?.Name);
                Console.WriteLine(cmd.Name);
                Console.WriteLine(cmd.Permissions.FirstOrDefault());
                Commands.ChatCommands.Add(cmd);
                Console.WriteLine(Commands.ChatCommands.Last().CommandDelegate?.Method?.Name);
                Console.WriteLine(Commands.ChatCommands.Last().Name);
                Console.WriteLine(Commands.ChatCommands.Last().Permissions.FirstOrDefault());
            }
        }

        #endregion
        #region UnloadAll

        /// <summary> Deletes all commands that were registered
        /// by <see cref="LoadAll(TerrariaPlugin)"/>.
        /// </summary>
        public static void UnloadAll(TerrariaPlugin Plugin)
        {
            for (int i = Commands.ChatCommands.Count - 1; i >= 0; i--)
            {
                Command command = Commands.ChatCommands[i];
                if ((command is CommandByManager cmd)
                    && (cmd.PluginType == Plugin.GetType()))
                { Commands.ChatCommands.RemoveAt(i); }
            }
        }

        #endregion
        #region CommandByManager

        internal class CommandByManager : Command
        {
            public Type PluginType { get; }
            public CommandByManager(Type PluginType,
                List<string> permissions, CommandDelegate cmd,
                params string[] names)
                : base(permissions, cmd, names)
            { this.PluginType = PluginType; }
        }

        #endregion
    }
}