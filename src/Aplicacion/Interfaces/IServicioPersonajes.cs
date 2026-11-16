using Aplicacion.Dtos;

namespace Aplicacion.Interfaces;

public interface IServicioPersonajes
{
    Task<IEnumerable<PersonajeDto>> ListarAsync();
    Task<PersonajeDto> ObtenerAsync(int id);
    Task<PersonajeDto> CrearAsync(CrearPersonajeDto dto);
    Task<PersonajeDto> ActualizarNombreAsync(int id, ActualizarPersonajeDto dto);
    Task EliminarAsync(int id);
}
