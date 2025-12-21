using System;
using System.Collections.Generic;

namespace ShoeStoreData.Models;

public partial class OrderItem
{
    public int OrderId { get; set; }

    public string Article { get; set; } = null!;

    public int Quantity { get; set; }

    public virtual Product ArticleNavigation { get; set; } = null!;

    public virtual Order Order { get; set; } = null!;
}
