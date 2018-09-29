using System;
namespace CommandManager
{
    /// <summary> Contains data with unparsed (input string)
    /// and parsed (by specified <see cref="Type"/> in
    /// <see cref="ParameterTypesAttribute"/>) objects. </summary>
    public class Parameter
    {
        /// <summary> Player's input. </summary>
        public string Original { get; }
        /// <summary> Parsed (by specified <see cref="Type"/> in
        /// <see cref="ParameterTypesAttribute"/>) object. </summary>
        public object Parsed { get; }
        /// <summary> True, if <see cref="Parsed"/>
        /// is not null. </summary>
        public bool HasParsed => (Parsed != null);
        #region Constructor

        internal Parameter(string Original, object Parsed)
        {
            this.Original = Original;
            this.Parsed = Parsed;
        }

        #endregion
        /// <summary> Returns true, if <see cref="Parsed"/>
        /// is not null and value type matches. </summary>
        public bool Get<T>(out T Value)
        {
            Value = default(T);
            if (!HasParsed || (Parsed.GetType() != typeof(T)))
            { return false; }
            Value = (T)Parsed;
            return true;
        }
    }
}