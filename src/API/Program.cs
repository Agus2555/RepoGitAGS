using Aplicacion.Interfaces;
using Aplicacion.Servicios;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Persistencia;
using Persistencia.Entidades;
using Persistencia.Repositorios;

// ── Configuración ───────────────────────────────────────────────────────────
var config = new ConfigurationBuilder()
    .SetBasePath(AppContext.BaseDirectory)
    .AddJsonFile("appsettings.json", optional: false)
    .Build();

string desarrolloCs = config.GetConnectionString("Desarrollo")
    ?? throw new InvalidOperationException("Falta ConnectionStrings:Desarrollo en appsettings.json.");
string adminCs = config.GetConnectionString("Administrador")
    ?? throw new InvalidOperationException("Falta ConnectionStrings:Administrador en appsettings.json.");

// ── Contenedor de DI ────────────────────────────────────────────────────────
var servicios = new ServiceCollection();

// Infraestructura: factory singleton porque las cadenas de conexión no cambian.
servicios.AddSingleton<IDbConnectionFactory>(_ => new MySqlConnectionFactory(desarrolloCs, adminCs));

// Repositorios
servicios.AddScoped<IPersonajeRepository, PersonajeRepository>();
servicios.AddScoped<IBatallaRepository,   BatallaRepository>();
servicios.AddScoped<IAdministracionRepository, AdministracionRepository>();

// Servicios de aplicación
servicios.AddScoped<IEstrategiaTurno,   EstrategiaTurnoPorDefecto>();
servicios.AddScoped<IServicioCombate,   ServicioCombate>();
servicios.AddScoped<IServicioPersonajes, ServicioPersonajes>();
servicios.AddScoped<IServicioBatallas,  ServicioBatallas>();

var sp = servicios.BuildServiceProvider();

// ── Demo ─────────────────────────────────────────────────────────────────────
// Verificar que la BD existe (usa la conexión administrador: privilegio global).
var adminRepo = sp.GetRequiredService<IAdministracionRepository>();
bool bdExiste = await adminRepo.VerificarBaseDatosExisteAsync("simulador_combate");
Console.WriteLine($"Base de datos 'simulador_combate': {(bdExiste ? "OK" : "NO ENCONTRADA")}");

if (!bdExiste)
{
    Console.WriteLine("Ejecute DDL.SQL → SP.SQL → ESTADISTICAS.SQL → USUARIOS.SQL en MariaDB y reintente.");
    return;
}

var svcPersonajes = sp.GetRequiredService<IServicioPersonajes>();
var svcBatallas   = sp.GetRequiredService<IServicioBatallas>();

// Registrar personajes con sus recursos iniciales
var guerrero = await svcPersonajes.RegistrarAsync(new Guerrero("Thorin",  100, 25, 10, 10));
var mago     = await svcPersonajes.RegistrarAsync(new Mago("Gandalf",     80,  30,  5, 50));
var arquero  = await svcPersonajes.RegistrarAsync(new Arquero("Legolas",  90,  22,  6, 15));
var asesino  = await svcPersonajes.RegistrarAsync(new Asesino("Erebus",   75,  28,  4, 30));

Console.WriteLine($"\nPersonajes registrados: {guerrero.Nombre} | {mago.Nombre} | {arquero.Nombre} | {asesino.Nombre}");

// Simular un combate
Console.WriteLine($"\n=== {guerrero.Nombre} vs {mago.Nombre} ===");
var resultado = await svcBatallas.SimularAsync(guerrero.Id, mago.Id);

Console.WriteLine($"Turnos: {resultado.TurnosJugados}");
Console.WriteLine(resultado.Ganador is not null
    ? $"Ganador: {resultado.Ganador.Nombre}"
    : "Resultado: Empate");

foreach (var turno in resultado.Turnos)
{
    Console.WriteLine(
        $"  [{turno.Numero}] {turno.NombreActor} → {turno.NombreAccion} → {turno.Danio} daño a {turno.NombreObjetivo}");
}
