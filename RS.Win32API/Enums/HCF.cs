namespace RS.Win32API.Enums
{
    /// <summary>
    /// HIGHCONTRAST flags
    /// </summary>
    [Flags]
    public enum HCF
    {
        HIGHCONTRASTON = 0x00000001,
        AVAILABLE = 0x00000002,
        HOTKEYACTIVE = 0x00000004,
        CONFIRMHOTKEY = 0x00000008,
        HOTKEYSOUND = 0x00000010,
        INDICATOR = 0x00000020,
        HOTKEYAVAILABLE = 0x00000040,
    }
}
