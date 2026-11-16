using System;

namespace Persistencia.Entidades
{
    public class Mago : Personaje
    {
        public int Mana { get; private set; }

        public override int Recurso => Mana;
        public override string NombreRecurso => "Maná";
        public override string NombreHabilidad => "Bola de Fuego";

        // Estadísticas por defecto: es lo que se usa cuando la API crea un Mago solo con nombre y tipo.
        public Mago(string nombre) : this(nombre, 80, 14, 4, 40) { }

        public Mago(string nombre, int vidaInicial, int ataque, int defensa, int manaInicial)
            : base(nombre, vidaInicial, ataque, defensa)
        {
            if (manaInicial < 0) throw new ArgumentException("El maná no puede ser negativo.");
            Mana = manaInicial;
        }

        // Ataque básico: proyectil mágico estándar, sin costo de maná.
        public override int Atacar(Personaje objetivo)
        {
            ValidarEstado();
            return objetivo.RecibirDanio(Ataque);
        }

        // Habilidad: Bola de Fuego. Consume 10 maná, daño doble.
        // Si no hay maná suficiente, no hace nada (devuelve 0).
        public override int UsarHabilidad(Personaje objetivo)
        {
            ValidarEstado();

            if (Mana < 10)
                return 0;

            Mana -= 10;
            return objetivo.RecibirDanio(Ataque * 2);
        }

        protected override void AsignarRecurso(int valor) => Mana = valor;
    }
}
