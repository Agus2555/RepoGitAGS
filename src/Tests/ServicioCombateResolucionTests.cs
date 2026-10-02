using Aplicacion.Interfaces;
using Aplicacion.Servicios;
using Persistencia.Entidades;
using Xunit;

namespace Tests;

/// <summary>
/// Pruebas sobre la resolución del combate (ServicioCombate.Combatir).
/// Sin repositorios, sin BD: solo lógica de resolución en memoria.
/// </summary>
public class ServicioCombateResolucionTests
{
    private class EstrategiaAtacar : IEstrategiaTurno
    {
        public TipoAccion ElegirAccion(Personaje actor, Personaje objetivo) => TipoAccion.Atacar;
    }

    private class EstrategiaDefender : IEstrategiaTurno
    {
        public TipoAccion ElegirAccion(Personaje actor, Personaje objetivo) => TipoAccion.Defender;
    }

    // Permite asignar una acción fija distinta a cada personaje (por nombre).
    private class EstrategiaPorActor(Dictionary<string, TipoAccion> acciones) : IEstrategiaTurno
    {
        public TipoAccion ElegirAccion(Personaje actor, Personaje objetivo) => acciones[actor.Nombre];
    }

    private static ServicioCombate CrearServicio() => new(new EstrategiaAtacar());

    [Fact]
    public void Combatir_TerminaConGanador_CuandoUnPersonajeMuere()
    {
        var fuerte = new Guerrero("Fuerte", 100, 50, 2, 0); // ataque alto
        var debil  = new Guerrero("Débil",   10,  5, 1, 0); // poca vida

        var resultado = CrearServicio().Combatir(fuerte, debil);

        Assert.NotNull(resultado.Ganador);
        Assert.Equal("Fuerte", resultado.Ganador.Nombre);
        Assert.False(debil.EstaVivo);
    }

    [Fact]
    public void Combatir_GanadorEsElQueQuedaVivo()
    {
        var p1 = new Mago("Mago",     50, 10, 2, 0);   // poca vida
        var p2 = new Guerrero("G", 200, 50, 1, 0);     // ataque alto

        var resultado = CrearServicio().Combatir(p1, p2);

        Assert.NotNull(resultado.Ganador);
        Assert.True(resultado.Ganador.EstaVivo);
    }

    [Fact]
    public void Combatir_TerminaEnEmpate_CuandoSeAlcanzaMaxTurnos()
    {
        // Defensa tan alta que nadie puede matar al otro en 3 turnos.
        var p1 = new Guerrero("T1", 10_000, 1, 999, 0);
        var p2 = new Guerrero("T2", 10_000, 1, 999, 0);

        var resultado = CrearServicio().Combatir(p1, p2, maxTurnos: 3);

        Assert.Null(resultado.Ganador);           // empate
        Assert.Equal(3, resultado.TurnosJugados); // se consumieron los 3 turnos
    }

    [Fact]
    public void Combatir_LanzaExcepcion_SiAmbosPersonajesSonElMismo()
    {
        var p = new Guerrero("P", 100, 20, 5, 0);

        Assert.Throws<ArgumentException>(() => CrearServicio().Combatir(p, p));
    }

    [Fact]
    public void Combatir_RegistraAlMenosUnTurno()
    {
        var p1 = new Guerrero("A", 100, 50, 2, 0);
        var p2 = new Guerrero("B",  10,  5, 1, 0);

        var resultado = CrearServicio().Combatir(p1, p2);

        Assert.NotEmpty(resultado.Turnos);
    }

    [Fact]
    public void Combatir_PrimerTurno_EsDeP1ContraP2()
    {
        var p1 = new Guerrero("A", 100, 50, 2, 0);
        var p2 = new Guerrero("B", 100,  5, 1, 0);

        var resultado = CrearServicio().Combatir(p1, p2);

        var primero = resultado.Turnos[0];
        Assert.Equal("A", primero.NombreActor);
        Assert.Equal("B", primero.NombreObjetivo);
        Assert.Equal(1, primero.Numero);
    }

