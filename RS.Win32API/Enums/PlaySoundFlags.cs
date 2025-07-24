using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RS.Win32API.Enums
{
    [Flags]
    public enum PlaySoundFlags
    {
        SND_SYNC = 0x00000000, /* play synchronously (default) */
        SND_ASYNC = 0x00000001, /* play asynchronously */
        SND_NODEFAULT = 0x00000002, /* silence (!default) if sound not found */
        SND_MEMORY = 0x00000004, /* pszSound points to a memory file */
        SND_LOOP = 0x00000008, /* loop the sound until next sndPlaySound */
        SND_NOSTOP = 0x00000010, /* don't stop any currently playing sound */
        SND_PURGE = 0x00000040, /* purge non-static events for task */
        SND_APPLICATION = 0x00000080, /* look for application specific association */
        SND_NOWAIT = 0x00002000, /* don't wait if the driver is busy */
        SND_ALIAS = 0x00010000, /* name is a registry alias */
        SND_FILENAME = 0x00020000, /* name is file name */
        SND_RESOURCE = 0x00040000, /* name is resource name or atom */
    }
}
