using BookManagement.Data;
using BookManagement.Models.Entity;
using System.Linq.Expressions;

namespace BookManagement.Service
{
    public class BaseService<T> : IBaseService<T> where T : BaseEntity
    {
        protected readonly IGenericRepository<T> _baseRepo;
        private readonly ILogger<T> _logger;
        public BaseService(IGenericRepository<T> baseRepo, ILogger<T> logger)
        {
            _baseRepo = baseRepo;
            _logger = logger;
        }

        /// <summary>
        /// Lấy toàn bộ dữ liệu
        /// </summary>
        /// <returns></returns>
        public virtual List<T> GetAll()
        {
            var entities = _baseRepo.GetDbSet().ToList();

            return entities.ToList();
        }
        
        /// <summary>
        /// Lấy toàn bộ dữ liệu
        /// </summary>
        /// <returns></returns>
        public virtual IQueryable<T> GetDbSet()
        {
            var entities = _baseRepo.GetDbSet();

            return entities;
        }

        /// <summary>
        /// Lấy ra thông tin bản ghi theo khóa chính
        /// </summary>
        /// <param name="id">Khóa chính</param>
        /// <returns></returns>
        public async virtual Task<T> GetEntityById(int id)
        {
            return await _baseRepo.GetById(id);
        }

        /// <summary>
        /// Lấy ra thông tin bản ghi
        /// </summary>
        /// <returns></returns>
        public async virtual Task<T> Get(Expression<Func<T, bool>> expresstion)
        {
            return await _baseRepo.Get(expresstion);
        }

        /// <summary>
        /// Lấy ra thông tin bản ghi
        /// </summary>
        /// <returns></returns>
        public async virtual Task<bool> Exist(Expression<Func<T, bool>> expresstion)
        {
            return await _baseRepo.Exist(expresstion);
        }

        /// <summary>
        /// Lấy ra thông tin bản ghi
        /// </summary>
        /// <returns></returns>
        public async virtual Task<int> Count(Expression<Func<T, bool>> expresstion)
        {
            return await _baseRepo.Count(expresstion);
        }

        /// <summary>
        /// Lấy ra thông tin 
        /// </summary>
        /// <returns></returns>
        public async virtual Task<List<T>> GetList(Expression<Func<T, bool>> expresstion)
        {
            return await _baseRepo.GetList(expresstion);
        }

        /// <summary>
        /// Thêm mới dữ liệu
        /// </summary>
        /// <param name="entity">Param đầu vào</param>
        /// <returns></returns>
        public async virtual Task Insert(T entity)
        {
            await this.PrepareBeforeInsert(entity);

            _baseRepo.Add(entity);
            await _baseRepo.SaveChangeAsync();

            await this.AfterSaveInsertEntity(entity);
        }

        /// <summary>
        /// Thêm mới list dữ liệu
        /// </summary>
        /// <param name="entities">Param đầu vào</param>
        /// <returns></returns>
        public async virtual Task InsertMulti(List<T> entities)
        {
            if (entities.Any())
            {
                foreach (T entity in entities)
                {
                    await this.PrepareBeforeInsert(entity);
                }

                _baseRepo.AddRange(entities);
                await _baseRepo.SaveChangeAsync();

                foreach (T entity in entities)
                {
                    await this.AfterSaveInsertEntity(entity);
                }
            }
        }

        /// <summary>
        /// Cập nhật dữ liệu
        /// </summary>
        /// <param name="entity">Param truyền vào</param>
        /// <returns></returns>
        public async virtual Task Update(T entity)
        {
            this.PrepareBeforeUpdate(entity);

            _baseRepo.Update(entity);
            await _baseRepo.SaveChangeAsync();

            await this.AfterSaveUpdateEntity(entity);
        }

        /// <summary>
        /// Xóa 1 bản ghi theo id
        /// </summary>
        /// <param name="id">ID bản ghi</param>
        /// <returns></returns>
        public async virtual Task Delete(int id)
        {
            var entity = await _baseRepo.GetById(id);
            if (entity != null)
            {
                await _baseRepo.Delete(entity);
                await _baseRepo.SaveChangeAsync();

                await this.AfterSaveDeleteEntity(entity);
            }
        }

        /// <summary>
        /// Chuẩn bị dữ liệu trước khi insert
        /// </summary>
        /// <param name="entity"></param>
        public async virtual Task PrepareBeforeInsert(T entity)
        {
            entity.Id = 0;
            entity.CreatedDate = DateTime.Now;
        }

        /// <summary>
        /// Chuẩn bị dữ liệu trước khi update
        /// </summary>
        /// <param name="entity"></param>
        public virtual void PrepareBeforeUpdate(T entity)
        {
            entity.UpdatedDate = DateTime.Now;
        }

        /// <summary>
        /// Xử lý sau khi insert dữ liệu (nếu có)
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public virtual async Task AfterSaveInsertEntity(T entity)
        {

        }

        /// <summary>
        /// Xử lý sau khi update dữ liệu (nếu có)
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public virtual async Task AfterSaveUpdateEntity(T entity)
        {

        }

        /// <summary>
        /// Xử lý sau khi delete dữ liệu (nếu có)
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public virtual async Task AfterSaveDeleteEntity(T entity)
        {

        }
    }
}
