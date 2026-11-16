namespace Aplicacion.Dtos;

// ── Entrada ─────────────────────────────────────────────────────────────────

/// <summary>POST /api/personajes. Las estadísticas no se mandan: salen de la clase del tipo.</summary>
public record CrearPersonajeDto(string? Nombre, string? Tipo);

/// <summary>PUT /api/personajes/{id}. Lo único modificable es el nombre.</summary>
public record ActualizarPersonajeDto(string? Nombre);

// ── Salida ──────────────────────────────────────────────────────────────────

/// <summary>Datos públicos de un personaje (estadísticas base de su clase).</summary>
public record PersonajeDto(
    int Id,
    string Nombre,
    string Tipo,
    int VidaMaxima,
    int Ataque,
    int Defensa,
    string NombreRecurso,
    int RecursoInicial,
    string Habilidad);
