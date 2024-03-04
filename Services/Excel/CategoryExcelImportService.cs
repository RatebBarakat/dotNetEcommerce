using ecommerce.Data;
using ecommerce.Excel;
using ecommerce.Models;
using Hangfire;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using System.Drawing;

namespace ecommerce.Services.Excel
{
    public class CategoryExcelImportService
    {
        private AppDbContext _context;

        public CategoryExcelImportService(AppDbContext context)
        {
            _context = context;
        }

        public void Start(string path, int size)
        {
            List<Category> products = new();
            var excelService = new ExcelImportService<Category>();
            var fileInfo = new FileInfo(path);
            using (var package = new ExcelPackage(fileInfo))
            {
                var count = excelService.GetCountOfRows(package);
                for (int start = 2; start < count; start += size)
                {
                    bool last = start + size >= count;
                    var end = Math.Min(start + size, count);
                    var data = excelService.GetRowsInRange(package, start, end);

                    BackgroundJob.Enqueue(() => ImportFromChunk(data, last, path));

                    products.Clear();
                }
            }
        }

        public void ImportFromChunk(Dictionary<int, Dictionary<string, string?>> chunkData, bool last, string pathToRemove)
        {
            var categories = new List<Category>();
            foreach (var rowNumber in chunkData.Keys)
            {
                var rowData = chunkData[rowNumber];
                string? Name = rowData["Name"];

                categories.Add(new Category
                {
                    Name = Name,
                });
            }

            _context.Categories.AddRange(categories);

            if (last == true)
            {
                _context.Categories.Add(new Category
                {
                    Name = "complete import"
                });

                if (File.Exists(pathToRemove))
                {
                    File.Delete(pathToRemove);
                }
            }

            _context.SaveChanges();
        }
    }
}
