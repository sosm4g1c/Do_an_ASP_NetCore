namespace BookManagement.Models.Entity
{
    /// <summary>
    /// Danh mục
    /// </summary>
    public class Category : BaseEntity
    {
        /// <summary>
        /// Mã danh mục
        /// </summary>
        public string CategoryCode { get; set; }
        /// <summary>
        /// Tên danh mục
        /// </summary>
        public string CategoryName { get; set; }
        /// <summary>
        /// Mô tả
        /// </summary>
        public string Description { get; set; }
        /// <summary>
        /// Trạng thái
        /// </summary>
        public bool IsActive { get; set; }
    }
}
