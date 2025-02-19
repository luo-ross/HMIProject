using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace RS.Commons.Helpers
{
    public class PathHelper
    {
        public static string MapPath()
        {
            return AppDomain.CurrentDomain.BaseDirectory;
        }
        public static string MapPath(string path)
        {
            return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, path);
        }
    }
}
