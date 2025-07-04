using System;
using System.Collections.Generic;

namespace OrchidsShop.DAL.Entities;

public partial class Category
{
    public Guid Id { get; set; }

    public string Name { get; set; } = null!;

    public virtual ICollection<Orchid> Orchids { get; set; } = new List<Orchid>();
}
