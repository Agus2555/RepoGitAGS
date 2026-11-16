using Persistencia.Entidades;
using Xunit;

namespace Tests;

/// <summary>
/// Pruebas de las reglas del combate: turnos alternados, fuera de turno, finalización,
/// nota del turno y deshacer el último turno. Sin MySQL: todo en memoria.
/// Los personajes se crean con la fábrica (estadísticas por defecto) para que tengan Id.
/// </summary>
public class CombateTests
{
    // Thorin (Guerrero, Id 1) vs Gandalf (Mago, Id 2): mismos valores que los datos de muestra.
    private static Combate GuerreroVsMago() =>
        new(FabricaPersonajes.Crear("Guerrero", "Thorin", 1),
            FabricaPersonajes.Crear("Mago", "Gandalf", 2));

    // Erebus (Asesino, Id 4) vs Gandalf (Mago, Id 2): termina en 3 turnos si los dos atacan
    // (Erebus 41 de daño, Gandalf 9, Erebus remata).
    private static Combate AsesinoVsMago() =>
        new(FabricaPersonajes.Crear("Asesino", "Erebus", 4),
            FabricaPersonajes.Crear("Mago", "Gandalf", 2));

    // ── Turnos alternados ────────────────────────────────────────────────────

    [Fact]
    public void Combate_EmpiezaElParticipante1()
    {
        var combate = GuerreroVsMago();

        Assert.Equal(1, combate.AtacanteDeTurno.Id);
        Assert.Equal(2, combate.DefensorDeTurno.Id);
        Assert.Equal(EstadoCombate.EnCurso, combate.Estado);
        Assert.Equal(0, combate.CantidadTurnos);
    }

    [Fact]
    public void Turnos_SeAlternanEntreP1YP2()
    {
        var combate = GuerreroVsMago();

        var t1 = combate.EjecutarTurno(1, "atacar");
        var t2 = combate.EjecutarTurno(2, "atacar");
        var t3 = combate.EjecutarTurno(1, "atacar");

        Assert.Equal([1, 2, 1], new[] { t1.AtacanteId, t2.AtacanteId, t3.AtacanteId });
        Assert.Equal([2, 1, 2], new[] { t1.DefensorId, t2.DefensorId, t3.DefensorId });   // el defensor es el otro
        Assert.Equal([1, 2, 3], new[] { t1.Numero, t2.Numero, t3.Numero });
        Assert.Equal(2, combate.AtacanteDeTurno.Id);
    }

    [Fact]
    public void Turno_FueraDeTurno_LanzaExcepcionYNoCambiaNada()
    {
        var combate = GuerreroVsMago();

        Assert.Throws<InvalidOperationException>(() => combate.EjecutarTurno(2, "atacar"));

        Assert.Equal(0, combate.CantidadTurnos);
        Assert.Equal(120, combate.Participante1.Vida);
        Assert.Equal(80, combate.Participante2.Vida);
    }

    [Theory]
    [InlineData("defender")]
    [InlineData("")]
    [InlineData(null)]
    public void Turno_AccionInvalida_LanzaExcepcionYNoCambiaNada(string? accion)
    {
        var combate = GuerreroVsMago();

        Assert.Throws<ArgumentException>(() => combate.EjecutarTurno(1, accion));

        Assert.Equal(0, combate.CantidadTurnos);
        Assert.Equal(80, combate.Participante2.Vida);
    }

    [Fact]
    public void Turno_AceptaLaAccionEnMayusculas()
    {
        var turno = GuerreroVsMago().EjecutarTurno(1, "ATACAR");

        Assert.Equal("atacar", turno.Accion);
    }

    // ── Daño y habilidad dentro del combate ──────────────────────────────────

