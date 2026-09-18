using System;
using System.Threading.Tasks;
using Aplicacion.Servicios;
using Persistencia.Entidades;
using Persistencia.Repositorios;

class Program
{
    static async Task Main(string[] args)
    {
        // 1. Instanciamos la conexión a la base de datos
        string connectionString = "Server=localhost;Database=combates_db;Uid=root;Pwd=tu_contraseña;";
        var repository = new BatallaRepository(connectionString);
        var gestor = new GestorCombate(repository);

        // 2. Creamos dos combatientes con datos de prueba
        Personaje p1 = new Guerrero("Thorin", vidaInicial: 100, ataque: 25, defensa: 10, furiaInicial: 10);
        Personaje p2 = new Guerrero("Ragnar", vidaInicial: 90, ataque: 22, defensa: 8, furiaInicial: 20);

        // 3. Arrancamos el combate
        await gestor.IniciarBatalla(p1, p2);
    }
}