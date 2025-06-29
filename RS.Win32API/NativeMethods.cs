using RS.Win32API.Enums;
using RS.Win32API.Interfaces;
using RS.Win32API.SafeHandles;
using RS.Win32API.Standard;
using RS.Win32API.Structs;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Runtime.Versioning;
using System.Security;
using System.Security.Permissions;
using System.Text;

namespace RS.Win32API
{
    public class NativeMethods
    {
        public const int PLANES = 14;
        public const int BI_RGB = 0;
        public const int BITSPIXEL = 12;
        public const int MAX_PATH = 260;
        public const int INFOTIPSIZE = 1024;
        public const int TRUE = 1;
        public const int FALSE = 0;
        public const int DIB_RGB_COLORS = 0;

        public const int SPI_GETBEEP = 1;
        public const int SPI_SETBEEP = 2;
        public const int SPI_GETMOUSE = 3;
        public const int SPI_SETMOUSE = 4;
        public const int SPI_GETBORDER = 5;
        public const int SPI_SETBORDER = 6;
        public const int SPI_GETKEYBOARDSPEED = 10;
        public const int SPI_SETKEYBOARDSPEED = 11;
        public const int SPI_LANGDRIVER = 12;
        public const int SPI_ICONHORIZONTALSPACING = 13;
        public const int SPI_GETSCREENSAVETIMEOUT = 14;
        public const int SPI_SETSCREENSAVETIMEOUT = 15;
        public const int SPI_GETSCREENSAVEACTIVE = 16;
        public const int SPI_SETSCREENSAVEACTIVE = 17;
        public const int SPI_GETGRIDGRANULARITY = 18;
        public const int SPI_SETGRIDGRANULARITY = 19;
        public const int SPI_SETDESKWALLPAPER = 20;
        public const int SPI_SETDESKPATTERN = 21;
        public const int SPI_GETKEYBOARDDELAY = 22;
        public const int SPI_SETKEYBOARDDELAY = 23;
        public const int SPI_ICONVERTICALSPACING = 24;
        public const int SPI_GETICONTITLEWRAP = 25;
        public const int SPI_SETICONTITLEWRAP = 26;
        public const int SPI_GETMENUDROPALIGNMENT = 27;
        public const int SPI_SETMENUDROPALIGNMENT = 28;
        public const int SPI_SETDOUBLECLKWIDTH = 29;
        public const int SPI_SETDOUBLECLKHEIGHT = 30;
        public const int SPI_GETICONTITLELOGFONT = 31;
        public const int SPI_SETDOUBLECLICKTIME = 32;
        public const int SPI_SETMOUSEBUTTONSWAP = 33;
        public const int SPI_SETICONTITLELOGFONT = 34;
        public const int SPI_GETFASTTASKSWITCH = 35;
        public const int SPI_SETFASTTASKSWITCH = 36;
        public const int SPI_SETDRAGFULLWINDOWS = 37;
        public const int SPI_GETDRAGFULLWINDOWS = 38;
        public const int SPI_GETNONCLIENTMETRICS = 41;
        public const int SPI_SETNONCLIENTMETRICS = 42;
        public const int SPI_GETMINIMIZEDMETRICS = 43;
        public const int SPI_SETMINIMIZEDMETRICS = 44;
        public const int SPI_GETICONMETRICS = 45;
        public const int SPI_SETICONMETRICS = 46;
        public const int SPI_SETWORKAREA = 47;
        public const int SPI_GETWORKAREA = 48;
        public const int SPI_SETPENWINDOWS = 49;
        public const int SPI_GETHIGHCONTRAST = 66;
        public const int SPI_SETHIGHCONTRAST = 67;
        public const int SPI_GETKEYBOARDPREF = 68;
        public const int SPI_SETKEYBOARDPREF = 69;
        public const int SPI_GETSCREENREADER = 70;
        public const int SPI_SETSCREENREADER = 71;
        public const int SPI_GETANIMATION = 72;
        public const int SPI_SETANIMATION = 73;
        public const int SPI_GETFONTSMOOTHING = 74;
        public const int SPI_SETFONTSMOOTHING = 75;
        public const int SPI_SETDRAGWIDTH = 76;
        public const int SPI_SETDRAGHEIGHT = 77;
        public const int SPI_SETHANDHELD = 78;
        public const int SPI_GETLOWPOWERTIMEOUT = 79;
        public const int SPI_GETPOWEROFFTIMEOUT = 80;
        public const int SPI_SETLOWPOWERTIMEOUT = 81;
        public const int SPI_SETPOWEROFFTIMEOUT = 82;
        public const int SPI_GETLOWPOWERACTIVE = 83;
        public const int SPI_GETPOWEROFFACTIVE = 84;
        public const int SPI_SETLOWPOWERACTIVE = 85;
        public const int SPI_SETPOWEROFFACTIVE = 86;
        public const int SPI_SETCURSORS = 87;
        public const int SPI_SETICONS = 88;
        public const int SPI_GETDEFAULTINPUTLANG = 89;
        public const int SPI_SETDEFAULTINPUTLANG = 90;
        public const int SPI_SETLANGTOGGLE = 91;
        public const int SPI_GETWINDOWSEXTENSION = 92;
        public const int SPI_SETMOUSETRAILS = 93;
        public const int SPI_GETMOUSETRAILS = 94;
        public const int SPI_SETSCREENSAVERRUNNING = 97;
        public const int SPI_SCREENSAVERRUNNING = SPI_SETSCREENSAVERRUNNING;
        public const int SPI_GETFILTERKEYS = 50;
        public const int SPI_SETFILTERKEYS = 51;
        public const int SPI_GETTOGGLEKEYS = 52;
        public const int SPI_SETTOGGLEKEYS = 53;
        public const int SPI_GETMOUSEKEYS = 54;
        public const int SPI_SETMOUSEKEYS = 55;
        public const int SPI_GETSHOWSOUNDS = 56;
        public const int SPI_SETSHOWSOUNDS = 57;
        public const int SPI_GETSTICKYKEYS = 58;
        public const int SPI_SETSTICKYKEYS = 59;
        public const int SPI_GETACCESSTIMEOUT = 60;
        public const int SPI_SETACCESSTIMEOUT = 61;
        public const int SPI_GETSERIALKEYS = 62;
        public const int SPI_SETSERIALKEYS = 63;
        public const int SPI_GETSOUNDSENTRY = 64;
        public const int SPI_SETSOUNDSENTRY = 65;
        public const int SPI_GETSNAPTODEFBUTTON = 95;
        public const int SPI_SETSNAPTODEFBUTTON = 96;
        public const int SPI_GETMOUSEHOVERWIDTH = 98;
        public const int SPI_SETMOUSEHOVERWIDTH = 99;
        public const int SPI_GETMOUSEHOVERHEIGHT = 100;
        public const int SPI_SETMOUSEHOVERHEIGHT = 101;
        public const int SPI_GETMOUSEHOVERTIME = 102;
        public const int SPI_SETMOUSEHOVERTIME = 103;
        public const int SPI_GETWHEELSCROLLLINES = 104;
        public const int SPI_SETWHEELSCROLLLINES = 105;
        public const int SPI_GETMENUSHOWDELAY = 106;
        public const int SPI_SETMENUSHOWDELAY = 107;
        public const int SPI_GETSHOWIMEUI = 110;
        public const int SPI_SETSHOWIMEUI = 111;
        public const int SPI_GETMOUSESPEED = 112;
        public const int SPI_SETMOUSESPEED = 113;
        public const int SPI_GETSCREENSAVERRUNNING = 114;
        public const int SPI_GETDESKWALLPAPER = 115;
        public const int SPI_GETACTIVEWINDOWTRACKING = 0x1000;
        public const int SPI_SETACTIVEWINDOWTRACKING = 0x1001;
        public const int SPI_GETMENUANIMATION = 0x1002;
        public const int SPI_SETMENUANIMATION = 0x1003;
        public const int SPI_GETCOMBOBOXANIMATION = 0x1004;
        public const int SPI_SETCOMBOBOXANIMATION = 0x1005;
        public const int SPI_GETLISTBOXSMOOTHSCROLLING = 0x1006;
        public const int SPI_SETLISTBOXSMOOTHSCROLLING = 0x1007;
        public const int SPI_GETGRADIENTCAPTIONS = 0x1008;
        public const int SPI_SETGRADIENTCAPTIONS = 0x1009;
        public const int SPI_GETKEYBOARDCUES = 0x100A;
        public const int SPI_SETKEYBOARDCUES = 0x100B;
        public const int SPI_GETMENUUNDERLINES = SPI_GETKEYBOARDCUES;
        public const int SPI_SETMENUUNDERLINES = SPI_SETKEYBOARDCUES;
        public const int SPI_GETACTIVEWNDTRKZORDER = 0x100C;
        public const int SPI_SETACTIVEWNDTRKZORDER = 0x100D;
        public const int SPI_GETHOTTRACKING = 0x100E;
        public const int SPI_SETHOTTRACKING = 0x100F;
        public const int SPI_GETMENUFADE = 0x1012;
        public const int SPI_SETMENUFADE = 0x1013;
        public const int SPI_GETSELECTIONFADE = 0x1014;
        public const int SPI_SETSELECTIONFADE = 0x1015;
        public const int SPI_GETTOOLTIPANIMATION = 0x1016;
        public const int SPI_SETTOOLTIPANIMATION = 0x1017;
        public const int SPI_GETTOOLTIPFADE = 0x1018;
        public const int SPI_SETTOOLTIPFADE = 0x1019;
        public const int SPI_GETCURSORSHADOW = 0x101A;
        public const int SPI_SETCURSORSHADOW = 0x101B;
        public const int SPI_GETUIEFFECTS = 0x103E;
        public const int SPI_SETUIEFFECTS = 0x103F;
        public const int SPI_GETFOREGROUNDLOCKTIMEOUT = 0x2000;
        public const int SPI_SETFOREGROUNDLOCKTIMEOUT = 0x2001;
        public const int SPI_GETACTIVEWNDTRKTIMEOUT = 0x2002;
        public const int SPI_SETACTIVEWNDTRKTIMEOUT = 0x2003;
        public const int SPI_GETFOREGROUNDFLASHCOUNT = 0x2004;
        public const int SPI_SETFOREGROUNDFLASHCOUNT = 0x2005;
        public const int SPI_GETCARETWIDTH = 0x2006;
        public const int SPI_SETCARETWIDTH = 0x2007;


        public const int MONITOR_DEFAULTTONULL = 0x00000000;
        public const int MONITOR_DEFAULTTOPRIMARY = 0x00000001;
        public const int MONITOR_DEFAULTTONEAREST = 0x00000002;

        public const int GWL_HINSTANCE = -6;


        // Window
        public const int WM_USER = 0x0400;
        public const int NIM_ADD = 0x00000000,
       NIM_MODIFY = 0x00000001,
       NIM_DELETE = 0x00000002,
       NIF_MESSAGE = 0x00000001,
       NIM_SETVERSION = 0x00000004,
       NIF_ICON = 0x00000002,
       NIF_INFO = 0x00000010,
       NIF_TIP = 0x00000004,
       NIIF_NONE = 0x00000000,
       NIIF_INFO = 0x00000001,
       NIIF_WARNING = 0x00000002,
       NIIF_ERROR = 0x00000003,
       NIN_BALLOONSHOW = (WM_USER + 2),
       NIN_BALLOONHIDE = (WM_USER + 3),
       NIN_BALLOONTIMEOUT = (WM_USER + 4),
       NIN_BALLOONUSERCLICK = (WM_USER + 5),
       NFR_ANSI = 1,
       NFR_UNICODE = 2,
       NM_CLICK = ((0 - 0) - 2),
       NM_DBLCLK = ((0 - 0) - 3),
       NM_RCLICK = ((0 - 0) - 5),
       NM_RDBLCLK = ((0 - 0) - 6),
       NM_CUSTOMDRAW = ((0 - 0) - 12),
       NM_RELEASEDCAPTURE = ((0 - 0) - 16),
       NONANTIALIASED_QUALITY = 3;

        public const int WH_JOURNALPLAYBACK = 1,
      WH_GETMESSAGE = 3,
      WH_MOUSE = 7,
      WSF_VISIBLE = 0x0001,
      WM_NULL = 0x0000,
      WM_CREATE = 0x0001,
      WM_DELETEITEM = 0x002D,
      WM_DESTROY = 0x0002,
      WM_MOVE = 0x0003,
      WM_SIZE = 0x0005,
      WM_ACTIVATE = 0x0006,
      WA_INACTIVE = 0,
      WA_ACTIVE = 1,
      WA_CLICKACTIVE = 2,
      WM_SETFOCUS = 0x0007,
      WM_KILLFOCUS = 0x0008,
      WM_ENABLE = 0x000A,
      WM_SETREDRAW = 0x000B,
      WM_SETTEXT = 0x000C,
      WM_GETTEXT = 0x000D,
      WM_GETTEXTLENGTH = 0x000E,
      WM_PAINT = 0x000F,
      WM_CLOSE = 0x0010,
      WM_QUERYENDSESSION = 0x0011,
      WM_QUIT = 0x0012,
      WM_QUERYOPEN = 0x0013,
      WM_ERASEBKGND = 0x0014,
      WM_SYSCOLORCHANGE = 0x0015,
      WM_ENDSESSION = 0x0016,
      WM_SHOWWINDOW = 0x0018,
      WM_WININICHANGE = 0x001A,
      WM_SETTINGCHANGE = 0x001A,
      WM_DEVMODECHANGE = 0x001B,
      WM_ACTIVATEAPP = 0x001C,
      WM_FONTCHANGE = 0x001D,
      WM_TIMECHANGE = 0x001E,
      WM_CANCELMODE = 0x001F,
      WM_SETCURSOR = 0x0020,
      WM_MOUSEACTIVATE = 0x0021,
      WM_CHILDACTIVATE = 0x0022,
      WM_QUEUESYNC = 0x0023,
      WM_GETMINMAXINFO = 0x0024,
      WM_PAINTICON = 0x0026,
      WM_ICONERASEBKGND = 0x0027,
      WM_NEXTDLGCTL = 0x0028,
      WM_SPOOLERSTATUS = 0x002A,
      WM_DRAWITEM = 0x002B,
      WM_MEASUREITEM = 0x002C,
      WM_VKEYTOITEM = 0x002E,
      WM_CHARTOITEM = 0x002F,
      WM_SETFONT = 0x0030,
      WM_GETFONT = 0x0031,
      WM_SETHOTKEY = 0x0032,
      WM_GETHOTKEY = 0x0033,
      WM_QUERYDRAGICON = 0x0037,
      WM_COMPAREITEM = 0x0039,
      WM_GETOBJECT = 0x003D,
      WM_COMPACTING = 0x0041,
      WM_COMMNOTIFY = 0x0044,
      WM_WINDOWPOSCHANGING = 0x0046,
      WM_WINDOWPOSCHANGED = 0x0047,
      WM_POWER = 0x0048,
      WM_COPYDATA = 0x004A,
      WM_CANCELJOURNAL = 0x004B,
      WM_NOTIFY = 0x004E,
      WM_INPUTLANGCHANGEREQUEST = 0x0050,
      WM_INPUTLANGCHANGE = 0x0051,
      WM_TCARD = 0x0052,
      WM_HELP = 0x0053,
      WM_USERCHANGED = 0x0054,
      WM_NOTIFYFORMAT = 0x0055,
      WM_CONTEXTMENU = 0x007B,
      WM_STYLECHANGING = 0x007C,
      WM_STYLECHANGED = 0x007D,
      WM_DISPLAYCHANGE = 0x007E,
      WM_GETICON = 0x007F,
      WM_SETICON = 0x0080,
      WM_NCCREATE = 0x0081,
      WM_NCDESTROY = 0x0082,
      WM_NCCALCSIZE = 0x0083,
      WM_NCHITTEST = 0x0084,
      WM_NCPAINT = 0x0085,
      WM_NCACTIVATE = 0x0086,
      WM_GETDLGCODE = 0x0087,
      WM_NCMOUSEMOVE = 0x00A0,
      WM_NCMOUSELEAVE = 0x02A2,
      WM_NCLBUTTONDOWN = 0x00A1,
      WM_NCLBUTTONUP = 0x00A2,
      WM_NCLBUTTONDBLCLK = 0x00A3,
      WM_NCRBUTTONDOWN = 0x00A4,
      WM_NCRBUTTONUP = 0x00A5,
      WM_NCRBUTTONDBLCLK = 0x00A6,
      WM_NCMBUTTONDOWN = 0x00A7,
      WM_NCMBUTTONUP = 0x00A8,
      WM_NCMBUTTONDBLCLK = 0x00A9,
      WM_NCXBUTTONDOWN = 0x00AB,
      WM_NCXBUTTONUP = 0x00AC,
      WM_NCXBUTTONDBLCLK = 0x00AD,
      WM_KEYFIRST = 0x0100,
      WM_KEYDOWN = 0x0100,
      WM_KEYUP = 0x0101,
      WM_CHAR = 0x0102,
      WM_DEADCHAR = 0x0103,
      WM_CTLCOLOR = 0x0019,
      WM_SYSKEYDOWN = 0x0104,
      WM_SYSKEYUP = 0x0105,
      WM_SYSCHAR = 0x0106,
      WM_SYSDEADCHAR = 0x0107,
      WM_KEYLAST = 0x0108,
      WM_IME_STARTCOMPOSITION = 0x010D,
      WM_IME_ENDCOMPOSITION = 0x010E,
      WM_IME_COMPOSITION = 0x010F,
      WM_IME_KEYLAST = 0x010F,
      WM_INITDIALOG = 0x0110,
      WM_COMMAND = 0x0111,
      WM_SYSCOMMAND = 0x0112,
      WM_TIMER = 0x0113,
      WM_HSCROLL = 0x0114,
      WM_VSCROLL = 0x0115,
      WM_INITMENU = 0x0116,
      WM_INITMENUPOPUP = 0x0117,
      WM_MENUSELECT = 0x011F,
      WM_MENUCHAR = 0x0120,
      WM_ENTERIDLE = 0x0121,
      WM_UNINITMENUPOPUP = 0x0125,
      WM_CHANGEUISTATE = 0x0127,
      WM_UPDATEUISTATE = 0x0128,
      WM_QUERYUISTATE = 0x0129,
      WM_CTLCOLORMSGBOX = 0x0132,
      WM_CTLCOLOREDIT = 0x0133,
      WM_CTLCOLORLISTBOX = 0x0134,
      WM_CTLCOLORBTN = 0x0135,
      WM_CTLCOLORDLG = 0x0136,
      WM_CTLCOLORSCROLLBAR = 0x0137,
      WM_CTLCOLORSTATIC = 0x0138,
      WM_MOUSEFIRST = 0x0200,
      WM_MOUSEMOVE = 0x0200,
      WM_LBUTTONDOWN = 0x0201,
      WM_LBUTTONUP = 0x0202,
      WM_LBUTTONDBLCLK = 0x0203,
      WM_RBUTTONDOWN = 0x0204,
      WM_RBUTTONUP = 0x0205,
      WM_RBUTTONDBLCLK = 0x0206,
      WM_MBUTTONDOWN = 0x0207,
      WM_MBUTTONUP = 0x0208,
      WM_MBUTTONDBLCLK = 0x0209,
      WM_XBUTTONDOWN = 0x020B,
      WM_XBUTTONUP = 0x020C,
      WM_XBUTTONDBLCLK = 0x020D,
      WM_MOUSEWHEEL = 0x020A,
      WM_MOUSELAST = 0x020A;


