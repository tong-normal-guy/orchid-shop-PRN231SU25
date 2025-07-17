using System.Linq.Expressions;
using LinqKit;
using OrchidsShop.BLL.Commons.Paginations;
using OrchidsShop.DAL.Entities;

namespace OrchidsShop.BLL.DTOs.Accounts.Requests;

public class QueryAccountRequest : PaginationRequest<Account>
{
    public string? Search { get; set; }
    public string? Roles { get; set; }

    /// <summary>
    /// This is a comma-separated list of account IDs, used to filter accounts by their IDs
    /// </summary>
    public string? Ids { get; set; }
    public override Expression<Func<Account, bool>> GetExpressions()
    {
        var predicate = PredicateBuilder.New<Account>(true);
        if (!string.IsNullOrEmpty(Search))
        {
            predicate = predicate.And(x => x.Name.Contains(Search));
        }
        if (!string.IsNullOrEmpty(Roles))
        {
            predicate = predicate.And(x => x.Role.Name.Contains(Roles));
        }
        if (!string.IsNullOrEmpty(Ids))
        {
            var ids = Ids.Split(',').Select(Guid.Parse).ToList();
            predicate = predicate.And(x => ids.Contains(x.Id));
        }
        return predicate;
    }
}