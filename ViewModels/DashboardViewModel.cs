// ViewModels/DashboardViewModel.cs
using System.Collections.Generic;

namespace PetConnect.ViewModels
{
    public class DashboardViewModel
    {
        // Para el gráfico de Servicios
        public Dictionary<string, int> ConteoServiciosPorTipo { get; set; } = new Dictionary<string, int>();

        // Para el gráfico de Usuarios Registrados (Datos de ejemplo)
        public List<string> EtiquetasTiempoUsuarios { get; set; } = new List<string>();
        public List<int> DatosConteoUsuarios { get; set; } = new List<int>();

        // Para el gráfico de Noticias Publicadas
        public List<string> EtiquetasTiempoNoticias { get; set; } = new List<string>();
        public List<int> DatosConteoNoticias { get; set; } = new List<int>();
    }
}