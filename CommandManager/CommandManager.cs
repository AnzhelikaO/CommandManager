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
                string[] names = method.GetCustomAttribute<Names>().CommandNames;
                bool replace = !attributes.Any(a => (a is DoNotReplaceIfNameExists));
                List<Command> saved = new List<Command>();
                #region RemovedMatched

                if (replace)
                {
                    for (int i = Commands.ChatCommands.Count - 1; i >= 0; i--)
                    {
                        Command c = Commands.ChatCommands[i];
                        if (c.Names.Any(n => names.Contains(n)))
                        {
                            saved.Add(c.Clone());
                            Commands.ChatCommands.RemoveAt(i);
                        }
                    }
                }

                #endregion
                Command savedC = saved.FirstOrDefault();
                Permissions permissions =
                    (Permissions)attributes.FirstOrDefault(a => (a is Permissions));
                if (permissions == null)
                {
                    permissions =
                        new Permissions((savedC?.Permissions
                            ?? new List<string>()).ToArray());
                }
                CommandByManager cmd = new CommandByManager
                    (Plugin.GetType(), saved, permissions.CommandPermissions,
                    (CommandDelegate)Delegate.CreateDelegate
                    (typeof(CommandDelegate), method), names);
                #region Help, HelpDesc, DisallowServer, DoNotLog attributes

                bool hText = false, hDesc = false, server = false, log = false;
                foreach (Attribute a in attributes)
                {
                    if (a is Help h)
                    {
                        hText = true;
                        cmd.HelpText = h.CommandHelp;
                    }
                    else if (a is HelpDesc d)
                    {
                        hDesc = true;
                        cmd.HelpDesc = d.CommandHelpDesc;
                    }
                    else if (a is DisallowServer)
                    {
                        server = true;
                        cmd.AllowServer = false;
                    }
                    else if (a is DoNotLog)
                    {
                        log = true;
                        cmd.DoLog = false;
                    }
                }

                #endregion
                #region RestoreSomeData

                if (replace && (savedC != null))
                {
                    if (!hText && (savedC.HelpText != null))
                    { cmd.HelpText = savedC.HelpText; }
                    if (!hDesc && (savedC.HelpDesc != null))
                    { cmd.HelpText = savedC.HelpText; }
                    if (!server)
                    { cmd.AllowServer = savedC.AllowServer; }
                    if (!log)
                    { cmd.DoLog = savedC.DoLog; }
                }

                #endregion
                Commands.ChatCommands.Add(cmd);
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
                {
                    Commands.ChatCommands.RemoveAt(i);
                    Commands.ChatCommands.AddRange(cmd.Saved);
                }
            }
        }

        #endregion
        #region CommandByManager

        internal class CommandByManager : Command
        {
            public Type PluginType { get; }
            public List<Command> Saved { get; }
            public CommandByManager(Type PluginType,
                List<Command> Saved, List<string> permissions,
                CommandDelegate cmd, params string[] names)
                : base(permissions, cmd, names)
            {
                this.PluginType = PluginType;
                this.Saved = (Saved ?? new List<Command>());
            }
        }

        #endregion
    }
}