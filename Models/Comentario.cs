using System;
using System.ComponentModel.DataAnnotations;

namespace PetConnect.Models
{
    public class Comentario
    {
        public int Id { get; set; }
        public required string Autor { get; set; }
        public required string Texto { get; set; }
        public DateTime FechaComentario { get; set; }

        public int NoticiaId { get; set; }
        public Noticia? Noticia { get; set; }
    }
}