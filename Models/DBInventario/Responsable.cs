using System;
using System.Collections.Generic;

namespace inventario_coprotab.Models.DBInventario;

public partial class Responsable
{
    public int IdResponsable { get; set; }

    public string Nombre { get; set; } = null!;

    public string Apellido { get; set; } = null!;

    public string Sector { get; set; } = null!;

    public string? Email { get; set; }

    public string? Telefono { get; set; }

    public virtual ICollection<Movimiento> Movimientos { get; set; } = new List<Movimiento>();

    public virtual ICollection<RelacionDispositivoComponente> RelacionDispositivoComponentes { get; set; } = new List<RelacionDispositivoComponente>();

    public virtual ICollection<Reparacione> Reparaciones { get; set; } = new List<Reparacione>();
}
