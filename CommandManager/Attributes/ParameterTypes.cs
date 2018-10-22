#region Using
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using TShockAPI;
using TShockAPI.DB;
#endregion
namespace CommandManager
{
    #region Summary

    /// <summary> Will work only with <see cref="CommandInfoAttribute"/> attribute.
    /// <para> Adding this attribute to command delegate will check parameter
    /// count, their types and return parsed values back. </para>
    /// If the last element of NamesAndTypes array is Boolean
    /// (allow merge in error message):
    /// <para> — Array must contain 3 or more elements; </para>
    /// — Amount of elements of array must be uneven.
    /// <para> Else: </para>
    /// — Array must contain 2 or more elements;
    /// <para> — Amount of elements of array must be even. </para>
    /// Even elements of array (parameter names) must be <see cref="string"/>.
    /// <para> Parameter names must be unique. </para>
    /// If parameter name ends with '-notype', parameter will be shown
    /// as '&lt;Name&gt;' instead of '&lt;Name (Type)&gt;'.
    /// <para> Uneven elements of array (parameter types)
    /// must be <see cref="Type"/> or string[] (that contains parameter.ToLower()). </para>
    /// ——————————
    /// <para> Predefined supported types: </para>
    /// <see cref="byte"/>, <see cref="decimal"/>, <see cref="double"/>,
    /// <see cref="float"/>, <see cref="int"/>, <see cref="long"/>, <see cref="short"/>,
    /// <see cref="uint"/>, <see cref="ulong"/>, <see cref="ushort"/>
    /// <para> <see cref="TSPlayer"/>, <see cref="User"/> </para>
    /// <see cref="Buff"/>, <see cref="Item"/>, <see cref="ItemPrefix"/>,
    /// <see cref="MapPointX"/>, <see cref="MapPointY"/>
    /// <para> <see cref="GameTime"/>, <see cref="CommandTime"/> </para>
    /// <see cref="Group"/>, <see cref="Region"/>, <see cref="Warp"/></summary>

    #endregion
    public class ParameterTypesAttribute : Attribute
    {
        internal ParameterInfo[] ParameterTypes { get; }
        /// <summary> Parameters after required ones count as optional. </summary>
        internal int RequiredParametersCount { get; }
        #region Constructor

        #region Summary

        /// <summary> Will work only with <see cref="CommandInfoAttribute"/> attribute.
        /// <para> Adding this attribute to command delegate will check parameter
        /// count, their types and return parsed values back. </para>
        /// If the last element of NamesAndTypes array is Boolean
        /// (allow merge in error message):
        /// <para> — Array must contain 3 or more elements; </para>
        /// — Amount of elements of array must be uneven.
        /// <para> Else: </para>
        /// — Array must contain 2 or more elements;
        /// <para> — Amount of elements of array must be even. </para>
        /// Even elements of array (parameter names) must be <see cref="string"/>.
        /// <para> Parameter names must be unique. </para>
        /// If parameter name ends with '-notype', parameter will be shown
        /// as '&lt;Name&gt;' instead of '&lt;Name (Type)&gt;'.
        /// <para> Uneven elements of array (parameter types)
        /// must be <see cref="Type"/> or string[] (that contains parameter.ToLower()). </para>
        /// ——————————
        /// <para> Predefined supported types: </para>
        /// <see cref="byte"/>, <see cref="decimal"/>, <see cref="double"/>,
        /// <see cref="float"/>, <see cref="int"/>, <see cref="long"/>, <see cref="short"/>,
        /// <see cref="uint"/>, <see cref="ulong"/>, <see cref="ushort"/>
        /// <para> <see cref="TSPlayer"/>, <see cref="User"/> </para>
        /// <see cref="Buff"/>, <see cref="Item"/>, <see cref="ItemPrefix"/>,
        /// <see cref="MapPointX"/>, <see cref="MapPointY"/>
        /// <para> <see cref="GameTime"/>, <see cref="CommandTime"/> </para>
        /// <see cref="Group"/>, <see cref="Region"/>, <see cref="Warp"/></summary>

