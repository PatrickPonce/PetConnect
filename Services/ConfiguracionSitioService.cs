using Microsoft.EntityFrameworkCore;
using PetConnect.Data;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PetConnect.Services
{
    public class ConfiguracionSitioService
    {
        private readonly ApplicationDbContext _context;
        private Dictionary<string, string> _configuracionesCache;

        public ConfiguracionSitioService(ApplicationDbContext context)
        {
            _context = context;
        }

        private async Task CargarConfiguracionesAsync()
        {
            if (_configuracionesCache == null)
            {
                _configuracionesCache = await _context.ConfiguracionesSitio
                                                    .AsNoTracking()
                                                    .ToDictionaryAsync(c => c.Clave, c => c.Valor);
            }
        }

        public async Task<string> ObtenerValorAsync(string clave, string valorPorDefecto)
        {
            await CargarConfiguracionesAsync();
            return _configuracionesCache.GetValueOrDefault(clave, valorPorDefecto);
        }
    }
}