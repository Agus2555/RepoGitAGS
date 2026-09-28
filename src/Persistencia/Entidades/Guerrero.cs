using System;

namespace Persistencia.Entidades
{
    public class Guerrero : Personaje
    {
        public int Furia { get; private set; }

        // Recurso expuesto con nombre propio para el dominio;
        // Personaje.Recurso lo reutiliza para la BD (columna RecursoEspecial).
        public override int Recurso => Furia;
        public override string NombreHabilidad => "Golpe Devastador";

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

        // Defender genera furia pasivamente: cuanto más aguanta, más fuerte golpea.
        public override int Defender()
        {
            ValidarEstado();
            Furia += 5;
            return 0;
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
    }
}
