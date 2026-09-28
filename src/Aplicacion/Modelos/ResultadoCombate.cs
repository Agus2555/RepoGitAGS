using Persistencia.Entidades;

namespace Aplicacion.Modelos;

/// <summary>
/// Resultado completo de un combate ejecutado en memoria por ServicioCombate.
/// Ganador = null indica empate (ambos cayeron o se alcanzó maxTurnos).
/// </summary>
public class ResultadoCombate
{
    public IReadOnlyList<TurnoCombate> Turnos { get; }
    public Personaje? Ganador { get; }
    public int TurnosJugados { get; }

    public ResultadoCombate(IEnumerable<TurnoCombate> turnos, Personaje? ganador, int turnosJugados)
    {
        Turnos = new List<TurnoCombate>(turnos).AsReadOnly();
        Ganador = ganador;
        TurnosJugados = turnosJugados;
    }
}
