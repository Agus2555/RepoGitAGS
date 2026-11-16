namespace Persistencia.Entidades;

/// <summary>
/// Único lugar donde se conocen las clases concretas de personaje.
/// Se usa solo para CREAR personajes (POST de la API) o RECONSTRUIRLOS desde la BD.
/// La ejecución del turno nunca pregunta por la clase: eso lo resuelve el polimorfismo.
/// </summary>
public static class FabricaPersonajes
{
    /// <summary>
    /// Crea la subclase correcta según el tipo, con las estadísticas por defecto de esa clase.
    /// No distingue mayúsculas ("mago" y "Mago" sirven igual).
    /// Si id > 0 (lectura desde BD), lo asigna al objeto creado.
    /// </summary>
    public static Personaje Crear(string? tipo, string? nombre, int id = 0)
    {
        Personaje personaje = tipo?.Trim().ToLower() switch
        {
            "guerrero" => new Guerrero(nombre!),
            "mago"     => new Mago(nombre!),
            "arquero"  => new Arquero(nombre!),
            "asesino"  => new Asesino(nombre!),
            _          => throw new ArgumentException(
                              $"Tipo de personaje inválido: '{tipo}'. Usá Guerrero, Mago, Arquero o Asesino.")
        };

        if (id > 0)
            personaje.AsignarId(id);

        return personaje;
    }
}
