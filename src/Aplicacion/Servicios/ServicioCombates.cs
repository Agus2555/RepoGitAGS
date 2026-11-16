using Aplicacion.Dtos;
using Aplicacion.Excepciones;
using Aplicacion.Interfaces;
using Persistencia.Entidades;
using Persistencia.Repositorios;

namespace Aplicacion.Servicios;

/// <summary>Casos de uso de combates: listar, ver, crear, cambiar participantes y borrar.</summary>
public class ServicioCombates : IServicioCombates
{
    private readonly ICombateRepository _combates;
    private readonly IPersonajeRepository _personajes;
    private readonly ITurnoRepository _turnos;

    public ServicioCombates(ICombateRepository combates, IPersonajeRepository personajes, ITurnoRepository turnos)
    {
        _combates   = combates   ?? throw new ArgumentNullException(nameof(combates));
        _personajes = personajes ?? throw new ArgumentNullException(nameof(personajes));
        _turnos     = turnos     ?? throw new ArgumentNullException(nameof(turnos));
    }

    public async Task<IEnumerable<CombateResumenDto>> ListarAsync()
    {
        var combates = await _combates.ListarAsync();
        return combates.Select(Mapeo.ACombateResumenDto).ToList();
    }

    public async Task<IEnumerable<CombateResumenDto>> ListarPorPersonajeAsync(int personajeId)
    {
        if (await _personajes.ObtenerPorIdAsync(personajeId) == null)
            throw new NoEncontradoException($"No existe el personaje con Id {personajeId}.");

        var combates = await _combates.ListarPorPersonajeAsync(personajeId);
        return combates.Select(Mapeo.ACombateResumenDto).ToList();   // lista vacía si no participó
    }

    public async Task<CombateDetalleDto> ObtenerAsync(int id)
    {
        var combate = await BuscarCombateAsync(id);
        var turnos = await _turnos.ListarPorCombateAsync(id);
        return Mapeo.ACombateDetalleDto(combate, turnos);
    }

    public async Task<CombateDetalleDto> CrearAsync(ParticipantesDto dto)
    {
        ValidarParticipantes(dto);
        var p1 = await BuscarPersonajeAsync(dto.Personaje1Id);
        var p2 = await BuscarPersonajeAsync(dto.Personaje2Id);

        // Combate nuevo: cada participante arranca con la vida y el recurso inicial de su clase.
        var combate = new Combate(p1, p2);
        await _combates.CrearAsync(combate);

        return Mapeo.ACombateDetalleDto(combate, []);
    }

    public async Task<CombateDetalleDto> ActualizarParticipantesAsync(int id, ParticipantesDto dto)
    {
        ValidarParticipantes(dto);
        var combate = await BuscarCombateAsync(id);

        if (combate.CantidadTurnos > 0)
            throw new ConflictoException("No se pueden cambiar los participantes: el combate ya tiene turnos.");

        var p1 = await BuscarPersonajeAsync(dto.Personaje1Id);
        var p2 = await BuscarPersonajeAsync(dto.Personaje2Id);

        combate.CambiarParticipantes(p1, p2);
        await _combates.ActualizarParticipantesAsync(combate);

        return Mapeo.ACombateDetalleDto(combate, []);
    }

    public async Task EliminarAsync(int id)
    {
        var combate = await BuscarCombateAsync(id);

        if (combate.CantidadTurnos > 0)
            throw new ConflictoException("No se puede eliminar: el combate ya tiene turnos.");

        await _combates.EliminarAsync(id);
    }

    private static void ValidarParticipantes(ParticipantesDto dto)
    {
        if (dto.Personaje1Id <= 0 || dto.Personaje2Id <= 0)
            throw new DatosInvalidosException("Los Id de los personajes tienen que ser mayores a cero.");
        if (dto.Personaje1Id == dto.Personaje2Id)
            throw new DatosInvalidosException("Un combate necesita dos personajes distintos.");
    }

    private async Task<Combate> BuscarCombateAsync(int id) =>
        await _combates.ObtenerPorIdAsync(id)
        ?? throw new NoEncontradoException($"No existe el combate con Id {id}.");

    private async Task<Personaje> BuscarPersonajeAsync(int id) =>
        await _personajes.ObtenerPorIdAsync(id)
        ?? throw new NoEncontradoException($"No existe el personaje con Id {id}.");
}
