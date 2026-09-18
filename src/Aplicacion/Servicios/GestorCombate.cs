using System;
using System.Threading.Tasks;
using Persistencia.Entidades;
using Persistencia.Repositorios;

namespace Aplicacion.Servicios;

public class GestorCombate
{
    private readonly BatallaRepository _repository;

    public GestorCombate(BatallaRepository repository)
    {
        _repository = repository;
    }

    public async Task IniciarBatalla(Personaje p1, Personaje p2)
    {
        Console.WriteLine($"\n--- INICIANDO COMBATE: {p1.Nombre} vs {p2.Nombre} ---");

        // 1. Registramos el inicio de la batalla en la BD y obtenemos su ID (SP)
        int batallaId = await _repository.RegistrarBatallaAsync(p1.Id, p2.Id);

        int turno = 1;

        // 2. Bucle de combate por turnos
        while (p1.EstaVivo && p2.EstaVivo)
        {
            Console.WriteLine($"\n--- Turno {turno} ---");

            // --- Ataque de P1 a P2 ---
            int vidaPreviaP2 = p2.Vida;
            p1.Atacar(p2);
            int danioP1 = vidaPreviaP2 - p2.Vida;

            Console.WriteLine($"{p1.Nombre} ataca a {p2.Nombre} y le inflige {danioP1} de daño. (Vida {p2.Nombre}: {p2.Vida})");

            // Persistimos el turno en la BD
            await _repository.RegistrarTurnoAsync(batallaId, turno, p1.Id, p2.Id, danioP1, "Ataque Básico");

            if (!p2.EstaVivo)
            {
                Console.WriteLine($"\n🏆 ¡{p2.Nombre} cayó! ¡{p1.Nombre} es el ganador!");
                await _repository.FinalizarBatallaAsync(batallaId, p1.Id);
                break;
            }

            // --- Ataque de P2 a P1 ---
            int vidaPreviaP1 = p1.Vida;
            p2.Atacar(p1);
            int danioP2 = vidaPreviaP1 - p1.Vida;

            Console.WriteLine($"{p2.Nombre} ataca a {p1.Nombre} y le inflige {danioP2} de daño. (Vida {p1.Nombre}: {p1.Vida})");

            // Persistimos el turno en la BD
            await _repository.RegistrarTurnoAsync(batallaId, turno, p2.Id, p1.Id, danioP2, "Ataque Básico");

            if (!p1.EstaVivo)
            {
                Console.WriteLine($"\n🏆 ¡{p1.Nombre} cayó! ¡{p2.Nombre} es el ganador!");
                await _repository.FinalizarBatallaAsync(batallaId, p2.Id);
                break;
            }

            turno++;
        }

        Console.WriteLine("\nCombate finalizado y registrado con éxito.");
    }
}