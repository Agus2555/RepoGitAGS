using System;

namespace Persistencia.Entidades
{
    public abstract class Personaje
    {
        // Id es internal set para que solo los repositorios (mismo assembly) lo asignen al leer la BD.
        public int Id { get; internal set; }
        public string Nombre { get; private set; }
        public int Vida { get; private set; }
        public int VidaMaxima { get; private set; }
        public int Ataque { get; private set; }
        public int Defensa { get; private set; }

        public bool EstaVivo => Vida > 0;

        // Recurso específico de cada clase (Furia, Maná, Flechas, Energía).
        // abstract obliga a cada subclase a exponerlo con su nombre propio.
        public abstract int Recurso { get; }

        // Nombre de la habilidad especial, para mostrar en el historial de turnos.
        public virtual string NombreHabilidad => "Sin Habilidad";

        protected Personaje(string nombre, int vidaInicial, int ataque, int defensa)
        {
            if (string.IsNullOrWhiteSpace(nombre)) throw new ArgumentException("El nombre no puede estar vacío.");
            if (vidaInicial <= 0) throw new ArgumentException("La vida inicial debe ser mayor a cero.");
            if (ataque <= 0 || defensa <= 0) throw new ArgumentException("El ataque y la defensa deben ser mayores a cero.");

            Nombre = nombre;
            Vida = vidaInicial;
            VidaMaxima = vidaInicial;
            Ataque = ataque;
            Defensa = defensa;
        }

        // Solo los repositorios (Persistencia) deben poder asignar el Id luego del INSERT.
        internal void AsignarId(int id) => Id = id;

        protected void ValidarEstado()
        {
            if (!EstaVivo)
                throw new InvalidOperationException($"{Nombre} está derrotado y no puede realizar acciones.");
        }

        // Devuelve el daño efectivamente infligido (para que ServicioCombate lo registre).
        public abstract int Atacar(Personaje objetivo);

        // Comportamiento defensivo propio de cada clase; devuelve 0 (sin daño).
        // virtual → cada subclase lo personaliza; si no lo hace, no pasa nada.
        public virtual int Defender()
        {
            ValidarEstado();
            return 0;
        }

        // Habilidad especial con costo de recurso; devuelve daño o 0 si no hay recurso.
        public virtual int UsarHabilidad(Personaje objetivo)
        {
            ValidarEstado();
            return 0;
        }

        // Encapsula el daño recibido y garantiza que la vida nunca sea negativa.
        // Devuelve el daño real aplicado (lo que se resta de la vida).
        public virtual int RecibirDanio(int danioRecibido)
        {
            int danioReal = danioRecibido - Defensa;

            // La defensa nunca anula por completo: mínimo 1 punto de daño.
            if (danioReal <= 0)
                danioReal = 1;

            // Aplicar solo lo que queda de vida para no ir a negativo.
            int danioAplicado = Math.Min(danioReal, Vida);
            Vida -= danioAplicado;

            return danioAplicado;
        }
    }
}
