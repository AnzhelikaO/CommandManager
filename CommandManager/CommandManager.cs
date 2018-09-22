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
        #region GetAllCommandMethodsWithOrWithoutTokens

        private static List<Tuple<MethodInfo, string>>
            GetAllCommandMethodsWithOrWithoutTokens
            (TerrariaPlugin Plugin, string[] Tokens)
        {
            List<Tuple<MethodInfo, string>> cmds =
                new List<Tuple<MethodInfo, string>>();
            bool checkTokens = (Tokens?.Length > 0);
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
                        || !attributes.Any(a => a.GetType() == typeof(CommandInfo)))
                    { continue; }
                    if (checkTokens)
                    {
                        string[] tokens = attributes.Where(a => (a is Token))
                                                    .Select(a => ((Token)a).CommandToken)
                                                    .ToArray();
                        string matched = tokens.FirstOrDefault(t => Tokens.Contains(t));
                        if (matched != null)
                        { cmds.Add(new Tuple<MethodInfo, string>(method, matched)); }
                    }
                    else if (!attributes.Any(a => a.GetType() == typeof(Token)))
                    { cmds.Add(new Tuple<MethodInfo, string>(method, null)); }
                }
            }
            return cmds;
        }

        #endregion
        #region GetAllCommandMethods

        #region Summary

        /// <summary> Gets all public static methods of Plugin's
        /// assembly with <see cref="CommandInfo"/> attribute
        /// and without <see cref="DoNotRegister"/> and
        /// <see cref="Token"/> attributes that are type
        /// of <see cref="CommandDelegate"/>. </summary>
        /// <param name="Plugin"> Plugin instance. </param>

        #endregion
        public static List<MethodInfo> GetAllCommandMethods
            (TerrariaPlugin Plugin) =>
            GetAllCommandMethodsWithOrWithoutTokens(Plugin, null)
                .Select(m => m.Item1).ToList();

        #region Summary

        /// <summary> Gets all public static methods of Plugin's
        /// assembly with <see cref="CommandInfo"/> and matching
        /// <see cref="Token"/> attributes
        /// and without <see cref="DoNotRegister"/> attribute
        /// of <see cref="CommandDelegate"/>. </summary>
        /// <param name="Plugin"> Plugin instance. </param>
        /// <param name="Tokens"> Command method must
        /// contain one of these tokens. </param>

        #endregion
        public static List<Tuple<MethodInfo, string>> GetAllCommandMethods
            (TerrariaPlugin Plugin, params string[] Tokens) =>
            GetAllCommandMethodsWithOrWithoutTokens(Plugin, Tokens);

        #endregion
        #region LoadAllWithOrWithoutTokens

        private static void LoadAllWithOrWithoutTokens
            (TerrariaPlugin Plugin, string[] Tokens)
        {
            foreach (Tuple<MethodInfo, string> methodInfo in
                GetAllCommandMethodsWithOrWithoutTokens(Plugin, Tokens))
            {
                MethodInfo method = methodInfo.Item1;
                List<Attribute> attributes = method.GetCustomAttributes().ToList();
                CommandInfo info = method.GetCustomAttribute<CommandInfo>();

                string[] names = info.CommandNames;
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
                if (info.CommandPermissions?.Count == 0)
                {
                    info.CommandPermissions =
                        (savedC?.Permissions ?? new List<string>());
                }
                CommandByManager cmd = new CommandByManager
                    (Plugin.GetType(), saved, methodInfo.Item2,
                    info.CommandPermissions,
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
                    if (!hDesc && (savedC.HelpDesc?.Length > 0))
                    { cmd.HelpDesc = savedC.HelpDesc; }
                    if (!server) { cmd.AllowServer = savedC.AllowServer; }
                    if (!log) { cmd.DoLog = savedC.DoLog; }
                }

                #endregion
                Commands.ChatCommands.Add(cmd);
            }
        }

        #endregion
        #region LoadAll

        #region Summary

        /// <summary> Registers all methods found in
        /// <see cref="GetAllCommandMethods(TerrariaPlugin)"/>.
        /// </summary> <param name="Plugin"> Plugin instance. </param>

        #endregion
        public static void LoadAll(TerrariaPlugin Plugin) =>
            LoadAllWithOrWithoutTokens(Plugin, null);

        #region Summary

        /// <summary> Registers all methods found in
        /// <see cref="GetAllCommandMethods(TerrariaPlugin, string[])"/>.
        /// </summary> <param name="Plugin"> Plugin instance. </param>
        /// <param name="Tokens"> Command method must
        /// contain one of these tokens. </param>

        #endregion
        public static void LoadAll(TerrariaPlugin Plugin,
            params string[] Tokens) =>
            LoadAllWithOrWithoutTokens(Plugin, Tokens);

        #endregion
        #region UnloadAllWithOrWithoutTokens

        private static void UnloadAllWithOrWithoutTokens
            (TerrariaPlugin Plugin, string[] Tokens)
        {
            for (int i = Commands.ChatCommands.Count - 1; i >= 0; i--)
            {
                Command command = Commands.ChatCommands[i];
                if ((command is CommandByManager cmd)
                    && (cmd.PluginType == Plugin.GetType())
                    && (Tokens?.Contains(cmd.Token) ?? true))
                {
                    Commands.ChatCommands.RemoveAt(i);
                    Commands.ChatCommands.AddRange(cmd.Saved);
                }
            }
        }

        #endregion
        #region UnloadAll

        #region Summary

        /// <summary> Deletes all commands that were registered
        /// by <see cref="LoadAll(TerrariaPlugin)"/>.
        /// </summary> <param name="Plugin"> Plugin instance. </param>

        #endregion
        public static void UnloadAll(TerrariaPlugin Plugin) =>
            UnloadAllWithOrWithoutTokens(Plugin, null);

        #region Summary

        /// <summary> Deletes all commands that were registered
        /// by <see cref="LoadAll(TerrariaPlugin, string[])"/>.
        /// </summary> <param name="Plugin"> Plugin instance. </param>
        /// <param name="Tokens"> Command method must
        /// contain one of these tokens. </param>

        #endregion
        public static void UnloadAll(TerrariaPlugin Plugin,
            params string[] Tokens) =>
            UnloadAllWithOrWithoutTokens(Plugin, Tokens);

        #endregion
        #region CommandByManager

        internal class CommandByManager : Command
        {
            public Type PluginType { get; }
            public string Token { get; }
            public List<Command> Saved { get; }
            public CommandByManager(Type PluginType,
                List<Command> Saved, string Token,
                List<string> permissions, CommandDelegate cmd,
                params string[] names) : base(permissions, cmd, names)
            {
                this.PluginType = PluginType;
                this.Saved = (Saved ?? new List<Command>());
                this.Token = Token;
            }
        }

        #endregion
    }
}