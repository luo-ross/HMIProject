using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RS.Commons.Exceptions
{
    public class ErrorException : Exception
    {
        public ErrorException() : base()
        {

        }
        public ErrorException(string message) : base(message)
        {

        }

        public ErrorException(string message, Exception ex) : base(message, ex)
        {

        }
    }
}
