using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RS.Commons.Exceptions
{
    public class WarningException : Exception
    {
        public WarningException() : base()
        {

        }
        public WarningException(string message) : base(message)
        {

        }

        public WarningException(string message, Exception ex) : base(message, ex)
        {

        }
    }
}
