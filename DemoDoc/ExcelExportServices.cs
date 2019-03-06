
public class ExcelExportServices
{
    /// <summary>
    /// 起始行，默认从1开始
    /// </summary>
    private int StartRow = 1;

    /// <summary>
    /// 起始列，默认从1开始
    /// </summary>
    private int StartCol = 1;

    /// <summary>
    /// 字体大小
    /// </summary>
    private int FontSize = 7;

    private double DefaultWidth = 9;
    private double DefaultHeight = 20;

    #region 导出excel

    /// <summary>
    /// 生成excel
    /// </summary>
    /// <param name="dt">数据源主表</param>
    /// <param name="secList">数据源子表</param>
    /// <param name="reportDate">报表时间</param>
    /// <param name="creatorName">操作人</param>
    /// <param name="hotelName">酒店名</param>
    public void GetExcelExport(DataTable dt, Dictionary<string, decimal> secList, DateTime reportDate, string creatorName, string hotelName)
    {
        var ex = new ExcelHelper();
        var ms = GetExcelExport(dt, secList, reportDate, creatorName, hotelName, "");
        ex.ResponseExcel(ms, "ExcelExportReport_" + DateTime.Now);
    }

    /// <summary>
    /// 生成excel
    /// </summary>
    /// <param name="dt">数据源主表</param>
    /// <param name="secList">数据源子表</param>
    /// <param name="reportDate">报表时间</param>
    /// <param name="creatorName">操作人</param>
    /// <param name="hotelName">酒店名</param>
    /// <param name="sheetTitle">sheet名字</param>
    /// <returns></returns>
    private MemoryStream GetExcelExport(DataTable dt, Dictionary<string, decimal> secList
         , DateTime reportDate, string creatorName, string hotelName, string sheetTitle)
    {
        using (ExcelPackage pck = new ExcelPackage())
        {
            int startRow = this.StartRow;
            int startCol = this.StartCol;
            int currentRow = startRow;//当前行的游标
            int currentCol = startCol;//当前列的游标

            #region 添加sheet名
            ExcelWorksheet worksheet = pck.Workbook.Worksheets.Add("sheet1");
            worksheet.DefaultColWidth = 13;
            worksheet.DefaultRowHeight = 24;
            if (string.IsNullOrEmpty(sheetTitle))
            {
                sheetTitle = "ExcelExport";
            }
            worksheet.Name = sheetTitle;
            #endregion

            #region 报表介绍信息
            startRow = currentRow;
            worksheet.Cells[currentRow++, startCol].Value = "营收报表";
            worksheet.Cells[currentRow++, startCol].Value = "ExcelExport  Report";
            worksheet.Cells[currentRow++, startCol].Value = $"打印日期：{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}  酒店日期：{reportDate.ToString("yyyy-MM-dd")}";
            worksheet.Cells[currentRow++, startCol].Value = $"打印操作员：{creatorName}     酒店名称：{hotelName}";
            using (ExcelRange curRange = worksheet.Cells[startRow, startCol, currentRow - 1, startCol])
            {
                curRange.SetStyleFont(name: "微软雅黑", size: this.FontSize);
                curRange.SetStyleAlignment(ExcelHorizontalAlignment.Left, ExcelVerticalAlignment.Center);
            }
            #endregion

            currentRow++;//空行

            #region 营收数据
            var fromRow = currentRow;

            #region 表头
            currentCol = this.StartCol;
            using (ExcelRange curRange = worksheet.Cells[currentRow, currentCol, currentRow + 1, currentCol])
            {
                curRange.Value = "营业项目";
                curRange.Merge = true;
                curRange.SetStyleAlignment(ExcelHorizontalAlignment.Center, ExcelVerticalAlignment.Center);
                curRange.SetStyleFont(name: "微软雅黑", size: this.FontSize);
                curRange.Style.Border.BorderAround(ExcelBorderStyle.Thin);
            }

            currentCol += 1;
            using (ExcelRange curRange = worksheet.Cells[currentRow, currentCol, currentRow, currentCol + 3])
            {
                curRange.Value = "本年实际情况";
                curRange.Merge = true;
                curRange.SetStyleAlignment(ExcelHorizontalAlignment.Center, ExcelVerticalAlignment.Center);
                curRange.SetStyleFont(name: "微软雅黑", size: this.FontSize);
                curRange.Style.Border.BorderAround(ExcelBorderStyle.Thin);
            }

            currentCol += 4;
            using (ExcelRange curRange = worksheet.Cells[currentRow, currentCol, currentRow, currentCol + 3])
            {
                curRange.Value = "去年同期";
                curRange.Merge = true;
                curRange.SetStyleAlignment(ExcelHorizontalAlignment.Center, ExcelVerticalAlignment.Center);
                curRange.SetStyleFont(name: "微软雅黑", size: this.FontSize);
                curRange.Style.Border.BorderAround(ExcelBorderStyle.Thin);
            }

            currentRow++;

            startCol = this.StartCol + 1;
            currentCol = startCol;
            worksheet.Cells[currentRow, currentCol++].Value = "当日";
            worksheet.Cells[currentRow, currentCol++].Value = "月累计";
            worksheet.Cells[currentRow, currentCol++].Value = "上月累计";
            worksheet.Cells[currentRow, currentCol++].Value = "年累计";
            worksheet.Cells[currentRow, currentCol++].Value = "月累计";
            worksheet.Cells[currentRow, currentCol++].Value = "月差异%\n（今年本月-去年同月）/去年同月";
            worksheet.Cells[currentRow, currentCol++].Value = "年累计";
            worksheet.Cells[currentRow, currentCol++].Value = "年差异%\n（本年 - 去年）/ 去年";
            for (int i = startCol; i <= currentCol - 1; i++)
            {
                worksheet.Cells[currentRow, i].Style.WrapText = true;
                worksheet.Cells[currentRow, i].SetStyleFont(name: "微软雅黑", size: this.FontSize);
                worksheet.Cells[currentRow, i].SetStyleAlignment(ExcelHorizontalAlignment.Center, ExcelVerticalAlignment.Center);
                worksheet.Cells[currentRow, i].Style.Border.BorderAround(ExcelBorderStyle.Thin);
            }

            currentRow++;
            #endregion

            #region 表内容
            if (dt != null && dt.Rows.Count > 0)
            {
                startCol = this.StartCol;
                worksheet.Cells[currentRow, startCol].LoadFromDataTable(dt, false);

                startRow = currentRow;
                currentRow += dt.Rows.Count;
                using (ExcelRange curRange = worksheet.Cells[startRow, startCol, currentRow, currentCol - 1])
                {
                    curRange.Style.WrapText = true;
                    curRange.SetStyleFont(name: "微软雅黑", size: this.FontSize);
                    curRange.SetStyleAlignment(ExcelHorizontalAlignment.Center, ExcelVerticalAlignment.Center);
                }

                var itemNameSP = new List<string> {
                        EnumUtil.GetDescriptionByEnum(Model.Enum.Revenue.RevenueItemEnum.RoomFeeIncome),
                        EnumUtil.GetDescriptionByEnum(Model.Enum.Revenue.RevenueItemEnum.CateringIncom),
                        EnumUtil.GetDescriptionByEnum(Model.Enum.Revenue.RevenueItemEnum.RoomIncome),
                        EnumUtil.GetDescriptionByEnum(Model.Enum.Revenue.RevenueItemEnum.OtherIncome),
                        EnumUtil.GetDescriptionByEnum(Model.Enum.Revenue.RevenueItemEnum.TotalIncomeClassification),
                    };
                //首列
                for (int i = startRow; i <= currentRow - 1; i++)
                {
                    if (itemNameSP.Contains(worksheet.Cells[i, 1].Value.ToString()))
                    {
                        worksheet.Cells[i, 1].SetStyleFont(name: "微软雅黑", size: this.FontSize, bold: true);
                        worksheet.Cells[i, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                    }
                }
            }
            else
            {
                dt = new DataTable();
            }
            #endregion

            #endregion

            currentRow++;//空行

            #region 其他分类统计表

            #region 标题
            currentCol = this.StartCol;

            //出租率
            using (ExcelRange curRange = worksheet.Cells[currentRow, currentCol, currentRow, currentCol + 1])
            {
                curRange.Value = "出租率";
                curRange.Merge = true;
                curRange.Style.Font.Bold = true;
            }

            //平均房价
            currentCol += 2;
            using (ExcelRange curRange = worksheet.Cells[currentRow, currentCol, currentRow, currentCol + 1])
            {
                curRange.Value = "平均房价";
                curRange.Merge = true;
                curRange.Style.Font.Bold = true;
            }

            //住店人数
            currentCol += 2;
            using (ExcelRange curRange = worksheet.Cells[currentRow, currentCol, currentRow, currentCol + 1])
            {
                curRange.Value = "住店人数";
                curRange.Merge = true;
                curRange.Style.Font.Bold = true;
            }

            #endregion

            #region 内容
            startRow = currentRow + 1;//数据填写行
            var maxRow = startRow;

            //出租率
            currentRow = startRow;
            currentCol = this.StartCol;
            worksheet.Cells[currentRow, currentCol].Value = "散客";
            worksheet.Cells[currentRow++, currentCol + 1].Value = secList[EnumUtil.GetDescriptionByEnum(Model.Enum.Revenue.RevenueItemEnum.IndividualRentalRate)];

            worksheet.Cells[currentRow, currentCol].Value = "团队";
            worksheet.Cells[currentRow++, currentCol + 1].Value = secList[EnumUtil.GetDescriptionByEnum(Model.Enum.Revenue.RevenueItemEnum.TeamRentalRate)];

            worksheet.Cells[currentRow, currentCol].Value = "会议";
            worksheet.Cells[currentRow++, currentCol + 1].Value = secList[EnumUtil.GetDescriptionByEnum(Model.Enum.Revenue.RevenueItemEnum.ConferenceRentalRate)];

            worksheet.Cells[currentRow, currentCol].Value = "常住";
            worksheet.Cells[currentRow++, currentCol + 1].Value = secList[EnumUtil.GetDescriptionByEnum(Model.Enum.Revenue.RevenueItemEnum.PermanentRentalRate)];

            worksheet.Cells[currentRow, currentCol].Value = "总计";
            worksheet.Cells[currentRow++, currentCol + 1].Value = secList[EnumUtil.GetDescriptionByEnum(Model.Enum.Revenue.RevenueItemEnum.TotalRentalRate)];

            maxRow = currentRow - 1 > maxRow ? currentRow - 1 : maxRow;

            //平均房价
            currentRow = startRow;
            currentCol += 2;
            worksheet.Cells[currentRow, currentCol].Value = "散客";
            worksheet.Cells[currentRow++, currentCol + 1].Value = secList[EnumUtil.GetDescriptionByEnum(Model.Enum.Revenue.RevenueItemEnum.IndividualAverage)];

            worksheet.Cells[currentRow, currentCol].Value = "团队";
            worksheet.Cells[currentRow++, currentCol + 1].Value = secList[EnumUtil.GetDescriptionByEnum(Model.Enum.Revenue.RevenueItemEnum.TeamAverage)];

            maxRow = currentRow - 1 > maxRow ? currentRow - 1 : maxRow;

            //住店人数
            currentRow = startRow;
            currentCol += 2;
            worksheet.Cells[currentRow, currentCol].Value = "内宾";
            worksheet.Cells[currentRow++, currentCol + 1].Value = secList[EnumUtil.GetDescriptionByEnum(Model.Enum.Revenue.RevenueItemEnum.InnerGuests)];

            worksheet.Cells[currentRow, currentCol].Value = "外宾";
            worksheet.Cells[currentRow++, currentCol + 1].Value = secList[EnumUtil.GetDescriptionByEnum(Model.Enum.Revenue.RevenueItemEnum.ForeignGuests)];

            maxRow = currentRow - 1 > maxRow ? currentRow - 1 : maxRow;

            #endregion

            #endregion

            #region 设置样式
            var lastRow = 4 + 1 + 2 + dt.Rows.Count + 1 + 6;//最后一行 35
            var lastCol = 9;//最后一列
            for (int i = 1; i <= lastRow; i++)
            {
                worksheet.Row(i).Height = this.DefaultHeight;
                if (i == 7)
                    worksheet.Row(i).Height = 51;
            }
            for (int i = 1; i <= lastCol; i++)
            {
                worksheet.Column(i).Width = this.DefaultWidth;
                if (i == 1)
                    worksheet.Column(i).Width = 11.5;
            }

            //营业项目+其他表格画细线
            for (int x = 8; x <= lastRow; x++)
            {
                for (int y = 1; y <= lastCol; y++)
                {
                    worksheet.Cells[x, y].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                    if ((x >= 8 && x <= 28 && y == 7)//月差异
                        || (x >= 8 && x <= 28 && y == 9)//年差异
                        || (x >= 31 && x <= lastRow && y == 2)//出租率
                        )
                    {
                        //设置单元格为百分比
                        worksheet.Cells[x, y].Style.Numberformat.Format = "0.00%";
                    }
                }
            }
            //其他分类统计表画粗线
            using (ExcelRange curRange = worksheet.Cells[30, this.StartCol, 30, 6])
            {
                curRange.SetStyleFont(name: "微软雅黑", size: this.FontSize);
                curRange.SetStyleAlignment(ExcelHorizontalAlignment.Center, ExcelVerticalAlignment.Center);
                curRange.Style.Border.BorderAround(ExcelBorderStyle.Thick);
            }
            (new List<int> { 0, 2, 4 }).ForEach(move =>
            {
                using (ExcelRange curRange = worksheet.Cells[30, this.StartCol + move, lastRow, this.StartCol + 1 + move])
                {
                    curRange.SetStyleFont(name: "微软雅黑", size: this.FontSize);
                    curRange.SetStyleAlignment(ExcelHorizontalAlignment.Center, ExcelVerticalAlignment.Center);
                    curRange.Style.Border.BorderAround(ExcelBorderStyle.Thick);
                }
            });
            #endregion

            using (MemoryStream stream = new MemoryStream())
            {
                pck.SaveAs(stream);
                return stream;
            }
        }
    }
    #endregion

}
