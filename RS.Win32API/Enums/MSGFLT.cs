namespace RS.Win32API.Enums
{
    /// <summary>
    /// MSGFLT_*.  New in Vista.  Realiased in Windows 7.
    /// </summary>
    public enum MSGFLT
    {
        // Win7 versions of this enum:
        RESET = 0,
        ALLOW = 1,
        DISALLOW = 2,

        // Vista versions of this enum:
        // ADD = 1,
        // REMOVE = 2,
    }
}
