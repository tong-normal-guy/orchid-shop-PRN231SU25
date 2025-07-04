using System;
using Microsoft.EntityFrameworkCore;
using OrchidsShop.DAL.Repos;

namespace OrchidsShop.DAL.Contexts;

public interface IUnitOfWork : IDisposable
{
    DbContext DbContext { get; }
    IRepository<T> Repository<T>() where T : class;
    
    /// Other features of the repository can be added here.
    #region Repositories

    IOrchidRepository OrchidRepository { get; }

    #endregion
    
    void SaveChanges();
    int SaveManualChanges();
    Task SaveChangesAsync();
    Task<int> SaveManualChangesAsync();
}
