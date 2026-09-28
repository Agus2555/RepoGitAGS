using System;

namespace Persistencia.Entidades
{
    // El daguero: letal pero frágil. Su defensa es la esquiva, no la armadura.
    public class Asesino : Personaje
    {
        public int Energia { get; private set; }

        // Flag activo después de Defender(); se consume en el siguiente RecibirDanio.
        private bool _esquivando = false;

        public override int Recurso => Energia;
        public override string NombreHabilidad => "Puñalada Letal";

        public Asesino(string nombre, int vidaInicial, int ataque, int defensa, int energiaInicial)
            : base(nombre, vidaInicial, ataque, defensa)
        {
            if (energiaInicial < 0) throw new ArgumentException("La energía no puede ser negativa.");
            Energia = energiaInicial;
        }

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

        // Defender activa la esquiva para el próximo golpe recibido.
        public override int Defender()
        {
            ValidarEstado();
            _esquivando = true;
            return 0;
        }

        // Habilidad: Puñalada Letal. Consume 15 de energía, daño triple.
        public override int UsarHabilidad(Personaje objetivo)
        {
            ValidarEstado();

            if (Energia < 15)
                return 0;

            Energia -= 15;
            return objetivo.RecibirDanio(Ataque * 3);
        }

        // Override de RecibirDanio: si está esquivando, anula el golpe y desactiva el flag.
        public override int RecibirDanio(int danioRecibido)
        {
            if (_esquivando)
            {
                _esquivando = false;
                return 0; // golpe esquivado completamente
            }

            return base.RecibirDanio(danioRecibido);
        }
    }
}
