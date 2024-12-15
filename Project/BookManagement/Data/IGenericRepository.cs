using System.Linq.Expressions;

namespace BookManagement.Data
{
    public interface IGenericRepository<T>
    {
        void Add(T entity);
        void Update(T entity);
        void AddRange(List<T> entity);
        void UpdateRange(List<T> entity);
        Task Delete(T entity);
        Task DeleteRange(List<T> entity);
        Task<T> Get(Expression<Func<T, bool>> expresstion);
        Task<List<T>> GetList(Expression<Func<T, bool>> expresstion);
        //Task<List<T>> Paging(Expression<Func<T, bool>> expresstion, int pageSize, int pageIndex);
        //Task<PagingMetaData> PagingTotal(Expression<Func<T, bool>> expresstion, int pageSize);
        Task<T> GetById(int id);
        IQueryable<T> GetDbSet();
        Task<int> Count(Expression<Func<T, bool>> expresstion);
        Task<bool> Exist(Expression<Func<T, bool>> expresstion);
        //Task<T> Get(Expression<Func<T, bool>> expresstion, string orderBy);
        //Task<PagingDataModel<TEntity>> GetPagingAsync<TEntity>(
        //    PaginationParam paginationParam,
        //    IConfigurationProvider mapperConfiguration,
        //    params Expression<Func<T, bool>>[]? conditions
        //) where TEntity : class;

        //PagingDataModel<TEntity> GetPagingWithSource<TEntity>(
        //    IEnumerable<TEntity> dataSource,
        //    PaginationParam paginationParam,
        //    params Expression<Func<TEntity, bool>>[]? conditions
        //) where TEntity : class;
        Task<int> SaveChangeAsync();
    }
}
