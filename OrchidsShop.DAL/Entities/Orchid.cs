using System;
using System.Collections.Generic;

namespace OrchidsShop.DAL.Entities;

public partial class Orchid
{
    public Guid Id { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public string? Url { get; set; }

    public decimal Price { get; set; }

    public bool IsNatural { get; set; }

    public Guid CategoryId { get; set; }

    public virtual Category Category { get; set; } = null!;

    public virtual ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();
}
