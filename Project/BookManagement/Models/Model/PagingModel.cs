using BookManagement.Models.Entity;
using System.ComponentModel.DataAnnotations;
using X.PagedList;

namespace BookManagement.Models.Model
{
    public class PagingModel<T> where T : class
    {
        public int TotalRecord { get; set; }
        public IPagedList<T> DataPaging { get; set; }
    }

    public class NewsPagingModel
    {
        public string? Keyword { get; set; }
        public PagingModel<News> Paging { get; set; }
    }
}