        public const int WHEEL_DELTA = 120,
       WM_PARENTNOTIFY = 0x0210,
       WM_ENTERMENULOOP = 0x0211,
       WM_EXITMENULOOP = 0x0212,
       WM_NEXTMENU = 0x0213,
       WM_SIZING = 0x0214,
       WM_CAPTURECHANGED = 0x0215,
       WM_MOVING = 0x0216,
       WM_POWERBROADCAST = 0x0218,
       WM_DEVICECHANGE = 0x0219,
       WM_IME_SETCONTEXT = 0x0281,
       WM_IME_NOTIFY = 0x0282,
       WM_IME_CONTROL = 0x0283,
       WM_IME_COMPOSITIONFULL = 0x0284,
       WM_IME_SELECT = 0x0285,
       WM_IME_CHAR = 0x0286,
       WM_IME_KEYDOWN = 0x0290,
       WM_IME_KEYUP = 0x0291,
       WM_MDICREATE = 0x0220,
       WM_MDIDESTROY = 0x0221,
       WM_MDIACTIVATE = 0x0222,
       WM_MDIRESTORE = 0x0223,
       WM_MDINEXT = 0x0224,
       WM_MDIMAXIMIZE = 0x0225,
       WM_MDITILE = 0x0226,
       WM_MDICASCADE = 0x0227,
       WM_MDIICONARRANGE = 0x0228,
       WM_MDIGETACTIVE = 0x0229,
       WM_MDISETMENU = 0x0230,
       WM_ENTERSIZEMOVE = 0x0231,
       WM_EXITSIZEMOVE = 0x0232,
       WM_DROPFILES = 0x0233,
       WM_MDIREFRESHMENU = 0x0234,
       WM_MOUSEHOVER = 0x02A1,
       WM_MOUSELEAVE = 0x02A3,
       WM_DPICHANGED = 0x02E0,
       WM_GETDPISCALEDSIZE = 0x02e1,
       WM_DPICHANGED_BEFOREPARENT = 0x02E2,
       WM_DPICHANGED_AFTERPARENT = 0x02E3,
       WM_CUT = 0x0300,
       WM_COPY = 0x0301,
       WM_PASTE = 0x0302,
       WM_CLEAR = 0x0303,
       WM_UNDO = 0x0304,
       WM_RENDERFORMAT = 0x0305,
       WM_RENDERALLFORMATS = 0x0306,
       WM_DESTROYCLIPBOARD = 0x0307,
       WM_DRAWCLIPBOARD = 0x0308,
       WM_PAINTCLIPBOARD = 0x0309,
       WM_VSCROLLCLIPBOARD = 0x030A,
       WM_SIZECLIPBOARD = 0x030B,
       WM_ASKCBFORMATNAME = 0x030C,
       WM_CHANGECBCHAIN = 0x030D,
       WM_HSCROLLCLIPBOARD = 0x030E,
       WM_QUERYNEWPALETTE = 0x030F,
       WM_PALETTEISCHANGING = 0x0310,
       WM_PALETTECHANGED = 0x0311,
       WM_HOTKEY = 0x0312,
       WM_PRINT = 0x0317,
       WM_PRINTCLIENT = 0x0318,
       WM_THEMECHANGED = 0x031A,
       WM_HANDHELDFIRST = 0x0358,
       WM_HANDHELDLAST = 0x035F,
       WM_AFXFIRST = 0x0360,
       WM_AFXLAST = 0x037F,
       WM_PENWINFIRST = 0x0380,
       WM_PENWINLAST = 0x038F,
       WM_APP = unchecked((int)0x8000),
       WM_REFLECT = WM_USER + 0x1C00,
       WS_OVERLAPPED = 0x00000000,
       WS_POPUP = unchecked((int)0x80000000),
       WS_CHILD = 0x40000000,
       WS_MINIMIZE = 0x20000000,
       WS_VISIBLE = 0x10000000,
       WS_DISABLED = 0x08000000,
       WS_CLIPSIBLINGS = 0x04000000,
       WS_CLIPCHILDREN = 0x02000000,
       WS_MAXIMIZE = 0x01000000,
       WS_CAPTION = 0x00C00000,
       WS_BORDER = 0x00800000,
       WS_DLGFRAME = 0x00400000,
       WS_VSCROLL = 0x00200000,
       WS_HSCROLL = 0x00100000,
       WS_SYSMENU = 0x00080000,
       WS_THICKFRAME = 0x00040000,
       WS_TABSTOP = 0x00010000,
       WS_MINIMIZEBOX = 0x00020000,
       WS_MAXIMIZEBOX = 0x00010000,
       WS_EX_DLGMODALFRAME = 0x00000001,
       WS_EX_MDICHILD = 0x00000040,
       WS_EX_TOOLWINDOW = 0x00000080,
       WS_EX_CLIENTEDGE = 0x00000200,
       WS_EX_CONTEXTHELP = 0x00000400,
       WS_EX_RIGHT = 0x00001000,
       WS_EX_LEFT = 0x00000000,
       WS_EX_RTLREADING = 0x00002000,
       WS_EX_LEFTSCROLLBAR = 0x00004000,
       WS_EX_CONTROLPARENT = 0x00010000,
       WS_EX_STATICEDGE = 0x00020000,
       WS_EX_APPWINDOW = 0x00040000,
       WS_EX_LAYERED = 0x00080000,
       WS_EX_TOPMOST = 0x00000008,
       WS_EX_LAYOUTRTL = 0x00400000,
       WS_EX_NOINHERITLAYOUT = 0x00100000,
       WPF_SETMINPOSITION = 0x0001,
       WM_CHOOSEFONT_GETLOGFONT = (0x0400 + 1);


        public const int TRANSPARENT = 1,
       OPAQUE = 2,
       TME_HOVER = 0x00000001,
       TME_LEAVE = 0x00000002,
       TPM_LEFTBUTTON = 0x0000,
       TPM_RIGHTBUTTON = 0x0002,
       TPM_LEFTALIGN = 0x0000,
       TPM_RIGHTALIGN = 0x0008,
       TPM_VERTICAL = 0x0040,
       TV_FIRST = 0x1100,
       TBSTATE_CHECKED = 0x01,
       TBSTATE_ENABLED = 0x04,
       TBSTATE_HIDDEN = 0x08,
       TBSTATE_INDETERMINATE = 0x10,
       TBSTYLE_BUTTON = 0x00,
       TBSTYLE_SEP = 0x01,
       TBSTYLE_CHECK = 0x02,
       TBSTYLE_DROPDOWN = 0x08,
       TBSTYLE_TOOLTIPS = 0x0100,
       TBSTYLE_FLAT = 0x0800,
       TBSTYLE_LIST = 0x1000,
       TBSTYLE_EX_DRAWDDARROWS = 0x00000001,
       TB_ENABLEBUTTON = (0x0400 + 1),
       TB_ISBUTTONCHECKED = (0x0400 + 10),
       TB_ISBUTTONINDETERMINATE = (0x0400 + 13),
       TB_ADDBUTTONSA = (0x0400 + 20),
       TB_ADDBUTTONSW = (0x0400 + 68),
       TB_INSERTBUTTONA = (0x0400 + 21),
       TB_INSERTBUTTONW = (0x0400 + 67),
       TB_DELETEBUTTON = (0x0400 + 22),
       TB_GETBUTTON = (0x0400 + 23),
       TB_SAVERESTOREA = (0x0400 + 26),
       TB_SAVERESTOREW = (0x0400 + 76),
       TB_ADDSTRINGA = (0x0400 + 28),
       TB_ADDSTRINGW = (0x0400 + 77),
       TB_BUTTONSTRUCTSIZE = (0x0400 + 30),
       TB_SETBUTTONSIZE = (0x0400 + 31),
       TB_AUTOSIZE = (0x0400 + 33),
       TB_GETROWS = (0x0400 + 40),
       TB_GETBUTTONTEXTA = (0x0400 + 45),
       TB_GETBUTTONTEXTW = (0x0400 + 75),
       TB_SETIMAGELIST = (0x0400 + 48),
       TB_GETRECT = (0x0400 + 51),
       TB_GETBUTTONSIZE = (0x0400 + 58),
       TB_GETBUTTONINFOW = (0x0400 + 63),
       TB_SETBUTTONINFOW = (0x0400 + 64),
       TB_GETBUTTONINFOA = (0x0400 + 65),
       TB_SETBUTTONINFOA = (0x0400 + 66),
       TB_MAPACCELERATORA = (0x0400 + 78),
       TB_SETEXTENDEDSTYLE = (0x0400 + 84),
       TB_MAPACCELERATORW = (0x0400 + 90),
       TB_GETTOOLTIPS = (0x0400 + 35),
       TB_SETTOOLTIPS = (0x0400 + 36),
       TBIF_IMAGE = 0x00000001,
       TBIF_TEXT = 0x00000002,
       TBIF_STATE = 0x00000004,
       TBIF_STYLE = 0x00000008,
       TBIF_COMMAND = 0x00000020,
       TBIF_SIZE = 0x00000040,
       TBN_GETBUTTONINFOA = ((0 - 700) - 0),
       TBN_GETBUTTONINFOW = ((0 - 700) - 20),
       TBN_QUERYINSERT = ((0 - 700) - 6),
       TBN_DROPDOWN = ((0 - 700) - 10),
       TBN_HOTITEMCHANGE = ((0 - 700) - 13),
       TBN_GETDISPINFOA = ((0 - 700) - 16),
       TBN_GETDISPINFOW = ((0 - 700) - 17),
       TBN_GETINFOTIPA = ((0 - 700) - 18),
       TBN_GETINFOTIPW = ((0 - 700) - 19),
       TTS_ALWAYSTIP = 0x01,
       TTS_NOPREFIX = 0x02,
       TTS_NOANIMATE = 0x10,
       TTS_NOFADE = 0x20,
       TTS_BALLOON = 0x40,
           //TTI_NONE                =0,
           //TTI_INFO                =1,
           TTI_WARNING = 2,
           //TTI_ERROR               =3,
           TTF_IDISHWND = 0x0001,
       TTF_RTLREADING = 0x0004,
       TTF_TRACK = 0x0020,
       TTF_CENTERTIP = 0x0002,
       TTF_SUBCLASS = 0x0010,
       TTF_TRANSPARENT = 0x0100,
       TTF_ABSOLUTE = 0x0080,
       TTDT_AUTOMATIC = 0,
       TTDT_RESHOW = 1,
       TTDT_AUTOPOP = 2,
       TTDT_INITIAL = 3,
       TTM_TRACKACTIVATE = (0x0400 + 17),
       TTM_TRACKPOSITION = (0x0400 + 18),
       TTM_ACTIVATE = (0x0400 + 1),
       TTM_POP = (0x0400 + 28),
       TTM_ADJUSTRECT = (0x400 + 31),
       TTM_SETDELAYTIME = (0x0400 + 3),
       TTM_SETTITLEA = (WM_USER + 32),  // wParam = TTI_*, lParam = char* szTitle
           TTM_SETTITLEW = (WM_USER + 33), // wParam = TTI_*, lParam = wchar* szTitle
           TTM_ADDTOOLA = (0x0400 + 4),
       TTM_ADDTOOLW = (0x0400 + 50),
       TTM_DELTOOLA = (0x0400 + 5),
       TTM_DELTOOLW = (0x0400 + 51),
       TTM_NEWTOOLRECTA = (0x0400 + 6),
       TTM_NEWTOOLRECTW = (0x0400 + 52),
       TTM_RELAYEVENT = (0x0400 + 7),
       TTM_GETTIPBKCOLOR = (0x0400 + 22),
       TTM_SETTIPBKCOLOR = (0x0400 + 19),
       TTM_SETTIPTEXTCOLOR = (0x0400 + 20),
       TTM_GETTIPTEXTCOLOR = (0x0400 + 23),
       TTM_GETTOOLINFOA = (0x0400 + 8),
       TTM_GETTOOLINFOW = (0x0400 + 53),
       TTM_SETTOOLINFOA = (0x0400 + 9),
       TTM_SETTOOLINFOW = (0x0400 + 54),
       TTM_HITTESTA = (0x0400 + 10),
       TTM_HITTESTW = (0x0400 + 55),
       TTM_GETTEXTA = (0x0400 + 11),
       TTM_GETTEXTW = (0x0400 + 56),
       TTM_UPDATE = (0x0400 + 29),
       TTM_UPDATETIPTEXTA = (0x0400 + 12),
       TTM_UPDATETIPTEXTW = (0x0400 + 57),
       TTM_ENUMTOOLSA = (0x0400 + 14),
       TTM_ENUMTOOLSW = (0x0400 + 58),
       TTM_GETCURRENTTOOLA = (0x0400 + 15),
       TTM_GETCURRENTTOOLW = (0x0400 + 59),
       TTM_WINDOWFROMPOINT = (0x0400 + 16),
       TTM_GETDELAYTIME = (0x0400 + 21),
       TTM_SETMAXTIPWIDTH = (0x0400 + 24),
       TTM_GETBUBBLESIZE = (0x0400 + 30),
       TTN_GETDISPINFOA = ((0 - 520) - 0),
       TTN_GETDISPINFOW = ((0 - 520) - 10),
       TTN_SHOW = ((0 - 520) - 1),
       TTN_POP = ((0 - 520) - 2),
       TTN_NEEDTEXTA = ((0 - 520) - 0),
       TTN_NEEDTEXTW = ((0 - 520) - 10),
       TBS_AUTOTICKS = 0x0001,
       TBS_VERT = 0x0002,
       TBS_TOP = 0x0004,
       TBS_BOTTOM = 0x0000,
       TBS_BOTH = 0x0008,
       TBS_NOTICKS = 0x0010,
       TBM_GETPOS = (0x0400),
       TBM_SETTIC = (0x0400 + 4),
       TBM_SETPOS = (0x0400 + 5),
       TBM_SETRANGE = (0x0400 + 6),
       TBM_SETRANGEMIN = (0x0400 + 7),
       TBM_SETRANGEMAX = (0x0400 + 8),
       TBM_SETTICFREQ = (0x0400 + 20),
       TBM_SETPAGESIZE = (0x0400 + 21),
       TBM_SETLINESIZE = (0x0400 + 23),
       TB_LINEUP = 0,
       TB_LINEDOWN = 1,
       TB_PAGEUP = 2,
       TB_PAGEDOWN = 3,
       TB_THUMBPOSITION = 4,
       TB_THUMBTRACK = 5,
       TB_TOP = 6,
       TB_BOTTOM = 7,
       TB_ENDTRACK = 8,
       TVS_HASBUTTONS = 0x0001,
       TVS_HASLINES = 0x0002,
       TVS_LINESATROOT = 0x0004,
       TVS_EDITLABELS = 0x0008,
       TVS_SHOWSELALWAYS = 0x0020,
       TVS_RTLREADING = 0x0040,
       TVS_CHECKBOXES = 0x0100,
       TVS_TRACKSELECT = 0x0200,
       TVS_FULLROWSELECT = 0x1000,
       TVS_NONEVENHEIGHT = 0x4000,
       TVS_INFOTIP = 0x0800,
       TVS_NOTOOLTIPS = 0x0080,
       TVIF_TEXT = 0x0001,
       TVIF_IMAGE = 0x0002,
       TVIF_PARAM = 0x0004,
       TVIF_STATE = 0x0008,
       TVIF_HANDLE = 0x0010,
       TVIF_SELECTEDIMAGE = 0x0020,
       TVIS_SELECTED = 0x0002,
       TVIS_EXPANDED = 0x0020,
       TVIS_EXPANDEDONCE = 0x0040,
       TVIS_STATEIMAGEMASK = 0xF000,
       TVI_ROOT = (unchecked((int)0xFFFF0000)),
       TVI_FIRST = (unchecked((int)0xFFFF0001)),
       TVM_INSERTITEMA = (0x1100 + 0),
       TVM_INSERTITEMW = (0x1100 + 50),
       TVM_DELETEITEM = (0x1100 + 1),
       TVM_EXPAND = (0x1100 + 2),
       TVE_COLLAPSE = 0x0001,
       TVE_EXPAND = 0x0002,
       TVM_GETITEMRECT = (0x1100 + 4),
       TVM_GETINDENT = (0x1100 + 6),
       TVM_SETINDENT = (0x1100 + 7),
       TVM_GETIMAGELIST = (0x1100 + 8),
       TVM_SETIMAGELIST = (0x1100 + 9),
       TVM_GETNEXTITEM = (0x1100 + 10),
       TVGN_NEXT = 0x0001,
       TVGN_PREVIOUS = 0x0002,
       TVGN_FIRSTVISIBLE = 0x0005,
       TVGN_NEXTVISIBLE = 0x0006,
       TVGN_PREVIOUSVISIBLE = 0x0007,
       TVGN_DROPHILITE = 0x0008,
       TVGN_CARET = 0x0009,
       TVM_SELECTITEM = (0x1100 + 11),
       TVM_GETITEMA = (0x1100 + 12),
       TVM_GETITEMW = (0x1100 + 62),
       TVM_SETITEMA = (0x1100 + 13),
       TVM_SETITEMW = (0x1100 + 63),
       TVM_EDITLABELA = (0x1100 + 14),
       TVM_EDITLABELW = (0x1100 + 65),
       TVM_GETEDITCONTROL = (0x1100 + 15),
       TVM_GETVISIBLECOUNT = (0x1100 + 16),
       TVM_HITTEST = (0x1100 + 17),
       TVM_ENSUREVISIBLE = (0x1100 + 20),
       TVM_ENDEDITLABELNOW = (0x1100 + 22),
       TVM_GETISEARCHSTRINGA = (0x1100 + 23),
       TVM_GETISEARCHSTRINGW = (0x1100 + 64),
       TVM_SETITEMHEIGHT = (0x1100 + 27),
       TVM_GETITEMHEIGHT = (0x1100 + 28),
       TVN_SELCHANGINGA = ((0 - 400) - 1),
       TVN_SELCHANGINGW = ((0 - 400) - 50),
       TVN_GETINFOTIPA = ((0 - 400) - 13),
       TVN_GETINFOTIPW = ((0 - 400) - 14),
       TVN_SELCHANGEDA = ((0 - 400) - 2),
       TVN_SELCHANGEDW = ((0 - 400) - 51),
       TVC_UNKNOWN = 0x0000,
       TVC_BYMOUSE = 0x0001,
       TVC_BYKEYBOARD = 0x0002,
       TVN_GETDISPINFOA = ((0 - 400) - 3),
       TVN_GETDISPINFOW = ((0 - 400) - 52),
       TVN_SETDISPINFOA = ((0 - 400) - 4),
       TVN_SETDISPINFOW = ((0 - 400) - 53),
       TVN_ITEMEXPANDINGA = ((0 - 400) - 5),
       TVN_ITEMEXPANDINGW = ((0 - 400) - 54),
       TVN_ITEMEXPANDEDA = ((0 - 400) - 6),
       TVN_ITEMEXPANDEDW = ((0 - 400) - 55),
       TVN_BEGINDRAGA = ((0 - 400) - 7),
       TVN_BEGINDRAGW = ((0 - 400) - 56),
       TVN_BEGINRDRAGA = ((0 - 400) - 8),
       TVN_BEGINRDRAGW = ((0 - 400) - 57),
       TVN_BEGINLABELEDITA = ((0 - 400) - 10),
       TVN_BEGINLABELEDITW = ((0 - 400) - 59),
       TVN_ENDLABELEDITA = ((0 - 400) - 11),
       TVN_ENDLABELEDITW = ((0 - 400) - 60),
       TCS_BOTTOM = 0x0002,
       TCS_RIGHT = 0x0002,
       TCS_FLATBUTTONS = 0x0008,
       TCS_HOTTRACK = 0x0040,
       TCS_VERTICAL = 0x0080,
       TCS_TABS = 0x0000,
       TCS_BUTTONS = 0x0100,
       TCS_MULTILINE = 0x0200,
       TCS_RIGHTJUSTIFY = 0x0000,
       TCS_FIXEDWIDTH = 0x0400,
       TCS_RAGGEDRIGHT = 0x0800,
       TCS_OWNERDRAWFIXED = 0x2000,
       TCS_TOOLTIPS = 0x4000,
       TCM_SETIMAGELIST = (0x1300 + 3),
       TCIF_TEXT = 0x0001,
       TCIF_IMAGE = 0x0002,
       TCM_GETITEMA = (0x1300 + 5),
       TCM_GETITEMW = (0x1300 + 60),
       TCM_SETITEMA = (0x1300 + 6),
       TCM_SETITEMW = (0x1300 + 61),
       TCM_INSERTITEMA = (0x1300 + 7),
       TCM_INSERTITEMW = (0x1300 + 62),
       TCM_DELETEITEM = (0x1300 + 8),
       TCM_DELETEALLITEMS = (0x1300 + 9),
       TCM_GETITEMRECT = (0x1300 + 10),
       TCM_GETCURSEL = (0x1300 + 11),
       TCM_SETCURSEL = (0x1300 + 12),
       TCM_ADJUSTRECT = (0x1300 + 40),
       TCM_SETITEMSIZE = (0x1300 + 41),
       TCM_SETPADDING = (0x1300 + 43),
       TCM_GETROWCOUNT = (0x1300 + 44),
       TCM_GETTOOLTIPS = (0x1300 + 45),
       TCM_SETTOOLTIPS = (0x1300 + 46),
       TCN_SELCHANGE = ((0 - 550) - 1),
       TCN_SELCHANGING = ((0 - 550) - 2),
       TBSTYLE_WRAPPABLE = 0x0200,
       TVM_SETBKCOLOR = (TV_FIRST + 29),
       TVM_SETTEXTCOLOR = (TV_FIRST + 30),
       TYMED_NULL = 0,
       TVM_GETLINECOLOR = (TV_FIRST + 41),
       TVM_SETLINECOLOR = (TV_FIRST + 40),
       TVM_SETTOOLTIPS = (TV_FIRST + 24),
       TVSIL_STATE = 2,
       TVM_SORTCHILDRENCB = (TV_FIRST + 21),
       TMPF_FIXED_PITCH = 0x01;


