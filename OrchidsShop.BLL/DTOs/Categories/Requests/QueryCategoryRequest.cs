using System.Linq.Expressions;
using LinqKit;
using OrchidsShop.BLL.Commons.Paginations;
using OrchidsShop.DAL.Entities;

namespace OrchidsShop.BLL.DTOs.Categories.Requests;

public class QueryCategoryRequest : PaginationRequest<Category>
{
    public string? Search { get; set; }
    public List<string>? Ids { get; set; }
    
    public override Expression<Func<Category, bool>> GetExpressions()
    {
        var predicate = PredicateBuilder.New<Category>(true);

        if (!string.IsNullOrEmpty(Search))
        {
            predicate = predicate.And(c => c.Name.Contains(Search));
        }

        if (Ids != null && Ids.Any())
        {
            predicate = predicate.And(c => Ids.Contains(c.Id.ToString()));
        }

        return predicate;
    }
}
