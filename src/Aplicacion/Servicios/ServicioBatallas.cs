using Aplicacion.Interfaces;
using Aplicacion.Modelos;
using Persistencia.Repositorios;

namespace Aplicacion.Servicios;

/// <summary>
/// Orquestador con persistencia.
/// Responsabilidades: cargar personajes, registrar la batalla en BD,
/// delegar el combate a ServicioCombate, persistir cada turno y cerrar la batalla.
/// No contiene ninguna lógica de combate propia (eso le pertenece a ServicioCombate).
/// </summary>
public class ServicioBatallas : IServicioBatallas
{
    private readonly IBatallaRepository _batallaRepo;
    private readonly IPersonajeRepository _personajeRepo;
    private readonly IServicioCombate _servicioCombate;

    public ServicioBatallas(
        IBatallaRepository batallaRepository,
        IPersonajeRepository personajeRepository,
        IServicioCombate servicioCombate)
    {
        _batallaRepo     = batallaRepository  ?? throw new ArgumentNullException(nameof(batallaRepository));
        _personajeRepo   = personajeRepository ?? throw new ArgumentNullException(nameof(personajeRepository));
        _servicioCombate = servicioCombate    ?? throw new ArgumentNullException(nameof(servicioCombate));
    }

    public async Task<ResultadoCombate> SimularAsync(int personaje1Id, int personaje2Id)
    {
        var p1 = await _personajeRepo.ObtenerPorIdAsync(personaje1Id)
            ?? throw new InvalidOperationException($"Personaje con Id={personaje1Id} no encontrado.");

        var p2 = await _personajeRepo.ObtenerPorIdAsync(personaje2Id)
            ?? throw new InvalidOperationException($"Personaje con Id={personaje2Id} no encontrado.");

        // sp_RegistrarBatalla: inserta batalla + 2 filas en ParticipantesBatalla con estado inicial.
        int batallaId = await _batallaRepo.RegistrarBatallaAsync(p1.Id, p2.Id);

        // El combate ocurre completamente en memoria.
        var resultado = _servicioCombate.Combatir(p1, p2);

        // Persistir cada turno: sp_RegistrarTurnoYActualizarDanio acumula daño en ParticipantesBatalla.
        foreach (var turno in resultado.Turnos)
        {
            int atacanteId = turno.NombreActor == p1.Nombre ? p1.Id : p2.Id;
            int defensorId = turno.NombreObjetivo == p1.Nombre ? p1.Id : p2.Id;

            await _batallaRepo.RegistrarTurnoAsync(
                batallaId, turno.Numero, atacanteId, defensorId, turno.Danio, turno.NombreAccion);
        }

        // sp_FinalizarBatalla: cierra la batalla y marca resultado en ParticipantesBatalla.
        await _batallaRepo.FinalizarBatallaAsync(batallaId, resultado.Ganador?.Id);

        return resultado;
    }
}
