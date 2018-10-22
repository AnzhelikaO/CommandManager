#region Using
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using TShockAPI;
#endregion
namespace CommandManager
{
    /// <summary> Contains command caller,
    /// message and parameters. </summary>
    public class CommandManagerArgs
    {
        /// <summary> Original command args. </summary>
        public CommandArgs Args { get; }
        /// <summary> Command caller. </summary>
        public TSPlayer Player => Args.Player;
        /// <summary> Command input. </summary>
        public string Message => Args.Message;
        /// <summary> True, if <see cref="ConfigFile.CommandSilentSpecifier"/>. </summary>
        public bool Silent => Args.Silent;
        /// <summary> Unparsed array, splitted by spaces and quotes. </summary>
        public List<string> Parameters => Args.Parameters;
        /// <summary> Collection of parameters, both original and parsed
        /// by <see cref="ParameterTypesAttribute"/>. </summary>
        public ReadOnlyDictionary<string, Parameter[]> ParsedParameters { get; }
        #region Constructor

        internal CommandManagerArgs(CommandArgs CommandArgs,
            Dictionary<string, Parameter[]> ParsedParameters)
        {
            this.Args = CommandArgs;
            this.ParsedParameters =
                new ReadOnlyDictionary<string, Parameter[]>(ParsedParameters);
        }

        #endregion
        #region GetPlayerInput

        /// <summary> Returns player input. </summary>
        public string Get(string Name) =>
            GetMany(Name).FirstOrDefault();

        /// <summary> Returns player input. </summary>
        public string[] GetMany(string Name)
        {
            if (!ParsedParameters.TryGetValue(Name, out Parameter[] parameter))
            { return new string[1] { null }; }
            return parameter.Select(p => p.Original).ToArray();
        }

        #endregion
        #region GetParsedPlayerInput

        /// <summary> Returns parsed value of player input. </summary>
        public T Get<T>(string Name) =>
            GetMany<T>(Name).FirstOrDefault();

        /// <summary> Returns parsed value of player input. </summary>
        public T[] GetMany<T>(string Name)
        {
            if (!ParsedParameters.TryGetValue(Name, out Parameter[] parameter))
            { return new T[1] { default(T) }; }
            List<T> param = new List<T>();
            foreach (Parameter p in parameter)
            {
                if (p.Get(out T pp)) { param.Add(pp); }
                else { param.Add(default(T)); }
            }
            return param.ToArray();
        }

        #endregion
    }
}