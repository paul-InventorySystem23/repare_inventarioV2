using System;
using System.Collections.Generic;

namespace inventario_coprotab.Models.DBInventario;

public partial class Dispositivo
{
    public int IdDispositivo { get; set; }

    public string Nombre { get; set; } = null!;

    public string? Descripcion { get; set; }

    public int? IdMarca { get; set; }

    public int? IdTipo { get; set; }

    public string? CodigoInventario { get; set; }

    public string? NroSerie { get; set; }

    public string? Estado { get; set; }

    public DateOnly? FechaAlta { get; set; }

    public DateOnly? FechaBaja { get; set; }

    public int? StockActual { get; set; }

    public int? StockMinimo { get; set; }

    public bool EstadoRegistro { get; set; }

    public virtual Marca? IdMarcaNavigation { get; set; }

    public virtual TipoHardware? IdTipoNavigation { get; set; }

    public virtual ICollection<Movimiento> Movimientos { get; set; } = new List<Movimiento>();

    public virtual ICollection<ReparacionConsumible> ReparacionConsumibles { get; set; } = new List<ReparacionConsumible>();

    public virtual ICollection<Reparacione> Reparaciones { get; set; } = new List<Reparacione>();
}
