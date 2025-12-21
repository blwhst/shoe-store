using System;
using System.Collections.Generic;

namespace ShoeStoreData.Models;

public partial class Supplier
{
    public string Name { get; set; } = null!;

    public int SupplierId { get; set; }

    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
}
