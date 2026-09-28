using Dapper;
using Persistencia.Entidades;

namespace Persistencia.Repositorios;

/// <summary>
/// Operaciones CRUD simples → se resuelven con Dapper directo, sin Stored Procedures.
/// (La consigna reserva los SP para operaciones de complejidad media o alta.)
/// </summary>
public class PersonajeRepository : IPersonajeRepository
{
    private readonly IDbConnectionFactory _factory;

    public PersonajeRepository(IDbConnectionFactory factory)
    {
        _factory = factory ?? throw new ArgumentNullException(nameof(factory));
    }

    public async Task<Personaje> RegistrarAsync(Personaje personaje)
    {
        using var db = _factory.CrearConexionDesarrollo();

        int id = await db.QuerySingleAsync<int>(
            @"INSERT INTO Personajes (Nombre, TipoClase, Vida, VidaMaxima, Ataque, Defensa, RecursoEspecial)
              VALUES (@Nombre, @TipoClase, @Vida, @VidaMaxima, @Ataque, @Defensa, @RecursoEspecial);
              SELECT LAST_INSERT_ID();",
            new
            {
                Nombre        = personaje.Nombre,
                TipoClase     = personaje.GetType().Name,
                Vida          = personaje.Vida,
                VidaMaxima    = personaje.VidaMaxima,
                Ataque        = personaje.Ataque,
                Defensa       = personaje.Defensa,
                RecursoEspecial = personaje.Recurso
            });

        // AsignarId es internal: accesible desde este assembly (Persistencia).
        personaje.AsignarId(id);
        return personaje;
    }

    public async Task<Personaje?> ObtenerPorIdAsync(int id)
    {
        using var db = _factory.CrearConexionDesarrollo();

        var fila = await db.QueryFirstOrDefaultAsync(
            "SELECT Id, Nombre, TipoClase, Vida, VidaMaxima, Ataque, Defensa, RecursoEspecial FROM Personajes WHERE Id = @Id",
            new { Id = id });

        if (fila == null) return null;

        return FabricaPersonajes.Crear(
            (string)fila.TipoClase,
            (string)fila.Nombre,
            (int)fila.Vida,
            (int)fila.Ataque,
            (int)fila.Defensa,
            (int)fila.RecursoEspecial,
            (int)fila.Id);
    }

    public async Task<IEnumerable<Personaje>> ListarAsync()
    {
        using var db = _factory.CrearConexionDesarrollo();

        var filas = await db.QueryAsync(
            "SELECT Id, Nombre, TipoClase, Vida, VidaMaxima, Ataque, Defensa, RecursoEspecial FROM Personajes");

        return filas.Select(r => FabricaPersonajes.Crear(
            (string)r.TipoClase,
            (string)r.Nombre,
            (int)r.Vida,
            (int)r.Ataque,
            (int)r.Defensa,
            (int)r.RecursoEspecial,
            (int)r.Id));
    }
}
