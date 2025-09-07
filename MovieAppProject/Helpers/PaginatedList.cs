using Microsoft.EntityFrameworkCore;

namespace MovieAppProject.Helpers
{
    public class PaginatedList<T> : List<T>
    {
        public int PageIndex { get; set; }
        public int TotalPages { get; set; }

        public PaginatedList(List<T>Items, int count, int pageIndex, int pageSize) 
        {
           PageIndex = pageIndex;
           TotalPages = (int)Math.Ceiling(count / (double)pageSize);     
           this.AddRange(Items);
        }

        public bool HasPreviousPage => PageIndex > 1;
        public bool HasNextPage => PageIndex < TotalPages;

        public static async Task<PaginatedList<T>> CreateAysnc(IQueryable<T>source, int pageIndex,int pageSize)
        {
            var count = await source.CountAsync();
            var items = await source.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToListAsync();
            return new PaginatedList<T>(items, count, pageIndex, pageSize);
        }
    }
}
