using System;
using System.Collections.Generic;

namespace inventario_coprotab.Models.DBInventario;

public partial class Relacion
{
    public int IdRelacion { get; set; }

    public DateOnly Fecha { get; set; }

    public virtual ICollection<RelacionDetalle> RelacionDetalles { get; set; } = new List<RelacionDetalle>();
}
