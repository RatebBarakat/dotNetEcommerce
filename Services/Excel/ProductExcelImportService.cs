using ecommerce.Data;
using ecommerce.Excel;
using ecommerce.Hubs;
using ecommerce.Models;
using Hangfire;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using System.Drawing;
using System.Security.Claims;

namespace ecommerce.Services.Excel
{
    public class ProductExcelImportService
    {
        private AppDbContext _context;
        private readonly IHubContext<NotificationHub> _hubContext;
        private string _userEmail = string.Empty;


        public ProductExcelImportService(AppDbContext context, IHubContext<NotificationHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
        }

        public void Start(string path, int size, string email)
        {
            List<Product> products = new();
            var excelService = new ExcelImportService<Product>();
            var fileInfo = new FileInfo(path);
            using (var package = new ExcelPackage(fileInfo))
            {
                var count = excelService.GetCountOfRows(package);

                for (int start = 2; start < count; start += size)
                {
                    bool last = start + size >= count;
                    var end = Math.Min(start + size, count);
                    var data = excelService.GetRowsInRange(package, start, end);

                    BackgroundJob.Enqueue(() => ImportFromChunk(data, last, path, email));

                    products.Clear();
                }
            }
        }

        public async Task ImportFromChunk(Dictionary<int, Dictionary<string, string?>> chunkData, bool last, string pathToRemove, string email)
        {
            var products = new List<Product>();
            foreach (var rowNumber in chunkData.Keys)
            {
                var rowData = chunkData[rowNumber];
                string? Name = rowData["Name"];
                int Quantity = int.Parse(rowData["Quantity"]);
                decimal Price = decimal.Parse(rowData["Price"].Trim());
                string? SmallDescription = rowData["SmallDescription"];
                string? Description = rowData["Description"];
                string? categoryName = rowData["Category"];
                int categoryId = await GetCategoryId(categoryName);

                products.Add(new Product
                {
                    Name = Name,
                    Quantity = Quantity,
                    Price = Price,
                    SmallDescription = SmallDescription ?? "",
                    Description = Description ?? "",
                    CategoryId = categoryId
                });
            }

            _context.Products.AddRange(products);

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

                BackgroundJob.Enqueue(() => notify(email, "excelImportCompleted", "product imported successfully", "product"));

            }
            _context.SaveChanges();
        }

        public async Task notify(string userId, string title, string message, string type)
        {
            await _hubContext.Clients.Group(userId).SendAsync(title, message, type);
        }


        private async Task<int> GetCategoryId(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException("name of category is required");
            }

            var category = await _context.Categories.FirstOrDefaultAsync(c => c.Name == name);

            if (category != null)
            {
                return category.Id;
            }
            else
            {
                var newCat = await _context.Categories.AddAsync(new Category
                {
                    Name = name
                });

                await _context.SaveChangesAsync();

                return newCat.Entity.Id;
            }
        }

    }
}
