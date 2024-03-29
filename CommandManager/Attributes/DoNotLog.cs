﻿#region Using
using System;
using TShockAPI;
#endregion
namespace CommandManager
{
    /// <summary> Will work only with
    /// <see cref="CommandInfoAttribute"/> attribute.
    /// <para> Adding this attribute to command delegate
    /// sets <see cref="Command.DoLog"/>
    /// to false. </para> </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class DoNotLogAttribute : Attribute { }
}