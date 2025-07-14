using System.Linq.Expressions;
using LinqKit;
using Microsoft.IdentityModel.Tokens;
using OrchidsShop.BLL.Commons.Paginations;
using OrchidsShop.DAL.Entities;

namespace OrchidsShop.BLL.DTOs.Orchids.Requests;

public class QueryOrchidRequest : PaginationRequest<Orchid>
{
    public string? Search { get; set; }
    public bool? IsNarutal { get; set; }
    
    /// <summary>
    /// A list of category names is separate by ","
    /// </summary>
    public string? Categories { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    
    /// <summary>
    /// A list of ids is separate by ","
    /// </summary>
    public string? Ids { get; set; }
    
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
        if (!Categories.IsNullOrEmpty())
        {
            var categoryNames = Categories.Split(',').ToList();
            predicate = predicate.And(x => categoryNames.Contains(x.Category.Name));
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
        if (!Ids.IsNullOrEmpty())
        {
            var orchidIds = Ids.Split(',').Select(Guid.Parse).ToList();
            predicate = predicate.And(x => orchidIds.Contains(x.Id));
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