using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RS.Widgets.Commons
{

    public class ShortCutUtil
    {

        public static List<string> ShortCutList;
        static ShortCutUtil()
        {
            ShortCutList = new List<string>();
            ShortCutList.Add("1");
            ShortCutList.Add("2");
            ShortCutList.Add("3");
            ShortCutList.Add("4");
            ShortCutList.Add("5");
            ShortCutList.Add("6");
            ShortCutList.Add("7");
            ShortCutList.Add("8");
            ShortCutList.Add("9");
            ShortCutList.Add("a");
            ShortCutList.Add("b");
            ShortCutList.Add("c");
            ShortCutList.Add("d");
            ShortCutList.Add("e");
            ShortCutList.Add("f");
            ShortCutList.Add("g");
            ShortCutList.Add("h");
            ShortCutList.Add("i");
            ShortCutList.Add("j");
            ShortCutList.Add("k");
            ShortCutList.Add("l");
            ShortCutList.Add("m");
            ShortCutList.Add("n");
            ShortCutList.Add("o");
            ShortCutList.Add("p");
            ShortCutList.Add("q");
            ShortCutList.Add("r");
            ShortCutList.Add("s");
            ShortCutList.Add("t");
            ShortCutList.Add("u");
            ShortCutList.Add("v");
            ShortCutList.Add("w");
            ShortCutList.Add("x");
            ShortCutList.Add("y");
            ShortCutList.Add("z");
            ShortCutList.Add("A");
            ShortCutList.Add("B");
            ShortCutList.Add("C");
            ShortCutList.Add("D");
            ShortCutList.Add("E");
            ShortCutList.Add("F");
            ShortCutList.Add("G");
            ShortCutList.Add("H");
            ShortCutList.Add("I");
            ShortCutList.Add("J");
            ShortCutList.Add("K");
            ShortCutList.Add("L");
            ShortCutList.Add("M");
            ShortCutList.Add("N");
            ShortCutList.Add("O");
            ShortCutList.Add("P");
            ShortCutList.Add("Q");
            ShortCutList.Add("R");
            ShortCutList.Add("S");
            ShortCutList.Add("T");
            ShortCutList.Add("U");
            ShortCutList.Add("V");
            ShortCutList.Add("W");
            ShortCutList.Add("X");
            ShortCutList.Add("Y");
            ShortCutList.Add("Z");
        }
        public static string GetShortCutKey(int index)
        {
            if (index >= 0 && index < ShortCutList.Count)
            {
                return ShortCutList[index];
            }
            else
            {
                return null;
            }
        }

        public static string GetShortCutKey(List<string> keyUsed)
        {
            return ShortCutList.Except(keyUsed).FirstOrDefault();
        }
    }
}