        public const int stc4 = 0x0443,
      SHGFP_TYPE_CURRENT = 0,
      STGM_READ = 0x00000000,
      STGM_WRITE = 0x00000001,
      STGM_READWRITE = 0x00000002,
      STGM_SHARE_EXCLUSIVE = 0x00000010,
      STGM_CREATE = 0x00001000,
      STGM_TRANSACTED = 0x00010000,
      STGM_CONVERT = 0x00020000,
      STGM_DELETEONRELEASE = 0x04000000,
      STARTF_USESHOWWINDOW = 0x00000001,
      SB_HORZ = 0,
      SB_VERT = 1,
      SB_CTL = 2,
      SB_LINEUP = 0,
      SB_LINELEFT = 0,
      SB_LINEDOWN = 1,
      SB_LINERIGHT = 1,
      SB_PAGEUP = 2,
      SB_PAGELEFT = 2,
      SB_PAGEDOWN = 3,
      SB_PAGERIGHT = 3,
      SB_THUMBPOSITION = 4,
      SB_THUMBTRACK = 5,
      SB_LEFT = 6,
      SB_RIGHT = 7,
      SB_ENDSCROLL = 8,
      SB_TOP = 6,
      SB_BOTTOM = 7,
      SIZE_RESTORED = 0,
      SIZE_MAXIMIZED = 2,
      ESB_ENABLE_BOTH = 0x0000,
      ESB_DISABLE_BOTH = 0x0003,
      SORT_DEFAULT = 0x0,
      SUBLANG_DEFAULT = 0x01,
      SW_HIDE = 0,
      SW_NORMAL = 1,
      SW_SHOWMINIMIZED = 2,
      SW_SHOWMAXIMIZED = 3,
      SW_MAXIMIZE = 3,
      SW_SHOWNOACTIVATE = 4,
      SW_SHOW = 5,
      SW_MINIMIZE = 6,
      SW_SHOWMINNOACTIVE = 7,
      SW_SHOWNA = 8,
      SW_RESTORE = 9,
      SW_MAX = 10,
      SWP_NOSIZE = 0x0001,
      SWP_NOMOVE = 0x0002,
      SWP_NOZORDER = 0x0004,
      SWP_NOACTIVATE = 0x0010,
      SWP_SHOWWINDOW = 0x0040,
      SWP_HIDEWINDOW = 0x0080,
      SWP_DRAWFRAME = 0x0020,
      SWP_NOOWNERZORDER = 0x0200,
      SM_CXSCREEN = 0,
      SM_CYSCREEN = 1,
      SM_CXVSCROLL = 2,
      SM_CYHSCROLL = 3,
      SM_CYCAPTION = 4,
      SM_CXBORDER = 5,
      SM_CYBORDER = 6,
      SM_CYVTHUMB = 9,
      SM_CXHTHUMB = 10,
      SM_CXICON = 11,
      SM_CYICON = 12,
      SM_CXCURSOR = 13,
      SM_CYCURSOR = 14,
      SM_CYMENU = 15,
      SM_CYKANJIWINDOW = 18,
      SM_MOUSEPRESENT = 19,
      SM_CYVSCROLL = 20,
      SM_CXHSCROLL = 21,
      SM_DEBUG = 22,
      SM_SWAPBUTTON = 23,
      SM_CXMIN = 28,
      SM_CYMIN = 29,
      SM_CXSIZE = 30,
      SM_CYSIZE = 31,
      SM_CXFRAME = 32,
      SM_CYFRAME = 33,
      SM_CXMINTRACK = 34,
      SM_CYMINTRACK = 35,
      SM_CXDOUBLECLK = 36,
      SM_CYDOUBLECLK = 37,
      SM_CXICONSPACING = 38,
      SM_CYICONSPACING = 39,
      SM_MENUDROPALIGNMENT = 40,
      SM_PENWINDOWS = 41,
      SM_DBCSENABLED = 42,
      SM_CMOUSEBUTTONS = 43,
      SM_CXFIXEDFRAME = 7,
      SM_CYFIXEDFRAME = 8,
      SM_SECURE = 44,
      SM_CXEDGE = 45,
      SM_CYEDGE = 46,
      SM_CXMINSPACING = 47,
      SM_CYMINSPACING = 48,
      SM_CXSMICON = 49,
      SM_CYSMICON = 50,
      SM_CYSMCAPTION = 51,
      SM_CXSMSIZE = 52,
      SM_CYSMSIZE = 53,
      SM_CXMENUSIZE = 54,
      SM_CYMENUSIZE = 55,
      SM_ARRANGE = 56,
      SM_CXMINIMIZED = 57,
      SM_CYMINIMIZED = 58,
      SM_CXMAXTRACK = 59,
      SM_CYMAXTRACK = 60,
      SM_CXMAXIMIZED = 61,
      SM_CYMAXIMIZED = 62,
      SM_NETWORK = 63,
      SM_CLEANBOOT = 67,
      SM_CXDRAG = 68,
      SM_CYDRAG = 69,
      SM_SHOWSOUNDS = 70,
      SM_CXMENUCHECK = 71,
      SM_CYMENUCHECK = 72,
      SM_MIDEASTENABLED = 74,
      SM_MOUSEWHEELPRESENT = 75,
      SM_XVIRTUALSCREEN = 76,
      SM_YVIRTUALSCREEN = 77,
      SM_CXVIRTUALSCREEN = 78,
      SM_CYVIRTUALSCREEN = 79,
      SM_CMONITORS = 80,
      SM_SAMEDISPLAYFORMAT = 81,
      SM_REMOTESESSION = 0x1000;


        public const int SW_SCROLLCHILDREN = 0x0001,
       SW_INVALIDATE = 0x0002,
       SW_ERASE = 0x0004,
       SW_SMOOTHSCROLL = 0x0010,
       SC_SIZE = 0xF000,
       SC_MINIMIZE = 0xF020,
       SC_MAXIMIZE = 0xF030,
       SC_CLOSE = 0xF060,
       SC_KEYMENU = 0xF100,
       SC_RESTORE = 0xF120,
       SC_MOVE = 0xF010,
       SC_CONTEXTHELP = 0xF180,
       SS_LEFT = 0x00000000,
       SS_CENTER = 0x00000001,
       SS_RIGHT = 0x00000002,
       SS_OWNERDRAW = 0x0000000D,
       SS_NOPREFIX = 0x00000080,
       SS_SUNKEN = 0x00001000,
       SBS_HORZ = 0x0000,
       SBS_VERT = 0x0001,
       SIF_RANGE = 0x0001,
       SIF_PAGE = 0x0002,
       SIF_POS = 0x0004,
       SIF_TRACKPOS = 0x0010,
       SIF_ALL = (0x0001 | 0x0002 | 0x0004 | 0x0010),
       SPI_GETDROPSHADOW = 0x1024,
       SPI_GETFLATMENU = 0x1022,
       SPI_GETFONTSMOOTHINGTYPE = 0x200A,
       SPI_GETFONTSMOOTHINGCONTRAST = 0x200C,
       // SPI_GETICONMETRICS =        0x002D,
       SM_CYFOCUSBORDER = 84,
       SM_CXFOCUSBORDER = 83,
       SM_CYSIZEFRAME = SM_CYFRAME,
       SM_CXSIZEFRAME = SM_CXFRAME,
       SBARS_SIZEGRIP = 0x0100,
       SB_SETTEXTA = (0x0400 + 1),
       SB_SETTEXTW = (0x0400 + 11),
       SB_GETTEXTA = (0x0400 + 2),
       SB_GETTEXTW = (0x0400 + 13),
       SB_GETTEXTLENGTHA = (0x0400 + 3),
       SB_GETTEXTLENGTHW = (0x0400 + 12),
       SB_SETPARTS = (0x0400 + 4),
       SB_SIMPLE = (0x0400 + 9),
       SB_GETRECT = (0x0400 + 10),
       SB_SETICON = (0x0400 + 15),
       SB_SETTIPTEXTA = (0x0400 + 16),
       SB_SETTIPTEXTW = (0x0400 + 17),
       SB_GETTIPTEXTA = (0x0400 + 18),
       SB_GETTIPTEXTW = (0x0400 + 19),
       SBT_OWNERDRAW = 0x1000,
       SBT_NOBORDERS = 0x0100,
       SBT_POPOUT = 0x0200,
       SBT_RTLREADING = 0x0400,
       SRCCOPY = 0x00CC0020,
       SRCAND = 0x008800C6, /* dest = source AND dest          */
       SRCPAINT = 0x00EE0086, /* dest = source OR dest           */
       NOTSRCCOPY = 0x00330008, /* dest = (NOT source)             */
       STATFLAG_DEFAULT = 0x0,
       STATFLAG_NONAME = 0x1,
       STATFLAG_NOOPEN = 0x2,
       STGC_DEFAULT = 0x0,
       STGC_OVERWRITE = 0x1,
       STGC_ONLYIFCURRENT = 0x2,
       STGC_DANGEROUSLYCOMMITMERELYTODISKCACHE = 0x4,
       STREAM_SEEK_SET = 0x0,
       STREAM_SEEK_CUR = 0x1,
       STREAM_SEEK_END = 0x2;


        public const int
        /* FONT WEIGHT (BOLD) VALUES */
        FW_DONTCARE = 0,
        FW_NORMAL = 400,
        FW_BOLD = 700,
        // some others...

        /* FONT CHARACTER SET */
        ANSI_CHARSET = 0,
        DEFAULT_CHARSET = 1,
        // plus others ....

        /* Font OutPrecision */
        OUT_DEFAULT_PRECIS = 0,
        OUT_TT_PRECIS = 4,
        OUT_TT_ONLY_PRECIS = 7,

        /* polygon fill mode */
        ALTERNATE = 1,
        WINDING = 2,

        // text align
        TA_DEFAULT = 0,

        // brush
        BS_SOLID = 0,
        HOLLOW_BRUSH = 5,

        // Binary raster operations.
        R2_BLACK = 1,  /*  0       */
        R2_NOTMERGEPEN = 2,  /* DPon     */
        R2_MASKNOTPEN = 3,  /* DPna     */
        R2_NOTCOPYPEN = 4,  /* PN       */
        R2_MASKPENNOT = 5,  /* PDna     */
        R2_NOT = 6,  /* Dn       */
        R2_XORPEN = 7,  /* DPx      */
        R2_NOTMASKPEN = 8,  /* DPan     */
        R2_MASKPEN = 9,  /* DPa      */
        R2_NOTXORPEN = 10, /* DPxn     */
        R2_NOP = 11, /* D        */
        R2_MERGENOTPEN = 12, /* DPno     */
        R2_COPYPEN = 13, /* P        */
        R2_MERGEPENNOT = 14, /* PDno     */
        R2_MERGEPEN = 15, /* DPo      */
        R2_WHITE = 16 /*  1       */;

        public const int GMEM_FIXED = 0x0000,
       GMEM_MOVEABLE = 0x0002,
       GMEM_NOCOMPACT = 0x0010,
       GMEM_NODISCARD = 0x0020,
       GMEM_ZEROINIT = 0x0040,
       GMEM_MODIFY = 0x0080,
       GMEM_DISCARDABLE = 0x0100,
       GMEM_NOT_BANKED = 0x1000,
       GMEM_SHARE = 0x2000,
       GMEM_DDESHARE = 0x2000,
       GMEM_NOTIFY = 0x4000,
       GMEM_LOWER = GMEM_NOT_BANKED,
       GMEM_VALID_FLAGS = 0x7F72,
       GMEM_INVALID_HANDLE = 0x8000,
       GHND = (GMEM_MOVEABLE | GMEM_ZEROINIT),
       GPTR = (GMEM_FIXED | GMEM_ZEROINIT),
       GCL_WNDPROC = (-24),
       GWL_WNDPROC = (-4),
       GWL_HWNDPARENT = (-8),
       GWL_STYLE = (-16),
       GWL_EXSTYLE = (-20),
       GWL_ID = (-12),
       GW_HWNDFIRST = 0,
       GW_HWNDLAST = 1,
       GW_HWNDNEXT = 2,
       GW_HWNDPREV = 3,
       GW_CHILD = 5,
       GMR_VISIBLE = 0,
       GMR_DAYSTATE = 1,
       GDI_ERROR = (unchecked((int)0xFFFFFFFF)),
       GDTR_MIN = 0x0001,
       GDTR_MAX = 0x0002,
       GDT_VALID = 0,
       GDT_NONE = 1,
       GA_PARENT = 1,
       GA_ROOT = 2;


        public const int
       HC_ACTION = 0,
       HC_GETNEXT = 1,
       HC_SKIP = 2,
       HTNOWHERE = 0,
       HTCLIENT = 1,
       HTBOTTOM = 15,
       HTTRANSPARENT = (-1),
       HTBOTTOMLEFT = 16,
       HTBOTTOMRIGHT = 17,
       HELPINFO_WINDOW = 0x0001,
       HCF_HIGHCONTRASTON = 0x00000001,
       HDI_ORDER = 0x0080,
       HDI_WIDTH = 0x0001,
       HDM_GETITEMCOUNT = (0x1200 + 0),
       HDM_INSERTITEMA = (0x1200 + 1),
       HDM_INSERTITEMW = (0x1200 + 10),
       HDM_GETITEMA = (0x1200 + 3),
       HDM_GETITEMW = (0x1200 + 11),
       HDM_SETITEMA = (0x1200 + 4),
       HDM_SETITEMW = (0x1200 + 12),
       HDN_ITEMCHANGINGA = ((0 - 300) - 0),
       HDN_ITEMCHANGINGW = ((0 - 300) - 20),
       HDN_ITEMCHANGEDA = ((0 - 300) - 1),
       HDN_ITEMCHANGEDW = ((0 - 300) - 21),
       HDN_ITEMCLICKA = ((0 - 300) - 2),
       HDN_ITEMCLICKW = ((0 - 300) - 22),
       HDN_ITEMDBLCLICKA = ((0 - 300) - 3),
       HDN_ITEMDBLCLICKW = ((0 - 300) - 23),
       HDN_DIVIDERDBLCLICKA = ((0 - 300) - 5),
       HDN_DIVIDERDBLCLICKW = ((0 - 300) - 25),
       HDN_BEGINTDRAG = ((0 - 300) - 10),
       HDN_BEGINTRACKA = ((0 - 300) - 6),
       HDN_BEGINTRACKW = ((0 - 300) - 26),
       HDN_ENDDRAG = ((0 - 300) - 11),
       HDN_ENDTRACKA = ((0 - 300) - 7),
       HDN_ENDTRACKW = ((0 - 300) - 27),
       HDN_TRACKA = ((0 - 300) - 8),
       HDN_TRACKW = ((0 - 300) - 28),
       HDN_GETDISPINFOA = ((0 - 300) - 9),
       HDN_GETDISPINFOW = ((0 - 300) - 29);


        public const int IME_CMODE_NATIVE = 0x0001,
       IME_CMODE_KATAKANA = 0x0002,
       IME_CMODE_FULLSHAPE = 0x0008,
       INPLACE_E_NOTOOLSPACE = unchecked((int)0x800401A1),
       ICON_SMALL = 0,
       ICON_BIG = 1,
       IDC_ARROW = 32512,
       IDC_IBEAM = 32513,
       IDC_WAIT = 32514,
       IDC_CROSS = 32515,
       IDC_SIZEALL = 32646,
       IDC_SIZENWSE = 32642,
       IDC_SIZENESW = 32643,
       IDC_SIZEWE = 32644,
       IDC_SIZENS = 32645,
       IDC_UPARROW = 32516,
       IDC_NO = 32648,
       IDC_HAND = 32649,
       IDC_APPSTARTING = 32650,
       IDC_HELP = 32651,
       IMAGE_ICON = 1,
       IMAGE_CURSOR = 2,
       ICC_LISTVIEW_CLASSES = 0x00000001,
       ICC_TREEVIEW_CLASSES = 0x00000002,
       ICC_BAR_CLASSES = 0x00000004,
       ICC_TAB_CLASSES = 0x00000008,
       ICC_PROGRESS_CLASS = 0x00000020,
       ICC_DATE_CLASSES = 0x00000100,
       ILC_MASK = 0x0001,
       ILC_COLOR = 0x0000,
       ILC_COLOR4 = 0x0004,
       ILC_COLOR8 = 0x0008,
       ILC_COLOR16 = 0x0010,
       ILC_COLOR24 = 0x0018,
       ILC_COLOR32 = 0x0020,
       ILC_MIRROR = 0x00002000,
       ILD_NORMAL = 0x0000,
       ILD_TRANSPARENT = 0x0001,
       ILD_MASK = 0x0010,
       ILD_ROP = 0x0040,

       // ImageList 
       //
       ILP_NORMAL = 0,
       ILP_DOWNLEVEL = 1,
       ILS_NORMAL = 0x0,
       ILS_GLOW = 0x1,
       ILS_SHADOW = 0x2,
       ILS_SATURATE = 0x4,
       ILS_ALPHA = 0x8;


        public const int LR_DEFAULTCOLOR = 0x0000,
                        LR_MONOCHROME = 0x0001,
                        LR_COLOR = 0x0002,
                        LR_COPYRETURNORG = 0x0004,
                        LR_COPYDELETEORG = 0x0008,
                        LR_LOADFROMFILE = 0x0010,
                        LR_LOADTRANSPARENT = 0x0020,
                        LR_DEFAULTSIZE = 0x0040,
                        LR_VGACOLOR = 0x0080,
                        LR_LOADMAP3DCOLORS = 0x1000,
                        LR_CREATEDIBSECTION = 0x2000,
                        LR_COPYFROMRESOURCE = 0x4000,
                        LR_SHARED = unchecked((int)0x8000);

        public const int ERROR_FILE_NOT_FOUND = 2;
        public const int ERROR_PATH_NOT_FOUND = 3;
        public const int ERROR_ACCESS_DENIED = 5;
        public const int ERROR_INVALID_DRIVE = 15;
        public const int ERROR_SHARING_VIOLATION = 32;
        public const int ERROR_FILE_EXISTS = 80;
        public const int ERROR_INVALID_PARAMETER = 87;
        public const int ERROR_FILENAME_EXCED_RANGE = 206;
        public const int ERROR_NO_MORE_ITEMS = 259;
        public const int ERROR_OPERATION_ABORTED = 995;



        public const UInt32 PD_ALLPAGES = 0x00000000;
        public const UInt32 PD_SELECTION = 0x00000001;
        public const UInt32 PD_PAGENUMS = 0x00000002;
        public const UInt32 PD_NOSELECTION = 0x00000004;
        public const UInt32 PD_NOPAGENUMS = 0x00000008;
        public const UInt32 PD_USEDEVMODECOPIESANDCOLLATE = 0x00040000;
        public const UInt32 PD_DISABLEPRINTTOFILE = 0x00080000;
        public const UInt32 PD_HIDEPRINTTOFILE = 0x00100000;
        public const UInt32 PD_CURRENTPAGE = 0x00400000;
        public const UInt32 PD_NOCURRENTPAGE = 0x00800000;
        public const UInt32 PD_RESULT_CANCEL = 0x0;
        public const UInt32 PD_RESULT_PRINT = 0x1;
        public const UInt32 PD_RESULT_APPLY = 0x2;
        public const UInt32 START_PAGE_GENERAL = 0xFFFFFFFF;


        public const int S_OK = 0x00000000;
        public const int S_FALSE = 0x00000001;



        //GetDeviceCaps()
        public const int LOGPIXELSX = 88;
        public const int LOGPIXELSY = 90;

        public const int GMMP_USE_DISPLAY_POINTS = 1;
        public const int GMMP_USE_HIGH_RESOLUTION_POINTS = 2;

        public const int TME_CANCEL = (unchecked((int)0x80000000));

        public const int ABM_GETTASKBARPOS = 0x00000005;

        public static HandleRef HWND_TOP = new HandleRef(null, (IntPtr)0);

        public static HandleRef HWND_BOTTOM = new HandleRef(null, (IntPtr)1);

        public static HandleRef HWND_TOPMOST = new HandleRef(null, new IntPtr(-1));

        public static HandleRef HWND_NOTOPMOST = new HandleRef(null, new IntPtr(-2));

        public static readonly DpiAwarenessContextHandle DPI_AWARENESS_CONTEXT_UNAWARE = DpiAwarenessContextHandle.DPI_AWARENESS_CONTEXT_UNAWARE;

