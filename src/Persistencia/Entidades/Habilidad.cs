using System;

namespace Persistencia.Entidades
{
    public class Habilidad
    {
        public int Id { get; private set; }
        public string Nombre { get; private set; }
        public int Costo { get; private set; }
        public int Poder { get; private set; }

        public Habilidad(string nombre, int costo, int poder)
        {
            if (string.IsNullOrWhiteSpace(nombre)) throw new ArgumentException("La habilidad debe tener un nombre válido.");
            if (costo < 0) throw new ArgumentException("El costo de uso no puede ser negativo.");
            
            Nombre = nombre;
            Costo = costo;
            Poder = poder;
        }
    }
}