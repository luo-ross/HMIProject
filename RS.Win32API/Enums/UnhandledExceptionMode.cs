using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RS.Win32API.Enums
{
    /// <devdoc>
    ///     Determines the exception mode of NativeWindow's WndProc method.  Pass
    ///     a value of this enum into SetUnhandledExceptionMode to control how
    ///     new NativeWindow objects handle exceptions.  
    /// </devdoc>
    public enum UnhandledExceptionMode
    {
        Automatic,
        ThrowException,
        CatchException
    }
}
