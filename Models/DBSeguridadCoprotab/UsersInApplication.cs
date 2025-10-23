using System;
using System.Collections.Generic;

namespace inventario_coprotab.Models.DBSeguridadCoprotab;

public partial class UsersInApplication
{
    public int UsersInApplicationId { get; set; }

    public int UserId { get; set; }

    public int ApplicationId { get; set; }

    public virtual Application Application { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
