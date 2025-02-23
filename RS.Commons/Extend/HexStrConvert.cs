using System;
using System.IO;
using System.Text;

namespace RS.Commons.Extend
{
    public class HexStrConvert
    {
        public static byte StrHexToByte(string str)
        {
            byte b = 0;
            try
            {
                b = Convert.ToByte(str, 16);
            }
            catch (Exception)
            {
            }
            return b;
        }

        public static byte[] StrASICToByte(string s)
        {
            Encoding GB2312 = Encoding.GetEncoding("GB2312");
            byte[] array = GB2312.GetBytes(s);
            return array;
        }

        public static byte[] StrHexArrToByte(string s)
        {
            s = s.Replace(" ", "");
            byte[] array = new byte[s.Length / 2];
            int num = 0;
            for (int i = 0; i < s.Length; i += 2)
            {
                try
                {
                    array[num++] = Convert.ToByte(s.Substring(i, 2), 16);
                }
                catch (Exception)
                {
                }
            }
            return array;
        }

        public static byte[] StrToBytes(string s)
        {
            StringBuilder builder = new StringBuilder();
            byte[] bytes = new byte[s.Length];
            for (int i = 0; i < s.Length; i++)
            {
                bytes[i] = (byte)s[i];
            }
            return bytes;
        }

        public static string StrToHex(string s)
        {
            StringBuilder builder = new StringBuilder();
            byte[] bytes = new byte[s.Length];
            for (int i = 0; i < s.Length; i++)
            {
                bytes[i] = (byte)s[i];
                builder.Append(Convert.ToString(bytes[i], 16).ToUpper().PadLeft(2, '0') + " ");
            }
            return builder.ToString().Trim();
        }

        public static string BytesToString(byte[] bytes)
        {
            string str = "";
            for (int i = 0; i < bytes.Length; i++)
            {
                str += ((char)bytes[i]).ToString();
            }
            return str;
        }

        public static string HexToStr(string s)
        {
            string str = string.Empty;
            s = s.Replace(" ", "");
            byte[] array = new byte[s.Length / 2];
            int num = 0;
            for (int i = 0; i < s.Length; i += 2)
            {
                try
                {
                    array[num++] = Convert.ToByte(s.Substring(i, 2), 16);
                }
                catch (Exception)
                {
                }
            }
            str = Encoding.Default.GetString(array);
            return str.Replace("\0", "");
        }

        public static double BytesToDouble(byte[] temp)
        {
            MemoryStream memk1 = new MemoryStream(8);
            BinaryWriter binwritek1 = new BinaryWriter(memk1);
            BinaryReader binreadk1 = new BinaryReader(memk1);
            binwritek1.Write(temp);
            memk1.Position = 0;
            return binreadk1.ReadDouble();
        }

        public static byte[] DoubleToBytes(double data)
        {
            MemoryStream memk1 = new MemoryStream(8);
            BinaryWriter binwritek1 = new BinaryWriter(memk1);
            BinaryReader binreadk1 = new BinaryReader(memk1);
            binwritek1.Write(data);
            memk1.Position = 0;
            byte[] temp = binreadk1.ReadBytes(8);
            return temp;
        }

        public static bool LRCcheck(string str)
        {
            try
            {
                byte[] bytes = StrToBytes(str);
                int[] intArr = new int[bytes.Length];
                for (int i = 0; i < bytes.Length; i++)
                {
                    intArr[i] = Convert.ToInt32(bytes[i]);
                }

                int dataLength = Convert.ToInt32(Convert.ToChar(Convert.ToInt32(bytes[6])).ToString()) * 10 + Convert.ToInt32(Convert.ToChar(Convert.ToInt32(bytes[7])).ToString());
                int data = 0;
                for (int i = 0; i < 8 + dataLength; i++)
                {
                    data += intArr[i];
                }
                data = 255 - data % 256 + 1;
                int dataLRC = Convert.ToInt32(bytes[9 + dataLength]);//2位,第一位默认为16进制30，即十进制0，不参与运算
                if (data == dataLRC || data == dataLRC % 256)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch
            {
                return false;
            }
        }

        public static int GetLRC(string strByte)
        {
            try
            {
                string[] str = strByte.Trim().Split(' ');
                byte[] array = new byte[str.Length];
                for (int i = 0; i < str.Length; i++)
                {
                    array[i] = (byte)Convert.ToInt32(str[i]);
                }

                int dataLength = Convert.ToInt32(Convert.ToChar(array[7]).ToString()) * 10 + Convert.ToInt32(Convert.ToChar(array[8]).ToString());
                int data = 0;
                for (int i = 1; i < 9 + dataLength; i++)
                {
                    data += array[i];
                }
                data = 255 - data % 256 + 1;
                return data;
            }
            catch
            {
                return -1;
            }
        }

    }
}
