using Aplicacion.Modelos;

namespace Aplicacion.Interfaces;

/// <summary>
/// Orquesta la persistencia: carga personajes, llama a ServicioCombate
/// y persiste el resultado. No contiene lógica de combate propia.
/// </summary>
public interface IServicioBatallas
{
    Task<ResultadoCombate> SimularAsync(int personaje1Id, int personaje2Id);
}
