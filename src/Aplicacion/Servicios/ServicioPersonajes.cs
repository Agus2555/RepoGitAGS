using Aplicacion.Interfaces;
using Persistencia.Entidades;
using Persistencia.Repositorios;

namespace Aplicacion.Servicios;

public class ServicioPersonajes : IServicioPersonajes
{
    private readonly IPersonajeRepository _repository;

    public ServicioPersonajes(IPersonajeRepository repository)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    }

    public async Task<Personaje> RegistrarAsync(Personaje personaje)
    {
        if (personaje == null) throw new ArgumentNullException(nameof(personaje));
        return await _repository.RegistrarAsync(personaje);
    }

    public async Task<Personaje?> ObtenerPorIdAsync(int id)
    {
        if (id <= 0) throw new ArgumentOutOfRangeException(nameof(id), "El id debe ser mayor a cero.");
        return await _repository.ObtenerPorIdAsync(id);
    }

    public async Task<IEnumerable<Personaje>> ListarAsync() => await _repository.ListarAsync();
}
