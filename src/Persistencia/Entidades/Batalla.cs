using System;

namespace Persistencia.Entidades
{
    public class Batalla
    {
        public int Id { get; private set; }
        public Personaje Personaje1 { get; private set; }
        public Personaje Personaje2 { get; private set; }
        public bool Finalizada { get; private set; }
        public Personaje Ganador { get; private set; }

#pragma warning disable CS8618 // Un campo que no acepta valores NULL debe contener un valor distinto de NULL al salir del constructor. Considere la posibilidad de agregar el modificador "required" o declararlo como un valor que acepta valores NULL.
        public Batalla(Personaje personaje1, Personaje personaje2)
#pragma warning restore CS8618 // Un campo que no acepta valores NULL debe contener un valor distinto de NULL al salir del constructor. Considere la posibilidad de agregar el modificador "required" o declararlo como un valor que acepta valores NULL.
        {
            if (personaje1 == null || personaje2 == null) 
                throw new ArgumentNullException("Los personajes no pueden ser nulos.");
            if (personaje1 == personaje2) 
                throw new ArgumentException("Un personaje no puede pelear contra sí mismo.");

            Personaje1 = personaje1;
            Personaje2 = personaje2;
            Finalizada = false;
        }

        // Método para procesar un ataque y evaluar si la batalla concluyó
        public void EjecutarTurno(Personaje atacante, Personaje defensor)
        {
            if (Finalizada) 
                throw new InvalidOperationException("La batalla ya ha finalizado.");

            atacante.Atacar(defensor);
            RevisarGanador();
        }

        // Lógica interna para chequear el estado de los participantes
        private void RevisarGanador()
        {
            if (!Personaje1.EstaVivo || !Personaje2.EstaVivo)
            {
                Finalizada = true;
                
                if (Personaje1.EstaVivo) 
                {
                    Ganador = Personaje1;
                }
                else if (Personaje2.EstaVivo) 
                {
                    Ganador = Personaje2;
                }
                // Si ambos mueren simultáneamente (ej. por algún reflejo de daño), Ganador queda null (Empate)
            }
        }
    }
}