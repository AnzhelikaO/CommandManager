#region Using
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TShockAPI;
#endregion
namespace CommandManager
{
    internal class Creater
    {
        #region CreateCommandDelegate

        public static CommandDelegate Create(MethodInfo Method,
            string ExceptionName, bool HasCommandInfoAttribute,
            string CommandName, int SubCommandIndex = -2)
        {
            if (!Method.IsCMDelegate(ExceptionName,
                HasCommandInfoAttribute, out string Exception))
            { throw new ArgumentException(Exception); }
            ParameterTypesAttribute @params =
                Method.GetCustomAttribute<ParameterTypesAttribute>();
            string errorMessage =
                Method.GetCustomAttribute<ErrorMessageAttribute>()?.Message
                    ?? @params?.CreateErrorMessage(CommandName);
            #region Subcommands
            
            List<SubCommandAttribute> subCommands =
                    Method.GetCustomAttributes(typeof(SubCommandAttribute))
                          .Select(a => (SubCommandAttribute)a)
                          .ToList();
            int index = (Method.GetCustomAttribute<CommandInfoAttribute>()?.SubCommandParameterIndex ?? -2);
            if (index < -1) { index = SubCommandIndex; }
            index += (Method.GetCustomAttribute<SubCommandIndexSkipAttribute>()?.SkipCount ?? 0);
            int switchIndex = index;

            if ((subCommands.Count > 0) && (index >= -1))
            {
                string[] names = subCommands.SelectMany(c => c.Names ?? new string[0]).ToArray();
                index++;
                foreach (SubCommandAttribute subCommand in subCommands)
                {
                    SubCommandAttribute.SubCommands[subCommand.Method] =
                        Create(subCommand.Method, "", false, CommandName, index);
                }
                return (args =>
                {
                    if (args.Parameters.Count <= ((@params == null)
                                                    ? (switchIndex + 1)
                                                    : (@params.RequiredParametersCount - 1)))
                    {
                        args.Player.SendErrorMessage(errorMessage ?? "Invalid syntax.");
                        return;
                    }

                    string param = args.Parameters[index].ToLower();
                    SubCommandAttribute subCommand =
                        subCommands.FirstOrDefault(c => (c.Names?.Contains(param) ?? false));
                    if (subCommand == null)
                    {
                        subCommand = subCommands.FirstOrDefault(c => (c.Names == null));
                        if (subCommand == null)
                        {
                            args.Player.SendErrorMessage(errorMessage
                                ?? $"Allowed {index + 1} parameter types: " +
                                     string.Join(", ", names));
                            return;
                        }
                    }

                    SubCommandAttribute.SubCommands[subCommand.Method].Invoke(args);
                    return;
                });
            }

            #endregion
            #region WithoutParameterTypes

            else if (@params == null)
            {
                return (args =>
                {
                    Dictionary<string, Parameter[]> parameters =
                        new Dictionary<string, Parameter[]>();
                    for (int i = 0; i < args.Parameters.Count; i++)
                    {
                        parameters.Add(i.ToString(),
                            new Parameter[] { new Parameter(args.Parameters[i], null) });
                    }

                    Method.Invoke(null,
                        new object[] { new CommandManagerArgs(args, parameters) });
                });
            }

            #endregion
            #region WithParameterTypes

            else
            {
                return (args =>
                {
                    if ((@params.RequiredParametersCount != -1)
                        && (args.Parameters.Count < @params.RequiredParametersCount))
                    {
                        args.Player.SendErrorMessage(errorMessage);
                        return;
                    }

                    Dictionary<string, Parameter[]> parameters =
                        new Dictionary<string, Parameter[]>();
                    for (int i = 0; i < @params.ParameterTypes.Length; i++)
                    {
                        ParameterInfo p = @params.ParameterTypes[i];
                        if ((i < args.Parameters.Count) && (p != null))
                        {
                            if (p.AllowMergeInErrorMessage.HasValue)
                            {
                                if (!p.Parse(args.Parameters.Skip(i),
                                    out ParameterParseResult[] result))
                                {
                                    args.Player.SendErrorMessage(result.FirstOrDefault
                                        (r => (r.Error != null)).Error);
                                    return;
                                }
                                parameters.Add(p.Name, result.Select(r =>
                                    new Parameter(r.Input, r.Output)).ToArray());
                            }
                            else if (!p.Parse(args.Parameters[i],
                                out ParameterParseResult result))
                            {
                                args.Player.SendErrorMessage(result.Error);
                                return;
                            }
                            else
                            {
                                Parameter param = new Parameter(result.Input, result.Output);
                                parameters.Add(p.Name, new Parameter[] { param });
                            }
                        }
                        else
                        {
                            Parameter param = new Parameter
                                (args.Parameters.ElementAtOrDefault(i), null);
                            parameters.Add(p.Name, new Parameter[] { param });
                        }
                    }

                    Method.Invoke(null,
                        new object[] { new CommandManagerArgs(args, parameters) });
                });
            }

            #endregion
        }

