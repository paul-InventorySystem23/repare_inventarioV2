using System;
using System.Collections.Generic;

namespace inventario_coprotab.Models.DBSeguridadCoprotab;

public partial class Role
{
    public int RoleId { get; set; }

    public int ApplicationId { get; set; }

    public string RoleName { get; set; } = null!;

    public string Description { get; set; } = null!;

    public virtual Application Application { get; set; } = null!;

    public virtual ICollection<UsersInRole> UsersInRoles { get; set; } = new List<UsersInRole>();
}
