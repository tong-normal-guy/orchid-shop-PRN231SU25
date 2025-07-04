using System;
using System.Collections.Generic;

namespace OrchidsShop.DAL.Entities;

public partial class Order
{
    public Guid Id { get; set; }

    public decimal TotalAmound { get; set; }

    public DateOnly OrderDate { get; set; }

    public Guid AccountId { get; set; }

    public string Status { get; set; } = null!;

    public virtual Account Account { get; set; } = null!;

    public virtual ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();
}
