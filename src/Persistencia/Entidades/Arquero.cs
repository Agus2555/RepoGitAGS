using System;

namespace Persistencia.Entidades
{
    public class Arquero : Personaje
    {
        public int Flechas { get; private set; }

        public Arquero(string nombre, int vidaInicial, int ataque, int defensa, int flechasIniciales) 
            : base(nombre, vidaInicial, ataque, defensa)
        {
            if (flechasIniciales < 0) throw new ArgumentException("La cantidad de flechas no puede ser negativa.");
            Flechas = flechasIniciales;
        }

        public override void Atacar(Personaje objetivo)
        {
            ValidarEstado();

            if (Flechas > 0)
            {
                Flechas--;
                // El arquero ataca a distancia buscando puntos débiles (ignora parte de la defensa)
                int danioCritico = Ataque + 10; 
                objetivo.RecibirDanio(danioCritico);
            }
            else
            {
                // Ataque desesperado cuerpo a cuerpo si se queda sin flechas
                objetivo.RecibirDanio(Ataque / 2);
            }
        }
    }
}