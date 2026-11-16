namespace Aplicacion.Dtos;

// ── Entrada ─────────────────────────────────────────────────────────────────

/// <summary>POST y PUT /api/combates: los Id de los dos personajes que pelean.</summary>
public record ParticipantesDto(int Personaje1Id, int Personaje2Id);

// ── Salida ──────────────────────────────────────────────────────────────────

/// <summary>Un participante con su vida y recurso dentro de ese combate.</summary>
public record ParticipanteDto(
    int Id,
    string Nombre,
    string Tipo,
    int Vida,
    int VidaMaxima,
    string NombreRecurso,
    int Recurso);

/// <summary>Para las listas: Id, participantes y estado.</summary>
public record CombateResumenDto(
    int Id,
    ParticipanteDto Participante1,
    ParticipanteDto Participante2,
    string Estado,
    int CantidadTurnos,
    int? GanadorId);

/// <summary>GET /api/combates/{id}: combate, participantes, turnos y resultado.</summary>
public record CombateDetalleDto(
    int Id,
    string Estado,
    ParticipanteDto Participante1,
    ParticipanteDto Participante2,
    int? SiguienteAtacanteId,
    int? GanadorId,
    string Resultado,
    IEnumerable<TurnoDto> Turnos);
