using Dapper;
using Persistencia.Entidades;

namespace Persistencia.Repositorios;

/// <summary>
/// Acceso a la tabla combates con Dapper.
/// Lecturas → conexión invitado. Escrituras → conexión administrador.
/// El repositorio solo lee y guarda: las reglas del combate están en la entidad Combate.
/// </summary>
public class CombateRepository : ICombateRepository
{
    private readonly IDbConnectionFactory _factory;

    // Consulta base: el combate, sus dos personajes y cuántos turnos tiene.
    private const string SelectCombates = @"
        SELECT c.Id, c.Estado, c.GanadorId, c.VidaP1, c.VidaP2, c.RecursoP1, c.RecursoP2,
               p1.Id AS P1Id, p1.Nombre AS P1Nombre, p1.Tipo AS P1Tipo,
               p2.Id AS P2Id, p2.Nombre AS P2Nombre, p2.Tipo AS P2Tipo,
               (SELECT COUNT(*) FROM turnos t WHERE t.CombateId = c.Id) AS CantidadTurnos
        FROM combates c
        INNER JOIN personajes p1 ON p1.Id = c.Personaje1Id
        INNER JOIN personajes p2 ON p2.Id = c.Personaje2Id";

    public CombateRepository(IDbConnectionFactory factory)
    {
        _factory = factory ?? throw new ArgumentNullException(nameof(factory));
    }

    // ── Lecturas (invitado) ─────────────────────────────────────────────────

    public async Task<IEnumerable<Combate>> ListarAsync()
    {
        using var db = _factory.CrearConexionInvitado();

        var filas = await db.QueryAsync<CombateFila>(SelectCombates + " ORDER BY c.Id");

        return filas.Select(Reconstruir).ToList();
    }

    public async Task<IEnumerable<Combate>> ListarPorPersonajeAsync(int personajeId)
    {
        using var db = _factory.CrearConexionInvitado();

        var filas = await db.QueryAsync<CombateFila>(
            SelectCombates + " WHERE c.Personaje1Id = @Id OR c.Personaje2Id = @Id ORDER BY c.Id",
            new { Id = personajeId });

        return filas.Select(Reconstruir).ToList();
    }

    public async Task<Combate?> ObtenerPorIdAsync(int id)
    {
        using var db = _factory.CrearConexionInvitado();

        var fila = await db.QueryFirstOrDefaultAsync<CombateFila>(SelectCombates + " WHERE c.Id = @Id", new { Id = id });

        return fila == null ? null : Reconstruir(fila);
    }

    // ── Escrituras (administrador) ──────────────────────────────────────────

    public async Task CrearAsync(Combate combate)
    {
        using var db = _factory.CrearConexionAdministrador();

        int id = await db.QuerySingleAsync<int>(
            @"INSERT INTO combates (Personaje1Id, Personaje2Id, VidaP1, VidaP2, RecursoP1, RecursoP2, Estado)
              VALUES (@Personaje1Id, @Personaje2Id, @VidaP1, @VidaP2, @RecursoP1, @RecursoP2, @Estado);
              SELECT LAST_INSERT_ID();",
            Parametros(combate));

        combate.Id = id;
    }

    public async Task ActualizarParticipantesAsync(Combate combate)
    {
        using var db = _factory.CrearConexionAdministrador();

        await db.ExecuteAsync(
            @"UPDATE combates
              SET Personaje1Id = @Personaje1Id, Personaje2Id = @Personaje2Id,
                  VidaP1 = @VidaP1, VidaP2 = @VidaP2, RecursoP1 = @RecursoP1, RecursoP2 = @RecursoP2
              WHERE Id = @Id",
            Parametros(combate));
    }

    public async Task EliminarAsync(int id)
    {
        using var db = _factory.CrearConexionAdministrador();

        await db.ExecuteAsync("DELETE FROM combates WHERE Id = @Id", new { Id = id });
    }

    // ── Conversión fila ↔ entidad ───────────────────────────────────────────

    private static Combate Reconstruir(CombateFila f)
    {
        // Los personajes salen con sus valores por defecto; después se les pone
        // la vida y el recurso que tienen en ESTE combate.
        var p1 = FabricaPersonajes.Crear(f.P1Tipo, f.P1Nombre, f.P1Id);
        var p2 = FabricaPersonajes.Crear(f.P2Tipo, f.P2Nombre, f.P2Id);
        p1.RestaurarEstado(f.VidaP1, f.RecursoP1);
        p2.RestaurarEstado(f.VidaP2, f.RecursoP2);

        return new Combate(f.Id, p1, p2, Enum.Parse<EstadoCombate>(f.Estado), f.GanadorId, f.CantidadTurnos);
    }

    private static object Parametros(Combate c) => new
    {
        c.Id,
        Personaje1Id = c.Participante1.Id,
        Personaje2Id = c.Participante2.Id,
        VidaP1 = c.Participante1.Vida,
        VidaP2 = c.Participante2.Vida,
        RecursoP1 = c.Participante1.Recurso,
        RecursoP2 = c.Participante2.Recurso,
        Estado = c.Estado.ToString()
    };

    private class CombateFila
    {
        public int Id { get; set; }
        public string Estado { get; set; } = "";
        public int? GanadorId { get; set; }
        public int VidaP1 { get; set; }
        public int VidaP2 { get; set; }
        public int RecursoP1 { get; set; }
        public int RecursoP2 { get; set; }
        public int P1Id { get; set; }
        public string P1Nombre { get; set; } = "";
        public string P1Tipo { get; set; } = "";
        public int P2Id { get; set; }
        public string P2Nombre { get; set; } = "";
        public string P2Tipo { get; set; } = "";
        public int CantidadTurnos { get; set; }
    }
}
