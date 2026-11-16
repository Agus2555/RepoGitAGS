using System;

namespace Persistencia.Entidades
{
    // El daguero: letal pero frágil. Depende de su energía para los golpes críticos.
    public class Asesino : Personaje
    {
        public int Energia { get; private set; }

        public override int Recurso => Energia;
        public override string NombreRecurso => "Energía";
        public override string NombreHabilidad => "Puñalada Letal";

        // Estadísticas por defecto: es lo que se usa cuando la API crea un Asesino solo con nombre y tipo.
        public Asesino(string nombre) : this(nombre, 90, 15, 5, 30) { }

        public Asesino(string nombre, int vidaInicial, int ataque, int defensa, int energiaInicial)
            : base(nombre, vidaInicial, ataque, defensa)
        {
            if (energiaInicial < 0) throw new ArgumentException("La energía no puede ser negativa.");
            Energia = energiaInicial;
        }

        // Ataque básico: con energía hace un golpe crítico (triple);
        // sin energía golpea normal y recupera 5.
        public override int Atacar(Personaje objetivo)
        {
            ValidarEstado();

            if (Energia >= 15)
            {
                Energia -= 15;
                return objetivo.RecibirDanio(Ataque * 3);
            }
            else
            {
                Energia += 5; // recupera energía con ataques básicos
                return objetivo.RecibirDanio(Ataque);
            }
        }

        // Habilidad: Puñalada Letal. Consume 15 de energía, daño triple.
        // A diferencia del ataque, si no hay energía no hace nada (devuelve 0).
        public override int UsarHabilidad(Personaje objetivo)
        {
            ValidarEstado();

            if (Energia < 15)
                return 0;

            Energia -= 15;
            return objetivo.RecibirDanio(Ataque * 3);
        }

        protected override void AsignarRecurso(int valor) => Energia = valor;
    }
}
