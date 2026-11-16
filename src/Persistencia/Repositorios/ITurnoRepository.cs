using Persistencia.Entidades;

namespace Persistencia.Repositorios;

public interface ITurnoRepository
{
    /// <summary>Turnos del combate en orden de ejecución.</summary>
    Task<IEnumerable<Turno>> ListarPorCombateAsync(int combateId);

    /// <summary>null si el turno no existe o no pertenece a ese combate.</summary>
    Task<Turno?> ObtenerAsync(int combateId, int turnoId);

    /// <summary>En UNA transacción: inserta el turno y actualiza el estado del combate.</summary>
    Task GuardarTurnoAsync(Combate combate, Turno turno);

    Task ActualizarNotaAsync(Turno turno);

    /// <summary>En OTRA transacción: borra el último turno y guarda el combate restaurado.</summary>
    Task EliminarUltimoTurnoAsync(Combate combate, Turno turno);
}
