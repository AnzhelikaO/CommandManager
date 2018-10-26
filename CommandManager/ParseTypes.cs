#region Using
using System;
using System.Text.RegularExpressions;
using Terraria;
using TShockAPI;
#pragma warning disable 1591
#endregion
namespace CommandManager
{
    #region Buff
    
    public struct Buff
    {
        public byte ID { get; }
        public string Name { get; }
        #region Constructor ID

        /// <exception cref="ArgumentException"></exception>
        public Buff(byte ID)
        {
            this.ID = ID;
            this.Name = TShock.Utils.GetBuffName(ID);
            if (this.Name == "null")
            { throw new ArgumentException("Invalid buff ID.", "ID"); }
        }

        #endregion
        #region Constructor Name

        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ArgumentNullException"></exception>
        public Buff(string Name)
        {
            if (Name == null)
            { throw new ArgumentNullException("Name"); }
            int index = Array.FindIndex(Lang._buffNameCache, (b => (b.Value == Name)));
            if (index == -1)
            { throw new ArgumentException("Invalid buff name.", "Name"); }
            this.ID = (byte)index;
            this.Name = Name;
        }

        #endregion
        #region Helpers

        #region Operators

        public static implicit operator Buff(byte Buff) => new Buff(Buff);
        public static implicit operator byte(Buff Buff) => Buff.ID;

        public static bool operator ==(Buff Buff1, Buff Buff2) => (Buff1.ID == Buff2.ID);
        public static bool operator !=(Buff Buff1, Buff Buff2) => (Buff1.ID != Buff2.ID);
        public static bool operator >(Buff Buff1, Buff Buff2) => (Buff1.ID > Buff2.ID);
        public static bool operator <(Buff Buff1, Buff Buff2) => (Buff1.ID < Buff2.ID);
        public static bool operator >=(Buff Buff1, Buff Buff2) => (Buff1.ID >= Buff2.ID);
        public static bool operator <=(Buff Buff1, Buff Buff2) => (Buff1.ID <= Buff2.ID);

        #endregion

        public override bool Equals(object obj)
        {
            if (obj == null) { return false; }
            else if (!(obj is Buff b)) { return false; }
            else { return (this == b); }
        }
        public override int GetHashCode() => ID.GetHashCode();
        public override string ToString() => $"{Name} ({ID})";

        #endregion
    }

    #endregion
    #region CommandTime
    
    public struct CommandTime
    {
        public TimeSpan Time { get; }
        public CommandTime(TimeSpan Time) =>
            this.Time = Time;

        #region Helpers

        #region Operators
        
        public static implicit operator CommandTime(TimeSpan Time) => new CommandTime(Time);
        public static implicit operator TimeSpan(CommandTime Time) => Time.Time;

        public static bool operator ==(CommandTime Time1,
            CommandTime Time2) => (Time1.Time == Time2.Time);
        public static bool operator !=(CommandTime Time1,
            CommandTime Time2) => (Time1.Time != Time2.Time);
        public static bool operator >(CommandTime Time1,
            CommandTime Time2) => (Time1.Time > Time2.Time);
        public static bool operator <(CommandTime Time1,
            CommandTime Time2) => (Time1.Time < Time2.Time);
        public static bool operator >=(CommandTime Time1,
            CommandTime Time2) => (Time1.Time >= Time2.Time);
        public static bool operator <=(CommandTime Time1,
            CommandTime Time2) => (Time1.Time <= Time2.Time);

        #endregion

        public override bool Equals(object obj)
        {
            if (obj == null) { return false; }
            else if (!(obj is CommandTime t)) { return false; }
            else { return (this == t); }
        }
        public override int GetHashCode() => Time.GetHashCode();
        public override string ToString() => Time.ToString();

        #endregion
    }

    #endregion
    #region GameTime

    public struct GameTime
    {
        #region Variables

        public static readonly Regex FormatRegex =
            new Regex(@"^(?<Hours>2[0-3]|[0-1]?[0-9]):(?<Minutes>[0-5]?[0-9])$",
                RegexOptions.Compiled);
        public static readonly GameTime Day = new GameTime("4:30");
        public static readonly GameTime Noon = new GameTime("12:00");
        public static readonly GameTime Night = new GameTime("19:30");
        public static readonly GameTime Midnight = new GameTime("0:00");

        #endregion

