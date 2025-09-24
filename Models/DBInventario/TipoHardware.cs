using System;
using System.Collections.Generic;

namespace inventario_coprotab.Models.DBInventario;

public partial class TipoHardware
{
    public int IdTipo { get; set; }

    public string Descripcion { get; set; } = null!;

    public virtual ICollection<Dispositivo> Dispositivos { get; set; } = new List<Dispositivo>();
}
