#region Using
using System;
using TShockAPI;
#endregion
namespace CommandManager
{
    /// <summary> Will work only with
    /// <see cref="CommandInfoAttribute"/> attribute.
    /// <para> Adding this attribute to command delegate
    /// sets <see cref="Command.AllowServer"/>
    /// to false. </para> </summary>
    public class DisallowServerAttribute : Attribute { }
}