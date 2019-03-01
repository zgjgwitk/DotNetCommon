using OfficeOpenXml;
using OfficeOpenXml.Style;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using static ZJW.Common.Helper.EntityHelper;

namespace ZJW.Common.Helper
{
    /// <summary>  
    /// EpPlus读取Excel帮助类+读取csv帮助类  
    /// </summary>  
    public class LoadExcelHelper
    {
        #region 由List创建简单Exel.列头取字段的Description或字段名  
        /// <summary>  
        /// 由List创建简单Exel.列头取字段的Description或字段名  
        /// </summary>  
        /// <typeparam name="T"></typeparam>  
        /// <param name="filePath">The file path.</param>  
        /// <param name="dataList">The data list.</param>  
        public static void CreateExcelByList<T>(Stream newFile, List<T> dataList) where T : class
        {

            //string dirPath = Path.GetDirectoryName(filePath);
            //string fileName = Path.GetFileName(filePath);
            //FileInfo newFile = new FileInfo(filePath);
            //if (newFile.Exists)
            //{
            //    newFile.Delete();  // ensures we create a new workbook  
            //    newFile = new FileInfo(filePath);
            //}
            PropertyInfo[] properties = null;
            if (dataList.Count > 0)
            {
                Type type = dataList[0].GetType();
                properties = type.GetProperties(BindingFlags.Instance | BindingFlags.Public);
                var filedDescriptions = EntityHelper.GetPropertyDescriptions<T>(true);//字段与excel列名对应关系  
                using (ExcelPackage package = new ExcelPackage(newFile))
                {
                    ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("sheet1");
                    //设置表头单元格格式  
                    using (var range = worksheet.Cells[1, 1, 1, properties.Length])
                    {
                        range.Style.Font.Bold = true;
                        range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                        range.Style.Fill.BackgroundColor.SetColor(Color.DarkBlue);
                        range.Style.Font.Color.SetColor(Color.White);
                    }
                    int row = 1, col;
                    object objColValue;
                    string colValue;
                    //表头  
                    for (int j = 0; j < properties.Length; j++)
                    {
                        row = 1;
                        col = j + 1;
                        var description = filedDescriptions.Where(o => o.Key == properties[j].Name).Select(o => o.Value).FirstOrDefault();
                        worksheet.Cells[row, col].Value = (description == null || string.IsNullOrEmpty(description.Description)) ? properties[j].Name : description.Description;
                    }
                    worksheet.View.FreezePanes(row + 1, 1); //冻结表头  
                    //各行数据  
                    for (int i = 0; i < dataList.Count; i++)
                    {
                        row = i + 2;
                        for (int j = 0; j < properties.Length; j++)
                        {
                            col = j + 1;
                            objColValue = properties[j].GetValue(dataList[i], null);
                            colValue = objColValue == null ? "" : objColValue.ToString();
                            worksheet.Cells[row, col].Value = colValue;
                        }
                    }
                    package.Save();
                }

            }
        }
        #endregion

        #region 读取Excel数据到DataSet  
        /// <summary>  
        /// 读取Excel数据到DataSet  
        /// </summary>  
        /// <param name="filePath">The file path.</param>  
        /// <returns></returns>  
        public static DataSet ReadExcelToDataSet(Stream newFile)
        {
            DataSet ds = new DataSet("ds");
            DataRow dr;
            object objCellValue;
            string cellValue;
            //using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.ReadWrite))
            using (ExcelPackage package = new ExcelPackage(newFile))
            {
                foreach (var sheet in package.Workbook.Worksheets)
                {
                    if (sheet.Dimension == null) continue;
                    var columnCount = sheet.Dimension.End.Column;
                    var rowCount = sheet.Dimension.End.Row;
                    if (rowCount > 0)
                    {
                        DataTable dt = new DataTable(sheet.Name);
                        for (int j = 0; j < columnCount; j++)//设置DataTable列名  
                        {
                            objCellValue = sheet.Cells[1, j + 1].Value;
                            cellValue = objCellValue == null ? "" : objCellValue.ToString();
                            dt.Columns.Add(cellValue, typeof(string));
                        }
                        for (int i = 2; i <= rowCount; i++)
                        {
                            dr = dt.NewRow();
                            for (int j = 1; j <= columnCount; j++)
                            {
                                objCellValue = sheet.Cells[i, j].Value;
                                cellValue = objCellValue == null ? "" : objCellValue.ToString();
                                dr[j - 1] = cellValue;
                            }
                            dt.Rows.Add(dr);
                        }
                        ds.Tables.Add(dt);
                    }
                }
            }
            return ds;

        }
        #endregion

