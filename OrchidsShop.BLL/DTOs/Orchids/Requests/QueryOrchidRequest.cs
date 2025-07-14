using System.Linq.Expressions;
using LinqKit;
using OrchidsShop.BLL.Commons.Paginations;
using OrchidsShop.DAL.Entities;

namespace OrchidsShop.BLL.DTOs.Orchids.Requests;

public class QueryOrchidRequest : PaginationRequest<Orchid>
{
    public string? Search { get; set; }
    public bool? IsNarutal { get; set; }
    public List<string>? Categories { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public List<string>? Ids { get; set; }
    
    public override Expression<Func<Orchid, bool>> GetExpressions()
    {
        var predicate = PredicateBuilder.New<Orchid>(true);

        // Search filter
        if (!string.IsNullOrWhiteSpace(Search))
        {
            predicate = predicate.And(x => x.Name.Contains(Search) || 
                                          (x.Description != null && x.Description.Contains(Search)));
        }

        // Natural filter
        if (IsNarutal.HasValue)
        {
            predicate = predicate.And(x => x.IsNatural == IsNarutal.Value);
        }

        // Categories filter
        if (Categories != null && Categories.Any())
        {
            predicate = predicate.And(x => Categories.Contains(x.CategoryId.ToString()));
        }

        // Price range filter
        if (MinPrice.HasValue)
        {
            predicate = predicate.And(x => x.Price >= MinPrice.Value);
        }

        if (MaxPrice.HasValue)
        {
            predicate = predicate.And(x => x.Price <= MaxPrice.Value);
        }

        // IDs filter
        if (Ids != null && Ids.Any())
        {
            predicate = predicate.And(x => Ids.Contains(x.Id.ToString()));
        }

        return predicate;
    }
    
    public Expression<Func<Orchid, bool>> GetMinMaxPriceExpression()
    {
        if (MinPrice.HasValue && MaxPrice.HasValue)
        {
            return orchid => orchid.Price >= MinPrice.Value && orchid.Price <= MaxPrice.Value;
        }
        else if (MinPrice.HasValue)
        {
            return orchid => orchid.Price >= MinPrice.Value;
        }
        else if (MaxPrice.HasValue)
        {
            return orchid => orchid.Price <= MaxPrice.Value;
        }
        return _ => true; // No price filter
    }
}