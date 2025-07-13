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