    [Fact]
    public void Turno_Atacar_AplicaElDanioAlDefensor()
    {
        var combate = GuerreroVsMago();

        var turno = combate.EjecutarTurno(1, "atacar");

        Assert.Equal(16, turno.Danio);                     // 20 de ataque - 4 de defensa del Mago
        Assert.Equal(64, combate.Participante2.Vida);
        Assert.Equal(10, combate.Participante1.Recurso);   // el Guerrero ganó 10 de furia
    }

    [Fact]
    public void Turno_Habilidad_UsaLaReglaDeLaClase()
    {
        var combate = GuerreroVsMago();
        combate.EjecutarTurno(1, "atacar");

        var turno = combate.EjecutarTurno(2, "habilidad");   // Bola de Fuego: 14*2 - 8 = 20

        Assert.Equal(20, turno.Danio);
        Assert.Equal(100, combate.Participante1.Vida);
        Assert.Equal(30, combate.Participante2.Recurso);     // gastó 10 de maná
    }

    // ── Finalización ─────────────────────────────────────────────────────────

    [Fact]
    public void Combate_Finaliza_CuandoElDefensorLlegaACero_YGanaElAtacante()
    {
        var combate = AsesinoVsMago();

        combate.EjecutarTurno(4, "atacar");   // Gandalf 80 → 39
        combate.EjecutarTurno(2, "atacar");   // Erebus  90 → 81
        var ultimo = combate.EjecutarTurno(4, "atacar");   // Gandalf 39 → 0

        Assert.Equal(39, ultimo.Danio);
        Assert.Equal(EstadoCombate.Finalizado, combate.Estado);
        Assert.Equal(4, combate.GanadorId);
        Assert.Equal("Erebus", combate.Ganador!.Nombre);
        Assert.False(combate.Participante2.EstaVivo);
    }

    [Fact]
    public void Combate_Finalizado_NoAceptaMasTurnos()
    {
        var combate = AsesinoVsMago();
        combate.EjecutarTurno(4, "atacar");
        combate.EjecutarTurno(2, "atacar");
        combate.EjecutarTurno(4, "atacar");

        var ex = Assert.Throws<InvalidOperationException>(() => combate.EjecutarTurno(2, "atacar"));

        Assert.Contains("finalizó", ex.Message);
        Assert.Equal(3, combate.CantidadTurnos);
    }

    [Fact]
    public void Combate_EnCurso_NoTieneGanador()
    {
        var combate = GuerreroVsMago();
        combate.EjecutarTurno(1, "atacar");

        Assert.Null(combate.GanadorId);
        Assert.Null(combate.Ganador);
    }

    // ── Nota del turno ───────────────────────────────────────────────────────

    [Fact]
    public void CambiarNota_NoAlteraElDanioNiLasVidas()
    {
        var combate = GuerreroVsMago();
        var turno = combate.EjecutarTurno(1, "atacar");
        int danioAntes = turno.Danio;

        turno.CambiarNota("Buen golpe de Thorin");

        Assert.Equal("Buen golpe de Thorin", turno.Nota);
        Assert.Equal(danioAntes, turno.Danio);
        Assert.Equal(64, combate.Participante2.Vida);
        Assert.Equal(120, combate.Participante1.Vida);
    }

    [Fact]
    public void CambiarNota_Invalida_LanzaExcepcionYNoCambiaLaNota()
    {
        var turno = GuerreroVsMago().EjecutarTurno(1, "atacar");
        turno.CambiarNota("Original");

        Assert.Throws<ArgumentException>(() => turno.CambiarNota(null));
        Assert.Throws<ArgumentException>(() => turno.CambiarNota(new string('x', 201)));

        Assert.Equal("Original", turno.Nota);
    }

    // ── Deshacer el último turno ─────────────────────────────────────────────

