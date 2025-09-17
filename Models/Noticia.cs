using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations.Schema; 

namespace PetConnect.Models;

[Table("noticias")]
public class Noticia
{
    [Column("id")]
    public int Id { get; set; }

    [Column("titulo")]
    public string Titulo { get; set; } = string.Empty;

    [Column("resumen")]
    public string Resumen { get; set; } = string.Empty;

    [Column("contenido")]
    public string Contenido { get; set; } = string.Empty;

    [Column("fechapublicacion")]
    public DateTime FechaPublicacion { get; set; }

    [Column("imagenurl")]
    public string? ImagenUrl { get; set; }

    [Column("destacada")]
    public bool Destacada { get; set; }

    [Column("usuarioid")]
    public int? UsuarioId { get; set; }

    [Column("categoriaid")]
    public int? CategoriaId { get; set; }
}