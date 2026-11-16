using Aplicacion.Dtos;
using Aplicacion.Excepciones;
using Aplicacion.Interfaces;
using Persistencia.Entidades;
using Persistencia.Repositorios;

namespace Aplicacion.Servicios;

/// <summary>
/// Casos de uso de personajes: recibe DTOs, valida, usa el repositorio
/// y devuelve DTOs. Los errores se informan con las excepciones de Aplicacion.Excepciones.
/// </summary>
public class ServicioPersonajes : IServicioPersonajes
{
    private readonly IPersonajeRepository _personajes;

    public ServicioPersonajes(IPersonajeRepository personajes)
    {
        _personajes = personajes ?? throw new ArgumentNullException(nameof(personajes));
    }

    public async Task<IEnumerable<PersonajeDto>> ListarAsync()
    {
        var personajes = await _personajes.ListarAsync();
        return personajes.Select(Mapeo.APersonajeDto).ToList();
    }

    public async Task<PersonajeDto> ObtenerAsync(int id) => Mapeo.APersonajeDto(await BuscarAsync(id));

    public async Task<PersonajeDto> CrearAsync(CrearPersonajeDto dto)
    {
        Personaje personaje;
        try
        {
            // DTO → entidad: la fábrica elige la clase y sus estadísticas según el tipo.
            personaje = FabricaPersonajes.Crear(dto.Tipo, dto.Nombre);
        }
        catch (ArgumentException ex)
        {
            throw new DatosInvalidosException(ex.Message);   // tipo o nombre inválido → 400
        }

        await _personajes.CrearAsync(personaje);
        return Mapeo.APersonajeDto(personaje);
    }

    public async Task<PersonajeDto> ActualizarNombreAsync(int id, ActualizarPersonajeDto dto)
    {
        var personaje = await BuscarAsync(id);

        try
        {
            personaje.CambiarNombre(dto.Nombre);
        }
        catch (ArgumentException ex)
        {
            throw new DatosInvalidosException(ex.Message);   // nombre inválido → 400
        }

        await _personajes.ActualizarNombreAsync(personaje.Id, personaje.Nombre);
        return Mapeo.APersonajeDto(personaje);
    }

    public async Task EliminarAsync(int id)
    {
        await BuscarAsync(id);

        if (await _personajes.ParticipoEnCombatesAsync(id))
            throw new ConflictoException("No se puede eliminar: el personaje participó en al menos un combate.");

        await _personajes.EliminarAsync(id);
    }

    private async Task<Personaje> BuscarAsync(int id) =>
        await _personajes.ObtenerPorIdAsync(id)
        ?? throw new NoEncontradoException($"No existe el personaje con Id {id}.");
}
