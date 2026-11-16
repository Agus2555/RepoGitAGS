using Dapper;
using Persistencia.Entidades;

namespace Persistencia.Repositorios;

/// <summary>
/// Acceso a la tabla turnos con Dapper.
/// Guardar un turno y deshacer el último tocan DOS tablas (turnos y combates),
/// por eso se hacen dentro de una transacción: o se guardan las dos cosas o ninguna.
/// </summary>
public class TurnoRepository : ITurnoRepository
{
    private readonly IDbConnectionFactory _factory;

    private const string SelectTurnos = @"
        SELECT Id, CombateId, Numero, AtacanteId, DefensorId, Accion, Danio, Nota,
               VidaP1Antes, VidaP2Antes, RecursoP1Antes, RecursoP2Antes
        FROM turnos";

    // Guarda en combates el estado que tiene la entidad en memoria.
    private const string ActualizarEstadoCombate = @"
        UPDATE combates
        SET VidaP1 = @VidaP1, VidaP2 = @VidaP2, RecursoP1 = @RecursoP1, RecursoP2 = @RecursoP2,
            Estado = @Estado, GanadorId = @GanadorId
        WHERE Id = @Id";

    public TurnoRepository(IDbConnectionFactory factory)
    {
        _factory = factory ?? throw new ArgumentNullException(nameof(factory));
    }

    // ── Lecturas (invitado) ─────────────────────────────────────────────────

    public async Task<IEnumerable<Turno>> ListarPorCombateAsync(int combateId)
    {
        using var db = _factory.CrearConexionInvitado();

        var filas = await db.QueryAsync<TurnoFila>(
            SelectTurnos + " WHERE CombateId = @CombateId ORDER BY Numero",
            new { CombateId = combateId });

        return filas.Select(Reconstruir).ToList();
    }

    public async Task<Turno?> ObtenerAsync(int combateId, int turnoId)
    {
        using var db = _factory.CrearConexionInvitado();

        var fila = await db.QueryFirstOrDefaultAsync<TurnoFila>(
            SelectTurnos + " WHERE Id = @TurnoId AND CombateId = @CombateId",
            new { TurnoId = turnoId, CombateId = combateId });

        return fila == null ? null : Reconstruir(fila);
    }

    // ── Escrituras (administrador) ──────────────────────────────────────────

    public async Task GuardarTurnoAsync(Combate combate, Turno turno)
    {
        using var db = _factory.CrearConexionAdministrador();
        db.Open();
        using var transaccion = db.BeginTransaction();

        try
        {
            int id = await db.QuerySingleAsync<int>(
                @"INSERT INTO turnos (CombateId, Numero, AtacanteId, DefensorId, Accion, Danio, Nota,
                                      VidaP1Antes, VidaP2Antes, RecursoP1Antes, RecursoP2Antes)
                  VALUES (@CombateId, @Numero, @AtacanteId, @DefensorId, @Accion, @Danio, @Nota,
                          @VidaP1Antes, @VidaP2Antes, @RecursoP1Antes, @RecursoP2Antes);
                  SELECT LAST_INSERT_ID();",
                turno, transaccion);

            await db.ExecuteAsync(ActualizarEstadoCombate, ParametrosEstado(combate), transaccion);

            transaccion.Commit();
            turno.Id = id;
        }
        catch
        {
            // Si falla cualquiera de las dos sentencias, no queda nada a medias.
            transaccion.Rollback();
            throw;
        }
    }

    public async Task ActualizarNotaAsync(Turno turno)
    {
        using var db = _factory.CrearConexionAdministrador();

        // Solo se toca la columna Nota: el daño y las vidas quedan igual.
        await db.ExecuteAsync("UPDATE turnos SET Nota = @Nota WHERE Id = @Id", new { turno.Id, turno.Nota });
    }

    public async Task EliminarUltimoTurnoAsync(Combate combate, Turno turno)
    {
        using var db = _factory.CrearConexionAdministrador();
        db.Open();
        using var transaccion = db.BeginTransaction();

        try
        {
            await db.ExecuteAsync("DELETE FROM turnos WHERE Id = @Id", new { turno.Id }, transaccion);

            // El combate ya viene restaurado en memoria (Combate.DeshacerTurno); acá solo se guarda.
            await db.ExecuteAsync(ActualizarEstadoCombate, ParametrosEstado(combate), transaccion);

            transaccion.Commit();
        }
        catch
        {
            transaccion.Rollback();
            throw;
        }
    }

    // ── Conversión fila ↔ entidad ───────────────────────────────────────────

    private static object ParametrosEstado(Combate c) => new
    {
        c.Id,
        VidaP1 = c.Participante1.Vida,
        VidaP2 = c.Participante2.Vida,
        RecursoP1 = c.Participante1.Recurso,
        RecursoP2 = c.Participante2.Recurso,
        Estado = c.Estado.ToString(),
        c.GanadorId
    };

    private static Turno Reconstruir(TurnoFila f) =>
        new(f.CombateId, f.Numero, f.AtacanteId, f.DefensorId, f.Accion, f.Danio,
            f.VidaP1Antes, f.VidaP2Antes, f.RecursoP1Antes, f.RecursoP2Antes, f.Nota)
        { Id = f.Id };

    private class TurnoFila
    {
        public int Id { get; set; }
        public int CombateId { get; set; }
        public int Numero { get; set; }
        public int AtacanteId { get; set; }
        public int DefensorId { get; set; }
        public string Accion { get; set; } = "";
        public int Danio { get; set; }
        public string? Nota { get; set; }
        public int VidaP1Antes { get; set; }
        public int VidaP2Antes { get; set; }
        public int RecursoP1Antes { get; set; }
        public int RecursoP2Antes { get; set; }
    }
}
