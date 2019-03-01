using OfficeOpenXml;
using OfficeOpenXml.Style;
using OfficeOpenXml.Table;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web;

namespace ZJW.Common.Helper
{
    public class ExcelHelper
    {
        private int startRow;
        private int startCol;
        private int defaultColWidth;
        private int defaultRowHeight;

        /// <summary>
        /// 起始行，默认从1开始
        /// </summary>
        public int StartRow
        {
            get { return startRow; }
            set
            {
                if (value < 1)
                {
                    value = 1;
                }
                startRow = value;
            }
        }
        /// <summary>
        /// 起始列，默认从1开始
        /// </summary>
        public int StartCol
        {
            get { return startCol; }
            set
            {
                if (value < 1)
                {
                    value = 1;
                }
                startCol = value;
            }
        }

        /// <summary>
        /// 默认列宽
        /// </summary>
        public int DefaultColWidth
        {
            get { return defaultColWidth; }
            set
            {
                if (value < 0)
                {
                    value = 0;
                }
                defaultColWidth = value;
            }
        }

        /// <summary>
        /// 默认行高
        /// </summary>
        public int DefaultRowHeight
        {
            get { return defaultRowHeight; }
            set
            {
                if (value < 0)
                {
                    value = 0;
                }
                defaultRowHeight = value;
            }
        }

        public TableStyles TableStyle { get; set; }


        public ExcelHelper()
        {
            init(1, 1, 16, 20, TableStyles.None);
        }

        /// <summary>
        /// 选项设置
        /// </summary>
        /// <param name="startRow">EPPlus从1开始计算</param>
        /// <param name="startCol">EPPlus从1开始计算</param>
        /// <param name="defaultRowHeight">默认行高</param>
        /// <param name="defaultColWidth">默认列宽</param>
        /// <param name="tableStyle">表样式</param>
        public void init(int startRow, int startCol, int defaultRowHeight, int defaultColWidth, TableStyles tableStyle)
        {
            this.StartRow = startRow;
            this.StartCol = startCol;
            this.DefaultRowHeight = defaultRowHeight;
            this.DefaultColWidth = defaultColWidth;
            this.TableStyle = tableStyle;
        }

        public MemoryStream DataTableToStream(DataTable dtSource, bool autoColumnHeder, string[] tableHeadNames, bool isFirstRowForTitle, string sheetTitle)
        {
            using (ExcelPackage pck = new ExcelPackage())
            {

                int startRow = this.StartRow;
                int startCol = this.StartCol;
                int dtColCount = dtSource.Columns.Count;
                ExcelWorksheet worksheet = pck.Workbook.Worksheets.Add("sheet1");
                worksheet.DefaultColWidth = DefaultColWidth;
                worksheet.DefaultRowHeight = DefaultRowHeight;
                if (string.IsNullOrEmpty(sheetTitle))
                {
                    sheetTitle = "Table1";
                }
                worksheet.Name = sheetTitle;
                if (isFirstRowForTitle)
                {
                    using (ExcelRange curRange = worksheet.Cells[startRow, startCol, startRow, startCol + dtColCount - 1])
                    {
                        curRange.Value = sheetTitle;
                        curRange.Merge = true;
                        curRange.SetStyleAlignment(ExcelHorizontalAlignment.Center, ExcelVerticalAlignment.Center);

                    }
                    worksheet.Row(startRow).Height = 24;

                    startRow++;
                }

                bool printdtHeader = true;
                if (!autoColumnHeder)
                {
                    printdtHeader = false;
                    for (int i = 0; i < tableHeadNames.Length; i++)
                    {
                        worksheet.Cells[startRow, i + startCol].Value = tableHeadNames[i];
                    }

                    using (ExcelRange curRange = worksheet.Cells[startRow, startCol, startRow, startCol + dtColCount - 1])
                    {
                        curRange.SetStyleAlignment(ExcelHorizontalAlignment.Center);
                        curRange.Style.Font.Bold = true;
                    }
                    worksheet.Row(startRow).Height = DefaultRowHeight + 2;
                    startRow++;
                }

                worksheet.Cells[startRow, startCol].LoadFromDataTable(dtSource, printdtHeader, this.TableStyle);

                foreach (DataColumn col in dtSource.Columns)
                {
                    using (ExcelRange curRange = worksheet.Cells[startRow, col.Ordinal + startCol, startRow + col.Table.Rows.Count, col.Ordinal + startCol])
                    {
                        curRange.SetCellStyleByType(col.DataType);
                    }

                }

                using (MemoryStream stream = new MemoryStream())
                {
                    pck.SaveAs(stream);
                    return stream;
                }
            }
        }



