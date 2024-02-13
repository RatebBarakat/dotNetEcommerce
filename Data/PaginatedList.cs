using Microsoft.EntityFrameworkCore;

namespace ecommerce.Data
{
    public class PaginatedList<T> 
    {
        public int page { get; private set; }

        public ICollection<T> data { get; private set; }
        public int total { get; private set; }

        public PaginatedList(List<T> items, int count, int pageIndex, int pageSize)
        {
            page = pageIndex;
            total = (int)Math.Ceiling(count / (double)pageSize);
            data = items;
        }

        public bool HasPreviousPage => page > 1;

        public bool HasNextPage => page < total;

        public static async Task<PaginatedList<T>> CreateAsync(IQueryable<T> source, int pageIndex, int pageSize)
        {
            var count = await source.CountAsync();
            var items = await source.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToListAsync();
            var totalPages = (int)Math.Ceiling(count / (double)pageSize);
            return new PaginatedList<T>(items, count, pageIndex, pageSize);
        }

    }
}
