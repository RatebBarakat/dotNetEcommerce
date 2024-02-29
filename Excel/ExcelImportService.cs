using System;
using System.IO;
using Microsoft.AspNetCore.Http;
using OfficeOpenXml;
using System.Reflection;

namespace ecommerce.Excel
{
    public class ExcelImportService<T> where T : class, new()
    {
        private readonly IFormFile _file;
        private ExcelPackage _excelPackage;

        public ExcelImportService(IFormFile file)
        {
            _file = file ?? throw new ArgumentNullException(nameof(file));
            using var stream = new MemoryStream();
            _file.CopyTo(stream);
            stream.Position = 0;
            _excelPackage = new ExcelPackage(stream);
        }

        public int GetCountOfRows()
        {
            return _excelPackage.Workbook.Worksheets[0].Rows.Count() + 1;
        }

        public string? GetAttributeValue(string propertyName, int rowNumber)
        {
            if (string.IsNullOrEmpty(propertyName))
                throw new ArgumentException("Property name cannot be null or empty.", nameof(propertyName));

            if (rowNumber < 0)
                throw new ArgumentException("Row number must be greater than or equal to 1.", nameof(rowNumber));

            var worksheet = _excelPackage.Workbook.Worksheets[0];

            PropertyInfo propertyInfo = typeof(T).GetProperty(propertyName);
            if (propertyInfo == null)
                throw new ArgumentException($"Property '{propertyName}' not found in type '{typeof(T).Name}'.");

            int columnIndex = GetColumnIndex(worksheet, propertyName);
            if (columnIndex == -1)
                throw new ArgumentException($"Property '{propertyName}' not found in the Excel sheet.");

            if (worksheet.Dimension == null || rowNumber > worksheet.Dimension.End.Row)
                throw new ArgumentException($"Row number '{rowNumber}' exceeds the number of rows in the Excel sheet.");

            string? cellValue = worksheet.Cells[rowNumber, columnIndex].Value?.ToString();
            return cellValue;
        }

        private int GetColumnIndex(ExcelWorksheet worksheet, string propertyName)
        {
            int columnIndex = -1;
            int totalColumns = worksheet.Dimension.End.Column;

            for (int i = 1; i <= totalColumns; i++)
            {
                if (worksheet.Cells[1, i].Value?.ToString() == propertyName)
                {
                    columnIndex = i;
                    break;
                }
            }

            return columnIndex;
        }

        public Dictionary<int, Dictionary<string, string?>> GetRowsInRange(int startRow, int endRow)
        {
            var worksheet = _excelPackage.Workbook.Worksheets[0];
            var rows = new Dictionary<int, Dictionary<string, string?>>();

            for (int row = startRow; row <= endRow; row++)
            {
                var rowData = new Dictionary<string, string?>();
                foreach (var property in typeof(T).GetProperties())
                {
                    var propertyName = property.Name;
                    var columnIndex = GetColumnIndex(worksheet, propertyName);

                    if (columnIndex != -1)
                    {
                        var cellValue = worksheet.Cells[row, columnIndex].Value?.ToString();
                        rowData.Add(propertyName, cellValue);
                    }
                    else
                    {
                        rowData.Add(propertyName, null);
                    }
                }
                rows.Add(row, rowData);
            }

            return rows;
        }
    }
}
