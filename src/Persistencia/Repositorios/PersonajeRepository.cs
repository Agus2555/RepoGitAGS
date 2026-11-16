using Dapper;
using Persistencia.Entidades;

namespace Persistencia.Repositorios;

/// <summary>
/// Acceso a la tabla personajes con Dapper.
/// Lecturas → conexión invitado. Escrituras → conexión administrador.
/// </summary>
public class PersonajeRepository : IPersonajeRepository
{
    private readonly IDbConnectionFactory _factory;

    public PersonajeRepository(IDbConnectionFactory factory)
    {
        _factory = factory ?? throw new ArgumentNullException(nameof(factory));
    }

    // ── Lecturas (invitado) ─────────────────────────────────────────────────

    public async Task<IEnumerable<Personaje>> ListarAsync()
    {
        using var db = _factory.CrearConexionInvitado();

        var filas = await db.QueryAsync<PersonajeFila>("SELECT Id, Nombre, Tipo FROM personajes ORDER BY Id");

        return filas.Select(f => FabricaPersonajes.Crear(f.Tipo, f.Nombre, f.Id)).ToList();
    }

    public async Task<Personaje?> ObtenerPorIdAsync(int id)
    {
        using var db = _factory.CrearConexionInvitado();

        var fila = await db.QueryFirstOrDefaultAsync<PersonajeFila>(
            "SELECT Id, Nombre, Tipo FROM personajes WHERE Id = @Id",
            new { Id = id });

        return fila == null ? null : FabricaPersonajes.Crear(fila.Tipo, fila.Nombre, fila.Id);
    }

    public async Task<bool> ParticipoEnCombatesAsync(int id)
    {
        using var db = _factory.CrearConexionInvitado();

        int cantidad = await db.ExecuteScalarAsync<int>(
            "SELECT COUNT(*) FROM combates WHERE Personaje1Id = @Id OR Personaje2Id = @Id",
            new { Id = id });

        return cantidad > 0;
    }

    // ── Escrituras (administrador) ──────────────────────────────────────────

    public async Task<Personaje> CrearAsync(Personaje personaje)
    {
        using var db = _factory.CrearConexionAdministrador();

        int id = await db.QuerySingleAsync<int>(
            @"INSERT INTO personajes (Nombre, Tipo) VALUES (@Nombre, @Tipo);
              SELECT LAST_INSERT_ID();",
            new { personaje.Nombre, personaje.Tipo });

        // AsignarId es internal: accesible desde este assembly (Persistencia).
        personaje.AsignarId(id);
        return personaje;
    }

    public async Task ActualizarNombreAsync(int id, string nombre)
    {
        using var db = _factory.CrearConexionAdministrador();

        await db.ExecuteAsync("UPDATE personajes SET Nombre = @Nombre WHERE Id = @Id", new { Id = id, Nombre = nombre });
    }

    public async Task EliminarAsync(int id)
    {
        using var db = _factory.CrearConexionAdministrador();

        await db.ExecuteAsync("DELETE FROM personajes WHERE Id = @Id", new { Id = id });
    }

    // Forma de una fila de la tabla; Dapper llena las propiedades por nombre de columna.
    private class PersonajeFila
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = "";
        public string Tipo { get; set; } = "";
    }
}
