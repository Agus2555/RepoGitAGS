using System.Data;
using Dapper;

namespace Persistencia.Repositorios;

public class BatallaRepository : IBatallaRepository
{
    private readonly IDbConnectionFactory _factory;

    public BatallaRepository(IDbConnectionFactory factory)
    {
        _factory = factory ?? throw new ArgumentNullException(nameof(factory));
    }

    public async Task<int> RegistrarBatallaAsync(int personaje1Id, int personaje2Id)
    {
        using var db = _factory.CrearConexionDesarrollo();

        var parametros = new DynamicParameters();
        parametros.Add("p_Personaje1_Id", personaje1Id);
        parametros.Add("p_Personaje2_Id", personaje2Id);
        parametros.Add("p_BatallaId", dbType: DbType.Int32, direction: ParameterDirection.Output);

        await db.ExecuteAsync("sp_RegistrarBatalla", parametros, commandType: CommandType.StoredProcedure);

        return parametros.Get<int>("p_BatallaId");
    }

    public async Task RegistrarTurnoAsync(
        int batallaId, int turno, int atacanteId, int defensorId, int danio, string? accion)
    {
        using var db = _factory.CrearConexionDesarrollo();

        var parametros = new DynamicParameters();
        parametros.Add("p_BatallaId",      batallaId);
        parametros.Add("p_Turno",          turno);
        parametros.Add("p_AtacanteId",     atacanteId);
        parametros.Add("p_DefensorId",     defensorId);
        parametros.Add("p_DanioRealizado", danio);
        parametros.Add("p_Accion",         accion);

        await db.ExecuteAsync(
            "sp_RegistrarTurnoYActualizarDanio",
            parametros,
            commandType: CommandType.StoredProcedure);
    }

    public async Task FinalizarBatallaAsync(int batallaId, int? ganadorId)
    {
        using var db = _factory.CrearConexionDesarrollo();

        var parametros = new DynamicParameters();
        parametros.Add("p_BatallaId",  batallaId);
        // Dapper envía DBNull cuando el valor es null → el SP recibe NULL (empate).
        parametros.Add("p_GanadorId",  ganadorId, dbType: DbType.Int32);

        await db.ExecuteAsync("sp_FinalizarBatalla", parametros, commandType: CommandType.StoredProcedure);
    }
}
