namespace Persistencia.Entidades;

/// <summary>
/// Enumera las acciones que un personaje puede ejecutar en su turno.
/// IEstrategiaTurno (en Aplicacion) elige cuál; ServicioCombate la despacha.
/// </summary>
public enum TipoAccion
{
    Atacar,
    Defender,
    UsarHabilidad
}
