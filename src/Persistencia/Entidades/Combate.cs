using System;

namespace Persistencia.Entidades
{
    /// <summary>
    /// Combate de dos participantes con turnos alternados.
    /// Todas las reglas del combate viven acá (no en los repositorios ni en SQL):
    /// a quién le toca, qué pasa en cada turno, cuándo termina y cómo se deshace un turno.
    /// </summary>
    public class Combate
    {
        public int Id { get; internal set; }
        public Personaje Participante1 { get; private set; }
        public Personaje Participante2 { get; private set; }
        public EstadoCombate Estado { get; private set; }
        public int? GanadorId { get; private set; }
        public int CantidadTurnos { get; private set; }

        // Combate nuevo: arranca en curso y sin turnos.
        public Combate(Personaje participante1, Personaje participante2)
        {
            ValidarParticipantes(participante1, participante2);
            Participante1 = participante1;
            Participante2 = participante2;
            Estado = EstadoCombate.EnCurso;
        }

        // Lo usan los repositorios para reconstruir un combate guardado en la BD.
        internal Combate(int id, Personaje participante1, Personaje participante2,
                         EstadoCombate estado, int? ganadorId, int cantidadTurnos)
            : this(participante1, participante2)
        {
            Id = id;
            Estado = estado;
            GanadorId = ganadorId;
            CantidadTurnos = cantidadTurnos;
        }

        // Con cantidad de turnos par le toca a P1 y con impar a P2: así se alternan solos.
        public Personaje AtacanteDeTurno => CantidadTurnos % 2 == 0 ? Participante1 : Participante2;

        // El defensor de cada acción es siempre el otro participante.
        public Personaje DefensorDeTurno => CantidadTurnos % 2 == 0 ? Participante2 : Participante1;

        public Personaje? Ganador => GanadorId == null ? null : BuscarParticipante(GanadorId.Value);

        // Devuelve el participante con ese Id, o null si no pelea en este combate.
        public Personaje? BuscarParticipante(int personajeId)
        {
            if (Participante1.Id == personajeId) return Participante1;
            if (Participante2.Id == personajeId) return Participante2;
            return null;
        }

        public bool Participa(int personajeId) => BuscarParticipante(personajeId) != null;

        // PUT de combate: solo se pueden cambiar los participantes antes del primer turno.
        public void CambiarParticipantes(Personaje participante1, Personaje participante2)
        {
            if (CantidadTurnos > 0)
                throw new InvalidOperationException("No se pueden cambiar los participantes de un combate que ya tiene turnos.");

            ValidarParticipantes(participante1, participante2);
            Participante1 = participante1;
            Participante2 = participante2;
        }

        /// <summary>
        /// Ejecuta un turno: el atacante hace la acción y el defensor es el otro participante.
        /// Devuelve el Turno con la "foto" de antes, listo para guardarse.
        /// </summary>
        public Turno EjecutarTurno(int atacanteId, string? accion)
        {
            if (Estado == EstadoCombate.Finalizado)
                throw new InvalidOperationException("El combate ya finalizó, no se pueden jugar más turnos.");
            if (atacanteId != AtacanteDeTurno.Id)
                throw new InvalidOperationException(
                    $"Acción fuera de turno: le toca a {AtacanteDeTurno.Nombre} (Id {AtacanteDeTurno.Id}).");

            Personaje atacante = AtacanteDeTurno;
            Personaje defensor = DefensorDeTurno;
            string accionElegida = accion?.Trim().ToLower() ?? "";

            // Foto del estado ANTES del turno: es lo que se guarda para poder deshacerlo.
            int vidaP1Antes = Participante1.Vida;
            int vidaP2Antes = Participante2.Vida;
            int recursoP1Antes = Participante1.Recurso;
            int recursoP2Antes = Participante2.Recurso;

            // Switch sobre la ACCIÓN que eligió el cliente, no sobre la clase del personaje.
            // Qué hace Atacar o UsarHabilidad lo decide cada subclase (polimorfismo).
            int danio = accionElegida switch
            {
                "atacar"    => atacante.Atacar(defensor),
                "habilidad" => atacante.UsarHabilidad(defensor),
                _           => throw new ArgumentException("Acción inválida: usá 'atacar' o 'habilidad'.")
            };

            CantidadTurnos++;

            // El combate termina cuando el defensor queda sin vida; gana el atacante.
            if (!defensor.EstaVivo)
            {
                Estado = EstadoCombate.Finalizado;
                GanadorId = atacante.Id;
            }

            return new Turno(Id, CantidadTurnos, atacante.Id, defensor.Id, accionElegida, danio,
                             vidaP1Antes, vidaP2Antes, recursoP1Antes, recursoP2Antes);
        }

        public bool EsUltimoTurno(Turno turno) => turno.Numero == CantidadTurnos;

        // Deshace el último turno usando la foto guardada en él.
        public void DeshacerTurno(Turno turno)
        {
            if (!EsUltimoTurno(turno))
                throw new InvalidOperationException("Solo se puede deshacer el último turno.");

            Participante1.RestaurarEstado(turno.VidaP1Antes, turno.RecursoP1Antes);
            Participante2.RestaurarEstado(turno.VidaP2Antes, turno.RecursoP2Antes);

            // Antes de cualquier turno el combate estaba en curso y sin ganador
            // (en un combate finalizado no se pueden jugar turnos).
            Estado = EstadoCombate.EnCurso;
            GanadorId = null;
            CantidadTurnos--;
        }

        private static void ValidarParticipantes(Personaje participante1, Personaje participante2)
        {
            if (participante1 == null || participante2 == null)
                throw new ArgumentNullException(nameof(participante1), "Los participantes no pueden ser nulos.");
            if (participante1 == participante2 || (participante1.Id != 0 && participante1.Id == participante2.Id))
                throw new ArgumentException("Un personaje no puede pelear contra sí mismo.");
        }
    }
}
