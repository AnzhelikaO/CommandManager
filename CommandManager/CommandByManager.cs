#region Using
using System.Collections.Generic;
using System.Reflection;
using TShockAPI;
#endregion
namespace CommandManager
{
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
}