using Aplicacion.Dtos;

namespace Aplicacion.Interfaces;

public interface IServicioCombates
{
    Task<IEnumerable<CombateResumenDto>> ListarAsync();
    Task<IEnumerable<CombateResumenDto>> ListarPorPersonajeAsync(int personajeId);
    Task<CombateDetalleDto> ObtenerAsync(int id);
    Task<CombateDetalleDto> CrearAsync(ParticipantesDto dto);
    Task<CombateDetalleDto> ActualizarParticipantesAsync(int id, ParticipantesDto dto);
    Task EliminarAsync(int id);
}
