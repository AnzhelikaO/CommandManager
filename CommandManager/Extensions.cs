#region Using
using System.Collections.Generic;
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