namespace Persistencia.Repositorios;

public interface IBatallaRepository
{
    /// <summary>Llama a sp_RegistrarBatalla (también inserta los 2 participantes con estado inicial).</summary>
    Task<int> RegistrarBatallaAsync(int personaje1Id, int personaje2Id);

    /// <summary>Llama a sp_RegistrarTurnoYActualizarDanio (inserta turno y acumula daño en ParticipantesBatalla).</summary>
    Task RegistrarTurnoAsync(int batallaId, int turno, int atacanteId, int defensorId, int danio, string? accion);

    /// <summary>
    /// Llama a sp_FinalizarBatalla.
    /// ganadorId = null indica empate (ambos cayeron o se alcanzó maxTurnos).
    /// </summary>
    Task FinalizarBatallaAsync(int batallaId, int? ganadorId);
}
