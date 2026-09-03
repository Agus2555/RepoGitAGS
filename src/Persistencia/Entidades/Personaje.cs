using System;

namespace Persistencia.Entidades
{
    public abstract class Personaje
    {
        // Propiedades encapsuladas: se pueden leer desde afuera, pero solo se modifican internamente
        public int Id { get; private set; }
        public string Nombre { get; private set; }
        public int Vida { get; private set; }
        public int Ataque { get; private set; }
        public int Defensa { get; private set; }
        
        // Propiedad calculada: devuelve false si la vida llega a 0
        public bool EstaVivo => Vida > 0;

        // Constructor con las reglas de la Actividad 6
        protected Personaje(string nombre, int vidaInicial, int ataque, int defensa)
        {
            if (string.IsNullOrWhiteSpace(nombre)) throw new ArgumentException("El nombre no puede estar vacío.");
            if (vidaInicial <= 0) throw new ArgumentException("La vida inicial debe ser mayor a cero.");
            if (ataque <= 0 || defensa <= 0) throw new ArgumentException("El ataque y la defensa deben ser mayores a cero.");

            Nombre = nombre;
            Vida = vidaInicial;
            Ataque = ataque;
            Defensa = defensa;
        }

        // Método común para validar que un personaje muerto no haga acciones (Actividad 6)
        protected void ValidarEstado()
        {
            if (!EstaVivo)
            {
                throw new InvalidOperationException($"{Nombre} está derrotado y no puede realizar acciones.");
            }
        }

        // La solución a la Actividad 5: Métodos abstractos. 
        // Cada clase hija estará obligada a programar su propia forma de atacar.
        public abstract void Atacar(Personaje objetivo);

        // Método que encapsula el comportamiento de recibir daño para que la vida nunca sea negativa
        public virtual void RecibirDanio(int danioRecibido)
        {
            int danioReal = danioRecibido - Defensa;
            
            if (danioReal > 0)
            {
                Vida -= danioReal;
                if (Vida < 0) 
                {
                    Vida = 0; // Regla Actividad 6: Vida dentro de los límites permitidos (no negativa)
                }
            }
        }
    }
}