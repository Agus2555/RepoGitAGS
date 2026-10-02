using Aplicacion.Interfaces;
using Aplicacion.Servicios;
using Persistencia.Entidades;
using Xunit;

namespace Tests;

/// <summary>
/// Pruebas sobre ServicioCombate (capa de Aplicacion).
/// No hay repositorios, Dapper ni conexión a la BD:
/// cada test es aislado e independiente del estado de la base de datos.
/// Las estrategias de test son clases privadas fijas que inyectamos
/// para que las aserciones sean deterministas.
/// </summary>
public class ServicioCombateAccionesTests
{
    // ── Estrategias fijas para tests ─────────────────────────────────────────
    private class EstrategiaAtacar : IEstrategiaTurno
    {
        public TipoAccion ElegirAccion(Personaje actor, Personaje objetivo) => TipoAccion.Atacar;
    }

    private class EstrategiaHabilidad : IEstrategiaTurno
    {
        public TipoAccion ElegirAccion(Personaje actor, Personaje objetivo) => TipoAccion.UsarHabilidad;
    }

    private class EstrategiaDefender : IEstrategiaTurno
    {
        public TipoAccion ElegirAccion(Personaje actor, Personaje objetivo) => TipoAccion.Defender;
    }

    private static ServicioCombate ConEstrategia(IEstrategiaTurno e) => new(e);

    // ── Guerrero ─────────────────────────────────────────────────────────────

    [Fact]
    public void Guerrero_AlAtacarSinFuria_AcumulaDiezDeFuria()
    {
        var guerrero = new Guerrero("G", 100, 20, 5, furiaInicial: 0);
        var objetivo = new Guerrero("E", 200, 5, 2, furiaInicial: 0);
        var servicio = ConEstrategia(new EstrategiaAtacar());

        servicio.EjecutarTurno(guerrero, objetivo, 1);

        Assert.Equal(10, guerrero.Furia);
    }

    [Fact]
    public void Guerrero_AlUsarGolpeDevastador_ConsumeFuriaYHaceDanioExtra()
    {
        var guerrero = new Guerrero("G", 100, 20, 5, furiaInicial: 20);
        var objetivo = new Guerrero("E", 500, 5, 1, furiaInicial: 0); // mucha vida para no morir
        var servicio = ConEstrategia(new EstrategiaHabilidad());

        var turno = servicio.EjecutarTurno(guerrero, objetivo, 1);

        Assert.Equal(0, guerrero.Furia);             // consumió los 20 puntos
        Assert.Equal("Golpe Devastador", turno.NombreAccion);
        Assert.True(turno.Danio > 0);
    }

    [Fact]
    public void Guerrero_SinFuriaParaHabilidad_DevuelveCeroDanio()
    {
        var guerrero = new Guerrero("G", 100, 20, 5, furiaInicial: 5); // menos de 20
        var objetivo = new Guerrero("E", 100, 5, 2, furiaInicial: 0);
        var servicio = ConEstrategia(new EstrategiaHabilidad());

        var turno = servicio.EjecutarTurno(guerrero, objetivo, 1);

        Assert.Equal(0, turno.Danio); // habilidad falla sin recurso suficiente
    }

    // ── Mago ─────────────────────────────────────────────────────────────────

    [Fact]
    public void Mago_ConMana_UsaBolaDeFuegoYHaceMasDanioQueAtaqueBasico()
    {
        // Mismo Mago, mismo objetivo: comparamos los daños con cada estrategia.
        var objetivo1 = new Guerrero("E1", 500, 5, 1, 0);
        var objetivo2 = new Guerrero("E2", 500, 5, 1, 0);

        var turnoBasico    = ConEstrategia(new EstrategiaAtacar())
            .EjecutarTurno(new Mago("M", 80, 20, 5, manaInicial: 0), objetivo1, 1);

        var turnoHabilidad = ConEstrategia(new EstrategiaHabilidad())
            .EjecutarTurno(new Mago("M", 80, 20, 5, manaInicial: 50), objetivo2, 1);

        Assert.True(turnoHabilidad.Danio > turnoBasico.Danio);
        Assert.Equal("Bola de Fuego", turnoHabilidad.NombreAccion);
    }

