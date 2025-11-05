using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace PetConnect.Hubs
{
    // Hereda de Hub
    public class ComentarioHub : Hub
    {
        // Esta función será llamada por el JavaScript del cliente
        // para unirse a un "grupo" (una sala por noticia)
        public async Task UnirseAGrupo(string grupoNoticia)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, grupoNoticia);
        }

        // (Opcional) Para cuando el cliente se va de la página
        public async Task DejarGrupo(string grupoNoticia)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, grupoNoticia);
        }
    }
}