namespace Persistencia.Repositorios;

/// <summary>
/// Operaciones que requieren privilegios globales sobre el servidor MySQL.
/// Usa CrearConexionAdministrador() en lugar de la conexión de desarrollo.
/// </summary>
public interface IAdministracionRepository
{
    Task<bool> VerificarBaseDatosExisteAsync(string nombreBaseDatos);
}