        #endregion
        public ParameterTypesAttribute(int RequiredParametersCount,
            params object[] NamesAndTypes)
        {
            if (NamesAndTypes == null)
            { throw new ArgumentNullException("NamesAndTypes"); }
            else if (NamesAndTypes.Length == 0)
            {
                throw new ArgumentException("Array must contain at least " +
                    "1 element.", "NamesAndTypes");
            }

            object last = NamesAndTypes.Last();
            bool merge = false, diff = false;
            if (last is bool)
            {
                diff = true;
                merge = (bool)last;
            }

            if (NamesAndTypes.Length < (diff ? 3 : 2))
            {
                throw new ArgumentException("If the last element of array " +
                    "is Boolean (allow merge in error message), array " +
                    "must contain 3 or more elements, else - 2 or more.",
                    "NamesAndTypes");
            }
            if ((NamesAndTypes.Length % 2) != (diff ? 1 : 0))
            {
                throw new ArgumentException("Amount of elements of array must be" +
                    "uneven with Boolean \"allow merge in error message\" in the " +
                    "last element and even without it.",
                    "NamesAndTypes");
            }

            int maxcount = (diff ? -1
                                 : (NamesAndTypes.Length / 2));
            if ((RequiredParametersCount < 0)
                || ((maxcount != -1) && (RequiredParametersCount > maxcount)))
            { throw new ArgumentOutOfRangeException("RequiredParametersCount"); }
            this.RequiredParametersCount = RequiredParametersCount;
            
            List<ParameterInfo> @params = new List<ParameterInfo>();
            for (int i = 0;
                 i < (NamesAndTypes.Length - (diff ? 1 : 0));
                 i += 2)
            {
                object oName = NamesAndTypes[i];
                object oType = NamesAndTypes[i + 1];
                bool showType = true;

                if (oName == null)
                {
                    throw new ArgumentNullException("Parameter" +
                        $"NamesTypesAndConfigurations[{i}]");
                }
                if (!(oName is string name))
                {
                    throw new ArgumentException("Even elements of " +
                        "array (parameter names) must be String.",
                        $"NamesAndTypes[{i}]");
                }
                
                if (name.ToLower().EndsWith("-notype"))
                {
                    showType = false;
                    name = name.Substring(0, (name.Length - 7)).Trim();
                }

                if (@params.Any(p => (p.Name == name)))
                {
                    throw new ArgumentException("Parameter names must be unique.",
                        $"NamesAndTypes[{i}]");
                }
                if (oType != null)
                {
                    if (oType is Type type)
                    { @params.Add(new ParameterInfo(name, type, showType)); }
                    else if (oType is string[] strings)
                    {
                        strings = strings.Where(s => !string.IsNullOrWhiteSpace(s))
                                         .Select(s => s.ToLower())
                                         .Distinct()
                                         .ToArray();
                        if (strings.Length == 0)
                        {
                            throw new ArgumentException("Parameter types array " +
                                "must contain at least 1 not null and not " +
                                "empty element.", $"NamesAndTypes[{i + 1}]");
                        }
                        @params.Add(new ParameterInfo(name, strings, showType));
                    }
                    else
                    {
                        throw new ArgumentException("Uneven elements of " +
                            "array (parameter types) must be Type or string[].",
                            $"NamesAndTypes[{i + 1}]");
                    }
                }
                else { @params.Add(new ParameterInfo(name, (Type)null, showType)); }
            }

            if (diff)
            {
                ParameterInfo param = @params.Last();
                param.AllowMergeInErrorMessage = merge;
            }

            this.ParameterTypes = @params.ToArray();
        }

        #endregion
        #region CreateErrorMessage

        /// <summary> Returns '/command &lt;Name (Type)&gt; ...' message. </summary>
        public string CreateErrorMessage(string CommandName, bool Silent = false)
        {
            string msg = ((Silent
                            ? TShock.Config.CommandSilentSpecifier
                            : TShock.Config.CommandSpecifier) +
                          CommandName);
            List<string> @params = new List<string>();
            for (int i = 0; i < ParameterTypes.Length; i++)
            {
                @params.Add(ParameterTypes[i].ToString
                    (i < RequiredParametersCount));
            }
            if (@params.Count > 0)
            { msg += " " + string.Join(" ", @params); }
            return msg;
        }

        #endregion
    }
}