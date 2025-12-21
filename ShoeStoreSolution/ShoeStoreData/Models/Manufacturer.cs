using System;
using System.Collections.Generic;

namespace ShoeStoreData.Models;

public partial class Manufacturer
{
    public string Name { get; set; } = null!;

    public int ManufacturerId { get; set; }

    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
}
