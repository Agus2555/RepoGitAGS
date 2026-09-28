using Aplicacion.Modelos;
using Persistencia.Entidades;

namespace Aplicacion.Interfaces;

/// <summary>
/// Servicio puro de combate: opera en memoria, sin repositorios ni BD.
/// Esta separación es la que permite probarlo en los tests unitarios.
/// </summary>
public interface IServicioCombate
{
    /// <summary>Ejecuta un único turno del actor sobre el objetivo y devuelve el resumen.</summary>
    TurnoCombate EjecutarTurno(Personaje actor, Personaje objetivo, int numeroTurno);

    /// <summary>Ejecuta el combate completo hasta que alguien muere o se alcanza maxTurnos.</summary>
    ResultadoCombate Combatir(Personaje p1, Personaje p2, int maxTurnos = 50);
}