        public bool IsDay { get; }
        public double Time { get; }
        public string Format { get; }
        #region Constructor TimeDay

        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public GameTime(double Time, bool Day)
        {
            if (Time < 0)
            {
                throw new ArgumentOutOfRangeException("Time",
                    "Time value cannot be less than 0.");
            }
            else if (Day && (Time > 54000))
            {
                throw new ArgumentOutOfRangeException("Time",
                    "Time value cannot be more than 54000.");
            }
            else if (!Day && (Time > 32400))
            {
                throw new ArgumentOutOfRangeException("Time",
                    "Time value cannot be more than 32400.");
            }
            this.IsDay = Day;
            this.Time = Time;
            double time = (((Time / 3600.0) + (Day ? 4.5 : 19.5)) % 24.0);
            this.Format = string.Format("{0}:{1:D2}", (int)Math.Floor(time),
                (int)Math.Round((time % 1.0) * 60.0));
        }

        #endregion
        #region Constructor Format

        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ArgumentNullException"></exception>
        public GameTime(string Format)
        {
            this.Format = Format ?? throw new ArgumentNullException("Format");
            Match match = FormatRegex.Match(Format);
            if (!match.Success)
            { throw new ArgumentException("Invalid time format.", "Format"); }
            decimal time = (decimal.Parse(match.Groups["Hours"].Value) +
                (decimal.Parse(match.Groups["Minutes"].Value) / 60.0m) - 4.5m);
            this.IsDay = true;
            if (time < 0) { time += 24; }
            else if (time >= 15)
            {
                this.IsDay = false;
                time -= 15;
            }
            this.Time = (double)(time * 3600m);
        }

        #endregion
        #region Helpers

        #region Operators

        public static bool operator ==(GameTime Time1, GameTime Time2) =>
            ((Time1.Time == Time2.Time) && (Time1.IsDay == Time2.IsDay));
        public static bool operator !=(GameTime Time1, GameTime Time2) =>
            ((Time1.Time != Time2.Time) || (Time1.IsDay != Time2.IsDay));
        public static bool operator >(GameTime Time1, GameTime Time2) =>
            ((Time1.IsDay == Time2.IsDay)
                ? (Time1.Time > Time2.Time)
                : Time1.IsDay ? false : true);
        public static bool operator <(GameTime Time1, GameTime Time2) =>
            ((Time1.IsDay == Time2.IsDay)
                ? (Time1.Time < Time2.Time)
                : Time1.IsDay ? true : false);
        public static bool operator >=(GameTime Time1, GameTime Time2) =>
            ((Time1.IsDay == Time2.IsDay)
                ? (Time1.Time >= Time2.Time)
                : Time1.IsDay ? false : true);
        public static bool operator <=(GameTime Time1, GameTime Time2) =>
            ((Time1.IsDay == Time2.IsDay)
                ? (Time1.Time <= Time2.Time)
                : Time1.IsDay ? true : false);

        #endregion

        public override bool Equals(object obj)
        {
            if (obj == null) { return false; }
            else if (!(obj is GameTime t)) { return false; }
            else { return (this == t); }
        }
        public override int GetHashCode() =>
            (Time.GetHashCode() << 1) + (IsDay ? 0 : 1);
        public override string ToString() => Format;

        #endregion
    }

    #endregion
    #region ItemPrefix

    public struct ItemPrefix
    {
        public byte ID { get; }
        public string Name { get; }
        #region Constructor ID

        /// <exception cref="ArgumentException"></exception>
        public ItemPrefix(byte ID)
        {
            this.ID = ID;
            this.Name = TShock.Utils.GetPrefixById(ID);
            if (string.IsNullOrWhiteSpace(this.Name))
            { throw new ArgumentException("Invalid prefix ID.", "ID"); }
        }

        #endregion
        #region Constructor Name

        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ArgumentNullException"></exception>
        public ItemPrefix(string Name)
        {
            if (Name == null)
            { throw new ArgumentNullException("Name"); }
            int index = Array.FindIndex(Lang.prefix, (b => (b.Value == Name)));
            if (index == -1)
            { throw new ArgumentException("Invalid prefix name.", "Name"); }
            this.ID = (byte)index;
            this.Name = Name;
        }

        #endregion
        #region Helpers

        #region Operators

        public static implicit operator ItemPrefix(byte Prefix) => new ItemPrefix(Prefix);
        public static implicit operator byte(ItemPrefix Prefix) => Prefix.ID;

