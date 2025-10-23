using System;
using System.Collections.Generic;

namespace inventario_coprotab.Models.DBSeguridadCoprotab;

public partial class Permiso
{
    public int PermisoId { get; set; }

    public int ApplicationId { get; set; }

    public string PermisoName { get; set; } = null!;

    public string Description { get; set; } = null!;

    public string? CodigoArbol { get; set; }
}
