using Aplicacion.Dtos;
using Aplicacion.Excepciones;
using Aplicacion.Interfaces;
using Persistencia.Entidades;
using Persistencia.Repositorios;

namespace Aplicacion.Servicios;

/// <summary>
/// Casos de uso de turnos. Las reglas (a quién le toca, daño, fin del combate, deshacer)
/// las resuelve la entidad Combate; acá se busca, se valida, se guarda y se mapea a DTO.
/// </summary>
public class ServicioTurnos : IServicioTurnos
{
    private readonly ICombateRepository _combates;
    private readonly ITurnoRepository _turnos;

    public ServicioTurnos(ICombateRepository combates, ITurnoRepository turnos)
    {
        _combates = combates ?? throw new ArgumentNullException(nameof(combates));
        _turnos   = turnos   ?? throw new ArgumentNullException(nameof(turnos));
    }

    public async Task<IEnumerable<TurnoDto>> ListarAsync(int combateId)
    {
        await BuscarCombateAsync(combateId);

        var turnos = await _turnos.ListarPorCombateAsync(combateId);
        return turnos.Select(Mapeo.ATurnoDto).ToList();   // lista vacía si todavía no hay turnos
    }

    public async Task<ResultadoTurnoDto> EjecutarAsync(int combateId, EjecutarTurnoDto dto)
    {
        var combate = await BuscarCombateAsync(combateId);

        if (!combate.Participa(dto.AtacanteId))
            throw new NoEncontradoException($"El atacante con Id {dto.AtacanteId} no existe en este combate.");

        Turno turno;
        try
        {
            turno = combate.EjecutarTurno(dto.AtacanteId, dto.Accion);
        }
        catch (InvalidOperationException ex)
        {
            throw new DatosInvalidosException(ex.Message);   // fuera de turno o combate finalizado → 400
        }
        catch (ArgumentException ex)
        {
            throw new DatosInvalidosException(ex.Message);   // acción inválida → 400
        }

        // Turno + nuevo estado del combate, en una sola transacción.
        await _turnos.GuardarTurnoAsync(combate, turno);

        return Mapeo.AResultadoTurnoDto(combate, turno);
    }

    public async Task<TurnoDto> ActualizarNotaAsync(int combateId, int turnoId, NotaDto dto)
    {
        await BuscarCombateAsync(combateId);
        var turno = await BuscarTurnoAsync(combateId, turnoId);

        try
        {
            turno.CambiarNota(dto.Nota);
        }
        catch (ArgumentException ex)
        {
            throw new DatosInvalidosException(ex.Message);   // nota inválida → 400
        }

        await _turnos.ActualizarNotaAsync(turno);
        return Mapeo.ATurnoDto(turno);
    }

    public async Task EliminarUltimoAsync(int combateId, int turnoId)
    {
        var combate = await BuscarCombateAsync(combateId);
        var turno = await BuscarTurnoAsync(combateId, turnoId);

        if (!combate.EsUltimoTurno(turno))
            throw new ConflictoException("Solo se puede borrar el último turno del combate.");

        // Restaura vidas, recursos, estado y ganador con la foto guardada en el turno...
        combate.DeshacerTurno(turno);

        // ...y en otra transacción borra el turno y guarda el combate restaurado.
        await _turnos.EliminarUltimoTurnoAsync(combate, turno);
    }

    private async Task<Combate> BuscarCombateAsync(int id) =>
        await _combates.ObtenerPorIdAsync(id)
        ?? throw new NoEncontradoException($"No existe el combate con Id {id}.");

    private async Task<Turno> BuscarTurnoAsync(int combateId, int turnoId) =>
        await _turnos.ObtenerAsync(combateId, turnoId)
        ?? throw new NoEncontradoException($"No existe el turno {turnoId} en el combate {combateId}.");
}
