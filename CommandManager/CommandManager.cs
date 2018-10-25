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

        #region RegisterPlugin

        /// <summary> Registers all <see cref="CommandManagerDelegate"/>
        /// with <see cref="CommandInfoAttribute"/>. </summary>
        public static string[] Register(TerrariaPlugin Plugin,
            bool SkipIfHasNotRegisterAttribute = false)
        {
            List<string> names = new List<string>();
            MethodInfo[] methods = Assembly.GetAssembly(Plugin.GetType())
                                           .GetTypes()
                                           .SelectMany(t => t.GetMethods())
                                           .Where(m => m.IsCMDelegate())
                                           .ToArray();
            
            foreach (MethodInfo method in methods)
            {
                if (Register(method, SkipIfHasNotRegisterAttribute,
                    out string name))
                { names.Add(name); }
            }
            return names.ToArray();
        }

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

        #region RegisterCommand

        /// <summary> Adds command in
        /// <see cref="Commands.ChatCommands"/> using
        /// information from method attributes. </summary>
        public static bool Register(CommandManagerDelegate Command,
            bool SkipIfHasDoNotRegisterAttribute = false) =>
            Register(Command.Method,
                SkipIfHasDoNotRegisterAttribute, out string Name);

        #endregion
        #region DeregisterCommand

        /// <summary> Deregisters command if it
        /// was registered before. </summary>
        public static bool Deregister(CommandManagerDelegate Command) =>
            Deregister(Command.Method);

        #endregion
        
        #region RegisterMethod

        private static bool Register(MethodInfo Method,
            bool SkipIfHasDoNotRegisterAttribute, out string Name)
        {
            Command cmd = Creater.Create(Method,
                SkipIfHasDoNotRegisterAttribute, out List<Command> Remove);
            Name = cmd?.Name;
            if (cmd == null) { return false; }
            foreach (Command c in Remove.Reverse<Command>())
            { Commands.ChatCommands.Remove(c); }
            Commands.ChatCommands.Add(cmd);
            return true;
        }

        #endregion        
        #region DeregisterMethod

        private static bool Deregister(MethodInfo Method)
        {
            foreach (SubCommandAttribute a in
                Method.GetCustomAttributes(typeof(SubCommandAttribute)))
            {
                Deregister(a.Method);
                SubCommandAttribute.SubCommands.Remove(a.Method);
            }

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
    }
}