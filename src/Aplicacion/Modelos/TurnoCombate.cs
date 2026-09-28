namespace Aplicacion.Modelos;

/// <summary>Registro inmutable de lo que ocurrió en un turno.</summary>
public class TurnoCombate
{
    public int Numero { get; }
    public string NombreActor { get; }
    public string NombreObjetivo { get; }
    public int Danio { get; }
    public string NombreAccion { get; }
    public bool ActorVivo { get; }
    public bool ObjetivoVivo { get; }

    public TurnoCombate(
        int numero,
        string nombreActor,
        string nombreObjetivo,
        int danio,
        string nombreAccion,
        bool actorVivo,
        bool objetivoVivo)
    {
        Numero = numero;
        NombreActor = nombreActor;
        NombreObjetivo = nombreObjetivo;
        Danio = danio;
        NombreAccion = nombreAccion;
        ActorVivo = actorVivo;
        ObjetivoVivo = objetivoVivo;
    }
}
