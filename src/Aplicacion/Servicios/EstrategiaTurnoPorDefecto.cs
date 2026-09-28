using Aplicacion.Interfaces;
using Persistencia.Entidades;

namespace Aplicacion.Servicios;

/// <summary>
/// Estrategia simple para la demo: el personaje siempre ataca.
/// En una versión futura podría elegir habilidad o defensa según el contexto.
/// </summary>
public class EstrategiaTurnoPorDefecto : IEstrategiaTurno
{
    public TipoAccion ElegirAccion(Personaje actor, Personaje objetivo) => TipoAccion.Atacar;
}
