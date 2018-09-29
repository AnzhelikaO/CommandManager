#region Using
using System;
#pragma warning disable 1591
#endregion
namespace CommandManager
{
    #region Buff

    public class Buff
    {
        public byte ID { get; }
        public string Name { get; }
        internal Buff(byte ID, string Name)
        {
            this.ID = ID;
            this.Name = Name;
        }
    }

    #endregion
    #region CommandTime

    public class CommandTime
    {
        public TimeSpan Time { get; }
        internal CommandTime(TimeSpan Time) =>
            this.Time = Time;
        public override string ToString() =>
            Time.ToString();
    }

    #endregion
    #region GameTime

    public class GameTime
    {
        public bool Day { get; }
        public double Time { get; }
        string Format { get; }
        internal GameTime(bool Day, double Time, string Format)
        {
            this.Day = Day;
            this.Time = Time;
            this.Format = Format;
        }
        public override string ToString() => Format;
    }

    #endregion
    #region ItemPrefix

    public class ItemPrefix
    {
        public byte ID { get; }
        public string Name { get; }
        internal ItemPrefix(byte ID, string Name)
        {
            this.ID = ID;
            this.Name = Name;
        }
    }

    #endregion
    #region MapPointX

    public class MapPointX
    {
        public int TileX { get; }
        public float X => (TileX * 16);
        internal MapPointX(int TileX) =>
            this.TileX = TileX;
        public override string ToString() =>
            TileX.ToString();
    }

    #endregion
    #region MapPointY

    public class MapPointY
    {
        public int TileY { get; }
        public float Y => (TileY * 16);
        internal MapPointY(int TileY) =>
            this.TileY = TileY;
        public override string ToString() =>
            TileY.ToString();
    }

    #endregion
}