    [Fact]
    public void Mago_SinMana_HabilidadDevuelveCeroDanio()
    {
        var mago    = new Mago("M", 80, 20, 5, manaInicial: 0);
        var objetivo = new Guerrero("E", 100, 5, 2, 0);
        var servicio = ConEstrategia(new EstrategiaHabilidad());

        var turno = servicio.EjecutarTurno(mago, objetivo, 1);

        Assert.Equal(0, turno.Danio);
    }

    [Fact]
    public void Mago_AlDefender_RegeneraMana()
    {
        var mago    = new Mago("M", 80, 20, 5, manaInicial: 0);
        var dummy   = new Guerrero("D", 100, 5, 2, 0);
        var servicio = ConEstrategia(new EstrategiaDefender());

        servicio.EjecutarTurno(mago, dummy, 1);

        Assert.True(mago.Mana > 0);
    }

    // ── Arquero ──────────────────────────────────────────────────────────────

    [Fact]
    public void Arquero_SinFlechas_AtaqueBasicoHaceDanioReducido()
    {
        var arquero  = new Arquero("A", 90, 20, 5, flechasIniciales: 0);
        var objetivo = new Guerrero("E", 100, 5, 1, 0);

        // Con flechas: daño = Ataque + 10 - Defensa
        // Sin flechas: daño = Ataque / 2 - Defensa (mín 1)
        var turnoSinFlechas = ConEstrategia(new EstrategiaAtacar())
            .EjecutarTurno(arquero, objetivo, 1);

        var arqueroConFlechas = new Arquero("A2", 90, 20, 5, flechasIniciales: 5);
        var turnoConFlechas = ConEstrategia(new EstrategiaAtacar())
            .EjecutarTurno(arqueroConFlechas, new Guerrero("E2", 100, 5, 1, 0), 1);

        Assert.True(turnoConFlechas.Danio > turnoSinFlechas.Danio);
    }

    [Fact]
    public void Arquero_AlDefender_RecuperaUnaFlecha()
    {
        var arquero = new Arquero("A", 90, 20, 5, flechasIniciales: 0);
        var dummy   = new Guerrero("D", 100, 5, 2, 0);

        ConEstrategia(new EstrategiaDefender()).EjecutarTurno(arquero, dummy, 1);

        Assert.Equal(1, arquero.Flechas);
    }

    // ── Asesino ──────────────────────────────────────────────────────────────

    [Fact]
    public void Asesino_AlDefender_EsquivaElSiguienteGolpe()
    {
        var asesino  = new Asesino("As", 90, 20, 5, energiaInicial: 20);
        var atacante = new Guerrero("G", 100, 50, 5, furiaInicial: 0);
        var dummy    = new Guerrero("D", 100, 5, 2, 0);

        // Asesino activa la esquiva
        ConEstrategia(new EstrategiaDefender()).EjecutarTurno(asesino, dummy, 1);

        // El atacante golpea al asesino: debería esquivar
        int vidaAntes = asesino.Vida;
        asesino.RecibirDanio(50); // llamada directa, simula el golpe del rival

        Assert.Equal(vidaAntes, asesino.Vida); // sin cambio: esquivó
        Assert.True(asesino.EstaVivo);
    }

    // ── Reglas generales (polimórficas) ──────────────────────────────────────

    [Fact]
    public void VidaNuncaEsNegativa_CuandoElDanioSuperaLaVida()
    {
        var atacante = new Guerrero("G", 100, 200, 5, 0); // ataque enorme
        var defensor = new Guerrero("D", 50,  10,  1, 0);
        var servicio = ConEstrategia(new EstrategiaAtacar());

        servicio.EjecutarTurno(atacante, defensor, 1);

        Assert.Equal(0, defensor.Vida);
        Assert.False(defensor.EstaVivo);
    }

    [Fact]
    public void PersonajeDerrotado_NoPuedeActuar_LanzaExcepcion()
    {
        var muerto  = new Guerrero("M", 100, 20, 5, 0);
        var objetivo = new Guerrero("E", 100, 20, 5, 0);
        muerto.RecibirDanio(999);

        var servicio = ConEstrategia(new EstrategiaAtacar());

        Assert.Throws<InvalidOperationException>(() => servicio.EjecutarTurno(muerto, objetivo, 1));
    }

