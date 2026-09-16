using Persistencia.Entidades;
using Persistencia.Repositorios; // <-- Agregá esta línea
namespace Aplicacion.Servicios;

public class GestorCombate
{
    private readonly BatallaRepository _repository;

    // Aplicamos inyección de dependencias por constructor
    public GestorCombate(BatallaRepository repository)
    {
        _repository = repository;
    }

    public async Task IniciarBatalla(Personaje p1, Personaje p2)
    {
        Console.WriteLine($"\n--- INICIANDO COMBATE: {p1.Nombre} vs {p2.Nombre} ---");
        
        // 1. Registramos la batalla en la BD y obtenemos el ID
        int batallaId = await _repository.RegistrarBatallaAsync(1, 2); // IDs de ejemplo

        // Acá iría el bucle de turnos (while p1.EstaVivo && p2.EstaVivo)
        // ...

        Console.WriteLine("Combate finalizado.");
    }
}