        #region 读取csv数据到List<T>,列头与字段的Description对应  
        /// <summary>  
        /// 读取csv数据到List<T>,列头与字段的Description对应  
        /// </summary>  
        /// <typeparam name="T">输出类型</typeparam>  
        /// <param name="filePath">文件路径</param>  
        /// <returns></returns>  
        public static List<T> ReadCsvToModelList<T>(string filePath, string uploadMonth, long userId) where T : class
        {
            List<T> list = new List<T>();
            object objCellValue;
            string cellValue;
            string columnName;
            var filedDescriptions = EntityHelper.GetPropertyDescriptions<T>(false);//字段与excel列名对应关系  
            var fieldIndexs = new List<EntityProperty>();//Excel列索引与字段名对应关系  
            int lineCount = 1;
            string mes = "";
            Type type = typeof(T);
            int iUserId = int.Parse(userId.ToString());
            var properties = type.GetProperties();
            using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
            using (StreamReader sr = new StreamReader(fs, Encoding.Default))
            {

                string line;
                var columnCount = 0;
                bool isEmptyCellValue = false;//是否有必须填写但实际没填写的数据  
                while (true)
                {
                    isEmptyCellValue = false;
                    line = sr.ReadLine();
                    if (string.IsNullOrWhiteSpace(line))
                    {
                        break;
                    }
                    string[] split = SplitCsvLine(line);
                    if (lineCount == 1)//列头  
                    {
                        columnCount = split.Length;
                        for (int j = 0; j < columnCount; j++)//设置DataTable列名  
                        {
                            objCellValue = split[j];
                            cellValue = objCellValue == null ? "" : objCellValue.ToString();
                            var cellFieldName = filedDescriptions.Where(o => o.Key.ToString() == cellValue).Select(o => o.Value).FirstOrDefault();
                            fieldIndexs.Add(new EntityProperty() { Key = j.ToString(), Value = cellFieldName });
                        }
                        lineCount++;
                        continue;
                    }

                    //当第一列为空时退出csv读取  
                    if (string.IsNullOrWhiteSpace(split[0]))
                    {
                        break;
                    }
                    if (split.Length > columnCount)
                    {
                        mes += lineCount.ToString() + ","; //string.Format("第{0}行读取有误，数据列数{1}大于标题列数{2}，请检查该行单元格内是否有逗号<\br>", lineCount, split.Length, dataTypes.Length);  
                        lineCount++;
                        continue;
                    }
                    if (split.Length < columnCount)
                    {
                        mes += lineCount.ToString() + ",";//string.Format("第{0}行数据读取有误，数据列数{1}小于标题列数{2}，请检查该行单元格内是否有逗号<\br>", lineCount, split.Length, dataTypes.Length);  
                        lineCount++;
                        continue;
                    }
                    T model = Activator.CreateInstance<T>();

                    for (int j = 0; j < columnCount; j++)
                    {
                        objCellValue = split[j];
                        var field = fieldIndexs.First(o => o.Key == j.ToString()).Value;
                        columnName = field.FieldName;
                        if (string.IsNullOrEmpty(columnName)) continue;
                        PropertyInfo p = properties.FirstOrDefault(o => string.Equals(o.Name, columnName, StringComparison.InvariantCultureIgnoreCase));
                        if (p == null) continue;
                        SetPropertyValue<T>(ref model, ref p, ref objCellValue, ref field, ref isEmptyCellValue);
                    }
                    if (isEmptyCellValue)
                    {
                        continue;
                    }
                    SetPropertyValueForKnowColumns<T>(ref model, ref properties, ref uploadMonth, ref userId, ref iUserId);
                    list.Add(model);
                    lineCount++;
                }
            }
            if (mes != "")
                throw new Exception("第" + mes.TrimEnd(',') + "行读取有误，请检查该行单元格内是否有逗号");
            #region 判断第一行数据是否合格，Excel列名和字段对不上的情况，检查一个就等于检查全部  
            var firstModel = list.FirstOrDefault();
            CheckModelRequiredData<T>(ref firstModel, ref properties, ref filedDescriptions);
            #endregion
            return list;
        }

