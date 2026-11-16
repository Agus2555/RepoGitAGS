using System;

namespace Persistencia.Entidades
{
    public class Arquero : Personaje
    {
        public int Flechas { get; private set; }

        public override int Recurso => Flechas;
        public override string NombreRecurso => "Flechas";
        public override string NombreHabilidad => "Lluvia de Flechas";

        // Estadísticas por defecto: es lo que se usa cuando la API crea un Arquero solo con nombre y tipo.
        public Arquero(string nombre) : this(nombre, 100, 16, 6, 10) { }

        public Arquero(string nombre, int vidaInicial, int ataque, int defensa, int flechasIniciales)
            : base(nombre, vidaInicial, ataque, defensa)
        {
            if (flechasIniciales < 0) throw new ArgumentException("La cantidad de flechas no puede ser negativa.");
            Flechas = flechasIniciales;
        }

        // Ataque básico: si tiene flechas, disparo de precisión (+10); si no, ataque cuerpo a cuerpo reducido.
        public override int Atacar(Personaje objetivo)
        {
            ValidarEstado();

            if (Flechas > 0)
            {
                Flechas--;
                return objetivo.RecibirDanio(Ataque + 10);
            }
            else
            {
                return objetivo.RecibirDanio(Ataque / 2);
            }
        }

        // Habilidad: Lluvia de Flechas. Consume 3 flechas, daño doble.
        public override int UsarHabilidad(Personaje objetivo)
        {
            ValidarEstado();

            if (Flechas < 3)
                return 0;

            Flechas -= 3;
            return objetivo.RecibirDanio(Ataque * 2);
        }

        protected override void AsignarRecurso(int valor) => Flechas = valor;
    }
}