    [Fact]
    public void PolimorfismoTurno_MismaCiamada_DaniosDiferentesPorClase()
    {
        // Todos con los mismos stats base, diferentes clases → diferente daño.
        var objetivo1 = new Guerrero("E1", 500, 5, 1, 0);
        var objetivo2 = new Guerrero("E2", 500, 5, 1, 0);
        var objetivo3 = new Guerrero("E3", 500, 5, 1, 0);
        var objetivo4 = new Guerrero("E4", 500, 5, 1, 0);

        var servicio = ConEstrategia(new EstrategiaAtacar());

        int danioGuerrero = servicio.EjecutarTurno(new Guerrero("G", 100, 20, 5, 0),    objetivo1, 1).Danio;
        int danioMago     = servicio.EjecutarTurno(new Mago("M", 100, 20, 5, 0),        objetivo2, 1).Danio;
        int danioArquero  = servicio.EjecutarTurno(new Arquero("A", 100, 20, 5, 5),     objetivo3, 1).Danio; // con flechas
        int danioAsesino  = servicio.EjecutarTurno(new Asesino("As", 100, 20, 5, 20),   objetivo4, 1).Danio; // con energía

        // Todos tienen el mismo ataque base pero mecánicas distintas → no todos son iguales.
        // No precisamos los valores exactos; basta con que no todos sean idénticos.
        int[] danos = [danioGuerrero, danioMago, danioArquero, danioAsesino];
        Assert.True(danos.Distinct().Count() > 1,
            "Se espera que las cuatro clases produzcan al menos dos valores de daño distintos (polimorfismo).");
    }

    // ── Guerrero (casos adicionales) ─────────────────────────────────────────

    [Fact]
    public void Guerrero_AtaqueBasicoSinFuria_HaceAtaqueMenosDefensa()
    {
        var guerrero = new Guerrero("G", 100, 20, 5, furiaInicial: 0);
        var objetivo = new Guerrero("E", 500, 5, 2, furiaInicial: 0);

        var turno = ConEstrategia(new EstrategiaAtacar()).EjecutarTurno(guerrero, objetivo, 1);

        Assert.Equal(18, turno.Danio);          // 20 - 2
        Assert.Equal(482, objetivo.Vida);
        Assert.Equal("Ataque Básico", turno.NombreAccion);
    }

    [Fact]
    public void Guerrero_AtaqueBasicoConFuria_ConsumeFuriaYSumaQuinceDeDanio()
    {
        var guerrero = new Guerrero("G", 100, 20, 5, furiaInicial: 20);
        var objetivo = new Guerrero("E", 500, 5, 2, furiaInicial: 0);

        var turno = ConEstrategia(new EstrategiaAtacar()).EjecutarTurno(guerrero, objetivo, 1);

        Assert.Equal(33, turno.Danio);          // (20 + 15) - 2
        Assert.Equal(0, guerrero.Furia);
    }

    [Fact]
    public void Guerrero_GolpeDevastador_HaceUnoPuntoCincoVecesElAtaque()
    {
        var guerrero = new Guerrero("G", 100, 20, 5, furiaInicial: 25);
        var objetivo = new Guerrero("E", 500, 5, 1, furiaInicial: 0);

        var turno = ConEstrategia(new EstrategiaHabilidad()).EjecutarTurno(guerrero, objetivo, 1);

        Assert.Equal(29, turno.Danio);          // (20 + 10) - 1
        Assert.Equal(5, guerrero.Furia);        // 25 - 20
    }

    [Fact]
    public void Guerrero_HabilidadSinFuria_NoConsumeFuriaNiDaniaAlObjetivo()
    {
        var guerrero = new Guerrero("G", 100, 20, 5, furiaInicial: 19);
        var objetivo = new Guerrero("E", 100, 5, 2, furiaInicial: 0);

        ConEstrategia(new EstrategiaHabilidad()).EjecutarTurno(guerrero, objetivo, 1);

        Assert.Equal(19, guerrero.Furia);
        Assert.Equal(100, objetivo.Vida);
    }