        #endregion

        #region 设置字段属性的值  
        /// <summary>  
        /// 设置字段属性的值  
        /// </summary>  
        /// <typeparam name="T"></typeparam>  
        /// <param name="model">The model.</param>  
        /// <param name="p">The p.</param>  
        /// <param name="objCellValue">The object cell value.</param>  
        /// <param name="field">The field.</param>  
        /// <param name="isEmptyCellValue">if set to <c>true</c> [is empty cell value].</param>  
        /// <exception cref="BusinessException">出错，字段名： + p.Name + ,类型： + p.PropertyType.ToString() + ,数据： + (objCellValue == null ?  : objCellValue.ToString()) + ,错误信息： + ex.Message</exception>  
        private static void SetPropertyValue<T>(ref T model, ref PropertyInfo p, ref object objCellValue, ref EntityDescription field, ref bool isEmptyCellValue) where T : class
        {
            var propertyType = p.PropertyType.ToString();
            switch (propertyType)
            {
                case "System.String":
                    {
                        if (objCellValue is string == false)
                        {
                            if (objCellValue == null) objCellValue = "";
                            else objCellValue = objCellValue.ToString();
                            if (field.IsRequire && objCellValue.ToString() == "")
                            {
                                isEmptyCellValue = true;
                                break;
                            }
                        }
                        if (objCellValue != null) objCellValue = objCellValue.ToString().Replace("\r\n", "").Trim();
                    }
                    break;
                case "System.Decimal":
                case "System.Nullable`1[System.Decimal]":
                    {
                        if (objCellValue is decimal == false)
                        {
                            if (objCellValue != null)
                            {
                                if (objCellValue.ToString().EndsWith("%"))
                                {
                                    objCellValue = Convert.ToDecimal(objCellValue.ToString().TrimEnd('%')) / 100M;
                                }
                                else objCellValue = Convert.ToDecimal(objCellValue.ToString());
                            }
                        }
                    }
                    break;
                case "System.Int32":
                case "System.Nullable`1[System.Int32]":
                    {
                        if (objCellValue is int == false)
                        {
                            if (objCellValue != null) objCellValue = Convert.ToInt32(objCellValue);
                        }
                    }
                    break;
                case "System.Int64":
                case "System.Nullable`1[System.Int64]":
                    {
                        if (objCellValue is long == false)
                        {
                            if (objCellValue != null) objCellValue = Convert.ToInt64(objCellValue);
                        }
                    }
                    break;
                case "System.DateTime":
                case "System.Nullable`1[System.DateTime]":
                    {
                        if (objCellValue is DateTime == false)
                        {
                            if (objCellValue != null) objCellValue = ToDateTimeValue(objCellValue.ToString());
                        }
                    }
                    break;
                case "System.Boolean":
                case "System.Nullable`1[System.Boolean]":
                    {
                        if (objCellValue is bool == false)
                        {

                            if (objCellValue != null)
                            {
                                var tempValue = objCellValue.ToString().Trim();
                                if (tempValue == "#N/A") tempValue = "";
                                else if (tempValue == "是") tempValue = "True";
                                else if (tempValue == "否") tempValue = "False";
                                if (tempValue != "") objCellValue = Convert.ToBoolean(tempValue);
                                else objCellValue = null;
                            }
                        }
                    }
                    break;
            }
            try
            {
                p.SetValue(model, objCellValue, null);
            }
            catch (Exception ex)
            {
                throw new Exception("出错，字段名：" + p.Name + ",类型：" + p.PropertyType.ToString()
                    + ",数据：" + (objCellValue == null ? "" : objCellValue.ToString())
                    + ",错误信息：" + ex.Message, ex);
            }
        }
        #endregion

