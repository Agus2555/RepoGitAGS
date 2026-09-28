using Persistencia.Entidades;

namespace Persistencia.Repositorios;

public interface IPersonajeRepository
{
    /// <summary>Inserta el personaje en la BD y le asigna el Id generado.</summary>
    Task<Personaje> RegistrarAsync(Personaje personaje);

    /// <summary>Devuelve null si el Id no existe.</summary>
    Task<Personaje?> ObtenerPorIdAsync(int id);

    Task<IEnumerable<Personaje>> ListarAsync();
}
