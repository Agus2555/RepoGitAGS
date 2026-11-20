using Aplicacion.Dtos;
using Aplicacion.Excepciones;
using Aplicacion.Interfaces;
using Aplicacion.Servicios;
using Persistencia;
using Persistencia.Repositorios;

var builder = WebApplication.CreateBuilder(args);

// ── Conexiones ──────────────────────────────────────────────────────────────
// Se leen de variables de entorno (ConnectionStrings__Invitado y ConnectionStrings__Administrador),
// así las contraseñas nunca quedan escritas en el repositorio. Ver README.
string? conexionInvitado = builder.Configuration.GetConnectionString("Invitado");
string? conexionAdministrador = builder.Configuration.GetConnectionString("Administrador");

if (string.IsNullOrWhiteSpace(conexionInvitado) || string.IsNullOrWhiteSpace(conexionAdministrador))
    throw new InvalidOperationException(
        "Faltan las variables de entorno ConnectionStrings__Invitado y/o ConnectionStrings__Administrador (ver README).");

// ── Inyección de dependencias ───────────────────────────────────────────────
// Singleton: las cadenas de conexión no cambian, alcanza con una fábrica para toda la app.
builder.Services.AddSingleton<IDbConnectionFactory>(new MySqlConnectionFactory(conexionInvitado, conexionAdministrador));

// Scoped: una instancia nueva por cada solicitud HTTP.
builder.Services.AddScoped<IPersonajeRepository, PersonajeRepository>();
builder.Services.AddScoped<ICombateRepository, CombateRepository>();
builder.Services.AddScoped<ITurnoRepository, TurnoRepository>();
builder.Services.AddScoped<IServicioPersonajes, ServicioPersonajes>();
builder.Services.AddScoped<IServicioCombates, ServicioCombates>();
builder.Services.AddScoped<IServicioTurnos, ServicioTurnos>();

// ── Swagger ─────────────────────────────────────────────────────────────────
builder.Services.AddEndpointsApiExplorer();   // hace que Swagger "vea" las rutas de Minimal API
builder.Services.AddSwaggerGen(opciones =>    // genera el documento OpenAPI (swagger.json)
    opciones.SwaggerDoc("v1", new() { Title = "Combate por API", Version = "v1" }));

var app = builder.Build();

app.UseSwagger();     // publica /swagger/v1/swagger.json
app.UseSwaggerUI();   // publica la interfaz en /swagger

// ── Manejo de errores ───────────────────────────────────────────────────────
// Los servicios lanzan excepciones propias y acá se traducen al código HTTP.
app.Use(async (contexto, siguiente) =>
{
    try
    {
        await siguiente(contexto);
    }
    catch (DatosInvalidosException ex)  { await ResponderError(contexto, 400, ex.Message); }
    catch (BadHttpRequestException)     { await ResponderError(contexto, 400, "El cuerpo de la solicitud no es un JSON válido."); }
    catch (NoEncontradoException ex)    { await ResponderError(contexto, 404, ex.Message); }
    catch (ConflictoException ex)       { await ResponderError(contexto, 409, ex.Message); }
});

// ── Personajes ──────────────────────────────────────────────────────────────
var personajes = app.MapGroup("/api/personajes").WithTags("Personajes");

personajes.MapGet("", async (IServicioPersonajes servicio) =>
        Results.Ok(await servicio.ListarAsync()))
    .WithSummary("Lista todos los personajes")
    .Produces<IEnumerable<PersonajeDto>>(200);

personajes.MapGet("/{id:int}", async (int id, IServicioPersonajes servicio) =>
        Results.Ok(await servicio.ObtenerAsync(id)))
    .WithSummary("Obtiene un personaje")
    .Produces<PersonajeDto>(200)
    .Produces<ErrorDto>(404);

personajes.MapGet("/{id:int}/combates", async (int id, IServicioCombates servicio) =>
        Results.Ok(await servicio.ListarPorPersonajeAsync(id)))
    .WithSummary("Combates en los que participó el personaje (lista vacía si no participó)")
    .Produces<IEnumerable<CombateResumenDto>>(200)
    .Produces<ErrorDto>(404);

personajes.MapPost("", async (CrearPersonajeDto dto, IServicioPersonajes servicio) =>
    {
        var creado = await servicio.CrearAsync(dto);
        return Results.Created($"/api/personajes/{creado.Id}", creado);
    })
    .WithSummary("Crea un personaje con nombre y tipo (Guerrero, Mago, Arquero o Asesino)")
    .Produces<PersonajeDto>(201)
    .Produces<ErrorDto>(400);

