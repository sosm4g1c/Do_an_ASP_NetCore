using AutoMapper;
using BookManagement.Models.Entity;
using BookManagement.Models.Model;

namespace BookManagement.Data
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<User, RegisterModel>();
            CreateMap<RegisterModel, User>();
            CreateMap<CategoryModel, Category>();
            CreateMap<Category, CategoryModel>();
            CreateMap<Order, OrderViewModel>();
            CreateMap<OrderViewModel, Order>();
            CreateMap<UserInfomationModel, User>();
            CreateMap<User, UserInfomationModel>();
            CreateMap<ApproveReviewModel, BookReview>();
            CreateMap<BookReview, ApproveReviewModel>();
        }
    }
}
