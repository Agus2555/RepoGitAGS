using Persistencia.Entidades;

namespace Persistencia.Repositorios;

public interface IPersonajeRepository
{
    Task<IEnumerable<Personaje>> ListarAsync();

    /// <summary>Devuelve null si el Id no existe.</summary>
    Task<Personaje?> ObtenerPorIdAsync(int id);

    /// <summary>true si el personaje aparece en algún combate (no se puede borrar).</summary>
    Task<bool> ParticipoEnCombatesAsync(int id);

    /// <summary>Inserta el personaje en la BD y le asigna el Id generado.</summary>
    Task<Personaje> CrearAsync(Personaje personaje);

    Task ActualizarNombreAsync(int id, string nombre);

    Task EliminarAsync(int id);
}
