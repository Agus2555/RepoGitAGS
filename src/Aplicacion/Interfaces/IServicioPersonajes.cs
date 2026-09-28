using Persistencia.Entidades;

namespace Aplicacion.Interfaces;

public interface IServicioPersonajes
{
    Task<Personaje> RegistrarAsync(Personaje personaje);
    Task<Personaje?> ObtenerPorIdAsync(int id);
    Task<IEnumerable<Personaje>> ListarAsync();
}