    [Fact]
    public void Combatir_DanioEnTurnos_SiempreEsMayorIgualACero()
    {
        var p1 = new Guerrero("A", 100, 20, 5, 0);
        var p2 = new Mago("M",     80, 15, 4, 30);

        var resultado = CrearServicio().Combatir(p1, p2);

        Assert.All(resultado.Turnos, t => Assert.True(t.Danio >= 0));
    }

    // ── Gana P1 / Gana P2 ────────────────────────────────────────────────────

    [Fact]
    public void Combatir_P1Gana_SiMataEnElPrimerGolpe_P2NuncaActua()
    {
        var p1 = new Guerrero("Fuerte", 100, 50, 2, 0);
        var p2 = new Guerrero("Débil",   10,  5, 1, 0);

        var resultado = CrearServicio().Combatir(p1, p2);

        Assert.Same(p1, resultado.Ganador);
        Assert.Single(resultado.Turnos);
        Assert.Equal(100, p1.Vida); // no recibió daño
        Assert.False(resultado.Turnos[0].ObjetivoVivo);
    }

    [Fact]
    public void Combatir_P2Gana_CuandoP1EsElDebil()
    {
        // Inverso del caso anterior: el débil ataca primero, pero no alcanza.
        var p1 = new Guerrero("Débil",   10,  5, 1, 0);
        var p2 = new Guerrero("Fuerte", 100, 50, 2, 0);

        var resultado = CrearServicio().Combatir(p1, p2);

        Assert.Same(p2, resultado.Ganador);
        Assert.False(p1.EstaVivo);
        Assert.Equal(97, p2.Vida);             // recibió un golpe de 5 - 2
        Assert.Equal(2, resultado.Turnos.Count);
        Assert.Equal("Fuerte", resultado.Turnos[^1].NombreActor);
    }

    [Fact]
    public void Combatir_ConStatsIdenticos_GanaP1PorIniciativa()
    {
        var p1 = new Guerrero("Primero", 30, 20, 5, 0);
        var p2 = new Guerrero("Segundo", 30, 20, 5, 0);

        var resultado = CrearServicio().Combatir(p1, p2);

        Assert.Same(p1, resultado.Ganador);
        Assert.Equal(15, p1.Vida);
        Assert.Equal(0, p2.Vida);
    }

    [Fact]
    public void Combatir_InvertirOrdenConStatsIdenticos_CambiaElGanador()
    {
        var a = new Guerrero("A", 30, 20, 5, 0);
        var b = new Guerrero("B", 30, 20, 5, 0);

        var resultado = CrearServicio().Combatir(b, a); // ahora B ataca primero

        Assert.Same(b, resultado.Ganador);
        Assert.False(a.EstaVivo);
    }

    [Theory]
    [InlineData("Guerrero", "Mago")]
    [InlineData("Mago",     "Arquero")]
    [InlineData("Arquero",  "Asesino")]
    [InlineData("Asesino",  "Guerrero")]
    [InlineData("Mago",     "Mago")]
    public void Combatir_P1MuchoMasFuerte_GanaP1(string claseP1, string claseP2)
    {
        var p1 = FabricaPersonajes.Crear(claseP1, "P1", 1000, 200, 50, 0);
        var p2 = FabricaPersonajes.Crear(claseP2, "P2",   50,  10,  1, 0);

        var resultado = CrearServicio().Combatir(p1, p2);

        Assert.Same(p1, resultado.Ganador);
        Assert.True(p1.EstaVivo);
        Assert.False(p2.EstaVivo);
    }

    [Theory]
    [InlineData("Guerrero", "Mago")]
    [InlineData("Mago",     "Arquero")]
    [InlineData("Arquero",  "Asesino")]
    [InlineData("Asesino",  "Guerrero")]
    [InlineData("Mago",     "Mago")]
    public void Combatir_P2MuchoMasFuerte_GanaP2(string claseP1, string claseP2)
    {
        var p1 = FabricaPersonajes.Crear(claseP1, "P1",   50,  10,  1, 0);
        var p2 = FabricaPersonajes.Crear(claseP2, "P2", 1000, 200, 50, 0);

        var resultado = CrearServicio().Combatir(p1, p2);

        Assert.Same(p2, resultado.Ganador);
        Assert.True(p2.EstaVivo);
        Assert.False(p1.EstaVivo);
    }

