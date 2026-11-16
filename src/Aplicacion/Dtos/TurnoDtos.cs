namespace Aplicacion.Dtos;

// ── Entrada ─────────────────────────────────────────────────────────────────

/// <summary>POST /api/combates/{id}/turnos. Accion: "atacar" o "habilidad".</summary>
public record EjecutarTurnoDto(int AtacanteId, string? Accion);

/// <summary>PUT /api/combates/{id}/turnos/{turnoId}. Lo único modificable es la nota.</summary>
public record NotaDto(string? Nota);

// ── Salida ──────────────────────────────────────────────────────────────────

/// <summary>
/// Un turno jugado. No expone la "foto" de vidas y recursos de antes del turno:
/// eso es un dato interno que solo sirve para deshacerlo.
/// </summary>
public record TurnoDto(
    int Id,
    int Numero,
    int AtacanteId,
    int DefensorId,
    string Accion,
    int Danio,
    string? Nota);

/// <summary>Respuesta del POST de turno: el turno y cómo quedó el combate (incluido el ganador).</summary>
public record ResultadoTurnoDto(
    TurnoDto Turno,
    string Estado,
    ParticipanteDto Participante1,
    ParticipanteDto Participante2,
    int? GanadorId,
    string Resultado);
