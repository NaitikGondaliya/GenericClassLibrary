using System;
using System.Collections.Generic;
using System.Text;

namespace ShivOhm.Infrastructure
{
    public class ApiResponseModel
    {
        public object Data { get; set; }
        public string Message { get; set; }
        public object StatusCodes { get; set; }
    }

    public class PaginationModel<TEntity> where TEntity : class
    {
        public int TotalCount { get; set; }
        public int FilteredCount { get; set; }
        public IReadOnlyList<TEntity> PageData { get; set; }
    }
}
