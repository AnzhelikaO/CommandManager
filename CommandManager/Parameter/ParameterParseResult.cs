namespace CommandManager
{
    /// <summary> Temporary data storage between
    /// <see cref="CommandManager.Parsers"/> and
    /// <see cref="CommandManagerArgs.ParsedParameters"/>. </summary>
    public class ParameterParseResult
    {
        /// <summary> <see cref="CommandManagerArgs.Parameters"/>[i]. </summary>
        public string Input { get; }
        /// <summary> Parsed value (by one functions in
        /// <see cref="CommandManager.Parsers"/>). </summary>
        public object Output { get; }
        /// <summary> If true, <see cref="Input"/> was successfully
        /// parsed, <see cref="Output"/> has value and
        /// <see cref="Error"/> is null. </summary>
        public bool Success { get; }
        /// <summary> Message that will be shown to command
        /// caller if parse was not successful. </summary>
        public string Error { get; internal set; }
        #region Constructor

        /// <summary> Temporary data storage between
        /// <see cref="CommandManager.Parsers"/> and
        /// <see cref="CommandManagerArgs.ParsedParameters"/>. </summary>
        public ParameterParseResult(string Input,
            object Output, bool Success, string Error)
        {
            this.Input = Input;
            this.Output = Output;
            this.Success = Success;
            this.Error = Error;
        }

        #endregion
    }
}