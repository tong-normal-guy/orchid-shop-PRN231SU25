using AutoMapper;
using OrchidsShop.BLL.DTOs.Categories.Requests;
using OrchidsShop.BLL.DTOs.Categories.Responses;
using OrchidsShop.BLL.DTOs.Orchids.Requests;
using OrchidsShop.BLL.DTOs.Orchids.Responses;
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
    }
}