        public static readonly DpiAwarenessContextHandle DPI_AWARENESS_CONTEXT_SYSTEM_AWARE = DpiAwarenessContextHandle.DPI_AWARENESS_CONTEXT_SYSTEM_AWARE;

        public static readonly DpiAwarenessContextHandle DPI_AWARENESS_CONTEXT_PER_MONITOR_AWARE = DpiAwarenessContextHandle.DPI_AWARENESS_CONTEXT_PER_MONITOR_AWARE;

        public static readonly DpiAwarenessContextHandle DPI_AWARENESS_CONTEXT_PER_MONITOR_AWARE_V2 = DpiAwarenessContextHandle.DPI_AWARENESS_CONTEXT_PER_MONITOR_AWARE_V2;


        [return: MarshalAs(UnmanagedType.Bool)]
        public delegate bool MonitorEnumProc(IntPtr hMonitor, IntPtr hdcMonitor, ref RECT lprcMonitor, IntPtr dwData);

        /// <summary>Delegate declaration that matches native WndProc signatures.</summary>
        public delegate IntPtr WndProc(IntPtr hwnd, WM uMsg, IntPtr wParam, IntPtr lParam);

        /// <summary>Delegate declaration that matches native WndProc signatures.</summary>
        public delegate IntPtr WndProcHook(IntPtr hwnd, WM uMsg, IntPtr wParam, IntPtr lParam, ref bool handled);

        /// <summary>Delegate declaration that matches managed WndProc signatures.</summary>
        public delegate IntPtr MessageHandler(WM uMsg, IntPtr wParam, IntPtr lParam, out bool handled);




        public static HandleRef NullHandleRef = new HandleRef(null, IntPtr.Zero);
        ///<SecurityNote>
        /// Critical as this code performs an elevation.The call to handle collector
        /// is by itself not dangerous because handle collector simply
        /// stores a count of the number of instances of a given handle and not the handle itself.
        ///</SecurityNote>


        [DllImport(ExternDll.User32, ExactSpelling = true, EntryPoint = "ReleaseDC", CharSet = CharSet.Auto)]
        public static extern int IntReleaseDC(HandleRef hWnd, HandleRef hDC);


        ///<SecurityNote>
        /// Critical as this code performs an elevation.
        ///</SecurityNote>

        public static int ReleaseDC(HandleRef hWnd, HandleRef hDC)
        {
            SafeHandles.HandleCollector.Remove((IntPtr)hDC, CommonHandles.HDC);
            return IntReleaseDC(hWnd, hDC);
        }



        ///<SecurityNote>
        /// Critical as this code performs an elevation.
        ///</SecurityNote>


        [DllImport(ExternDll.User32)]
        public static extern int GetSystemMetrics(SM nIndex);

        ///<SecurityNote>
        /// Critical as this code performs an elevation.
        ///</SecurityNote>


        [DllImport(ExternDll.Gdi32, SetLastError = true, ExactSpelling = true, CharSet = CharSet.Auto)]
        public static extern int GetDeviceCaps(HandleRef hDC, int nIndex);


        ///<SecurityNote>
        /// Critical as this code performs an elevation.
        ///</SecurityNote>


        [DllImport(ExternDll.User32, SetLastError = true, ExactSpelling = true, EntryPoint = "GetDC", CharSet = CharSet.Auto)]
        public static extern IntPtr IntGetDC(HandleRef hWnd);

        ///<SecurityNote>
        /// Critical as this code performs an elevation. The call to handle collector is
        /// by itself not dangerous because handle collector simply
        /// stores a count of the number of instances of a given
        /// handle and not the handle itself.
        ///</SecurityNote>

        public static IntPtr GetDC(HandleRef hWnd)
        {
            IntPtr hDc = IntGetDC(hWnd);
            if (hDc == IntPtr.Zero)
            {
                throw new Win32Exception();
            }

            return SafeHandles.HandleCollector.Add(hDc, CommonHandles.HDC);
        }


