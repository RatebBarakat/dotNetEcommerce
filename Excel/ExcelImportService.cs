using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using OfficeOpenXml;

namespace ecommerce.Excel
{
    public class ExcelImportService<T> where T : class, new()
    {

        public ExcelImportService()
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        }

        public int GetCountOfRows(ExcelPackage excel)
        {
            return excel.Workbook.Worksheets[0].Dimension.Rows;
        }

        public Dictionary<int, Dictionary<string, string?>> GetRowsInRange(ExcelPackage excel, int startRow, int endRow)
        {
            var rows = new Dictionary<int, Dictionary<string, string?>>();
            var worksheet = excel.Workbook.Worksheets[0];
            for (int row = startRow; row <= endRow; row++)
            {
                var rowData = new Dictionary<string, string?>();
                for (int col = 1; col <= worksheet.Dimension.Columns; col++)
                {
                    var columnName = worksheet.Cells[1, col].Value?.ToString();
                    var cellValue = worksheet.Cells[row, col].Value?.ToString();
                    rowData.Add(columnName, cellValue);
                }
                rows.Add(row, rowData);
            }
            return rows;
        }
    }
}
