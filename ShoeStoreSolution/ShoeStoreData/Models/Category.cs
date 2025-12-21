using System;
using System.Collections.Generic;

namespace ShoeStoreData.Models;

public partial class Category
{
    public string Name { get; set; } = null!;

    public int CategoryId { get; set; }

    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
}