    [Fact]
    public void Guerrero_AlDefender_AcumulaCincoDeFuriaYNoHaceDanio()
    {
        var guerrero = new Guerrero("G", 100, 20, 5, furiaInicial: 0);
        var objetivo = new Guerrero("E", 100, 5, 2, furiaInicial: 0);

        var turno = ConEstrategia(new EstrategiaDefender()).EjecutarTurno(guerrero, objetivo, 1);

        Assert.Equal(5, guerrero.Furia);
        Assert.Equal(0, turno.Danio);
        Assert.Equal("Defensa", turno.NombreAccion);
        Assert.Equal(100, objetivo.Vida);
    }

    // ── Mago (casos adicionales) ─────────────────────────────────────────────

    [Fact]
    public void Mago_BolaDeFuego_ConsumeDiezDeManaYHaceDanioDoble()
    {
        var mago     = new Mago("M", 80, 20, 5, manaInicial: 50);
        var objetivo = new Guerrero("E", 500, 5, 1, 0);

        var turno = ConEstrategia(new EstrategiaHabilidad()).EjecutarTurno(mago, objetivo, 1);

        Assert.Equal(39, turno.Danio);          // (20 * 2) - 1
        Assert.Equal(40, mago.Mana);
    }

    [Fact]
    public void Mago_ConManaInsuficiente_NoConsumeMana()
    {
        var mago     = new Mago("M", 80, 20, 5, manaInicial: 9);
        var objetivo = new Guerrero("E", 100, 5, 2, 0);

        var turno = ConEstrategia(new EstrategiaHabilidad()).EjecutarTurno(mago, objetivo, 1);

        Assert.Equal(0, turno.Danio);
        Assert.Equal(9, mago.Mana);
        Assert.Equal(100, objetivo.Vida);
    }

    [Fact]
    public void Mago_AtaqueBasico_NoConsumeMana()
    {
        var mago     = new Mago("M", 80, 20, 5, manaInicial: 30);
        var objetivo = new Guerrero("E", 500, 5, 2, 0);

        var turno = ConEstrategia(new EstrategiaAtacar()).EjecutarTurno(mago, objetivo, 1);

        Assert.Equal(18, turno.Danio);          // 20 - 2
        Assert.Equal(30, mago.Mana);
    }

    [Fact]
    public void Mago_AlDefender_RegeneraExactamenteCincoDeMana()
    {
        var mago  = new Mago("M", 80, 20, 5, manaInicial: 3);
        var dummy = new Guerrero("D", 100, 5, 2, 0);

        ConEstrategia(new EstrategiaDefender()).EjecutarTurno(mago, dummy, 1);

        Assert.Equal(8, mago.Mana);
    }

    // ── Arquero (casos adicionales) ──────────────────────────────────────────

    [Fact]
    public void Arquero_ConFlechas_ConsumeUnaYSumaDiezDeDanio()
    {
        var arquero  = new Arquero("A", 90, 20, 5, flechasIniciales: 5);
        var objetivo = new Guerrero("E", 500, 5, 1, 0);

        var turno = ConEstrategia(new EstrategiaAtacar()).EjecutarTurno(arquero, objetivo, 1);

        Assert.Equal(29, turno.Danio);          // (20 + 10) - 1
        Assert.Equal(4, arquero.Flechas);
    }

    [Fact]
    public void Arquero_SinFlechas_HaceLaMitadDelAtaque()
    {
        var arquero  = new Arquero("A", 90, 20, 5, flechasIniciales: 0);
        var objetivo = new Guerrero("E", 500, 5, 1, 0);

        var turno = ConEstrategia(new EstrategiaAtacar()).EjecutarTurno(arquero, objetivo, 1);

        Assert.Equal(9, turno.Danio);           // (20 / 2) - 1
        Assert.Equal(0, arquero.Flechas);
    }

    [Fact]
    public void Arquero_LluviaDeFlechas_ConsumeTresFlechasYHaceDanioDoble()
    {
        var arquero  = new Arquero("A", 90, 20, 5, flechasIniciales: 3);
        var objetivo = new Guerrero("E", 500, 5, 1, 0);

        var turno = ConEstrategia(new EstrategiaHabilidad()).EjecutarTurno(arquero, objetivo, 1);

        Assert.Equal(39, turno.Danio);          // (20 * 2) - 1
        Assert.Equal(0, arquero.Flechas);
        Assert.Equal("Lluvia de Flechas", turno.NombreAccion);
    }

