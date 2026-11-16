using Aplicacion.Dtos;
using Persistencia.Entidades;

namespace Aplicacion.Servicios;

/// <summary>
/// Único lugar donde las entidades del dominio se transforman en DTOs de respuesta.
/// Los servicios devuelven siempre DTOs: la API nunca expone una entidad directamente.
/// </summary>
internal static class Mapeo
{
    public static PersonajeDto APersonajeDto(Personaje p) =>
        new(p.Id, p.Nombre, p.Tipo, p.VidaMaxima, p.Ataque, p.Defensa, p.NombreRecurso, p.Recurso, p.NombreHabilidad);

    public static ParticipanteDto AParticipanteDto(Personaje p) =>
        new(p.Id, p.Nombre, p.Tipo, p.Vida, p.VidaMaxima, p.NombreRecurso, p.Recurso);

    public static TurnoDto ATurnoDto(Turno t) =>
        new(t.Id, t.Numero, t.AtacanteId, t.DefensorId, t.Accion, t.Danio, t.Nota);

    public static CombateResumenDto ACombateResumenDto(Combate c) =>
        new(c.Id,
            AParticipanteDto(c.Participante1),
            AParticipanteDto(c.Participante2),
            c.Estado.ToString(),
            c.CantidadTurnos,
            c.GanadorId);

    public static CombateDetalleDto ACombateDetalleDto(Combate c, IEnumerable<Turno> turnos) =>
        new(c.Id,
            c.Estado.ToString(),
            AParticipanteDto(c.Participante1),
            AParticipanteDto(c.Participante2),
            c.Estado == EstadoCombate.EnCurso ? c.AtacanteDeTurno.Id : null,
            c.GanadorId,
            Resultado(c),
            turnos.Select(ATurnoDto).ToList());

    public static ResultadoTurnoDto AResultadoTurnoDto(Combate c, Turno t) =>
        new(ATurnoDto(t),
            c.Estado.ToString(),
            AParticipanteDto(c.Participante1),
            AParticipanteDto(c.Participante2),
            c.GanadorId,
            Resultado(c));

    // Texto corto para que el cliente vea cómo va el combate.
    private static string Resultado(Combate c) =>
        c.Ganador != null
            ? $"Ganó {c.Ganador.Nombre}"
            : $"En curso: le toca a {c.AtacanteDeTurno.Nombre}";
}
