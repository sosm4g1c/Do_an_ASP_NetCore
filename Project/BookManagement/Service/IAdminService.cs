using BookManagement.Models.Entity;
using BookManagement.Models.Model;
using static BookManagement.Constant.Enumerations;

namespace BookManagement.Service
{
    public interface IAdminService 
    {
        Task<DashboardModel> GetDashboardOverview(int? viewType);
        object UploadImage(UploadModel upload);
    }
}
