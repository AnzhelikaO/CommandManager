#region Using
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Terraria;
using TerrariaApi.Server;
using TShockAPI;
#endregion
namespace CommandManager
{
    /// <summary> Allows <see cref="CommandInfoAttribute"/>
    /// and other command attributes. </summary>
    public delegate void CommandManagerDelegate(CommandManagerArgs args);
    /// <summary> Load/unload commands. </summary>
    [ApiVersion(2, 1)]
    public partial class CommandManager : TerrariaPlugin
    {
        #region Plugin
        #pragma warning disable 1591

        public override string Name => "CommandManager";
        public override Version Version =>
            Assembly.GetExecutingAssembly().GetName().Version;
        public override string Author => "Anzhelika";
        public override string Description =>
            "Autoregister commands with specific attributes.";
        public CommandManager(Main game) : base(game) { }
        public override void Initialize() =>
            ServerApi.Hooks.GamePostInitialize.Register(this, OnPostInit, int.MinValue);
        void OnPostInit(EventArgs args) => RegisterAll(ServerApi.Plugins.ToArray());
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            { ServerApi.Hooks.GamePostInitialize.Deregister(this, OnPostInit); }
            base.Dispose(disposing);
        }

        #pragma warning restore 1591
        #endregion
        #region RegisterAll

        private void RegisterAll(PluginContainer[] Plugins)
        {
            string format = "[CommandManager] Autoregistered " +
                "{0} commands in plugin '{1}':\n{2}";
            foreach (PluginContainer Plugin in Plugins)
            {
                string[] names = Register(Plugin.Plugin, true);
                if (names.Length > 0)
                {
                    TShock.Log.ConsoleInfo(format,
                        names.Length, Plugin.Plugin.Name,
                        string.Join("\n", PaginationTools.BuildLinesFromTerms(names)));
                }
            }
        }

        #endregion
        #region RegisterMethod

