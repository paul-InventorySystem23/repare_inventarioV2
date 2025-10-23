using System;
using System.Collections.Generic;

namespace inventario_coprotab.Models.DBSeguridadCoprotab;

public partial class UsersInRole
{
    public int UsersInRoleId { get; set; }

    public int UserId { get; set; }

    public int RoleId { get; set; }

    public virtual Role Role { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
