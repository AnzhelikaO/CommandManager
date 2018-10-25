#region Using
using System;
using TShockAPI;
#endregion
namespace CommandManager
{
    /// <summary> Will work only with
    /// <see cref="CommandInfoAttribute"/> attribute.
    /// <para> Adding this attribute to command delegate means
    /// that if any of <see cref="Commands.ChatCommands"/> already contain
    /// one of <see cref="CommandInfoAttribute.CommandNames"/>, this command
    /// will be saved and replaced by current command method. </para>
    /// Leave <see cref="CommandInfoAttribute.CommandPermissions"/> null to
    /// copy permission from saved command.
    /// <para> Other properties will be also copied if suitable attribute
    /// was not found. </para>
    /// Other methods can also potentially replace your command, leaving
    /// saved commands deep in recursive tree. </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class ReplaceIfExistsAttribute : Attribute { }
}