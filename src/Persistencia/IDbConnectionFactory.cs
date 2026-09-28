using System.Data;

namespace Persistencia;

/// <summary>
/// Abstracción para las dos conexiones diferenciadas que exige la consigna:
/// - Desarrollo: acceso restringido solo a simulador_combate (usuario 5to_agbd).
/// - Administrador: acceso global al servidor (usuario admin_combate).
/// Los repositorios de negocio usan siempre CrearConexionDesarrollo().
/// Solo AdministracionRepository usa CrearConexionAdministrador().
/// </summary>
public interface IDbConnectionFactory
{
    IDbConnection CrearConexionDesarrollo();
    IDbConnection CrearConexionAdministrador();
}