        private static bool Register(MethodInfo Method,
            bool SkipIfHasDoNotRegisterAttribute, out string Name)
        {
            #region Check if static, public, has CommandInfoAttribute

            Name = null;
            if (!Method.IsStatic || !Method.IsPublic) { return false; }
            List<Attribute> attributes = Method.GetCustomAttributes().ToList();
            if (!attributes.Any(a => (a is CommandInfoAttribute))
                || (SkipIfHasDoNotRegisterAttribute
                        ? attributes.Any(a => (a is DoNotRegisterAttribute))
                        : false))
            { return false; }

            #endregion
            #region DeleteOldMethod

            int old = Commands.ChatCommands.FindIndex
                (c => ((c is CommandByManager c2) && (c2.MethodInfo == Method)));
            if (old != -1) { Commands.ChatCommands.RemoveAt(old); }

            #endregion
            #region ReadNames

            CommandInfoAttribute info =
                Method.GetCustomAttribute<CommandInfoAttribute>();
            string[] names = info.CommandNames;
            Name = names[0];

            #endregion
            #region FindCommandsWithSameNames

            List<Command> found = Commands.ChatCommands.Where(c =>
                c.Names.Any(n => names.Contains(n))).ToList();
            Command savedC = found.FirstOrDefault();
            bool replace = attributes.Any(a => (a is ReplaceIfExistsAttribute));
            List<Command> saved = new List<Command>();
            if (replace)
            {
                if (info.CommandPermissions?.Count == 0)
                {
                    info.CommandPermissions =
                        (savedC?.Permissions ?? new List<string>());
                }
                foreach (Command c in found)
                {
                    saved.Add(c.Clone());
                    Commands.ChatCommands.Remove(c);
                }
            }

            #endregion
            #region ParameterTypes

            ParameterTypesAttribute @params =
                (ParameterTypesAttribute)attributes.FirstOrDefault(a =>
                    (a is ParameterTypesAttribute));
            CommandDelegate @delegate;

            #region Without ParameterTypesAttribute

            if (@params == null)
            {
                @delegate = (args =>
                {
                    Dictionary<string, Parameter[]> parameters =
                        new Dictionary<string, Parameter[]>();
                    for (int i = 0; i < args.Parameters.Count; i++)
                    {
                        parameters.Add(i.ToString(),
                            new Parameter[] { new Parameter(args.Parameters[i], null) });
                    }

                    Method.Invoke(null,
                        new object[]
                        {
                                new CommandManagerArgs
                                (
                                    args.Player,
                                    args.Message,
                                    args.Silent,
                                    args.Parameters,
                                    parameters
                                )
                        });
                });
            }

            #endregion
            #region With ParameterTypesAttribute

            else
            {
                @delegate = (args =>
                {
                    if ((@params.RequiredParametersCount != -1)
                        && (args.Parameters.Count < @params.RequiredParametersCount))
                    {
                        args.Player.SendErrorMessage
                        (@params.CreateErrorMessage(names[0]));
                        return;
                    }

                    Dictionary<string, Parameter[]> parameters =
                        new Dictionary<string, Parameter[]>();
                    for (int i = 0; i < @params.ParameterTypes.Length; i++)
                    {
                        ParameterInfo p = @params.ParameterTypes[i];
                        if ((i < args.Parameters.Count) && (p != null))
                        {
                            if (p.AllowMergeInErrorMessage.HasValue)
                            {
                                if (!p.Parse(args.Parameters.Skip(i),
                                    out ParameterParseResult[] result))
                                {
                                    args.Player.SendErrorMessage(result.FirstOrDefault
                                        (r => (r.Error != null)).Error);
                                    return;
                                }
                                parameters.Add(p.Name, result.Select(r =>
                                    new Parameter(r.Input, r.Output)).ToArray());
                            }
                            else if (!p.Parse(args.Parameters[i],
                                out ParameterParseResult result))
                            {
                                args.Player.SendErrorMessage(result.Error);
                                return;
                            }
                            else
                            {
                                Parameter param = new Parameter(result.Input, result.Output);
                                parameters.Add(p.Name, new Parameter[] { param });
                            }
                        }
                        else
                        {
                            Parameter param = new Parameter
                                (args.Parameters.ElementAtOrDefault(i), null);
                            parameters.Add(p.Name, new Parameter[] { param });
                        }
                    }

                    Method.Invoke(null,
                        new object[]
                        {
                                new CommandManagerArgs
                                (
                                    args.Player,
                                    args.Message,
                                    args.Silent,
                                    args.Parameters,
                                    parameters
                                )
                        });
                });
            }

            #endregion

            #endregion
            CommandByManager cmd = new CommandByManager(Method,
                saved, info.CommandPermissions, @delegate, names);
            #region Help, HelpDesc, DisallowServer, DoNotLog attributes

            bool hText = false, hDesc = false, server = false, log = false;
            foreach (Attribute a in attributes)
            {
                if (a is HelpAttribute h)
                {
                    hText = true;
                    cmd.HelpText = h.CommandHelp;
                }
                else if (a is HelpDescAttribute d)
                {
                    hDesc = true;
                    cmd.HelpDesc = d.CommandHelpDesc;
                }
                else if (a is DisallowServerAttribute)
                {
                    server = true;
                    cmd.AllowServer = false;
                }
                else if (a is DoNotLogAttribute)
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
            return true;
        }

        #endregion
        #region RegisterCommand

        /// <summary> Adds command in
        /// <see cref="Commands.ChatCommands"/> using
        /// information from method attributes. </summary>
        public static bool Register(CommandManagerDelegate Command,
            bool SkipIfHasDoNotRegisterAttribute = false) =>
            Register(Command.Method,
                SkipIfHasDoNotRegisterAttribute, out string Name);

        #endregion
        #region RegisterPlugin

        /// <summary> Registers all <see cref="CommandManagerDelegate"/>
        /// with <see cref="CommandInfoAttribute"/>. </summary>
        public static string[] Register(TerrariaPlugin Plugin,
            bool SkipIfHasNotRegisterAttribute = false)
        {
            List<string> names = new List<string>();
            foreach (Type type in Assembly.GetAssembly(Plugin.GetType()).GetTypes())
            {
                foreach (MethodInfo method in type.GetMethods())
                {
                    if (Register(method, SkipIfHasNotRegisterAttribute,
                        out string name))
                    { names.Add(name); }
                }
            }
            return names.ToArray();
        }

        #endregion
        #region DeregisterMethod

        private static bool Deregister(MethodInfo Method)
        {
            int index = Commands.ChatCommands.FindIndex
                (c => ((c is CommandByManager c2) && (c2.MethodInfo == Method)));
            if (index != -1)
            {
                CommandByManager found = (CommandByManager)Commands.ChatCommands[index];
                if (found.Saved?.Count > 0)
                { Commands.ChatCommands.AddRange(found.Saved.Where(c => (c != null))); }
                Commands.ChatCommands.Remove(found);
            }
            return (index != -1);
        }

        #endregion
        #region DeregisterCommand

        /// <summary> Deregisters command if it
        /// was registered before. </summary>
        public static bool Deregister(CommandManagerDelegate Command) =>
            Deregister(Command.Method);

        #endregion
        #region DeregisterPlugin

        /// <summary> Deregisters all automatically
        /// and mainly registered commands. </summary>
        public static void Deregister(TerrariaPlugin Plugin)
        {
            int count = 0;
            foreach (Type type in Assembly.GetAssembly(Plugin.GetType()).GetTypes())
            {
                foreach (MethodInfo method in type.GetMethods())
                { if (Deregister(method)) { count++; } }
            }
            if (count > 0)
            {
                TShock.Log.ConsoleInfo($"[CommandManager] Deregistered " +
                    $"{count} commands in plugin '{Plugin.Name}'.");
            }
        }

        #endregion
        #region CommandByManager

        internal class CommandByManager : Command
        {
            public MethodInfo MethodInfo { get; }
            public List<Command> Saved { get; }
            public CommandByManager(MethodInfo MethodInfo,
                List<Command> Saved, List<string> permissions,
                CommandDelegate cmd, params string[] names)
                : base(permissions, cmd, names)
            {
                this.MethodInfo = MethodInfo;
                this.Saved = Saved;
            }
        }

        #endregion
    }
}