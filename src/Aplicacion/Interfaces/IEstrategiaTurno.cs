using Persistencia.Entidades;

namespace Aplicacion.Interfaces;

/// <summary>
/// Decide qué acción toma el actor en su turno.
/// ServicioCombate la llama antes de despachar la acción al personaje.
/// Los tests inyectan implementaciones fijas para que las aserciones sean deterministas.
/// </summary>
public interface IEstrategiaTurno
{
    TipoAccion ElegirAccion(Personaje actor, Personaje objetivo);
}