        public static bool operator ==(ItemPrefix ItemPrefix1,
            ItemPrefix ItemPrefix2) => (ItemPrefix1.ID == ItemPrefix2.ID);
        public static bool operator !=(ItemPrefix ItemPrefix1,
            ItemPrefix ItemPrefix2) => (ItemPrefix1.ID != ItemPrefix2.ID);
        public static bool operator >(ItemPrefix ItemPrefix1,
            ItemPrefix ItemPrefix2) => (ItemPrefix1.ID > ItemPrefix2.ID);
        public static bool operator <(ItemPrefix ItemPrefix1,
            ItemPrefix ItemPrefix2) => (ItemPrefix1.ID < ItemPrefix2.ID);
        public static bool operator >=(ItemPrefix ItemPrefix1,
            ItemPrefix ItemPrefix2) => (ItemPrefix1.ID >= ItemPrefix2.ID);
        public static bool operator <=(ItemPrefix ItemPrefix1,
            ItemPrefix ItemPrefix2) => (ItemPrefix1.ID <= ItemPrefix2.ID);

        #endregion

        public override bool Equals(object obj)
        {
            if (obj == null) { return false; }
            else if (!(obj is ItemPrefix p)) { return false; }
            else { return (this == p); }
        }
        public override int GetHashCode() => ID.GetHashCode();
        public override string ToString() => $"{Name} ({ID})";

        #endregion
    }

    #endregion
    #region MapPointX

    public struct MapPointX
    {
        public short TileX { get; }
        public float X => TileX * 16;
        #region Constructor

        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public MapPointX(short TileX)
        {
            if ((TileX < 0) || (TileX >= Main.maxTilesX))
            { throw new ArgumentOutOfRangeException("TileX"); }
            this.TileX = TileX;
        }

        #endregion
        #region Helpers

        #region Operators

        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public static implicit operator MapPointX(short TileX) => new MapPointX(TileX);
        public static implicit operator short(MapPointX TileX) => TileX.TileX;

        public static bool operator ==(MapPointX MapPointX1,
            MapPointX MapPointX2) => (MapPointX1.TileX == MapPointX2.TileX);
        public static bool operator !=(MapPointX MapPointX1,
            MapPointX MapPointX2) => (MapPointX1.TileX != MapPointX2.TileX);
        public static bool operator >(MapPointX MapPointX1,
            MapPointX MapPointX2) => (MapPointX1.TileX > MapPointX2.TileX);
        public static bool operator <(MapPointX MapPointX1,
            MapPointX MapPointX2) => (MapPointX1.TileX < MapPointX2.TileX);
        public static bool operator >=(MapPointX MapPointX1,
            MapPointX MapPointX2) => (MapPointX1.TileX >= MapPointX2.TileX);
        public static bool operator <=(MapPointX MapPointX1,
            MapPointX MapPointX2) => (MapPointX1.TileX <= MapPointX2.TileX);

        #endregion

        public override bool Equals(object obj)
        {
            if (obj == null) { return false; }
            else if (!(obj is MapPointX x)) { return false; }
            else { return (this == x); }
        }
        public override int GetHashCode() => TileX.GetHashCode();
        public override string ToString() => TileX.ToString();

        #endregion
    }

    #endregion
    #region MapPointY

    public struct MapPointY
    {
        public short TileY { get; }
        public float Y => TileY * 16;
        #region Constructor

        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public MapPointY(short TileY)
        {
            if ((TileY < 0) || (TileY >= Main.maxTilesY))
            { throw new ArgumentOutOfRangeException("TileY"); }
            this.TileY = TileY;
        }

        #endregion
        #region Helpers

        #region Operators

        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public static implicit operator MapPointY(short TileY) => new MapPointY(TileY);
        public static implicit operator short(MapPointY TileY) => TileY.TileY;

        public static bool operator ==(MapPointY MapPointY1,
            MapPointY MapPointY2) => (MapPointY1.TileY == MapPointY2.TileY);
        public static bool operator !=(MapPointY MapPointY1,
            MapPointY MapPointY2) => (MapPointY1.TileY != MapPointY2.TileY);
        public static bool operator >(MapPointY MapPointY1,
            MapPointY MapPointY2) => (MapPointY1.TileY > MapPointY2.TileY);
        public static bool operator <(MapPointY MapPointY1,
            MapPointY MapPointY2) => (MapPointY1.TileY < MapPointY2.TileY);
        public static bool operator >=(MapPointY MapPointY1,
            MapPointY MapPointY2) => (MapPointY1.TileY >= MapPointY2.TileY);
        public static bool operator <=(MapPointY MapPointY1,
            MapPointY MapPointY2) => (MapPointY1.TileY <= MapPointY2.TileY);

        #endregion

        public override bool Equals(object obj)
        {
            if (obj == null) { return false; }
            else if (!(obj is MapPointY y)) { return false; }
            else { return (this == y); }
        }
        public override int GetHashCode() => TileY.GetHashCode();
        public override string ToString() => TileY.ToString();

        #endregion
    }

    #endregion
}