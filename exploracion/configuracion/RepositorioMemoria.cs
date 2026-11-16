namespace Configuracion;

public record PersonajeResumen(int Id, string Nombre, string Tipo);

// ── Repositorio ──────────────────────────────────────────────────────────────

/// <summary>Contrato del repositorio: la API y el servicio solo conocen esto.</summary>
public interface IRepositorioPersonajes
{
    IEnumerable<PersonajeResumen> Listar();
    PersonajeResumen? Obtener(int id);
}

/// <summary>
/// Implementación en memoria. Mañana se puede cambiar por una con MySQL
/// sin tocar las rutas: solo se cambia el registro en Program.cs.
/// </summary>
public class RepositorioPersonajesMemoria : IRepositorioPersonajes
{
    private readonly List<PersonajeResumen> _personajes =
    [
        new(1, "Thorin",  "Guerrero"),
        new(2, "Gandalf", "Mago"),
        new(3, "Legolas", "Arquero")
    ];

    public IEnumerable<PersonajeResumen> Listar() => _personajes;

    public PersonajeResumen? Obtener(int id) => _personajes.FirstOrDefault(p => p.Id == id);
}

// ── Servicio ─────────────────────────────────────────────────────────────────

public interface IServicioPersonajes
{
    IEnumerable<PersonajeResumen> Listar();
    PersonajeResumen? Obtener(int id);
}

/// <summary>El servicio recibe el repositorio por constructor (inyección de dependencias).</summary>
public class ServicioPersonajes(IRepositorioPersonajes repositorio) : IServicioPersonajes
{
    public IEnumerable<PersonajeResumen> Listar() => repositorio.Listar();

    public PersonajeResumen? Obtener(int id) => repositorio.Obtener(id);
}
