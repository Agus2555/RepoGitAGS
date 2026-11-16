namespace Exploracion.Dto;

/// <summary>
/// Entidad del dominio (versión de prueba). Tiene más datos de los que el cliente
/// necesita ver: la vida y la fecha de creación son internas y NO salen en la respuesta.
/// </summary>
public class PersonajeEntidad
{
    public int Id { get; set; }
    public string Nombre { get; set; } = "";
    public string Tipo { get; set; } = "";
    public int Vida { get; set; }
    public DateTime FechaCreacion { get; set; }
}

/// <summary>
/// Servicio de ejemplo: recibe el DTO de entrada, arma la entidad, la guarda en memoria
/// y devuelve el DTO de salida. La transformación DTO ↔ entidad pasa ACÁ, en el servicio.
/// </summary>
public class ServicioPersonajes
{
    // "Base de datos" en memoria: todavía no se conecta MySQL.
    private readonly List<PersonajeEntidad> _personajes = [];
    private int _ultimoId = 0;

    public PersonajeCreadoDto Crear(CrearPersonajeDto dto)
    {
        // DTO de entrada → entidad
        var entidad = new PersonajeEntidad
        {
            Id = ++_ultimoId,
            Nombre = dto.Nombre!.Trim(),
            Tipo = dto.Tipo!.Trim(),
            Vida = 100,
            FechaCreacion = DateTime.Now
        };

        _personajes.Add(entidad);

        // entidad → DTO de salida (no se devuelve la entidad directamente)
        return new PersonajeCreadoDto(entidad.Id, entidad.Nombre);
    }
}
