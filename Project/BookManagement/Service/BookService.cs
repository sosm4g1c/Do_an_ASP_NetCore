using AutoMapper;
using BookManagement.Constant;
using BookManagement.Data;
using BookManagement.Models.Entity;
using BookManagement.Models.Model;
using System.Linq;
using System.Linq.Expressions;
using static BookManagement.Constant.Enumerations;
using X.PagedList;

namespace BookManagement.Service
{
    public class BookService : BaseService<Book>, IBookService
    {
        private readonly IMapper _mapper;
        private readonly IBaseService<Category> _cateService;
        private readonly IBaseService<BookReview> _reviewService;

        public BookService(IGenericRepository<Book> baseRepo,
            ILogger<Book> logger,
            IBaseService<Category> cateService,
            IBaseService<BookReview> reviewService,
            IMapper mapper) : base(baseRepo, logger)
        {
            _mapper = mapper;
            _cateService = cateService;
            _reviewService = reviewService;
        }

        public List<Book> GetBookActiveInCategoryActive(Expression<Func<Book, bool>> expresstion)
        {
            var cateActive = _cateService.GetDbSet().Where(x => x.IsActive).ToList();

            var bookActive = from b in _baseRepo.GetDbSet().Where(expresstion).ToList()
                             join c in cateActive
                             on b.CategoryId equals c.Id
                             where b.IsActive && b.Quantity > 0
                             select b;

            return  bookActive.ToList();
        }

        public async Task<PagingModel<ApproveReviewModel>> GetPagingReviewBook(ApproveStatus status, int? pageIndex)
        {
            var reviews = await _reviewService.GetList(x => x.Status == status);
            var reviewModel = _mapper.Map<List<ApproveReviewModel>>(reviews);

            foreach (var rv in reviewModel)
            {
                var book = await _baseRepo.GetById(rv.BookId);

                rv.BookName = book.BookName;
                rv.BookImage = book.BookImage;
            }

            var pagingResult = new PagingModel<ApproveReviewModel>()
            {
                TotalRecord = reviews.Count(),
                DataPaging = reviewModel.OrderByDescending(x => x.CreatedDate).ToPagedList(pageIndex ?? 1, 10),
            };

            return pagingResult;
        }
    }
}
