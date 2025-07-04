using System;
using System.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using OrchidsShop.DAL.Repos;

namespace OrchidsShop.DAL.Contexts;

public class UnitOfWork : IUnitOfWork, IDisposable
{
    public UnitOfWork(DbContext context)
    {
        _context = context;
        _repositories = new Dictionary<string, object>();
    }

    private Dictionary<string, object> _repositories { get; }
    private readonly DbContext _context;
    private bool _disposed;

    public DbContext DbContext => _context;

    public IRepository<T> Repository<T>() where T : class
    {
        var type = typeof(T);
        var typeName = type.Name;

        lock (_repositories)
        {
            if (_repositories.ContainsKey(typeName))
            {
                return (IRepository<T>)_repositories[typeName];
            }

            var repository = new Repository<T>(_context);

            _repositories.Add(typeName, repository);
            return repository;
        }
    }

    #region Repositories
    
    private IOrchidRepository? _orchidRepository;
    public IOrchidRepository OrchidRepository 
        => _orchidRepository ??= new OrchidRepository(_context);

    #endregion
    
    public void SaveChanges()
    {
        _context.SaveChangesAsync();
    }

    public int SaveManualChanges()
    {
        return _context.SaveChanges();
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }

    public async Task<int> SaveManualChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }

    public void Dispose()
    {
        Dispose();
        GC.SuppressFinalize(this);
    }
}
