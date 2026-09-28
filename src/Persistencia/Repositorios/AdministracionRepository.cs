using Dapper;

namespace Persistencia.Repositorios;

/// <summary>
/// Ejemplo concreto del principio de mínimo privilegio:
/// la comprobación de existencia de una base de datos requiere leer
/// information_schema, algo que el usuario de desarrollo no puede hacer.
/// Por eso usa la conexión administrador.
/// </summary>
public class AdministracionRepository : IAdministracionRepository
{
    private readonly IDbConnectionFactory _factory;

    public AdministracionRepository(IDbConnectionFactory factory)
    {
        _factory = factory ?? throw new ArgumentNullException(nameof(factory));
    }

    public async Task<bool> VerificarBaseDatosExisteAsync(string nombreBaseDatos)
    {
        using var db = _factory.CrearConexionAdministrador();

        int cantidad = await db.QuerySingleAsync<int>(
            "SELECT COUNT(*) FROM information_schema.SCHEMATA WHERE SCHEMA_NAME = @Nombre",
            new { Nombre = nombreBaseDatos });

        return cantidad > 0;
    }
}
