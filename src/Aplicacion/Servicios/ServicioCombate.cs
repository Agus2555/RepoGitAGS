using Aplicacion.Interfaces;
using Aplicacion.Modelos;
using Persistencia.Entidades;

namespace Aplicacion.Servicios;

/// <summary>
/// Servicio puro: no tiene repositorios ni Console.
/// Toda la lógica de turno y de resolución de combate vive acá.
/// Los tests inyectan IEstrategiaTurno para controlar las acciones.
/// </summary>
public class ServicioCombate : IServicioCombate
{
    private readonly IEstrategiaTurno _estrategia;

    public ServicioCombate(IEstrategiaTurno estrategia)
    {
        _estrategia = estrategia ?? throw new ArgumentNullException(nameof(estrategia));
    }

    public TurnoCombate EjecutarTurno(Personaje actor, Personaje objetivo, int numeroTurno)
    {
        var accion = _estrategia.ElegirAccion(actor, objetivo);

        // Switch sobre el ENUM de acciones, no sobre el tipo del personaje: no hay is/typeof.
        int danio = accion switch
        {
            TipoAccion.Atacar        => actor.Atacar(objetivo),
            TipoAccion.Defender      => actor.Defender(),
            TipoAccion.UsarHabilidad => actor.UsarHabilidad(objetivo),
            _                        => throw new ArgumentOutOfRangeException(nameof(accion))
        };

        string nombreAccion = accion switch
        {
            TipoAccion.Atacar        => "Ataque Básico",
            TipoAccion.Defender      => "Defensa",
            TipoAccion.UsarHabilidad => actor.NombreHabilidad,
            _                        => throw new ArgumentOutOfRangeException(nameof(accion))
        };

        return new TurnoCombate(
            numeroTurno,
            actor.Nombre,
            objetivo.Nombre,
            danio,
            nombreAccion,
            actor.EstaVivo,
            objetivo.EstaVivo);
    }

    public ResultadoCombate Combatir(Personaje p1, Personaje p2, int maxTurnos = 50)
    {
        if (p1 == p2)
            throw new ArgumentException("Un personaje no puede pelear contra sí mismo.");
        if (!p1.EstaVivo || !p2.EstaVivo)
            throw new ArgumentException("Ambos personajes deben estar vivos para iniciar el combate.");

        var turnos = new List<TurnoCombate>();
        int numero = 1;

        while (p1.EstaVivo && p2.EstaVivo && numero <= maxTurnos)
        {
            turnos.Add(EjecutarTurno(p1, p2, numero));

            // p1 puede haber eliminado a p2 en este turno.
            if (!p2.EstaVivo) break;

            turnos.Add(EjecutarTurno(p2, p1, numero));

            numero++;
        }

        // Determinar ganador: null si hay empate (ambos vivos al agotar turnos, o caída simultánea).
        Personaje? ganador = (p1.EstaVivo, p2.EstaVivo) switch
        {
            (true, false) => p1,
            (false, true) => p2,
            _             => null
        };

        return new ResultadoCombate(turnos, ganador, numero - 1);
    }
}