        public void DataTableToExcel(DataTable dtSource, string fileName)
        {
            DataTableToExcel(dtSource, fileName, true, null, false, null);
        }
        /// <summary>
        /// 自定义列表头 
        /// </summary>
        /// <param name="dtSource">数据源</param>
        /// <param name="fileName">导出文件名</param>
        /// <param name="tableHeadNames">自定义表头名称</param>
        public void DataTableToExcel(DataTable dtSource, string fileName, string[] tableHeadNames)
        {
            DataTableToExcel(dtSource, fileName, false, tableHeadNames, false, null);
        }
        /// <summary>
        /// 自定义列表头,sheet标题
        /// </summary>
        /// <param name="dtSource">数据源</param>
        /// <param name="fileName">导出文件名</param>
        /// <param name="tableHeadNames">自定义表头名称</param>
        /// <param name="sheetTitle">自定义sheet标题</param>
        public void DataTableToExcel(DataTable dtSource, string fileName, string[] tableHeadNames, string sheetTitle)
        {
            DataTableToExcel(dtSource, fileName, false, tableHeadNames, true, sheetTitle);
        }
        /// <summary>
        ///  自定义sheet标题
        /// </summary>
        /// <param name="dtSource">数据源</param>
        /// <param name="fileName">导出文件名</param>
        /// <param name="sheetTitle">自定义sheet标题</param>
        public void DataTableToExcel(DataTable dtSource, string fileName, string sheetTitle)
        {
            DataTableToExcel(dtSource, fileName, true, null, false, sheetTitle);
        }

