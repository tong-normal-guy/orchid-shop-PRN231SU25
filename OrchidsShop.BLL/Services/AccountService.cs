using System;
using AutoMapper;
using OrchidsShop.DAL.Contexts;

namespace OrchidsShop.BLL.Services;

public class AccountService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public AccountService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }
    
}