    [Fact]
    public void Arquero_ConMenosDeTresFlechas_HabilidadFalla()
    {
        var arquero  = new Arquero("A", 90, 20, 5, flechasIniciales: 2);
        var objetivo = new Guerrero("E", 100, 5, 1, 0);

        var turno = ConEstrategia(new EstrategiaHabilidad()).EjecutarTurno(arquero, objetivo, 1);

        Assert.Equal(0, turno.Danio);
        Assert.Equal(2, arquero.Flechas);
        Assert.Equal(100, objetivo.Vida);
    }

    // ── Asesino (casos adicionales) ──────────────────────────────────────────

    [Fact]
    public void Asesino_AtaqueConEnergia_ConsumeQuinceYHaceDanioTriple()
    {
        var asesino  = new Asesino("As", 90, 20, 5, energiaInicial: 15);
        var objetivo = new Guerrero("E", 500, 5, 1, 0);

        var turno = ConEstrategia(new EstrategiaAtacar()).EjecutarTurno(asesino, objetivo, 1);

        Assert.Equal(59, turno.Danio);          // (20 * 3) - 1
        Assert.Equal(0, asesino.Energia);
    }

    [Fact]
    public void Asesino_AtaqueSinEnergia_RecuperaCincoYHaceDanioNormal()
    {
        var asesino  = new Asesino("As", 90, 20, 5, energiaInicial: 0);
        var objetivo = new Guerrero("E", 500, 5, 1, 0);

        var turno = ConEstrategia(new EstrategiaAtacar()).EjecutarTurno(asesino, objetivo, 1);

        Assert.Equal(19, turno.Danio);          // 20 - 1
        Assert.Equal(5, asesino.Energia);
    }

    [Fact]
    public void Asesino_PunaladaLetal_ConsumeEnergiaYHaceDanioTriple()
    {
        var asesino  = new Asesino("As", 90, 20, 5, energiaInicial: 20);
        var objetivo = new Guerrero("E", 500, 5, 1, 0);

        var turno = ConEstrategia(new EstrategiaHabilidad()).EjecutarTurno(asesino, objetivo, 1);

        Assert.Equal(59, turno.Danio);
        Assert.Equal(5, asesino.Energia);
        Assert.Equal("Puñalada Letal", turno.NombreAccion);
    }

    [Fact]
    public void Asesino_SinEnergiaParaHabilidad_DevuelveCeroDanio()
    {
        var asesino  = new Asesino("As", 90, 20, 5, energiaInicial: 14);
        var objetivo = new Guerrero("E", 100, 5, 1, 0);

        var turno = ConEstrategia(new EstrategiaHabilidad()).EjecutarTurno(asesino, objetivo, 1);

        Assert.Equal(0, turno.Danio);
        Assert.Equal(14, asesino.Energia);
    }

    [Fact]
    public void Asesino_EsquivaSoloUnGolpe_ElSegundoLoDania()
    {
        var asesino = new Asesino("As", 90, 20, 5, energiaInicial: 0);
        var dummy   = new Guerrero("D", 100, 5, 2, 0);

        ConEstrategia(new EstrategiaDefender()).EjecutarTurno(asesino, dummy, 1);

        Assert.Equal(0, asesino.RecibirDanio(50));  // esquivado
        Assert.Equal(45, asesino.RecibirDanio(50)); // 50 - 5: ya no esquiva
        Assert.Equal(45, asesino.Vida);
    }

    [Fact]
    public void Asesino_Esquivando_ElTurnoDelAtacanteRegistraCeroDanio()
    {
        var asesino  = new Asesino("As", 90, 20, 5, energiaInicial: 0);
        var atacante = new Guerrero("G", 100, 50, 5, furiaInicial: 0);

        ConEstrategia(new EstrategiaDefender()).EjecutarTurno(asesino, atacante, 1);
        var turno = ConEstrategia(new EstrategiaAtacar()).EjecutarTurno(atacante, asesino, 1);

        Assert.Equal(0, turno.Danio);
        Assert.Equal(90, asesino.Vida);
    }

    // ── Reglas generales (casos adicionales) ─────────────────────────────────

