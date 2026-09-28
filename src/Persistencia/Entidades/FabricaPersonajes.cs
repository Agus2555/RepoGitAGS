namespace Persistencia.Entidades;

/// <summary>
/// Único lugar donde se conocen las clases concretas de personaje.
/// Los repositorios la usan al reconstruir objetos desde la BD,
/// sin dispersar switch/if en toda la capa de acceso a datos.
/// </summary>
public static class FabricaPersonajes
{
    /// <summary>
    /// Crea la subclase correcta según tipoClase.
    /// Si id > 0 (lectura desde BD), lo asigna al objeto creado.
    /// </summary>
    public static Personaje Crear(
        string tipoClase,
        string nombre,
        int vidaInicial,
        int ataque,
        int defensa,
        int recurso,
        int id = 0)
    {
        Personaje personaje = tipoClase switch
        {
            "Guerrero" => new Guerrero(nombre, vidaInicial, ataque, defensa, recurso),
            "Mago"     => new Mago(nombre, vidaInicial, ataque, defensa, recurso),
            "Arquero"  => new Arquero(nombre, vidaInicial, ataque, defensa, recurso),
            "Asesino"  => new Asesino(nombre, vidaInicial, ataque, defensa, recurso),
            _          => throw new ArgumentException($"Tipo de clase no reconocido: '{tipoClase}'.")
        };

        if (id > 0)
            personaje.AsignarId(id);

        return personaje;
    }
}
