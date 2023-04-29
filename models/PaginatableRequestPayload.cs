using System;
using System.Collections.Generic;

namespace models
{
    public class PaginatableRequestPayload
    {
        public PaginatableQuery? Query { get; set; }
        public long Limit { get; set; } = 20;
        public long Offset { get; set; } = 0;
    }
    public class PaginatableQuery
    {
        public string? Keyword { get; set; }
        public PaginatableQueryMeta? Meta { get; set; }
        public PaginatableSort? Sort { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }
    public class PaginatableSort
    {
        public string? Id { get; set; }
        public string? Dir { get; set; }
    }
    public class PaginatableQueryMeta
    {
        public string? FormId { get; set; }
    }
    public class PaginatableResponsePayload<T>
    {
        public long? Total { get; set; }
        public List<T> Data { get; set; }
    }
}