        #region 其他已知属性赋默认值  
        /// <summary>  
        /// 其他已知属性赋默认值  
        /// </summary>  
        /// <typeparam name="T"></typeparam>  
        /// <param name="model">The model.</param>  
        /// <param name="properties">The properties.</param>  
        /// <param name="uploadMonth">The upload month.</param>  
        /// <param name="userId">The user identifier.</param>  
        /// <param name="iUserId">The i user identifier.</param>  
        private static void SetPropertyValueForKnowColumns<T>(ref T model, ref PropertyInfo[] properties, ref string uploadMonth, ref long userId, ref int iUserId)
        {
            var monthProperty = properties.FirstOrDefault(o => string.Equals(o.Name, "Month", StringComparison.InvariantCultureIgnoreCase));
            if (monthProperty != null)
            {
                monthProperty.SetValue(model, uploadMonth, null);
            }
            var createTimeProperty = properties.FirstOrDefault(o => string.Equals(o.Name, "CreationTime", StringComparison.InvariantCultureIgnoreCase));
            if (createTimeProperty != null)
            {
                createTimeProperty.SetValue(model, DateTime.Now, null);
            }
            var modifyTimeProperty = properties.FirstOrDefault(o => string.Equals(o.Name, "LastModificationTime", StringComparison.InvariantCultureIgnoreCase));
            if (modifyTimeProperty != null)
            {
                modifyTimeProperty.SetValue(model, DateTime.Now, null);
            }
            var createUserIdProperty = properties.FirstOrDefault(o => string.Equals(o.Name, "CreatorUserId", StringComparison.InvariantCultureIgnoreCase));
            if (createUserIdProperty != null)
            {
                if (createTimeProperty.PropertyType.ToString() == "System.Int32") createUserIdProperty.SetValue(model, iUserId, null);
                else createUserIdProperty.SetValue(model, userId, null);
            }
        }
        #endregion

        #region 最后判断第一行数据是否合格，Excel列名和字段对不上的情况，检查一个就等于检查全部  
        /// <summary>  
        /// 最后判断第一行数据是否合格，Excel列名和字段对不上的情况，检查一个就等于检查全部  
        /// </summary>  
        /// <typeparam name="T"></typeparam>  
        /// <param name="model">The model.</param>  
        /// <param name="filedDescriptions">The filed descriptions.</param>  
        private static void CheckModelRequiredData<T>(ref T firstModel, ref PropertyInfo[] properties, ref List<EntityProperty> filedDescriptions)
        {
            if (firstModel != null)
            {
                var fieldNameList = filedDescriptions.Where(o => o.Value != null).Select(o => o.Value).ToList();

                foreach (var p in properties)
                {
                    var fieldNameModel = fieldNameList.FirstOrDefault(o => string.Equals(o.FieldName, p.Name, StringComparison.InvariantCultureIgnoreCase));
                    if (fieldNameModel == null || fieldNameModel.IsRequire == false) continue;//为空或没有Require标记，跳过  
                    object objCellValue = p.GetValue(firstModel);
                    if (objCellValue == null || objCellValue.ToString() == "")
                    {
                        throw new Exception("出错，字段名：" + p.Name + ",类型：" + p.PropertyType.ToString() + " 必填字段数据为空！");
                    }
                }
            }
        }
        #endregion

