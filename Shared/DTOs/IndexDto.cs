namespace Shared.DTOs
{
    public class IndexDto
    {
        public int Page { get; set; } = 1;
        public int Limit { get; set; } = 10;
        public string? Search { get; set; }
        public IndexSortDto? Sort { get; set; }
        public List<FilterDto>? Filters { get; set; }
    }
    public class FilterDto
    {
        public string? Field { get; set; }        // مثلاً "Age"
        public string? Operator { get; set; }     // مثلاً "==", "!=", ">", "<", "Contains"
        public string? Value { get; set; }        // مقدار به صورت رشته
    }
    public class IndexResDto<T>
    {
        public int Total { get; set; }
        public int Page { get; set; }
        public int Limit { get; set; }
        public Dictionary<string, string> Details { get; set; } = new();
        public List<T> Data { get; set; }
    }
    public class IndexSortDto
    {
        public string By { get; set; }
        public string Type { get; set; }
    }
}
