using System;

namespace Persistencia.Entidades
{
    public class Batalla
    {
        public int Id { get; internal set; }
        public Personaje Personaje1 { get; private set; }
        public Personaje Personaje2 { get; private set; }
        public bool Finalizada { get; private set; }

        // Nullable: null significa empate (ambos cayeron o maxTurnos alcanzado).
        public Personaje? Ganador { get; private set; }

        public Batalla(Personaje personaje1, Personaje personaje2)
        {
            if (personaje1 == null || personaje2 == null)
                throw new ArgumentNullException(nameof(personaje1), "Los personajes no pueden ser nulos.");
            if (personaje1 == personaje2)
                throw new ArgumentException("Un personaje no puede pelear contra sí mismo.");

            Personaje1 = personaje1;
            Personaje2 = personaje2;
            Finalizada = false;
        }

        // Ejecuta un ataque y evalúa si la batalla concluyó.
        // Devuelve el daño infligido para que el llamador lo persista.
        public int EjecutarTurno(Personaje atacante, Personaje defensor)
        {
            if (Finalizada)
                throw new InvalidOperationException("La batalla ya ha finalizado.");

            int danio = atacante.Atacar(defensor);
            RevisarGanador();
            return danio;
        }

        private void RevisarGanador()
        {
            if (!Personaje1.EstaVivo || !Personaje2.EstaVivo)
            {
                Finalizada = true;

                if (Personaje1.EstaVivo)
                    Ganador = Personaje1;
                else if (Personaje2.EstaVivo)
                    Ganador = Personaje2;
                // Si ambos caen simultáneamente (rarísimo), Ganador queda null → empate.
            }
        }
    }
}
