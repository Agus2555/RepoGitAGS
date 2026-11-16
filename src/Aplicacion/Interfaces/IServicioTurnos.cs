using Aplicacion.Dtos;

namespace Aplicacion.Interfaces;

public interface IServicioTurnos
{
    Task<IEnumerable<TurnoDto>> ListarAsync(int combateId);
    Task<ResultadoTurnoDto> EjecutarAsync(int combateId, EjecutarTurnoDto dto);
    Task<TurnoDto> ActualizarNotaAsync(int combateId, int turnoId, NotaDto dto);
    Task EliminarUltimoAsync(int combateId, int turnoId);
}