        #region 读取Excel数据到List<T>,列头与字段的Description对应  
        /// <summary>  
        /// 读取Excel数据到List<T>,列头与字段的Description对应  
        /// </summary>  
        /// <typeparam name="T"></typeparam>  
        /// <param name="filePath">The file path.</param>  
        /// <returns></returns>  
        public static List<T> ReadExcelToModelList<T>(Stream newFile, string uploadMonth, long userId, out string log, int titleRow = 1) where T : class
        {
            ExcelPackage package = null;
            FileStream fs = null;
            try
            {
                //ConcurrentBag<T> list = new ConcurrentBag<T>();  
                List<T> list = new List<T>();
                log = string.Format("{0:yyyy-MM-dd HH:mm:ss}开始载入文件\r\n", DateTime.Now);
                var filedDescriptions = EntityHelper.GetPropertyDescriptions<T>(false);//字段与excel列名对应关系  
                var fieldIndexs = new List<KeyValuePair<int, EntityDescription>>();//Excel列索引与字段名对应关系  
                //fs = new FileStream(filePath, FileMode.Open, FileAccess.ReadWrite);
                package = new ExcelPackage();
                package.Load(newFile);
                log += string.Format("{0:yyyy-MM-dd HH:mm:ss}完成载入文件{1}，开始解析\r\n", DateTime.Now);
                if (package.Workbook.Worksheets.Count == 0)
                {
                    throw new Exception("读取的Excel中sheet数量为0，请检查文件格式是否为xlsx！");
                }
                var sheet = package.Workbook.Worksheets[1];

                if (sheet.Dimension == null) return list.ToList();
                var columnCount = sheet.Dimension.End.Column;
                var rowCount = sheet.Dimension.End.Row;
                if (rowCount == 0) return list.ToList();
                //DataTable dt = new DataTable(sheet.Name);  
                for (int j = 1; j <= columnCount; j++)//列名与字段名对应关系  
                {
                    object objCellValue = sheet.Cells[titleRow, j].Value;
                    string cellValue = objCellValue == null ? "" : objCellValue.ToString();
                    if (cellValue.Length == 0) continue;
                    var cellFieldName = filedDescriptions.Where(o => o.Key.ToString() == cellValue).Select(o => o.Value).FirstOrDefault();
                    fieldIndexs.Add(new KeyValuePair<int, EntityDescription>(j, cellFieldName));
                    //dt.Columns.Add(cellValue, typeof(string));  
                }
                Type type = typeof(T);
                int iUserId = int.Parse(userId.ToString());
                var properties = type.GetProperties();
                //List<int> rowIndexList = Enumerable.Range(titleRow + 1, rowCount - titleRow).ToList();//数据行的行号集合  
                //Parallel.ForEach(rowIndexList, (i) =>  
                for (int i = titleRow + 1; i <= rowCount; i++)
                {
                    #region 处理Excel每行数据  
                    object objCellValue = null;
                    string columnName = null;
                    bool isEmptyCellValue = false;//是否有必须填写但实际没填写的数据  
                    T model = Activator.CreateInstance<T>();
                    for (int j = 1; j <= columnCount; j++)
                    {
                        objCellValue = sheet.Cells[i, j].Value;
                        var fieldPair = fieldIndexs.FirstOrDefault(o => o.Key == j);
                        var field = fieldPair.Value;
                        columnName = field == null ? "" : field.FieldName;
                        if (string.IsNullOrEmpty(columnName)) continue;
                        PropertyInfo p = properties.FirstOrDefault(o => string.Equals(o.Name, columnName, StringComparison.InvariantCultureIgnoreCase));
                        if (p == null) continue;
                        SetPropertyValue<T>(ref model, ref p, ref objCellValue, ref field, ref isEmptyCellValue);

                    }
                    if (!isEmptyCellValue)
                    {
                        SetPropertyValueForKnowColumns<T>(ref model, ref properties, ref uploadMonth, ref userId, ref iUserId);
                        list.Add(model);
                    }

                    #endregion
                }
                //);  
                #region 判断第一行数据是否合格，Excel列名和字段对不上的情况，检查一个就等于检查全部  
                var firstModel = list.FirstOrDefault();
                CheckModelRequiredData<T>(ref firstModel, ref properties, ref filedDescriptions);
                #endregion
                log += string.Format("{0:yyyy-MM-dd HH:mm:ss}完成解析文件\r\n", DateTime.Now);
                return list;
            }
            finally
            {
                if (package != null) package.Dispose();//释放Excel对象资源  
                if (fs != null)//关闭和释放文件流资源  
                {
                    fs.Close(); fs.Dispose();
                }
            }

        }
        #endregion

