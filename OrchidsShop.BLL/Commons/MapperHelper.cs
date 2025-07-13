using AutoMapper;
using OrchidsShop.BLL.DTOs.Categories.Responses;
using OrchidsShop.BLL.DTOs.Orchids.Responses;
using OrchidsShop.DAL.Entities;

namespace OrchidsShop.BLL.Commons;

public class MapperHelper : Profile
{
    public MapperHelper()
    {
        CreateMap<Orchid, QueryOrchidResponse>();

        CreateMap<Category, QueryCategoryResponse>();
    }
}