        private void DataTableToExcel(DataTable dtSource, string fileName, bool autoColumnHeder, string[] tableHeadNames, bool isFirstRowForTitle, string sheetTitle)
        {
            using (MemoryStream ms = DataTableToStream(dtSource, autoColumnHeder, tableHeadNames, isFirstRowForTitle, sheetTitle))
            {
                ResponseExcel(ms, fileName);
            }
        }
        /// <summary>
        /// 可迭代集合导出Excel
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="collectionSource">需实体的属性，需加上DisplayName特性，供Excel显示</param>
        /// <param name="propertieNames">需要导出的属性名数组</param>
        /// <param name="fileName">文件名</param>
        /// <param name="sheetName">sheet名</param>
        public void CollectionToExcel<T>(IEnumerable<T> collectionSource, string[] propertieNames, string fileName, string sheetName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
            {
                fileName = "file";
            }
            if (string.IsNullOrWhiteSpace(sheetName))
            {
                sheetName = fileName;
            }

            using (ExcelPackage package = new ExcelPackage())
            {
                ExcelWorksheet worksheet = package.Workbook.Worksheets.Add(sheetName);
                int startRow = StartRow;
                int startCol = StartCol;
                worksheet.DefaultColWidth = DefaultColWidth;
                worksheet.DefaultRowHeight = DefaultRowHeight;


                List<PropertyInfo> properties = typeof(T).GetProperties().ToList();
                if (propertieNames != null && propertieNames.Length > 0)
                {
                    for (int i = properties.Count - 1; i >= 0; i--)
                    {
                        bool hasIn = false;
                        foreach (var name in propertieNames)
                        {
                            if (name == properties[i].Name)
                            {
                                hasIn = true;
                            }
                        }
                        if (!hasIn)
                        {
                            properties.RemoveAt(i);
                        }
                    }
                }


                worksheet.Cells[startRow, startCol].LoadFromCollection<T>(collectionSource, true, this.TableStyle, BindingFlags.GetProperty, properties.ToArray());

                foreach (var col in properties)
                {
                    using (ExcelRange curRange = worksheet.Cells[startRow, startCol, startRow + collectionSource.Count(), startCol++])
                    {
                        curRange.SetCellStyleByType(col.PropertyType);
                    }

                }
                using (MemoryStream stream = new MemoryStream())
                {
                    package.SaveAs(stream);
                    ResponseExcel(stream, fileName);
                }
            }

        }
        /// <summary>
        /// DataSet导出至Excel（一个table对应一个sheet）
        /// </summary>
        /// <param name="dsSource">dataSet数据源</param>
        /// <param name="fileName">文件名称</param>
        public void DataSetToExcelWidthManySheets(DataSet dsSource, string fileName)
        {
            using (MemoryStream ms = DataSetToStreamSheets(dsSource))
            {
                ResponseExcel(ms, fileName);
            }
        }
        /// <summary>
        /// DataSet导出至Excel（ 在一个sheet内，所有表类型一致时使用）
        /// </summary>
        /// <param name="dsSource">dataSet数据源</param>
        /// <param name="fileName">文件名称</param>
        public void DataSetToExcelWidthOneSheet(DataSet dsSource, string fileName)
        {
            using (MemoryStream ms = DataSetToStreamOneSheet(dsSource))
            {
                ResponseExcel(ms, fileName);
            }
        }

        private MemoryStream DataSetToStreamOneSheet(DataSet dsSource)
        {
            using (ExcelPackage package = new ExcelPackage())
            {
                if (dsSource == null)
                {
                    throw new ArgumentNullException("DataSet 不能为null");
                }
                if (string.IsNullOrEmpty(dsSource.DataSetName) && dsSource.Tables.Count > 0)
                {
                    dsSource.DataSetName = dsSource.Tables[0].TableName;
                }

                ExcelWorksheet worksheet = package.Workbook.Worksheets.Add(dsSource.DataSetName);
                int startRow = StartRow;
                int firstRow = StartRow;
                int startCol = StartCol;
                worksheet.DefaultColWidth = DefaultColWidth;
                worksheet.DefaultRowHeight = DefaultRowHeight;

                worksheet.Cells[StartRow, StartCol].LoadFromDataTable(dsSource.Tables[0], true, this.TableStyle);
                //有表头，故+1
                startRow += dsSource.Tables[0].Rows.Count + 1;

                for (int i = 1; i < dsSource.Tables.Count; i++)
                {
                    if (dsSource.Tables[i].Rows.Count > 0)
                    {
                        worksheet.Cells[startRow, startCol].LoadFromDataTable(dsSource.Tables[i], false, this.TableStyle);
                        startRow += dsSource.Tables[i].Rows.Count;
                    }
                }

                foreach (DataColumn col in dsSource.Tables[0].Columns)
                {

                    using (ExcelRange curRange = worksheet.Cells[firstRow + 1, col.Ordinal + startCol, startRow + 1, col.Ordinal + startCol])
                    {
                        curRange.SetCellStyleByType(col.DataType);
                    }

                }
                using (MemoryStream stream = new MemoryStream())
                {
                    package.SaveAs(stream);
                    return stream;
                }
            }
        }

