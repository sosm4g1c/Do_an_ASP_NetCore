using BookManagement.Models.Entity;

namespace BookManagement.Models.Model
{
    public class BookPagingModel
    {
        public string? Keyword { get; set; }
        public int? CategoryId { get; set; }
        public PagingModel<Book> Paging { get; set; }
    }
}
