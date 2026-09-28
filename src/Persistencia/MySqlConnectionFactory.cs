using System.Data;
using MySqlConnector;

namespace Persistencia;

public class MySqlConnectionFactory : IDbConnectionFactory
{
    private readonly string _desarrolloCs;
    private readonly string _adminCs;

    public MySqlConnectionFactory(string desarrolloConnectionString, string adminConnectionString)
    {
        if (string.IsNullOrWhiteSpace(desarrolloConnectionString))
            throw new ArgumentNullException(nameof(desarrolloConnectionString));
        if (string.IsNullOrWhiteSpace(adminConnectionString))
            throw new ArgumentNullException(nameof(adminConnectionString));

        _desarrolloCs = desarrolloConnectionString;
        _adminCs = adminConnectionString;
    }

    // Repositorios de negocio: usuario restringido a simulador_combate.
    public IDbConnection CrearConexionDesarrollo() => new MySqlConnection(_desarrolloCs);

    // Solo AdministracionRepository: usuario con acceso global (information_schema, etc.).
    public IDbConnection CrearConexionAdministrador() => new MySqlConnection(_adminCs);
}
