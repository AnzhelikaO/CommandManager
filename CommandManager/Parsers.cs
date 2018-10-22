#region Using
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ID;
using TShockAPI;
using TShockAPI.DB;
using TShockAPI.Localization;
#pragma warning disable 1591
#endregion
namespace CommandManager
{
    public partial class CommandManager
    {
        public static System.Text.RegularExpressions.Regex CommandTimeFormat =
            new System.Text.RegularExpressions.Regex
            (@"(?<y>\d{1,2}[yY])?(?<mon>\d{1,9}M)?(?<d>\d{1,9}[dD])?" +
             @"(?<h>\d{1,9}[hH])?(?<min>\d{1,9}m)?(?<s>\d{1,9}[sS])?",
            System.Text.RegularExpressions.RegexOptions.Compiled);

        #region Summary

        /// <summary> Predefined supported types to be parsed by
        /// <see cref="ParameterTypesAttribute"/>:
        /// <para> <see cref="byte"/>, <see cref="decimal"/>, <see cref="double"/>,
        /// <see cref="float"/>, <see cref="int"/>, <see cref="long"/>, <see cref="short"/>,
        /// <see cref="uint"/>, <see cref="ulong"/>, <see cref="ushort"/> </para>
        /// <see cref="TSPlayer"/>, <see cref="User"/>
        /// <para> <see cref="Buff"/>, <see cref="Item"/>, <see cref="ItemPrefix"/>,
        /// <see cref="MapPointX"/>, <see cref="MapPointY"/> </para>
        /// <see cref="GameTime"/>, <see cref="CommandTime"/>
        /// <para> <see cref="Group"/>, <see cref="Region"/>, <see cref="Warp"/> </para> </summary>

