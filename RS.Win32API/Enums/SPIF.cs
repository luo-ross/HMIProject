namespace RS.Win32API.Enums
{
    /// <summary>
    /// SystemParameterInfo flag values, SPIF_*
    /// </summary>
    [Flags]
    public enum SPIF
    {
        None = 0,
        UPDATEINIFILE = 0x01,
        SENDCHANGE = 0x02,
        SENDWININICHANGE = SENDCHANGE,
    }
}
