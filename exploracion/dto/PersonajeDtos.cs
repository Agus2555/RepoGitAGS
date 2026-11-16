// ─────────────────────────────────────────────────────────────────────────────
// Indagación - Actividad 2: DTO entre la API y el servicio.
// Un DTO (Data Transfer Object) es una clase "tonta", solo datos, que define
// QUÉ entra y QUÉ sale de la API. No tiene reglas ni comportamiento.
// ─────────────────────────────────────────────────────────────────────────────
namespace Exploracion.Dto;

/// <summary>DTO de ENTRADA: lo que manda el cliente para crear un personaje.</summary>
public record CrearPersonajeDto(string? Nombre, string? Tipo);

/// <summary>DTO de SALIDA: lo único que le devolvemos al cliente (no la entidad entera).</summary>
public record PersonajeCreadoDto(int Id, string Nombre);
