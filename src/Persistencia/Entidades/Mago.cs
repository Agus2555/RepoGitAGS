using System;

namespace Persistencia.Entidades
{
    public class Mago : Personaje
    {
        public int Mana { get; private set; }

        public override int Recurso => Mana;
        public override string NombreHabilidad => "Bola de Fuego";

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

        // Defender regenera maná: el mago medita para recuperar energía mágica.
        public override int Defender()
        {
            ValidarEstado();
            Mana += 5;
            return 0;
        }

        // Habilidad: Bola de Fuego. Consume 10 maná, daño doble.
        // Representación simplificada de "ignora parte de la defensa":
        // el poder bruto del daño doble supera en la mayoría de los casos la reducción de defensa.
        public override int UsarHabilidad(Personaje objetivo)
        {
            ValidarEstado();

            if (Mana < 10)
                return 0;

            Mana -= 10;
            return objetivo.RecibirDanio(Ataque * 2);
        }
    }
}
