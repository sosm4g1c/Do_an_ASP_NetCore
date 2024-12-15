using BookManagement.Models.Entity;
using BookManagement.Models.Model;
using System.Linq.Expressions;
using static BookManagement.Constant.Enumerations;

namespace BookManagement.Service
{
    public interface IBookService : IBaseService<Book>
    {
        List<Book> GetBookActiveInCategoryActive(Expression<Func<Book, bool>> expresstion);
        Task<PagingModel<ApproveReviewModel>> GetPagingReviewBook(ApproveStatus status, int? pageIndex);
    }
}
