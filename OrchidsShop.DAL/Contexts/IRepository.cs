using System;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;

namespace OrchidsShop.DAL.Contexts;

public interface IRepository<T> where T : class
{
    public IQueryable<T?> Get();
    
    /// <summary>
    /// Pagination and filtering, no count
    /// </summary>
    /// <param name="filter"></param>
    /// <param name="orderBy"></param>
    /// <param name="includeProperties"></param>
    /// <param name="pageIndex"></param>
    /// <param name="pageSize"></param>
    /// <returns></returns>
    public IEnumerable<T> Get(
        Expression<Func<T, bool>> filter = null,
        Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null,
        string includeProperties = "",
        int? pageIndex = null,
        int? pageSize = null);

    /// <summary>
    /// Pagination, filtering and count
    /// </summary>
    /// <param name="filter"></param>
    /// <param name="orderBy"></param>
    /// <param name="includeProperties"></param>
    /// <param name="pageIndex"></param>
    /// <param name="pageSize"></param>
    /// <returns></returns>
    public (IEnumerable<T> Data, int Count) GetWithCount(
        Expression<Func<T, bool>> filter = null,
        Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null,
        string includeProperties = "",
        int? pageIndex = null,
        int? pageSize = null);

    public IQueryable<T?> Where(Expression<Func<T?, bool>> predic = null);
    public void Add(T? entity);
    public void Update(T? entity);
    public int Count();
    public int SaveChanges();
    public T? FirstOrDefault(Expression<Func<T?, bool>> predicate = null);
    public T? SingleOrDefault(Expression<Func<T?, bool>> predicate = null);
    
    /// <summary>
    /// Use with primary key
    /// If no keyValues are provided, it returns null.
    /// 
    /// Use this method to find an entity by its primary key.
    /// If you want to retrieve an entity without specifying a key, call without parameters.
    /// </summary>
    /// <param name="keyValues"></param>
    /// <returns></returns>
    public T? Find(params object?[]? keyValues);

    /// <summary>
    /// Adds an entity to the context. Auto save if param <paramref name="saveChanges"/> is default, set it false to save manual .
    /// </summary>
    /// <param name="entity"></param>
    /// <param name="saveChanges"></param>
    public Task AddAsync(T? entity, bool saveChanges = true);
    
    /// <summary>
    /// Adds a range of entities to the context. Auto save if param <paramref name="saveChanges"/> is default, set it false to save manual.
    /// 
    /// Use this method to add multiple entities at once.
    /// If you want to add a single entity, use <see cref="AddAsync(T?, bool)"/>.
    /// </summary>
    /// <param name="entities"></param>
    /// <param name="saveChanges"></param>
    public Task AddRangeAsync(List<T> entities, bool saveChanges = true);
    
    /// <summary>
    /// Updates an entity in the context. Auto save if param <paramref name="saveChanges"/> is default, set it false to save manual.
    /// 
    /// Use this method to update an existing entity.
    /// </summary>
    /// <param name="entity"></param>
    /// <param name="saveChanges"></param>
    public Task UpdateAsync(T? entity, bool saveChanges = true);
    
    /// <summary>
    /// Removes an entity from the context. Auto save if param <paramref name="saveChanges"/> is default, set it false to save manual.
    /// 
    /// Use this method to remove an entity from the context.
    /// If you want to remove multiple entities, use <see cref="RemoveRange(List{T})"/>.
    /// </summary>
    /// <param name="entity"></param>
    /// <param name="saveChanges"></param>
    public Task RemoveAsync(T? entity, bool saveChanges = true);
    public Task<T?> FirstOrDefaultAsync(Expression<Func<T?, bool>> predicate = null);
    
    public Task<T?> SingleOrDefaultAsync(Expression<Func<T?, bool>> predicate = null);
    
    /// <summary>
    /// use when know primary key
    /// </summary>
    /// <param name="keyValues">id</param>
    /// <returns>entity?</returns>
    public Task<T?> FindAsync(params object?[]? keyValues);
    public Task<int> SaveAsync();
}
