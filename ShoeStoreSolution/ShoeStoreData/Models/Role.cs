using System;
using System.Collections.Generic;

namespace ShoeStoreData.Models;

public partial class Role
{
    public string RoleName { get; set; } = null!;

    public int RoleId { get; set; }

    public virtual ICollection<User> Users { get; set; } = new List<User>();
}
