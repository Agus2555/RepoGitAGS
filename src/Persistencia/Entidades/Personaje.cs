using System;

namespace Persistencia.Entidades
{
    public abstract class Personaje
    {
        public const int LargoMaximoNombre = 50;

        // Id es internal set para que solo los repositorios (mismo assembly) lo asignen al leer la BD.
        public int Id { get; internal set; }
        public string Nombre { get; private set; }
        public int Vida { get; private set; }
        public int VidaMaxima { get; private set; }
        public int Ataque { get; private set; }
        public int Defensa { get; private set; }

        public bool EstaVivo => Vida > 0;

        // Nombre de la clase concreta ("Guerrero", "Mago"...). Es lo que se guarda en la columna Tipo.
        public string Tipo => GetType().Name;

        // Recurso específico de cada clase (Furia, Maná, Flechas, Energía).
        // abstract obliga a cada subclase a exponerlo con su nombre propio.
        public abstract int Recurso { get; }
        public abstract string NombreRecurso { get; }

        // Nombre de la habilidad especial, para mostrarlo en las respuestas de la API.
        public abstract string NombreHabilidad { get; }

        protected Personaje(string nombre, int vidaInicial, int ataque, int defensa)
        {
            if (vidaInicial <= 0) throw new ArgumentException("La vida inicial debe ser mayor a cero.");
            if (ataque <= 0 || defensa <= 0) throw new ArgumentException("El ataque y la defensa deben ser mayores a cero.");

            Nombre = ValidarNombre(nombre);
            Vida = vidaInicial;
            VidaMaxima = vidaInicial;
            Ataque = ataque;
            Defensa = defensa;
        }

        // Solo los repositorios (Persistencia) deben poder asignar el Id luego del INSERT.
        internal void AsignarId(int id) => Id = id;

        // Lo único que se puede modificar de un personaje desde la API (PUT).
        public void CambiarNombre(string? nombre) => Nombre = ValidarNombre(nombre);

        // Pone la vida y el recurso que el personaje tiene DENTRO de un combate.
        // Se usa al reconstruir un combate desde la BD y al deshacer un turno.
        internal void RestaurarEstado(int vida, int recurso)
        {
            if (vida < 0 || vida > VidaMaxima) throw new ArgumentException("La vida está fuera de rango.");
            if (recurso < 0) throw new ArgumentException("El recurso no puede ser negativo.");

            Vida = vida;
            AsignarRecurso(recurso);
        }

        // Cada subclase sabe en qué propiedad guarda su recurso (Furia, Mana...).
        protected abstract void AsignarRecurso(int valor);

        protected void ValidarEstado()
        {
            if (!EstaVivo)
                throw new InvalidOperationException($"{Nombre} está derrotado y no puede realizar acciones.");
        }

        // Las dos acciones del turno. abstract: cada clase está obligada a tener sus propias reglas.
        // Devuelven el daño efectivamente infligido (para registrarlo en el turno).
        public abstract int Atacar(Personaje objetivo);
        public abstract int UsarHabilidad(Personaje objetivo);

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

        private static string ValidarNombre(string? nombre)
        {
            if (string.IsNullOrWhiteSpace(nombre)) throw new ArgumentException("El nombre no puede estar vacío.");
            if (nombre.Trim().Length > LargoMaximoNombre)
                throw new ArgumentException($"El nombre no puede superar los {LargoMaximoNombre} caracteres.");

            return nombre.Trim();
        }
    }
}
