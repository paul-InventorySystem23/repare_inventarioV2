using System;
using System.Collections.Generic;

namespace inventario_coprotab.Models.DBSeguridadCoprotab;

public partial class PermisosInRole
{
    public int PermisosInRoleId { get; set; }

    public int RoleId { get; set; }

    public int PermisoId { get; set; }
}
