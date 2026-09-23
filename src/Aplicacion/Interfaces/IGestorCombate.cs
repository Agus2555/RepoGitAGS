using System.Threading.Tasks;
using Persistencia.Entidades;

namespace Aplicacion.Interfaces;

public interface IGestorCombate
{
    Task IniciarBatalla(Personaje p1, Personaje p2);
}