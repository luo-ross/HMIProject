namespace RS.Win32API.Enums
{
    /// <summary>
    /// CombingRgn flags.  RGN_*
    /// </summary>
    public enum RGN
    {
        /// <summary>
        /// Creates the intersection of the two combined regions.
        /// </summary>
        AND = 1,
        /// <summary>
        /// Creates the union of two combined regions.
        /// </summary>
        OR = 2,
        /// <summary>
        /// Creates the union of two combined regions except for any overlapping areas.
        /// </summary>
        XOR = 3,
        /// <summary>
        /// Combines the parts of hrgnSrc1 that are not part of hrgnSrc2.
        /// </summary>
        DIFF = 4,
        /// <summary>
        /// Creates a copy of the region identified by hrgnSrc1.
        /// </summary>
        COPY = 5,
    }
} 