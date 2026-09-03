using System;

namespace Persistencia.Entidades
{
    public class Guerrero : Personaje
    {
        public int Furia { get; private set; }

        public Guerrero(string nombre, int vidaInicial, int ataque, int defensa, int furiaInicial) 
            : base(nombre, vidaInicial, ataque, defensa)
        {
            if (furiaInicial < 0) throw new ArgumentException("La furia no puede ser negativa."); // Regla Actividad 6
            Furia = furiaInicial;
        }

        public override void Atacar(Personaje objetivo)
        {
            ValidarEstado(); // Regla Actividad 6: no puede atacar si está derrotado

            int danio = Ataque;
            if (Furia >= 20)
            {
                danio += 15; // Daño extra por furia
                Furia -= 20;
            }
            else
            {
                Furia += 10; // Genera furia con ataques normales
            }
            
            objetivo.RecibirDanio(danio);
        }
    }
}