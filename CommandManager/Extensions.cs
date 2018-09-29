#region Using
using System.Collections.Generic;
using System.Linq;
using TShockAPI;
#pragma warning disable 1591
#endregion
namespace CommandManager
{
    public static class Extensions
    {
        public static Command Clone(this Command Command)
        {
            if (Command == null) { return null; }
            if (Command is CommandManager.CommandByManager cmd)
            {
                return new CommandManager.CommandByManager
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
    }
}