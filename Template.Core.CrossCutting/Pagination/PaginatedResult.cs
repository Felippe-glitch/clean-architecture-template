namespace Template.Core.CrossCutting.Pagination
{
    public class PaginatedResult<T> where T : class
    {
        public IList<T> Data {get; set;}
        public int PageNumber {get; set;}
        public int PageSize {get; set;}
        public int TotalItems {get; set;}
        public int TotalPages {get; set;}
        public Boolean HasNext => PageNumber < TotalPages;
        public Boolean HasPrev => PageNumber > 1;

        public PaginatedResult() {}

        public PaginatedResult(IList<T> data, int pageNumber, int pageSize, int totalItems)
        {
            Data = data;
            PageNumber = pageNumber < 1 ? 1 : pageNumber;
            PageSize = pageSize < 1 ? 1 : pageSize;
            TotalItems = totalItems < 0 ? 0 : totalItems;
            TotalPages = (int) Math.Ceiling(totalItems / (double)pageSize);
        }
    }
}