    [Fact]
    public void Combatir_ElPerdedorQuedaConVidaCero_YElGanadorConVidaPositiva()
    {
        var p1 = new Arquero("Arq", 60, 15, 3, 10);
        var p2 = new Mago("Mag",    40, 12, 2, 0);

        var resultado = CrearServicio().Combatir(p1, p2);

        Assert.NotNull(resultado.Ganador);
        var perdedor = resultado.Ganador == p1 ? p2 : p1;
        Assert.True(resultado.Ganador.Vida > 0);
        Assert.Equal(0, perdedor.Vida);
    }

    [Fact]
    public void Combatir_LaEstrategiaPuedeCambiarAlGanador()
    {
        // Mismo enfrentamiento: con ataque básico el Mago pierde,
        // con Bola de Fuego gana.
        Personaje NuevoMago()     => new Mago("Mago", 50, 20, 1, 30);
        Personaje NuevoGuerrero() => new Guerrero("Guerrero", 60, 20, 2, 0);

        var mago1 = NuevoMago();
        var guerrero1 = NuevoGuerrero();
        var conAtaque = new ServicioCombate(new EstrategiaAtacar()).Combatir(mago1, guerrero1);

        var mago2 = NuevoMago();
        var guerrero2 = NuevoGuerrero();
        var conHabilidad = new ServicioCombate(new EstrategiaPorActor(new()
        {
            ["Mago"]     = TipoAccion.UsarHabilidad,
            ["Guerrero"] = TipoAccion.Atacar
        })).Combatir(mago2, guerrero2);

        Assert.Same(guerrero1, conAtaque.Ganador);
        Assert.Same(mago2, conHabilidad.Ganador);
    }

    // ── Empates ──────────────────────────────────────────────────────────────

    [Fact]
    public void Combatir_Empate_AmbosQuedanVivosYSeRegistranDosTurnosPorRonda()
    {
        var p1 = new Guerrero("T1", 10_000, 1, 999, 0);
        var p2 = new Guerrero("T2", 10_000, 1, 999, 0);

        var resultado = CrearServicio().Combatir(p1, p2, maxTurnos: 5);

        Assert.Null(resultado.Ganador);
        Assert.True(p1.EstaVivo);
        Assert.True(p2.EstaVivo);
        Assert.Equal(10, resultado.Turnos.Count);
        Assert.Equal(5, resultado.TurnosJugados);
    }

    [Fact]
    public void Combatir_Empate_ConUnSoloTurnoMaximo()
    {
        var p1 = new Mago("M", 1000, 10, 5, 0);
        var p2 = new Arquero("A", 1000, 10, 5, 0);

        var resultado = CrearServicio().Combatir(p1, p2, maxTurnos: 1);

        Assert.Null(resultado.Ganador);
        Assert.Equal(1, resultado.TurnosJugados);
        Assert.Equal(2, resultado.Turnos.Count);
    }

    [Fact]
    public void Combatir_Empate_ConCeroTurnosMaximos_NoSeJuegaNada()
    {
        var p1 = new Guerrero("A", 100, 50, 5, 0);
        var p2 = new Guerrero("B", 100, 50, 5, 0);

        var resultado = CrearServicio().Combatir(p1, p2, maxTurnos: 0);

        Assert.Null(resultado.Ganador);
        Assert.Empty(resultado.Turnos);
        Assert.Equal(0, resultado.TurnosJugados);
        Assert.Equal(100, p1.Vida);
        Assert.Equal(100, p2.Vida);
    }

    [Fact]
    public void Combatir_Empate_SiAmbosSoloDefienden()
    {
        var p1 = new Guerrero("G", 100, 50, 5, 0);
        var p2 = new Asesino("As", 100, 50, 5, 0);

        var resultado = new ServicioCombate(new EstrategiaDefender()).Combatir(p1, p2, maxTurnos: 10);

        Assert.Null(resultado.Ganador);
        Assert.All(resultado.Turnos, t => Assert.Equal(0, t.Danio));
        Assert.Equal(100, p1.Vida);
        Assert.Equal(100, p2.Vida);
        Assert.Equal(50, ((Guerrero)p1).Furia); // 10 turnos × 5
    }