        #endregion
        #region CreateCommandByManager

        public static CommandByManager Create(MethodInfo Method,
            bool SkipIfHasDoNotRegisterAttribute, out List<Command> Remove)
        {
            Remove = new List<Command>();
            if (!Method.IsCMDelegate("", true, out string Exception))
            { return null; }
            List<Attribute> attributes = Method.GetCustomAttributes().ToList();
            #region DeleteOldMethod

            int old = Commands.ChatCommands.FindIndex
                (c => ((c is CommandByManager c2) && (c2.MethodInfo == Method)));
            if (old != -1) { Remove.Add(Commands.ChatCommands[old]); }

            #endregion
            #region ReadNames

            CommandInfoAttribute info =
                Method.GetCustomAttribute<CommandInfoAttribute>();
            string[] names = info.CommandNames;
            string Name = names[0];

            #endregion
            #region FindCommandsWithSameNames

            List<Command> found = Commands.ChatCommands.Where(c =>
                c.Names.Any(n => names.Contains(n))).ToList();
            Command savedC = found.FirstOrDefault();
            bool replace = attributes.Any(a => (a is ReplaceIfExistsAttribute));
            List<Command> saved = new List<Command>();
            if (replace)
            {
                if (info.CommandPermissions?.Count == 0)
                {
                    info.CommandPermissions =
                        (savedC?.Permissions ?? new List<string>());
                }
                foreach (Command c in found)
                {
                    saved.Add(c.Clone());
                    if (!Remove.Contains(c)) { Remove.Add(c); }
                }
            }
            else
            {
                foreach (Command c in found)
                { if (!Remove.Contains(c)) { Remove.Add(c); } }
            }

            #endregion
            CommandDelegate @delegate = Create(Method,
                "OriginalMethodInfo", true, Name);
            CommandByManager cmd = new CommandByManager(Method,
                saved, info.CommandPermissions, @delegate, names);
            #region Help, HelpDesc, DisallowServer, DoNotLog attributes

            bool hText = false, hDesc = false, server = false, log = false;
            foreach (Attribute a in attributes)
            {
                if (a is HelpAttribute h)
                {
                    hText = true;
                    cmd.HelpText = h.CommandHelp;
                }
                else if (a is HelpDescAttribute d)
                {
                    hDesc = true;
                    cmd.HelpDesc = d.CommandHelpDesc;
                }
                else if (a is DisallowServerAttribute)
                {
                    server = true;
                    cmd.AllowServer = false;
                }
                else if (a is DoNotLogAttribute)
                {
                    log = true;
                    cmd.DoLog = false;
                }
            }

            #endregion
            #region RestoreSomeData

            if (replace && (savedC != null))
            {
                if (!hText && (savedC.HelpText != null))
                { cmd.HelpText = savedC.HelpText; }
                if (!hDesc && (savedC.HelpDesc?.Length > 0))
                { cmd.HelpDesc = savedC.HelpDesc; }
                if (!server) { cmd.AllowServer = savedC.AllowServer; }
                if (!log) { cmd.DoLog = savedC.DoLog; }
            }

            #endregion
            return cmd;
        }

        #endregion
    }
}