        /// <SecurityNote>
        ///     Critical: This code elevates to unmanaged code permission
        /// </SecurityNote>
        [SecurityCritical, SuppressUnmanagedCodeSecurity]
        [DllImport(ExternDll.Kernel32, EntryPoint = "GetModuleFileName", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern int IntGetModuleFileName(HandleRef hModule, StringBuilder buffer, int length);

        /// <SecurityNote>
        ///     Critical: This code elevates to unmanaged code permission by calling into IntGetModuleFileName
        /// </SecurityNote>

        public static string GetModuleFileName(HandleRef hModule)
        {
            // .Net is currently far behind Windows with regard to supporting paths longer than MAX_PATH.
            // At one point it was tested trying to load UNC paths longer than MAX_PATH and mscorlib threw
            // FileIOExceptions before WPF was even on the stack.
            // All the same, we still want to have this grow-and-retry logic because the CLR can be hosted
            // in a native application.  Callers bothering to use this rather than Assembly based reflection
            // are likely doing so because of (at least the potential for) the returned name referring to a
            // native module.
            StringBuilder buffer = new StringBuilder(MAX_PATH);
            while (true)
            {
                int size = IntGetModuleFileName(hModule, buffer, buffer.Capacity);
                if (size == 0)
                {
                    throw new Win32Exception();
                }

                // GetModuleFileName returns nSize when it's truncated but does NOT set the last error.
                // MSDN documentation says this has changed in Windows 2000+.
                if (size == buffer.Capacity)
                {
                    // Enlarge the buffer and try again.
                    buffer.EnsureCapacity(buffer.Capacity * 2);
                    continue;
                }

                return buffer.ToString();
            }

        }

        /// <SecurityNote>
        ///     Critical: This elevates to unmanaged code permission
        /// </SecurityNote>
        [SecurityCritical, SuppressUnmanagedCodeSecurity]
        [DllImport(ExternDll.Shell32, CharSet = System.Runtime.InteropServices.CharSet.Auto, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        public static extern int ExtractIconEx(
                                        string szExeFileName,
                                        int nIconIndex,
                                        out IconHandle phiconLarge,
                                        out IconHandle phiconSmall,
                                        int nIcons);


        /// <SecurityNote>
        /// Critical as suppressing UnmanagedCodeSecurity
        /// </SecurityNote>
        [SecurityCritical, SuppressUnmanagedCodeSecurity]
        [DllImport(ExternDll.Gdi32, SetLastError = true, ExactSpelling = true, CharSet = CharSet.Auto, EntryPoint = "CreateDIBSection")]
        public static extern BitmapHandle publicCreateDIBSection(HandleRef hdc, ref BITMAPINFO bitmapInfo, int iUsage, ref IntPtr ppvBits, SafeFileMappingHandle hSection, int dwOffset);
        /// <SecurityNote>
        /// Critical - The method invokes publicCreateDIBSection.
        /// </SecurityNote>

        public static BitmapHandle CreateDIBSection(HandleRef hdc, ref BITMAPINFO bitmapInfo, int iUsage, ref IntPtr ppvBits, SafeFileMappingHandle hSection, int dwOffset)
        {
            if (hSection == null)
            {
                // PInvoke marshalling does not handle null SafeHandle, we must pass an IntPtr.Zero backed SafeHandle
                hSection = new SafeFileMappingHandle(IntPtr.Zero);
            }

            BitmapHandle hBitmap = publicCreateDIBSection(hdc, ref bitmapInfo, iUsage, ref ppvBits, hSection, dwOffset);
            int error = Marshal.GetLastWin32Error();

            if (hBitmap.IsInvalid)
            {
                Debug.WriteLine("CreateDIBSection failed. Error = " + error);
            }

            return hBitmap;
        }



        ///<SecurityNote>
        /// Critical as this code performs an elevation.
        ///</SecurityNote>


        [DllImport(ExternDll.User32, EntryPoint = "DestroyIcon", CharSet = System.Runtime.InteropServices.CharSet.Auto, SetLastError = true)]
        public static extern bool IntDestroyIcon(IntPtr hIcon);

        ///<SecurityNote>
        /// Critical: calls a critical method (IntDestroyIcon)
        ///</SecurityNote>

        public static bool DestroyIcon(IntPtr hIcon)
        {
            bool result = IntDestroyIcon(hIcon);
            int error = Marshal.GetLastWin32Error();

            if (!result)
            {
                // To be consistent with out other PInvoke wrappers
                // we should "throw" here.  But we don't want to
                // introduce new "throws" w/o time to follow up on any
                // new problems that causes.
                Debug.WriteLine("DestroyIcon failed.  Error = " + error);
                //throw new Win32Exception();
            }

            return result;
        }



        ///<SecurityNote>
        /// Critical as this code performs an elevation.
        ///</SecurityNote>


        [DllImport(ExternDll.Gdi32, EntryPoint = "DeleteObject", CharSet = System.Runtime.InteropServices.CharSet.Auto, SetLastError = true)]
        public static extern bool IntDeleteObject(IntPtr hObject);

        ///<SecurityNote>
        /// Critical: calls a critical method (IntDeleteObject)
        ///</SecurityNote>

        public static bool DeleteObject(IntPtr hObject)
        {
            bool result = IntDeleteObject(hObject);
            int error = Marshal.GetLastWin32Error();

            if (!result)
            {
                // To be consistent with out other PInvoke wrappers
                // we should "throw" here.  But we don't want to
                // introduce new "throws" w/o time to follow up on any
                // new problems that causes.
                Debug.WriteLine("DeleteObject failed.  Error = " + error);
                //throw new Win32Exception();
            }

            return result;
        }



        [DllImport(ExternDll.Kernel32, EntryPoint = "CloseHandle", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool IntCloseHandle(HandleRef handle);

        ///<SecurityNote>
        /// Critical: Closes a passed in handle, LinkDemand on Marshal.GetLastWin32Error
        ///</SecurityNote>

        public static bool CloseHandleNoThrow(HandleRef handle)
        {
            SafeHandles.HandleCollector.Remove((IntPtr)handle, CommonHandles.Kernel);

            bool result = IntCloseHandle(handle);
            int error = Marshal.GetLastWin32Error();

            if (!result)
            {
                Debug.WriteLine("CloseHandle failed.  Error = " + error);
            }

            return result;

        }



        /// <SecurityNote>
        /// Critical as suppressing UnmanagedCodeSecurity
        /// </SecurityNote>
        [SecurityCritical, SuppressUnmanagedCodeSecurity]
        [DllImport(ExternDll.Gdi32, SetLastError = true, ExactSpelling = true, CharSet = CharSet.Auto, EntryPoint = "CreateBitmap")]
        public static extern BitmapHandle publicCreateBitmap(int width, int height, int planes, int bitsPerPixel, byte[] lpvBits);
        /// <SecurityNote>
        /// Critical - The method invokes publicCreateBitmap.
        /// </SecurityNote>

        public static BitmapHandle CreateBitmap(int width, int height, int planes, int bitsPerPixel, byte[] lpvBits)
        {
            BitmapHandle hBitmap = publicCreateBitmap(width, height, planes, bitsPerPixel, lpvBits);
            int error = Marshal.GetLastWin32Error();

            if (hBitmap.IsInvalid)
            {
                Debug.WriteLine("CreateBitmap failed. Error = " + error);
            }

            return hBitmap;
        }



        /// <SecurityNote>
        /// Critical as suppressing UnmanagedCodeSecurity
        /// </SecurityNote>
        [SecurityCritical, SuppressUnmanagedCodeSecurity]
        [DllImport(ExternDll.User32, SetLastError = true, ExactSpelling = true, CharSet = CharSet.Auto, EntryPoint = "CreateIconIndirect")]
        public static extern IconHandle publicCreateIconIndirect([In, MarshalAs(UnmanagedType.LPStruct)] ICONINFO iconInfo);
        /// <SecurityNote>
        /// Critical - The method invokes publicCreateIconIndirect.
        /// </SecurityNote>

        public static IconHandle CreateIconIndirect([In, MarshalAs(UnmanagedType.LPStruct)] ICONINFO iconInfo)
        {
            IconHandle hIcon = publicCreateIconIndirect(iconInfo);
            int error = Marshal.GetLastWin32Error();

            if (hIcon.IsInvalid)
            {
                Debug.WriteLine("CreateIconIndirect failed. Error = " + error);
            }

            return hIcon;
        }


        [DllImport(ExternDll.User32, CharSet = CharSet.Auto)]
        [ResourceExposure(ResourceScope.None)]
        public static extern bool SystemParametersInfo(int nAction, int nParam, ref RECT rc, int nUpdate);
        [DllImport(ExternDll.User32, CharSet = CharSet.Auto)]
        [ResourceExposure(ResourceScope.None)]
        public static extern bool SystemParametersInfo(int nAction, int nParam, ref int value, int ignore);
        [DllImport(ExternDll.User32, CharSet = CharSet.Auto)]
        [ResourceExposure(ResourceScope.None)]
        public static extern bool SystemParametersInfo(int nAction, int nParam, ref bool value, int ignore);
        [DllImport(ExternDll.User32, CharSet = CharSet.Auto)]
        [ResourceExposure(ResourceScope.None)]
        public static extern bool SystemParametersInfo(int nAction, int nParam, ref HIGHCONTRAST_I rc, int nUpdate);
        [DllImport(ExternDll.User32, CharSet = CharSet.Auto)]
        [ResourceExposure(ResourceScope.None)]
        public static extern bool SystemParametersInfo(int nAction, int nParam, [In, Out] NONCLIENTMETRICS metrics, int nUpdate);
        [DllImport(ExternDll.User32, CharSet = CharSet.Unicode)]
        public static extern bool SystemParametersInfo(int uiAction, int uiParam, IntPtr pvParam, int fWinIni);

        [DllImport(ExternDll.User32, CharSet = CharSet.Auto)]
        [ResourceExposure(ResourceScope.Process)]
        public static extern IntPtr FindWindow(string className, string windowName);

        [DllImport(ExternDll.User32, CharSet = CharSet.Auto)]
        public static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);


        public delegate bool EnumThreadWindowsCallback(IntPtr hWnd, IntPtr lParam);
        [DllImport(ExternDll.User32, CharSet = CharSet.Auto, SetLastError = true)]
        [ResourceExposure(ResourceScope.None)]
        public static extern bool EnumWindows(EnumThreadWindowsCallback callback, IntPtr extraData);

        [DllImport(ExternDll.User32, CharSet = CharSet.Auto)]
        public static extern IntPtr FindWindowEx(IntPtr hwndParent, IntPtr hwndChildAfter, string lpszClass, string lpszWindow);

        [DllImport(ExternDll.User32, CharSet = CharSet.Auto)]
        public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport(ExternDll.User32, CharSet = CharSet.Auto)]
        public static extern IntPtr SetParent(IntPtr hWndChild, IntPtr hWndNewParent);

        [DllImport(ExternDll.User32, SetLastError = true, ExactSpelling = true)]
        public static extern IntPtr MonitorFromWindow(HandleRef handle, int flags);

        [DllImport(ExternDll.User32, EntryPoint = "GetMonitorInfo", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool IntGetMonitorInfo(HandleRef hmonitor, [In, Out] MONITORINFOEX info);

        /// <SecurityNote>
        ///    Critical: This code calls into unmanaged code which elevates
        ///    TreatAsSafe: This method is ok to give out
        /// </SecurityNote>
        [SecurityCritical, SecurityTreatAsSafe]
        public static void GetMonitorInfo(HandleRef hmonitor, [In, Out] MONITORINFOEX info)
        {
            if (IntGetMonitorInfo(hmonitor, info) == false)
            {
                throw new Win32Exception();
            }
        }


        [DllImport(ExternDll.User32, EntryPoint = "GetClientRect", ExactSpelling = true, CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool IntGetClientRect(HandleRef hWnd, [In, Out] ref RECT rect);


        /// <SecurityNote>
        ///    Critical: This code calls into unmanaged code which elevates
        ///    TreatAsSafe: This method is ok to give out
        /// </SecurityNote>
        [SecurityCritical, SecurityTreatAsSafe]
        public static void GetClientRect(HandleRef hWnd, [In, Out] ref RECT rect)
        {
            if (!IntGetClientRect(hWnd, ref rect))
            {
                throw new Win32Exception();
            }
        }



        [DllImport(ExternDll.User32, EntryPoint = "GetWindowRect", ExactSpelling = true, CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool IntGetWindowRect(HandleRef hWnd, [In, Out] ref RECT rect);

        /// <SecurityNote>
        ///    Critical: This code calls into unmanaged code which elevates
        ///    TreatAsSafe: This method is ok to give out
        /// </SecurityNote>
        [SecurityCritical, SecurityTreatAsSafe]
        public static void GetWindowRect(HandleRef hWnd, [In, Out] ref RECT rect)
        {
            if (!IntGetWindowRect(hWnd, ref rect))
            {
                throw new Win32Exception();
            }
        }


        ///<SecurityNote>
        /// Critical as this code performs an elevation to unmanaged code
        ///</SecurityNote>


        [DllImport(ExternDll.User32, ExactSpelling = true, CharSet = CharSet.Auto)]
        public static extern IntPtr GetActiveWindow();



        // We have this wrapper because casting IntPtr to int may
        // generate OverflowException when one of high 32 bits is set.
        public static int IntPtrToInt32(IntPtr intPtr)
        {
            return unchecked((int)intPtr.ToInt64());
        }


        ///<SecurityNote>
        /// Critical - performs an elevation via SUC.
        ///</SecurityNote>


        [DllImport(ExternDll.User32, EntryPoint = "ClientToScreen", SetLastError = true, ExactSpelling = true, CharSet = CharSet.Auto)]
        public static extern int IntClientToScreen(HandleRef hWnd, [In, Out] POINT pt);

        ///<SecurityNote>
        ///     Critical calls critical code - IntClientToScreen
        ///</SecurityNote>

        public static void ClientToScreen(HandleRef hWnd, [In, Out] POINT pt)
        {
            if (IntClientToScreen(hWnd, pt) == 0)
            {
                throw new Win32Exception();
            }
        }


        [DllImport(ExternDll.Shell32, CharSet = CharSet.Auto)]
        [ResourceExposure(ResourceScope.None)]
        public static extern int Shell_NotifyIcon(int message, NOTIFYICONDATA pnid);


        [DllImport(ExternDll.User32, CharSet = CharSet.Auto)]
        [ResourceExposure(ResourceScope.None)]
        public static extern IntPtr PostMessage(HandleRef hwnd, int msg, int wparam, int lparam);


        [ReflectionPermission(SecurityAction.Assert, Unrestricted = true)]
        public static object PtrToStructure(IntPtr lparam, Type cls)
        {
            return Marshal.PtrToStructure(lparam, cls);
        }


        [DllImport(ExternDll.User32, CharSet = System.Runtime.InteropServices.CharSet.Auto, EntryPoint = "SetClassLongPtr")]
        [ResourceExposure(ResourceScope.None)]
        public static extern IntPtr SetClassLongPtr64(HandleRef hwnd, int nIndex, IntPtr dwNewLong);

        //SetWindowLong won't work correctly for 64-bit: we should use SetWindowLongPtr instead.  On
        //32-bit, SetWindowLongPtr is just #defined as SetWindowLong.  SetWindowLong really should 
        //take/return int instead of IntPtr/HandleRef, but since we're running this only for 32-bit
        //it'll be OK.
        public static IntPtr SetWindowLong(HandleRef hWnd, int nIndex, HandleRef dwNewLong)
        {
            if (IntPtr.Size == 4)
            {
                return SetWindowLongPtr32(hWnd, nIndex, dwNewLong);
            }
            return SetWindowLongPtr64(hWnd, nIndex, dwNewLong);
        }

        [DllImport(ExternDll.User32, CharSet = CharSet.Auto, EntryPoint = "SetWindowLong")]
        [ResourceExposure(ResourceScope.None)]
        public static extern IntPtr SetWindowLongPtr32(HandleRef hWnd, int nIndex, HandleRef dwNewLong);

        [DllImport(ExternDll.User32, CharSet = CharSet.Auto, EntryPoint = "SetWindowLongPtr")]
        [ResourceExposure(ResourceScope.None)]
        public static extern IntPtr SetWindowLongPtr64(HandleRef hWnd, int nIndex, HandleRef dwNewLong);


        [DllImport(ExternDll.Kernel32, ExactSpelling = true, CharSet = CharSet.Ansi)]
        [ResourceExposure(ResourceScope.Process)]
        public static extern IntPtr GetProcAddress(HandleRef hModule, string lpProcName);


        [DllImport(ExternDll.Kernel32, CharSet = CharSet.Auto)]
        [ResourceExposure(ResourceScope.Process)]
        public static extern IntPtr GetModuleHandle(string modName);


        [DllImport(ExternDll.Gdi32, SetLastError = true, ExactSpelling = true, CharSet = CharSet.Auto)]
        [ResourceExposure(ResourceScope.Process)]
        public static extern IntPtr GetStockObject(int nIndex);


        [DllImport(ExternDll.User32, CharSet = System.Runtime.InteropServices.CharSet.Auto, BestFitMapping = false)]
        [ResourceExposure(ResourceScope.None)]
        public static extern bool GetClassInfo(HandleRef hInst, string lpszClass, [In, Out] WNDCLASS_I wc);


        [DllImport(ExternDll.User32, CharSet = CharSet.Auto, SetLastError = true)]
        [ResourceExposure(ResourceScope.None)]
        public static extern short RegisterClass(WNDCLASS_D wc);

        [DllImport(ExternDll.User32, CharSet = CharSet.Auto, SetLastError = true)]
        [ResourceExposure(ResourceScope.None)]
        public static extern bool UnregisterClass(string className, HandleRef hInstance);


        [DllImport(ExternDll.User32, ExactSpelling = true, CharSet = CharSet.Auto)]
        public static extern bool IsWindow(HandleRef hWnd);



        [DllImport(ExternDll.User32, ExactSpelling = true, CharSet = System.Runtime.InteropServices.CharSet.Auto)]
        public static extern int GetWindowThreadProcessId(HandleRef hWnd, out int lpdwProcessId);


        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport(ExternDll.Kernel32, ExactSpelling = true, CharSet = System.Runtime.InteropServices.CharSet.Auto)]
        [ResourceExposure(ResourceScope.None)]
        public static extern bool GetExitCodeThread(HandleRef hWnd, out int lpdwExitCode);


        [DllImport(ExternDll.User32, CharSet = CharSet.Auto)]
        [ResourceExposure(ResourceScope.None)]
        public static extern IntPtr SendMessageTimeout(HandleRef hWnd, int msg, IntPtr wParam, IntPtr lParam, int flags, int timeout, out IntPtr pdwResult);


        [DllImport(ExternDll.User32, CharSet = CharSet.Auto)]
        [ResourceExposure(ResourceScope.None)]
        public static extern int RegisterWindowMessage(string msg);

        public static int wmUnSubclass = -1;
        public static int WM_UIUNSUBCLASS
        {
            get
            {
                if (wmUnSubclass == -1)
                {
                    wmUnSubclass = RegisterWindowMessage("WinFormsUnSubclass");
                }
                return wmUnSubclass;
            }
        }


        [DllImport(ExternDll.User32, EntryPoint = "CreateWindowEx", CharSet = CharSet.Auto, SetLastError = true)]
        [ResourceExposure(ResourceScope.Process)]
        public static extern IntPtr IntCreateWindowEx(int dwExStyle, string lpszClassName,
                                                string lpszWindowName, int style, int x, int y, int width, int height,
                                                HandleRef hWndParent, HandleRef hMenu, HandleRef hInst, [MarshalAs(UnmanagedType.AsAny)] object pvParam);
        [ResourceExposure(ResourceScope.Process)]
        [ResourceConsumption(ResourceScope.Process)]
        public static IntPtr CreateWindowEx(int dwExStyle, string lpszClassName,
                                         string lpszWindowName, int style, int x, int y, int width, int height,
                                         HandleRef hWndParent, HandleRef hMenu, HandleRef hInst, [MarshalAs(UnmanagedType.AsAny)] object pvParam)
        {
            return IntCreateWindowEx(dwExStyle, lpszClassName,
                                         lpszWindowName, style, x, y, width, height, hWndParent, hMenu,
                                         hInst, pvParam);
        }


        [DllImport(ExternDll.User32, CharSet = CharSet.Auto)]
        [ResourceExposure(ResourceScope.None)]
        public static extern IntPtr DefWindowProc(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);


        [DllImport(ExternDll.User32, CharSet = CharSet.Auto)]
        [ResourceExposure(ResourceScope.None)]
        public static extern IntPtr CallWindowProc(IntPtr wndProc, IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);


        [DllImport(ExternDll.User32, ExactSpelling = true, EntryPoint = "DestroyWindow", CharSet = CharSet.Auto)]
        [ResourceExposure(ResourceScope.None)]
        public static extern bool IntDestroyWindow(HandleRef hWnd);
        public static bool DestroyWindow(HandleRef hWnd)
        {
            return IntDestroyWindow(hWnd);
        }


        [DllImport(ExternDll.User32)]
        [ResourceExposure(ResourceScope.None)]
        public static extern int GetClassName(HandleRef hwnd, StringBuilder lpClassName, int nMaxCount);

        //SetClassLong won't work correctly for 64-bit: we should use SetClassLongPtr instead.  On
        //32-bit, SetClassLongPtr is just #defined as SetClassLong.  SetClassLong really should 
        //take/return int instead of IntPtr/HandleRef, but since we're running this only for 32-bit
        //it'll be OK.
        public static IntPtr SetClassLong(HandleRef hWnd, int nIndex, IntPtr dwNewLong)
        {
            if (IntPtr.Size == 4)
            {
                return SetClassLongPtr32(hWnd, nIndex, dwNewLong);
            }
            return SetClassLongPtr64(hWnd, nIndex, dwNewLong);
        }

        [DllImport(ExternDll.User32, CharSet = CharSet.Auto, EntryPoint = "SetClassLong")]
        [ResourceExposure(ResourceScope.None)]
        public static extern IntPtr SetClassLongPtr32(HandleRef hwnd, int nIndex, IntPtr dwNewLong);


        [DllImport(ExternDll.User32, CharSet = CharSet.Auto, EntryPoint = "SetWindowLong")]
        [ResourceExposure(ResourceScope.None)]
        public static extern IntPtr SetWindowLongPtr32(HandleRef hWnd, int nIndex, NativeMethods.WndProc wndproc);


        [DllImport(ExternDll.User32, CharSet = CharSet.Auto, EntryPoint = "SetWindowLongPtr")]
        [ResourceExposure(ResourceScope.None)]
        public static extern IntPtr SetWindowLongPtr64(HandleRef hWnd, int nIndex, NativeMethods.WndProc wndproc);

        public static IntPtr SetWindowLong(HandleRef hWnd, int nIndex, NativeMethods.WndProc wndproc)
        {
            if (IntPtr.Size == 4)
            {
                return SetWindowLongPtr32(hWnd, nIndex, wndproc);
            }
            return SetWindowLongPtr64(hWnd, nIndex, wndproc);
        }


        // for Windows vista to windows 8.
        [DllImport(ExternDll.User32, SetLastError = true)]
        [ResourceExposure(ResourceScope.None)]
        public static extern bool SetProcessDPIAware();

        // for windows 8.1 and above 
        [DllImport(ExternDll.ShCore, SetLastError = true)]
        [ResourceExposure(ResourceScope.None)]
        public static extern int SetProcessDpiAwareness(PROCESS_DPI_AWARENESS awareness);

        // for Windows 10 version RS2 and above
        [DllImport(ExternDll.User32, SetLastError = true)]
        [ResourceExposure(ResourceScope.None)]
        public static extern bool SetProcessDpiAwarenessContext(int dpiFlag);

        // Available in Windows 10 version RS1 and above.
        [DllImport(ExternDll.User32)]
        [ResourceExposure(ResourceScope.None)]
        public static extern int GetThreadDpiAwarenessContext();

        // Available in Windows 10 version RS1 and above.
        [DllImport(ExternDll.User32)]
        [ResourceExposure(ResourceScope.None)]
        public static extern bool AreDpiAwarenessContextsEqual(int dpiContextA, int dpiContextB);


        [DllImport(ExternDll.User32, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool AreDpiAwarenessContextsEqual([In] IntPtr dpiContextA, [In] IntPtr dpiContextB);

        [DllImport(ExternDll.User32, ExactSpelling = true, CharSet = CharSet.Auto)]
        [ResourceExposure(ResourceScope.None)]
        public static extern bool GetCursorPos([In, Out] POINT pt);


        [DllImport(ExternDll.User32, ExactSpelling = true, CharSet = CharSet.Auto)]
        [ResourceExposure(ResourceScope.None)]
        public static extern bool SetForegroundWindow(HandleRef hWnd);

        [DllImport(ExternDll.User32, ExactSpelling = true, CharSet = System.Runtime.InteropServices.CharSet.Auto)]
        [ResourceExposure(ResourceScope.None)]
        public static extern bool TrackPopupMenuEx(HandleRef hmenu, int fuFlags, int x, int y, HandleRef hwnd, TPMPARAMS tpm);


        [DllImport(ExternDll.User32, ExactSpelling = true, CharSet = System.Runtime.InteropServices.CharSet.Auto)]
        [ResourceExposure(ResourceScope.None)]
        public static extern bool SetWindowPos(HandleRef hWnd, HandleRef hWndInsertAfter,
                                              int x, int y, int cx, int cy, int flags);


        [DllImport(ExternDll.Kernel32, CharSet = CharSet.Auto)]
        [ResourceExposure(ResourceScope.None)]
        public static extern int lstrlen(String s);

        //GetWindowLong won't work correctly for 64-bit: we should use GetWindowLongPtr instead.  On
        //32-bit, GetWindowLongPtr is just #defined as GetWindowLong.  GetWindowLong really should 
        //take/return int instead of IntPtr/HandleRef, but since we're running this only for 32-bit
        //it'll be OK.
        public static IntPtr GetWindowLong(HandleRef hWnd, int nIndex)
        {
            if (IntPtr.Size == 4)
            {
                return GetWindowLong32(hWnd, nIndex);
            }
            return GetWindowLongPtr64(hWnd, nIndex);
        }


        [DllImport(ExternDll.User32, CharSet = CharSet.Auto, EntryPoint = "GetWindowLong")]
        [ResourceExposure(ResourceScope.None)]
        public static extern IntPtr GetWindowLong32(HandleRef hWnd, int nIndex);

        [DllImport(ExternDll.User32, CharSet = CharSet.Auto, EntryPoint = "GetWindowLongPtr")]
        [ResourceExposure(ResourceScope.None)]
        public static extern IntPtr GetWindowLongPtr64(HandleRef hWnd, int nIndex);


        [DllImport(ExternDll.User32)]
        public static extern uint SHAppBarMessage(uint dwMessage, ref APPBARDATA data);

        [DllImport(ExternDll.User32)]
        public static extern int SystemParametersInfo(uint uiAction, uint uiParam, IntPtr pvParam, uint fWinIni);
        

        public static IntPtr GetWindowStyle(HandleRef hWnd, bool exStyle)
        {
            int nIndex = (exStyle ? (-20) : (-16));
            return NativeMethods.GetWindowLong(hWnd, nIndex);
        }

        [DllImport(ExternDll.User32, CharSet = CharSet.Auto, EntryPoint = "ScreenToClient", ExactSpelling = true, SetLastError = true)]
        public static extern int IntScreenToClient(HandleRef hWnd, [In][Out] POINT pt);

        public static void ScreenToClient(HandleRef hWnd, [In][Out] POINT pt)
        {
            if (NativeMethods.IntScreenToClient(hWnd, pt) == 0)
            {
                throw new Win32Exception();
            }
        }

        [DllImport(ExternDll.User32, CharSet = CharSet.Auto, EntryPoint = "GetCursorPos", ExactSpelling = true)]

        public static extern bool IntTryGetCursorPos([In][Out] POINT pt);

        public static bool TryGetCursorPos([In][Out] POINT pt)
        {
            bool flag = IntTryGetCursorPos(pt);
            if (!flag)
            {
                pt.x = 0;
                pt.y = 0;
            }
            return flag;
        }


        [DllImport(ExternDll.User32, CharSet = CharSet.Auto, ExactSpelling = true)]

        public static extern bool ShowWindow(HandleRef hWnd, int nCmdShow);

        [DllImport(ExternDll.User32, CharSet = CharSet.Auto, ExactSpelling = true, SetLastError = true)]
        public static extern IntPtr GetWindow(HandleRef hWnd, int uCmd);

    

        [DllImport(ExternDll.User32, CharSet = CharSet.Auto, ExactSpelling = true)]
        public static extern IntPtr GetForegroundWindow();


        [DllImport(ExternDll.Kernel32, CharSet = CharSet.Unicode)]
        public static extern IntPtr LoadLibrary(string lpFileName);


        [DllImport(ExternDll.PresentationNativeDll, CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool IsWindows10RS5OrGreater();

        [DllImport(ExternDll.PresentationNativeDll, CallingConvention = CallingConvention.Cdecl)]

        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool IsWindows10RS4OrGreater();

        [DllImport(ExternDll.PresentationNativeDll, CallingConvention = CallingConvention.Cdecl)]

        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool IsWindows10RS3OrGreater();

        [DllImport(ExternDll.PresentationNativeDll, CallingConvention = CallingConvention.Cdecl)]

        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool IsWindows10RS2OrGreater();

        [DllImport(ExternDll.PresentationNativeDll, CallingConvention = CallingConvention.Cdecl)]

        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool IsWindows10RS1OrGreater();

        [DllImport(ExternDll.PresentationNativeDll, CallingConvention = CallingConvention.Cdecl)]

        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool IsWindows10TH2OrGreater();

        [DllImport(ExternDll.PresentationNativeDll, CallingConvention = CallingConvention.Cdecl)]

        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool IsWindows10TH1OrGreater();

        [DllImport(ExternDll.PresentationNativeDll, CallingConvention = CallingConvention.Cdecl)]

        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool IsWindows10OrGreater();

        [DllImport(ExternDll.PresentationNativeDll, CallingConvention = CallingConvention.Cdecl)]

        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool IsWindows8Point1OrGreater();

        [DllImport(ExternDll.PresentationNativeDll, CallingConvention = CallingConvention.Cdecl)]

        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool IsWindows8OrGreater();

        [DllImport(ExternDll.PresentationNativeDll, CallingConvention = CallingConvention.Cdecl)]

        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool IsWindows7SP1OrGreater();

        [DllImport(ExternDll.PresentationNativeDll, CallingConvention = CallingConvention.Cdecl)]

        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool IsWindows7OrGreater();

        [DllImport(ExternDll.PresentationNativeDll, CallingConvention = CallingConvention.Cdecl)]

        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool IsWindowsVistaSP2OrGreater();

        [DllImport(ExternDll.PresentationNativeDll, CallingConvention = CallingConvention.Cdecl)]

        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool IsWindowsVistaSP1OrGreater();

        [DllImport(ExternDll.PresentationNativeDll, CallingConvention = CallingConvention.Cdecl)]

        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool IsWindowsVistaOrGreater();

        [DllImport(ExternDll.PresentationNativeDll, CallingConvention = CallingConvention.Cdecl)]

        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool IsWindowsXPSP3OrGreater();

        [DllImport(ExternDll.PresentationNativeDll, CallingConvention = CallingConvention.Cdecl)]

        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool IsWindowsXPSP2OrGreater();

        [DllImport(ExternDll.PresentationNativeDll, CallingConvention = CallingConvention.Cdecl)]

        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool IsWindowsXPSP1OrGreater();

        [DllImport(ExternDll.PresentationNativeDll, CallingConvention = CallingConvention.Cdecl)]

        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool IsWindowsXPOrGreater();

        [DllImport(ExternDll.PresentationNativeDll, CallingConvention = CallingConvention.Cdecl)]

        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool IsWindowsServer();

        [SecuritySafeCritical]
        public static DpiAwarenessContextHandle GetWindowDpiAwarenessContext(IntPtr hwnd)
        {
            return NativeMethods.GetWindowDpiAwarenessContext(hwnd);
        }

        [DllImport(ExternDll.User32)]

        public static extern DPI_HOSTING_BEHAVIOR GetThreadDpiHostingBehavior();


        [DllImport(ExternDll.User32)]

        public static extern DpiAwarenessContextHandle SetThreadDpiAwarenessContext(DpiAwarenessContextHandle dpiContext);


        [DllImport(ExternDll.User32, CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool IsProcessDPIAware();


        [DllImport(ExternDll.Kernel32, CharSet = CharSet.Auto, ExactSpelling = true)]
        public static extern int GetCurrentProcessId();


        [DllImport(ExternDll.Kernel32, CharSet = CharSet.Auto, SetLastError = true)]
        public static extern IntPtr OpenProcess(int dwDesiredAccess, bool fInherit, int dwProcessId);


        [DllImport(ExternDll.ShCore)]
        internal static extern uint GetProcessDpiAwareness([In] HandleRef hProcess, out IntPtr awareness);


        public static PROCESS_DPI_AWARENESS GetProcessDpiAwareness(HandleRef hProcess)
        {
            IntPtr awareness = IntPtr.Zero;
            int processDpiAwareness = (int)NativeMethods.GetProcessDpiAwareness(hProcess, out awareness);
            if (processDpiAwareness != 0)
            {
                Marshal.ThrowExceptionForHR(processDpiAwareness);
            }

            return (PROCESS_DPI_AWARENESS)NativeMethods.IntPtrToInt32(awareness);
        }



        [DllImport(ExternDll.User32)]
        internal static extern uint GetDpiForSystem();


        [DllImport(ExternDll.User32, CharSet = CharSet.Unicode)]
        internal static extern uint GetDpiForWindow([In] HandleRef hwnd);


        [DllImport(ExternDll.ShCore, CharSet = CharSet.Auto, SetLastError = true)]
        public static extern uint GetDpiForMonitor(HandleRef hMonitor, MONITOR_DPI_TYPE dpiType, out uint dpiX, out uint dpiY);


        [DllImport(ExternDll.User32)]
        public static extern bool IsWinEventHookInstalled(int winevent);



        [DllImport(ExternDll.Kernel32, BestFitMapping = false, CharSet = CharSet.Ansi, EntryPoint = "GetProcAddress")]
        public static extern IntPtr GetProcAddressNoThrow(HandleRef hModule, string lpProcName);


        [DllImport(ExternDll.User32, ExactSpelling = true)]
        public static extern IntPtr MonitorFromPoint(POINTSTRUCT pt, int flags);


        [DllImport(ExternDll.User32, ExactSpelling = true)]
        public static extern IntPtr MonitorFromRect(ref RECT rect, int flags);

        [DllImport(ExternDll.User32, CharSet = CharSet.Auto, ExactSpelling = true)]
        public static extern IntPtr GetCursor();


        [DllImport(ExternDll.User32, CharSet = CharSet.Auto, EntryPoint = "GetIconInfo", SetLastError = true)]
        public static extern bool GetIconInfoImpl(HandleRef hIcon, [Out] ICONINFO_IMPL piconinfo);


        public static void GetIconInfo(HandleRef hIcon, out ICONINFO piconinfo)
        {
            bool flag = false;
            int num = 0;
            piconinfo = new ICONINFO();
            ICONINFO_IMPL iCONINFO_IMPL = new ICONINFO_IMPL();
            try
            {
            }
            finally
            {
                flag = GetIconInfoImpl(hIcon, iCONINFO_IMPL);
                num = Marshal.GetLastWin32Error();
                if (flag)
                {
                    piconinfo.hbmMask = BitmapHandle.CreateFromHandle(iCONINFO_IMPL.hbmMask);
                    piconinfo.hbmColor = BitmapHandle.CreateFromHandle(iCONINFO_IMPL.hbmColor);
                    piconinfo.fIcon = iCONINFO_IMPL.fIcon;
                    piconinfo.xHotspot = iCONINFO_IMPL.xHotspot;
                    piconinfo.yHotspot = iCONINFO_IMPL.yHotspot;
                }
            }

            if (!flag)
            {
                throw new Win32Exception();
            }
        }


        [DllImport(ExternDll.User32, CharSet = CharSet.Auto, EntryPoint = "DestroyCursor", ExactSpelling = true)]
        public static extern bool IntDestroyCursor(IntPtr hCurs);


        [DllImport(ExternDll.Gdi32, CharSet = CharSet.Auto, SetLastError = true)]
        public static extern int GetObject(HandleRef hObject, int nSize, [In][Out] BITMAP bm);


        [DllImport(ExternDll.Gdi32, CharSet = CharSet.Auto, ExactSpelling = true, SetLastError = true)]
        public static extern int GetBitmapBits(HandleRef hbmp, int cbBuffer, byte[] lpvBits);


        [DllImport(ExternDll.Uxtheme, CharSet = CharSet.Unicode)]

        public static extern bool BeginPanningFeedback(HandleRef hwnd);

        [DllImport(ExternDll.Uxtheme, CharSet = CharSet.Unicode)]
        public static extern bool UpdatePanningFeedback(HandleRef hwnd, int lTotalOverpanOffsetX, int lTotalOverpanOffsetY, bool fInInertia);

        [DllImport(ExternDll.Uxtheme, CharSet = CharSet.Unicode)]
        public static extern bool EndPanningFeedback(HandleRef hwnd, bool fAnimateBack);

        [DllImport(ExternDll.User32)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool EnumDisplayMonitors(IntPtr hdc, IntPtr lprcClip, NativeMethods.MonitorEnumProc lpfnEnum, IntPtr lParam);



        /// <include file='doc\NativeMethods.uex' path='docs/doc[@for="NativeMethods.Util.GetPInvokeStringLength"]/*' />
        /// <devdoc>
        ///     Computes the string size that should be passed to a typical Win32 call.
        ///     This will be the character count under NT, and the ubyte count for Windows 95.
        /// </devdoc>
        public static int GetPInvokeStringLength(String s)
        {
            if (s == null)
            {
                return 0;
            }

            if (Marshal.SystemDefaultCharSize == 2)
            {
                return s.Length;
            }
            else
            {
                if (s.Length == 0)
                {
                    return 0;
                }
                if (s.IndexOf('\0') > -1)
                {
                    return GetEmbeddedNullStringLengthAnsi(s);
                }
                else
                {
                    return lstrlen(s);
                }
            }
        }

        private static int GetEmbeddedNullStringLengthAnsi(String s)
        {
            int n = s.IndexOf('\0');
            if (n > -1)
            {
                String left = s.Substring(0, n);
                String right = s.Substring(n + 1);
                return GetPInvokeStringLength(left) + GetEmbeddedNullStringLengthAnsi(right) + 1;
            }
            else
            {
                return GetPInvokeStringLength(s);
            }
        }


        [DllImport(ExternDll.User32, CharSet = CharSet.Auto, ExactSpelling = true)]
        public static extern bool EnumThreadWindows(int dwThreadId, NativeMethods.EnumThreadWindowsCallback lpfn, HandleRef lParam);


        [DllImport(ExternDll.Kernel32, CharSet = CharSet.Auto, ExactSpelling = true)]
        public static extern int GetCurrentThreadId();


        [DllImport(ExternDll.User32, CharSet = CharSet.Auto, ExactSpelling = true)]
        public static extern IntPtr GetCapture();

        [DllImport(ExternDll.User32, CharSet = CharSet.Auto, EntryPoint = "ReleaseCapture", ExactSpelling = true, SetLastError = true)]
        public static extern bool IntReleaseCapture();


        public static bool ReleaseCapture()
        {
            bool flag = NativeMethods.IntReleaseCapture();
            if (!flag)
            {
                throw new Win32Exception();
            }

            return flag;
        }


        [DllImport(ExternDll.User32, CharSet = CharSet.Auto, ExactSpelling = true)]
        public static extern IntPtr GetDesktopWindow();

     
      

        [DllImport(ExternDll.DwmAPI, BestFitMapping = false)]
        public static extern int DwmIsCompositionEnabled(out int enabled);

        [DllImport(ExternDll.User32, CharSet = CharSet.Auto, EntryPoint = "SendMessage")]
        public static extern IntPtr UnsafeSendMessage(IntPtr hWnd, WM msg, IntPtr wParam, IntPtr lParam);

        [DllImport(ExternDll.User32, EntryPoint = "ChangeWindowMessageFilterEx", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool IntChangeWindowMessageFilterEx(IntPtr hwnd, WM message, MSGFLT action, [Optional][In][Out] ref CHANGEFILTERSTRUCT pChangeFilterStruct);

        [DllImport(ExternDll.User32, EntryPoint = "ChangeWindowMessageFilter", SetLastError = true)]
        

        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool IntChangeWindowMessageFilter(WM message, MSGFLT dwFlag);


        public static HRESULT ChangeWindowMessageFilterEx(IntPtr hwnd, WM message, MSGFLT action, out MSGFLTINFO extStatus)
        {
            extStatus = MSGFLTINFO.NONE;
            if (!Utility.IsOSVistaOrNewer)
            {
                return HRESULT.S_FALSE;
            }

            if (!Utility.IsOSWindows7OrNewer)
            {
                if (!IntChangeWindowMessageFilter(message, action))
                {
                    return (HRESULT)Win32Error.GetLastError();
                }

                return HRESULT.S_OK;
            }

            CHANGEFILTERSTRUCT cHANGEFILTERSTRUCT = default(CHANGEFILTERSTRUCT);
            cHANGEFILTERSTRUCT.cbSize = (uint)Marshal.SizeOf(typeof(CHANGEFILTERSTRUCT));
            CHANGEFILTERSTRUCT pChangeFilterStruct = cHANGEFILTERSTRUCT;
            if (!IntChangeWindowMessageFilterEx(hwnd, message, action, ref pChangeFilterStruct))
            {
                return (HRESULT)Win32Error.GetLastError();
            }

            extStatus = pChangeFilterStruct.ExtStatus;
            return HRESULT.S_OK;
        }


        [DllImport(ExternDll.User32, BestFitMapping = false, CharSet = CharSet.Auto, EntryPoint = "SetWindowText", SetLastError = true)]
        public static extern bool IntSetWindowText(HandleRef hWnd, string text);

        public static void SetWindowText(HandleRef hWnd, string text)
        {
            if (!IntSetWindowText(hWnd, text))
            {
                throw new Win32Exception();
            }
        }


        [DllImport(ExternDll.User32, CharSet = CharSet.Auto, ExactSpelling = true)]
        public static extern bool IsWindowVisible(HandleRef hWnd);
      

        [DllImport(ExternDll.User32, CharSet = CharSet.Auto, ExactSpelling = true)]
        public static extern bool IsWindowEnabled(HandleRef hWnd);

        [DllImport(ExternDll.User32, CharSet = CharSet.Auto, EntryPoint = "GetWindowPlacement", ExactSpelling = true, SetLastError = true)]
        public static extern bool IntGetWindowPlacement(HandleRef hWnd, ref WINDOWPLACEMENT placement);


        public static void GetWindowPlacement(HandleRef hWnd, ref WINDOWPLACEMENT placement)
        {
            if (!IntGetWindowPlacement(hWnd, ref placement))
            {
                throw new Win32Exception();
            }
        }

        [DllImport(ExternDll.User32, CharSet = CharSet.Auto, SetLastError = true)]
        public static extern IntPtr SendMessage(HandleRef hWnd, WM msg, IntPtr wParam, IconHandle iconHandle);

        [DllImport(ExternDll.User32, SetLastError = true)]
        public static extern IntPtr SetActiveWindow(HandleRef hWnd);

        [DllImport(ExternDll.User32, CharSet = CharSet.Auto, EntryPoint = "SetWindowPlacement", ExactSpelling = true, SetLastError = true)]
        public static extern bool IntSetWindowPlacement(HandleRef hWnd, [In] ref WINDOWPLACEMENT placement);

        public static void SetWindowPlacement(HandleRef hWnd, [In] ref WINDOWPLACEMENT placement)
        {
            if (!IntSetWindowPlacement(hWnd, ref placement))
            {
                throw new Win32Exception();
            }
        }


        [DllImport(ExternDll.User32, CharSet = CharSet.Auto, EntryPoint = "PostMessage", SetLastError = true)]
        public static extern bool IntPostMessage(HandleRef hwnd, WM msg, IntPtr wparam, IntPtr lparam);

        public static void PostMessage(HandleRef hwnd, WM msg, IntPtr wparam, IntPtr lparam)
        {
            if (!IntPostMessage(hwnd, msg, wparam, lparam))
            {
                throw new Win32Exception();
            }
        }


        [DllImport(ExternDll.User32)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool IsIconic(IntPtr hWnd);


        [DllImport(ExternDll.User32, ExactSpelling = true, CharSet = System.Runtime.InteropServices.CharSet.Auto)]
        public static extern int GetMessagePos();


        [DllImport(ExternDll.User32, EntryPoint = "WindowFromPoint", ExactSpelling = true, CharSet = CharSet.Auto)]
        public static extern IntPtr IntWindowFromPoint(POINTSTRUCT pt);
        public static IntPtr WindowFromPoint(int x, int y)
        {
            POINTSTRUCT ps = new POINTSTRUCT(x, y);
            return IntWindowFromPoint(ps);
        }

        [DllImport(ExternDll.User32, ExactSpelling = true, SetLastError = true)]
        public static extern bool TrackMouseEvent(TRACKMOUSEEVENT tme);



        public static bool HandleTrackMouseEvent(TRACKMOUSEEVENT tme)
        {
            bool retVal = NativeMethods.TrackMouseEvent(tme);
            int win32Err = Marshal.GetLastWin32Error(); // Dance around FxCop
            if (!retVal && win32Err != 0)
            {
                throw new System.ComponentModel.Win32Exception(win32Err);
            }
            return retVal;
        }

        [DllImport(ExternDll.User32, CharSet = CharSet.Auto)]
        public static extern IntPtr GetMessageExtraInfo();


        [DllImport(ExternDll.User32, ExactSpelling = true, CharSet = CharSet.Auto)]
        public static extern IntPtr SetCursor(HandleRef hcursor);

        [DllImport(ExternDll.User32, ExactSpelling = true, CharSet = CharSet.Auto)]
        public static extern IntPtr SetCursor(SafeHandle hcursor);

        [DllImport(ExternDll.User32, ExactSpelling = true, CharSet = CharSet.Auto)]
        public static extern IntPtr SetCapture(HandleRef hwnd);


        ///<SecurityNote>
        /// Critical as this code performs an elevation.
        ///</SecurityNote>
        [DllImport(ExternDll.User32, ExactSpelling = true, CharSet = CharSet.Auto, SetLastError = true)]
        public static extern int GetMouseMovePointsEx(
                                        uint cbSize,
                                        [In] ref MOUSEMOVEPOINT pointsIn,
                                        [Out] MOUSEMOVEPOINT[] pointsBufferOut,
                                        int nBufPoints,
                                        uint resolution
                                   );


        [DllImport(ExternDll.User32, ExactSpelling = true, CharSet = System.Runtime.InteropServices.CharSet.Auto)]
        public static extern int GetMessageTime();


        [DllImport(ExternDll.User32, EntryPoint = "LoadImage", CharSet = System.Runtime.InteropServices.CharSet.Auto, SetLastError = true, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        public static extern CursorHandle LoadImageCursor(IntPtr hinst, string stName, int nType, int cxDesired, int cyDesired, int nFlags);


        [DllImport(ExternDll.User32, CharSet = System.Runtime.InteropServices.CharSet.Auto, SetLastError = true)]
        public static extern CursorHandle LoadCursor(HandleRef hInst, IntPtr iconId);


        /// <SecurityNote>
        ///    Critical: This code calls into unmanaged code which elevates
        ///    TreatAsSafe: This method is ok to give out
        /// </SecurityNote>
        public static CursorHandle HandleLoadCursor(HandleRef hInst, IntPtr iconId)
        {
            CursorHandle cursorHandle = NativeMethods.LoadCursor(hInst, iconId);
            if (cursorHandle == null || cursorHandle.IsInvalid)
            {
                throw new Win32Exception();
            }
            return cursorHandle;
        }


        [DllImport(ExternDll.Shell32, ExactSpelling = true)]
        public static extern void ILFree(IntPtr pidlList);

        [DllImport(ExternDll.Shell32, CharSet = CharSet.Unicode, ExactSpelling = true)]
        public static extern IntPtr ILCreateFromPathW(string pszPath);

        [DllImport(ExternDll.Shell32, ExactSpelling = true)]
        public static extern int SHOpenFolderAndSelectItems(IntPtr pidlList, uint cild, IntPtr children, uint dwFlags);



        [DllImport(ExternDll.User32)]
        
        public static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);

        [DllImport(ExternDll.User32)]
        public static extern SafeDC GetDC(IntPtr hwnd);

        [DllImport(ExternDll.User32, CharSet = CharSet.Unicode)]
        
        public static extern SafeDC CreateDC([MarshalAs(UnmanagedType.LPWStr)] string lpszDriver, [MarshalAs(UnmanagedType.LPWStr)] string lpszDevice, IntPtr lpszOutput, IntPtr lpInitData);

        [DllImport(ExternDll.User32, CharSet = CharSet.Unicode, SetLastError = true)]
        
        public static extern SafeDC CreateCompatibleDC(IntPtr hdc);

        [DllImport(ExternDll.User32)]
        
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool DeleteDC(IntPtr hdc);


        [DllImport(ExternDll.User32, EntryPoint = "SetWindowPos", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool _SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int x, int y, int cx, int cy, int uFlags);

        public static bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int x, int y, int cx, int cy, int uFlags)
        {
            if (!_SetWindowPos(hWnd, hWndInsertAfter, x, y, cx, cy, uFlags))
            {
                return false;
            }

            return true;
        }


        [DllImport(ExternDll.User32, EntryPoint = "GetWindowRect", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool _GetWindowRect(IntPtr hWnd, out RECT lpRect);

        public static RECT GetWindowRect(IntPtr hwnd)
        {
            if (!_GetWindowRect(hwnd, out var lpRect))
            {
                HRESULT.ThrowLastError();
            }

            return lpRect;
        }


        [DllImport(ExternDll.User32, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetWindowPlacement(IntPtr hwnd, WINDOWPLACEMENT lpwndpl);

        public static WINDOWPLACEMENT GetWindowPlacement(IntPtr hwnd)
        {
            WINDOWPLACEMENT wINDOWPLACEMENT = new WINDOWPLACEMENT();
            if (GetWindowPlacement(hwnd, wINDOWPLACEMENT))
            {
                return wINDOWPLACEMENT;
            }

            throw new Win32Exception();
        }


     

        [DllImport(ExternDll.User32, EntryPoint = "AdjustWindowRectEx", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool _AdjustWindowRectEx(ref RECT lpRect, IntPtr dwStyle, [MarshalAs(UnmanagedType.Bool)] bool bMenu, IntPtr dwExStyle);

        
        public static RECT AdjustWindowRectEx(RECT lpRect, IntPtr dwStyle, bool bMenu, IntPtr dwExStyle)
        {
            if (!_AdjustWindowRectEx(ref lpRect, dwStyle, bMenu, dwExStyle))
            {
                HRESULT.ThrowLastError();
            }

            return lpRect;
        }

       


        /// <SecurityNote>
        ///   Critical : Calls critical method
        /// </SecurityNote>
        [DllImport(ExternDll.Kernel32)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool FindClose(IntPtr handle);


        /// <SecurityNote>
        ///   Critical : P-Invokes
        /// </SecurityNote>


        [DllImport(ExternDll.User32, EntryPoint = "AdjustWindowRectEx", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool _AdjustWindowRectEx(ref RECT lpRect, WS dwStyle, [MarshalAs(UnmanagedType.Bool)] bool bMenu, WS_EX dwExStyle);

        /// <SecurityNote>
        ///   Critical : Calls critical method
        /// </SecurityNote>

        public static RECT AdjustWindowRectEx(RECT lpRect, WS dwStyle, bool bMenu, WS_EX dwExStyle)
        {
            // Native version modifies the parameter in place.
            if (!_AdjustWindowRectEx(ref lpRect, dwStyle, bMenu, dwExStyle))
            {
                HRESULT.ThrowLastError();
            }

            return lpRect;
        }

        /// <SecurityNote>
        ///   Critical : P-Invokes
        /// </SecurityNote>


        [DllImport(ExternDll.User32, EntryPoint = "ChangeWindowMessageFilter", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool _ChangeWindowMessageFilter(WM message, MSGFLT dwFlag);

        /// <SecurityNote>
        ///   Critical : P-Invokes
        /// </SecurityNote>


        [DllImport(ExternDll.User32, EntryPoint = "ChangeWindowMessageFilterEx", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool _ChangeWindowMessageFilterEx(IntPtr hwnd, WM message, MSGFLT action, [In, Out, Optional] ref CHANGEFILTERSTRUCT pChangeFilterStruct);



        /// <SecurityNote>
        ///   Critical : P-Invokes
        /// </SecurityNote>
        [DllImport(ExternDll.Gdi32)]
        public static extern CombineRgnResult CombineRgn(IntPtr hrgnDest, IntPtr hrgnSrc1, IntPtr hrgnSrc2, RGN fnCombineMode);

        /// <SecurityNote>
        ///   Critical : P-Invokes
        /// </SecurityNote>


        [DllImport(ExternDll.Shell32, EntryPoint = "CommandLineToArgvW", CharSet = CharSet.Unicode)]
        private static extern IntPtr _CommandLineToArgvW([MarshalAs(UnmanagedType.LPWStr)] string cmdLine, out int numArgs);

        /// <SecurityNote>
        ///   Critical : Calls critical method
        /// </SecurityNote>


        public static string[] CommandLineToArgvW(string cmdLine)
        {
            IntPtr argv = IntPtr.Zero;
            try
            {
                int numArgs = 0;

                argv = _CommandLineToArgvW(cmdLine, out numArgs);
                if (argv == IntPtr.Zero)
                {
                    throw new Win32Exception();
                }
                var result = new string[numArgs];

                for (int i = 0; i < numArgs; i++)
                {
                    IntPtr currArg = Marshal.ReadIntPtr(argv, i * Marshal.SizeOf(typeof(IntPtr)));
                    result[i] = Marshal.PtrToStringUni(currArg);
                }

                return result;
            }
            finally
            {

                IntPtr p = _LocalFree(argv);
                // Otherwise LocalFree failed.
                Assert.AreEqual(IntPtr.Zero, p);
            }
        }

        /// <SecurityNote>
        ///   Critical : P-Invokes
        /// </SecurityNote>


        [DllImport(ExternDll.Gdi32, EntryPoint = "CreateDIBSection", SetLastError = true)]
        private static extern SafeHBITMAP _CreateDIBSection(SafeDC hdc, [In] ref BITMAPINFO bitmapInfo, int iUsage, [Out] out IntPtr ppvBits, IntPtr hSection, int dwOffset);

        /// <SecurityNote>
        ///   Critical : P-Invokes
        /// </SecurityNote>


        [DllImport(ExternDll.Gdi32, EntryPoint = "CreateDIBSection", SetLastError = true)]
        private static extern SafeHBITMAP _CreateDIBSectionIntPtr(IntPtr hdc, [In] ref BITMAPINFO bitmapInfo, int iUsage, [Out] out IntPtr ppvBits, IntPtr hSection, int dwOffset);

        /// <SecurityNote>
        ///   Critical : Calls critical method
        /// </SecurityNote>


        public static SafeHBITMAP CreateDIBSection(SafeDC hdc, ref BITMAPINFO bitmapInfo, out IntPtr ppvBits, IntPtr hSection, int dwOffset)
        {
            const int DIB_RGB_COLORS = 0;
            SafeHBITMAP hBitmap = null;
            if (hdc == null)
            {
                hBitmap = _CreateDIBSectionIntPtr(IntPtr.Zero, ref bitmapInfo, DIB_RGB_COLORS, out ppvBits, hSection, dwOffset);
            }
            else
            {
                hBitmap = _CreateDIBSection(hdc, ref bitmapInfo, DIB_RGB_COLORS, out ppvBits, hSection, dwOffset);
            }

            if (hBitmap.IsInvalid)
            {
                HRESULT.ThrowLastError();
            }

            return hBitmap;
        }

        /// <SecurityNote>
        ///   Critical : P-Invokes
        /// </SecurityNote>


        [DllImport(ExternDll.Gdi32, EntryPoint = "CreateRoundRectRgn", SetLastError = true)]
        private static extern IntPtr _CreateRoundRectRgn(int nLeftRect, int nTopRect, int nRightRect, int nBottomRect, int nWidthEllipse, int nHeightEllipse);

        /// <SecurityNote>
        ///   Critical : Calls critical method
        /// </SecurityNote>


        public static IntPtr CreateRoundRectRgn(int nLeftRect, int nTopRect, int nRightRect, int nBottomRect, int nWidthEllipse, int nHeightEllipse)
        {
            IntPtr ret = _CreateRoundRectRgn(nLeftRect, nTopRect, nRightRect, nBottomRect, nWidthEllipse, nHeightEllipse);
            if (IntPtr.Zero == ret)
            {
                throw new Win32Exception();
            }
            return ret;
        }

        /// <SecurityNote>
        ///   Critical : P-Invokes
        /// </SecurityNote>


        [DllImport(ExternDll.Gdi32, EntryPoint = "CreateRectRgn", SetLastError = true)]
        private static extern IntPtr _CreateRectRgn(int nLeftRect, int nTopRect, int nRightRect, int nBottomRect);

        /// <SecurityNote>
        ///   Critical : Calls critical method
        /// </SecurityNote>


        public static IntPtr CreateRectRgn(int nLeftRect, int nTopRect, int nRightRect, int nBottomRect)
        {
            IntPtr ret = _CreateRectRgn(nLeftRect, nTopRect, nRightRect, nBottomRect);
            if (IntPtr.Zero == ret)
            {
                throw new Win32Exception();
            }
            return ret;
        }

        /// <SecurityNote>
        ///   Critical : P-Invokes
        /// </SecurityNote>


        [DllImport(ExternDll.Gdi32, EntryPoint = "CreateRectRgnIndirect", SetLastError = true)]
        private static extern IntPtr _CreateRectRgnIndirect([In] ref RECT lprc);

        /// <SecurityNote>
        ///   Critical : Calls critical method
        /// </SecurityNote>


        public static IntPtr CreateRectRgnIndirect(RECT lprc)
        {
            IntPtr ret = _CreateRectRgnIndirect(ref lprc);
            if (IntPtr.Zero == ret)
            {
                throw new Win32Exception();
            }
            return ret;
        }

        /// <SecurityNote>
        ///   Critical : P-Invokes
        /// </SecurityNote>


        [DllImport(ExternDll.Gdi32)]
        public static extern IntPtr CreateSolidBrush(int crColor);

        /// <SecurityNote>
        ///   Critical : P-Invokes
        /// </SecurityNote>


        [DllImport(ExternDll.User32, SetLastError = true, CharSet = CharSet.Unicode, EntryPoint = "CreateWindowExW")]
        private static extern IntPtr _CreateWindowEx(
            WS_EX dwExStyle,
            [MarshalAs(UnmanagedType.LPWStr)] string lpClassName,
            [MarshalAs(UnmanagedType.LPWStr)] string lpWindowName,
            WS dwStyle,
            int x,
            int y,
            int nWidth,
            int nHeight,
            IntPtr hWndParent,
            IntPtr hMenu,
            IntPtr hInstance,
            IntPtr lpParam);

        /// <SecurityNote>
        ///   Critical : Calls critical method
        /// </SecurityNote>


        public static IntPtr CreateWindowEx(
            WS_EX dwExStyle,
            string lpClassName,
            string lpWindowName,
            WS dwStyle,
            int x,
            int y,
            int nWidth,
            int nHeight,
            IntPtr hWndParent,
            IntPtr hMenu,
            IntPtr hInstance,
            IntPtr lpParam)
        {
            IntPtr ret = _CreateWindowEx(dwExStyle, lpClassName, lpWindowName, dwStyle, x, y, nWidth, nHeight, hWndParent, hMenu, hInstance, lpParam);
            if (IntPtr.Zero == ret)
            {
                HRESULT.ThrowLastError();
            }

            return ret;
        }

        /// <SecurityNote>
        ///   Critical : P-Invokes
        /// </SecurityNote>


        [DllImport(ExternDll.User32, CharSet = CharSet.Unicode, EntryPoint = "DefWindowProcW")]
        public static extern IntPtr DefWindowProc(IntPtr hWnd, WM Msg, IntPtr wParam, IntPtr lParam);



        /// <SecurityNote>
        ///   Critical : P-Invokes
        /// </SecurityNote>
        [DllImport(ExternDll.User32, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool DestroyWindow(IntPtr hwnd);

        /// <SecurityNote>
        ///   Critical : P-Invokes
        /// </SecurityNote>


        [DllImport(ExternDll.User32)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool IsWindow(IntPtr hwnd);

        /// <SecurityNote>
        ///   Critical : P-Invokes
        /// </SecurityNote>


        [DllImport(ExternDll.DwmAPI, PreserveSig = false)]
        public static extern void DwmExtendFrameIntoClientArea(IntPtr hwnd, ref MARGINS pMarInset);

        /// <SecurityNote>
        ///   Critical : P-Invokes
        /// </SecurityNote>


        [DllImport(ExternDll.DwmAPI, EntryPoint = "DwmIsCompositionEnabled", PreserveSig = false)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool _DwmIsCompositionEnabled();

        /// <SecurityNote>
        ///   Critical : P-Invokes
        /// </SecurityNote>


        [DllImport(ExternDll.DwmAPI, EntryPoint = "DwmGetColorizationColor", PreserveSig = true)]
        private static extern HRESULT _DwmGetColorizationColor(out uint pcrColorization, [Out, MarshalAs(UnmanagedType.Bool)] out bool pfOpaqueBlend);

        /// <SecurityNote>
        ///   Critical : Calls critical method
        /// </SecurityNote>

        public static bool DwmGetColorizationColor(out uint pcrColorization, out bool pfOpaqueBlend)
        {
            // Make this call safe to make on downlevel OSes...
            if (Utility.IsOSVistaOrNewer && IsThemeActive())
            {
                HRESULT hr = _DwmGetColorizationColor(out pcrColorization, out pfOpaqueBlend);
                if (hr.Succeeded)
                {
                    return true;
                }
            }

            // Default values.  If for some reason the native DWM API fails it's never enough of a reason
            // to bring down the app.  Empirically it still sometimes returns errors even when the theme service is on.
            // We'll still use the boolean return value to allow the caller to respond if they care.
            pcrColorization = 0xFF000000;
            pfOpaqueBlend = true;

            return false;
        }

        /// <SecurityNote>
        ///   Critical : Calls critical method
        /// </SecurityNote>


        public static bool DwmIsCompositionEnabled()
        {
            // Make this call safe to make on downlevel OSes...
            if (!Utility.IsOSVistaOrNewer)
            {
                return false;
            }
            return _DwmIsCompositionEnabled();
        }

        /// <SecurityNote>
        ///   Critical : P-Invokes
        /// </SecurityNote>


        [DllImport(ExternDll.DwmAPI)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool DwmDefWindowProc(IntPtr hwnd, WM msg, IntPtr wParam, IntPtr lParam, out IntPtr plResult);

        /// <SecurityNote>
        ///   Critical : P-Invokes
        /// </SecurityNote>


        [DllImport(ExternDll.DwmAPI, EntryPoint = "DwmSetWindowAttribute")]
        private static extern void _DwmSetWindowAttribute(IntPtr hwnd, DWMWA dwAttribute, ref int pvAttribute, int cbAttribute);

        /// <SecurityNote>
        ///   Critical : Calls critical method
        /// </SecurityNote>


        public static void DwmSetWindowAttributeFlip3DPolicy(IntPtr hwnd, DWMFLIP3D flip3dPolicy)
        {
            Assert.IsTrue(Utility.IsOSVistaOrNewer);
            var dwPolicy = (int)flip3dPolicy;
            _DwmSetWindowAttribute(hwnd, DWMWA.FLIP3D_POLICY, ref dwPolicy, sizeof(int));
        }

        /// <SecurityNote>
        ///   Critical : Calls critical method
        /// </SecurityNote>


        public static void DwmSetWindowAttributeDisallowPeek(IntPtr hwnd, bool disallowPeek)
        {
            Assert.IsTrue(Utility.IsOSWindows7OrNewer);
            int dwDisallow = (int)(disallowPeek ? Win32Value.TRUE : Win32Value.FALSE);
            _DwmSetWindowAttribute(hwnd, DWMWA.DISALLOW_PEEK, ref dwDisallow, sizeof(int));
        }

        /// <SecurityNote>
        ///   Critical : P-Invokes
        /// </SecurityNote>


        [DllImport(ExternDll.User32, EntryPoint = "EnableMenuItem")]
        private static extern int _EnableMenuItem(IntPtr hMenu, SC uIDEnableItem, MF uEnable);

        /// <SecurityNote>
        ///   Critical : Calls critical method
        /// </SecurityNote>


        public static MF EnableMenuItem(IntPtr hMenu, SC uIDEnableItem, MF uEnable)
        {
            // Returns the previous state of the menu item, or -1 if the menu item does not exist.
            int iRet = _EnableMenuItem(hMenu, uIDEnableItem, uEnable);
            return (MF)iRet;
        }

        /// <SecurityNote>
        ///   Critical : Calls critical method
        /// </SecurityNote>


        [DllImport(ExternDll.User32, EntryPoint = "RemoveMenu", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool _RemoveMenu(IntPtr hMenu, uint uPosition, uint uFlags);

        /// <SecurityNote>
        ///   Critical : Calls critical method
        /// </SecurityNote>


        public static void RemoveMenu(IntPtr hMenu, SC uPosition, MF uFlags)
        {
            if (!_RemoveMenu(hMenu, (uint)uPosition, (uint)uFlags))
            {
                throw new Win32Exception();
            }
        }

        /// <SecurityNote>
        ///   Critical : P-Invokes
        /// </SecurityNote>


        [DllImport(ExternDll.User32, EntryPoint = "DrawMenuBar", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool _DrawMenuBar(IntPtr hWnd);

        /// <SecurityNote>
        ///   Critical : Calls critical method
        /// </SecurityNote>


        public static void DrawMenuBar(IntPtr hWnd)
        {
            if (!_DrawMenuBar(hWnd))
            {
                throw new Win32Exception();
            }
        }



        /// <SecurityNote>
        ///   Critical : P-Invokes
        /// </SecurityNote>

        // Not shimming this SetLastError=true function because callers want to evaluate the reason for failure.

        [DllImport(ExternDll.Kernel32, CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern SafeFindHandle FindFirstFileW(string lpFileName, [In, Out, MarshalAs(UnmanagedType.LPStruct)] WIN32_FIND_DATAW lpFindFileData);

        /// <SecurityNote>
        ///   Critical : P-Invokes
        /// </SecurityNote>

        // Not shimming this SetLastError=true function because callers want to evaluate the reason for failure.

        [DllImport(ExternDll.Kernel32, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool FindNextFileW(SafeFindHandle hndFindFile, [In, Out, MarshalAs(UnmanagedType.LPStruct)] WIN32_FIND_DATAW lpFindFileData);

        /// <SecurityNote>
        ///   Critical : P-Invokes
        /// </SecurityNote>


        [DllImport(ExternDll.User32, EntryPoint = "GetClientRect", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool _GetClientRect(IntPtr hwnd, [Out] out RECT lpRect);

        /// <SecurityNote>
        ///   Critical : Calls critical method
        /// </SecurityNote>


        public static RECT GetClientRect(IntPtr hwnd)
        {
            RECT rc;
            if (!_GetClientRect(hwnd, out rc))
            {
                HRESULT.ThrowLastError();
            }
            return rc;
        }

        /// <SecurityNote>
        ///   Critical : P-Invokes
        /// </SecurityNote>

        [DllImport("uxtheme.dll", EntryPoint = "GetCurrentThemeName", CharSet = CharSet.Unicode)]
        private static extern HRESULT _GetCurrentThemeName(
            StringBuilder pszThemeFileName,
            int dwMaxNameChars,
            StringBuilder pszColorBuff,
            int cchMaxColorChars,
            StringBuilder pszSizeBuff,
            int cchMaxSizeChars);

        /// <SecurityNote>
        ///   Critical : Calls critical method
        /// </SecurityNote>

        public static void GetCurrentThemeName(out string themeFileName, out string color, out string size)
        {
            // Not expecting strings longer than MAX_PATH.  We will return the error
            var fileNameBuilder = new StringBuilder((int)Win32Value.MAX_PATH);
            var colorBuilder = new StringBuilder((int)Win32Value.MAX_PATH);
            var sizeBuilder = new StringBuilder((int)Win32Value.MAX_PATH);

            // This will throw if the theme service is not active (e.g. not UxTheme!IsThemeActive).
            _GetCurrentThemeName(fileNameBuilder, fileNameBuilder.Capacity,
                                 colorBuilder, colorBuilder.Capacity,
                                 sizeBuilder, sizeBuilder.Capacity)
                .ThrowIfFailed();

            themeFileName = fileNameBuilder.ToString();
            color = colorBuilder.ToString();
            size = sizeBuilder.ToString();
        }

        /// <SecurityNote>
        ///   Critical : P-Invokes
        /// </SecurityNote>

        [DllImport("uxtheme.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool IsThemeActive();

        /// <SecurityNote>
        ///   Critical : P-Invokes
        /// </SecurityNote>


        [DllImport(ExternDll.Gdi32)]
        public static extern int GetDeviceCaps(SafeDC hdc, DeviceCap nIndex);

        /// <SecurityNote>
        ///   Critical : P-Invokes
        /// </SecurityNote>


        [DllImport(ExternDll.Kernel32, EntryPoint = "GetModuleFileName", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern int _GetModuleFileName(IntPtr hModule, StringBuilder lpFilename, int nSize);

        /// <SecurityNote>
        ///   Critical : Calls critical method
        /// </SecurityNote>


        public static string GetModuleFileName(IntPtr hModule)
        {
            var buffer = new StringBuilder((int)Win32Value.MAX_PATH);
            while (true)
            {
                int size = _GetModuleFileName(hModule, buffer, buffer.Capacity);
                if (size == 0)
                {
                    HRESULT.ThrowLastError();
                }

                // GetModuleFileName returns nSize when it's truncated but does NOT set the last error.
                // MSDN documentation says this has changed in Windows 2000+.
                if (size == buffer.Capacity)
                {
                    // Enlarge the buffer and try again.
                    buffer.EnsureCapacity(buffer.Capacity * 2);
                    continue;
                }

                return buffer.ToString();
            }
        }

        /// <SecurityNote>
        ///   Critical : P-Invokes
        /// </SecurityNote>

        [DllImport(ExternDll.Kernel32, EntryPoint = "GetModuleHandleW", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern IntPtr _GetModuleHandleW([MarshalAs(UnmanagedType.LPWStr)] string lpModuleName);

        /// <SecurityNote>
        ///   Critical : Calls critical method
        /// </SecurityNote>

        public static IntPtr GetModuleHandleW(string lpModuleName)
        {
            IntPtr retPtr = _GetModuleHandleW(lpModuleName);
            if (retPtr == IntPtr.Zero)
            {
                HRESULT.ThrowLastError();
            }
            return retPtr;
        }

        /// <SecurityNote>
        ///   Critical : P-Invokes
        /// </SecurityNote>


        [DllImport(ExternDll.User32, EntryPoint = "GetMonitorInfo", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool _GetMonitorInfo(IntPtr hMonitor, [In, Out] MONITORINFO lpmi);

        /// <SecurityNote>
        ///   Critical : Calls critical method
        /// </SecurityNote>


        public static MONITORINFO GetMonitorInfo(IntPtr hMonitor)
        {
            var mi = new MONITORINFO();
            if (!_GetMonitorInfo(hMonitor, mi))
            {
                throw new Win32Exception();
            }
            return mi;
        }

        /// <SecurityNote>
        ///   Critical : P-Invokes
        /// </SecurityNote>

        [DllImport(ExternDll.Gdi32, EntryPoint = "GetStockObject", SetLastError = true)]
        private static extern IntPtr _GetStockObject(StockObject fnObject);

        /// <SecurityNote>
        ///   Critical : Calls critical method
        /// </SecurityNote>

        public static IntPtr GetStockObject(StockObject fnObject)
        {
            IntPtr retPtr = _GetStockObject(fnObject);
            if (retPtr == null)
            {
                HRESULT.ThrowLastError();
            }
            return retPtr;
        }

        /// <SecurityNote>
        ///   Critical : P-Invokes
        /// </SecurityNote>


        [DllImport(ExternDll.User32)]
        public static extern IntPtr GetSystemMenu(IntPtr hWnd, [MarshalAs(UnmanagedType.Bool)] bool bRevert);

     

        /// <summary>
        /// Sets attributes to control how visual styles are applied to a specified window.
        /// </summary>
        /// <param name="hwnd">
        /// Handle to a window to apply changes to.
        /// </param>
        /// <param name="eAttribute">
        /// Value of type WINDOWTHEMEATTRIBUTETYPE that specifies the type of attribute to set.
        /// The value of this parameter determines the type of data that should be passed in the pvAttribute parameter.
        /// Can be the following value:
        /// <list>WTA_NONCLIENT (Specifies non-client related attributes).</list>
        /// pvAttribute must be a pointer of type WTA_OPTIONS.
        /// </param>
        /// <param name="pvAttribute">
        /// A pointer that specifies attributes to set. Type is determined by the value of the eAttribute value.
        /// </param>
        /// <param name="cbAttribute">
        /// Specifies the size, in bytes, of the data pointed to by pvAttribute.
        /// </param>
        /// <SecurityNote>
        ///   Critical : P-Invokes
        /// </SecurityNote>


        [DllImport(ExternDll.Uxtheme, PreserveSig = false)]
        public static extern void SetWindowThemeAttribute([In] IntPtr hwnd, [In] WINDOWTHEMEATTRIBUTETYPE eAttribute, [In] ref WTA_OPTIONS pvAttribute, [In] uint cbAttribute);



        /// <SecurityNote>
        ///   Critical : P-Invokes
        /// </SecurityNote>
        [DllImport(ExternDll.Gdiplus)]
        public static extern Status GdipCreateBitmapFromStream(IStream stream, out IntPtr bitmap);

        /// <SecurityNote>
        ///   Critical : P-Invokes
        /// </SecurityNote>


        [DllImport(ExternDll.Gdiplus)]
        public static extern Status GdipCreateHBITMAPFromBitmap(IntPtr bitmap, out IntPtr hbmReturn, Int32 background);

        /// <SecurityNote>
        ///   Critical : P-Invokes
        /// </SecurityNote>


        [DllImport(ExternDll.Gdiplus)]
        public static extern Status GdipCreateHICONFromBitmap(IntPtr bitmap, out IntPtr hbmReturn);

        /// <SecurityNote>
        ///   Critical : P-Invokes
        /// </SecurityNote>


        [DllImport(ExternDll.Gdiplus)]
        public static extern Status GdipDisposeImage(IntPtr image);

        /// <SecurityNote>
        ///   Critical : P-Invokes
        /// </SecurityNote>


        [DllImport(ExternDll.Gdiplus)]
        public static extern Status GdipImageForceValidation(IntPtr image);

        /// <SecurityNote>
        ///   Critical : P-Invokes
        /// </SecurityNote>


        [DllImport(ExternDll.Gdiplus)]
        public static extern Status GdiplusStartup(out IntPtr token, StartupInput input, out StartupOutput output);

        /// <SecurityNote>
        ///   Critical : P-Invokes
        /// </SecurityNote>


        [DllImport(ExternDll.Gdiplus)]
        public static extern Status GdiplusShutdown(IntPtr token);

        /// <SecurityNote>
        ///   Critical : P-Invokes
        /// </SecurityNote>


        [DllImport(ExternDll.User32)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool IsWindowVisible(IntPtr hwnd);

        /// <SecurityNote>
        ///   Critical : P-Invokes
        /// </SecurityNote>


        [DllImport(ExternDll.Kernel32, EntryPoint = "LocalFree", SetLastError = true)]
        private static extern IntPtr _LocalFree(IntPtr hMem);

        /// <SecurityNote>
        ///   Critical : P-Invokes
        /// </SecurityNote>


        [DllImport(ExternDll.User32)]
        public static extern IntPtr MonitorFromWindow(IntPtr hwnd, uint dwFlags);

        /// <SecurityNote>
        ///   Critical : P-Invokes
        /// </SecurityNote>


        [DllImport(ExternDll.User32, EntryPoint = "PostMessage", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool _PostMessage(IntPtr hWnd, WM Msg, IntPtr wParam, IntPtr lParam);

        /// <SecurityNote>
        ///   Critical : Calls critical method
        /// </SecurityNote>


        public static void PostMessage(IntPtr hWnd, WM Msg, IntPtr wParam, IntPtr lParam)
        {
            if (!_PostMessage(hWnd, Msg, wParam, lParam))
            {
                throw new Win32Exception();
            }
        }

        /// <SecurityNote>
        ///   Critical : P-Invokes
        /// </SecurityNote>


        [DllImport(ExternDll.User32, SetLastError = true, EntryPoint = "RegisterClassExW")]
        private static extern short _RegisterClassEx([In] ref WNDCLASSEX lpwcx);

        // Note that this will throw HRESULT_FROM_WIN32(ERROR_CLASS_ALREADY_EXISTS) on duplicate registration.
        // If needed, consider adding a Try* version of this function that returns the error code since that
        // may be ignorable.
        /// <SecurityNote>
        ///   Critical : Calls critical method
        /// </SecurityNote>


        public static short RegisterClassEx(ref WNDCLASSEX lpwcx)
        {
            short ret = _RegisterClassEx(ref lpwcx);
            if (ret == 0)
            {
                HRESULT.ThrowLastError();
            }

            return ret;
        }



        /// <SecurityNote>
        ///   Critical : P-Invokes
        /// </SecurityNote>
        [DllImport(ExternDll.User32, EntryPoint = "SetActiveWindow", SetLastError = true)]
        private static extern IntPtr _SetActiveWindow(IntPtr hWnd);

        /// <SecurityNote>
        ///   Critical : Calls critical method
        /// </SecurityNote>


        public static IntPtr SetActiveWindow(IntPtr hwnd)
        {
            Verify.IsNotDefault(hwnd, "hwnd");
            IntPtr ret = _SetActiveWindow(hwnd);
            if (ret == IntPtr.Zero)
            {
                HRESULT.ThrowLastError();
            }

            return ret;
        }

        // This is aliased as a macro in 32bit Windows.
        /// <SecurityNote>
        ///   Critical : P-Invokes
        /// </SecurityNote>


        public static IntPtr SetClassLongPtr(IntPtr hwnd, GCLP nIndex, IntPtr dwNewLong)
        {
            if (8 == IntPtr.Size)
            {
                return SetClassLongPtr64(hwnd, nIndex, dwNewLong);
            }
            return new IntPtr(SetClassLongPtr32(hwnd, nIndex, dwNewLong.ToInt32()));
        }

        /// <SecurityNote>
        ///   Critical : P-Invokes
        /// </SecurityNote>

        [SuppressMessage("Microsoft.Interoperability", "CA1400:PInvokeEntryPointsShouldExist")]

        [DllImport(ExternDll.User32, EntryPoint = "SetClassLong", SetLastError = true)]
        private static extern int SetClassLongPtr32(IntPtr hWnd, GCLP nIndex, int dwNewLong);

        /// <SecurityNote>
        ///   Critical : P-Invokes
        /// </SecurityNote>

        [SuppressMessage("Microsoft.Interoperability", "CA1400:PInvokeEntryPointsShouldExist")]

        [DllImport(ExternDll.User32, EntryPoint = "SetClassLongPtr", SetLastError = true)]
        private static extern IntPtr SetClassLongPtr64(IntPtr hWnd, GCLP nIndex, IntPtr dwNewLong);

        /// <SecurityNote>
        ///   Critical : P-Invokes
        /// </SecurityNote>


        [DllImport(ExternDll.Kernel32, SetLastError = true)]
        public static extern ErrorModes SetErrorMode(ErrorModes newMode);

        /// <SecurityNote>
        ///   Critical : P-Invokes
        /// </SecurityNote>


        [DllImport(ExternDll.Kernel32, SetLastError = true, EntryPoint = "SetProcessWorkingSetSize")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool _SetProcessWorkingSetSize(IntPtr hProcess, IntPtr dwMinimiumWorkingSetSize, IntPtr dwMaximumWorkingSetSize);

        /// <SecurityNote>
        ///   Critical : Calls critical method
        /// </SecurityNote>


        public static void SetProcessWorkingSetSize(IntPtr hProcess, int dwMinimumWorkingSetSize, int dwMaximumWorkingSetSize)
        {
            if (!_SetProcessWorkingSetSize(hProcess, new IntPtr(dwMinimumWorkingSetSize), new IntPtr(dwMaximumWorkingSetSize)))
            {
                throw new Win32Exception();
            }
        }

        // This is aliased as a macro in 32bit Windows.
        /// <SecurityNote>
        ///   Critical : Calls critical method
        /// </SecurityNote>


   

        /// <SecurityNote>
        ///   Critical : P-Invokes
        /// </SecurityNote>


        [DllImport(ExternDll.User32, EntryPoint = "SetWindowRgn", SetLastError = true)]
        private static extern int _SetWindowRgn(IntPtr hWnd, IntPtr hRgn, [MarshalAs(UnmanagedType.Bool)] bool bRedraw);

        /// <SecurityNote>
        ///   Critical : Calls critical method
        /// </SecurityNote>


        public static void SetWindowRgn(IntPtr hWnd, IntPtr hRgn, bool bRedraw)
        {
            int err = _SetWindowRgn(hWnd, hRgn, bRedraw);
            if (0 == err)
            {
                throw new Win32Exception();
            }
        }

        /// <SecurityNote>
        ///   Critical : P-Invokes
        /// </SecurityNote>


        [DllImport(ExternDll.User32, EntryPoint = "SetWindowPos", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool _SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int x, int y, int cx, int cy, SWP uFlags);

        /// <SecurityNote>
        ///   Critical : P-Invokes
        /// </SecurityNote>


        public static bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int x, int y, int cx, int cy, SWP uFlags)
        {
            if (!_SetWindowPos(hWnd, hWndInsertAfter, x, y, cx, cy, uFlags))
            {
                // If this fails it's never worth taking down the process.  Let the caller deal with the error if they want.
                return false;
            }

            return true;
        }

        /// <SecurityNote>
        ///   Critical : P-Invokes
        /// </SecurityNote>


        [DllImport(ExternDll.Shell32, SetLastError = false)]
        public static extern Win32Error SHFileOperation(ref SHFILEOPSTRUCT lpFileOp);

        /// <SecurityNote>
        ///   Critical : P-Invokes
        /// </SecurityNote>

        [DllImport(ExternDll.User32)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool ShowWindow(IntPtr hwnd, SW nCmdShow);

        /// <SecurityNote>
        ///   Critical : P-Invokes
        /// </SecurityNote>


        [DllImport(ExternDll.User32, EntryPoint = "SystemParametersInfoW", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool _SystemParametersInfo_String(SPI uiAction, int uiParam, [MarshalAs(UnmanagedType.LPWStr)] string pvParam, SPIF fWinIni);

        /// <summary>Overload of SystemParametersInfo for getting and setting NONCLIENTMETRICS.</summary>
        /// <SecurityNote>
        ///   Critical : P-Invokes
        /// </SecurityNote>
        [DllImport(ExternDll.User32, EntryPoint = "SystemParametersInfoW", SetLastError = true, CharSet = CharSet.Unicode)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool _SystemParametersInfo_NONCLIENTMETRICS(SPI uiAction, int uiParam, [In, Out] ref NONCLIENTMETRICS pvParam, SPIF fWinIni);

        /// <summary>Overload of SystemParametersInfo for getting and setting HIGHCONTRAST.</summary>
        /// <SecurityNote>
        ///   Critical : P-Invokes
        /// </SecurityNote>
        [DllImport(ExternDll.User32, EntryPoint = "SystemParametersInfoW", SetLastError = true, CharSet = CharSet.Unicode)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool _SystemParametersInfo_HIGHCONTRAST(SPI uiAction, int uiParam, [In, Out] ref HIGHCONTRAST pvParam, SPIF fWinIni);

        /// <SecurityNote>
        ///   Critical : Calls critical method
        /// </SecurityNote>
        public static void SystemParametersInfo(SPI uiAction, int uiParam, string pvParam, SPIF fWinIni)
        {
            if (!_SystemParametersInfo_String(uiAction, uiParam, pvParam, fWinIni))
            {
                HRESULT.ThrowLastError();
            }
        }

        /// <SecurityNote>
        ///   Critical : P-Invokes
        /// </SecurityNote>
        public static NONCLIENTMETRICS SystemParameterInfo_GetNONCLIENTMETRICS()
        {
            var metrics = Utility.IsOSVistaOrNewer
                ? NONCLIENTMETRICS.VistaMetricsStruct
                : NONCLIENTMETRICS.XPMetricsStruct;

            if (!_SystemParametersInfo_NONCLIENTMETRICS(SPI.GETNONCLIENTMETRICS, metrics.cbSize, ref metrics, SPIF.None))
            {
                HRESULT.ThrowLastError();
            }

            return metrics;
        }

        /// <SecurityNote>
        ///   Critical : P-Invokes
        /// </SecurityNote>
        public static HIGHCONTRAST SystemParameterInfo_GetHIGHCONTRAST()
        {
            var hc = new HIGHCONTRAST { cbSize = Marshal.SizeOf(typeof(HIGHCONTRAST)) };

            if (!_SystemParametersInfo_HIGHCONTRAST(SPI.GETHIGHCONTRAST, hc.cbSize, ref hc, SPIF.None))
            {
                HRESULT.ThrowLastError();
            }

            return hc;
        }

        // This function is strange in that it returns a BOOL if TPM_RETURNCMD isn't specified, but otherwise the command Id.
        // Currently it's only used with TPM_RETURNCMD, so making the signature match that.
        /// <SecurityNote>
        ///   Critical : P-Invokes
        /// </SecurityNote>


        [DllImport(ExternDll.User32)]
        public static extern uint TrackPopupMenuEx(IntPtr hmenu, uint fuFlags, int x, int y, IntPtr hwnd, IntPtr lptpm);

        /// <SecurityNote>
        ///   Critical : P-Invokes
        /// </SecurityNote>


        [DllImport(ExternDll.Gdi32, EntryPoint = "SelectObject", SetLastError = true)]
        private static extern IntPtr _SelectObject(SafeDC hdc, IntPtr hgdiobj);

        /// <SecurityNote>
        ///   Critical : Calls critical method
        /// </SecurityNote>


        public static IntPtr SelectObject(SafeDC hdc, IntPtr hgdiobj)
        {
            IntPtr ret = _SelectObject(hdc, hgdiobj);
            if (ret == IntPtr.Zero)
            {
                HRESULT.ThrowLastError();
            }
            return ret;
        }

        /// <SecurityNote>
        ///   Critical : P-Invokes
        /// </SecurityNote>


        [DllImport(ExternDll.Gdi32, EntryPoint = "SelectObject", SetLastError = true)]
        private static extern IntPtr _SelectObjectSafeHBITMAP(SafeDC hdc, SafeHBITMAP hgdiobj);

        /// <SecurityNote>
        ///   Critical : Calls critical method
        /// </SecurityNote>


        public static IntPtr SelectObject(SafeDC hdc, SafeHBITMAP hgdiobj)
        {
            IntPtr ret = _SelectObjectSafeHBITMAP(hdc, hgdiobj);
            if (ret == IntPtr.Zero)
            {
                HRESULT.ThrowLastError();
            }
            return ret;
        }

        /// <SecurityNote>
        ///   Critical : P-Invokes
        /// </SecurityNote>


        [DllImport(ExternDll.User32, SetLastError = true)]
        public static extern int SendInput(int nInputs, ref INPUT pInputs, int cbSize);

        // Depending on the message, callers may want to call GetLastError based on the return value.
        /// <SecurityNote>
        ///   Critical : P-Invokes
        /// </SecurityNote>

        [DllImport(ExternDll.User32, SetLastError = true)]
        public static extern IntPtr SendMessage(IntPtr hWnd, WM Msg, IntPtr wParam, IntPtr lParam);

        /// <SecurityNote>
        ///   Critical : P-Invokes
        /// </SecurityNote>


        [DllImport(ExternDll.User32, EntryPoint = "UnregisterClass", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool _UnregisterClassAtom(IntPtr lpClassName, IntPtr hInstance);

        /// <SecurityNote>
        ///   Critical : P-Invokes
        /// </SecurityNote>


        [DllImport(ExternDll.User32, EntryPoint = "UnregisterClass", CharSet = CharSet.Unicode, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool _UnregisterClassName(string lpClassName, IntPtr hInstance);

        /// <SecurityNote>
        ///   Critical : Calls critical method
        /// </SecurityNote>


        public static void UnregisterClass(short atom, IntPtr hinstance)
        {
            if (!_UnregisterClassAtom(new IntPtr(atom), hinstance))
            {
                HRESULT.ThrowLastError();
            }
        }

        /// <SecurityNote>
        ///   Critical : Calls critical method
        /// </SecurityNote>

        public static void UnregisterClass(string lpClassName, IntPtr hInstance)
        {
            if (!_UnregisterClassName(lpClassName, hInstance))
            {
                HRESULT.ThrowLastError();
            }
        }

        /// <SecurityNote>
        ///   Critical : P-Invokes
        /// </SecurityNote>


        [DllImport(ExternDll.User32, SetLastError = true, EntryPoint = "UpdateLayeredWindow")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool _UpdateLayeredWindow(
            IntPtr hwnd,
            SafeDC hdcDst,
            [In] ref POINT pptDst,
            [In] ref SIZE psize,
            SafeDC hdcSrc,
            [In] ref POINT pptSrc,
            int crKey,
            ref BLENDFUNCTION pblend,
            ULW dwFlags);

        /// <SecurityNote>
        ///   Critical : P-Invokes
        /// </SecurityNote>


        [DllImport(ExternDll.User32, SetLastError = true, EntryPoint = "UpdateLayeredWindow")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool _UpdateLayeredWindowIntPtr(
            IntPtr hwnd,
            IntPtr hdcDst,
            IntPtr pptDst,
            IntPtr psize,
            IntPtr hdcSrc,
            IntPtr pptSrc,
            int crKey,
            ref BLENDFUNCTION pblend,
            ULW dwFlags);

        /// <SecurityNote>
        ///   Critical : Calls critical method
        /// </SecurityNote>


        public static void UpdateLayeredWindow(
            IntPtr hwnd,
            SafeDC hdcDst,
            ref POINT pptDst,
            ref SIZE psize,
            SafeDC hdcSrc,
            ref POINT pptSrc,
            int crKey,
            ref BLENDFUNCTION pblend,
            ULW dwFlags)
        {
            if (!_UpdateLayeredWindow(hwnd, hdcDst, ref pptDst, ref psize, hdcSrc, ref pptSrc, crKey, ref pblend, dwFlags))
            {
                HRESULT.ThrowLastError();
            }
        }

        /// <SecurityNote>
        ///   Critical : Calls critical method
        /// </SecurityNote>


        public static void UpdateLayeredWindow(
            IntPtr hwnd,
            int crKey,
            ref BLENDFUNCTION pblend,
            ULW dwFlags)
        {
            if (!_UpdateLayeredWindowIntPtr(hwnd, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, crKey, ref pblend, dwFlags))
            {
                HRESULT.ThrowLastError();
            }
        }

        #region Win7 declarations

        /// <SecurityNote>
        ///   Critical : P-Invokes
        /// </SecurityNote>

        [DllImport(ExternDll.Shell32, EntryPoint = "SHAddToRecentDocs")]
        private static extern void _SHAddToRecentDocs_String(SHARD uFlags, [MarshalAs(UnmanagedType.LPWStr)] string pv);

        // This overload is required.  There's a cast in the Shell code that causes the wrong vtbl to be used
        // if we let the marshaller convert the parameter to an IUnknown.
        /// <SecurityNote>
        ///   Critical : P-Invokes
        /// </SecurityNote>

        [DllImport(ExternDll.Shell32, EntryPoint = "SHAddToRecentDocs")]
        private static extern void _SHAddToRecentDocs_ShellLink(SHARD uFlags, IShellLinkW pv);

        /// <SecurityNote>
        ///   Critical : Calls critical method
        /// </SecurityNote>

        public static void SHAddToRecentDocs(string path)
        {
            _SHAddToRecentDocs_String(SHARD.PATHW, path);
        }

        // Win7 only.
        /// <SecurityNote>
        ///   Critical : Calls critical method
        /// </SecurityNote>

        public static void SHAddToRecentDocs(IShellLinkW shellLink)
        {
            _SHAddToRecentDocs_ShellLink(SHARD.LINK, shellLink);
        }


        //#define DWM_SIT_DISPLAYFRAME    0x00000001  // Display a window frame around the provided bitmap

        /// <SecurityNote>
        ///   Critical : P-Invokes
        /// </SecurityNote>

        [DllImport(ExternDll.DwmAPI, EntryPoint = "DwmGetCompositionTimingInfo")]
        private static extern HRESULT _DwmGetCompositionTimingInfo(IntPtr hwnd, ref DWM_TIMING_INFO pTimingInfo);

        /// <SecurityNote>
        ///   Critical : Calls critical method
        /// </SecurityNote>

        public static DWM_TIMING_INFO? DwmGetCompositionTimingInfo(IntPtr hwnd)
        {
            if (!Utility.IsOSVistaOrNewer)
            {
                // API was new to Vista.
                return null;
            }

            var dti = new DWM_TIMING_INFO { cbSize = Marshal.SizeOf(typeof(DWM_TIMING_INFO)) };
            HRESULT hr = _DwmGetCompositionTimingInfo(hwnd, ref dti);
            if (hr == HRESULT.E_PENDING)
            {
                // The system isn't yet ready to respond.  Return null rather than throw.
                return null;
            }
            hr.ThrowIfFailed();

            return dti;
        }

        /// <SecurityNote>
        ///   Critical : P-Invokes
        /// </SecurityNote>


        [DllImport(ExternDll.DwmAPI, PreserveSig = false)]
        public static extern void DwmInvalidateIconicBitmaps(IntPtr hwnd);

        /// <SecurityNote>
        ///   Critical : P-Invokes
        /// </SecurityNote>


        [DllImport(ExternDll.DwmAPI, PreserveSig = false)]
        public static extern void DwmSetIconicThumbnail(IntPtr hwnd, IntPtr hbmp, DWM_SIT dwSITFlags);

        /// <SecurityNote>
        ///   Critical : P-Invokes
        /// </SecurityNote>


        [DllImport(ExternDll.DwmAPI, PreserveSig = false)]
        public static extern void DwmSetIconicLivePreviewBitmap(IntPtr hwnd, IntPtr hbmp, RefPOINT pptClient, DWM_SIT dwSITFlags);

        /// <SecurityNote>
        ///   Critical : P-Invokes
        /// </SecurityNote>


        [DllImport(ExternDll.Shell32, PreserveSig = false)]
        public static extern void SHGetItemFromDataObject(IDataObject pdtobj, DOGIF dwFlags, [In] ref Guid riid, [Out, MarshalAs(UnmanagedType.Interface)] out object ppv);

        /// <SecurityNote>
        ///   Critical : P-Invokes
        /// </SecurityNote>


        [DllImport(ExternDll.Shell32, PreserveSig = false)]
        public static extern HRESULT SHCreateItemFromParsingName([MarshalAs(UnmanagedType.LPWStr)] string pszPath, IBindCtx pbc, [In] ref Guid riid, [Out, MarshalAs(UnmanagedType.Interface)] out object ppv);

        /// <SecurityNote>
        ///   Critical : P-Invokes
        /// </SecurityNote>


        [DllImport(ExternDll.Shell32)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool Shell_NotifyIcon(NIM dwMessage, [In] NOTIFYICONDATA lpdata);

        /// <summary>
        /// Sets the User Model AppID for the current process, enabling Windows to retrieve this ID
        /// </summary>
        /// <param name="AppID"></param>
        /// <SecurityNote>
        ///   Critical : P-Invokes
        /// </SecurityNote>


        [DllImport(ExternDll.Shell32, PreserveSig = false)]
        public static extern void SetCurrentProcessExplicitAppUserModelID([MarshalAs(UnmanagedType.LPWStr)] string AppID);

        /// <summary>
        /// Retrieves the User Model AppID that has been explicitly set for the current process via SetCurrentProcessExplicitAppUserModelID
        /// </summary>
        /// <param name="AppID"></param>
        /// <SecurityNote>
        ///   Critical : P-Invokes
        /// </SecurityNote>


        [DllImport(ExternDll.Shell32)]
        public static extern HRESULT GetCurrentProcessExplicitAppUserModelID([Out, MarshalAs(UnmanagedType.LPWStr)] out string AppID);
        #endregion


        [DllImport(ExternDll.DwmAPI)]
        public static extern int DwmSetWindowAttribute(IntPtr hwnd, uint attr, ref int attrValue, int attrSize);


        /// <SecurityNote>
        ///     Critical: Because of suppression
        /// </SecurityNote>
        [DllImport(ExternDll.Comdlg32, CharSet = CharSet.Auto)]
        public static extern Int32 PrintDlgEx(IntPtr pdex);

        /// <SecurityNote>
        ///     Critical: Because of suppression
        /// </SecurityNote>
        [DllImport(ExternDll.Kernel32)]
        public static extern Int32 GlobalFree(IntPtr hMem);

        /// <SecurityNote>
        ///     Critical: Because of suppression
        /// </SecurityNote>
        [DllImport(ExternDll.Kernel32)]
        public static extern IntPtr GlobalLock(IntPtr hMem);

        /// <SecurityNote>
        ///     Critical: Because of suppression
        /// </SecurityNote>
        [DllImport(ExternDll.Kernel32)]
        public static extern bool GlobalUnlock(IntPtr hMem);

        ///<SecurityNote>
        ///     Critical - elevates via a SUC.
        ///</SecurityNote>
        [DllImport(ExternDll.User32, SetLastError = true, CharSet = System.Runtime.InteropServices.CharSet.Auto, BestFitMapping = false)]
        public static extern int MessageBox(HandleRef hWnd, string text, string caption, int type);

    }
}
