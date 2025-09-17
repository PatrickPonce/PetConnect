using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using System.ComponentModel.DataAnnotations.Schema;

namespace PetConnect.Models;

[Table("resenas")]
public class Resena
{
    [Column("id")]
    public int Id { get; set; }

    [Column("contenido")]
    public string Contenido { get; set; } = string.Empty;

    [Column("puntuacion")]
    public short Puntuacion { get; set; } // 'short' equivale a SMALLINT

    [Column("fechacreacion")]
    public DateTime FechaCreacion { get; set; }

    [Column("usuarioid")]
    public int UsuarioId { get; set; }

    [Column("servicioid")]
    public int ServicioId { get; set; }
}