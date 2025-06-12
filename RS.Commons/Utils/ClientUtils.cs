using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace RS.Win32API
{
    public static class ClientUtils
    {
        public static bool IsCriticalException(Exception ex)
        {
            if (!(ex is NullReferenceException) && !(ex is StackOverflowException) && !(ex is OutOfMemoryException) && !(ex is ThreadAbortException) && !(ex is ExecutionEngineException) && !(ex is IndexOutOfRangeException))
            {
                return ex is AccessViolationException;
            }

            return true;
        }

        public static bool IsSecurityOrCriticalException(Exception ex)
        {
            if (!(ex is SecurityException))
            {
                return IsCriticalException(ex);
            }

            return true;
        }

        public static int GetBitCount(uint x)
        {
            int num = 0;
            while (x != 0)
            {
                x &= x - 1;
                num++;
            }

            return num;
        }

        public static bool IsEnumValid(Enum enumValue, int value, int minValue, int maxValue)
        {
            return value >= minValue && value <= maxValue;
        }

        public static bool IsEnumValid(Enum enumValue, int value, int minValue, int maxValue, int maxNumberOfBitsOn)
        {
            return value >= minValue && value <= maxValue && GetBitCount((uint)value) <= maxNumberOfBitsOn;
        }

        public static bool IsEnumValid_Masked(Enum enumValue, int value, uint mask)
        {
            return (value & mask) == value;
        }

        public static bool IsEnumValid_NotSequential(Enum enumValue, int value, params int[] enumValues)
        {
            for (int i = 0; i < enumValues.Length; i++)
            {
                if (enumValues[i] == value)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
