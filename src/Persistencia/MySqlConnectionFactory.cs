using System.Data;
using MySqlConnector;

namespace Persistencia;

public class MySqlConnectionFactory : IDbConnectionFactory
{
    private readonly string _invitadoCs;
    private readonly string _administradorCs;

    public MySqlConnectionFactory(string invitadoConnectionString, string administradorConnectionString)
    {
        if (string.IsNullOrWhiteSpace(invitadoConnectionString))
            throw new ArgumentNullException(nameof(invitadoConnectionString));
        if (string.IsNullOrWhiteSpace(administradorConnectionString))
            throw new ArgumentNullException(nameof(administradorConnectionString));

        _invitadoCs = invitadoConnectionString;
        _administradorCs = administradorConnectionString;
    }

    // Lecturas: usuario invitado (solo SELECT).
    public IDbConnection CrearConexionInvitado() => new MySqlConnection(_invitadoCs);

    // Escrituras: usuario administrador.
    public IDbConnection CrearConexionAdministrador() => new MySqlConnection(_administradorCs);
}
