using System;
using System.Collections.Generic;

namespace inventario_coprotab.Models.DBInventario;

public partial class ReparacionConsumible
{
    public int IdReparacionConsumible { get; set; }

    public int IdReparacion { get; set; }

    public int IdDispositivo { get; set; }

    public int Cantidad { get; set; }

    public virtual Dispositivo IdDispositivoNavigation { get; set; } = null!;

    public virtual Reparacione IdReparacionNavigation { get; set; } = null!;
}
