using AutoMapper;
using OrchidsShop.BLL.DTOs.Accounts.Requests;
using OrchidsShop.BLL.DTOs.Accounts.Responses;
using OrchidsShop.BLL.DTOs.Categories.Requests;
using OrchidsShop.BLL.DTOs.Categories.Responses;
using OrchidsShop.BLL.DTOs.Orchids.Requests;
using OrchidsShop.BLL.DTOs.Orchids.Responses;
using OrchidsShop.BLL.DTOs.Orders.Requests;
using OrchidsShop.BLL.DTOs.Orders.Responses;
using OrchidsShop.DAL.Entities;

namespace OrchidsShop.BLL.Commons;

public class MapperHelper : Profile
{
    public MapperHelper()
    {
        CreateMap<Orchid, QueryOrchidResponse>();
        CreateMap<CommandOrchidRequest, Orchid>();
        
        // Category mappings
        CreateMap<Category, QueryCategoryResponse>();
        CreateMap<CommandCategoryRequest, Category>();
        
        // Order mappings
        CreateMap<Order, QueryOrderResponse>()
            .ForMember(dest => dest.TotalAmount, opt => opt.MapFrom(src => src.TotalAmount));
        CreateMap<CommandOrderRequest, Order>();
        
        // OrderDetail mappings
        CreateMap<OrderDetail, QueryOrderDetailResponse>();
        CreateMap<CommandOrderDetailRequest, OrderDetail>();
        
        // Account mappings
        CreateMap<Account, QueryAccountResponse>()
            .ForMember(dest => dest.Role, opt => opt.MapFrom(src => src.Role.Name));
        CreateMap<CommandAccountRequest, Account>();

        CreateMap<Role, QueryRoleResponse>();
    }
}