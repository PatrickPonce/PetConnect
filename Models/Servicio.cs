using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using System.ComponentModel.DataAnnotations.Schema;

namespace PetConnect.Models;

[Table("servicios")]
public class Servicio
{
    [Column("id")]
    public int Id { get; set; }

    [Column("nombre")]
    public string Nombre { get; set; } = string.Empty;

    [Column("descripcioncorta")]
    public string DescripcionCorta { get; set; } = string.Empty;

    [Column("descripcionlarga")]
    public string? DescripcionLarga { get; set; }

    [Column("direccion")]
    public string? Direccion { get; set; }

    [Column("latitud")]
    public decimal? Latitud { get; set; }

    [Column("longitud")]
    public decimal? Longitud { get; set; }

    [Column("telefono")]
    public string? Telefono { get; set; }

    [Column("emailcontacto")]
    public string? EmailContacto { get; set; }

    [Column("sitioweb")]
    public string? SitioWeb { get; set; }

    [Column("imagenprincipalurl")]
    public string? ImagenPrincipalUrl { get; set; }

    [Column("categoriaid")]
    public int? CategoriaId { get; set; }
    
    public Categoria? Categoria { get; set; }

}