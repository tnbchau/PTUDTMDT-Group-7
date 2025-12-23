namespace YenMay_web.Models.ViewModels.Common
{
    public class PaginationViewModel
    {
        public int PageIndex { get; set; } = 1;
        public int PageSize { get; set; } = 12;
        public int TotalCount { get; set; }

        public int TotalPages =>
            TotalCount == 0 ? 0 :
            (int)Math.Ceiling(TotalCount / (double)PageSize);

        public bool HasPreviousPage => PageIndex > 1;
        public bool HasNextPage => PageIndex < TotalPages;

        public int StartItem =>
            TotalCount == 0 ? 0 : (PageIndex - 1) * PageSize + 1;

        public int EndItem =>
            TotalCount == 0 ? 0 : Math.Min(PageIndex * PageSize, TotalCount);

        public IEnumerable<int> GetPageNumbers(int pagesToShow = 5)
        {
            if (TotalPages <= 1)
                return Enumerable.Empty<int>();

            pagesToShow = Math.Max(3, pagesToShow);
            var half = pagesToShow / 2;

            var start = Math.Max(1, PageIndex - half);
            var end = Math.Min(TotalPages, start + pagesToShow - 1);

            start = Math.Max(1, end - pagesToShow + 1);

            return Enumerable.Range(start, end - start + 1);
        }
        public string GetPageUrl(int pageNumber, HttpRequest request)
        {
            var queryParams = request.Query.ToDictionary(x => x.Key, x => x.Value.ToString());

            queryParams["page"] = pageNumber.ToString();

            if (queryParams.ContainsKey("pageNumber"))
                queryParams.Remove("pageNumber");

            var queryString = string.Join("&", queryParams.Select(x => $"{x.Key}={x.Value}"));

            return $"{request.Path}?{queryString}";
        }
    }
}