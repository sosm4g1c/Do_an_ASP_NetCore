using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace BookManagement.Models.Entity
{
    /// <summary>
    /// Sách
    /// </summary>
    public class Book : BaseEntity
    {
        /// <summary>
        /// Mã sách
        /// </summary>
        [Required(AllowEmptyStrings = false, ErrorMessage = "Mã sách không được để trống")]
        [MaxLength(12, ErrorMessage = "Mã sách chứa tối đa 12 ký tự")]
        public string BookCode { get; set; }
        /// <summary>
        /// Tên sách
        /// </summary>
        [Required(AllowEmptyStrings = false, ErrorMessage = "Tiêu đề không được để trống")]
        [MaxLength(500, ErrorMessage = "Tiêu đề chứa tối đa 500 ký tự")]
        public string BookName { get; set; }
        /// <summary>
        /// Mã danh mục 
        /// </summary>
        [Required(ErrorMessage = "Vui lòng chọn danh mục")]
        public int CategoryId { get; set; }
        /// <summary>
        /// Tên danh mục
        /// </summary>
        public string CategoryName { get; set; }
        /// <summary>
        /// Tác giả
        /// </summary>
        [AllowNull]
        [MaxLength(255, ErrorMessage = "Tác giả chứa tối đa 255 ký tự")]
        public string? Author { get; set; }
        /// <summary>
        /// Nhà xuất bản
        /// </summary>
        [AllowNull]
        [MaxLength(255, ErrorMessage = "Nhà xuất bản chứa tối đa 255 ký tự")]
        public string? Publisher { get; set; }
        /// <summary>
        /// Năm xuất bản
        /// </summary>
        [AllowNull]
        public string? PublishTime { get; set; }
        /// <summary>
        /// Trọng lượng
        /// </summary>
        [AllowNull]
        [Range(1, 10000, ErrorMessage = "Khối lượng không hợp lệ (1-10.000)")]
        public double? BookWeight { get; set; }
        /// <summary>
        /// Kích thước
        /// </summary>
        [AllowNull]
        [MaxLength(255, ErrorMessage = "Kích thước chứa tối đa 255 ký tự")]
        public string? BookSize { get; set; }
        /// <summary>
        /// Số trang
        /// </summary>
        [AllowNull]
        [Range(1, 10000, ErrorMessage = "Số trang không hợp lệ (1-10.000)")]
        public int? BookPage { get; set; }
        /// <summary>
        /// Số lượng hiện tại
        /// </summary>
        [Required(ErrorMessage = "Số lượng không được để trống")]
        [Range(1, 1000, ErrorMessage = "Số lượng không hợp lệ (1-1.000)")]
        public int Quantity { get; set; }
        /// <summary>
        /// Số lượng Đã bán
        /// </summary>
        public int SoldQuantity { get; set; } = 0;
        /// <summary>
        /// Giá bán
        /// </summary>
        [Required(ErrorMessage = "Giá bán không được để trống")]
        [Range(1, 1000000000, ErrorMessage = "Giá bán không hợp lệ (1-1.000.000.000)")]
        public int Price { get; set; }
        /// <summary>
        /// Giá Khuyến mại
        /// </summary>
        [AllowNull]
        [Range(1, 1000000000, ErrorMessage = "Giá bán không hợp lệ (1-1.000.000.000)")]
        public int? PriceDiscount { get; set; }
        /// <summary>
        /// Mô tả
        /// </summary>
        [AllowNull]
        public string? Description { get; set; }
        /// <summary>
        /// Ảnh bìa sách
        /// </summary>
        [Required(ErrorMessage = "Vui lòng chọn ảnh bìa sách")]
        public string BookImage { get; set; }
        /// <summary>
        /// Các ảnh thêm
        /// </summary>
        [Required(ErrorMessage = "Vui lòng chọn thêm ảnh mô tả")]
        public string InfoImage { get; set; }
        /// <summary>
        /// Trạng thái
        /// </summary>
        public bool IsActive { get; set; } = true;
    }
}
