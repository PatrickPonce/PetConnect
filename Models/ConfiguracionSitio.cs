using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PetConnect.Models;

[Table("configuracionsitio")]
public class ConfiguracionSitio
{
    [Key] // Le indicamos a EF que esta es la Clave Primaria
    [Column("clave")]
    public string Clave { get; set; } = string.Empty;

    [Column("valor")]
    public string Valor { get; set; } = string.Empty;
}