using System.ComponentModel.DataAnnotations;
using System.Linq.Dynamic.Core;
using System.Linq.Expressions;
using LinqKit;
using OrchidsShop.BLL.Commons.Constants;

namespace OrchidsShop.BLL.Commons.Paginations;

public abstract class PaginationRequest<T> where T : class
{
    private int _pageNumber = PaginationConstant.DefaultPageNumber;

    private int _pageSize = PaginationConstant.DefaultPageSize;
    
    /// <summary>
    /// Page number for pagination (1-based)
    /// </summary>
    [Display(Name = "PageNumber", Description = "Page number for pagination (1-based)")]
    public int PageNumber
    {
        get => _pageNumber;
        set => _pageNumber = value > 0
            ? value
            : PaginationConstant.DefaultPageNumber;
    }

    /// <summary>
    /// Number of items per page
    /// </summary>
    [Display(Name = "PageSize", Description = "Number of items per page")]
    public int PageSize
    {
        get => _pageSize;
        set => _pageSize = value > 0 && value <= PaginationConstant.MaxPageSize
            ? value
            : PaginationConstant.DefaultPageSize;
    }

    /// <summary>
    /// Column to sort by (default: CreatedAt)
    /// </summary>
    [Display(Name = "SortColumn", Description = "Column to sort by (default: CreatedAt)")]
    public string? SortColumn { get; set; } = "CreatedAt";

    /// <summary>
    /// Sort direction (Asc or Desc, default: Desc)
    /// </summary>
    [Display(Name = "SortDir", Description = "Sort direction (Asc or Desc, default: Desc)")]
    public SortDirection? SortDir { get; set; } = SortDirection.Desc;

    protected Expression<Func<T, bool>> Expression = PredicateBuilder.New<T>(true);

    public abstract Expression<Func<T, bool>> GetExpressions();

    public Func<IQueryable<T>, IOrderedQueryable<T>>? GetOrder()
    {
        if (string.IsNullOrWhiteSpace(SortColumn)) return null;

        return query => query.OrderBy($"{SortColumn} {SortDir.ToString().ToLower()}");
    }

    public string? GetDynamicOrder()
    {
        if (string.IsNullOrWhiteSpace(SortColumn)) return null;

        return $"{SortColumn} {SortDir.ToString().ToLower()}";
    }
}