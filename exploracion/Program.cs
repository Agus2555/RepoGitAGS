// ─────────────────────────────────────────────────────────────────────────────
// Indagación - Actividad 1: probar antes de construir.
// Prueba de concepto independiente del proyecto final:
//   1) una ruta GET que devuelve un objeto fijo y se ejecuta desde Swagger,
//   2) Guerrero y Mago respondiendo a la MISMA acción sin preguntar su tipo,
//   3) (Actividad 2) un POST que usa los DTO y el servicio en memoria de la carpeta dto/.
// Ejecutar:  dotnet run --project exploracion   →   http://localhost:5000/swagger
// ─────────────────────────────────────────────────────────────────────────────
using Exploracion.Dto;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();            // hace que Swagger "vea" las rutas de Minimal API
builder.Services.AddSwaggerGen();                      // genera el documento OpenAPI (swagger.json)
builder.Services.AddSingleton<ServicioPersonajes>();   // Actividad 2: servicio con lista en memoria

var app = builder.Build();

app.UseSwagger();     // publica /swagger/v1/swagger.json
app.UseSwaggerUI();   // publica la interfaz en /swagger

// 1) Ruta GET que devuelve un objeto fijo.
//    .Produces<T>(200) le dice a Swagger qué devuelve y con qué código.
app.MapGet("/api/prueba", () => Results.Ok(new PersonajeFijo(1, "Thorin", "Guerrero")))
   .WithSummary("Devuelve un personaje fijo")
   .Produces<PersonajeFijo>(200);

// 2) Polimorfismo: a los dos se les pide lo mismo (Atacar) y cada uno responde a su manera.
//    No hay ningún if ni switch preguntando "¿sos Guerrero o Mago?".
app.MapGet("/api/prueba/polimorfismo", () =>
{
    List<PersonajePrueba> personajes = [new GuerreroPrueba("Thorin"), new MagoPrueba("Gandalf")];

    var respuestas = personajes
        .Select(p => new RespuestaAccion(p.Nombre, p.GetType().Name, p.Atacar()))
        .ToList();

    return Results.Ok(respuestas);
})
   .WithSummary("Guerrero y Mago responden a la misma acción")
   .Produces<List<RespuestaAccion>>(200);

// 3) Actividad 2: entra un DTO, el servicio trabaja con la entidad y sale OTRO DTO.
app.MapPost("/api/prueba/personajes", (CrearPersonajeDto dto, ServicioPersonajes servicio) =>
{
    if (string.IsNullOrWhiteSpace(dto.Nombre) || string.IsNullOrWhiteSpace(dto.Tipo))
        return Results.BadRequest("El nombre y el tipo son obligatorios.");

    PersonajeCreadoDto creado = servicio.Crear(dto);
    return Results.Created($"/api/prueba/personajes/{creado.Id}", creado);
})
   .WithSummary("Crea un personaje en memoria usando DTOs")
   .Produces<PersonajeCreadoDto>(201)
   .Produces<string>(400);

app.Run();

// ── Tipos de la prueba ───────────────────────────────────────────────────────

record PersonajeFijo(int Id, string Nombre, string Tipo);

record RespuestaAccion(string Nombre, string Clase, string Resultado);

abstract class PersonajePrueba(string nombre)
{
    public string Nombre { get; } = nombre;

    // Cada subclase está obligada a decir cómo ataca.
    public abstract string Atacar();
}

class GuerreroPrueba(string nombre) : PersonajePrueba(nombre)
{
    public override string Atacar() => $"{Nombre} golpea con la espada: 20 de daño y junta 10 de furia.";
}

class MagoPrueba(string nombre) : PersonajePrueba(nombre)
{
    public override string Atacar() => $"{Nombre} lanza un proyectil mágico: 14 de daño sin gastar maná.";
}