personajes.MapPut("/{id:int}", async (int id, ActualizarPersonajeDto dto, IServicioPersonajes servicio) =>
        Results.Ok(await servicio.ActualizarNombreAsync(id, dto)))
    .WithSummary("Cambia el nombre del personaje")
    .Produces<PersonajeDto>(200)
    .Produces<ErrorDto>(400)
    .Produces<ErrorDto>(404);

personajes.MapDelete("/{id:int}", async (int id, IServicioPersonajes servicio) =>
    {
        await servicio.EliminarAsync(id);
        return Results.NoContent();
    })
    .WithSummary("Elimina un personaje que no participó en combates")
    .Produces(204)
    .Produces<ErrorDto>(404)
    .Produces<ErrorDto>(409);

// ── Combates ────────────────────────────────────────────────────────────────
var combates = app.MapGroup("/api/combates").WithTags("Combates");

combates.MapGet("", async (IServicioCombates servicio) =>
        Results.Ok(await servicio.ListarAsync()))
    .WithSummary("Lista los combates con sus participantes y estado")
    .Produces<IEnumerable<CombateResumenDto>>(200);

combates.MapPost("", async (ParticipantesDto dto, IServicioCombates servicio) =>
    {
        var creado = await servicio.CrearAsync(dto);
        return Results.Created($"/api/combates/{creado.Id}", creado);
    })
    .WithSummary("Inicia un combate entre dos personajes distintos")
    .Produces<CombateDetalleDto>(201)
    .Produces<ErrorDto>(400)
    .Produces<ErrorDto>(404);

combates.MapGet("/{id:int}", async (int id, IServicioCombates servicio) =>
        Results.Ok(await servicio.ObtenerAsync(id)))
    .WithSummary("Combate con participantes, turnos y resultado")
    .Produces<CombateDetalleDto>(200)
    .Produces<ErrorDto>(404);

combates.MapPut("/{id:int}", async (int id, ParticipantesDto dto, IServicioCombates servicio) =>
        Results.Ok(await servicio.ActualizarParticipantesAsync(id, dto)))
    .WithSummary("Cambia los dos participantes (solo antes del primer turno)")
    .Produces<CombateDetalleDto>(200)
    .Produces<ErrorDto>(400)
    .Produces<ErrorDto>(404)
    .Produces<ErrorDto>(409);

combates.MapDelete("/{id:int}", async (int id, IServicioCombates servicio) =>
    {
        await servicio.EliminarAsync(id);
        return Results.NoContent();
    })
    .WithSummary("Elimina un combate sin turnos")
    .Produces(204)
    .Produces<ErrorDto>(404)
    .Produces<ErrorDto>(409);

// ── Turnos ──────────────────────────────────────────────────────────────────
var turnos = app.MapGroup("/api/combates/{id:int}/turnos").WithTags("Turnos");

turnos.MapGet("", async (int id, IServicioTurnos servicio) =>
        Results.Ok(await servicio.ListarAsync(id)))
    .WithSummary("Turnos del combate en orden de ejecución")
    .Produces<IEnumerable<TurnoDto>>(200)
    .Produces<ErrorDto>(404);

turnos.MapPost("", async (int id, EjecutarTurnoDto dto, IServicioTurnos servicio) =>
        Results.Ok(await servicio.EjecutarAsync(id, dto)))
    .WithSummary("Ejecuta un turno: atacanteId y accion ('atacar' o 'habilidad')")
    .Produces<ResultadoTurnoDto>(200)
    .Produces<ErrorDto>(400)
    .Produces<ErrorDto>(404);

turnos.MapPut("/{turnoId:int}", async (int id, int turnoId, NotaDto dto, IServicioTurnos servicio) =>
        Results.Ok(await servicio.ActualizarNotaAsync(id, turnoId, dto)))
    .WithSummary("Modifica solo la nota del turno")
    .Produces<TurnoDto>(200)
    .Produces<ErrorDto>(400)
    .Produces<ErrorDto>(404);

turnos.MapDelete("/{turnoId:int}", async (int id, int turnoId, IServicioTurnos servicio) =>
    {
        await servicio.EliminarUltimoAsync(id, turnoId);
        return Results.NoContent();
    })
    .WithSummary("Borra el último turno y restaura el estado previo del combate")
    .Produces(204)
    .Produces<ErrorDto>(404)
    .Produces<ErrorDto>(409);

app.Run();

// Escribe el cuerpo de error { "mensaje": "..." } con el código indicado.
static async Task ResponderError(HttpContext contexto, int codigo, string mensaje)
{
    contexto.Response.StatusCode = codigo;
    await contexto.Response.WriteAsJsonAsync(new ErrorDto(mensaje));
}
