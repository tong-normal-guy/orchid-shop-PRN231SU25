using System.Linq.Expressions;
using LinqKit;
using Microsoft.IdentityModel.Tokens;
using OrchidsShop.BLL.Commons.Paginations;
using OrchidsShop.DAL.Entities;

namespace OrchidsShop.BLL.DTOs.Orders.Requests;

public class QueryOrderRequest : PaginationRequest<Order>
{
    /// <summary>
    /// A list of ids is separate by ","
    /// </summary>
    public string? Ids { get; set; }
    
    /// <summary>
    /// A list of statuses is separate by ","
    /// </summary>
    public string? Statuses { get; set; }

    public Guid? AccountId { get; set; }

    public DateOnly? FromDate { get; set; }

    public DateOnly? ToDate { get; set; }
    
    /// <summary>
    /// Use to determine is order manager page of admin or not
    /// </summary>
    public bool? IsManagment { get; set; }
    
    public override Expression<Func<Order, bool>> GetExpressions()
    {
        var predicate = PredicateBuilder.New<Order>(true);

        // IDs filter
        if (!Ids.IsNullOrEmpty())
        {
            var orderIds = Ids.Split(',').Select(Guid.Parse).ToList();
            predicate = predicate.And(x => orderIds.Contains(x.Id));
        }

        // Statuses filter
        if (!Statuses.IsNullOrEmpty())
        {
            var statuses = Statuses.Split(',').ToList();
            predicate = predicate.And(x => statuses.Contains(x.Status));
        }

        // Account ID filter
        if (AccountId.HasValue)
        {
            predicate = predicate.And(x => x.AccountId == AccountId.Value);
        }

        // From date filter
        if (FromDate.HasValue && ToDate.HasValue)
        {
            predicate = predicate.And(x => x.OrderDate >= FromDate.Value && x.OrderDate <= ToDate.Value);
        }

        return predicate;
    }
}