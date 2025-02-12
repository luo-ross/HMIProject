using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using RS.Widgets.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RS.Widgets.Common.Excels
{
    public class ExcelHelper
    {

        /// <summary>
        /// 获取excel工作簿
        /// </summary>
        /// <param name="filePath">文件路径</param>
        /// <returns></returns>
        /// <exception cref="NotSupportedException">不支持的文件格式</exception>
        public static IWorkbook GetWorkbook(string filePath)
        {
            IWorkbook workbook;
            // 根据文件扩展名选择合适的工作簿类型
            if (filePath.EndsWith(".xlsx"))
            {
                using (FileStream file = new FileStream(filePath, FileMode.Open, FileAccess.Read,FileShare.ReadWrite))
                {
                    return new XSSFWorkbook(file);
                }
            }
            else if (filePath.EndsWith(".xls"))
            {
                using (FileStream file = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    return new HSSFWorkbook(file);
                }
            }
            else
            {
                throw new NotSupportedException("不支持的文件格式");
            }
        }


        public static object ConvertCellValue(string cellValue, DataTypeEnum dataType)
        {
            switch (dataType)
            {
                case DataTypeEnum.Boolean:
                    return bool.TryParse(cellValue, out bool boolResult) ? boolResult : (object)null;

                case DataTypeEnum.Short:
                    return short.TryParse(cellValue, out short shortResult) ? shortResult : (object)null;

                case DataTypeEnum.UShort:
                    return ushort.TryParse(cellValue, out ushort ushortResult) ? ushortResult : (object)null;

                case DataTypeEnum.Int:
                    return int.TryParse(cellValue, out int intResult) ? intResult : (object)null;

                case DataTypeEnum.UInt:
                    return uint.TryParse(cellValue, out uint uintResult) ? uintResult : (object)null;

                case DataTypeEnum.Long:
                    return long.TryParse(cellValue, out long longResult) ? longResult : (object)null;

                case DataTypeEnum.ULong:
                    return ulong.TryParse(cellValue, out ulong ulongResult) ? ulongResult : (object)null;

                case DataTypeEnum.Float:
                    return float.TryParse(cellValue, out float floatResult) ? floatResult : (object)null;

                case DataTypeEnum.Double:
                    return double.TryParse(cellValue, out double doubleResult) ? doubleResult : (object)null;

                case DataTypeEnum.String:
                    return cellValue; // 直接返回字符串

                default:
                    return null;
            }
        }
    }
}
