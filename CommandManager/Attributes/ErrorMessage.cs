using System;
namespace CommandManager
{
    #region Summary

    /// <summary> Will work only with
    /// <see cref="CommandInfoAttribute"/> attribute.
    /// <para> If not null, message overrides automatically 
    /// created syntax error message. </para> </summary>
    
    #endregion
    public class ErrorMessageAttribute : Attribute
    {
        #region Summary

        /// <summary> If not null and not empty, this
        /// message will be shown if player's input doesn't
        /// match syntax of the command. </summary>

        #endregion
        public string Message { get; }
        #region Summary

        /// <summary> Will work only with
        /// <see cref="CommandInfoAttribute"/> attribute.
        /// <para> If not null, message overrides automatically 
        /// created syntax error message. </para> </summary>

        #endregion
        public ErrorMessageAttribute(string Message) => this.Message = Message;
    }
}