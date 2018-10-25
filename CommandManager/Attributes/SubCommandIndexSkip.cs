using System;
namespace CommandManager
{
    #region Summary

    /// <summary> Will work only if method was added in
    /// one of <see cref="SubCommandAttribute"/>.
    /// <para> Add this attribute if your subcommand
    /// parameter index doesn't go right after
    /// previous subcommand index. </para> </summary>

    #endregion
    [AttributeUsage(AttributeTargets.Method)]
    public class SubCommandIndexSkipAttribute : Attribute
    {
        /// <summary> Amount of parameters to skip (relative
        /// to previous subcommand index). </summary>
        public int SkipCount { get; }
        #region Summary

        /// <summary> Will work only if method was added in
        /// one of <see cref="SubCommandAttribute"/>.
        /// <para> Add this attribute if your subcommand
        /// parameter index doesn't go right after
        /// previous subcommand index. </para> </summary>

        #endregion
        public SubCommandIndexSkipAttribute(int SkipCount) =>
            this.SkipCount = ((SkipCount > 0)
                                ? SkipCount
                                : throw new ArgumentOutOfRangeException("SkipCount"));
    }
}