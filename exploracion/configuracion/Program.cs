// ─────────────────────────────────────────────────────────────────────────────
// Indagación - Actividad 3: registrar dependencias y mostrar Swagger.
// Ejecutar:  dotnet run --project exploracion/configuracion   →   http://localhost:5000/swagger
// ─────────────────────────────────────────────────────────────────────────────
using Configuracion;

var builder = WebApplication.CreateBuilder(args);

// Registro de dependencias: "cuando alguien pida la interfaz, dale esta clase".
// Singleton → una sola instancia para toda la app (la lista en memoria no se pierde entre requests).
builder.Services.AddSingleton<IRepositorioPersonajes, RepositorioPersonajesMemoria>();
// Scoped → una instancia nueva por cada request HTTP.
builder.Services.AddScoped<IServicioPersonajes, ServicioPersonajes>();

// Swagger: estas dos líneas generan el documento OpenAPI...
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// ...y estas dos lo publican: /swagger/v1/swagger.json y la interfaz en /swagger.
app.UseSwagger();
app.UseSwaggerUI();

// Minimal API inyecta el servicio como parámetro de la ruta: la ruta no hace "new" de nada.
app.MapGet("/api/personajes", (IServicioPersonajes servicio) => Results.Ok(servicio.Listar()))
   .WithSummary("Lista personajes desde el repositorio en memoria")
   .Produces<IEnumerable<PersonajeResumen>>(200);

app.MapGet("/api/personajes/{id:int}", (int id, IServicioPersonajes servicio) =>
{
    var personaje = servicio.Obtener(id);
    return personaje == null ? Results.NotFound() : Results.Ok(personaje);
})
   .WithSummary("Obtiene un personaje (404 si no existe)")
   .Produces<PersonajeResumen>(200)
   .Produces(404);

app.Run();
