using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.SS.Util;
using NPOI.XSSF.UserModel;
using System;
using System.Data;
using System.IO;

namespace RS.Commons.Helper
{

    public class ExcelHelper : IDisposable
    {
        public string filePath = null; //文件名
        public IWorkbook workbook = null;
        public FileStream fs = null;
        public Stream stream = null;
        public bool disposed;
        public ExcelHelper()
        {
            disposed = false;
        }
        public ExcelHelper(Stream stream)
        {
            this.stream = stream;
            disposed = false;
        }
        public ExcelHelper(string fileName)
        {
            this.filePath = fileName;
            disposed = false;
        }


        public ICell GetValeCell(ISheet sheet, ICell cell)
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

        /// <summary>
        /// 将DataTable数据导入到excel中 
        /// </summary>
        /// <param name="data">要导入的数据</param>
        /// <param name="isColumnWritten">DataTable的列名是否要导入</param>
        /// <param name="sheetName">要导入的excel的sheet的名称</param>
        /// <returns>导入数据行数(包含列名那一行)</returns>
        public int DataTableToExcel(DataTable data, string sheetName, bool isColumnWritten)
        {
            int i = 0;
            int j = 0;
            int count = 0;
            ISheet sheet = null;

            fs = new FileStream(filePath, System.IO.FileMode.Open, System.IO.FileAccess.Read, FileShare.ReadWrite);
            if (filePath.IndexOf(".xlsx") > 0) // 2007版本
                workbook = new XSSFWorkbook();
            else if (filePath.IndexOf(".xls") > 0) // 2003版本
                workbook = new HSSFWorkbook();

            try
            {
                if (workbook != null)
                {
                    sheet = workbook.CreateSheet(sheetName);
                }
                else
                {
                    return -1;
                }

                if (isColumnWritten == true) //写入DataTable的列名
                {
                    IRow row = sheet.CreateRow(0);
                    for (j = 0; j < data.Columns.Count; ++j)
                    {
                        row.CreateCell(j).SetCellValue(data.Columns[j].ColumnName);
                    }
                    count = 1;
                }
                else
                {
                    count = 0;
                }

                for (i = 0; i < data.Rows.Count; ++i)
                {
                    IRow row = sheet.CreateRow(count);
                    for (j = 0; j < data.Columns.Count; ++j)
                    {
                        row.CreateCell(j).SetCellValue(data.Rows[i][j].ToString());
                    }
                    ++count;
                }
                workbook.Write(fs); //写入到excel
                return count;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception: " + ex.Message);
                return -1;
            }
        }

        /// <summary>
        /// 将excel中的数据导入到DataTable中
        /// </summary>
        /// <param name="sheetName">excel工作薄sheet的名称</param>
        /// <param name="startRow">从表的第几行开始取数据</param>
        /// <param name="isFirstRowColumn">第一行是否是DataTable的列名</param>
        /// <returns>返回的DataTable</returns>
        public DataTable ExcelToDataTable(string sheetName, int startRow, bool isFirstRowColumn)
        {
            ISheet sheet = null;
            DataTable data = new DataTable();
            try
            {
                fs = new FileStream(filePath, System.IO.FileMode.Open, System.IO.FileAccess.Read, FileShare.ReadWrite);
                if (filePath.IndexOf(".xlsx") > 0) // 2007版本
                    workbook = new XSSFWorkbook(fs);
                else if (filePath.IndexOf(".xls") > 0) // 2003版本
                    workbook = new HSSFWorkbook(fs);

                if (sheetName != null)
                {
                    sheet = workbook.GetSheet(sheetName);
                    if (sheet == null) //如果没有找到指定的sheetName对应的sheet，则尝试获取第一个sheet
                    {
                        sheet = workbook.GetSheetAt(0);
                    }
                }
                else
                {
                    sheet = workbook.GetSheetAt(0);
                }
                if (sheet != null)
                {
                    IRow firstRow = sheet.GetRow(0);
                    int cellCount = firstRow.LastCellNum; //一行最后一个cell的编号 即总的列数

                    if (isFirstRowColumn)
                    {
                        for (int i = firstRow.FirstCellNum; i < cellCount; ++i)
                        {
                            ICell cell = firstRow.GetCell(i);
                            if (cell != null)
                            {
                                string cellValue = cell.StringCellValue;
                                if (cellValue != null)
                                {
                                    DataColumn column = new DataColumn(cellValue);
                                    data.Columns.Add(column);
                                }
                            }
                        }
                        startRow = sheet.FirstRowNum + 1;
                    }

                    //最后一列的标号
                    int rowCount = sheet.LastRowNum;
                    for (int i = startRow; i <= rowCount; ++i)
                    {
                        IRow row = sheet.GetRow(i);
                        if (row == null) continue; //没有数据的行默认是null　　　　　　　

                        DataRow dataRow = data.NewRow();
                        for (int j = row.FirstCellNum; j < cellCount; ++j)
                        {
                            if (row.GetCell(j) != null) //同理，没有数据的单元格都默认是null
                                dataRow[j] = row.GetCell(j).ToString();
                        }
                        data.Rows.Add(dataRow);
                    }
                }

                return data;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception: " + ex.Message);
                return null;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    if (fs != null)
                    {
                        fs.Close();
                    }
                }

                fs = null;
                disposed = true;
            }
        }
    }
}
