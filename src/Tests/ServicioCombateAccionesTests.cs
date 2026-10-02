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
}
