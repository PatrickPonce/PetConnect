using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using System.ComponentModel.DataAnnotations.Schema;

namespace PetConnect.Models;

[Table("favoritos")]
public class Favorito
{
    [Column("id")]
    public int Id { get; set; }

    [Column("usuarioid")]
    public int UsuarioId { get; set; }

    [Column("entidadid")]
    public int EntidadId { get; set; }

    [Column("tipoentidad")]
    public string TipoEntidad { get; set; } = string.Empty; // Se maneja como string

    [Column("fechacreacion")]
    public DateTime FechaCreacion { get; set; }
}