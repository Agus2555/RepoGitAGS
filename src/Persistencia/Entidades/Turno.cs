using System;

namespace Persistencia.Entidades
{
    /// <summary>
    /// Registro de un turno jugado. Además de lo que pasó (atacante, acción, daño)
    /// guarda la "foto" de vida y recurso de ambos participantes ANTES del turno,
    /// que es lo que se usa para deshacerlo.
    /// </summary>
    public class Turno
    {
        public const int LargoMaximoNota = 200;

        public int Id { get; internal set; }
        public int CombateId { get; private set; }
        public int Numero { get; private set; }
        public int AtacanteId { get; private set; }
        public int DefensorId { get; private set; }
        public string Accion { get; private set; }
        public int Danio { get; private set; }
        public string? Nota { get; private set; }

        public int VidaP1Antes { get; private set; }
        public int VidaP2Antes { get; private set; }
        public int RecursoP1Antes { get; private set; }
        public int RecursoP2Antes { get; private set; }

        // internal: los turnos solo los crea Combate.EjecutarTurno o un repositorio al leer la BD.
        internal Turno(
            int combateId, int numero, int atacanteId, int defensorId, string accion, int danio,
            int vidaP1Antes, int vidaP2Antes, int recursoP1Antes, int recursoP2Antes, string? nota = null)
        {
            CombateId = combateId;
            Numero = numero;
            AtacanteId = atacanteId;
            DefensorId = defensorId;
            Accion = accion;
            Danio = danio;
            VidaP1Antes = vidaP1Antes;
            VidaP2Antes = vidaP2Antes;
            RecursoP1Antes = recursoP1Antes;
            RecursoP2Antes = recursoP2Antes;
            Nota = nota;
        }

        // La nota es lo único editable de un turno: no toca el daño ni las vidas.
        public void CambiarNota(string? nota)
        {
            if (nota == null)
                throw new ArgumentException("La nota es obligatoria (puede ser un texto vacío).");
            if (nota.Length > LargoMaximoNota)
                throw new ArgumentException($"La nota no puede superar los {LargoMaximoNota} caracteres.");

            Nota = nota;
        }
    }
}
