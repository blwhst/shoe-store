using System;
using System.Collections.Generic;

namespace ShoeStoreData.Models;

public partial class Customer
{
    public string FullName { get; set; } = null!;

    public int CustomerId { get; set; }

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
}
