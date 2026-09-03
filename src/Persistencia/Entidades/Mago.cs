using System;

namespace Persistencia.Entidades
{
    public class Mago : Personaje
    {
        public int Mana { get; private set; }

        public Mago(string nombre, int vidaInicial, int ataque, int defensa, int manaInicial) 
            : base(nombre, vidaInicial, ataque, defensa)
        {
            if (manaInicial < 0) throw new ArgumentException("El maná no puede ser negativo."); // Regla Actividad 6
            Mana = manaInicial;
        }

        public override void Atacar(Personaje objetivo)
        {
            ValidarEstado(); // Regla Actividad 6: personajes derrotados no pueden atacar

            if (Mana >= 10)
            {
                Mana -= 10;
                // El mago ignora parte de la defensa rival usando magia
                int danioMagico = Ataque * 2; 
                objetivo.RecibirDanio(danioMagico);
            }
            else
            {
                // Ataque básico si no hay maná
                objetivo.RecibirDanio(Ataque);
            }
        }
    }
}