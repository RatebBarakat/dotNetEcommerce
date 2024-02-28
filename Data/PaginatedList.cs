using Microsoft.EntityFrameworkCore;

namespace ecommerce.Data
{
    public class PaginatedList<T>
    {
        public int page { get; set; }

        public ICollection<T> data { get; set; }
        public int total { get; set; }

        public PaginatedList(List<T> items, int total, int pageIndex, int pageSize)
        {
            page = pageIndex;
            this.total = total;
            data = items;
        }

        public bool HasPreviousPage => page > 1;

        public bool HasNextPage => page < total;

        public static async Task<PaginatedList<T>> CreateAsync(IQueryable<T> source, int pageIndex, int pageSize)
        {
            int page = pageIndex > 0 ? pageIndex : 1;
            var count = await source.CountAsync();
            var items = await source.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
            var total = (int)Math.Ceiling((decimal)count / pageSize);
            return new PaginatedList<T>(items, total, page, pageSize);
        }

    }
}