        private MemoryStream DataSetToStreamSheets(DataSet dsSource)
        {
            using (ExcelPackage package = new ExcelPackage())
            {
                if (dsSource == null)
                {
                    throw new ArgumentNullException("DataSet 不能为null");
                }
                for (int i = 0; i < dsSource.Tables.Count; i++)
                {
                    ExcelWorksheet worksheet = package.Workbook.Worksheets.Add(dsSource.Tables[i].TableName);
                    int startRow = StartRow;
                    int startCol = StartCol;
                    worksheet.DefaultColWidth = DefaultColWidth;
                    worksheet.DefaultRowHeight = DefaultRowHeight;
                    worksheet.Cells[startRow, startCol].LoadFromDataTable(dsSource.Tables[i], true, this.TableStyle);
                    foreach (DataColumn col in dsSource.Tables[i].Columns)
                    {
                        using (ExcelRange curRange = worksheet.Cells[startRow + 1, col.Ordinal + startCol, startRow + col.Table.Rows.Count + 1, col.Ordinal + startCol])
                        {
                            curRange.SetCellStyleByType(col.DataType);
                        }
                    }
                }
                using (MemoryStream stream = new MemoryStream())
                {
                    package.SaveAs(stream);
                    return stream;
                }
            }
        }


        /// <summary>
        /// 向客户端发送Excel
        /// </summary>
        /// <param name="ms"></param>
        /// <param name="fileName"></param>
        /// <param name="extensionName"></param>
        private void ResponseExcel(MemoryStream ms, string fileName, string extensionName = "xlsx")
        {
            HttpContext.Current.Response.AddHeader("Content-Disposition", string.Format("attachment;filename={0}.{1}", fileName, extensionName));
            HttpContext.Current.Response.AddHeader("Content-Length", ms.ToArray().Length.ToString());
            HttpContext.Current.Response.ContentEncoding = System.Text.Encoding.UTF8;
            HttpContext.Current.Response.ContentType = "application/vnd.ms-excel";
            HttpContext.Current.Response.BinaryWrite(ms.ToArray());
            HttpContext.Current.Response.Flush();
        }

    }
    /// <summary>
    /// EPPlus扩展类
    /// </summary>
    public static class EPPlusExtend
    {
        /// <summary>
        /// 设置颜色样式（前景色，背景色）
        /// </summary>
        /// <param name="range"></param>
        /// <param name="foreColor"></param>
        /// <param name="bgColor"></param>
        public static ExcelRange SetStyleColor(this ExcelRange range, Color foreColor, Color bgColor = default(Color))
        {
            range.Style.Font.Color.SetColor(foreColor);
            if (bgColor != default(Color))
            {
                range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                range.Style.Fill.BackgroundColor.SetColor(bgColor);
            }

            return range;
        }
        /// <summary>
        /// 设置对齐样式
        /// </summary>
        /// <param name="range"></param>
        /// <param name="hAlign">水平</param>
        /// <param name="vAlign">垂直</param>
        public static ExcelRange SetStyleAlignment(this ExcelRange range, ExcelHorizontalAlignment hAlign, ExcelVerticalAlignment vAlign = ExcelVerticalAlignment.Justify)
        {
            range.Style.HorizontalAlignment = hAlign;
            range.Style.VerticalAlignment = vAlign;

            return range;
        }
        /// <summary>
        /// 根据值得类型设置默认显示样式
        /// </summary>
        /// <param name="range"></param>
        /// <param name="valueType"></param>
        /// <returns></returns>
        public static ExcelRange SetCellStyleByType(this ExcelRange range, Type valueType)
        {

            switch (valueType.ToString())
            {
                case "System.String":
                    break;
                case "System.DateTime":
                    range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    range.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                    range.Style.Numberformat.Format = "yyyy-MM-dd HH:mm:ss";
                    break;
                case "System.Boolean":
                    break;
                case "System.Int16":
                case "System.Int32":
                case "System.Int64":
                case "System.Byte":
                    break;
                case "System.Decimal":
                case "System.Double":

                    break;
                case "System.DBNull":
                    break;
                default:
                    break;
            }
            return range;
        }
    }
}