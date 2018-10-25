#region Using
using System;
using TShockAPI;
#endregion
namespace CommandManager
{
    /// <summary> Adding this attribute to
    /// command delegate means that it will
    /// not be automatically added to
    /// <see cref="Commands.ChatCommands"/>. </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class DoNotRegisterAttribute : Attribute { }
}