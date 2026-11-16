using Persistencia.Entidades;

namespace Persistencia.Repositorios;

public interface ICombateRepository
{
    Task<IEnumerable<Combate>> ListarAsync();

    /// <summary>Combates donde participa el personaje (como P1 o P2).</summary>
    Task<IEnumerable<Combate>> ListarPorPersonajeAsync(int personajeId);

    /// <summary>Reconstruye el combate con sus dos participantes y su estado actual. null si no existe.</summary>
    Task<Combate?> ObtenerPorIdAsync(int id);

    /// <summary>Inserta el combate y le asigna el Id generado.</summary>
    Task CrearAsync(Combate combate);

    /// <summary>Guarda los nuevos participantes y su estado inicial (PUT, antes del primer turno).</summary>
    Task ActualizarParticipantesAsync(Combate combate);

    Task EliminarAsync(int id);
}