    [Theory]
    [InlineData("Guerrero", "Golpe Devastador")]
    [InlineData("Mago",     "Bola de Fuego")]
    [InlineData("Arquero",  "Lluvia de Flechas")]
    [InlineData("Asesino",  "Puñalada Letal")]
    public void Habilidad_NombreAccion_CorrespondeALaClase(string clase, string nombreEsperado)
    {
        var actor    = FabricaPersonajes.Crear(clase, "X", 100, 20, 5, 50);
        var objetivo = new Guerrero("E", 500, 5, 1, 0);

        var turno = ConEstrategia(new EstrategiaHabilidad()).EjecutarTurno(actor, objetivo, 1);

        Assert.Equal(nombreEsperado, turno.NombreAccion);
        Assert.True(turno.Danio > 0);
    }

    [Theory]
    [InlineData("Guerrero")]
    [InlineData("Mago")]
    [InlineData("Arquero")]
    [InlineData("Asesino")]
    public void Defender_NuncaHaceDanio_EnNingunaClase(string clase)
    {
        var actor    = FabricaPersonajes.Crear(clase, "X", 100, 20, 5, 50);
        var objetivo = new Guerrero("E", 100, 5, 1, 0);

        var turno = ConEstrategia(new EstrategiaDefender()).EjecutarTurno(actor, objetivo, 1);

        Assert.Equal(0, turno.Danio);
        Assert.Equal(100, objetivo.Vida);
        Assert.Equal("Defensa", turno.NombreAccion);
    }

    [Fact]
    public void DefensaMayorAlAtaque_SiempreHaceAlMenosUnoDeDanio()
    {
        var atacante = new Guerrero("G", 100, 5, 5, 0);
        var tanque   = new Guerrero("T", 100, 5, 999, 0);

        var turno = ConEstrategia(new EstrategiaAtacar()).EjecutarTurno(atacante, tanque, 1);

        Assert.Equal(1, turno.Danio);
        Assert.Equal(99, tanque.Vida);
    }

    [Fact]
    public void Turno_RegistraNumeroNombresYEstadoDeAmbos()
    {
        var actor    = new Guerrero("Actor", 100, 20, 5, 0);
        var objetivo = new Guerrero("Objetivo", 100, 5, 2, 0);

        var turno = ConEstrategia(new EstrategiaAtacar()).EjecutarTurno(actor, objetivo, 7);

        Assert.Equal(7, turno.Numero);
        Assert.Equal("Actor", turno.NombreActor);
        Assert.Equal("Objetivo", turno.NombreObjetivo);
        Assert.True(turno.ActorVivo);
        Assert.True(turno.ObjetivoVivo);
    }

    [Fact]
    public void Turno_QueMataAlObjetivo_RegistraObjetivoMuerto()
    {
        var actor    = new Guerrero("Actor", 100, 50, 5, 0);
        var objetivo = new Guerrero("Objetivo", 10, 5, 1, 0);

        var turno = ConEstrategia(new EstrategiaAtacar()).EjecutarTurno(actor, objetivo, 1);

        Assert.True(turno.ActorVivo);
        Assert.False(turno.ObjetivoVivo);
        Assert.Equal(10, turno.Danio); // solo se aplica la vida que quedaba
    }

    [Fact]
    public void PersonajeDerrotado_NoPuedeDefender_LanzaExcepcion()
    {
        var muerto   = new Mago("M", 100, 20, 5, 0);
        var objetivo = new Guerrero("E", 100, 20, 5, 0);
        muerto.RecibirDanio(999);

        Assert.Throws<InvalidOperationException>(
            () => ConEstrategia(new EstrategiaDefender()).EjecutarTurno(muerto, objetivo, 1));
    }

    [Fact]
    public void PersonajeDerrotado_NoPuedeUsarHabilidad_LanzaExcepcion()
    {
        var muerto   = new Arquero("A", 100, 20, 5, 10);
        var objetivo = new Guerrero("E", 100, 20, 5, 0);
        muerto.RecibirDanio(999);

        Assert.Throws<InvalidOperationException>(
            () => ConEstrategia(new EstrategiaHabilidad()).EjecutarTurno(muerto, objetivo, 1));
    }

    [Fact]
    public void ServicioCombate_SinEstrategia_LanzaExcepcion()
    {
        Assert.Throws<ArgumentNullException>(() => new ServicioCombate(null!));
    }
}
