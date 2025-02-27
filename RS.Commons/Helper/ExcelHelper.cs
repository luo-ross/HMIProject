using NPOI.HSSF.UserModel;
using NPOI.SS.Formula.Functions;
using NPOI.SS.UserModel;
using NPOI.SS.Util;
using NPOI.XSSF.UserModel;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RS.Commons.Helper
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
            if (Path.GetExtension(filePath).Equals(".xlsx", StringComparison.OrdinalIgnoreCase))
            {
                using (FileStream file = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    return new XSSFWorkbook(file);
                }
            }
            else if (Path.GetExtension(filePath).Equals(".xls", StringComparison.OrdinalIgnoreCase))
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

        /// <summary>
        /// 创建Excel工作簿
        /// </summary>
        /// <param name="filePath">文件路径</param>
        /// <returns></returns>
        /// <exception cref="NotSupportedException"></exception>
        public static IWorkbook CreateWorkbook(string filePath)
        {
            IWorkbook workbook;
            // 根据文件扩展名选择合适的工作簿类型
            if (Path.GetExtension(filePath).Equals(".xlsx", StringComparison.OrdinalIgnoreCase))
            {
                workbook = new XSSFWorkbook();
            }
            else if (Path.GetExtension(filePath).Equals(".xls", StringComparison.OrdinalIgnoreCase))
            {
                workbook = new HSSFWorkbook();
            }
            else
            {
                throw new NotSupportedException("不支持的文件格式");
            }

            return workbook;
        }


        public static short ConvertHexColorToExcelColor(IWorkbook workbook, string hexColor)
        {
            // 去除十六进制颜色代码中的 # 符号（如果有的话）
            if (hexColor.StartsWith("#"))
            {
                hexColor = hexColor.Substring(1);
            }

            // 解析 RGB 值
            byte red = Convert.ToByte(hexColor.Substring(0, 2), 16);
            byte green = Convert.ToByte(hexColor.Substring(2, 2), 16);
            byte blue = Convert.ToByte(hexColor.Substring(4, 2), 16);

            // 获取调色板
            XSSFColor color = new XSSFColor(new byte[] { red, green, blue });

            // 获取调色板的索引（这里对于 XSSF 不需要手动添加到调色板，直接使用颜色对象）
            return color.Indexed;
        }



        public ICell GetValueCell(ISheet sheet, ICell cell)
        {
            ICell cellValue = null;
            if (cell.IsMergedCell)
            {
                for (int ii = 0; ii < sheet.NumMergedRegions; ii++)
                {
                    var cellrange = sheet.GetMergedRegion(ii);
                    if (cell.ColumnIndex >= cellrange.FirstColumn && cell.ColumnIndex <= cellrange.LastColumn
                        && cell.RowIndex >= cellrange.FirstRow && cell.RowIndex <= cellrange.LastRow)
                    {
                        var mergeCellValue = sheet.GetRow(cellrange.FirstRow).GetCell(cellrange.FirstColumn);
                        if (mergeCellValue != null)
                        {
                            cellValue = mergeCellValue;
                        }
                    }
                }
                if (cellValue == null)
                {
                    cellValue = cell;
                }
            }
            else
            {
                cellValue = cell;
            }
            return cellValue;
        }

        // 判断合并单元格重载
        // 调用时要在输出变量前加 out
        public bool IsMergeCell(ISheet sheet, int rowIndex, int colIndex, out int rowSpan, out int colSpan)
        {
            bool result = false;
            int regionsCount = sheet.NumMergedRegions;
            rowSpan = 1;
            colSpan = 1;
            for (int i = 0; i < regionsCount; i++)
            {
                CellRangeAddress range = sheet.GetMergedRegion(i);
                sheet.IsMergedRegion(range);
                if (range.FirstRow == rowIndex && range.FirstColumn == colIndex)
                {
                    rowSpan = range.LastRow - range.FirstRow + 1;
                    colSpan = range.LastColumn - range.FirstColumn + 1;
                    break;
                }
            }
            try
            {
                result = sheet.GetRow(rowIndex).GetCell(colIndex).IsMergedCell;
            }
            catch
            {
            }
            return result;
        }

        public object GetCellValue(ICell cell)
        {
            switch (cell.CellType)
            {
                case CellType.Boolean:
                    {
                        return cell.BooleanCellValue;
                    }
                case CellType.Numeric:
                    {
                        return cell.NumericCellValue;
                    }
                case CellType.String:
                    {
                        return cell.StringCellValue;
                    }
                case CellType.Error:
                    {
                        return cell.ErrorCellValue;
                    }
                case CellType.Formula:
                    {
                        return cell.CellFormula == null ? "" : cell.CellFormula;
                    }
                default:
                    return "";
            }

        }

        public void CloneCellValue(ICell newCell, ICell sourceCell)
        {
            switch (sourceCell.CellType)
            {
                case CellType.Boolean:
                    {
                        newCell.SetCellValue(sourceCell.BooleanCellValue);
                        break;
                    }
                case CellType.Numeric:
                    {
                        newCell.SetCellValue(sourceCell.NumericCellValue);
                        break;
                    }
                case CellType.String:
                    {
                        newCell.SetCellValue(sourceCell.StringCellValue);
                        break;
                    }
                case CellType.Error:
                    {
                        newCell.SetCellValue(sourceCell.ErrorCellValue);
                        break;
                    }
                case CellType.Formula:
                    {
                        newCell.SetCellFormula(sourceCell.CellFormula);
                        break;
                    }
            }

        }

    }
}
