using System;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;

namespace OrchidsShop.DAL.Contexts;

public class Repository<T> : IRepository<T> where T : class
{
    public DbSet<T?> Entities => DbContext.Set<T>();
    
    public DbContext DbContext { get; }

    public Repository(DbContext dbContext)
    {
        DbContext = dbContext;
    }
    
    public IQueryable<T?> Get()
    {
        return Entities.AsQueryable();
    }

    public IQueryable<T> GetPagingQueryable(int pageNumber, int pageSize)
    {
        var query = Entities.AsQueryable();
        return query.Skip((pageNumber - 1) * pageSize).Take(pageSize);
    }

    /// <summary>
    /// Pagination and filtering, no count
    /// </summary>
    /// <param name="filter"></param>
    /// <param name="orderBy"></param>
    /// <param name="includeProperties"></param>
    /// <param name="pageIndex"></param>
    /// <param name="pageSize"></param>
    /// <returns></returns>
    public virtual IEnumerable<T> Get(
        Expression<Func<T, bool>> filter = null,
        Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null,
        string includeProperties = "",
        int? pageIndex = null,
        int? pageSize = null)
    {
        IQueryable<T> query = Entities;

        if (filter != null)
        {
            query = query.Where(filter);
        }

        foreach (var includeProperty in includeProperties.Split
                     (new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
        {
            query = query.Include(includeProperty);
        }

        if (orderBy != null)
        {
            query = orderBy(query);
        }

        // Implementing pagination
        if (pageIndex.HasValue && pageSize.HasValue)
        {
            // Ensure the pageIndex and pageSize are valid
            int validPageIndex = pageIndex.Value > 0 ? pageIndex.Value - 1 : 0;
            int validPageSize =
                pageSize.Value > 0
                    ? pageSize.Value
                    : 10; // Assuming a default pageSize of 10 if an invalid value is passed
            if (pageSize.Value > 0)
            {
                query = query.Skip(validPageIndex * validPageSize).Take(validPageSize);
            }
        }

        return query.ToList();
    }

    /// <summary>
    /// Pagination, filtering and count
    /// </summary>
    /// <param name="filter"></param>
    /// <param name="orderBy"></param>
    /// <param name="includeProperties"></param>
    /// <param name="pageIndex"></param>
    /// <param name="pageSize"></param>
    /// <returns></returns>
    public virtual (IEnumerable<T> Data, int Count) GetWithCount(
        Expression<Func<T, bool>> filter = null,
        Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null,
        string includeProperties = "",
        int? pageIndex = null,
        int? pageSize = null)
    {
        IQueryable<T> query = Entities;

        if (filter != null)
        {
            query = query.Where(filter);
        }

        foreach (var includeProperty in includeProperties.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
        {
            query = query.Include(includeProperty);
        }

        if (orderBy != null)
        {
            query = orderBy(query);
        }

        // Get the total count before pagination
        int count = query.Count();

        // Implementing pagination
        if (pageIndex.HasValue && pageSize.HasValue)
        {
            int validPageIndex = pageIndex.Value > 0 ? pageIndex.Value - 1 : 0;
            int validPageSize = pageSize.Value > 0 ? pageSize.Value : 10; // Default pageSize of 10 if invalid
            query = query.Skip(validPageIndex * validPageSize).Take(validPageSize);
        }

        // Returning data with count
        return (Data: query.ToList(), Count: count);
    }

    /// <summary>
    /// Conditional where clause
     /// If no predicate is provided, it returns all entities.
     /// 
     /// Use this method to filter entities based on a condition.
     /// If you want to retrieve all entities, call without parameters.
    /// </summary>
    /// <param name="predic"></param>
    /// <returns></returns>
    public IQueryable<T?> Where(Expression<Func<T?, bool>> predic = null)
    {
        return Entities.Where(predic).AsQueryable();
    }

    public void Add(T? entity)
    {
        Entities.Add(entity);
    }

    public T? SingleOrDefault(Expression<Func<T?, bool>> predicate)
    {
        return Entities.SingleOrDefault(predicate);
    }

    /// <summary>
    /// Use with primary key
     /// If no keyValues are provided, it returns null.
     /// 
     /// Use this method to find an entity by its primary key.
     /// If you want to retrieve an entity without specifying a key, call without parameters.
    /// </summary>
    /// <param name="keyValues"></param>
    /// <returns></returns>
    public T? Find(params object?[]? keyValues)
    {
        return Entities.Find(keyValues);
    }

    public void RemoveRange(List<T> entities)
    {
        Entities.RemoveRange(entities);
    }
    
    /// <summary>
    /// Adds an entity to the context. Auto save if param <paramref name="saveChanges"/> is default, set it false to save manual .
    /// </summary>
    /// <param name="entity"></param>
    /// <param name="saveChanges"></param>
    public async Task AddAsync(T? entity, bool saveChanges = true)
    {
        await Entities.AddAsync(entity);
        if (saveChanges) await DbContext.SaveChangesAsync();
    }

    /// <summary>
    /// Adds a range of entities to the context. Auto save if param <paramref name="saveChanges"/> is default, set it false to save manual.
     /// 
     /// Use this method to add multiple entities at once.
     /// If you want to add a single entity, use <see cref="AddAsync(T?, bool)"/>.
    /// </summary>
    /// <param name="entities"></param>
    /// <param name="saveChanges"></param>
    public async Task AddRangeAsync(List<T> entities, bool saveChanges = true)
    {
        await Entities.AddRangeAsync(entities);
        if (saveChanges) await DbContext.SaveChangesAsync();
    }

    public void Update(T? entity)
    {
        Entities.Update(entity);
    }

    /// <summary>
    /// Updates an entity in the context. Auto save if param <paramref name="saveChanges"/> is default, set it false to save manual.
     /// 
     /// Use this method to update an existing entity.
    /// </summary>
    /// <param name="entity"></param>
    /// <param name="saveChanges"></param>
    public async Task UpdateAsync(T? entity, bool saveChanges = true)
    {
        Entities.Update(entity);
        if (saveChanges) await DbContext.SaveChangesAsync();
    }

    /// <summary>
    /// Removes an entity from the context. Auto save if param <paramref name="saveChanges"/> is default, set it false to save manual.
     /// 
     /// Use this method to remove an entity from the context.
     /// If you want to remove multiple entities, use <see cref="RemoveRange(List{T})"/>.
    /// </summary>
    /// <param name="entity"></param>
    /// <param name="saveChanges"></param>
    public async Task RemoveAsync(T? entity, bool saveChanges = true)
    {
        Entities.Remove(entity);
        if (saveChanges) await DbContext.SaveChangesAsync();
    }

    public int Count()
    {
        return Entities.Count();
    }

    public T? FirstOrDefault()
    {
        return Entities.FirstOrDefault();
    }

    public T? LastOrDefault()
    {
        return Entities.LastOrDefault();
    }

    public T? FirstOrDefault(Expression<Func<T?, bool>> predicate)
    {
        return Entities.FirstOrDefault(predicate);
    }

    public async Task<T?> FirstOrDefaultAsync()
    {
        return await Entities.FirstOrDefaultAsync();
    }

    public async Task<T?> FirstOrDefaultAsync(Expression<Func<T?, bool>> predicate)
    {
        return await Entities.FirstOrDefaultAsync(predicate);
    }

    public async Task<T?> SingleOrDefaultAsync(Expression<Func<T?, bool>> predicate)
    {
        return await Entities.SingleOrDefaultAsync(predicate);
    }

    public int SaveChanges()
    {
        return DbContext.SaveChanges();
    }

    public async Task<int> SaveAsync()
    {
        return await DbContext.SaveChangesAsync();
    }

    public async Task<T?> FindAsync(params object?[]? keyValues)
    {
        return await Entities.FindAsync(keyValues);
    }

    public T? Find(Expression<Func<T, bool>> expression)
    {
        return Entities.Find(expression);
    }
}