        #region Splits the CSV line.  
        /// <summary>  
        /// Splits the CSV line.  
        /// </summary>  
        /// <param name="s">The s.</param>  
        /// <returns></returns>  
        private static string[] SplitCsvLine(string s)
        {
            Regex regex = new Regex("\".*?\"");
            var a = regex.Matches(s).Cast<Match>().Select(m => m.Value).ToList();
            var b = regex.Replace(s, "%_%");
            var c = b.Split(',');
            for (int i = 0, j = 0; i < c.Length && j < a.Count; i++)
            {
                if (c[i] == "%_%")
                {
                    c[i] = a[j++];
                }
            }
            return c;
        }
        #endregion

        #region Excel中数字时间转换成时间格式  
        /// <summary>  
        /// Excel中数字时间转换成时间格式  
        /// </summary>  
        /// <param name="timeStr">数字,如:42095.7069444444/0.650694444444444</param>  
        /// <returns>日期/时间格式</returns>  
        public static DateTime ToDateTimeValue(string strNumber)
        {
            if (!string.IsNullOrWhiteSpace(strNumber))
            {
                Decimal tempValue;
                DateTime tempret;
                if (DateTime.TryParse(strNumber, out tempret))
                {
                    return tempret;
                }
                if (strNumber.Length == 8 && strNumber.Contains(".") == false)//20160430  
                {

                    strNumber = strNumber.Insert(4, "-").Insert(6 + 1, "-");
                    if (DateTime.TryParse(strNumber, out tempret))
                    {
                        return tempret;
                    }
                    else return default(DateTime);
                }
                //先检查 是不是数字;  
                if (Decimal.TryParse(strNumber, out tempValue))
                {
                    //天数,取整  
                    int day = Convert.ToInt32(Math.Truncate(tempValue));
                    //这里也不知道为什么. 如果是小于32,则减1,否则减2  
                    //日期从1900-01-01开始累加   
                    // day = day < 32 ? day - 1 : day - 2;  
                    DateTime dt = new DateTime(1900, 1, 1).AddDays(day < 32 ? (day - 1) : (day - 2));

                    //小时:减掉天数,这个数字转换小时:(* 24)   
                    Decimal hourTemp = (tempValue - day) * 24;//获取小时数  
                    //取整.小时数  
                    int hour = Convert.ToInt32(Math.Truncate(hourTemp));
                    //分钟:减掉小时,( * 60)  
                    //这里舍入,否则取值会有1分钟误差.  
                    Decimal minuteTemp = Math.Round((hourTemp - hour) * 60, 2);//获取分钟数  
                    int minute = Convert.ToInt32(Math.Truncate(minuteTemp));

                    //秒:减掉分钟,( * 60)  
                    //这里舍入,否则取值会有1秒误差.  
                    Decimal secondTemp = Math.Round((minuteTemp - minute) * 60, 2);//获取秒数  
                    int second = Convert.ToInt32(Math.Truncate(secondTemp));
                    if (second >= 60)
                    {
                        second -= 60;
                        minute += 1;
                    }
                    if (minute >= 60)
                    {
                        minute -= 60;
                        hour += 1;
                    }

                    //时间格式:00:00:00  
                    string resultTimes = string.Format("{0}:{1}:{2}",
                            (hour < 10 ? ("0" + hour) : hour.ToString()),
                            (minute < 10 ? ("0" + minute) : minute.ToString()),
                            (second < 10 ? ("0" + second) : second.ToString()));
                    var str = string.Format("{0} {1}", dt.ToString("yyyy-MM-dd"), resultTimes);
                    try
                    {
                        return DateTime.Parse(str);
                    }
                    catch (Exception ex)
                    {
                        throw new Exception("DateTime.Parse出错,str：" + str, ex);
                    }

                }
            }
            return default(DateTime);
        }
        #endregion

    }
}
