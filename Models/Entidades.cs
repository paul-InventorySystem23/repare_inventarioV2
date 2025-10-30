using carrito.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace carrito.Models
{
    [Table("dispositivos")]
    public class Dispositivo
    {
        [Key]
        [Column("id_dispositivo")]
        public int IdDispositivo { get; set; }

        [Column("nombre")]
        [StringLength(255)]
        public string Nombre { get; set; }

        [Column("descripcion")]
        [StringLength(500)]
        public string Descripcion { get; set; }

        [Column("id_marca")]
        public int IdMarca { get; set; }

        [Column("id_tipo")]
        public int IdTipo { get; set; }

        [Column("codigo_inventario")]
        [StringLength(50)]
        public string CodigoInventario { get; set; }

        [Column("nro_serie")]
        [StringLength(100)]
        public string NroSerie { get; set; }

        [Column("estado")]
        [StringLength(50)]
        public string Estado { get; set; }

        [Column("fecha_alta")]
        public DateTime FechaAlta { get; set; }

        [Column("fecha_baja")]
        public DateTime? FechaBaja { get; set; }

        [Column("stock_actual")]
        public int StockActual { get; set; }

        [Column("stock_minimo")]
        public int StockMinimo { get; set; }

        [Column("estado_registro")]
        public bool EstadoRegistro { get; set; }

        // Relaciones
        public virtual ICollection<RelacionDetalle> RelacionDetalles { get; set; }
    }
}

[Table("componentes")]
public class Componente
{
    [Key]
    [Column("id_componente")]
    public int IdComponente { get; set; }

    [Column("nombre")]
    [StringLength(255)]
    public string Nombre { get; set; }

    [Column("descripcion")]
    [StringLength(500)]
    public string Descripcion { get; set; }

    [Column("id_marca")]
    public int IdMarca { get; set; }

    [Column("id_tipo")]
    public int IdTipo { get; set; }

    [Column("nro_serie")]
    [StringLength(100)]
    public string NroSerie { get; set; }

    [Column("estado")]
    [StringLength(50)]
    public string Estado { get; set; }

    [Column("fecha_instalacion")]
    public DateTime? FechaInstalacion { get; set; }

    [Column("estado_registro")]
    public bool EstadoRegistro { get; set; }

    [Column("cantidad")]
    public int Cantidad { get; set; }

    [Column("stock_minimo")]
    public int StockMinimo { get; set; }

    // Relaciones
    public virtual ICollection<RelacionDetalle> RelacionDetalles { get; set; }
}

[Table("relacion")]
public class Relacion
{
    [Key]
    [Column("id_relacion")]
    public int IdRelacion { get; set; }

    [Column("fecha", TypeName = "date")]
    public DateTime Fecha { get; set; }

    public virtual ICollection<RelacionDetalle> RelacionDetalles { get; set; }
}

[Table("relacion_detalle")]
public class RelacionDetalle
{
    [Key]
    [Column("id_relacion_detalle")]
    public int IdRelacionDetalle { get; set; }

    [Column("id_relacion")]
    public int IdRelacion { get; set; }

    [Column("id_dispositivo")]
    public int IdDispositivo { get; set; }

    [Column("id_componente")]
    public int IdComponente { get; set; }

    [ForeignKey("IdRelacion")]
    public virtual Relacion Relacion { get; set; }

    [ForeignKey("IdDispositivo")]
    public virtual Dispositivo Dispositivo { get; set; }

    [ForeignKey("IdComponente")]
    public virtual Componente Componente { get; set; }
}
