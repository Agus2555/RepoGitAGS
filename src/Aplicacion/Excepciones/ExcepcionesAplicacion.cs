namespace Aplicacion.Excepciones;

// Los servicios lanzan estas excepciones y el Program.cs de la API
// las traduce al código HTTP que corresponde. Así los servicios no saben nada de HTTP.

/// <summary>El recurso pedido no existe → 404.</summary>
public class NoEncontradoException(string mensaje) : Exception(mensaje);

/// <summary>Datos inválidos o acción no permitida (fuera de turno, combate finalizado) → 400.</summary>
public class DatosInvalidosException(string mensaje) : Exception(mensaje);

/// <summary>La operación rompe una restricción (borrar algo con historial, etc.) → 409.</summary>
public class ConflictoException(string mensaje) : Exception(mensaje);
