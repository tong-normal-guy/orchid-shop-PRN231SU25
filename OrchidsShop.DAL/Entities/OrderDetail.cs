using System;
using System.Collections.Generic;

namespace OrchidsShop.DAL.Entities;

public partial class OrderDetail
{
    public Guid Id { get; set; }

    public decimal Price { get; set; }

    public int Quantity { get; set; }

    public Guid OrchidId { get; set; }

    public Guid OrderId { get; set; }

    public virtual Orchid Orchid { get; set; } = null!;

    public virtual Order Order { get; set; } = null!;
}
