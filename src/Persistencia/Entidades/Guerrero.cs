using System;

namespace Persistencia.Entidades
{
    public class Guerrero : Personaje
    {
        public int Furia { get; private set; }

        // Recurso expuesto con nombre propio para el dominio;
        // Personaje.Recurso lo reutiliza para la BD y las respuestas de la API.
        public override int Recurso => Furia;
        public override string NombreRecurso => "Furia";
        public override string NombreHabilidad => "Golpe Devastador";

        // Estadísticas por defecto: es lo que se usa cuando la API crea un Guerrero solo con nombre y tipo.
        public Guerrero(string nombre) : this(nombre, 120, 20, 8, 0) { }

        public Guerrero(string nombre, int vidaInicial, int ataque, int defensa, int furiaInicial)
            : base(nombre, vidaInicial, ataque, defensa)
        {
            if (furiaInicial < 0) throw new ArgumentException("La furia no puede ser negativa.");
            Furia = furiaInicial;
        }

        // Ataque básico: si hay furia, la consume para un golpe extra;
        // si no, genera furia para el siguiente turno.
        public override int Atacar(Personaje objetivo)
        {
            ValidarEstado();

            int danio = Ataque;
            if (Furia >= 20)
            {
                danio += 15;
                Furia -= 20;
            }
            else
            {
                Furia += 10;
            }

            return objetivo.RecibirDanio(danio);
        }

        // Habilidad: Golpe Devastador. Consume 20 de furia, daño 1.5× el ataque base.
        // Si no hay furia suficiente, no hace nada (devuelve 0).
        public override int UsarHabilidad(Personaje objetivo)
        {
            ValidarEstado();

            if (Furia < 20)
                return 0;

            Furia -= 20;
            int danio = Ataque + Ataque / 2; // ×1.5 en enteros
            return objetivo.RecibirDanio(danio);
        }

        protected override void AsignarRecurso(int valor) => Furia = valor;
    }
}
