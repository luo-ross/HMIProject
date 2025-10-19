using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace RS.Widgets.Commons
{
    public class ValidHelper
    {
        /// <summary>
        /// 是否是电话号码
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static bool IsPhone(string input)
        {
            if (input.Length < 11)
            {
                return false;
            }
            //电信手机号码
            string dianxin = @"^1(33|49|53|73|74|77|80|81|89|99)\d{8}$";
            Regex regexDX = new Regex(dianxin);
            //联通手机号码
            string liantong = @"^1(30|31|32|45|46|55|56|66|71|75|76|85|86)\d{8}";
            Regex regexLT = new Regex(liantong);
            //移动手机号码
            string yidong = @"^1(34|35|36|37|38|39|47|48|50|51|52|57|58|59|72|78|82|83|84|87|88|98)\d{8}";
            Regex regexYD = new Regex(yidong);
            if (regexDX.IsMatch(input) || regexLT.IsMatch(input) || regexYD.IsMatch(input))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
