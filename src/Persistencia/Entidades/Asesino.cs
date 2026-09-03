using System;

namespace Persistencia.Entidades
{
    // Representa al Asesino (el daguero 🔪)[cite: 1]
    public class Asesino : Personaje
    {
        public int Energia { get; private set; }

        public Asesino(string nombre, int vidaInicial, int ataque, int defensa, int energiaInicial) 
            : base(nombre, vidaInicial, ataque, defensa)
        {
            if (energiaInicial < 0) throw new ArgumentException("La energía no puede ser negativa."); // Regla Actividad 6[cite: 1]
            Energia = energiaInicial;
        }

        public override void Atacar(Personaje objetivo)
        {
            ValidarEstado();

            if (Energia >= 15)
            {
                Energia -= 15;
                // Ataque letal con daga que multiplica el daño
                objetivo.RecibirDanio(Ataque * 3); 
            }
            else
            {
                Energia += 5; // Recupera energía si hace un ataque básico
                objetivo.RecibirDanio(Ataque);
            }
        }
    }
}