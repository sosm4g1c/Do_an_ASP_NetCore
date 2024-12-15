using BookManagement.Models.Entity;
using System.Linq.Expressions;

namespace BookManagement.Data
{
    public class GenericRepository<T> : IGenericRepository<T> where T : BaseEntity
    {
        protected readonly DbSet<T> _dbSet;
        protected readonly IDbContext _dbContext;

        public GenericRepository(IDbContext context)
        {
            _dbContext = context;
            _dbSet = context.Set<T>();
        }

        public void Add(T entity)
        {
            entity.Id = 0;
            entity.CreatedDate = DateTime.Now;
            entity.UpdatedDate = DateTime.Now;
            _dbSet.Add(entity);
        }

        public void Update(T entity)
        {
            entity.UpdatedDate = DateTime.Now;
            _dbSet.Update(entity);
        }

        public void AddRange(List<T> entity)
        {
            if (entity != null && entity.Any())
            {
                entity = entity.Select(x => { x.Id = 0; x.CreatedDate = DateTime.Now; x.UpdatedDate = DateTime.Now; return x; }).ToList();
            }

            _dbSet.AddRange(entity);
        }

        public void UpdateRange(List<T> entity)
        {
            if (entity != null && entity.Any())
            {
                entity = entity.Select(x => { x.UpdatedDate = DateTime.Now; return x; }).ToList();
            }
            _dbSet.UpdateRange(entity);
        }

        public async Task Delete(T entity)
        {
            _dbSet.Attach(entity);
            await Task.FromResult(_dbSet.Remove(entity).Entity);
        }

        public async Task DeleteRange(List<T> entity)
        {
            _dbSet.RemoveRange(entity);
        }

        public async Task<T> Get(Expression<Func<T, bool>> expresstion)
        {
            var entity = await _dbSet.FirstOrDefaultAsync(expresstion);

            if (entity != null)
            {
                _dbContext.Entry(entity).State = EntityState.Detached;
            }

            return entity;
        }

        public async Task<List<T>> GetList(Expression<Func<T, bool>> expresstion)
        {
            var lisT = await _dbSet.Where(expresstion).ToListAsync();
            return lisT;
        }

        public async Task<int> Count(Expression<Func<T, bool>> expresstion)
        {
            return await _dbSet.CountAsync(expresstion);
        }

        public async Task<bool> Exist(Expression<Func<T, bool>> expresstion)
        {
            return await _dbSet.AnyAsync(expresstion);
        }

        //public async Task<List<T>> Paging(Expression<Func<T, bool>> expresstion, int pageSize, int pageIndex)
        //{
        //    var lisT = await _dbSet.AsNoTracking().Where(expresstion).Skip((pageIndex - 1) * pageSize).Take(pageSize).ToListAsync();
        //    return lisT;
        //}

        //public async Task<PagingMetaData> PagingTotal(Expression<Func<T, bool>> expresstion, int pageSize)
        //{
        //    var pagingTotal = new PagingMetaData();

        //    if (pageSize == 0)
        //    {
        //        pageSize = PageConfig.DefaultPageSize;
        //    }

        //    pagingTotal.Total = (await _dbSet.Where(expresstion).ToListAsync()).Count();
        //    pagingTotal.TotalPage = pagingTotal.Total % pageSize == 0 ? (pagingTotal.Total / pageSize) : (pagingTotal.Total / pageSize) + 1;

        //    return pagingTotal;
        //}

        public async Task<T> GetById(int id)
        {
            var entity = await _dbSet.FindAsync(id);

            if (entity != null)
            {
                _dbContext.Entry(entity).State = EntityState.Detached;
            }

            return entity;
        }

        public IQueryable<T> GetDbSet()
        {
            return _dbContext.Set<T>() as IQueryable<T>;
        }

        //public async Task<T> Get(Expression<Func<T, bool>> expresstion, string orderBy)
        //{
        //    var entity = await _dbSet.Where(expresstion).ApplySort(orderBy).FirstOrDefaultAsync();

        //    if (entity != null)
        //    {
        //        _dbContext.Entry(entity).State = EntityState.Detached;
        //    }

        //    return entity;
        //}

        //public virtual async Task<PagingDataModel<TEntity>> GetPagingAsync<TEntity>(
        //    PaginationParam paginationParam,
        //    IConfigurationProvider mapperConfiguration,
        //    params Expression<Func<T, bool>>[]? conditions
        //) where TEntity : class
        //{
        //    var query = _dbContext.Set<T>() as IQueryable<T>;
        //    if (conditions is { Length: > 0 })
        //    {
        //        query = conditions.Aggregate(query, (current, condition) => current.Where(condition));
        //    }

        //    int totalRecord = 0;

        //    var selectQuery = query
        //        .ProjectTo<TEntity>(mapperConfiguration)
        //        .ApplySort(orderBy: paginationParam.OrderBy);


        //    if (paginationParam.IsPaging)
        //    {
        //        totalRecord = await selectQuery.CountAsync();
        //        if (totalRecord <= 0)
        //            return new PagingDataModel<TEntity>();
        //    }

        //    var skipRows = (paginationParam.PageIndex - 1) * paginationParam.PageSize;
        //    var collection = await selectQuery
        //        .Skip(skipRows).Take(paginationParam.PageSize)
        //        .AsSplitQuery()
        //        .ToListAsync();

        //    return new PagingDataModel<TEntity>(
        //        pageSize: paginationParam.PageSize,
        //        totalCount: totalRecord,
        //        collections: collection);
        //}

        //public virtual PagingDataModel<TEntity> GetPagingWithSource<TEntity>(
        //    IEnumerable<TEntity> dataSource,
        //    PaginationParam paginationParam,
        //    params Expression<Func<TEntity, bool>>[]? conditions
        //) where TEntity : class
        //{
        //    var query = dataSource.AsQueryable();

        //    if (conditions is { Length: > 0 })
        //    {
        //        query = conditions.Aggregate(query, (current, condition) => current.Where(condition));
        //    }

        //    int totalRecord = 0;

        //    var selectQuery = query
        //        .ApplySort(orderBy: paginationParam.OrderBy);

        //    if (paginationParam.IsPaging)
        //    {
        //        totalRecord = selectQuery.Count();
        //        if (totalRecord <= 0)
        //            return new PagingDataModel<TEntity>();
        //    }

        //    var skipRows = (paginationParam.PageIndex - 1) * paginationParam.PageSize;
        //    var collection = selectQuery
        //        .Skip(skipRows).Take(paginationParam.PageSize)
        //        .AsSplitQuery()
        //        .ToList();

        //    return new PagingDataModel<TEntity>(
        //        pageSize: paginationParam.PageSize,
        //        totalCount: totalRecord,
        //        collections: collection);
        //}

        public async Task<int> SaveChangeAsync()
        {
            return await _dbContext.SaveChangesAsync();
        }
    }
}
