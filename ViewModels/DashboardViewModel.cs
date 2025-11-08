using System.Collections.Generic;
using PetConnect.Models; // Asegúrate de tener acceso a tus modelos (Usuario, Servicio, etc.)

namespace PetConnect.ViewModels
{
    public class DashboardViewModel
    {
        // --- KPIs (Indicadores Clave) ---
        public int TotalUsuarios { get; set; }
        public int TotalServicios { get; set; }
        public int TotalNoticias { get; set; }
        public int TotalResenas { get; set; } // O Comentarios, lo que prefieras destacar

        // --- Datos para Gráficos ---
        // (Simplificado: arrays de enteros para Chart.js)
        public int[] UsuariosPorMes { get; set; } = new int[12]; // Ene-Dic
        public string[] MesesLabels { get; set; } = { "Ene", "Feb", "Mar", "Abr", "May", "Jun", "Jul", "Ago", "Sep", "Oct", "Nov", "Dic" };
        
        // Desglose de servicios por tipo
        public int CantidadVeterinarias { get; set; }
        public int CantidadPetShops { get; set; }
        public int CantidadLugares { get; set; }
        public int CantidadGuarderias { get; set; }
        public int CantidadAdopcion { get; set; }

        // --- Actividad Reciente ---
        // (Necesitarás adaptar esto a tus modelos reales de usuario/actividad)
        public List<ActividadReciente> UltimasActividades { get; set; } = new List<ActividadReciente>();
    }

    // Clase auxiliar para mostrar en la tabla de actividad
    public class ActividadReciente
    {
        public string Descripcion { get; set; }
        public string Fecha { get; set; }
        public string Icono { get; set; } // Para Material Symbols
        public string ColorIcono { get; set; } // Clase CSS o color hex
    }
}