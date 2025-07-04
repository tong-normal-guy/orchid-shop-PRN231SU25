using System;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using OrchidsShop.DAL.Contexts;
using OrchidsShop.DAL.Entities;

namespace OrchidsShop.DAL.Repos;

public class OrchidRepository : Repository<Orchid>, IOrchidRepository
{
    public OrchidRepository(DbContext dbContext) : base(dbContext)
    {
    }

    
}
