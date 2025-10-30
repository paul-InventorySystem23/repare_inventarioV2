using System;
using System.Collections.Generic;

namespace inventario_coprotab.Models.DBInventario;

public partial class RelacionDetalle
{
    public int IdRelacionDetalle { get; set; }

    public int IdRelacion { get; set; }

    public int IdDispositivo { get; set; }

    public int IdComponente { get; set; }

    public virtual Componente IdComponenteNavigation { get; set; } = null!;

    public virtual Dispositivo IdDispositivoNavigation { get; set; } = null!;

    public virtual Relacion IdRelacionNavigation { get; set; } = null!;
}