    [Fact]
    public void Combatir_Empate_AsesinoQueSiempreEsquivaNoRecibeDanio()
    {
        // El Asesino (P1) defiende antes de cada ataque del Guerrero: esquiva todos.
        var asesino  = new Asesino("As", 100, 20, 5, 0);
        var guerrero = new Guerrero("G", 100, 80, 5, 0);
        var servicio = new ServicioCombate(new EstrategiaPorActor(new()
        {
            ["As"] = TipoAccion.Defender,
            ["G"]  = TipoAccion.Atacar
        }));

        var resultado = servicio.Combatir(asesino, guerrero, maxTurnos: 10);

        Assert.Null(resultado.Ganador);
        Assert.Equal(100, asesino.Vida);
        Assert.Equal(100, guerrero.Vida);
    }

    [Fact]
    public void Combatir_SiAsesinoNoDefiende_PierdeContraElMismoGuerrero()
    {
        // Contraparte del test anterior: sin esquiva, el Guerrero gana.
        var asesino  = new Asesino("As", 100, 20, 5, 0);
        var guerrero = new Guerrero("G", 100, 80, 5, 0);

        var resultado = CrearServicio().Combatir(asesino, guerrero, maxTurnos: 10);

        Assert.Same(guerrero, resultado.Ganador);
        Assert.False(asesino.EstaVivo);
    }

    // ── Orden y registro de turnos ───────────────────────────────────────────

    [Fact]
    public void Combatir_LosTurnosAlternanEntreP1YP2()
    {
        var p1 = new Guerrero("A", 10_000, 1, 999, 0);
        var p2 = new Guerrero("B", 10_000, 1, 999, 0);

        var resultado = CrearServicio().Combatir(p1, p2, maxTurnos: 3);

        string[] actores = resultado.Turnos.Select(t => t.NombreActor).ToArray();
        Assert.Equal(["A", "B", "A", "B", "A", "B"], actores);
    }

    [Fact]
    public void Combatir_CadaRondaCompartenNumeroP1YP2()
    {
        var p1 = new Guerrero("A", 10_000, 1, 999, 0);
        var p2 = new Guerrero("B", 10_000, 1, 999, 0);

        var resultado = CrearServicio().Combatir(p1, p2, maxTurnos: 3);

        int[] numeros = resultado.Turnos.Select(t => t.Numero).ToArray();
        Assert.Equal([1, 1, 2, 2, 3, 3], numeros);
    }

    [Fact]
    public void Combatir_ElCombateTerminaApenasMuereAlguien()
    {
        var p1 = new Guerrero("A", 30, 20, 5, 0);
        var p2 = new Guerrero("B", 30, 20, 5, 0);

        var resultado = CrearServicio().Combatir(p1, p2, maxTurnos: 50);

        // Solo el último turno puede tener al objetivo muerto.
        Assert.All(resultado.Turnos.Take(resultado.Turnos.Count - 1), t => Assert.True(t.ObjetivoVivo));
        Assert.False(resultado.Turnos[^1].ObjetivoVivo);
    }

    // ── Validaciones ─────────────────────────────────────────────────────────

    [Fact]
    public void Combatir_LanzaExcepcion_SiP1YaEstaDerrotado()
    {
        var p1 = new Guerrero("A", 100, 20, 5, 0);
        var p2 = new Guerrero("B", 100, 20, 5, 0);
        p1.RecibirDanio(999);

        Assert.Throws<ArgumentException>(() => CrearServicio().Combatir(p1, p2));
    }

    [Fact]
    public void Combatir_LanzaExcepcion_SiP2YaEstaDerrotado()
    {
        var p1 = new Guerrero("A", 100, 20, 5, 0);
        var p2 = new Guerrero("B", 100, 20, 5, 0);
        p2.RecibirDanio(999);

        Assert.Throws<ArgumentException>(() => CrearServicio().Combatir(p1, p2));
    }
}
