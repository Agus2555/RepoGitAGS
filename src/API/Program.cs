using Aplicacion.Interfaces;
using Aplicacion.Servicios;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Persistencia;
using Persistencia.Entidades;
using Persistencia.Repositorios;

// ── 1. Inicialización del Web Application Builder ───────────────────────────
var builder = WebApplication.CreateBuilder(args);

// Reutilizamos tu configuración de las cadenas de conexión desde appsettings.json
string desarrolloCs = builder.Configuration.GetConnectionString("Desarrollo")
    ?? throw new InvalidOperationException("Falta ConnectionStrings:Desarrollo en appsettings.json.");
string adminCs = builder.Configuration.GetConnectionString("Administrador")
    ?? throw new InvalidOperationException("Falta ConnectionStrings:Administrador en appsettings.json.");

// ── 2. Contenedor de DI (Inyección de Dependencias) ─────────────────────────
// Infraestructura
builder.Services.AddSingleton<IDbConnectionFactory>(_ => new MySqlConnectionFactory(desarrolloCs, adminCs));

// Repositorios
builder.Services.AddScoped<IPersonajeRepository, PersonajeRepository>();
builder.Services.AddScoped<IBatallaRepository,   BatallaRepository>();
builder.Services.AddScoped<IAdministracionRepository, AdministracionRepository>();

// Servicios de aplicación
builder.Services.AddScoped<IEstrategiaTurno,   EstrategiaTurnoPorDefecto>();
builder.Services.AddScoped<IServicioCombate,   ServicioCombate>();
builder.Services.AddScoped<IServicioPersonajes, ServicioPersonajes>();
builder.Services.AddScoped<IServicioBatallas,  ServicioBatallas>();

// Configuración requerida para que funcione Swagger / OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// ── 3. Configuración del Pipeline HTTP (Middlewares) ────────────────────────
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// ── 4. Verificación de Inicialización de Base de Datos ──────────────────────
// Mantenemos tu chequeo de consola inicial usando un Scope temporal
using (var scope = app.Services.CreateScope())
{
    var adminRepo = scope.ServiceProvider.GetRequiredService<IAdministracionRepository>();
    bool bdExiste = await adminRepo.VerificarBaseDatosExisteAsync("simulador_combate");
    Console.WriteLine($"Base de datos 'simulador_combate': {(bdExiste ? "OK" : "NO ENCONTRADA")}");
    
    if (!bdExiste)
    {
        Console.WriteLine("ALERTA: Ejecute DDL.SQL → SP.SQL → ESTADISTICAS.SQL → USUARIOS.SQL en MariaDB.");
    }
}

// ── 5. Endpoints de la Minimal API (Pruebas de Concepto) ────────────────────

// ENDPOINT 1: GET - Devuelve un objeto fijo independiente (Responde 200 OK)
app.MapGet("/api/poc/personaje-fijo", () =>
{
    var personajeFijo = new PersonajeFijoDto("Aragorn Fijo", "Guerrero", 100);
    return TypedResults.Ok(personajeFijo);
})
.WithName("ObtenerPersonajeFijo")
.WithOpenApi(op => new(op) 
{ 
    Summary = "PoC independiente: Obtiene un personaje estático (GET)",
    Description = "Devuelve un objeto de datos fijos para probar el mapeo y visualización en Swagger sin afectar la base de datos."
});


// ENDPOINT 2: POST - Simula una acción aplicando POLIMORFISMO directo desde el Cuerpo (Body)
app.MapPost("/api/poc/personaje-accion", (PersonajeFijoDto body) =>
{
    if (string.IsNullOrWhiteSpace(body.Nombre) || string.IsNullOrWhiteSpace(body.Clase))
    {
        return TypedResults.BadRequest("El nombre y la clase son requeridos en el cuerpo (Body).");
    }

    // Instanciación polimórfica conceptual según la clase enviada
    string mensajeResultado = body.Clase.ToLower() switch
    {
        "guerrero" => $"{body.Nombre} blandió su espada y realizó un Ataque Torbellino. ⚔️",
        "mago"     => $"{body.Nombre} canalizó su maná y lanzó una Bola de Fuego. 🔮",
        "arquero"  => $"{body.Nombre} tensó su arco y disparó una Flecha Certera. 🏹",
        "asesino"  => $"{body.Nombre} se ocultó en las sombras y ejecutó una Puñalada. 🗡️",
        _          => null
    };

    if (mensajeResultado == null)
    {
        return TypedResults.BadRequest($"La clase '{body.Clase}' no es válida en esta PoC.");
    }

    return TypedResults.Ok(new ResultadoAccionDto(mensajeResultado));
})
.WithName("EjecutarAccionPoC")
.WithOpenApi(op => new(op) 
{ 
    Summary = "PoC independiente: Compara acciones de clases mediante el cuerpo (POST)" 
});

// ── 6. Ejecución del Servidor Web ───────────────────────────────────────────
app.Run();

// ── 7. Data Transfer Objects (DTOs) necesarios para la API ──────────────────
// Colocados al final del archivo para mantener limpio el flujo principal de la API
public record PersonajeFijoDto(string Nombre, string Clase, int Vida);
public record ResultadoAccionDto(string Mensaje);
