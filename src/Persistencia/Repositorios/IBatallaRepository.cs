using System.Threading.Tasks;

namespace Persistencia.Repositorios;

public interface IBatallaRepository
{
    Task<int> RegistrarBatallaAsync(int personaje1Id, int personaje2Id);
    Task RegistrarTurnoAsync(int batallaId, int turno, int atacanteId, int defensorId, int danio, string? habilidadUsada);
    Task FinalizarBatallaAsync(int batallaId, int ganadorId);
}