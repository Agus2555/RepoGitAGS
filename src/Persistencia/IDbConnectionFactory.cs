using System.Data;

namespace Persistencia;

/// <summary>
/// Abstracción para las dos conexiones diferenciadas que exige la consigna:
/// - Invitado: solo SELECT sobre combate_api. Se usa en TODAS las lecturas (GET).
/// - Administrador: todos los permisos sobre combate_api. Se usa en TODAS las escrituras
///   (INSERT, UPDATE y DELETE de los POST, PUT y DELETE).
/// </summary>
public interface IDbConnectionFactory
{
    IDbConnection CrearConexionInvitado();
    IDbConnection CrearConexionAdministrador();
}
