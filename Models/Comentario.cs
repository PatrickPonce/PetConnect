namespace PetConnect.Models
{
    public class Comentario
    {
        public int Id { get; set; }
        public string? Usuario { get; set; }
        public string? Texto { get; set; }
        public DateTime Fecha { get; set; } = DateTime.Now;
    }
}