        #endregion
        public static Dictionary<Type, Func<string, ParameterParseResult>>
            Parsers = new Dictionary<Type, Func<string, ParameterParseResult>>()
            {
                #region Boolean

                {
                    typeof(bool),
                    (o =>
                    {
                        if (o == null)
                        {
                            return new ParameterParseResult(o, o,
                                false, "Boolean cannot be null.");
                        }
                        else if (!bool.TryParse(o, out bool b))
                        {
                            return new ParameterParseResult(o, o,
                                false, "Could not parse boolean.");
                        }
                        else { return new ParameterParseResult(o, b, true, null); }
                    })
                },

                #endregion
                #region Buff

                {
                    typeof(Buff),
                    (o =>
                    {
                        if (o == null)
                        {
                            return new ParameterParseResult(o, o,
                                false, "Buff cannot be null.");
                        }
                        #region ByID

                        if (int.TryParse(o, out int id))
                        {
                            if (id < 1)
                            {
                                return new ParameterParseResult(o, o,
                                    false, "Buff id cannot be less than 1.");
                            }
                            else if (id > Main.maxBuffTypes)
                            {
                                return new ParameterParseResult(o, o,
                                    false, "Buff id cannot be greater " +
                                    $"than {Main.maxBuffTypes}.");
                            }
                            return new ParameterParseResult
                                (o, new Buff((byte)id, TShock.Utils.GetBuffName(id)),
                                true, null);
                        }

                        #endregion
                        #region ByName

                        List<int> byName = TShock.Utils.GetBuffByName(o);
                        if (byName.Count == 0)
                        {
                            return new ParameterParseResult(o, o,
                                false, $"Buff '{o}' was not found.");
                        }
                        else if (byName.Count > 1)
                        {
                            return new ParameterParseResult(o, o,
                                false, "More than one match found: " +
                                string.Join(", ", byName.Select(b =>
                                $"{TShock.Utils.GetBuffName(b)} ({b})")) + ".");
                        }
                        return new ParameterParseResult
                            (o, new Buff((byte)byName[0],
                                TShock.Utils.GetBuffName(byName[0])),
                            true, null);

                        #endregion
                    })
                },

                #endregion
                #region Group                

                {
                    typeof(Group),
                    (o =>
                    {
                        if (o == null)
                        {
                            return new ParameterParseResult(o, o,
                                false, "Group cannot be null.");
                        }
                        Group group =
                            TShock.Groups.GetGroupByName(o);
                        if (group == null)
                        {
                            return new ParameterParseResult(o, o,
                                false, $"Group '{o}' was not found.");
                        }
                        return new ParameterParseResult(o, group, true, null);
                    })
                },

                #endregion
                #region Item
                {
                    typeof(Item),
                    (o =>
                    {
                        if (o == null)
                        {
                            return new ParameterParseResult(o, o,
                                false, "Item cannot be null.");
                        }
                        #region ByID

                        if (int.TryParse(o, out int id))
                        {
                            if (id < -48)
                            {
                                return new ParameterParseResult(o, o,
                                    false, "Item id cannot be less than -48.");
                            }
                            else if (id > Main.maxItemTypes)
                            {
                                return new ParameterParseResult(o, o,
                                    false, "Item id cannot be greater " +
                                    $"than {Main.maxItemTypes}.");
                            }
                            Item i = new Item();
                            i.netDefaults(id);
                            return new ParameterParseResult(o, i, true, null);
                        }

                        #endregion
                        #region ByTag

                        Item byTag = TShock.Utils.GetItemFromTag(o);
                        if (byTag != null)
                        { return new ParameterParseResult(o, byTag, true, null); }

                        #endregion
                        #region ByName

                        List<Item> byName = TShock.Utils.GetItemByName(o);
                        if (byName.Count == 0)
                        {
                            return new ParameterParseResult(o, o,
                                false, $"Item '{o}' was not found.");
                        }
                        else if (byName.Count > 1)
                        {
                            return new ParameterParseResult(o, o,
                                false, "More than one match found: " +
                                string.Join(", ", byName.Select(p =>
                                $"{p.Name} ({p.netID})")) + ".");
                        }
                        return new ParameterParseResult(o, byName[0], true, null);

                        #endregion
                    })
                },

                #endregion
                #region ItemPrefix

                {
                    typeof(ItemPrefix),
                    (o =>
                    {
                        if (o == null)
                        {
                            return new ParameterParseResult(o, o,
                                false, "Item prefix cannot be null.");
                        }
                        #region ByID

                        if (int.TryParse(o, out int id))
                        {
                            if (id < 1)
                            {
                                return new ParameterParseResult(o, o,
                                    false, "Item prefix id cannot be less than 1.");
                            }
                            else if (id >= PrefixID.Count)
                            {
                                return new ParameterParseResult(o, o,
                                    false, "Item prefix id cannot be " +
                                    $"greater than {PrefixID.Count - 1}.");
                            }
                            return new ParameterParseResult
                                (o, new ItemPrefix((byte)id,
                                    EnglishLanguage.GetPrefixById(id)),
                                true, null);
                        }

                        #endregion
                        #region ByName

                        List<int> byName = TShock.Utils.GetPrefixByName(o);
                        if (byName.Count == 0)
                        {
                            return new ParameterParseResult(o, o,
                                false, $"Item prefix '{o}' was not found.");
                        }
                        else if (byName.Count > 1)
                        {
                            return new ParameterParseResult(o, o,
                                false, "More than one match found: " +
                                string.Join(", ", byName.Select(p =>
                                $"{EnglishLanguage.GetPrefixById(p)} ({p})")) +
                                ".");
                        }
                        return new ParameterParseResult
                            (o, new Buff((byte)byName[0],
                                EnglishLanguage.GetPrefixById(byName[0])),
                            true, null);

                        #endregion
                    })
                },
                
                #endregion
                #region MapPointX

                {
                    typeof(MapPointX),
                    (o =>
                    {
                        if (o == null)
                        {
                            return new ParameterParseResult(o, o,
                                false, "X of a map point cannot be null.");
                        }
                        if (!int.TryParse(o, out int x))
                        {
                            return new ParameterParseResult(o, o,
                                false, "Could not parse X of a map point.");
                        }
                        else if (x < 0)
                        {
                            return new ParameterParseResult(o, o,
                                false, "X cannot be less than 0.");
                        }
                        else if (x > Main.maxTilesX)
                        {
                            return new ParameterParseResult(o, o,
                                false, $"X cannot be greater than {Main.maxTilesX}.");
                        }
                        else
                        {
                            return new ParameterParseResult
                                (o, new MapPointX(x), true, null);
                        }
                    })
                },

                #endregion
                #region MapPointY

                {
                    typeof(MapPointY),
                    (o =>
                    {
                        if (o == null)
                        {
                            return new ParameterParseResult(o, o,
                                false, "Y of a map point cannot be null.");
                        }
                        if (!int.TryParse(o, out int y))
                        {
                            return new ParameterParseResult(o, o,
                                false, "Could not parse Y of a map point.");
                        }
                        else if (y < 0)
                        {
                            return new ParameterParseResult(o, o,
                                false, "Y cannot be less than 0.");
                        }
                        else if (y > Main.maxTilesY)
                        {
                            return new ParameterParseResult(o, o,
                                false, $"Y cannot be greater than {Main.maxTilesY}.");
                        }
                        else
                        {
                            return new ParameterParseResult
                                (o, new MapPointY(y), true, null);
                        }
                    })
                },

                #endregion
                #region Numbers

                #region Byte

                {
                    typeof(byte),
                    (o =>
                    {
                        if (o == null)
                        {
                            return new ParameterParseResult(o, o,
                                false, "Byte cannot be null.");
                        }
                        else if (!byte.TryParse(o, out byte b))
                        {
                            return new ParameterParseResult(o, o,
                                false, "Could not parse byte.");
                        }
                        else { return new ParameterParseResult(o, b, true, null); }
                    })
                },

                #endregion
                #region Decimal

                {
                    typeof(decimal),
                    (o =>
                    {
                        if (o == null)
                        {
                            return new ParameterParseResult(o, o,
                                false, "Decimal cannot be null.");
                        }
                        else if (!decimal.TryParse(o, out decimal d))
                        {
                            return new ParameterParseResult(o, o,
                                false, "Could not parse decimal.");
                        }
                        else { return new ParameterParseResult(o, d, true, null); }
                    })
                },

                #endregion
                #region Double

                {
                    typeof(double),
                    (o =>
                    {
                        if (o == null)
                        {
                            return new ParameterParseResult(o, o,
                                false, "Double cannot be null.");
                        }
                        else if (!double.TryParse(o, out double d))
                        {
                            return new ParameterParseResult(o, o,
                                false, "Could not parse double.");
                        }
                        else { return new ParameterParseResult(o, d, true, null); }
                    })
                },

                #endregion
                #region Float

                {
                    typeof(float),
                    (o =>
                    {
                        if (o == null)
                        {
                            return new ParameterParseResult(o, o,
                                false, "Float cannot be null.");
                        }
                        else if (!float.TryParse(o, out float d))
                        {
                            return new ParameterParseResult(o, o,
                                false, "Could not parse float.");
                        }
                        else { return new ParameterParseResult(o, d, true, null); }
                    })
                },

                #endregion
                #region Int

                {
                    typeof(int),
                    (o =>
                    {
                        if (o == null)
                        {
                            return new ParameterParseResult(o, o,
                                false, "Int cannot be null.");
                        }
                        else if (!int.TryParse(o, out int i))
                        {
                            return new ParameterParseResult(o, o,
                                false, "Could not parse int.");
                        }
                        else { return new ParameterParseResult(o, i, true, null); }
                    })
                },

                #endregion
                #region Long

                {
                    typeof(long),
                    (o =>
                    {
                        if (o == null)
                        {
                            return new ParameterParseResult(o, o,
                                false, "Long cannot be null.");
                        }
                        else if (!long.TryParse(o, out long i))
                        {
                            return new ParameterParseResult(o, o,
                                false, "Could not parse long.");
                        }
                        else { return new ParameterParseResult(o, i, true, null); }
                    })
                },

                #endregion
                #region Short

                {
                    typeof(short),
                    (o =>
                    {
                        if (o == null)
                        {
                            return new ParameterParseResult(o, o,
                                false, "Short cannot be null.");
                        }
                        else if (!short.TryParse(o, out short i))
                        {
                            return new ParameterParseResult(o, o,
                                false, "Could not parse short.");
                        }
                        else { return new ParameterParseResult(o, i, true, null); }
                    })
                },

                #endregion
                #region UInt

                {
                    typeof(uint),
                    (o =>
                    {
                        if (o == null)
                        {
                            return new ParameterParseResult(o, o,
                                false, "UInt cannot be null.");
                        }
                        else if (!uint.TryParse(o, out uint i))
                        {
                            return new ParameterParseResult(o, o,
                                false, "Could not parse uint.");
                        }
                        else { return new ParameterParseResult(o, i, true, null); }
                    })
                },

                #endregion
                #region ULong

                {
                    typeof(ulong),
                    (o =>
                    {
                        if (o == null)
                        {
                            return new ParameterParseResult(o, o,
                                false, "ULong cannot be null.");
                        }
                        else if (!ulong.TryParse(o, out ulong i))
                        {
                            return new ParameterParseResult(o, o,
                                false, "Could not parse ulong.");
                        }
                        else { return new ParameterParseResult(o, i, true, null); }
                    })
                },

                #endregion
                #region UShort

                {
                    typeof(ushort),
                    (o =>
                    {
                        if (o == null)
                        {
                            return new ParameterParseResult(o, o,
                                false, "UShort cannot be null.");
                        }
                        else if (!ushort.TryParse(o, out ushort i))
                        {
                            return new ParameterParseResult(o, o,
                                false, "Could not parse ushort.");
                        }
                        else { return new ParameterParseResult(o, i, true, null); }
                    })
                },

                #endregion

                #endregion
                #region GameTime
                
                {
                    typeof(GameTime),
                    (o =>
                    {
                        if (o == null)
                        {
                            return new ParameterParseResult(o, o,
                                false, "Game time cannot be null.");
                        }

                        #region Day Noon Night Midnight

                        switch (o.ToLower())
                        {
                            case "day":
                                {
                                    return new ParameterParseResult
                                        (o, new GameTime(true, 0.0d, "4:30"),
                                        true, null);
                                }
                            case "noon":
                                {
                                    return new ParameterParseResult
                                        (o, new GameTime(true, 27000.0d, "12:00"),
                                        true, null);
                                }
                            case "night":
                                {
                                    return new ParameterParseResult
                                        (o, new GameTime(false, 0.0d, "19:30"),
                                        true, null);
                                }
                            case "midnight":
                                {
                                    return new ParameterParseResult
                                        (o, new GameTime(false, 16200.0d, "0:00"),
                                        true, null);
                                }
                        }

                        #endregion

                        string[] array = o.Split(':');
                        if (array.Length != 2)
                        {
                            return new ParameterParseResult(o, o,
                                false, "Proper format: hh:mm, in 24-hour time. " +
                                "Or day, noon, night, midnight.");
                        }
                        if (!int.TryParse(array[0], out int hours))
                        {
                            return new ParameterParseResult(o, o,
                                false, "Could not parse hours.");
                        }
                        else if (hours < 0)
                        {
                            return new ParameterParseResult(o, o,
                                false, "Hour amount cannot be less than 0.");
                        }
                        else if (hours > 23)
                        {
                            return new ParameterParseResult(o, o,
                                false, "Hour amount cannot be greater than 23.");
                        }
                        if (!int.TryParse(array[1], out int minutes))
                        {
                            return new ParameterParseResult(o, o,
                                false, "Could not parse minutes.");
                        }
                        else if (minutes < 0)
                        {
                            return new ParameterParseResult(o, o,
                                false, "Minute amount cannot be less than 0.");
                        }
                        else if (minutes > 59)
                        {
                            return new ParameterParseResult(o, o,
                                false, "Minute amount cannot be greater than 59.");
                        }
                        decimal time = hours + (minutes / 60.0m);
                        time -= 4.50m;
                        if (time < 0.00m) { time += 24.00m; }
                        return ((time >= 15.00m)
                                    ? new ParameterParseResult
                                        (
                                            o, new GameTime(false,
                                                (double)((time - 15.00m) * 3600.0m),
                                                o),
                                            true, null
                                        )
                                    : new ParameterParseResult
                                        (
                                            o, new GameTime(true,
                                                (double)(time * 3600.0m), o),
                                            true, null
                                        ));
                    })
                },

                #endregion
                #region CommandTime
                
                {
                    typeof(CommandTime),
                    (o =>
                    {
                        if (o == null)
                        {
                            return new ParameterParseResult(o, o,
                                false, "Command time cannot be null.");
                        }

                        System.Text.RegularExpressions.Match mTime =
                        CommandTimeFormat.Match(o);
                        if (!mTime.Success || string.IsNullOrWhiteSpace(o))
                        {
                            return new ParameterParseResult(o, o,
                                false, "Proper format: %y%M%d%m%s " +
                                "(years, months, days, minutes, " +
                                "seconds). Example: 5M1m37s.");
                        }

                        TimeSpan time = new TimeSpan(0, 0, 0, 0);
                        if (!string.IsNullOrWhiteSpace(mTime.Groups["y"].Value))
                        {
                            time.Add(DateTime.Today - DateTime.Today.AddYears
                                (int.Parse(mTime.Groups["y"].Value)));
                        }
                        if (!string.IsNullOrWhiteSpace(mTime.Groups["mon"].Value))
                        {
                            time.Add(DateTime.Today - DateTime.Today.AddMonths
                                (int.Parse(mTime.Groups["mon"].Value)));
                        }
                        if (!string.IsNullOrWhiteSpace(mTime.Groups["d"].Value))
                        {
                            time.Add(new TimeSpan(int.Parse
                                (mTime.Groups["d"].Value), 0, 0, 0));
                        }
                        if (!string.IsNullOrWhiteSpace(mTime.Groups["h"].Value))
                        {
                            time.Add(new TimeSpan(int.Parse
                                (mTime.Groups["h"].Value), 0, 0));
                        }
                        if (!string.IsNullOrWhiteSpace(mTime.Groups["min"].Value))
                        {
                            time.Add(new TimeSpan(0, int.Parse
                                (mTime.Groups["min"].Value), 0));
                        }
                        if (!string.IsNullOrWhiteSpace(mTime.Groups["s"].Value))
                        {
                            time.Add(new TimeSpan(0, 0, int.Parse
                                (mTime.Groups["s"].Value)));
                        }
                        return new ParameterParseResult
                            (o, new CommandTime(time), true, null);
                    })
                },

                #endregion
                #region Region

                {
                    typeof(Region),
                    (o =>
                    {
                        if (o == null)
                        {
                            return new ParameterParseResult(o, o,
                                false, "Region cannot be null.");
                        }
                        Region region =
                            TShock.Regions.GetRegionByName(o);
                        if (region == null)
                        {
                            return new ParameterParseResult(o, o,
                                false, $"Region '{o}' was not found.");
                        }
                        return new ParameterParseResult(o, region, true, null);
                    })
                },

                #endregion
                #region TSPlayer

                {
                    typeof(TSPlayer),
                    (o =>
                    {
                        if (o == null)
                        {
                            return new ParameterParseResult(o, o,
                                false, "Player cannot be null.");
                        }
                        #region ByID

                        if (int.TryParse(o, out int id))
                        {
                            if (id < 0)
                            {
                                return new ParameterParseResult(o, o,
                                    false, "Player index cannot be less than 0.");
                            }
                            else if (id > TShock.Players.Length)
                            {
                                return new ParameterParseResult(o, o,
                                    false, "Player index cannot be greater " +
                                    $"than {TShock.Players.Length}.");
                            }
                            TSPlayer byIndex = TShock.Players[id];
                            if ((byIndex == null) || !byIndex.Active)
                            {
                                return new ParameterParseResult(o, o,
                                    false, $"Player at index {id} was not found.");
                            }
                            return new ParameterParseResult(o, byIndex, true, null);
                        }

                        #endregion
                        #region ByName

                        List<TSPlayer> byName = TShock.Utils.FindPlayer(o);
                        if (byName.Count == 0)
                        {
                            return new ParameterParseResult(o, o,
                                false, $"Player '{o}' was not found.");
                        }
                        else if (byName.Count > 1)
                        {
                            return new ParameterParseResult(o, o,
                                false, "More than one match found: " +
                                string.Join(", ", byName.Select(p =>
                                $"{p.Name} ({p.Index})")) + ".");
                        }
                        return new ParameterParseResult(o, byName[0], true, null);

                        #endregion
                    })
                },

                #endregion
                #region User

                {
                    typeof(User),
                    (o =>
                    {
                        if (o == null)
                        {
                            return new ParameterParseResult(o, o,
                                false, "User cannot be null.");
                        }
                        User byName = TShock.Users.GetUserByName(o);
                        if (byName == null)
                        {
                            return new ParameterParseResult(o, o,
                                false, $"User '{o}' was not found.");
                        }
                        return new ParameterParseResult(o, byName, true, null);
                    })
                },

                #endregion
                #region Warp

                {
                    typeof(Warp),
                    (o =>
                    {
                        if (o == null)
                        {
                            return new ParameterParseResult(o, o,
                                false, "Warp cannot be null.");
                        }
                        Warp warp = TShock.Warps.Find(o);
                        if (warp == null)
                        {
                            return new ParameterParseResult(o, o,
                                false, $"Warp '{o}' was not found.");
                        }
                        return new ParameterParseResult(o, warp, true, null);
                    })
                },

                #endregion
            };
    }
}