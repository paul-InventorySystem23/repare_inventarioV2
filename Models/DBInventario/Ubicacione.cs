using System;
using System.Collections.Generic;

namespace inventario_coprotab.Models.DBInventario;

public partial class Ubicacione
{
    public int IdUbicacion { get; set; }

    public string Nombre { get; set; } = null!;

    public string? Descripcion { get; set; }

    public virtual ICollection<Movimiento> Movimientos { get; set; } = new List<Movimiento>();
}