    [Fact]
    public void DeshacerUltimoTurno_RestauraVidasRecursosYOrden()
    {
        var combate = GuerreroVsMago();
        combate.EjecutarTurno(1, "atacar");
        var t2 = combate.EjecutarTurno(2, "habilidad");

        combate.DeshacerTurno(t2);

        // Queda como después del turno 1.
        Assert.Equal(120, combate.Participante1.Vida);
        Assert.Equal(64, combate.Participante2.Vida);
        Assert.Equal(10, combate.Participante1.Recurso);
        Assert.Equal(40, combate.Participante2.Recurso);   // recuperó el maná
        Assert.Equal(1, combate.CantidadTurnos);
        Assert.Equal(2, combate.AtacanteDeTurno.Id);        // le vuelve a tocar a Gandalf
    }

    [Fact]
    public void DeshacerTurno_QueTerminoElCombate_VuelveAEnCursoSinGanador()
    {
        var combate = AsesinoVsMago();
        combate.EjecutarTurno(4, "atacar");
        combate.EjecutarTurno(2, "atacar");
        var ultimo = combate.EjecutarTurno(4, "atacar");

        combate.DeshacerTurno(ultimo);

        Assert.Equal(EstadoCombate.EnCurso, combate.Estado);
        Assert.Null(combate.GanadorId);
        Assert.Equal(39, combate.Participante2.Vida);
        Assert.True(combate.Participante2.EstaVivo);
        Assert.Equal(15, combate.Participante1.Recurso);   // recuperó la energía gastada
        Assert.Equal(4, combate.AtacanteDeTurno.Id);
    }

    [Fact]
    public void DeshacerTurno_QueNoEsElUltimo_LanzaExcepcionYNoCambiaNada()
    {
        var combate = GuerreroVsMago();
        var t1 = combate.EjecutarTurno(1, "atacar");
        combate.EjecutarTurno(2, "atacar");

        Assert.False(combate.EsUltimoTurno(t1));
        Assert.Throws<InvalidOperationException>(() => combate.DeshacerTurno(t1));
        Assert.Equal(2, combate.CantidadTurnos);
    }

    [Fact]
    public void DeshacerTodosLosTurnos_VuelveAlEstadoInicial()
    {
        var combate = GuerreroVsMago();
        var t1 = combate.EjecutarTurno(1, "atacar");
        var t2 = combate.EjecutarTurno(2, "habilidad");
        var t3 = combate.EjecutarTurno(1, "atacar");

        combate.DeshacerTurno(t3);
        combate.DeshacerTurno(t2);
        combate.DeshacerTurno(t1);

        Assert.Equal(0, combate.CantidadTurnos);
        Assert.Equal(120, combate.Participante1.Vida);
        Assert.Equal(80, combate.Participante2.Vida);
        Assert.Equal(0, combate.Participante1.Recurso);
        Assert.Equal(40, combate.Participante2.Recurso);
    }

    // ── Participantes ────────────────────────────────────────────────────────

    [Fact]
    public void Combate_ConElMismoPersonaje_LanzaExcepcion()
    {
        var p = FabricaPersonajes.Crear("Guerrero", "Solo", 1);

        Assert.Throws<ArgumentException>(() => new Combate(p, p));
    }

    [Fact]
    public void CambiarParticipantes_AntesDelPrimerTurno_Funciona()
    {
        var combate = GuerreroVsMago();
        var arquero = FabricaPersonajes.Crear("Arquero", "Legolas", 3);
        var asesino = FabricaPersonajes.Crear("Asesino", "Erebus", 4);

        combate.CambiarParticipantes(arquero, asesino);

        Assert.Equal(3, combate.Participante1.Id);
        Assert.Equal(4, combate.Participante2.Id);
        Assert.True(combate.Participa(4));
        Assert.False(combate.Participa(1));
    }

    [Fact]
    public void CambiarParticipantes_ConTurnos_LanzaExcepcion()
    {
        var combate = GuerreroVsMago();
        combate.EjecutarTurno(1, "atacar");

        Assert.Throws<InvalidOperationException>(() =>
            combate.CambiarParticipantes(FabricaPersonajes.Crear("Arquero", "L", 3),
                                         FabricaPersonajes.Crear("Asesino", "E", 4)));
    }
}
